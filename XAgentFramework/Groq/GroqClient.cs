using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace XAgentFramework.Groq
{
    public class GroqClient : IAgentClient
    {
        private const int TIMEOUTSECONDS = 300; // 5-minute timeout
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _systemPrompt;
        private List<Content> _history = [];

        public List<Content> History { get => _history; set => _history = value; }

        public GroqClient(string baseUrl, string apiKey, string systemPrompt, string model)
        {
            _apiKey = apiKey;
            _systemPrompt = systemPrompt;
            _model = model;

            // Ensure the base URL ends with '/' so relative paths append instead of replace.
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(TIMEOUTSECONDS)
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Sends a prompt along with optional file paths to the Groq (OpenAI-compatible) REST API.
        /// </summary>
        public async Task<Answer> Question(string prompt, IEnumerable<string>? filePaths = null, CancellationToken cancellationToken = default)
        {
            var userContent = new List<ContentPart>();

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                userContent.Add(new ContentPart
                {
                    Type = "text",
                    Text = prompt
                });
            }

            // Convert images to base64 data URLs
            if (filePaths != null)
            {
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        byte[] imageBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                        string mimeType = GetMimeType(filePath);
                        string base64 = Convert.ToBase64String(imageBytes);

                        userContent.Add(new ContentPart
                        {
                            Type = "image_url",
                            ImageUrl = new ImageUrl
                            {
                                Url = $"data:{mimeType};base64,{base64}"
                            }
                        });
                    }
                }
            }

            _history.Add(new Content
            {
                Role = "user",
                Parts = [new Part { Text = prompt ?? string.Empty, ContentParts = userContent }]
            });

            var payload = BuildRequestPayload();

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("chat/completions", payload, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(jsonString);

                    string aiTextResponse = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? string.Empty;

                    _history.Add(new Content
                    {
                        Role = "assistant",
                        Parts = [new Part { Text = aiTextResponse }]
                    });

                    return new Answer { Success = true, Text = aiTextResponse };
                }
                else
                {
                    RollbackLastUserTurn();

                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    string errorMessage = ParseErrorMessage(errorContent);

                    return new Answer
                    {
                        Success = false,
                        Text = !string.IsNullOrWhiteSpace(errorMessage) ? errorMessage : $"Error {response.StatusCode}: {response.ReasonPhrase}"
                    };
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                RollbackLastUserTurn();
                return new Answer { Success = false, Text = "Request was cancelled by user." };
            }
            catch (TaskCanceledException)
            {
                RollbackLastUserTurn();
                return new Answer { Success = false, Text = "Request timed out." };
            }
            catch (Exception ex)
            {
                RollbackLastUserTurn();
                return new Answer { Success = false, Text = $"HTTP Request Error: {ex.Message}" };
            }
        }

        public void UpdateHistory(List<Content> newHistory)
        {
            _history.Clear();
            _history.AddRange(newHistory);
        }

        private GroqRequest BuildRequestPayload()
        {
            var messages = new List<GroqMessage>
            {
                new() { Role = "system", Content = _systemPrompt }
            };

            foreach (var content in _history)
            {
                if (content.Role == "user" && content.Parts != null && content.Parts.Count > 0)
                {
                    var part = content.Parts[0];

                    // If this turn had multimodal content, send the parts array
                    if (part.ContentParts != null && part.ContentParts.Count > 0)
                    {
                        messages.Add(new GroqMessage
                        {
                            Role = "user",
                            Content = part.ContentParts
                        });
                    }
                    else
                    {
                        messages.Add(new GroqMessage
                        {
                            Role = "user",
                            Content = part.Text ?? string.Empty
                        });
                    }
                }
                else
                {
                    messages.Add(new GroqMessage
                    {
                        Role = content.Role == "model" ? "assistant" : content.Role,
                        Content = content.Parts?[0].Text ?? string.Empty
                    });
                }
            }

            return new GroqRequest
            {
                Model = _model,
                Messages = messages
            };
        }

        private void RollbackLastUserTurn()
        {
            if (_history.Count > 0)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        private static string ParseErrorMessage(string errorContent)
        {
            try
            {
                using var errorDoc = JsonDocument.Parse(errorContent);
                if (errorDoc.RootElement.TryGetProperty("error", out var errorObj) &&
                    errorObj.TryGetProperty("message", out var messageProp))
                {
                    return messageProp.GetString() ?? string.Empty;
                }
            }
            catch (JsonException) { }

            return errorContent;
        }

        private static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".heic" => "image/heic",
                ".heif" => "image/heif",
                _ => "application/octet-stream"
            };
        }
    }
}
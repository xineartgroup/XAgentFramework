using System.Net.Http.Json;
using System.Text.Json;

namespace XAgentFramework
{
    public class GeminiClient
    {
        private const int TIMEOUTSECONDS = 300; // 5-minute timeout
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _systemPrompt;
        private List<Content> _history = [];

        public List<Content> History { get => _history; set => _history = value; }

        public GeminiClient(string baseUrl, string apiKey, string systemPrompt)
        {
            _apiKey = apiKey;
            _systemPrompt = systemPrompt;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(TIMEOUTSECONDS)
            };
        }

        /// <summary>
        /// Sends a prompt along with optional image paths to the Gemini REST API.
        /// </summary>
        public async Task<Answer> Question(string prompt, IEnumerable<string>? imagePaths = null, CancellationToken cancellationToken = default)
        {
            var parts = new List<Part>();

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                parts.Add(new Part { Text = prompt });
            }

            // Convert images to base64 inline data parts
            if (imagePaths != null)
            {
                foreach (var filePath in imagePaths)
                {
                    if (File.Exists(filePath))
                    {
                        byte[] imageBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                        string mimeType = GetMimeType(filePath);

                        parts.Add(new Part
                        {
                            InlineData = new InlineData
                            {
                                MimeType = mimeType,
                                Data = Convert.ToBase64String(imageBytes)
                            }
                        });
                    }
                }
            }

            _history.Add(new Content
            {
                Role = "user",
                Parts = parts
            });

            var payload = new GeminiRequest
            {
                SystemInstruction = new Content
                {
                    Parts = [new Part { Text = _systemPrompt }]
                },
                Contents = _history
            };

            string url = $"?key={_apiKey}";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(jsonString);

                    var responseParts = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")
                        .EnumerateArray();

                    string aiTextResponse = string.Empty;

                    foreach (var part in responseParts)
                    {
                        // Ignore internal thinking process parts if present
                        if (part.TryGetProperty("thought", out var isThought) && isThought.GetBoolean())
                        {
                            continue;
                        }

                        if (part.TryGetProperty("text", out var textElement))
                        {
                            aiTextResponse = textElement.GetString() ?? string.Empty;
                        }
                    }

                    _history.Add(new Content
                    {
                        Role = "model",
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
                // Triggered explicitly via CancellationTokenSource.Cancel()
                RollbackLastUserTurn();
                return new Answer { Success = false, Text = "Request was cancelled by user." };
            }
            catch (TaskCanceledException)
            {
                // Triggered due to HttpClient timeout
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
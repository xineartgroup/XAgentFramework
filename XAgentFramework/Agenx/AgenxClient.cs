using System.Net;
using System.Net.Mail;

namespace XAgentFramework.Agenx
{
    public class AgenxClient : IAgentClient
    {
        private List<Content> _history = [];

        public List<Content> History { get => _history; set => _history = value; }

        public Task<Answer> Question(string prompt, IEnumerable<string>? filePaths = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (prompt.StartsWith("get datetime", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Task.FromResult(new Answer { Success = true, Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                }
                else if (prompt.StartsWith("get time", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Task.FromResult(new Answer { Success = true, Text = DateTime.Now.ToString("HH:mm:ss") });
                }
                else if (prompt.StartsWith("get date", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Task.FromResult(new Answer { Success = true, Text = DateTime.Now.ToString("yyyy-MM-dd") });
                }
                else if (prompt.StartsWith("send mail", StringComparison.CurrentCultureIgnoreCase))
                {
                    //send mail {title} {recipient(s)} {content}
                    var parts = prompt[10..].Split("] [", 5);
                    string title = parts.Length > 0 ? parts[0].Replace("[", "").Replace("]", "") : "";
                    string recipients = parts.Length > 1 ? parts[1].Replace("[", "").Replace("]", "") : "";
                    string content = parts.Length > 2 ? parts[2].Replace("[", "").Replace("]", "") : "";
                    var result = SendEmail(title, recipients, content);
                    return Task.FromResult(new Answer { Success = result == "Email sent successfully!", Text = result });
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                RollbackLastUserTurn();
                return Task.FromResult(new Answer { Success = false, Text = "Request was cancelled by user." });
            }
            catch (Exception ex)
            {
                RollbackLastUserTurn();
                return Task.FromResult(new Answer { Success = false, Text = $"Request Error: {ex.Message}" });
            }

            return Task.FromResult(new Answer { Success = true, Text = "Command not found." });
        }

        private void RollbackLastUserTurn()
        {
            if (_history.Count > 0)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        public void UpdateHistory(List<Content> newHistory)
        {
            _history.Clear();
            _history.AddRange(newHistory);
        }

        private static string SendEmail(string title, string recipients, string content)
        {
            string senderEmail = "xineartsolutions@gmail.com";
            string appPassword = "mzapeumbsuvkockv";

            MailMessage message = new()
            {
                From = new MailAddress(senderEmail, "XAgent")
            };
            message.To.Add(recipients);
            message.Subject = title;
            message.Body = content;
            message.IsBodyHtml = false; // Set to true if your body contains HTML

            SmtpClient client = new("smtp.gmail.com")
            {
                Port = 587, // Standard port for TLS
                EnableSsl = true, // Gmail requires SSL/TLS
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, appPassword)
            };

            try
            {
                client.Send(message);
                return ("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return ($"Failed to send email. Error: {ex.Message}");
            }
            finally
            {
                // Clean up resources
                message.Dispose();
                client.Dispose();
            }
        }
    }
}

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
                if (prompt.StartsWith("get time", StringComparison.CurrentCultureIgnoreCase))
                {
                    return Task.FromResult(new Answer { Success = true, Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
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
    }
}

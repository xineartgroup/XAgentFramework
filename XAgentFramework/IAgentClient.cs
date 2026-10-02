namespace XAgentFramework
{
    public interface IAgentClient
    {
        /// <summary>
        /// Gets or sets the current conversation history.
        /// </summary>
        List<Content> History { get; set; }

        /// <summary>
        /// Sends a prompt along with optional file paths to the LLM provider.
        /// </summary>
        /// <param name="prompt">The text prompt to send.</param>
        /// <param name="filePaths">Optional file paths (e.g., images, PDFs) to attach.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="Answer"/> object containing the status and text output.</returns>
        Task<Answer> Question(string prompt, IEnumerable<string>? filePaths = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the current conversation history with a new list of content turns.
        /// </summary>
        /// <param name="newHistory">The replacement history list.</param>
        void UpdateHistory(List<Content> newHistory);
    }
}
namespace XAgentFramework
{
    public class AppSettings
    {
        public Size WindowSize { get; set; } = new Size(800, 600);
        public bool WindowMaximized { get; set; } = false;
        public List<string> ChatAgentNames { get; set; } = [];
        public int SelectedAgentIndex { get; set; } = -1;
    }
}

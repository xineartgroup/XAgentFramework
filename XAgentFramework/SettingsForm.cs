namespace XAgentFramework
{
    public partial class SettingsForm : Form
    {
        public static readonly Dictionary<string, AgentInfo> ModelMap = [];

        public ChatItem? SelectedItem = null;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                int index = 0;
                int selectedIndex = -1;

                foreach (var kvp in ModelMap)
                {
                    cboModels.Items.Add(kvp.Key);
                    if (SelectedItem.AgentInfo.URL == kvp.Value.URL)
                    {
                        selectedIndex = index;
                    }
                    index++;
                }

                textBoxName.Text = SelectedItem.Name;
                textBoxPrompt.Text = SelectedItem.AgentInfo.SystemPrompt;
                cboModels.SelectedIndex = selectedIndex;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                SelectedItem.Name = textBoxName.Text;
                SelectedItem.AgentInfo.SystemPrompt = textBoxPrompt.Text + GetOrchestratorPrompt();
                SelectedItem.AgentInfo.URL = ModelMap[cboModels.SelectedItem?.ToString() ?? ""].URL;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private string GetOrchestratorPrompt()
        {
            if (chkOrchestrator.Checked)
            {
                string prompt = "\r\nYou are an orchestrator that can use other agents to complete tasks." +
                    "\r\nTo accomplish this, you can utilize the following tools." +
                    "\r\nFirst, determine if you need to use a tool to complete the task." +
                    "\r\nIf so, write a prompt for the tools, in the format:";

                int i = 1;
                foreach (KeyValuePair<string, AgentInfo> kvp in ModelMap)
                {
                    prompt += $"\r\n[{kvp.Key}]: [prompt{i++}]";
                }

                return prompt;
            }
            else
            {
                return "";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

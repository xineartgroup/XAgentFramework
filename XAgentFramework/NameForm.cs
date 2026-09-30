namespace XAgentFramework
{
    public partial class NameForm : Form
    {
        public string AgentName = string.Empty;
        public string AgentPrompt = string.Empty;

        public NameForm()
        {
            InitializeComponent();
        }

        private void NameForm_Load(object sender, EventArgs e)
        {
            textBoxName.Text = CleanName(AgentName);
            textBoxPrompt.Text = !string.IsNullOrWhiteSpace(AgentPrompt) ? AgentPrompt : string.Format("You are an agent called {0}.", textBoxName.Text);
        }

        private static string CleanName(string agentName)
        {
            agentName = agentName.Trim();
            agentName = agentName.Replace("\\", " ");
            agentName = agentName.Replace("/", " ");

            if (agentName.Length == 0)
            {
                agentName = "Agent 1";
            }

            if (agentName.Length < 3)
            {
                agentName = $"{agentName} 1";
            }

            if (agentName.Length > 50)
            {
                agentName = agentName[..50];
            }

            return agentName;
        }

        private static bool IsValidName(string agentName)
        {
            if (string.IsNullOrWhiteSpace(agentName))
            {
                return false;
            }
            if (agentName.Contains('\\') || agentName.Contains('/'))
            {
                return false;
            }
            if (agentName.Length < 3)
            {
                return false;
            }
            if (agentName.Length > 50)
            {
                return false;
            }
            return true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!IsValidName(textBoxName.Text))
            {
                MessageBox.Show("Agent name cannot contain \\ or / and must be between 3 and 50 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (LLMClientFactory.AgentsNameMap.ContainsKey(textBoxName.Text))
            {
                MessageBox.Show("Agent name already exists. Please choose a different name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AgentName = textBoxName.Text;
            AgentPrompt = textBoxPrompt.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

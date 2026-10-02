namespace XAgentFramework
{
    public partial class AgentSettingsForm : Form
    {
        public ChatItem? SelectedItem = null;

        public AgentSettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                int index = 0;
                int selectedIndex = -1;

                foreach (var kvp in ClientFactory.ModelsMap)
                {
                    cboModels.Items.Add(kvp.Key);
                    if (SelectedItem.AgentInfo.URL == kvp.Value.URL)
                    {
                        selectedIndex = index;
                    }
                    index++;
                }

                textBoxName.Text = SelectedItem.Name;
                textBoxPrompt.Text = SelectedItem.AgentInfo.Prompt;
                chkOrchestrator.Checked = SelectedItem.AgentInfo.IsOrchestrator;
                textBoxPrompt.Enabled = !SelectedItem.AgentInfo.AutoPrompt && !SelectedItem.AgentInfo.IsOrchestrator;
                cboModels.SelectedIndex = selectedIndex;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                var agentInfo = ClientFactory.ModelsMap[cboModels.SelectedItem?.ToString() ?? ""];
                SelectedItem.Name = textBoxName.Text;
                if (!SelectedItem.AgentInfo.AutoPrompt)
                {
                    SelectedItem.AgentInfo.Prompt = chkOrchestrator.Checked ? ClientFactory.GetOrchestrationPrompt(SelectedItem.Name) : textBoxPrompt.Text;
                }
                SelectedItem.AgentInfo.Model = cboModels.SelectedItem?.ToString() ?? "";
                SelectedItem.AgentInfo.URL = agentInfo.URL;
                SelectedItem.AgentInfo.Key = agentInfo.Key;
                SelectedItem.AgentInfo.IsOrchestrator = chkOrchestrator.Checked;
                for (int i = 0; i < SelectedItem.Messages.Count; i++)
                {
                    if (SelectedItem.Messages[i].Sender != SelectedItem.Name && !SelectedItem.Messages[i].Sender.Equals("me", StringComparison.CurrentCultureIgnoreCase))
                    {
                        SelectedItem.Messages[i].Sender = SelectedItem.Name;
                    }
                }
            }
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

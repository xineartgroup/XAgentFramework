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

                foreach (var kvp in LLMClientFactory.ModelsMap)
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
                var agentInfo = LLMClientFactory.ModelsMap[cboModels.SelectedItem?.ToString() ?? ""];
                SelectedItem.Name = textBoxName.Text;
                SelectedItem.AgentInfo.SystemPrompt = textBoxPrompt.Text + GetOrchestratorPrompt();
                SelectedItem.AgentInfo.Name = cboModels.SelectedItem?.ToString() ?? "";
                SelectedItem.AgentInfo.URL = agentInfo.URL;
                SelectedItem.AgentInfo.Key = agentInfo.Key;
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

        private string GetOrchestratorPrompt()
        {
            if (chkOrchestrator.Checked)
            {
                string prompt = "\r\nYou are an orchestrator that can use other agents to complete tasks." +
                    "\r\nTo accomplish this, you can utilize the following tools." +
                    "\r\nFirst, determine if you need to use a tool to complete the task." +
                    "\r\nIf so, write a prompt for the tools, in the format:";

                int i = 1;
                foreach (KeyValuePair<string, AgentInfo> kvp in LLMClientFactory.ModelsMap)
                {
                    prompt += $"\r\n  [{kvp.Key}]: [prompt{i++}]";
                }

                prompt += "\r\n\r\nIf you don't need to use a tool, just answer the question directly." +
                    "\r\nIf you do need to use a tool, write the prompt for the tool in the format above." +
                    "\r\nAfter you have written the prompt for the tool, wait for the response from the tool." +
                    "\r\nOnce you have received the response from the tool, you can continue to answer the question." +
                    "\r\nIf you need to use another tool, repeat the process above." +
                    "\r\nIf you have completed the task, write your final answer in the format:" +
                    "\r\n[Final Answer]: [your answer]" +
                    "\r\nHere are the list of available tools and what they can do based on their system prompt";

                i = 1;
                foreach (KeyValuePair<string, AgentInfo> kvp in LLMClientFactory.AgentsMap)
                {
                    prompt += $"\r\n  {kvp.Key}: {kvp.Value.SystemPrompt}";
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

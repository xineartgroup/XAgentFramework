using System.Configuration;
using System.Data;

namespace XAgentFramework
{
    public partial class SettingsForm : Form
    {
        private readonly Dictionary<string, string> modelMap = [];

        public ChatListItem? SelectedItem = null;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                var keys = ConfigurationManager.AppSettings.AllKeys;

                IEnumerable<string?> nameKeys = keys.Where(k => k != null && k.StartsWith("Model:") && k.EndsWith(":Name"));

                int index = 0;
                int selectedIndex = -1;

                foreach (var nameKey in nameKeys)
                {
                    string modelName = ConfigurationManager.AppSettings[nameKey] ?? "";

                    if (nameKey != null)
                    {
                        string urlKey = nameKey.Replace(":Name", ":Url");
                        string url = ConfigurationManager.AppSettings[urlKey] ?? "";

                        if (!string.IsNullOrEmpty(modelName) && !string.IsNullOrEmpty(url))
                        {
                            modelMap[modelName] = url;
                        }

                        if (SelectedItem.URL == url)
                        {
                            selectedIndex = index;
                        }
                    }

                    index++;
                }

                foreach (var kvp in modelMap)
                {
                    cboModels.Items.Add(kvp.Key);
                }

                textBoxName.Text = SelectedItem.Name;
                textBoxPrompt.Text = SelectedItem.SystemPrompt;
                cboModels.SelectedIndex = selectedIndex;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                SelectedItem.Name = textBoxName.Text;
                SelectedItem.SystemPrompt = textBoxPrompt.Text;
                SelectedItem.URL = modelMap[cboModels.SelectedItem?.ToString() ?? ""];
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

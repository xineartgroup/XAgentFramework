namespace XAgentFramework
{
    public partial class NameForm : Form
    {
        public string ModelName = string.Empty;
        public string ModelPrompt = string.Empty;

        public NameForm()
        {
            InitializeComponent();
        }

        private void NameForm_Load(object sender, EventArgs e)
        {
            textBoxName.Text = !string.IsNullOrWhiteSpace(ModelName) ? ModelName : "Agent 1";
            textBoxPrompt.Text = !string.IsNullOrWhiteSpace(ModelPrompt) ? ModelPrompt : string.Format("You are an agent called {0}.", textBoxName.Text);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ModelName = textBoxName.Text;
            ModelPrompt = textBoxPrompt.Text;
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

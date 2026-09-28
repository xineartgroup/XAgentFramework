namespace XAgentFramework
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCancel = new Button();
            btnOK = new Button();
            textBoxPrompt = new TextBox();
            textBoxName = new TextBox();
            cboModels = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            chkOrchestrator = new CheckBox();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(201, 322);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(100, 322);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 5;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // textBoxPrompt
            // 
            textBoxPrompt.Location = new Point(109, 70);
            textBoxPrompt.Multiline = true;
            textBoxPrompt.Name = "textBoxPrompt";
            textBoxPrompt.Size = new Size(259, 97);
            textBoxPrompt.TabIndex = 2;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(109, 41);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(259, 23);
            textBoxName.TabIndex = 3;
            // 
            // cboModels
            // 
            cboModels.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModels.FormattingEnabled = true;
            cboModels.Location = new Point(109, 12);
            cboModels.Name = "cboModels";
            cboModels.Size = new Size(259, 23);
            cboModels.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 44);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 7;
            label1.Text = "Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 15);
            label2.Name = "label2";
            label2.Size = new Size(41, 15);
            label2.TabIndex = 7;
            label2.Text = "Model";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 73);
            label3.Name = "label3";
            label3.Size = new Size(88, 15);
            label3.TabIndex = 7;
            label3.Text = "System Prompt";
            // 
            // chkOrchestrator
            // 
            chkOrchestrator.AutoSize = true;
            chkOrchestrator.Location = new Point(109, 173);
            chkOrchestrator.Name = "chkOrchestrator";
            chkOrchestrator.Size = new Size(127, 19);
            chkOrchestrator.TabIndex = 8;
            chkOrchestrator.Text = "Orchestrator Agent";
            chkOrchestrator.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(380, 363);
            Controls.Add(chkOrchestrator);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cboModels);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(textBoxPrompt);
            Controls.Add(textBoxName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "SettingsForm";
            Text = "Settings";
            Load += SettingsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCancel;
        private Button btnOK;
        private TextBox textBoxPrompt;
        private TextBox textBoxName;
        private ComboBox cboModels;
        private Label label1;
        private Label label2;
        private Label label3;
        private CheckBox chkOrchestrator;
    }
}
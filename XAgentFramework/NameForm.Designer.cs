namespace XAgentFramework
{
    partial class NameForm
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
            textBoxName = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            textBoxPrompt = new TextBox();
            chkOrchestrator = new CheckBox();
            SuspendLayout();
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(52, 12);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(219, 23);
            textBoxName.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(52, 201);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(196, 201);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // textBoxPrompt
            // 
            textBoxPrompt.Location = new Point(52, 41);
            textBoxPrompt.Multiline = true;
            textBoxPrompt.Name = "textBoxPrompt";
            textBoxPrompt.ScrollBars = ScrollBars.Vertical;
            textBoxPrompt.Size = new Size(219, 100);
            textBoxPrompt.TabIndex = 0;
            // 
            // chkOrchestrator
            // 
            chkOrchestrator.AutoSize = true;
            chkOrchestrator.Location = new Point(52, 147);
            chkOrchestrator.Name = "chkOrchestrator";
            chkOrchestrator.Size = new Size(127, 19);
            chkOrchestrator.TabIndex = 9;
            chkOrchestrator.Text = "Orchestrator Agent";
            chkOrchestrator.UseVisualStyleBackColor = true;
            // 
            // NameForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(324, 236);
            Controls.Add(chkOrchestrator);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(textBoxPrompt);
            Controls.Add(textBoxName);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "NameForm";
            Text = "Name";
            Load += NameForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxName;
        private Button btnOK;
        private Button btnCancel;
        private TextBox textBoxPrompt;
        private CheckBox chkOrchestrator;
    }
}
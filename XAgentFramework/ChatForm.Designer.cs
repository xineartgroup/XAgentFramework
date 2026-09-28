namespace XAgentFramework
{
    partial class ChatForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPrompt = new TextBox();
            btnSend = new Button();
            cboModels = new ComboBox();
            chatView1 = new ChatView();
            splitContainer1 = new SplitContainer();
            chatList1 = new ChatList();
            lblAttachmentStatus = new Label();
            btnCancelAttachment = new Button();
            btnAttach = new Button();
            lblStatus = new Label();
            btnLoadFromFile = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // txtPrompt
            // 
            txtPrompt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPrompt.BorderStyle = BorderStyle.FixedSingle;
            txtPrompt.Location = new Point(41, 296);
            txtPrompt.Multiline = true;
            txtPrompt.Name = "txtPrompt";
            txtPrompt.Size = new Size(325, 70);
            txtPrompt.TabIndex = 0;
            txtPrompt.KeyPress += TxtPrompt_KeyPress;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.FlatStyle = FlatStyle.Popup;
            btnSend.Image = Properties.Resources.send;
            btnSend.Location = new Point(372, 296);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(70, 70);
            btnSend.TabIndex = 1;
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += BtnSend_Click;
            // 
            // cboModels
            // 
            cboModels.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboModels.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModels.FormattingEnabled = true;
            cboModels.Location = new Point(13, 12);
            cboModels.Name = "cboModels";
            cboModels.Size = new Size(546, 23);
            cboModels.TabIndex = 3;
            cboModels.SelectedIndexChanged += CboModels_SelectedIndexChanged;
            // 
            // chatView1
            // 
            chatView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chatView1.BackColor = Color.White;
            chatView1.BorderStyle = BorderStyle.Fixed3D;
            chatView1.Location = new Point(3, 3);
            chatView1.Name = "chatView1";
            chatView1.Size = new Size(440, 264);
            chatView1.TabIndex = 4;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(14, 41);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(chatList1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lblAttachmentStatus);
            splitContainer1.Panel2.Controls.Add(btnCancelAttachment);
            splitContainer1.Panel2.Controls.Add(btnAttach);
            splitContainer1.Panel2.Controls.Add(chatView1);
            splitContainer1.Panel2.Controls.Add(txtPrompt);
            splitContainer1.Panel2.Controls.Add(btnSend);
            splitContainer1.Size = new Size(674, 369);
            splitContainer1.SplitterDistance = 224;
            splitContainer1.TabIndex = 5;
            // 
            // chatList1
            // 
            chatList1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chatList1.BackColor = Color.White;
            chatList1.BorderStyle = BorderStyle.Fixed3D;
            chatList1.Location = new Point(3, 3);
            chatList1.MinimumSize = new Size(200, 200);
            chatList1.Name = "chatList1";
            chatList1.Size = new Size(218, 363);
            chatList1.TabIndex = 0;
            // 
            // lblAttachmentStatus
            // 
            lblAttachmentStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblAttachmentStatus.BorderStyle = BorderStyle.Fixed3D;
            lblAttachmentStatus.Location = new Point(3, 270);
            lblAttachmentStatus.Name = "lblAttachmentStatus";
            lblAttachmentStatus.Size = new Size(439, 23);
            lblAttachmentStatus.TabIndex = 6;
            // 
            // btnCancelAttachment
            // 
            btnCancelAttachment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancelAttachment.FlatStyle = FlatStyle.Popup;
            btnCancelAttachment.Image = Properties.Resources.cancel;
            btnCancelAttachment.Location = new Point(3, 334);
            btnCancelAttachment.Name = "btnCancelAttachment";
            btnCancelAttachment.Size = new Size(32, 32);
            btnCancelAttachment.TabIndex = 5;
            btnCancelAttachment.UseVisualStyleBackColor = true;
            btnCancelAttachment.Click += BtnCancelAttachment_Click;
            // 
            // btnAttach
            // 
            btnAttach.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAttach.FlatStyle = FlatStyle.Popup;
            btnAttach.Image = Properties.Resources.attach;
            btnAttach.Location = new Point(3, 296);
            btnAttach.Name = "btnAttach";
            btnAttach.Size = new Size(32, 32);
            btnAttach.TabIndex = 5;
            btnAttach.UseVisualStyleBackColor = true;
            btnAttach.Click += BtnAttach_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.BorderStyle = BorderStyle.Fixed3D;
            lblStatus.Location = new Point(14, 413);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(675, 23);
            lblStatus.TabIndex = 6;
            // 
            // btnLoadFromFile
            // 
            btnLoadFromFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoadFromFile.Location = new Point(565, 11);
            btnLoadFromFile.Name = "btnLoadFromFile";
            btnLoadFromFile.Size = new Size(123, 23);
            btnLoadFromFile.TabIndex = 7;
            btnLoadFromFile.Text = "Load from File";
            btnLoadFromFile.UseVisualStyleBackColor = true;
            btnLoadFromFile.Click += BtnLoadFromFile_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(703, 445);
            Controls.Add(btnLoadFromFile);
            Controls.Add(lblStatus);
            Controls.Add(splitContainer1);
            Controls.Add(cboModels);
            MinimumSize = new Size(450, 350);
            Name = "ChatForm";
            Text = "LLM Chat";
            Load += Form1_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtPrompt;
        private Button btnSend;
        private ComboBox cboModels;
        private ChatView chatView1;
        private SplitContainer splitContainer1;
        private ChatList chatList1;
        private Label lblStatus;
        private Button btnLoadFromFile;
        private Button btnCancelAttachment;
        private Button btnAttach;
        private Label lblAttachmentStatus;
    }
}

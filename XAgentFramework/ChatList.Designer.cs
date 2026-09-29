namespace XAgentFramework
{
    partial class ChatList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            panelContainer = new Panel();
            panelContainer.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(194, 294);
            panel1.TabIndex = 0;
            panel1.Paint += Panel1_Paint;
            panel1.DoubleClick += Panel1_DoubleClick;
            panel1.MouseClick += Panel1_MouseClick;
            panel1.MouseLeave += Panel1_MouseLeave;
            panel1.MouseMove += Panel1_MouseMove;
            // 
            // panelContainer
            // 
            panelContainer.AutoScroll = true;
            panelContainer.Controls.Add(panel1);
            panelContainer.Dock = DockStyle.Fill;
            panelContainer.Location = new Point(0, 0);
            panelContainer.Name = "panelContainer";
            panelContainer.Size = new Size(200, 300);
            panelContainer.TabIndex = 0;
            // 
            // ChatList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelContainer);
            MinimumSize = new Size(200, 200);
            Name = "ChatList";
            Size = new Size(200, 300);
            panelContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panelContainer;
    }
}

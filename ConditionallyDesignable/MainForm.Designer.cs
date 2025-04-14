namespace ConditionallyDesignable
{
    partial class MainForm
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
            userControlEx = new UserControlEx();
            SuspendLayout();
            // 
            // userControlEx
            // 
            userControlEx.BackColor = Color.FromArgb(68, 68, 68);
            userControlEx.ContentType = ContentType.Immutable;
            userControlEx.Dock = DockStyle.Fill;
            userControlEx.Location = new Point(20, 20);
            userControlEx.Name = "userControlEx";
            userControlEx.Size = new Size(438, 204);
            userControlEx.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(170, 170, 170);
            ClientSize = new Size(478, 244);
            Controls.Add(userControlEx);
            Name = "MainForm";
            Padding = new Padding(20);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private UserControlEx userControlEx;
    }
}

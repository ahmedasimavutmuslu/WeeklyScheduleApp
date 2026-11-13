namespace WeeklyScheduleApp.Views
{
    partial class MainForm
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
            btnOpenPlanning = new Button();
            btnOpenUserManagement = new Button();
            SuspendLayout();
            // 
            // btnOpenPlanning
            // 
            btnOpenPlanning.Location = new Point(418, 261);
            btnOpenPlanning.Name = "btnOpenPlanning";
            btnOpenPlanning.Size = new Size(151, 29);
            btnOpenPlanning.TabIndex = 0;
            btnOpenPlanning.Text = "Shift Planlamayı Aç";
            btnOpenPlanning.UseVisualStyleBackColor = true;
            btnOpenPlanning.Click += btnOpenPlanning_Click;
            // 
            // btnOpenUserManagement
            // 
            btnOpenUserManagement.Location = new Point(659, 261);
            btnOpenUserManagement.Name = "btnOpenUserManagement";
            btnOpenUserManagement.Size = new Size(179, 29);
            btnOpenUserManagement.TabIndex = 1;
            btnOpenUserManagement.Text = "User Managementi Aç";
            btnOpenUserManagement.UseVisualStyleBackColor = true;
            btnOpenUserManagement.Click += btnOpenUserManagement_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 673);
            Controls.Add(btnOpenUserManagement);
            Controls.Add(btnOpenPlanning);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnOpenPlanning;
        private Button btnOpenUserManagement;
    }
}
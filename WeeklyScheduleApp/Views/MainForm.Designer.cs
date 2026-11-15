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
            btnOpenReport = new Button();
            btnOpenDetail = new Button();
            SuspendLayout();
            // 
            // btnOpenPlanning
            // 
            btnOpenPlanning.Location = new Point(263, 256);
            btnOpenPlanning.Name = "btnOpenPlanning";
            btnOpenPlanning.Size = new Size(151, 29);
            btnOpenPlanning.TabIndex = 0;
            btnOpenPlanning.Text = "Shift Planlamayı Aç";
            btnOpenPlanning.UseVisualStyleBackColor = true;
            btnOpenPlanning.Click += btnOpenPlanning_Click;
            // 
            // btnOpenUserManagement
            // 
            btnOpenUserManagement.Location = new Point(504, 256);
            btnOpenUserManagement.Name = "btnOpenUserManagement";
            btnOpenUserManagement.Size = new Size(179, 29);
            btnOpenUserManagement.TabIndex = 1;
            btnOpenUserManagement.Text = "User Managementi Aç";
            btnOpenUserManagement.UseVisualStyleBackColor = true;
            btnOpenUserManagement.Click += btnOpenUserManagement_Click;
            // 
            // btnOpenReport
            // 
            btnOpenReport.Location = new Point(773, 256);
            btnOpenReport.Name = "btnOpenReport";
            btnOpenReport.Size = new Size(172, 29);
            btnOpenReport.TabIndex = 2;
            btnOpenReport.Text = "Adalet ve Süre Raporu";
            btnOpenReport.UseVisualStyleBackColor = true;
            btnOpenReport.Click += btnOpenReport_Click;
            // 
            // btnOpenDetail
            // 
            btnOpenDetail.Location = new Point(263, 308);
            btnOpenDetail.Name = "btnOpenDetail";
            btnOpenDetail.Size = new Size(151, 29);
            btnOpenDetail.TabIndex = 3;
            btnOpenDetail.Text = "Shift Detaylarını Aç";
            btnOpenDetail.UseVisualStyleBackColor = true;
            btnOpenDetail.Click += btnOpenDetail_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 673);
            Controls.Add(btnOpenDetail);
            Controls.Add(btnOpenReport);
            Controls.Add(btnOpenUserManagement);
            Controls.Add(btnOpenPlanning);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnOpenPlanning;
        private Button btnOpenUserManagement;
        private Button btnOpenReport;
        private Button btnOpenDetail;
    }
}
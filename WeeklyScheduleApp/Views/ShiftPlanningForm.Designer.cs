namespace WeeklyScheduleApp.Views
{
    partial class ShiftPlanningForm
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
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBoxDetails = new GroupBox();
            dgvResult = new DataGridView();
            btnGenerateSchedule = new Button();
            dgvEmployees = new DataGridView();
            Select = new DataGridViewCheckBoxColumn();
            Username = new DataGridViewTextBoxColumn();
            SpecificTimeToStart = new DataGridViewTextBoxColumn();
            cmbShift = new ComboBox();
            dtpDate = new DateTimePicker();
            tableLayoutPanel1.SuspendLayout();
            groupBoxDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvEmployees).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.5F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69.5F));
            tableLayoutPanel1.Controls.Add(groupBoxDetails, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1262, 673);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBoxDetails
            // 
            groupBoxDetails.Controls.Add(dgvResult);
            groupBoxDetails.Controls.Add(btnGenerateSchedule);
            groupBoxDetails.Controls.Add(dgvEmployees);
            groupBoxDetails.Controls.Add(cmbShift);
            groupBoxDetails.Controls.Add(dtpDate);
            groupBoxDetails.Dock = DockStyle.Fill;
            groupBoxDetails.Location = new Point(387, 3);
            groupBoxDetails.Name = "groupBoxDetails";
            groupBoxDetails.Size = new Size(872, 667);
            groupBoxDetails.TabIndex = 0;
            groupBoxDetails.TabStop = false;
            groupBoxDetails.Text = "Detaylar";
            // 
            // dgvResult
            // 
            dgvResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Location = new Point(15, 404);
            dgvResult.Name = "dgvResult";
            dgvResult.RowHeadersWidth = 51;
            dgvResult.Size = new Size(481, 188);
            dgvResult.TabIndex = 4;
            // 
            // btnGenerateSchedule
            // 
            btnGenerateSchedule.Location = new Point(162, 312);
            btnGenerateSchedule.Name = "btnGenerateSchedule";
            btnGenerateSchedule.Size = new Size(200, 57);
            btnGenerateSchedule.TabIndex = 3;
            btnGenerateSchedule.Text = "Otomatik Görev Dağılımını Hesapla";
            btnGenerateSchedule.UseVisualStyleBackColor = true;
            btnGenerateSchedule.Click += btnGenerateSchedule_Click;
            // 
            // dgvEmployees
            // 
            dgvEmployees.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEmployees.Columns.AddRange(new DataGridViewColumn[] { Select, Username, SpecificTimeToStart });
            dgvEmployees.Location = new Point(15, 156);
            dgvEmployees.Name = "dgvEmployees";
            dgvEmployees.RowHeadersWidth = 51;
            dgvEmployees.Size = new Size(481, 150);
            dgvEmployees.TabIndex = 2;
            // 
            // Select
            // 
            Select.HeaderText = "Seçili mi?";
            Select.MinimumWidth = 6;
            Select.Name = "Select";
            Select.Width = 125;
            // 
            // Username
            // 
            Username.HeaderText = "Çalışan Adı";
            Username.MinimumWidth = 6;
            Username.Name = "Username";
            Username.Width = 125;
            // 
            // SpecificTimeToStart
            // 
            SpecificTimeToStart.HeaderText = "Özel Başlangıç Saati";
            SpecificTimeToStart.MinimumWidth = 6;
            SpecificTimeToStart.Name = "SpecificTimeToStart";
            SpecificTimeToStart.Width = 125;
            // 
            // cmbShift
            // 
            cmbShift.FormattingEnabled = true;
            cmbShift.Items.AddRange(new object[] { "Sabah", "Öğle", "Akşam" });
            cmbShift.Location = new Point(15, 104);
            cmbShift.Name = "cmbShift";
            cmbShift.Size = new Size(151, 28);
            cmbShift.TabIndex = 1;
            // 
            // dtpDate
            // 
            dtpDate.Location = new Point(15, 61);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new Size(250, 27);
            dtpDate.TabIndex = 0;
            // 
            // ShiftPlanningForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 673);
            Controls.Add(tableLayoutPanel1);
            Name = "ShiftPlanningForm";
            Text = "ShiftPlanningForm";
            Load += ShiftPlanningForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            groupBoxDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResult).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvEmployees).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBoxDetails;
        private ComboBox cmbShift;
        private DateTimePicker dtpDate;
        private DataGridView dgvResult;
        private Button btnGenerateSchedule;
        private DataGridView dgvEmployees;
        private DataGridViewCheckBoxColumn Select;
        private DataGridViewTextBoxColumn Username;
        private DataGridViewTextBoxColumn SpecificTimeToStart;
    }
}
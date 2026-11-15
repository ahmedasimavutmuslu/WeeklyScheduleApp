namespace WeeklyScheduleApp.Views
{
    partial class ShiftDetailForm
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
            groupBoxBilgiler = new GroupBox();
            btnExportExcel = new Button();
            btnLoadMatrix = new Button();
            cmbReviewShift = new ComboBox();
            dtpReviewDate = new DateTimePicker();
            groupBoxDetaylar = new GroupBox();
            dgvMatrix = new DataGridView();
            tableLayoutPanel1.SuspendLayout();
            groupBoxBilgiler.SuspendLayout();
            groupBoxDetaylar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMatrix).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24.5641842F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.4358139F));
            tableLayoutPanel1.Controls.Add(groupBoxBilgiler, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBoxDetaylar, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1262, 673);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBoxBilgiler
            // 
            groupBoxBilgiler.Controls.Add(btnExportExcel);
            groupBoxBilgiler.Controls.Add(btnLoadMatrix);
            groupBoxBilgiler.Controls.Add(cmbReviewShift);
            groupBoxBilgiler.Controls.Add(dtpReviewDate);
            groupBoxBilgiler.Dock = DockStyle.Fill;
            groupBoxBilgiler.Location = new Point(3, 3);
            groupBoxBilgiler.Name = "groupBoxBilgiler";
            groupBoxBilgiler.Size = new Size(304, 667);
            groupBoxBilgiler.TabIndex = 0;
            groupBoxBilgiler.TabStop = false;
            groupBoxBilgiler.Text = "Bilgiler";
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(65, 170);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(151, 29);
            btnExportExcel.TabIndex = 0;
            btnExportExcel.Text = "Excele Aktar";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // btnLoadMatrix
            // 
            btnLoadMatrix.Location = new Point(65, 135);
            btnLoadMatrix.Name = "btnLoadMatrix";
            btnLoadMatrix.Size = new Size(151, 29);
            btnLoadMatrix.TabIndex = 1;
            btnLoadMatrix.Text = "Matrisi Getir";
            btnLoadMatrix.UseVisualStyleBackColor = true;
            btnLoadMatrix.Click += btnLoadMatrix_Click;
            // 
            // cmbReviewShift
            // 
            cmbReviewShift.FormattingEnabled = true;
            cmbReviewShift.Location = new Point(65, 89);
            cmbReviewShift.Name = "cmbReviewShift";
            cmbReviewShift.Size = new Size(151, 28);
            cmbReviewShift.TabIndex = 0;
            // 
            // dtpReviewDate
            // 
            dtpReviewDate.Location = new Point(23, 40);
            dtpReviewDate.Name = "dtpReviewDate";
            dtpReviewDate.Size = new Size(250, 27);
            dtpReviewDate.TabIndex = 0;
            // 
            // groupBoxDetaylar
            // 
            groupBoxDetaylar.Controls.Add(dgvMatrix);
            groupBoxDetaylar.Dock = DockStyle.Fill;
            groupBoxDetaylar.Location = new Point(313, 3);
            groupBoxDetaylar.Name = "groupBoxDetaylar";
            groupBoxDetaylar.Size = new Size(946, 667);
            groupBoxDetaylar.TabIndex = 1;
            groupBoxDetaylar.TabStop = false;
            groupBoxDetaylar.Text = "Detaylar";
            // 
            // dgvMatrix
            // 
            dgvMatrix.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMatrix.Location = new Point(6, 26);
            dgvMatrix.Name = "dgvMatrix";
            dgvMatrix.RowHeadersWidth = 51;
            dgvMatrix.Size = new Size(931, 621);
            dgvMatrix.TabIndex = 0;
            // 
            // ShiftDetailForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 673);
            Controls.Add(tableLayoutPanel1);
            Name = "ShiftDetailForm";
            Text = "ShiftDetailForm";
            Load += ShiftDetailForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            groupBoxBilgiler.ResumeLayout(false);
            groupBoxDetaylar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMatrix).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBoxBilgiler;
        private Button btnExportExcel;
        private Button btnLoadMatrix;
        private ComboBox cmbReviewShift;
        private DateTimePicker dtpReviewDate;
        private GroupBox groupBoxDetaylar;
        private DataGridView dgvMatrix;
    }
}
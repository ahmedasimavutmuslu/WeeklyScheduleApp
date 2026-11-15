using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using WeeklyScheduleApp.Data;
using WeeklyScheduleApp.Models;

namespace WeeklyScheduleApp.Views
{
    public partial class ShiftDetailForm : Form
    {
        public ShiftDetailForm()
        {
            InitializeComponent();

            // Hücre renklendirme olayını bağlıyoruz
            dgvMatrix.CellFormatting += dgvMatrix_CellFormatting;
        }

        private void ShiftDetailForm_Load(object sender, EventArgs e)
        {
            // Vardiya combobox'ını doldurabilir veya dışarıdan parametre alabilirsin
            LoadShiftsToComboBox();
            dtpReviewDate.Value = DateTime.Today;
        }

        private void LoadShiftsToComboBox()
        {
            var shifts = new List<ShiftItem>
                {
                    new ShiftItem { Id = 1, Name = "Sabah (08:00 - 16:00)" },
                    new ShiftItem { Id = 2, Name = "Öğlen (16:00 - 00:00)" },
                    new ShiftItem { Id = 3, Name = "Gece (00:00 - 08:00)" }
                };

            cmbReviewShift.DataSource = shifts;
            cmbReviewShift.DisplayMember = "Name";
            cmbReviewShift.ValueMember = "Id";
        }

        private void btnLoadMatrix_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = dtpReviewDate.Value.Date;
            int shiftId = Convert.ToInt32(cmbReviewShift.SelectedValue);

            LoadMatrixViewToGrid(selectedDate, shiftId);
        }

        private void LoadMatrixViewToGrid(DateTime date, int shiftId)
        {
            using (var db = new AppDbContext())
            {
                // 1. O gün ve vardiyadaki tüm atamaları çekelim
                var schedules = db.ShiftSchedules
                    .Where(s => s.Date.Date == date.Date && s.IdShift == shiftId)
                    .Include(s => s.User)
                    .Include(s => s.TaskAllocations)
                    .ToList();

                if (!schedules.Any())
                {
                    MessageBox.Show("Seçilen tarih ve vardiya için kayıt bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgvMatrix.DataSource = null;
                    return;
                }

                // 2. DataTable oluşturalım
                DataTable dt = new DataTable();
                dt.Columns.Add("Zaman Bloğu", typeof(string));

                // O vardiyadaki çalışanları sütun yapalım
                var employees = schedules.Select(s => s.User).Distinct().OrderBy(u => u.Username).ToList();
                foreach (var emp in employees)
                {
                    dt.Columns.Add(emp.Username, typeof(string));
                }

                // 3. Zaman blokları
                var timeSlots = new List<string>
                {
                    "08:00 - 10:00",
                    "10:00 - 12:00",
                    "12:00 - 14:00",
                    "14:00 - 16:00"
                };

                foreach (var slotStr in timeSlots)
                {
                    DataRow row = dt.NewRow();
                    row["Zaman Bloğu"] = slotStr;

                    var parts = slotStr.Split('-');
                    TimeSpan slotStart = TimeSpan.Parse(parts[0].Trim());
                    TimeSpan slotEnd = TimeSpan.Parse(parts[1].Trim());

                    foreach (var emp in employees)
                    {
                        var allocation = schedules
                            .Where(s => s.UserId == emp.Id)
                            .SelectMany(s => s.TaskAllocations)
                            .FirstOrDefault(a => a.BlockStart == slotStart && a.BlockEnd == slotEnd);

                        row[emp.Username] = allocation != null ? allocation.TaskType : string.Empty;
                    }

                    dt.Rows.Add(row);
                }

                dgvMatrix.DataSource = dt;
            }
        }

        private void dgvMatrix_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null && e.CellStyle != null)
            {
                string cellValue = e.Value.ToString();

                if (cellValue.Contains("Live"))
                {
                    e.CellStyle.BackColor = Color.Yellow;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (cellValue.Contains("Audit"))
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (cellValue.Contains("MsgReport"))
                {
                    e.CellStyle.BackColor = Color.Plum; // Pembe/Mor tonu
                    e.CellStyle.ForeColor = Color.Black;
                }
                else if (cellValue.Contains("Other"))
                {
                    e.CellStyle.BackColor = Color.DarkGray;
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (dgvMatrix.DataSource == null)
            {
                MessageBox.Show("Dışa aktarılacak veri yok!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Dosyası (*.xlsx)|*.xlsx"; // Artık .xlsx uzantısı kullanıyoruz
            sfd.FileName = $"Vardiya_Matris_{dtpReviewDate.Value:yyyyMMdd}.xlsx";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Vardiya Plani");

                        // 1. DataGridView sütun başlıklarını Excel'e aktar
                        for (int i = 0; i < dgvMatrix.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = dgvMatrix.Columns[i].HeaderText;
                            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // 2. Satırları ve hücre renklerini Excel'e aktar
                        for (int i = 0; i < dgvMatrix.Rows.Count; i++)
                        {
                            if (dgvMatrix.Rows[i].IsNewRow) continue;

                            for (int j = 0; j < dgvMatrix.Columns.Count; j++)
                            {
                                var cellValue = dgvMatrix.Rows[i].Cells[j].Value?.ToString() ?? "";
                                var excelCell = worksheet.Cell(i + 2, j + 1);
                                excelCell.Value = cellValue;

                                // 📌 Arayüzdeki renk kurallarını Excel hücresine de uyguluyoruz:
                                if (cellValue.Contains("Live"))
                                {
                                    excelCell.Style.Fill.BackgroundColor = XLColor.Yellow;
                                }
                                else if (cellValue.Contains("Audit"))
                                {
                                    excelCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                                }
                                else if (cellValue.Contains("MsgReport"))
                                {
                                    excelCell.Style.Fill.BackgroundColor = XLColor.Plum;
                                }
                                else if (cellValue.Contains("Other"))
                                {
                                    excelCell.Style.Fill.BackgroundColor = XLColor.DarkGray;
                                    excelCell.Style.Font.FontColor = XLColor.White;
                                }
                            }
                        }

                        // Sütun genişliklerini otomatik ayarla
                        worksheet.Columns().AdjustToContents();

                        // Dosyayı kaydet
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Renkli Excel dosyası başarıyla dışa aktarıldı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya dışa aktarılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class ShiftItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

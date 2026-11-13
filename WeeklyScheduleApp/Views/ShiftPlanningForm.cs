using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeeklyScheduleApp.Data;
using WeeklyScheduleApp.Models;
using WeeklyScheduleApp.Services;

namespace WeeklyScheduleApp.Views
{
    public partial class ShiftPlanningForm : Form
    {
        private User _currentUser;

        public ShiftPlanningForm(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void ShiftPlanningForm_Load(object sender, EventArgs e)
        {
            LoadShifts();
            LoadEmployeesToGrid();
        }

        private void LoadShifts()
        {
            cmbShift.Items.Clear();
            cmbShift.Items.Add(new { Id = 1, Name = "Sabah (08:00 - 16:00)" });
            cmbShift.Items.Add(new { Id = 2, Name = "Öğlen (16:00 - 00:00)" });
            cmbShift.Items.Add(new { Id = 3, Name = "Gece (00:00 - 08:00)" });
            cmbShift.DisplayMember = "Name";
            cmbShift.ValueMember = "Id";
            if (cmbShift.Items.Count > 0) cmbShift.SelectedIndex = 0;
        }

        private void LoadEmployeesToGrid()
        {
            using (var db = new AppDbContext())
            {
                var employees = db.Users.Where(u => u.IsActive).ToList();

                // DataGridView sütunlarını manuel ayarlayalım (Checkbox ve kullanıcı adı için)
                dgvEmployees.AutoGenerateColumns = false;
                dgvEmployees.Columns.Clear();

                // 1. Sütun: Seçim Checkbox'ı
                DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn();
                chkCol.Name = "Select";
                chkCol.HeaderText = "Seç";
                chkCol.Width = 50;
                dgvEmployees.Columns.Add(chkCol);

                // 2. Sütun: Kullanıcı ID (Gizli tutulabilir ama arkada lazım)
                DataGridViewTextBoxColumn idCol = new DataGridViewTextBoxColumn();
                idCol.DataPropertyName = "Id";
                idCol.Name = "UserId";
                idCol.HeaderText = "ID";
                idCol.Visible = false;
                dgvEmployees.Columns.Add(idCol);

                // 3. Sütun: Kullanıcı Adı
                DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
                nameCol.DataPropertyName = "Username";
                nameCol.Name = "Username";
                nameCol.HeaderText = "Çalışan Adı";
                nameCol.ReadOnly = true;
                nameCol.Width = 150;
                dgvEmployees.Columns.Add(nameCol);

                // 4. Sütun: Esnek Başlangıç Saati (Örn: 14:00)
                DataGridViewTextBoxColumn timeCol = new DataGridViewTextBoxColumn();
                timeCol.Name = "CustomStartTime";
                timeCol.HeaderText = "Özel Başlangıç (Boş=Normal)";
                timeCol.Width = 150;
                dgvEmployees.Columns.Add(timeCol);

                // Veriyi bağla
                dgvEmployees.DataSource = employees;
            }
        }

        // 📌 BUTONA BASILDIĞINDA ÇALIŞACAK KISIM (BAĞLANTI NOKTASI)
        private void btnGenerateSchedule_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Seçilen Vardiya ve Tarihi Al
                dynamic selectedShift = cmbShift.SelectedItem;
                int shiftId = selectedShift.Id;
                DateTime selectedDate = dtpDate.Value.Date;

                // 2. Tablodan seçilen (Checkbox'ı işaretlenen) personelleri topla
                List<EmployeeShiftInput> selectedEmployees = new List<EmployeeShiftInput>();

                foreach (DataGridViewRow row in dgvEmployees.Rows)
                {
                    // Yeni satır (boş satır) kontrolü
                    if (row.IsNewRow) continue;

                    // Checkbox hücresinin değerini güvenli bir şekilde alalım
                    bool isSelected = false;
                    if (row.Cells["Select"].Value != null)
                    {
                        bool.TryParse(row.Cells["Select"].Value.ToString(), out isSelected);
                    }

                    if (isSelected)
                    {
                        int userId = Convert.ToInt32(row.Cells["UserId"].Value);
                        string username = row.Cells["Username"].Value?.ToString() ?? "";

                        TimeSpan? customTime = null;
                        if (row.Cells["CustomStartTime"].Value != null)
                        {
                            if (TimeSpan.TryParse(row.Cells["CustomStartTime"].Value.ToString(), out TimeSpan parsedTime))
                            {
                                customTime = parsedTime;
                            }
                        }

                        selectedEmployees.Add(new EmployeeShiftInput
                        {
                            UserId = userId,
                            Username = username,
                            CustomStartTime = customTime
                        });
                    }
                }

                if (selectedEmployees.Count < 2)
                {
                    MessageBox.Show("Vardiya planı oluşturmak için en az 2 çalışan seçmelisiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. Algoritma Servisini Çalıştır
                ShiftAllocationService allocator = new ShiftAllocationService();
                allocator.GenerateSchedule(shiftId, selectedDate, selectedEmployees);

                MessageBox.Show("Vardiya ve görev dağılımı başarıyla oluşturuldu ve veritabanına kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 4. Sonuçları ekrandaki sonuç tablosunda (dgvResult) gösterelim
                LoadResultsToGrid(selectedDate, shiftId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadResultsToGrid(DateTime date, int shiftId)
        {
            using (var db = new AppDbContext())
            {
                // 1. Adım: Veriyi SQLite'ı yormadan önce belleğe (ToList) çekelim
                var schedules = db.ShiftSchedules
                    .Where(s => s.Date.Date == date.Date && s.IdShift == shiftId)
                    .Include(s => s.User)
                    .Include(s => s.TaskAllocations)
                    .ToList(); // 👈 SQLite sorgusu burada biter, gerisi C# tarafında döner

                // 2. Adım: Bellekte (RAM üzerinde) nesneleri birleştirelim
                var allocations = schedules
                    .SelectMany(s => s.TaskAllocations.Select(t => new
                    {
                        Calisan = s.User != null ? s.User.Username : "Bilinmiyor",
                        BlokBaslangic = t.BlockStart.ToString(@"hh\:mm"),
                        BlokBitis = t.BlockEnd.ToString(@"hh\:mm"),
                        Gorev = t.TaskType
                    }))
                    .ToList();

                dgvResult.DataSource = allocations;
            }
        }

        private void btnClearShiftPlans_Click(object sender, EventArgs e)
        {
            using (var db = new AppDbContext())
            {
                // İlişkili tabloları sırasıyla temizleyelim
                db.TaskAllocations.RemoveRange(db.TaskAllocations);
                db.ShiftSchedules.RemoveRange(db.ShiftSchedules);
                db.UserTaskHistories.RemoveRange(db.UserTaskHistories);

                db.SaveChanges();

                MessageBox.Show("Tüm eski vardiya ve görev geçmişi temizlendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

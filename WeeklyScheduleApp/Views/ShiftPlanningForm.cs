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
                // 1. Seçilen Vardiya ve Tarihleri Al
                dynamic selectedShift = cmbShift.SelectedItem;
                int shiftId = selectedShift.Id;

                DateTime startDate = dtpStartDate.Value.Date;

                // Eğer formunuzda bir bitiş tarihi seçicisi (dtpEndDate) varsa onu alın, 
                // yoksa sadece başlangıç tarihi üzerinden tek günlük işlem yapın.
                // (Örn: dtpEndDate adında ikinci bir DateTimePicker eklediğinizi varsayıyoruz)
                DateTime endDate = dtpEndDate != null ? dtpEndDate.Value.Date : startDate;

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

                if (startDate > endDate)
                {
                    MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. Algoritma Servisini Çalıştır
                ShiftAllocationService allocator = new ShiftAllocationService();

                // Tarih aralığının durumuna göre tekli veya toplu metodu çağırıyoruz
                if (startDate == endDate)
                {
                    // Tek günlük üretim
                    allocator.GenerateSchedule(shiftId, startDate, selectedEmployees);
                    MessageBox.Show("Vardiya ve görev dağılımı başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadResultsToGrid(startDate, shiftId);
                }
                else
                {
                    // Toplu (Haftalık / Aylık) üretim döngüsü
                    allocator.GenerateScheduleForDateRange(shiftId, startDate, endDate, selectedEmployees);
                    MessageBox.Show($"{startDate.ToShortDateString()} ile {endDate.ToShortDateString()} arasındaki tüm günler için toplu plan başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadResultsForDateRange(startDate, endDate, shiftId); // İlk günü ekranda göster
                }
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
                    .ToList();

                // 2. Adım: Bellekte (RAM üzerinde) nesneleri birleştirelim
                var allocations = schedules
                    .SelectMany(s => s.TaskAllocations.Select(t => new
                    {
                        AllocationId = t.Id, // 👈 BURAYA EKLENDİ (Güncelleme için şart)
                        Calisan = s.User != null ? s.User.Username : "Bilinmiyor",
                        BlokBaslangic = t.BlockStart.ToString(@"hh\:mm"),
                        BlokBitis = t.BlockEnd.ToString(@"hh\:mm"),
                        Gorev = t.TaskType
                    }))
                    .ToList();

                dgvResult.DataSource = allocations;
                if (dgvResult.Columns["AllocationId"] != null)
                {
                    dgvResult.Columns["AllocationId"].Visible = false;
                }
            }
        }

        private void LoadResultsForDateRange(DateTime startDate, DateTime endDate, int shiftId)
        {
            using (var db = new AppDbContext())
            {
                // Seçilen tarih aralığındaki tüm vardiya ve görev atamalarını çekelim
                var results = db.ShiftSchedules
                    .Where(s => s.Date.Date >= startDate.Date && s.Date.Date <= endDate.Date && s.IdShift == shiftId)
                    .Include(s => s.User)
                    .Include(s => s.TaskAllocations)
                    .OrderBy(s => s.Date)
                    .ToList();

                // DataGridView'de gösterilebilmesi için düz bir liste (flatten) oluşturalım
                var gridDisplayList = new List<object>();

                foreach (var schedule in results)
                {
                    foreach (var allocation in schedule.TaskAllocations)
                    {
                        gridDisplayList.Add(new
                        {
                            AllocationId = allocation.Id, // 👈 BURAYA EKLENDİ (Güncelleme için şart)
                            Tarih = schedule.Date.ToShortDateString(),
                            Gun = schedule.Date.ToString("dddd"),
                            Personel = schedule.User.Username,
                            Baslangic = allocation.BlockStart.ToString(@"hh\:mm"),
                            Bitis = allocation.BlockEnd.ToString(@"hh\:mm"),
                            Gorev = allocation.TaskType
                        });
                    }
                }

                dgvResult.DataSource = gridDisplayList;
                if (dgvResult.Columns["AllocationId"] != null)
                {
                    dgvResult.Columns["AllocationId"].Visible = false;
                }
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

        private void btnManualUpdate_Click(object sender, EventArgs e)
        {
            if (dgvResult.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen değiştirmek istediğiniz vardiya bloğunu tablodan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbEditTaskType.SelectedItem == null)
            {
                MessageBox.Show("Lütfen yeni görev tipini seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçilen satırdan AllocationId değerini güvenli bir şekilde alalım
            var selectedRow = dgvResult.SelectedRows[0];
            int allocationId = Convert.ToInt32(selectedRow.Cells["AllocationId"].Value);
            string newTaskType = cmbEditTaskType.SelectedItem.ToString();

            using (var db = new AppDbContext())
            {
                var allocation = db.TaskAllocations
                    .Include(a => a.ShiftSchedule)
                    .FirstOrDefault(a => a.Id == allocationId);

                if (allocation != null)
                {
                    string oldTaskType = allocation.TaskType;
                    allocation.TaskType = newTaskType;

                    // 📌 İsteğe Bağlı & Önemli: Manuel değişiklik yapıldığında kümülatif adalet geçmişini de (UserTaskHistory) 
                    // tutarlı tutmak isteyebilirsiniz. Ancak şimdilik sadece görev atamasını değiştirmek yeterlidir.

                    db.SaveChanges();

                    MessageBox.Show("Görev başarıyla güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tabloyu tazele (Mevcut tarih aralığını tekrar yükle)
                    LoadResultsForDateRange(dtpStartDate.Value.Date, dtpEndDate.Value.Date, ((dynamic)cmbShift.SelectedItem).Id);
                }
                else
                {
                    MessageBox.Show("Seçilen kayıt veritabanında bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeeklyScheduleApp.Data;
using Microsoft.EntityFrameworkCore;

namespace WeeklyScheduleApp.Views
{
    public partial class FairnessReportForm : Form
    {
        public FairnessReportForm()
        {
            InitializeComponent();
        }

        private void FairnessReportForm_Load(object sender, EventArgs e)
        {
            // Varsayılan olarak son 7 günü (1 haftayı) seçili getirelim
            dtpEndDate.Value = DateTime.Today;
            dtpStartDate.Value = DateTime.Today.AddDays(-7);

            LoadFairnessReport();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            LoadFairnessReport();
        }

        private void LoadFairnessReport()
        {
            DateTime start = dtpStartDate.Value.Date;
            DateTime end = dtpEndDate.Value.Date;

            using (var db = new AppDbContext())
            {
                // 1. Önce aktif çalışanları alalım
                var users = db.Users
                    .Where(u => u.IsActive && u.Role == "Employee")
                    .ToList();

                // 2. Seçilen tarih aralığındaki tüm görev atamalarını SQLite'ı yormadan RAM'e çekelim
                var schedules = db.ShiftSchedules
                    .Where(s => s.Date.Date >= start && s.Date.Date <= end)
                    .Include(s => s.TaskAllocations)
                    .ToList();

                // 3. Her çalışan için görev bazlı toplam saatleri hesaplayalım
                var reportData = users.Select(user =>
                {
                    var userAllocations = schedules
                        .Where(s => s.UserId == user.Id)
                        .SelectMany(s => s.TaskAllocations)
                        .ToList();

                    // Blok sürelerini saat cinsinden toplayalım (Her blok varsayılan 2 saat)
                    double liveHours = userAllocations
                        .Where(a => a.TaskType == "Live")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    double msgHours = userAllocations
                        .Where(a => a.TaskType == "MsgReport")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    double auditHours = userAllocations
                        .Where(a => a.TaskType == "Audit")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    // 2 kişilik vardiyalardaki birleşik görevleri (Live+Audit / MsgReport+Audit) de ekleyelim
                    double comboLiveAudit = userAllocations
                        .Where(a => a.TaskType == "Live+Audit")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    double comboMsgAudit = userAllocations
                        .Where(a => a.TaskType == "MsgReport+Audit")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    double otherHours = userAllocations
                        .Where(a => a.TaskType == "Other")
                        .Sum(a => (a.BlockEnd - a.BlockStart).TotalHours);

                    double totalHours = liveHours + msgHours + auditHours + comboLiveAudit + comboMsgAudit + otherHours;

                    return new
                    {
                        Calisan = user.Username,
                        Live_Saat = liveHours,
                        MsgReport_Saat = msgHours,
                        Audit_Saat = auditHours,
                        Ikili_LiveAudit_Saat = comboLiveAudit,
                        Ikili_MsgAudit_Saat = comboMsgAudit,
                        Other_Saat = otherHours,
                        Toplam_Saat = totalHours
                    };
                }).ToList();

                dgvReport.DataSource = reportData;
            }
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Tüm vardiya planları, görev atamaları ve çalışma geçmişi silinecek. Emin misiniz?",
                "Verileri Sıfırla",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    // Çalışan hesaplarına dokunmadan sadece vardiya ve süre geçmişini temizler
                    db.TaskAllocations.RemoveRange(db.TaskAllocations);
                    db.ShiftSchedules.RemoveRange(db.ShiftSchedules);
                    db.UserTaskHistories.RemoveRange(db.UserTaskHistories);

                    db.SaveChanges();
                }

                MessageBox.Show("Tüm deneme verileri temizlendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Eğer bu butonu FairnessReportForm'a koyduysanız tabloyu hemen yenileyin:
                LoadFairnessReport();
            }
        }
    }
}

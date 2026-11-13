using Microsoft.EntityFrameworkCore;
using WeeklyScheduleApp.Data;
using WeeklyScheduleApp.Models;

namespace WeeklyScheduleApp.Services
{
    public class ShiftAllocationService
    {
        private readonly List<string> _coreTasks = new List<string> { "Live", "MsgReport", "Audit" };

        public void GenerateSchedule(int shiftId, DateTime date, List<EmployeeShiftInput> selectedEmployees)
        {
            using var db = new AppDbContext();

            // 1. Adım: Vardiya saat aralığını belirle
            (TimeSpan shiftStart, TimeSpan shiftEnd) = GetShiftTimes(shiftId);

            // 2. Adım: 2 saatlik 4 bloğu oluştur (0-2 saat, 2-4 saat, 4-6 saat, 6-8 saat)
            var blocks = GenerateTimeBlocks(shiftStart);

            // Her çalışan için son atanan görevi tutmak (ardışık görev yasağı için)
            Dictionary<int, string> lastAssignedTask = new Dictionary<int, string>();

            for (int blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
            {
                var block = blocks[blockIndex];
                int employeeCount = selectedEmployees.Count;

                if (employeeCount == 2)
                {
                    // 2 kişi için blok indeksini de gönderiyoruz (Rotasyon için)
                    AssignForTwoEmployees(selectedEmployees, block, blockIndex, db, date, shiftId);
                }
                else if (employeeCount == 3)
                {
                    AssignForThreeEmployees(selectedEmployees, block, lastAssignedTask, db, date, shiftId);
                }
                else if (employeeCount >= 4)
                {
                    AssignForFourOrMoreEmployees(selectedEmployees, block, lastAssignedTask, db, date, shiftId);
                }
            }

            db.SaveChanges();
        }

        private (TimeSpan start, TimeSpan end) GetShiftTimes(int shiftId)
        {
            return shiftId switch
            {
                1 => (new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),   // Sabah
                2 => (new TimeSpan(16, 0, 0), new TimeSpan(0, 0, 0)),  // Öğlen
                3 => (new TimeSpan(0, 0, 0), new TimeSpan(8, 0, 0)),   // Gece
                _ => (new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0))
            };
        }

        private List<(TimeSpan Start, TimeSpan End)> GenerateTimeBlocks(TimeSpan shiftStart)
        {
            var blocks = new List<(TimeSpan, TimeSpan)>();
            for (int i = 0; i < 4; i++)
            {
                TimeSpan blockStart = shiftStart.Add(TimeSpan.FromHours(i * 2));
                TimeSpan blockEnd = blockStart.Add(TimeSpan.FromHours(2));
                blocks.Add((blockStart, blockEnd));
            }
            return blocks;
        }

        private void AssignForTwoEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, int blockIndex, AppDbContext db, DateTime date, int shiftId)
        {
            // 2 kişi için olası iki ana kombinasyon:
            // Kombinasyon A: 1. Çalışan = Live + Audit, 2. Çalışan = MsgReport + Audit
            // Kombinasyon B: 1. Çalışan = MsgReport + Audit, 2. Çalışan = Live + Audit

            string task1, task2;

            // Blok indeksine göre (0, 1, 2, 3) görevleri kendi aralarında döndürelim (Rotasyon)
            if (blockIndex % 2 == 0)
            {
                task1 = "Live+Audit";
                task2 = "MsgReport+Audit";
            }
            else
            {
                task1 = "MsgReport+Audit";
                task2 = "Live+Audit";
            }

            // Çalışanlara ata
            SaveAllocation(db, employees[0].UserId, block.Start, block.End, task1, date, shiftId);
            SaveAllocation(db, employees[1].UserId, block.Start, block.End, task2, date, shiftId);
        }

        private void AssignForThreeEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            // O blok için tüm ana görevlerin havuzu (Her görev 1 kez yapılmalı)
            var availableTasks = new List<string>(_coreTasks);

            foreach (var emp in employees)
            {
                // Aday görevler: Hem bu blokta henüz seçilmemiş olanlar hem de kişinin kendi son görevi (ardışık yasağı) hariç tutulur
                var candidateTasks = new List<string>(availableTasks);

                if (lastAssignedTask.TryGetValue(emp.UserId, out string? lastTask))
                {
                    candidateTasks.Remove(lastTask);
                }

                // Eğer ardışık yasak yüzünden havuz tamamen bittiyse, en azından blokta kalan görevlerden devam et
                if (candidateTasks.Count == 0)
                {
                    candidateTasks = new List<string>(availableTasks);
                }

                // Haftalık adalet kuralına göre (eşitlik durumunda rastgele seçen) görevi seç
                string chosenTask = GetFairTaskForUser(db, emp.UserId, candidateTasks);

                SaveAllocation(db, emp.UserId, block.Start, block.End, chosenTask, date, shiftId);

                // Seçilen görevi bu çalışan için son görev olarak kaydet ve genel havuzdan düş
                lastAssignedTask[emp.UserId] = chosenTask;
                availableTasks.Remove(chosenTask);
            }
        }

        private void AssignForFourOrMoreEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            foreach (var emp in employees)
            {
                // 1. Önceki blokta bu kişi "Other" almış mı kontrol edelim
                bool hadOtherLastBlock = false;
                if (lastAssignedTask.TryGetValue(emp.UserId, out string? lastTask) && lastTask == "Other")
                {
                    hadOtherLastBlock = true;
                }

                string chosenTask;

                // Eğer son blokta Other aldıysa, bu blokta KESİNLİKLE Other veremeyiz (Ardışık Other yasağı)
                bool canTakeOther = !hadOtherLastBlock && CheckIfUserEligibleForOther(db, emp.UserId);

                if (canTakeOther && new Random().Next(0, 2) == 1) // %50 şans veya dengeleme mantığıyla
                {
                    chosenTask = "Other";
                }
                else
                {
                    var availableTasks = new List<string>(_coreTasks); // "Live", "MsgReport", "Audit"

                    // Normal ardışık görev yasağı (Eğer son görev core task ise onu bu blokta hariç tut)
                    if (lastTask != null && lastTask != "Other" && availableTasks.Contains(lastTask))
                    {
                        availableTasks.Remove(lastTask);
                    }
                    if (availableTasks.Count == 0) availableTasks = new List<string>(_coreTasks);

                    chosenTask = GetFairTaskForUser(db, emp.UserId, availableTasks);
                }

                SaveAllocation(db, emp.UserId, block.Start, block.End, chosenTask, date, shiftId);

                // Bu çalışanın bu bloktaki son görevini güncelle
                lastAssignedTask[emp.UserId] = chosenTask;
            }
        }

        private string GetFairTaskForUser(AppDbContext db, int userId, List<string> candidateTasks)
        {
            // Haftalık kümülatif süreleri al
            var userHistory = db.UserTaskHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.TaskType)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.MinutesWorked));

            int minMinutes = int.MaxValue;

            // Adaylar arasından en az süre çalışılan dakikayı bul
            foreach (var task in candidateTasks)
            {
                int minutes = userHistory.TryGetValue(task, out int val) ? val : 0;
                if (minutes < minMinutes)
                {
                    minMinutes = minutes;
                }
            }

            // En az süreye sahip olan (eşit olan) tüm görevleri listele (Örn: Hem Live hem Audit 0 dakika ise ikisi de adaydır)
            var bestTasks = candidateTasks.Where(task =>
            {
                int minutes = userHistory.TryGetValue(task, out int val) ? val : 0;
                return minutes == minMinutes;
            }).ToList();

            // 📌 KRİTİK DÜZELTME: Süreler eşitse hep ilkini seçmek yerine aralarından rastgele seç (Adaleti sağlar, Live yığılmasını önler)
            int index = new Random().Next(bestTasks.Count);
            return bestTasks[index];
        }

        private bool CheckIfUserEligibleForOther(AppDbContext db, int userId)
        {
            // Haftalık Live, MsgReport ve Audit sürelerinin birbirine yakın olup olmadığını kontrol eder (Tolerans: ±2 saat)
            var history = db.UserTaskHistories
                .Where(h => h.UserId == userId && h.TaskType != "Other")
                .GroupBy(h => h.TaskType)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.MinutesWorked));

            if (history.Count < 3) return false; // Henüz tüm alanlarda çalışmamışsa Other alamaz

            int live = history.GetValueOrDefault("Live", 0);
            int msg = history.GetValueOrDefault("MsgReport", 0);
            int audit = history.GetValueOrDefault("Audit", 0);

            int max = Math.Max(live, Math.Max(msg, audit));
            int min = Math.Min(live, Math.Min(msg, audit));

            // Fark 120 dakikadan (2 saatten) azsa dengelidir, Other alabilir
            return (max - min) <= 120;
        }

        private void SaveAllocation(AppDbContext db, int userId, TimeSpan start, TimeSpan end, string taskType, DateTime date, int shiftId)
        {
            // ShiftSchedule bul veya oluştur
            var schedule = db.ShiftSchedules
                .FirstOrDefault(s => s.UserId == userId && s.Date.Date == date.Date && s.IdShift == shiftId);

            if (schedule == null)
            {
                schedule = new ShiftSchedule
                {
                    UserId = userId,
                    IdShift = shiftId,
                    Date = date.Date
                };
                db.ShiftSchedules.Add(schedule);
                db.SaveChanges();
            }

            // TaskAllocation ekle
            var allocation = new TaskAllocation
            {
                ShiftScheduleId = schedule.Id,
                BlockStart = start,
                BlockEnd = end,
                TaskType = taskType
            };
            db.TaskAllocations.Add(allocation);

            // Eğer Other değilse kümülatif geçmişe (UserTaskHistory) 120 dk ekle
            if (taskType != "Other" && !taskType.Contains("+"))
            {
                db.UserTaskHistories.Add(new UserTaskHistory
                {
                    UserId = userId,
                    Date = date.Date,
                    TaskType = taskType,
                    MinutesWorked = 120
                });
            }
        }
    }

    public class EmployeeShiftInput
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public TimeSpan? CustomStartTime { get; set; }
    }
}
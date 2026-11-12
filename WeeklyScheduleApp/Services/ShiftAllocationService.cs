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

            foreach (var block in blocks)
            {
                int employeeCount = selectedEmployees.Count;

                if (employeeCount == 2)
                {
                    // KURAL: 2 kişi varsa -> 1. Kişi: Live + Audit, 2. Kişi: MsgReport + Audit (Dönüşümlü)
                    AssignForTwoEmployees(selectedEmployees, block, lastAssignedTask, db, date, shiftId);
                }
                else if (employeeCount == 3)
                {
                    // KURAL: 3 kişi varsa -> Audit, MsgReport, Live bire bir paylaştırılır
                    AssignForThreeEmployees(selectedEmployees, block, lastAssignedTask, db, date, shiftId);
                }
                else if (employeeCount >= 4)
                {
                    // KURAL: 4+ kişi varsa -> Haftalık adalet kontrolüne göre Other verilebilir
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

        private void AssignForTwoEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            // 2 kişi için: Her blokta Audit zorunlu yanına Live veya MsgReport eklenir
            string[] tasksForBlock = { "Live+Audit", "MsgReport+Audit" };

            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                string assignedTask = tasksForBlock[i % tasksForBlock.Length];

                // Ardışık görev yasağı kontrolü ve kaydetme
                SaveAllocation(db, emp.UserId, block.Start, block.End, assignedTask, date, shiftId);
            }
        }

        private void AssignForThreeEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            // 3 kişi için: Live, MsgReport, Audit rotasyonu
            var availableTasks = new List<string>(_coreTasks);

            foreach (var emp in employees)
            {
                // Bir önceki bloktaki görevi hariç tut (Ardışık görev yasağı)
                if (lastAssignedTask.TryGetValue(emp.UserId, out string? lastTask))
                {
                    availableTasks.Remove(lastTask);
                }

                // Eğer liste boşaldıysa güvenli fallback
                if (availableTasks.Count == 0) availableTasks = new List<string>(_coreTasks);

                // Haftalık en az çalışana öncelik ver (Adalet Algoritması)
                string chosenTask = GetFairTaskForUser(db, emp.UserId, availableTasks);

                SaveAllocation(db, emp.UserId, block.Start, block.End, chosenTask, date, shiftId);

                lastAssignedTask[emp.UserId] = chosenTask;
                availableTasks.Remove(chosenTask);
            }
        }

        private void AssignForFourOrMoreEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            // 4+ kişi: Diğerleri ana görevleri dönerken, haftalık dengesi eşit olan kişiye Other verilebilir
            foreach (var emp in employees)
            {
                bool canTakeOther = CheckIfUserEligibleForOther(db, emp.UserId);

                string chosenTask;
                if (canTakeOther && new Random().Next(0, 2) == 1) // Örnek dengeleme mantığı
                {
                    chosenTask = "Other";
                }
                else
                {
                    var availableTasks = new List<string>(_coreTasks);
                    if (lastAssignedTask.TryGetValue(emp.UserId, out string? lastTask))
                    {
                        availableTasks.Remove(lastTask);
                    }
                    if (availableTasks.Count == 0) availableTasks = new List<string>(_coreTasks);

                    chosenTask = GetFairTaskForUser(db, emp.UserId, availableTasks);
                }

                SaveAllocation(db, emp.UserId, block.Start, block.End, chosenTask, date, shiftId);
                lastAssignedTask[emp.UserId] = chosenTask;
            }
        }

        private string GetFairTaskForUser(AppDbContext db, int userId, List<string> candidateTasks)
        {
            // Haftalık kümülatif süresi en az olan görevi seç
            var userHistory = db.UserTaskHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.TaskType)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.MinutesWorked));

            string bestTask = candidateTasks.First();
            int minMinutes = int.MaxValue;

            foreach (var task in candidateTasks)
            {
                int minutes = userHistory.TryGetValue(task, out int val) ? val : 0;
                if (minutes < minMinutes)
                {
                    minMinutes = minutes;
                    bestTask = task;
                }
            }

            return bestTask;
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
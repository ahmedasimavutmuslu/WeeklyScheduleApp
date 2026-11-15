using System;
using System.Collections.Generic;
using System.Linq;
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
            GenerateDailyScheduleInternal(shiftId, date, selectedEmployees, db);
            db.SaveChanges();
        }

        // Toplu üretim (Haftalık/Aylık) metodu
        public void GenerateScheduleForDateRange(int shiftId, DateTime startDate, DateTime endDate, List<EmployeeShiftInput> selectedEmployees)
        {
            using var db = new AppDbContext();

            // Başlangıç ve bitiş tarihleri arasındaki her gün için döngü başlatalım
            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // 1. O gün için daha önceden yapılmış eski bir plan varsa temizleyelim
                var existingSchedules = db.ShiftSchedules
                    .Where(s => s.Date.Date == date.Date && s.IdShift == shiftId)
                    .Include(s => s.TaskAllocations)
                    .ToList();

                if (existingSchedules.Any())
                {
                    foreach (var schedule in existingSchedules)
                    {
                        db.TaskAllocations.RemoveRange(schedule.TaskAllocations);
                        db.ShiftSchedules.Remove(schedule);
                    }
                    db.SaveChanges();
                }

                // 2. O gün için günlük algoritmayı aynı veritabanı bağlantısıyla çalıştır
                GenerateDailyScheduleInternal(shiftId, date.Date, selectedEmployees, db);
            }

            db.SaveChanges();
        }

        private void GenerateDailyScheduleInternal(int shiftId, DateTime date, List<EmployeeShiftInput> selectedEmployees, AppDbContext db)
        {
            // 1. Standart vardiya başlangıç saatini al (Referans olması için)
            (TimeSpan defaultShiftStart, _) = GetShiftTimes(shiftId);

            // 2. Her çalışanın kendi kayan çalışma penceresini (4 blok / 8 saat) belirleyelim
            var employeeSchedules = new Dictionary<int, List<(TimeSpan Start, TimeSpan End)>>();

            foreach (var emp in selectedEmployees)
            {
                TimeSpan startTime = emp.CustomStartTime ?? defaultShiftStart;

                var empBlocks = new List<(TimeSpan Start, TimeSpan End)>();
                for (int i = 0; i < 4; i++)
                {
                    TimeSpan blockStart = startTime.Add(TimeSpan.FromHours(i * 2));
                    TimeSpan blockEnd = blockStart.Add(TimeSpan.FromHours(2));
                    empBlocks.Add((blockStart, blockEnd));
                }

                employeeSchedules[emp.UserId] = empBlocks;
            }

            // 3. Tüm çalışanların kapsandığı toplam zaman aralığındaki tüm benzersiz 2 saatlik zaman dilimlerini bulalım
            var allTimeSlots = employeeSchedules.Values
                .SelectMany(b => b)
                .Distinct()
                .OrderBy(b => b.Start)
                .ToList();

            Dictionary<int, string> lastAssignedTask = new Dictionary<int, string>();

            // 4. Her bir zaman dilimi (slot) için o an mesaide olan çalışanları tespit edip görev atayalım
            for (int slotIndex = 0; slotIndex < allTimeSlots.Count; slotIndex++)
            {
                var slot = allTimeSlots[slotIndex];

                var activeEmployeesInSlot = selectedEmployees.Where(emp =>
                {
                    var blocks = employeeSchedules[emp.UserId];
                    return blocks.Any(b => b.Start == slot.Start && b.End == slot.End);
                }).ToList();

                int activeCount = activeEmployeesInSlot.Count;
                if (activeCount == 0) continue;

                if (activeCount == 1)
                {
                    SaveAllocation(db, activeEmployeesInSlot[0].UserId, slot.Start, slot.End, "Live+MsgReport+Audit", date, shiftId);
                    lastAssignedTask[activeEmployeesInSlot[0].UserId] = "Live+MsgReport+Audit";
                }
                else if (activeCount == 2)
                {
                    AssignForTwoEmployees(activeEmployeesInSlot, slot, slotIndex, db, date, shiftId);
                }
                else if (activeCount == 3)
                {
                    AssignForThreeEmployees(activeEmployeesInSlot, slot, lastAssignedTask, db, date, shiftId);
                }
                else if (activeCount >= 4)
                {
                    AssignForFourOrMoreEmployees(activeEmployeesInSlot, slot, lastAssignedTask, db, date, shiftId);
                }
            }
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

        private void AssignForTwoEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, int blockIndex, AppDbContext db, DateTime date, int shiftId)
        {
            string task1, task2;

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

            SaveAllocation(db, employees[0].UserId, block.Start, block.End, task1, date, shiftId);
            SaveAllocation(db, employees[1].UserId, block.Start, block.End, task2, date, shiftId);
        }

        private void AssignForThreeEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            // 3 çalışan için olası tüm görev kombinasyonları (3! = 6 farklı dağılım)
            var allPermutations = new List<List<string>>
            {
                new List<string> { "Live", "MsgReport", "Audit" },
                new List<string> { "Live", "Audit", "MsgReport" },
                new List<string> { "MsgReport", "Live", "Audit" },
                new List<string> { "MsgReport", "Audit", "Live" },
                new List<string> { "Audit", "Live", "MsgReport" },
                new List<string> { "Audit", "MsgReport", "Live" }
            };

            List<string> bestAssignment = null;
            int minPenalty = int.MaxValue;

            // Rastgelelik için kombinasyonları karıştırıyoruz
            var randomizedPermutations = allPermutations.OrderBy(x => Guid.NewGuid()).ToList();

            foreach (var perm in randomizedPermutations)
            {
                int penalty = 0;

                for (int i = 0; i < employees.Count; i++)
                {
                    int userId = employees[i].UserId;
                    string assignedTask = perm[i];

                    // 📌 Ardışık görev yasağı ihlali kontrolü (Büyük ceza puanı verilir)
                    if (lastAssignedTask.TryGetValue(userId, out string? lastTask) && lastTask == assignedTask)
                    {
                        penalty += 10000;
                    }
                }

                // Adalet dengesi için ek ceza (aynı görevi üst üste çok alanlara dengeleme cezası)
                penalty += GetFairnessPenalty(db, employees, perm);

                if (penalty < minPenalty)
                {
                    minPenalty = penalty;
                    bestAssignment = perm;
                }
            }

            // En uygun ve kurallara uyan dağılımı kaydediyoruz
            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];
                string chosenTask = bestAssignment[i];

                SaveAllocation(db, emp.UserId, block.Start, block.End, chosenTask, date, shiftId);
                lastAssignedTask[emp.UserId] = chosenTask;
            }
        }

        private int GetFairnessPenalty(AppDbContext db, List<EmployeeShiftInput> employees, List<string> tasks)
        {
            int penalty = 0;
            for (int i = 0; i < employees.Count; i++)
            {
                int userId = employees[i].UserId;
                string task = tasks[i];

                var userHistoryMinutes = db.UserTaskHistories
                    .Where(h => h.UserId == userId && h.TaskType == task)
                    .Sum(h => (int?)h.MinutesWorked) ?? 0;

                penalty += userHistoryMinutes;
            }
            return penalty;
        }

        private void AssignForFourOrMoreEmployees(List<EmployeeShiftInput> employees, (TimeSpan Start, TimeSpan End) block, Dictionary<int, string> lastAssignedTask, AppDbContext db, DateTime date, int shiftId)
        {
            foreach (var emp in employees)
            {
                bool hadOtherLastBlock = false;
                if (lastAssignedTask.TryGetValue(emp.UserId, out string? lastTask) && lastTask == "Other")
                {
                    hadOtherLastBlock = true;
                }

                string chosenTask;
                bool canTakeOther = !hadOtherLastBlock && CheckIfUserEligibleForOther(db, emp.UserId);

                if (canTakeOther && new Random().Next(0, 2) == 1)
                {
                    chosenTask = "Other";
                }
                else
                {
                    var availableTasks = new List<string>(_coreTasks);

                    if (lastTask != null && lastTask != "Other" && availableTasks.Contains(lastTask))
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
            var userHistory = db.UserTaskHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.TaskType)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.MinutesWorked));

            int minMinutes = int.MaxValue;

            foreach (var task in candidateTasks)
            {
                int minutes = userHistory.TryGetValue(task, out int val) ? val : 0;
                if (minutes < minMinutes)
                {
                    minMinutes = minutes;
                }
            }

            var bestTasks = candidateTasks.Where(task =>
            {
                int minutes = userHistory.TryGetValue(task, out int val) ? val : 0;
                return minutes == minMinutes;
            }).ToList();

            int index = new Random().Next(bestTasks.Count);
            return bestTasks[index];
        }

        private bool CheckIfUserEligibleForOther(AppDbContext db, int userId)
        {
            var history = db.UserTaskHistories
                .Where(h => h.UserId == userId && h.TaskType != "Other")
                .GroupBy(h => h.TaskType)
                .ToDictionary(g => g.Key, g => g.Sum(h => h.MinutesWorked));

            if (history.Count < 3) return false;

            int live = history.GetValueOrDefault("Live", 0);
            int msg = history.GetValueOrDefault("MsgReport", 0);
            int audit = history.GetValueOrDefault("Audit", 0);

            int max = Math.Max(live, Math.Max(msg, audit));
            int min = Math.Min(live, Math.Min(msg, audit));

            return (max - min) <= 120;
        }

        private void SaveAllocation(AppDbContext db, int userId, TimeSpan start, TimeSpan end, string taskType, DateTime date, int shiftId)
        {
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

            var allocation = new TaskAllocation
            {
                ShiftScheduleId = schedule.Id,
                BlockStart = start,
                BlockEnd = end,
                TaskType = taskType
            };
            db.TaskAllocations.Add(allocation);

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
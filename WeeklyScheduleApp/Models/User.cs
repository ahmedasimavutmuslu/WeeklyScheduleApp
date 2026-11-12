using System;
using System.Collections.Generic;
using System.Text;

namespace WeeklyScheduleApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; 
        public string Role { get; set; } = "Employee"; 
        public bool IsActive { get; set; } = true;

        public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
        public ICollection<UserTaskHistory> TaskHistories { get; set; } = new List<UserTaskHistory>();
    }
}


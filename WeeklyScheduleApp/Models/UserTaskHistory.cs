using System;
using System.Collections.Generic;
using System.Text;

namespace WeeklyScheduleApp.Models
{
    public class UserTaskHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime Date { get; set; }
        public string TaskType { get; set; } = string.Empty; 
        public int MinutesWorked { get; set; } = 120; 
    }
}

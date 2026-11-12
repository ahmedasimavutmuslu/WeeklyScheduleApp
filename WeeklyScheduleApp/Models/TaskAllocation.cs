using System;
using System.Collections.Generic;
using System.Text;

namespace WeeklyScheduleApp.Models
{
    public class TaskAllocation
    {
        public int Id { get; set; }
        public int ShiftScheduleId { get; set; }
        public ShiftSchedule? ShiftSchedule { get; set; }

        public TimeSpan BlockStart { get; set; } 
        public TimeSpan BlockEnd { get; set; }   
        public string TaskType { get; set; } = string.Empty; 
    }
}

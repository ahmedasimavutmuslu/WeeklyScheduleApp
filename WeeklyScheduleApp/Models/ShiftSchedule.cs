using System;
using System.Collections.Generic;
using System.Text;

namespace WeeklyScheduleApp.Models
{
    public class ShiftSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int IdShift { get; set; }
        public Shift? Shift { get; set; }

        public DateTime Date { get; set; } 
        public TimeSpan? CustomStartTime { get; set; } 
        public TimeSpan? CustomEndTime { get; set; }

        public ICollection<TaskAllocation> TaskAllocations { get; set; } = new List<TaskAllocation>();
    }
}
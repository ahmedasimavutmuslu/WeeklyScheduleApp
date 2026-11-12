using System;
using System.Collections.Generic;
using System.Text;

namespace WeeklyScheduleApp.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public TimeSpan DefaultStartTime { get; set; }
        public TimeSpan DefaultEndTime { get; set; }

        public ICollection<ShiftSchedule> ShiftSchedules { get; set; } = new List<ShiftSchedule>();
    }
}

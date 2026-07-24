using System;

namespace FlightPlan.Core.Models
{
    public enum AlarmType
    {
        Timer,      // Countdown timer (e.g., 10 minutes)
        Countdown   // Countdown to a specific date/time (e.g., Launch day)
    }

    public class AlarmItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public AlarmType Type { get; set; }
        public DateTime? TargetTime { get; set; } // For Countdown
        public TimeSpan? Duration { get; set; }   // For Timer
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
    }
}

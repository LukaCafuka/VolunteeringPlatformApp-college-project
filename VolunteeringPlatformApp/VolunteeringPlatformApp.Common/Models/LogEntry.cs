using System;

namespace VolunteeringPlatformApp.Common.Models
{
    public partial class LogEntry
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Level { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? Exception { get; set; }
    }
} 
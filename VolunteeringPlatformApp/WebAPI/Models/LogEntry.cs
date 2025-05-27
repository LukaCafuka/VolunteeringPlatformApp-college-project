using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}

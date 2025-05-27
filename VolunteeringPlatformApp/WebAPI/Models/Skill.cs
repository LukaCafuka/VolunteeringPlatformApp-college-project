using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Models;

public partial class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [JsonIgnore]
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

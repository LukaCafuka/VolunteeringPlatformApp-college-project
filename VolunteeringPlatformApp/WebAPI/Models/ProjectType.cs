using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class ProjectType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

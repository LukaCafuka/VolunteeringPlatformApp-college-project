using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class Project
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ProjectTypeId { get; set; }

    public virtual ProjectType? ProjectType { get; set; }

    public virtual ICollection<AppUser> Appusers { get; set; } = new List<AppUser>();

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}

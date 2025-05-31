using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public partial class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project title is required.")]
    [StringLength(100, ErrorMessage = "Project title cannot exceed 100 characters.")]
    [Display(Name = "Project Title")]
    public string Title { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Project Type")]
    public int? ProjectTypeId { get; set; }

    public virtual ProjectType? ProjectType { get; set; }

    public virtual ICollection<AppUser> Appusers { get; set; } = new List<AppUser>();

    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}

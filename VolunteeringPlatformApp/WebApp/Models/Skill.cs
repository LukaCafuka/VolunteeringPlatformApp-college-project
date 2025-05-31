using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public partial class Skill
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Skill name is required.")]
    [StringLength(70, ErrorMessage = "Skill name cannot exceed 70 characters.")]
    [Display(Name = "Skill Name")]
    public string Name { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

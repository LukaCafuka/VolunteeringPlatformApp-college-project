using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public partial class ProjectType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project type name is required.")]
    [StringLength(70, ErrorMessage = "Type name cannot exceed 70 characters.")]
    [Display(Name = "Type Name")]
    public string Name { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

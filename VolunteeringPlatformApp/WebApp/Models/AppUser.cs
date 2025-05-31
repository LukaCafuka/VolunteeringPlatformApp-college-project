using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public partial class AppUser
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email address is required.")]
    [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = null!;

    public string PswdHash { get; set; } = null!;

    public string PswdSalt { get; set; } = null!;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(256, ErrorMessage = "First name cannot exceed 256 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(256, ErrorMessage = "Last name cannot exceed 256 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Administrator")]
    public bool IsAdmin { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

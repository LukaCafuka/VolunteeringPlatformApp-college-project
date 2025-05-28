using System;
using System.Collections.Generic;

namespace WebApp.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PswdHash { get; set; } = null!;

    public string PswdSalt { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}

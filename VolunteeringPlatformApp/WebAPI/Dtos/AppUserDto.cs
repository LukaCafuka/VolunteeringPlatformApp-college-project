using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos
{
    public class AppUserDto
    {
        public int Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; }
        
        public List<string> ProjectTitles { get; set; } = new List<string>();
    }
    
    public class AppUserUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [StringLength(256)]
        public string? FirstName { get; set; }
        
        [StringLength(256)]
        public string? LastName { get; set; }
        
        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public string? NewPassword { get; set; }
    }
} 
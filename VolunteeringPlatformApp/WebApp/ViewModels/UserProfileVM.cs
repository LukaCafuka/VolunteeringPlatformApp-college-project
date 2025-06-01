using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class UserProfileVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 50 characters long")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 50 characters long")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Provide a correct e-mail address")]
        public string Email { get; set; }

        public bool IsAdmin { get; set; }
    }
} 
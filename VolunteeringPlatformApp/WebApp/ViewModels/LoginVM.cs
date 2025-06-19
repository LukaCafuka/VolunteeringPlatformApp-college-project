using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(ValidationConstants.UsernameMaxLength, MinimumLength = ValidationConstants.UsernameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.UsernameLength)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.PasswordLength)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}

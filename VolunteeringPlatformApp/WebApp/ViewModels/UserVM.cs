using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class UserVM
    {
        public int Id { get; set; }

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

        [Required(ErrorMessage = "First name is required")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.NameLength)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.NameLength)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = ValidationConstants.ErrorMessages.EmailFormat)]
        [StringLength(ValidationConstants.EmailMaxLength)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
    }
}

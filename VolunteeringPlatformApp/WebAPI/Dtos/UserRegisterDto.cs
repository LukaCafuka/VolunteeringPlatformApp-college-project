using System.ComponentModel.DataAnnotations;
using VolunteeringPlatformApp.Common.Constants;

namespace WebAPI.Dtos
{
    public class UserRegisterDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(ValidationConstants.UsernameMaxLength, MinimumLength = ValidationConstants.UsernameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.UsernameLength)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.PasswordLength)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.NameLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(ValidationConstants.NameMaxLength, MinimumLength = ValidationConstants.NameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.NameLength)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = ValidationConstants.ErrorMessages.EmailFormat)]
        [StringLength(ValidationConstants.EmailMaxLength)]
        public string Email { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; } = false;
    }
}

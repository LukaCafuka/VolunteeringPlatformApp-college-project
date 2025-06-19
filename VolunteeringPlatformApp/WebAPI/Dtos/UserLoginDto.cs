using System.ComponentModel.DataAnnotations;
using VolunteeringPlatformApp.Common.Constants;

namespace WebAPI.Dtos
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(ValidationConstants.UsernameMaxLength, MinimumLength = ValidationConstants.UsernameMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.UsernameLength)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(ValidationConstants.PasswordMaxLength, MinimumLength = ValidationConstants.PasswordMinLength, 
            ErrorMessage = ValidationConstants.ErrorMessages.PasswordLength)]
        public string Password { get; set; } = string.Empty;
    }
}

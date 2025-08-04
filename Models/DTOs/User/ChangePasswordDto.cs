using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.DTOs.User
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Password confirmation does not match")]
        public string ConfirmPassword { get; set; }
    }
}

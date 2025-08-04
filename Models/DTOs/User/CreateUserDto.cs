using JobPortal.Api.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public int? CompanyId { get; set; }
    }
}

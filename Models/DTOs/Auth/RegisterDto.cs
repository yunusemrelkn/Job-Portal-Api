using JobPortal.Api.Models.Entities;

namespace JobPortal.Api.Models.DTOs.Auth
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public int? CompanyId { get; set; } // For employers
    }
}

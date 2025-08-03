using JobPortal.Api.Models.Entities;

namespace JobPortal.Api.Models.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public EmploymentDto? CurrentEmployment { get; set; }
    }
}

namespace JobPortal.Api.Models.DTOs.User
{
    public class EmploymentDto
    {
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public string? CompanyLocation { get; set; }
        public DateTime StartDate { get; set; }
    }
}

namespace JobPortal.Api.Models.DTOs.Company
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string SectorName { get; set; }
        public int EmployeeCount { get; set; }
        public int JobCount { get; set; }
    }
}

namespace JobPortal.Api.Models.DTOs.Company
{
    public class CreateCompanyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int SectorId { get; set; }

    }
}
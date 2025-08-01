namespace JobPortal.Api.Models.DTOs.Job
{
    public class CreateJobDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public int DepartmentId { get; set; }
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}

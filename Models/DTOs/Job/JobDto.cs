namespace JobPortal.Api.Models.DTOs.Job
{
    public class JobDto
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public bool IsFavorited { get; set; } = false;
        public bool HasApplied { get; set; } = false;
        public bool IsFilled { get; set; } = false;
    }

}

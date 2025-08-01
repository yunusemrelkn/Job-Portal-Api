namespace JobPortal.Api.Models.DTOs.CV
{
    public class CVDto
    {
        public int CvId { get; set; }
        public int UserId { get; set; }
        public string Summary { get; set; }
        public int? ExperienceYears { get; set; }
        public string EducationLevel { get; set; }
        public string SkillsText { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }
}

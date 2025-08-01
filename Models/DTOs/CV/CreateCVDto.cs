namespace JobPortal.Api.Models.DTOs.CV
{
    public class CreateCVDto
    {
        public string Summary { get; set; }
        public int? ExperienceYears { get; set; }
        public string EducationLevel { get; set; }
        public string SkillsText { get; set; }
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}

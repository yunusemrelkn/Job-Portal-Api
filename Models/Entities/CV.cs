using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class CV
    {
        [Key]
        public int CvId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(1000)]
        public string? Summary { get; set; }

        public int? ExperienceYears { get; set; }

        [StringLength(100)]
        public string? EducationLevel { get; set; }

        [StringLength(2000)]
        public string? SkillsText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public ICollection<CVSkill> CVSkills { get; set; } = new List<CVSkill>();
    }
}

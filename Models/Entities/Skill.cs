using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.Entities
{
    public class Skill
    {
        [Key]
        public int SkillId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation properties
        public ICollection<CVSkill> CVSkills { get; set; } = new List<CVSkill>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}

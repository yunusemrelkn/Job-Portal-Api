using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class CVSkill
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CV")]
        public int CvId { get; set; }

        [ForeignKey("Skill")]
        public int SkillId { get; set; }

        // Navigation properties
        public CV CV { get; set; }
        public Skill Skill { get; set; }
    }
}

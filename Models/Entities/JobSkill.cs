using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class JobSkill
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        [ForeignKey("Skill")]
        public int SkillId { get; set; }

        // Navigation properties
        public Job Job { get; set; }
        public Skill Skill { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace JobPortal.Api.Models.Entities
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [ForeignKey("User")]
        public int CreatedBy { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SalaryMax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Creator { get; set; }
        public Company Company { get; set; }
        public Department Department { get; set; }
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}

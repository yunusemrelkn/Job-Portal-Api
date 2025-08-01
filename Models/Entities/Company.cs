using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [ForeignKey("Sector")]
        public int SectorId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        // Navigation properties
        public Sector Sector { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<CompanyWorker> CompanyWorkers { get; set; } = new List<CompanyWorker>();
    }
}

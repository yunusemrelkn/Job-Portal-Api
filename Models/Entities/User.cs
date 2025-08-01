using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace JobPortal.Api.Models.Entities
{
    public enum UserRole
    {
        Admin,
        Employer,
        JobSeeker
    }
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [ForeignKey("Company")]
        public int? CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Company? Company { get; set; }
        public ICollection<CV> CVs { get; set; } = new List<CV>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Job> CreatedJobs { get; set; } = new List<Job>();
        public ICollection<CompanyWorker> CompanyWorkers { get; set; } = new List<CompanyWorker>();
    }
}

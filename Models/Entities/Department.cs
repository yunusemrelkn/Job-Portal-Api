using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.Entities
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation properties
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<CompanyWorker> CompanyWorkers { get; set; } = new List<CompanyWorker>();
    }
}

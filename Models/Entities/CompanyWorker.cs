using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class CompanyWorker
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        // Navigation properties
        public Company Company { get; set; }
        public User User { get; set; }
        public Department Department { get; set; }
    }
}

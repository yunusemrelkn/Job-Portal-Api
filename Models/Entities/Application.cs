using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public enum ApplicationStatus
    {
        Pending,
        Accepted,
        Rejected
    }
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public Job Job { get; set; }
    }
}

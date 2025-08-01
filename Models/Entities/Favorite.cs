using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Api.Models.Entities
{
    public class Favorite
    {
        [Key]
        public int FavId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Job")]
        public int JobId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Job Job { get; set; }
    }
}

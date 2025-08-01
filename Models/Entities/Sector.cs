using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.Entities
{
    public class Sector
    {
        [Key]
        public int SectorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation properties
        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}

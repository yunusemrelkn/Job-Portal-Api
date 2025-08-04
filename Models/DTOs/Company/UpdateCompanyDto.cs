using System.ComponentModel.DataAnnotations;

namespace JobPortal.Api.Models.DTOs.Company
{
    public class UpdateCompanyDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public int SectorId { get; set; }
    }
}

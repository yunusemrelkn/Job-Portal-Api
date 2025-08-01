using JobPortal.Api.Models.DTOs.CV;
using JobPortal.Api.Models.Entities;

namespace JobPortal.Api.Models.DTOs.Application
{
    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantEmail { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public CVDto CV { get; set; }
    }
}

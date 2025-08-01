using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Application;
using JobPortal.Api.Models.DTOs.CV;
using JobPortal.Api.Models.DTOs.Job;
using JobPortal.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplications()
        {
            var currentUserId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            IQueryable<Application> query = _context.Applications
                .Include(a => a.User)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company);

            if (userRole == "JobSeeker")
            {
                query = query.Where(a => a.UserId == currentUserId);
            }
            else if (userRole == "Employer")
            {
                var user = await _context.Users.FindAsync(currentUserId);
                query = query.Where(a => a.Job.CompanyId == user.CompanyId);
            }

            var applications = await query.Select(a => new ApplicationDto
            {
                ApplicationId = a.ApplicationId,
                UserId = a.UserId,
                ApplicantName = a.User.Name + " " + a.User.Surname,
                ApplicantEmail = a.User.Email,
                JobId = a.JobId,
                JobTitle = a.Job.Title,
                CompanyName = a.Job.Company.Name,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

            return Ok(applications);
        }

        [HttpPost("{jobId}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<ActionResult<ApplicationDto>> ApplyToJob(int jobId)
        {
            var currentUserId = GetCurrentUserId();

            // Check if user has already applied
            if (await _context.Applications.AnyAsync(a => a.UserId == currentUserId && a.JobId == jobId))
                return BadRequest("Already applied to this job");

            // Check if job exists
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                return NotFound("Job not found");

            var application = new Application
            {
                UserId = currentUserId.Value,
                JobId = jobId,
                Status = ApplicationStatus.Pending
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new ApplicationDto
            {
                ApplicationId = application.ApplicationId,
                UserId = application.UserId,
                JobId = application.JobId,
                Status = application.Status,
                CreatedAt = application.CreatedAt
            });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<ApplicationDto>> UpdateApplicationStatus(int id, [FromBody] ApplicationStatus status)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationId == id && a.Job.CompanyId == user.CompanyId);

            if (application == null)
                return NotFound();

            application.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new ApplicationDto
            {
                ApplicationId = application.ApplicationId,
                UserId = application.UserId,
                ApplicantName = application.User.Name + " " + application.User.Surname,
                ApplicantEmail = application.User.Email,
                JobId = application.JobId,
                JobTitle = application.Job.Title,
                Status = application.Status,
                CreatedAt = application.CreatedAt
            });
        }

        [HttpGet("{id}/cv")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<CVDto>> GetApplicantCV(int id)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.User)
                    .ThenInclude(u => u.CVs)
                        .ThenInclude(cv => cv.CVSkills)
                            .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(a => a.ApplicationId == id && a.Job.CompanyId == user.CompanyId);

            if (application == null)
                return NotFound();

            var latestCV = application.User.CVs.OrderByDescending(cv => cv.CreatedAt).FirstOrDefault();
            if (latestCV == null)
                return NotFound("No CV found for this applicant");

            var cvDto = new CVDto
            {
                CvId = latestCV.CvId,
                UserId = latestCV.UserId,
                Summary = latestCV.Summary,
                ExperienceYears = latestCV.ExperienceYears,
                EducationLevel = latestCV.EducationLevel,
                SkillsText = latestCV.SkillsText,
                CreatedAt = latestCV.CreatedAt,
                Skills = latestCV.CVSkills.Select(cs => cs.Skill.Name).ToList()
            };

            return Ok(cvDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> RemoveApplication(int id)
        {
            var currentUserId = GetCurrentUserId();
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == id && a.UserId == currentUserId);

            if (application == null)
                return NotFound();

            // Only allow removal of pending applications
            if (application.Status != ApplicationStatus.Pending)
                return BadRequest("Can only remove pending applications");

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }

    
}


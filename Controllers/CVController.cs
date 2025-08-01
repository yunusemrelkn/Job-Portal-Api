using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.CV;
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
    public class CVController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CVController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "JobSeeker")]
        public async Task<ActionResult<IEnumerable<CVDto>>> GetMyCVs()
        {
            var currentUserId = GetCurrentUserId();

            var cvs = await _context.CVs
                .Include(cv => cv.CVSkills)
                    .ThenInclude(cs => cs.Skill)
                .Where(cv => cv.UserId == currentUserId)
                .Select(cv => new CVDto
                {
                    CvId = cv.CvId,
                    UserId = cv.UserId,
                    Summary = cv.Summary,
                    ExperienceYears = cv.ExperienceYears,
                    EducationLevel = cv.EducationLevel,
                    SkillsText = cv.SkillsText,
                    CreatedAt = cv.CreatedAt,
                    Skills = cv.CVSkills.Select(cs => cs.Skill.Name).ToList()
                })
                .ToListAsync();

            return Ok(cvs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CVDto>> GetCV(int id)
        {
            var currentUserId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var cv = await _context.CVs
                .Include(cv => cv.CVSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(cv => cv.CvId == id);

            if (cv == null)
                return NotFound();

            // Only allow access to own CV (JobSeeker) or any CV (Employer/Admin)
            if (userRole == "JobSeeker" && cv.UserId != currentUserId)
                return Forbid();

            var cvDto = new CVDto
            {
                CvId = cv.CvId,
                UserId = cv.UserId,
                Summary = cv.Summary,
                ExperienceYears = cv.ExperienceYears,
                EducationLevel = cv.EducationLevel,
                SkillsText = cv.SkillsText,
                CreatedAt = cv.CreatedAt,
                Skills = cv.CVSkills.Select(cs => cs.Skill.Name).ToList()
            };

            return Ok(cvDto);
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<ActionResult<CVDto>> CreateCV(CreateCVDto dto)
        {
            var currentUserId = GetCurrentUserId();

            var cv = new CV
            {
                UserId = currentUserId.Value,
                Summary = dto.Summary,
                ExperienceYears = dto.ExperienceYears,
                EducationLevel = dto.EducationLevel,
                SkillsText = dto.SkillsText
            };

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            // Add CV skills
            foreach (var skillId in dto.SkillIds)
            {
                _context.CVSkills.Add(new CVSkill { CvId = cv.CvId, SkillId = skillId });
            }
            await _context.SaveChangesAsync();

            return await GetCV(cv.CvId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<ActionResult<CVDto>> UpdateCV(int id, UpdateCVDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var cv = await _context.CVs
                .Include(cv => cv.CVSkills)
                .FirstOrDefaultAsync(cv => cv.CvId == id && cv.UserId == currentUserId);

            if (cv == null)
                return NotFound();

            cv.Summary = dto.Summary;
            cv.ExperienceYears = dto.ExperienceYears;
            cv.EducationLevel = dto.EducationLevel;
            cv.SkillsText = dto.SkillsText;

            // Update CV skills
            _context.CVSkills.RemoveRange(cv.CVSkills);
            foreach (var skillId in dto.SkillIds)
            {
                _context.CVSkills.Add(new CVSkill { CvId = cv.CvId, SkillId = skillId });
            }

            await _context.SaveChangesAsync();
            return await GetCV(cv.CvId);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DeleteCV(int id)
        {
            var currentUserId = GetCurrentUserId();
            var cv = await _context.CVs.FirstOrDefaultAsync(cv => cv.CvId == id && cv.UserId == currentUserId);

            if (cv == null)
                return NotFound();

            _context.CVs.Remove(cv);
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

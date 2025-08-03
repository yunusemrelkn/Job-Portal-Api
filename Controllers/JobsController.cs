using JobPortal.Api.Data;
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
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs([FromQuery] string? search, [FromQuery] int? sectorId)
        {
            var currentUserId = GetCurrentUserId();

            var query = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Department)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => !j.IsFilled) // Only show unfilled jobs
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(j => j.Title.Contains(search) || j.Description.Contains(search));
            }

            if (sectorId.HasValue)
            {
                query = query.Where(j => j.Company.SectorId == sectorId.Value);
            }

            var jobs = await query.Select(j => new JobDto
            {
                JobId = j.JobId,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                CompanyName = j.Company.Name,
                DepartmentName = j.Department.Name,
                CreatedAt = j.CreatedAt,
                Skills = j.JobSkills.Select(js => js.Skill.Name).ToList(),
                IsFavorited = currentUserId.HasValue && _context.Favorites.Any(f => f.UserId == currentUserId && f.JobId == j.JobId),
                HasApplied = currentUserId.HasValue && _context.Applications.Any(a => a.UserId == currentUserId && a.JobId == j.JobId)
            }).ToListAsync();

            return Ok(jobs);
        }

        // Add employer-specific endpoint to see ALL jobs (including filled ones)
        [HttpGet("employer")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetEmployerJobs([FromQuery] bool includeFilledJobs = true)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user?.CompanyId == null)
                return BadRequest("User must be associated with a company");

            var query = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Department)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => j.CompanyId == user.CompanyId);

            // Optionally filter out filled jobs for employer view
            if (!includeFilledJobs)
            {
                query = query.Where(j => !j.IsFilled);
            }

            var jobs = await query.Select(j => new JobDto
            {
                JobId = j.JobId,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                CompanyName = j.Company.Name,
                DepartmentName = j.Department.Name,
                CreatedAt = j.CreatedAt,
                Skills = j.JobSkills.Select(js => js.Skill.Name).ToList(),
                IsFavorited = false, // Not applicable for employer view
                HasApplied = false,  // Not applicable for employer view
                IsFilled = j.IsFilled // Add this to JobDto
            }).ToListAsync();

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            var currentUserId = GetCurrentUserId();

            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Department)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
                return NotFound();

            var jobDto = new JobDto
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                CompanyName = job.Company.Name,
                DepartmentName = job.Department.Name,
                CreatedAt = job.CreatedAt,
                Skills = job.JobSkills.Select(js => js.Skill.Name).ToList(),
                IsFavorited = currentUserId.HasValue && await _context.Favorites.AnyAsync(f => f.UserId == currentUserId && f.JobId == id),
                HasApplied = currentUserId.HasValue && await _context.Applications.AnyAsync(a => a.UserId == currentUserId && a.JobId == id)
            };

            return Ok(jobDto);
        }
        

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<JobDto>> CreateJob(CreateJobDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user?.CompanyId == null)
                return BadRequest("User must be associated with a company");

            // Validate that all skillIds exist and are unique
            var uniqueSkillIds = dto.SkillIds.Distinct().ToList();
            var existingSkills = await _context.Skills
                .Where(s => uniqueSkillIds.Contains(s.SkillId))
                .Select(s => s.SkillId)
                .ToListAsync();

            if (existingSkills.Count != uniqueSkillIds.Count)
            {
                return BadRequest("One or more selected skills do not exist");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var job = new Job
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Location = dto.Location,
                    SalaryMin = dto.SalaryMin,
                    SalaryMax = dto.SalaryMax,
                    CreatedBy = currentUserId.Value,
                    CompanyId = user.CompanyId.Value,
                    DepartmentId = dto.DepartmentId,
                    IsFilled = false // Add this if you're implementing the hiring workflow
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Add job skills - only add unique skills
                foreach (var skillId in uniqueSkillIds)
                {
                    _context.JobSkills.Add(new JobSkill
                    {
                        JobId = job.JobId,
                        SkillId = skillId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetJob(job.JobId);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<JobDto>> UpdateJob(int id, UpdateJobDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.JobId == id && j.CreatedBy == currentUserId);

            if (job == null)
                return NotFound();

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Location = dto.Location;
            job.SalaryMin = dto.SalaryMin;
            job.SalaryMax = dto.SalaryMax;
            job.DepartmentId = dto.DepartmentId;

            // Update job skills
            _context.JobSkills.RemoveRange(job.JobSkills);
            foreach (var skillId in dto.SkillIds)
            {
                _context.JobSkills.Add(new JobSkill { JobId = job.JobId, SkillId = skillId });
            }

            await _context.SaveChangesAsync();
            return await GetJob(job.JobId);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var currentUserId = GetCurrentUserId();
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == id && j.CreatedBy == currentUserId);

            if (job == null)
                return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        [HttpGet("employer")]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetEmployerJobs()
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user?.CompanyId == null)
                return BadRequest("User must be associated with a company");

            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Department)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Where(j => j.CompanyId == user.CompanyId)
                .Select(j => new JobDto
                {
                    JobId = j.JobId,
                    Title = j.Title,
                    Description = j.Description,
                    Location = j.Location,
                    SalaryMin = j.SalaryMin,
                    SalaryMax = j.SalaryMax,
                    CompanyName = j.Company.Name,
                    DepartmentName = j.Department.Name,
                    CreatedAt = j.CreatedAt,
                    Skills = j.JobSkills.Select(js => js.Skill.Name).ToList(),
                    IsFavorited = false, // Not applicable for employer view
                    HasApplied = false   // Not applicable for employer view
                })
                .ToListAsync();

            return Ok(jobs);
        }
    }
}

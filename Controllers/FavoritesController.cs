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
    [Authorize(Roles = "JobSeeker")]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetFavorites()
        {
            var currentUserId = GetCurrentUserId();

            var favorites = await _context.Favorites
                .Include(f => f.Job)
                    .ThenInclude(j => j.Company)
                .Include(f => f.Job)
                    .ThenInclude(j => j.Department)
                .Include(f => f.Job)
                    .ThenInclude(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                .Where(f => f.UserId == currentUserId)
                .Select(f => new JobDto
                {
                    JobId = f.Job.JobId,
                    Title = f.Job.Title,
                    Description = f.Job.Description,
                    Location = f.Job.Location,
                    SalaryMin = f.Job.SalaryMin,
                    SalaryMax = f.Job.SalaryMax,
                    CompanyName = f.Job.Company.Name,
                    DepartmentName = f.Job.Department.Name,
                    CreatedAt = f.Job.CreatedAt,
                    Skills = f.Job.JobSkills.Select(js => js.Skill.Name).ToList(),
                    IsFavorited = true,
                    HasApplied = _context.Applications.Any(a => a.UserId == currentUserId && a.JobId == f.JobId)
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("{jobId}")]
        public async Task<IActionResult> AddToFavorites(int jobId)
        {
            var currentUserId = GetCurrentUserId();

            if (await _context.Favorites.AnyAsync(f => f.UserId == currentUserId && f.JobId == jobId))
                return BadRequest("Job already in favorites");

            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
                return NotFound("Job not found");

            var favorite = new Favorite
            {
                UserId = currentUserId.Value,
                JobId = jobId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{jobId}")]
        public async Task<IActionResult> RemoveFromFavorites(int jobId)
        {
            var currentUserId = GetCurrentUserId();
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == currentUserId && f.JobId == jobId);

            if (favorite == null)
                return NotFound();

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}

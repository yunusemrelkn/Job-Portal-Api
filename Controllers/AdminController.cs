using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Company;
using JobPortal.Api.Models.DTOs.User;
using JobPortal.Api.Models.Entities;
using JobPortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        public AdminController(ApplicationDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers([FromQuery] string? role)
        {
            var query = _context.Users
                .Include(u => u.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }

            var users = await query.Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : null,
                CreatedAt = u.CreatedAt
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("companies")]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAllCompanies()
        {
            var companies = await _context.Companies
                .Include(c => c.Sector)
                .Include(c => c.Users)
                .Include(c => c.Jobs)
                .Select(c => new CompanyDto
                {
                    CompanyId = c.CompanyId,
                    Name = c.Name,
                    Description = c.Description,
                    Location = c.Location,
                    SectorName = c.Sector.Name,
                    EmployeeCount = c.Users.Count,
                    JobCount = c.Jobs.Count
                })
                .ToListAsync();

            return Ok(companies);
        }

        [HttpPost("companies")]
        public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = dto.Location,
                SectorId = dto.SectorId
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var sector = await _context.Sectors.FindAsync(dto.SectorId);

            return Ok(new CompanyDto
            {
                CompanyId = company.CompanyId,
                Name = company.Name,
                Description = company.Description,
                Location = company.Location,
                SectorName = sector?.Name,
                EmployeeCount = 0,
                JobCount = 0
            });
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                JobSeekers = await _context.Users.CountAsync(u => u.Role == UserRole.JobSeeker),
                Employers = await _context.Users.CountAsync(u => u.Role == UserRole.Employer),
                Admins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalApplications = await _context.Applications.CountAsync(),
                PendingApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Pending),
                AcceptedApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Accepted),
                RejectedApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Rejected)
            };

            return Ok(stats);
        }
    }
}

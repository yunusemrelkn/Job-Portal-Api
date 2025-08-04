// Controllers/AdminController.cs - Enhanced Admin Controller
using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.User;
using JobPortal.Api.Models.DTOs.Company;
using JobPortal.Api.Models.DTOs.Job;
using JobPortal.Api.Models.DTOs.Application;
using JobPortal.Api.Models.DTOs.Others;
using JobPortal.Api.Models.Entities;
using JobPortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // === DASHBOARD & STATISTICS ===

        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardData()
        {
            var stats = new
            {
                // User Statistics
                TotalUsers = await _context.Users.CountAsync(),
                JobSeekers = await _context.Users.CountAsync(u => u.Role == UserRole.JobSeeker),
                Employers = await _context.Users.CountAsync(u => u.Role == UserRole.Employer),
                Admins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin),

                // Company Statistics
                TotalCompanies = await _context.Companies.CountAsync(),
                CompaniesBySector = await _context.Companies
                    .Include(c => c.Sector)
                    .GroupBy(c => c.Sector.Name)
                    .Select(g => new { Sector = g.Key, Count = g.Count() })
                    .ToListAsync(),

                // Job Statistics
                TotalJobs = await _context.Jobs.CountAsync(),
                ActiveJobs = await _context.Jobs.CountAsync(j => !j.IsFilled),
                FilledJobs = await _context.Jobs.CountAsync(j => j.IsFilled),
                JobsByDepartment = await _context.Jobs
                    .Include(j => j.Department)
                    .GroupBy(j => j.Department.Name)
                    .Select(g => new { Department = g.Key, Count = g.Count() })
                    .ToListAsync(),

                // Application Statistics
                TotalApplications = await _context.Applications.CountAsync(),
                PendingApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Pending),
                AcceptedApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Accepted),
                RejectedApplications = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Rejected),

                // Recent Activity
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new { u.Name, u.Surname, u.Email, u.Role, u.CreatedAt })
                    .ToListAsync(),

                RecentJobs = await _context.Jobs
                    .Include(j => j.Company)
                    .Include(j => j.Department)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .Select(j => new { j.Title, Company = j.Company.Name, Department = j.Department.Name, j.CreatedAt, j.IsFilled })
                    .ToListAsync(),

                RecentApplications = await _context.Applications
                    .Include(a => a.User)
                    .Include(a => a.Job)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .Select(a => new {
                        Applicant = a.User.Name + " " + a.User.Surname,
                        Job = a.Job.Title,
                        a.Status,
                        a.CreatedAt
                    })
                    .ToListAsync(),

                // Growth Statistics (last 30 days)
                UserGrowth = await GetGrowthStatistics("Users"),
                JobGrowth = await GetGrowthStatistics("Jobs"),
                ApplicationGrowth = await GetGrowthStatistics("Applications")
            };

            return Ok(stats);
        }

        private async Task<object> GetGrowthStatistics(string entity)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            int totalCount, last30Days, last7Days;

            switch (entity)
            {
                case "Users":
                    totalCount = await _context.Users.CountAsync();
                    last30Days = await _context.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);
                    last7Days = await _context.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo);
                    break;
                case "Jobs":
                    totalCount = await _context.Jobs.CountAsync();
                    last30Days = await _context.Jobs.CountAsync(j => j.CreatedAt >= thirtyDaysAgo);
                    last7Days = await _context.Jobs.CountAsync(j => j.CreatedAt >= sevenDaysAgo);
                    break;
                case "Applications":
                    totalCount = await _context.Applications.CountAsync();
                    last30Days = await _context.Applications.CountAsync(a => a.CreatedAt >= thirtyDaysAgo);
                    last7Days = await _context.Applications.CountAsync(a => a.CreatedAt >= sevenDaysAgo);
                    break;
                default:
                    return new { Total = 0, Last30Days = 0, Last7Days = 0 };
            }

            return new { Total = totalCount, Last30Days = last30Days, Last7Days = last7Days };
        }

        // === USER MANAGEMENT ===

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers([FromQuery] string? search, [FromQuery] UserRole? role)
        {
            var query = _context.Users.Include(u => u.Company).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) ||
                                       u.Surname.Contains(search) ||
                                       u.Email.Contains(search));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
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

        [HttpPost("users")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User with this email already exists");

            var user = new User
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                CompanyId = dto.CompanyId,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
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
                CreatedAt = user.CreatedAt
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

            // Prevent deleting the last admin
            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                if (adminCount <= 1)
                    return BadRequest("Cannot delete the last admin user");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRole newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Prevent removing the last admin
            if (user.Role == UserRole.Admin && newRole != UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                if (adminCount <= 1)
                    return BadRequest("Cannot change role of the last admin user");
            }

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User role updated successfully" });
        }

        // === COMPANY MANAGEMENT ===

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

        [HttpPut("companies/{id}")]
        public async Task<ActionResult<CompanyDto>> UpdateCompany(int id, UpdateCompanyDto dto)
        {
            var company = await _context.Companies.Include(c => c.Sector).FirstOrDefaultAsync(c => c.CompanyId == id);
            if (company == null)
                return NotFound();

            company.Name = dto.Name;
            company.Description = dto.Description;
            company.Location = dto.Location;
            company.SectorId = dto.SectorId;

            await _context.SaveChangesAsync();

            return Ok(new CompanyDto
            {
                CompanyId = company.CompanyId,
                Name = company.Name,
                Description = company.Description,
                Location = company.Location,
                SectorName = company.Sector?.Name,
                EmployeeCount = await _context.Users.CountAsync(u => u.CompanyId == company.CompanyId),
                JobCount = await _context.Jobs.CountAsync(j => j.CompanyId == company.CompanyId)
            });
        }

        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            // Check if company has employees or jobs
            var hasEmployees = await _context.Users.AnyAsync(u => u.CompanyId == id);
            var hasJobs = await _context.Jobs.AnyAsync(j => j.CompanyId == id);

            if (hasEmployees || hasJobs)
                return BadRequest("Cannot delete company that has employees or job postings");

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // === JOB MANAGEMENT ===

        [HttpGet("jobs")]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetAllJobs([FromQuery] bool? isFilled, [FromQuery] int? companyId)
        {
            var query = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Department)
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .AsQueryable();

            if (isFilled.HasValue)
            {
                query = query.Where(j => j.IsFilled == isFilled.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(j => j.CompanyId == companyId.Value);
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
                IsFilled = j.IsFilled
            }).ToListAsync();

            return Ok(jobs);
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("jobs/{id}/toggle-status")]
        public async Task<IActionResult> ToggleJobStatus(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            job.IsFilled = !job.IsFilled;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Job {(job.IsFilled ? "marked as filled" : "reopened")}" });
        }

        // === APPLICATION MANAGEMENT ===

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetAllApplications([FromQuery] ApplicationStatus? status)
        {
            var query = _context.Applications
                .Include(a => a.User)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
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

        // === SYSTEM MANAGEMENT ===

        [HttpGet("sectors")]
        public async Task<ActionResult<IEnumerable<SectorDto>>> GetAllSectors()
        {
            var sectors = await _context.Sectors
                .Select(s => new SectorDto
                {
                    SectorId = s.SectorId,
                    Name = s.Name
                })
                .ToListAsync();

            return Ok(sectors);
        }

        [HttpPost("sectors")]
        public async Task<ActionResult<SectorDto>> CreateSector([FromBody] string sectorName)
        {
            if (await _context.Sectors.AnyAsync(s => s.Name.ToLower() == sectorName.ToLower()))
                return BadRequest("Sector already exists");

            var sector = new Sector { Name = sectorName };
            _context.Sectors.Add(sector);
            await _context.SaveChangesAsync();

            return Ok(new SectorDto
            {
                SectorId = sector.SectorId,
                Name = sector.Name
            });
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAllDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpPost("departments")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] string departmentName)
        {
            if (await _context.Departments.AnyAsync(d => d.Name.ToLower() == departmentName.ToLower()))
                return BadRequest("Department already exists");

            var department = new Department { Name = departmentName };
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(new DepartmentDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name
            });
        }

        [HttpGet("skills")]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
        {
            var skills = await _context.Skills
                .Select(s => new SkillDto
                {
                    SkillId = s.SkillId,
                    Name = s.Name
                })
                .ToListAsync();

            return Ok(skills);
        }

        [HttpPost("skills")]
        public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] string skillName)
        {
            if (await _context.Skills.AnyAsync(s => s.Name.ToLower() == skillName.ToLower()))
                return BadRequest("Skill already exists");

            var skill = new Skill { Name = skillName };
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return Ok(new SkillDto
            {
                SkillId = skill.SkillId,
                Name = skill.Name
            });
        }

        [HttpDelete("skills/{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("departments/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound();

            var hasJobs = await _context.Jobs.AnyAsync(j => j.DepartmentId == id);
            if (hasJobs)
                return BadRequest("Cannot delete department that has job postings");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("sectors/{id}")]
        public async Task<IActionResult> DeleteSector(int id)
        {
            var sector = await _context.Sectors.FindAsync(id);
            if (sector == null)
                return NotFound();

            var hasCompanies = await _context.Companies.AnyAsync(c => c.SectorId == id);
            if (hasCompanies)
                return BadRequest("Cannot delete sector that has companies");

            _context.Sectors.Remove(sector);
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
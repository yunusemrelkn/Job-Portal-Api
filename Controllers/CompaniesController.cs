using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
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

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Sector)
                .Include(c => c.Users)
                .Include(c => c.Jobs)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            if (company == null)
                return NotFound();

            return Ok(new CompanyDto
            {
                CompanyId = company.CompanyId,
                Name = company.Name,
                Description = company.Description,
                Location = company.Location,
                SectorName = company.Sector.Name,
                EmployeeCount = company.Users.Count,
                JobCount = company.Jobs.Count
            });
        }
    }
}

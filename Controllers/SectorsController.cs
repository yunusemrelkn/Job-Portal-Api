using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Others;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SectorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SectorDto>>> GetSectors()
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
    }
}

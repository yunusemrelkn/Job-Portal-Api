using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Others;
using JobPortal.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly Data.ApplicationDbContext _context;

        public SkillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
    }
}


using JobPortal.Api.Data;
using JobPortal.Api.Models.DTOs.Others;
using JobPortal.Api.Models.DTOs.User;
using JobPortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;

        public UsersController(ApplicationDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Company)
                .Include(u => u.CompanyWorkers)
                    .ThenInclude(cw => cw.Company)
                .Include(u => u.CompanyWorkers)
                    .ThenInclude(cw => cw.Department)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (user == null)
                return NotFound();

            // Get employment information
            var employmentInfo = user.CompanyWorkers.FirstOrDefault();

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
                CreatedAt = user.CreatedAt,

                // Enhanced employment information
                CurrentEmployment = employmentInfo != null ? new EmploymentDto
                {
                    CompanyName = employmentInfo.Company.Name,
                    DepartmentName = employmentInfo.Department.Name,
                    CompanyLocation = employmentInfo.Company.Location,
                    StartDate = user.CreatedAt // You might want to add a separate employment start date
                } : null
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user == null)
                return NotFound();

            if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect");

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}

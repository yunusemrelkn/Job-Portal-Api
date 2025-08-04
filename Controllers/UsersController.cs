﻿using JobPortal.Api.Data;
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

        [HttpPut("profile")]
        public async Task<ActionResult<UserDto>> UpdateProfile(UpdateUserDto dto)
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

            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();

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

                // Use UserEmploymentInfo
                CurrentEmployment = employmentInfo != null ? new EmploymentDto
                {
                    CompanyName = employmentInfo.Company.Name,
                    DepartmentName = employmentInfo.Department.Name,
                    CompanyLocation = employmentInfo.Company.Location,
                    StartDate = user.CreatedAt
                } : null
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] Models.DTOs.Others.ChangePasswordDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user == null)
                return NotFound("User not found");

            // Verify current password
            if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect");

            // Additional validation (optional but recommended)
            if (dto.NewPassword == dto.CurrentPassword)
                return BadRequest("New password must be different from current password");

            // Hash the new password
            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);

            // Update timestamp (optional)
            // You might want to add a PasswordChangedAt field to track when password was last changed

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        // Optional: Add password history endpoint
        [HttpGet("password-info")]
        public async Task<ActionResult<object>> GetPasswordInfo()
        {
            var currentUserId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(currentUserId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                // You can add a PasswordChangedAt field to User model to track this
                LastChanged = "Not available", // Replace with actual field when implemented
                RequiresChange = false, // You can implement password expiry logic here
                AccountCreated = user.CreatedAt
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}

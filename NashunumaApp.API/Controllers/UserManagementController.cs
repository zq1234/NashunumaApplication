// Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Application.DTOs.User;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? tehsil = null,
            [FromQuery] string? userType = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _userManagementService.GetPagedUsersAsync(
                pageNumber, pageSize, searchTerm, province, district, tehsil, userType, isActive);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("users/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var result = await _userManagementService.GetUserByUsernameAsync(username);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("required"))
                {
                    return BadRequest(result);
                }
            }

            return Ok(result);
        }

        [HttpPut("users/{username}/location")]
        public async Task<IActionResult> TransferUserLocation(string username,[FromBody] UpdateUserLocationDto locationDto)
        {
            var modifiedBy = User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name) ?? "System";

            var result = await _userManagementService.TransferUserLocationAsync(username, locationDto, modifiedBy);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("Cannot transfer inactive"))
                {
                    return BadRequest(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPatch("users/{username}/status")]
        public async Task<IActionResult> ToggleUserStatus(string username, [FromBody] bool isActive)
        {
            
            if (string.IsNullOrWhiteSpace(username) || username.Trim().ToLower() == "undefined")
            {
                return BadRequest(new { isSuccess = false, message = "Invalid username" });
            }
            var modifiedBy = User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name) ?? "System";

            var result = await _userManagementService.ToggleUserStatusAsync(username, isActive, modifiedBy);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("cannot change your own status"))
                {
                    return BadRequest(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("users/{username}/block")]
        public async Task<IActionResult> BlockUser(string username)
        {
            var modifiedBy = User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name) ?? "System";

            var result = await _userManagementService.BlockUserAsync(username, modifiedBy);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                if (result.Message.Contains("cannot block yourself"))
                {
                    return BadRequest(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPatch("users/{username}/unblock")]
        public async Task<IActionResult> UnblockUser(string username)
        {
            var modifiedBy = User.FindFirstValue("username")
                             ?? User.FindFirstValue(ClaimTypes.Name)
                             ?? "System";

            var result = await _userManagementService.UnblockUserAsync(username, modifiedBy);

            if (!result.IsSuccess)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("users/by-location")]
        public async Task<IActionResult> GetUsersByLocation(
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? tehsil = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userManagementService.GetUsersByLocationAsync(
                province, district, tehsil, pageNumber, pageSize);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("statistics/location")]
        public async Task<IActionResult> GetLocationStatistics()
        {
            var result = await _userManagementService.GetLocationStatisticsAsync();
            return StatusCode(result.StatusCode, result);
        }
    }
}
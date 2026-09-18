using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using NashunumaApp.Application.Interfaces;
using System.Security.Claims;

namespace NashunumaApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FoodStockController : ControllerBase
    {
        private readonly IFoodStockService _foodStockService;
        private readonly ILogger<FoodStockController> _logger;

        public FoodStockController(
            IFoodStockService foodStockService,
            ILogger<FoodStockController> logger)
        {
            _foodStockService = foodStockService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFoodStocks([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10,[FromQuery] string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting paged food stocks - Page: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm ?? "null");

                var siteId = GetUserSiteId();
                _logger.LogDebug("Using SiteId from claims: {SiteId}", siteId ?? "null");

                var result = await _foodStockService.GetPagedFoodStocksAsync(pageNumber, pageSize, searchTerm, siteId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved {Count} food stocks", result.Data?.Items?.Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve food stocks: {Message}", result.Message);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged food stocks");
                return StatusCode(500, ApiResponse<PaginatedResponse<FoodStockDto>>.Failure("An unexpected error occurred"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodStockById(decimal id)
        {
            try
            {
                _logger.LogInformation("Getting food stock by ID: {Id}", id);

                var result = await _foodStockService.GetFoodStockByIdAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("not found"))
                    {
                        _logger.LogWarning("Food stock with ID {Id} not found", id);
                        return NotFound(result);
                    }
                    if (result.Message.Contains("Invalid ID"))
                    {
                        _logger.LogWarning("Invalid ID provided: {Id}", id);
                        return BadRequest(result);
                    }
                    _logger.LogWarning("Failed to get food stock: {Message}", result.Message);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved food stock with ID: {Id}", id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting food stock by ID: {Id}", id);
                return StatusCode(500, ApiResponse<FoodStockDto>.Failure("An unexpected error occurred"));
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryStats()
        {
            try
            {
                _logger.LogInformation("Getting summary statistics");

                var result = await _foodStockService.GetSummaryStatsAsync();

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully retrieved summary statistics");
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve summary statistics: {Message}", result.Message);
                }

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting summary statistics");
                return StatusCode(500, ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Failure("An unexpected error occurred"));
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportFoodStocks(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string format = "excel")
        {
            try
            {
                _logger.LogInformation("Exporting food stocks - SearchTerm: {SearchTerm}, Format: {Format}",
                    searchTerm ?? "null", format);

                var result = await _foodStockService.ExportFoodStocksAsync(searchTerm, format);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully exported food stocks with {DataLength} bytes",
                        result.Data?.Length ?? 0);

                    return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"FoodStock_{DateTime.Now:yyyy-MM-dd}.xlsx");
                }
                else
                {
                    _logger.LogWarning("Failed to export food stocks: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting food stocks");
                return StatusCode(500, ApiResponse<byte[]>.Failure("An unexpected error occurred"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodStock(int id)
        {
            try
            {
                _logger.LogInformation("Deleting food stock with ID: {Id}", id);

                var result = await _foodStockService.DeleteFoodStockAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("not found"))
                    {
                        _logger.LogWarning("Food stock with ID {Id} not found for deletion", id);
                        return NotFound(result);
                    }
                    _logger.LogWarning("Failed to delete food stock: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Successfully deleted food stock with ID: {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting food stock with ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.Failure("An unexpected error occurred"));
            }
        }

        
        [HttpGet("missing-dates")]
        public async Task<IActionResult> GetMissingStockDates()
        {
            try
            {
                var siteId = GetUserSiteId();
                var username = GetUserUsername();

                _logger.LogInformation("Getting missing stock dates for SiteId: {SiteId}, User: {Username}",
                    siteId ?? "null", username ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("User {Username} not assigned to any site", username ?? "unknown");
                    return Unauthorized("User not assigned to any site");
                }

                var result = await _foodStockService.GetMissingStockDatesAsync(siteId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to get missing stock dates for SiteId {SiteId}: {Message}",
                        siteId, result.Message);
                    return BadRequest(result);
                }

                // Create notification
                var notification = new MissingStockNotificationDto
                {
                    SiteId = siteId,
                    MissingDates = result.Data,
                    TotalMissingDays = result.Data.Count,
                    HasMissingEntries = result.Data.Count > 0,
                    Message = result.Data.Count > 0
                        ? $"You have pending stock entries for {result.Data.Count} date(s). Please complete the missing stock records."
                        : "All stock entries are up to date."
                };

                _logger.LogInformation("Found {Count} missing stock dates for SiteId: {SiteId}",
                    result.Data.Count, siteId);

                return Ok(ApiResponse<MissingStockNotificationDto>.Success(notification, "Missing dates retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting missing stock dates");
                return StatusCode(500, ApiResponse<MissingStockNotificationDto>.Failure("An unexpected error occurred"));
            }
        }

        
        [HttpPost("save")]
        public async Task<IActionResult> SaveStockInformation([FromBody] SaveStockInformationRequest request)
        {
            try
            {
                // Get current user's site ID
                var userSiteId = GetUserSiteId();
                var username = GetUserUsername();

                _logger.LogInformation("Saving stock information - User: {Username}, SiteId: {SiteId}, Date: {Date}",
                    username ?? "null", request.SiteId ?? "null", request.EnteredOn ?? "null");

                // Set site ID from authenticated user if not provided in request
                if (string.IsNullOrEmpty(request.SiteId))
                {
                    request.SiteId = userSiteId;
                    _logger.LogDebug("Set SiteId from authenticated user: {SiteId}", request.SiteId);
                }

                // Verify user has permission for this site
                if (request.SiteId != userSiteId)
                {
                    _logger.LogWarning("User {Username} attempted to save stock for unauthorized site {SiteId}",
                        username ?? "unknown", request.SiteId);
                    return Forbid("You don't have permission to save stock for this site");
                }

                // Set entered by to current user
                if (string.IsNullOrEmpty(request.EnteredBy))
                {
                    request.EnteredBy = username;
                    _logger.LogDebug("Set EnteredBy from authenticated user: {Username}", username);
                }
                if (request.EnteredBy != username)
                {
                    _logger.LogWarning("User {Username} attempted to save stock for unauthorized user: {Username}",
                        username ?? "unknown", request.SiteId);
                    return Forbid("You don't have permission to save stock for this user");
                }
                var result = await _foodStockService.SaveStockInformationAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to save stock information: {Message}, User: {Username}",
                        result.Message, username ?? "unknown");
                    return BadRequest(result);
                }

                _logger.LogInformation("Successfully saved stock information - User: {Username}, SiteId: {SiteId}, Date: {Date}",
                    username ?? "unknown", request.SiteId, request.EnteredOn ?? "null");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving stock information");
                return StatusCode(500, ApiResponse<object>.Failure($"An unexpected error occurred: {ex.Message}"));
            }
        }

        
        [HttpGet("history")]
        public async Task<IActionResult> GetSiteStockHistory()
        {
            try
            {
                var siteId = GetUserSiteId();
                var username = GetUserUsername();

                _logger.LogInformation("Getting stock history for SiteId: {SiteId}, User: {Username}",
                    siteId ?? "null", username ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("User {Username} not assigned to any site", username ?? "unknown");
                    return Unauthorized("User not assigned to any site");
                }

                var result = await _foodStockService.GetFoodStocksBySiteIdAsync(siteId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to get stock history for SiteId {SiteId}: {Message}",
                        siteId, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Successfully retrieved {Count} stock records for SiteId: {SiteId}",
                    result.Data?.Count ?? 0, siteId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting site stock history");
                return StatusCode(500, ApiResponse<List<FoodStockDto>>.Failure("An unexpected error occurred"));
            }
        }

        
        [HttpGet("exists")]
        public async Task<IActionResult> CheckStockExists([FromQuery] string date)
        {
            try
            {
                var siteId = GetUserSiteId();
                var username = GetUserUsername();

                _logger.LogInformation("Checking stock existence - SiteId: {SiteId}, Date: {Date}, User: {Username}",
                    siteId ?? "null", date ?? "null", username ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("User {Username} not assigned to any site", username ?? "unknown");
                    return Unauthorized("User not assigned to any site");
                }

                if (string.IsNullOrEmpty(date))
                {
                    _logger.LogWarning("Date parameter is null or empty");
                    return BadRequest(ApiResponse<bool>.Failure("Date is required"));
                }

                var result = await _foodStockService.ValidateStockExistsForDateAsync(siteId, date);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to check stock existence: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Stock exists for SiteId: {SiteId}, Date: {Date}: {Exists}",
                    siteId, date, result.Data);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking stock existence");
                return StatusCode(500, ApiResponse<bool>.Failure("An unexpected error occurred"));
            }
        }

        #region Helper Methods

        private string GetUserSiteId()
        {
            var siteId = User.FindFirst("siteid")?.Value;
            _logger.LogDebug("Retrieved SiteId from claims: {SiteId}", siteId ?? "null");
            return siteId;
        }

        private string GetUserUsername()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("username")?.Value;
            _logger.LogDebug("Retrieved Username from claims: {Username}", username ?? "null");
            return username;
        }

        #endregion
    }
}
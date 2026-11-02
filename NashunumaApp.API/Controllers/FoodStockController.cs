// API/Controllers/FoodStockController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Common.DTOs;
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

        public FoodStockController(IFoodStockService foodStockService, ILogger<FoodStockController> logger)
        {
            _foodStockService = foodStockService;
            _logger = logger;
        }

        /// <summary>
        /// GET api/foodstock
        /// Paged list — admin sees all rows, non-admin sees own rows only.
        /// Supports all caller filters: search, siteId, siteName, province,
        /// district, tehsil, date, dateFrom, dateTo, enteredBy, headName.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFoodStocks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? siteId = null,
            [FromQuery] string? siteName = null,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? tehsil = null,
            [FromQuery] string? date = null,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] string? enteredBy = null,
            [FromQuery] string? headName = null)
        {
            try
            {
                var username = GetUserUsername();

                _logger.LogInformation(
                    "Getting paged food stocks - User: {Username}, Page: {PageNumber}/{PageSize}, Search: {SearchTerm}, SiteId: {SiteId}, SiteName: {SiteName}, Province: {Province}, District: {District}, Tehsil: {Tehsil}, Date: {Date}, DateFrom: {DateFrom}, DateTo: {DateTo}, EnteredBy: {EnteredBy}, HeadName: {HeadName}",
                    username ?? "null", pageNumber, pageSize, searchTerm ?? "null",
                    siteId ?? "null", siteName ?? "null", province ?? "null",
                    district ?? "null", tehsil ?? "null", date ?? "null",
                    dateFrom ?? "null", dateTo ?? "null", enteredBy ?? "null", headName ?? "null");

                var result = await _foodStockService.GetPagedFoodStocksAsync(
                    username!,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    siteId,
                    province,
                    district,
                    tehsil,
                    siteName,
                    date,
                    dateFrom,
                    dateTo,
                    enteredBy,
                    headName);

                if (result.IsSuccess)
                    _logger.LogInformation("Successfully retrieved {Count} food stocks", result.Data?.Items?.Count ?? 0);
                else
                    _logger.LogWarning("Failed to retrieve food stocks: {Message}", result.Message);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged food stocks");
                return StatusCode(500, ApiResponse<PaginatedResponse<FoodStockDto>>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>GET api/foodstock/{id} — single stock row by primary key.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodStockById(decimal id)
        {
            try
            {
                var result = await _foodStockService.GetFoodStockByIdAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("not found")) return NotFound(result);
                    if (result.Message.Contains("Invalid ID")) return BadRequest(result);
                    _logger.LogWarning("Failed to get food stock: {Message}", result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting food stock by ID: {Id}", id);
                return StatusCode(500, ApiResponse<FoodStockDto>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>GET api/foodstock/summary — role-scoped summary counters.</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryStats()
        {
            try
            {
                var username = GetUserUsername();
                var result = await _foodStockService.GetSummaryStatsAsync(username!);

                if (!result.IsSuccess)
                    _logger.LogWarning("Failed to retrieve summary statistics: {Message}", result.Message);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting summary statistics");
                return StatusCode(500, ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>
        /// GET api/foodstock/export
        /// Role-scoped export. All filters from the paged list are supported.
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportFoodStocks(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? siteId = null,
            [FromQuery] string? siteName = null,
            [FromQuery] string? province = null,
            [FromQuery] string? district = null,
            [FromQuery] string? tehsil = null,
            [FromQuery] string? date = null,
            [FromQuery] string? dateFrom = null,
            [FromQuery] string? dateTo = null,
            [FromQuery] string? enteredBy = null,
            [FromQuery] string? headName = null,
            [FromQuery] string format = "excel")
        {
            try
            {
                var username = GetUserUsername();

                _logger.LogInformation(
                    "Exporting food stocks - User: {Username}, Search: {SearchTerm}, SiteId: {SiteId}, SiteName: {SiteName}, Province: {Province}, District: {District}, Tehsil: {Tehsil}, Date: {Date}, DateFrom: {DateFrom}, DateTo: {DateTo}, EnteredBy: {EnteredBy}, HeadName: {HeadName}, Format: {Format}",
                    username ?? "null", searchTerm ?? "null", siteId ?? "null", siteName ?? "null",
                    province ?? "null", district ?? "null", tehsil ?? "null",
                    date ?? "null", dateFrom ?? "null", dateTo ?? "null",
                    enteredBy ?? "null", headName ?? "null", format);

                var result = await _foodStockService.ExportFoodStocksAsync(
                    username!,
                    searchTerm,
                    siteId,
                    siteName,
                    province,
                    district,
                    tehsil,
                    date,
                    dateFrom,
                    dateTo,
                    enteredBy,
                    headName,
                    format);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to export food stocks: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Successfully exported food stocks with {DataLength} bytes", result.Data?.Length ?? 0);
                return File(
                    result.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"FoodStock_{DateTime.Now:yyyy-MM-dd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting food stocks");
                return StatusCode(500, ApiResponse<byte[]>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>DELETE api/foodstock/{id} — deletes a stock row.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodStock(int id)
        {
            try
            {
                var result = await _foodStockService.DeleteFoodStockAsync(id);

                if (!result.IsSuccess)
                {
                    if (result.Message.Contains("not found")) return NotFound(result);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting food stock with ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>GET api/foodstock/missing-dates — pending stock dates for the caller's site.</summary>
        [HttpGet("missing-dates")]
        public async Task<IActionResult> GetMissingStockDates()
        {
            try
            {
                var siteId = GetUserSiteId();
                var username = GetUserUsername();

                if (string.IsNullOrEmpty(siteId))
                    return Unauthorized("User not assigned to any site");

                var result = await _foodStockService.GetMissingStockDatesAsync(siteId);
                if (!result.IsSuccess)
                    return BadRequest(result);

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

                return Ok(ApiResponse<MissingStockNotificationDto>.Success(notification, "Missing dates retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting missing stock dates");
                return StatusCode(500, ApiResponse<MissingStockNotificationDto>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>POST api/foodstock/save — create or manual-update a stock row.</summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveStockInformation([FromBody] SaveStockInformationRequest request)
        {
            try
            {
                var userSiteId = GetUserSiteId();
                var username = GetUserUsername();

                if (string.IsNullOrEmpty(request.SiteId))
                    request.SiteId = userSiteId;

                if (request.SiteId != userSiteId)
                    return Forbid("You don't have permission to save stock for this site");

                if (string.IsNullOrEmpty(request.EnteredBy))
                    request.EnteredBy = username;

                if (request.EnteredBy != username)
                    return Forbid("You don't have permission to save stock for this user");

                var result = await _foodStockService.SaveStockInformationAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to save stock information: {Message}", result.Message);
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving stock information");
                return StatusCode(500, ApiResponse<object>.Failure($"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>GET api/foodstock/history — all stock rows for the caller's site.</summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetSiteStockHistory()
        {
            try
            {
                var siteId = GetUserSiteId();
                if (string.IsNullOrEmpty(siteId))
                    return Unauthorized("User not assigned to any site");

                var result = await _foodStockService.GetFoodStocksBySiteIdAsync(siteId);
                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting site stock history");
                return StatusCode(500, ApiResponse<List<FoodStockDto>>.Failure("An unexpected error occurred"));
            }
        }

        /// <summary>GET api/foodstock/exists?date=dd-MM-yyyy — whether a row exists for the date.</summary>
        [HttpGet("exists")]
        public async Task<IActionResult> CheckStockExists([FromQuery] string date)
        {
            try
            {
                var siteId = GetUserSiteId();
                if (string.IsNullOrEmpty(siteId))
                    return Unauthorized("User not assigned to any site");

                if (string.IsNullOrEmpty(date))
                    return BadRequest(ApiResponse<bool>.Failure("Date is required"));

                var result = await _foodStockService.ValidateStockExistsForDateAsync(siteId, date);
                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking stock existence");
                return StatusCode(500, ApiResponse<bool>.Failure("An unexpected error occurred"));
            }
        }
        [HttpGet("site/{siteId}")]
        public async Task<IActionResult> GetBySiteId(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                return BadRequest(ApiResponse<List<FoodStockDto>>.Failure("Site ID is required"));

            var result = await _foodStockService.GetFoodStocksBySiteIdAsync(siteId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        #region Helper Methods

        private string GetUserSiteId()
            => User.FindFirst("siteid")?.Value ?? string.Empty;

        private string GetUserUsername()
            => User.FindFirst(ClaimTypes.Name)?.Value
               ?? User.FindFirst("username")?.Value
               ?? string.Empty;

        #endregion
    }
}
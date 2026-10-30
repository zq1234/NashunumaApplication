// Application/Services/FoodStockService.cs

using AutoMapper;
using Microsoft.Extensions.Logging;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using System.Globalization;

namespace NashunumaApp.Application.Services
{
    /// <summary>
    /// Application service for food stock.
    /// Role-scoped methods accept a username (resolved by the controller from
    /// authenticated claims) and forward it to the repository, which applies
    /// admin vs. non-admin site scoping.
    /// </summary>
    public class FoodStockService : IFoodStockService
    {
        private readonly IFoodStockRepository _foodStockRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FoodStockService> _logger;

        public FoodStockService(
            IFoodStockRepository foodStockRepository,
            IMapper mapper,
            ILogger<FoodStockService> logger)
        {
            _foodStockRepository = foodStockRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // =====================================================================
        // Role-scoped read methods
        // =====================================================================

        public async Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(
            string username,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null)
        {
            try
            {
                _logger.LogInformation("Getting paged food stocks - User: {User}, PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                    username, pageNumber, pageSize, searchTerm ?? "null");

                // Validate pagination parameters.
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                // Delegate scoping + filtering to the repository.
                var result = await _foodStockRepository.GetPagedFoodStocksWithSiteAsync(
                    username,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    siteId
                );

                var items = result.Items;
                var totalCount = result.TotalCount;

                // Domain DTO -> Application DTO.
                var mappedItems = _mapper.Map<List<FoodStockDto>>(items);

                var paginatedResponse = new PaginatedResponse<FoodStockDto>
                {
                    Items = mappedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation("Successfully retrieved {Count} food stocks out of {TotalCount}",
                    mappedItems.Count, totalCount);

                return ApiResponse<PaginatedResponse<FoodStockDto>>.Success(
                    paginatedResponse,
                    "Food stocks with site details retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve paged food stocks - User: {User}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                    username, pageNumber, pageSize);
                return ApiResponse<PaginatedResponse<FoodStockDto>>.Failure(
                    $"Failed to retrieve food stocks: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Getting summary statistics for user {Username}", username);

                var summary = await _foodStockRepository.GetSummaryStatsAsync(username);

                _logger.LogInformation("Successfully retrieved summary statistics - TotalSites: {TotalSites}, NotUpdated: {NotUpdated}, LowStockSites: {LowStockSites}",
                    summary.TotalSites, summary.NotUpdated, summary.LowStockSites);

                return ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Success(
                    summary,
                    "Summary retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve summary statistics");
                return ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>.Failure(
                    $"Failed to retrieve summary: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<byte[]>> ExportFoodStocksAsync(
            string username,
            string? searchTerm,
            string? province,
            string? district,
            string? tehsil,
            string format)
        {
            try
            {
                _logger.LogInformation("Exporting food stocks - User: {User}, SearchTerm: {SearchTerm}, Province: {Province}, District: {District}, Tehsil: {Tehsil}, Format: {Format}",
                    username, searchTerm ?? "null", province ?? "null", district ?? "null", tehsil ?? "null", format);

                // Role-scoped fetch, then serialize to bytes.
                var data = await _foodStockRepository.GetExportDataAsync(
                    username, searchTerm, province, district, tehsil);
                var bytes = await _foodStockRepository.GenerateExportFileAsync(data, format);

                _logger.LogInformation("Successfully exported {Count} food stocks in {Format} format",
                    data.Count, format);

                return ApiResponse<byte[]>.Success(
                    bytes,
                    "Export generated successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export food stocks - Format: {Format}", format);
                return ApiResponse<byte[]>.Failure(
                    $"Failed to export: {ex.Message}"
                );
            }
        }

        // =====================================================================
        // Simple single-row operations
        // =====================================================================

        public async Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id)
        {
            try
            {
                _logger.LogInformation("Getting food stock by ID: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ID provided: {Id}", id);
                    return ApiResponse<FoodStockDto>.Failure("Invalid ID provided");
                }

                var foodStock = await _foodStockRepository.GetByIdAsync(id);

                if (foodStock == null)
                {
                    _logger.LogWarning("Food stock with ID {Id} not found", id);
                    return ApiResponse<FoodStockDto>.Failure($"Food stock with ID {id} not found");
                }

                var mappedItem = _mapper.Map<FoodStockDto>(foodStock);

                _logger.LogInformation("Successfully retrieved food stock with ID: {Id}", id);

                return ApiResponse<FoodStockDto>.Success(
                    mappedItem,
                    "Food stock retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve food stock by ID: {Id}", id);
                return ApiResponse<FoodStockDto>.Failure(
                    $"Failed to retrieve food stock: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<bool>> DeleteFoodStockAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting food stock with ID: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ID provided for deletion: {Id}", id);
                    return ApiResponse<bool>.Failure("Invalid ID provided");
                }

                var existing = await _foodStockRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Food stock with ID {Id} not found for deletion", id);
                    return ApiResponse<bool>.Failure($"Food stock with ID {id} not found");
                }

                await _foodStockRepository.DeleteAsync(existing);
                await _foodStockRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted food stock with ID: {Id}", id);

                return ApiResponse<bool>.Success(true, "Food stock deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete food stock with ID: {Id}", id);
                return ApiResponse<bool>.Failure(
                    $"Failed to delete food stock: {ex.Message}"
                );
            }
        }

        // =====================================================================
        // Save / update
        // =====================================================================

        /// <summary>
        /// Saves or updates stock information for a specific site with sequential
        /// date validation (no future dates, no gaps in weekday dates, no dupes).
        /// </summary>
        public async Task<ApiResponse<FoodStockDto>> SaveStockInformationAsync(SaveStockInformationRequest request)
        {
            try
            {
                _logger.LogInformation("Saving stock information - SiteId: {SiteId}, EnteredOn: {EnteredOn}, EnteredBy: {EnteredBy}",
                    request.SiteId ?? "null", request.EnteredOn ?? "null", request.EnteredBy ?? "null");

                // ---- Basic validation ------------------------------------------
                if (string.IsNullOrEmpty(request.SiteId))
                {
                    _logger.LogWarning("SaveStockInformation failed: Site ID is required");
                    return ApiResponse<FoodStockDto>.Failure("Site ID is required");
                }

                if (string.IsNullOrEmpty(request.OpeningStockBoxesWawa))
                {
                    _logger.LogWarning("SaveStockInformation failed: Opening stock boxes WAWA is required for SiteId: {SiteId}",
                        request.SiteId);
                    return ApiResponse<FoodStockDto>.Failure("Opening stock boxes WAWA is required");
                }

                // ---- Resolve target date ---------------------------------------
                string todayDate = DateTime.Now.ToString("dd-MM-yyyy");
                string enteredOn = !string.IsNullOrEmpty(request.EnteredOn) && request.EnteredOn != "null"
                    ? request.EnteredOn
                    : todayDate;

                _logger.LogDebug("Using entered date: {EnteredOn} for SiteId: {SiteId}", enteredOn, request.SiteId);

                var acceptedFormats = new[] { "yyyy-MM-dd", "dd-MM-yyyy" };
                if (!DateTime.TryParseExact(enteredOn, acceptedFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var targetDate))
                {
                    _logger.LogWarning("Invalid date format: {EnteredOn} for SiteId: {SiteId}", enteredOn, request.SiteId);
                    return ApiResponse<FoodStockDto>.Failure(
                        "Invalid date format. Please use yyyy-MM-dd or dd-MM-yyyy.");
                }

                targetDate = targetDate.Date;
                var today = DateTime.Now.Date;

                // Store as dd-MM-yyyy regardless of input format.
                enteredOn = targetDate.ToString("dd-MM-yyyy");

                // ---- Reject future dates ---------------------------------------
                if (targetDate > today)
                {
                    _logger.LogWarning("Cannot save stock for future date {EnteredOn} for SiteId: {SiteId}",
                        enteredOn, request.SiteId);
                    return ApiResponse<FoodStockDto>.Failure(
                        $"Cannot save stock for future date {enteredOn}.",
                        new List<string> { "FutureDate" });
                }

                // ---- Existing record for this date? ----------------------------
                var existingStock = await _foodStockRepository.GetBySiteIdAndDateAsync(request.SiteId, enteredOn);

                if (existingStock != null)
                {
                    // Manual-update path: overwrite the existing row.
                    if (!string.IsNullOrEmpty(request.IsManualUpdate) &&
                        (request.IsManualUpdate == "1" ||
                         request.IsManualUpdate.Equals("true", StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogInformation("Manual update requested for existing stock - SiteId: {SiteId} on Date: {EnteredOn}",
                            request.SiteId, enteredOn);

                        existingStock.OpeningStockBoxesMamta = ParseString(request.OpeningStockBoxesMamta);
                        existingStock.ReceivedStockBoxesMamta = ParseString(request.ReceivedStockBoxesMamta);
                        existingStock.DistributedBoxesMamta = ParseString(request.DistributedBoxesMamta);
                        existingStock.ClosingStockBoxesMamta = ParseString(request.ClosingStockBoxesMamta);
                        existingStock.OpeningStockSachetsMamta = ParseString(request.OpeningStockSachetsMamta);
                        existingStock.ClosingStockSachetsMamta = ParseString(request.ClosingStockSachetsMamta);
                        existingStock.DistributedSachetsMamta = ParseString(request.DistributedSachetsMamta);

                        existingStock.OpeningStockBoxesWawa = ParseString(request.OpeningStockBoxesWawa);
                        existingStock.ReceivedStockBoxesWawa = ParseString(request.ReceivedStockBoxesWawa);
                        existingStock.DistributedBoxesWawa = ParseString(request.DistributedBoxesWawa);
                        existingStock.ClosingStockBoxesWawa = ParseString(request.ClosingStockBoxesWawa);
                        existingStock.OpeningStockSachetsWawa = ParseString(request.OpeningStockSachetsWawa);
                        existingStock.ClosingStockSachetsWawa = ParseString(request.ClosingStockSachetsWawa);
                        existingStock.DistributedSachetsWawa = ParseString(request.DistributedSachetsWawa);

                        existingStock.Unit = ParseString(request.Unit);

                        existingStock.RutfReceived = ParseString(request.RutfReceived);
                        existingStock.RutfOpening = ParseString(request.RutfOpening);
                        existingStock.RutfDistributed = ParseString(request.RutfDistributed);
                        existingStock.RutfClosing = ParseString(request.RutfClosing);

                        existingStock.IfaReceived = ParseString(request.IfaReceived);
                        existingStock.IfaOpening = ParseString(request.IfaOpening);
                        existingStock.IfaDistributed = ParseString(request.IfaDistributed);
                        existingStock.IfaClosing = ParseString(request.IfaClosing);

                        existingStock.MmsReceived = ParseString(request.MmsReceived);
                        existingStock.MmsOpening = ParseString(request.MmsOpening);
                        existingStock.MmsDistributed = ParseString(request.MmsDistributed);
                        existingStock.MmsClosing = ParseString(request.MmsClosing);

                        existingStock.Remarks = ParseString(request.Remarks);
                        existingStock.EnteredBy = ParseString(request.EnteredBy) ?? existingStock.EnteredBy;
                        existingStock.ActivityTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                        existingStock.IsManualUpdate = "1";

                        await _foodStockRepository.SaveChangesAsync();

                        var mappedUpdated = _mapper.Map<FoodStockDto>(existingStock);
                        return ApiResponse<FoodStockDto>.Success(
                            mappedUpdated,
                            "Stock Information updated successfully (manual update)");
                    }

                    // Otherwise reject duplicate entries for the same day.
                    _logger.LogWarning("Stock already exists for SiteId: {SiteId} on Date: {EnteredOn}",
                        request.SiteId, enteredOn);
                    return ApiResponse<FoodStockDto>.Failure(
                        $"Stock information already exists for {enteredOn}! You cannot add multiple entries for the same day.",
                        new List<string> { "AlreadyExist" });
                }

                // ---- Enforce chronological entry with no weekday gaps ----------
                var existingDates = await _foodStockRepository.GetExistingStockDatesAsync(request.SiteId)
                                    ?? new List<string>();

                var parsedExistingDates = existingDates
                    .Select(d => DateTime.TryParseExact(d, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var dt) ? dt.Date : (DateTime?)null)
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                if (parsedExistingDates.Count > 0)
                {
                    // The most recent existing entry.
                    var lastExistingDate = parsedExistingDates[0];

                    // Must be strictly after the latest existing date.
                    if (targetDate <= lastExistingDate)
                    {
                        _logger.LogWarning(
                            "Cannot save stock for {EnteredOn}. Latest existing record is {LastDate} for SiteId: {SiteId}",
                            enteredOn, lastExistingDate.ToString("dd-MM-yyyy"), request.SiteId);

                        return ApiResponse<FoodStockDto>.Failure(
                            $"Cannot save stock for {enteredOn}. Please enter dates in chronological order. " +
                            $"Latest existing record is for {lastExistingDate:dd-MM-yyyy}.",
                            new List<string> { "InvalidSequence" });
                    }

                    // No non-weekend day may be skipped between lastExistingDate+1 and targetDate-1.
                    var existingSet = parsedExistingDates.ToHashSet();
                    var missingDates = new List<DateTime>();

                    for (var date = lastExistingDate.AddDays(1); date < targetDate; date = date.AddDays(1))
                    {
                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                            continue;

                        if (!existingSet.Contains(date.Date))
                            missingDates.Add(date);
                    }

                    if (missingDates.Count > 0)
                    {
                        missingDates = missingDates.OrderBy(d => d).ToList();
                        var missingDatesList = string.Join(", ", missingDates.Select(d => d.ToString("dd-MM-yyyy")));

                        _logger.LogWarning(
                            "Cannot save stock for {EnteredOn}. Missing previous dates for SiteId: {SiteId}. Missing: {MissingDates}",
                            enteredOn, request.SiteId, missingDatesList);

                        var firstMissing = missingDates[0].ToString("dd-MM-yyyy");

                        return ApiResponse<FoodStockDto>.Failure(
                            $"Cannot save stock for {enteredOn}. Please first enter stock for {firstMissing}. " +
                            $"Missing date(s) in order: {missingDatesList}",
                            new List<string> { "MissingPreviousDates" });
                    }
                }

                // ---- Create new row -------------------------------------------
                var stock = new NthSnfStock
                {
                    OpeningStockBoxesMamta = ParseString(request.OpeningStockBoxesMamta),
                    ReceivedStockBoxesMamta = ParseString(request.ReceivedStockBoxesMamta),
                    DistributedBoxesMamta = ParseString(request.DistributedBoxesMamta),
                    ClosingStockBoxesMamta = ParseString(request.ClosingStockBoxesMamta),
                    OpeningStockSachetsMamta = ParseString(request.OpeningStockSachetsMamta),
                    ClosingStockSachetsMamta = ParseString(request.ClosingStockSachetsMamta),
                    DistributedSachetsMamta = ParseString(request.DistributedSachetsMamta),

                    OpeningStockBoxesWawa = ParseString(request.OpeningStockBoxesWawa),
                    ReceivedStockBoxesWawa = ParseString(request.ReceivedStockBoxesWawa),
                    DistributedBoxesWawa = ParseString(request.DistributedBoxesWawa),
                    ClosingStockBoxesWawa = ParseString(request.ClosingStockBoxesWawa),
                    OpeningStockSachetsWawa = ParseString(request.OpeningStockSachetsWawa),
                    ClosingStockSachetsWawa = ParseString(request.ClosingStockSachetsWawa),
                    DistributedSachetsWawa = ParseString(request.DistributedSachetsWawa),

                    Unit = ParseString(request.Unit),

                    RutfReceived = ParseString(request.RutfReceived),
                    RutfOpening = ParseString(request.RutfOpening),
                    RutfDistributed = ParseString(request.RutfDistributed),
                    RutfClosing = ParseString(request.RutfClosing),

                    IfaReceived = ParseString(request.IfaReceived),
                    IfaOpening = ParseString(request.IfaOpening),
                    IfaDistributed = ParseString(request.IfaDistributed),
                    IfaClosing = ParseString(request.IfaClosing),

                    MmsReceived = ParseString(request.MmsReceived),
                    MmsOpening = ParseString(request.MmsOpening),
                    MmsDistributed = ParseString(request.MmsDistributed),
                    MmsClosing = ParseString(request.MmsClosing),

                    Remarks = ParseString(request.Remarks),
                    SiteId = ParseString(request.SiteId),

                    EnteredOn = enteredOn,
                    EnteredBy = request.EnteredBy,
                    ActivityTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    IsManualUpdate = "1"
                };

                _logger.LogDebug("Created stock entity for SiteId: {SiteId} with OpeningStockBoxesWawa: {OpeningStock}",
                    request.SiteId, stock.OpeningStockBoxesWawa);

                var created = await _foodStockRepository.AddAsync(stock);
                await _foodStockRepository.SaveChangesAsync();

                var mappedItem = _mapper.Map<FoodStockDto>(created);

                _logger.LogInformation(
                    "Stock information saved successfully for Site: {SiteId} on Date: {EnteredOn} by User: {EnteredBy}",
                    request.SiteId, enteredOn, request.EnteredBy ?? "System");

                return ApiResponse<FoodStockDto>.Success(mappedItem, "Stock Information saved successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock information for SiteId: {SiteId}, Date: {EnteredOn}",
                    request.SiteId ?? "null", request.EnteredOn ?? "null");
                return ApiResponse<FoodStockDto>.Failure(
                    $"Failed to save stock information: {ex.Message}");
            }
        }

        // =====================================================================
        // Site-scoped read methods
        // =====================================================================

        /// <summary>
        /// Returns all stock rows for a site, newest first.
        /// </summary>
        public async Task<ApiResponse<List<FoodStockDto>>> GetFoodStocksBySiteIdAsync(string siteId)
        {
            try
            {
                _logger.LogInformation("Getting food stocks by SiteId: {SiteId}", siteId ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("GetFoodStocksBySiteId failed: Site ID is required");
                    return ApiResponse<List<FoodStockDto>>.Failure("Site ID is required");
                }

                var stocks = await _foodStockRepository.GetBySiteIdAsync(siteId);
                var mappedItems = _mapper.Map<List<FoodStockDto>>(stocks);

                _logger.LogInformation("Successfully retrieved {Count} food stocks for SiteId: {SiteId}",
                    mappedItems.Count, siteId);

                return ApiResponse<List<FoodStockDto>>.Success(
                    mappedItems,
                    "Food stocks retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve food stocks for SiteId: {SiteId}", siteId);
                return ApiResponse<List<FoodStockDto>>.Failure(
                    $"Failed to retrieve food stocks: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Returns missing (weekday) dates between the latest existing entry and today.
        /// </summary>
        public async Task<ApiResponse<List<MissingStockDateDto>>> GetMissingStockDatesAsync(string siteId)
        {
            try
            {
                _logger.LogInformation("Getting missing stock dates for SiteId: {SiteId}", siteId ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("GetMissingStockDates failed: Site ID is required");
                    return ApiResponse<List<MissingStockDateDto>>.Failure("Site ID is required");
                }

                // Repository returns distinct EnteredOn values, newest first.
                var existingDates = await _foodStockRepository.GetExistingStockDatesAsync(siteId);
                _logger.LogDebug("Found {Count} existing stock dates for SiteId: {SiteId}",
                    existingDates.Count, siteId);

                // No records at all -> nothing to compare.
                if (existingDates == null || existingDates.Count == 0)
                {
                    _logger.LogWarning("No existing stock dates found for SiteId: {SiteId}", siteId);
                    return ApiResponse<List<MissingStockDateDto>>.Success(
                        new List<MissingStockDateDto>(),
                        "No orders found for this site");
                }

                // PINPOINT = the LATEST (newest) EnteredOn date -> first element.
                if (!DateTime.TryParseExact(existingDates[0], "dd-MM-yyyy", null,
                        DateTimeStyles.None, out var startDate))
                {
                    _logger.LogWarning("Latest EnteredOn '{Raw}' is not dd-MM-yyyy for SiteId: {SiteId}",
                        existingDates[0], siteId);
                    return ApiResponse<List<MissingStockDateDto>>.Failure(
                        "Invalid latest order date format");
                }

                startDate = startDate.Date;
                var endDate = DateTime.Now.Date;

                // If the latest EnteredOn is today (or future), there's nothing to check.
                if (startDate >= endDate)
                {
                    _logger.LogInformation(
                        "Latest EnteredOn ({Start}) is today or later for SiteId: {SiteId} — nothing missing",
                        startDate.ToString("dd-MM-yyyy"), siteId);

                    return ApiResponse<List<MissingStockDateDto>>.Success(
                        new List<MissingStockDateDto>(),
                        "No missing stock dates");
                }

                _logger.LogDebug("Checking missing stock dates from {Start} (latest EnteredOn) to {End} (today) for SiteId: {SiteId}",
                    startDate.ToString("dd-MM-yyyy"), endDate.ToString("dd-MM-yyyy"), siteId);

                // HashSet for O(1) lookups of existing dates.
                var existingDateSet = new HashSet<string>(
                    existingDates.Where(d => !string.IsNullOrEmpty(d)),
                    StringComparer.OrdinalIgnoreCase);

                var missingDates = new List<MissingStockDateDto>();

                // Walk only from the latest EnteredOn -> today.
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var dateStr = date.ToString("dd-MM-yyyy");

                    if (!existingDateSet.Contains(dateStr))
                    {
                        missingDates.Add(new MissingStockDateDto
                        {
                            Date = dateStr,
                            IsMissing = true,
                            DisplayDate = date.ToString("dd-MMM-yyyy")
                        });
                    }
                }

                // Newest / closest first.
                missingDates = missingDates
                    .OrderByDescending(d => DateTime.ParseExact(d.Date, "dd-MM-yyyy", null))
                    .ToList();

                _logger.LogInformation(
                    "Found {MissingCount} missing stock dates for SiteId: {SiteId} between {Start} and {End}",
                    missingDates.Count, siteId,
                    startDate.ToString("dd-MM-yyyy"), endDate.ToString("dd-MM-yyyy"));

                return ApiResponse<List<MissingStockDateDto>>.Success(
                    missingDates,
                    "Missing stock dates retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve missing stock dates for SiteId: {SiteId}", siteId);
                return ApiResponse<List<MissingStockDateDto>>.Failure(
                    $"Failed to retrieve missing stock dates: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Indicates whether a stock row exists for the given site + date.
        /// </summary>
        public async Task<ApiResponse<bool>> ValidateStockExistsForDateAsync(string siteId, string date)
        {
            try
            {
                _logger.LogInformation("Validating stock existence - SiteId: {SiteId}, Date: {Date}",
                    siteId ?? "null", date ?? "null");

                if (string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(date))
                {
                    _logger.LogWarning("ValidateStockExists failed: Site ID and date are required");
                    return ApiResponse<bool>.Failure("Site ID and date are required");
                }

                var stock = await _foodStockRepository.GetBySiteIdAndDateAsync(siteId, date);
                _logger.LogInformation("Stock exists for SiteId: {SiteId}, Date: {Date}: {Exists}",
                    siteId, date, stock != null);

                return ApiResponse<bool>.Success(stock != null, stock != null ? "Stock exists" : "Stock does not exist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate stock existence for SiteId: {SiteId}, Date: {Date}",
                    siteId, date);
                return ApiResponse<bool>.Failure(
                    $"Failed to validate stock existence: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Returns the next date the user should enter stock for, plus any missing
        /// dates between the latest existing entry and today.
        /// </summary>
        public async Task<ApiResponse<NextRequiredDateDto>> GetNextRequiredDateAsync(string siteId)
        {
            try
            {
                _logger.LogInformation("Getting next required date for SiteId: {SiteId}", siteId ?? "null");

                if (string.IsNullOrEmpty(siteId))
                {
                    _logger.LogWarning("GetNextRequiredDate failed: Site ID is required");
                    return ApiResponse<NextRequiredDateDto>.Failure("Site ID is required");
                }

                var today = DateTime.Now.Date;

                var existingDates = await _foodStockRepository.GetExistingStockDatesAsync(siteId);

                var parsedExistingDates = new List<DateTime>();
                foreach (var dateStr in existingDates)
                {
                    if (!string.IsNullOrEmpty(dateStr) &&
                        DateTime.TryParseExact(dateStr, "dd-MM-yyyy", null, DateTimeStyles.None, out var date))
                    {
                        parsedExistingDates.Add(date.Date);
                    }
                }

                // Dedupe + sort DESCENDING (latest first).
                parsedExistingDates = parsedExistingDates
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                // Start the walk from the day after the latest existing record,
                // or from today if there are none.
                DateTime startDate;
                if (parsedExistingDates.Any())
                    startDate = parsedExistingDates.First().AddDays(1);
                else
                    startDate = today;

                // Collect missing weekdays between startDate and today.
                var missingDates = new List<DateTime>();
                for (var date = startDate; date <= today; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    if (parsedExistingDates.Any(d => d.Date == date.Date))
                        continue;

                    missingDates.Add(date);
                }

                var response = new NextRequiredDateDto
                {
                    SiteId = siteId,
                    TodayDate = today.ToString("dd-MM-yyyy"),
                    HasMissingDates = missingDates.Any(),
                    MissingDates = missingDates.Select(d => new MissingStockDateDto
                    {
                        Date = d.ToString("dd-MM-yyyy"),
                        DisplayDate = d.ToString("dd-MMM-yyyy"),
                        IsMissing = true
                    }).ToList(),
                    NextRequiredDate = missingDates.Any() ? missingDates.First().ToString("dd-MM-yyyy") : null,
                    NextRequiredDateDisplay = missingDates.Any() ? missingDates.First().ToString("dd-MMM-yyyy") : null,
                    TotalMissingCount = missingDates.Count
                };

                _logger.LogInformation(
                    "Found {MissingCount} missing dates for SiteId: {SiteId}. Next required date: {NextDate}",
                    missingDates.Count, siteId, response.NextRequiredDate ?? "None");

                return ApiResponse<NextRequiredDateDto>.Success(
                    response,
                    missingDates.Any()
                        ? $"Please enter stock for {missingDates.Count} missing date(s). Next required date: {response.NextRequiredDateDisplay}"
                        : "All stock entries are up to date."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get next required date for SiteId: {SiteId}", siteId);
                return ApiResponse<NextRequiredDateDto>.Failure(
                    $"Failed to get next required date: {ex.Message}"
                );
            }
        }



        #region Private Helper Methods

        /// <summary>
        /// Trims the value and normalises null/empty/"null" to null.
        /// </summary>
        private string? ParseString(string? value)
        {
            if (string.IsNullOrEmpty(value) || value == "null")
                return null;

            return value.Trim();
        }

        /// <summary>
        /// Parses a string to decimal; returns null on empty/"null"/unparseable.
        /// </summary>
        private decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrEmpty(value) || value == "null")
                return null;

            if (decimal.TryParse(value, out decimal result))
                return result;

            _logger.LogDebug("Failed to parse decimal value: {Value}", value);
            return null;
        }

        #endregion
    }
}
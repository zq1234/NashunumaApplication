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

        public async Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting paged food stocks - PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm ?? "null");

                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                // Get paged data with site information
                var result = await _foodStockRepository.GetPagedFoodStocksWithSiteAsync(
                    pageNumber,
                    pageSize,
                    searchTerm
                );

                // Extract items and total count
                var items = result.Items;
                var totalCount = result.TotalCount;

                // Map Domain DTO to Application DTO using AutoMapper
                var mappedItems = _mapper.Map<List<FoodStockDto>>(items);

                // Create paginated response
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
                _logger.LogError(ex, "Failed to retrieve paged food stocks - PageNumber: {PageNumber}, PageSize: {PageSize}",
                    pageNumber, pageSize);
                return ApiResponse<PaginatedResponse<FoodStockDto>>.Failure(
                    $"Failed to retrieve food stocks: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id)
        {
            try
            {
                _logger.LogInformation("Getting food stock by ID: {Id}", id);

                // Validate ID
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ID provided: {Id}", id);
                    return ApiResponse<FoodStockDto>.Failure("Invalid ID provided");
                }

                // Get the food stock by ID
                var foodStock = await _foodStockRepository.GetByIdAsync(id);

                // Check if record exists
                if (foodStock == null)
                {
                    _logger.LogWarning("Food stock with ID {Id} not found", id);
                    return ApiResponse<FoodStockDto>.Failure($"Food stock with ID {id} not found");
                }

                // Map to DTO
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

        public async Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync()
        {
            try
            {
                _logger.LogInformation("Getting summary statistics");

                var summary = await _foodStockRepository.GetSummaryStatsAsync();

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

        public async Task<ApiResponse<byte[]>> ExportFoodStocksAsync(string? searchTerm, string format)
        {
            try
            {
                _logger.LogInformation("Exporting food stocks - SearchTerm: {SearchTerm}, Format: {Format}",
                    searchTerm ?? "null", format);

                var data = await _foodStockRepository.GetExportDataAsync(searchTerm);
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

        #region Stock Information Management

        /// Saves or updates stock information for a specific site with sequential date validation
        public async Task<ApiResponse<FoodStockDto>> SaveStockInformationAsync(SaveStockInformationRequest request)
        {
            try
            {
                _logger.LogInformation("Saving stock information - SiteId: {SiteId}, EnteredOn: {EnteredOn}, EnteredBy: {EnteredBy}",
                    request.SiteId ?? "null", request.EnteredOn ?? "null", request.EnteredBy ?? "null");

                // Validate required fields
                if (string.IsNullOrEmpty(request.SiteId))
                {
                    _logger.LogWarning("SaveStockInformation failed: Site ID is required");
                    return ApiResponse<FoodStockDto>.Failure("Site ID is required");
                }

                // Check if opening stock boxes wawa is provided 
                if (string.IsNullOrEmpty(request.OpeningStockBoxesWawa))
                {
                    _logger.LogWarning("SaveStockInformation failed: Opening stock boxes WAWA is required for SiteId: {SiteId}",
                        request.SiteId);
                    return ApiResponse<FoodStockDto>.Failure("Opening stock boxes WAWA is required");
                }

                // Get today's date in the format used by the system
                string todayDate = DateTime.Now.ToString("dd-MM-yyyy");
                string enteredOn = !string.IsNullOrEmpty(request.EnteredOn) && request.EnteredOn != "null"
                    ? request.EnteredOn
                    : todayDate;

                _logger.LogDebug("Using entered date: {EnteredOn} for SiteId: {SiteId}", enteredOn, request.SiteId);

                // Parse the entered date
                if (!DateTime.TryParseExact(enteredOn, "dd-MM-yyyy", null, DateTimeStyles.None, out var targetDate))
                {
                    _logger.LogWarning("Invalid date format: {EnteredOn} for SiteId: {SiteId}", enteredOn, request.SiteId);
                    return ApiResponse<FoodStockDto>.Failure("Invalid date format. Please use dd-MM-yyyy format.");
                }

                // Check if stock already exists for this date
                var existingStock = await _foodStockRepository.GetBySiteIdAndDateAsync(request.SiteId, enteredOn);
                if (existingStock != null)
                {
                    _logger.LogWarning("Stock already exists for SiteId: {SiteId} on Date: {EnteredOn}",
                        request.SiteId, enteredOn);
                    return ApiResponse<FoodStockDto>.Failure(
                        $"Stock information already exists for {enteredOn}! You cannot add multiple entries for the same day.",
                        new List<string> { "AlreadyExist" }
                    );
                }

                // Get all existing stock dates for this site
                var existingDates = await _foodStockRepository.GetExistingStockDatesAsync(request.SiteId);
                var parsedExistingDates = new List<DateTime>();

                foreach (var dateStr in existingDates)
                {
                    if (!string.IsNullOrEmpty(dateStr) &&
                        DateTime.TryParseExact(dateStr, "dd-MM-yyyy", null, DateTimeStyles.None, out var date))
                    {
                        parsedExistingDates.Add(date);
                    }
                }

                // Sort existing dates (oldest to newest)
                parsedExistingDates = parsedExistingDates.OrderBy(d => d).ToList();

                // Check if we have any existing records
                var today = DateTime.Now.Date;

                // Sequential date validation
                if (parsedExistingDates.Any())
                {
                    var lastExistingDate = parsedExistingDates.Max();

                    // Check if trying to save a date older than the last existing date
                    if (targetDate.Date < lastExistingDate.Date)
                    {
                        _logger.LogWarning("Cannot save stock for {EnteredOn}. Last existing record is for {LastDate}",
                            enteredOn, lastExistingDate.ToString("dd-MM-yyyy"));
                        return ApiResponse<FoodStockDto>.Failure(
                            $"Cannot save stock for {enteredOn}. Please enter dates in chronological order. Last existing record is for {lastExistingDate:dd-MM-yyyy}.",
                            new List<string> { "InvalidSequence" }
                        );
                    }

                    // Check if there's a gap between the last existing date and the target date
                    var expectedDate = lastExistingDate.Date.AddDays(1);
                    var missingDates = new List<DateTime>();

                    // Check all dates from the day after the last existing date to the target date
                    for (var date = expectedDate; date < targetDate.Date; date = date.AddDays(1))
                    {
                        if (!parsedExistingDates.Any(d => d.Date == date.Date))
                        {
                            missingDates.Add(date);
                        }
                    }

                    if (missingDates.Any())
                    {
                        var missingDatesList = string.Join(", ", missingDates.Select(d => d.ToString("dd-MM-yyyy")));
                        _logger.LogWarning("Cannot save stock for {EnteredOn}. Missing previous dates for SiteId: {SiteId}. Missing dates: {MissingDates}",
                            enteredOn, request.SiteId, missingDatesList);

                        return ApiResponse<FoodStockDto>.Failure(
                            $"Cannot save stock for {enteredOn}. Please first enter stock for the following missing date(s): {missingDatesList}",
                            new List<string> { "MissingPreviousDates" }
                        );
                    }
                }
                else
                {
                    // No existing records - check if this is a valid starting date
                    // Allow saving for today or any date from the last 30 days
                    var startDate = today.AddDays(-30);
                    if (targetDate.Date < startDate)
                    {
                        _logger.LogWarning("Cannot save stock for {EnteredOn}. Date is too old.", enteredOn);
                        return ApiResponse<FoodStockDto>.Failure(
                            $"Cannot save stock for {enteredOn}. The date is too far in the past. Please start from {startDate:dd-MM-yyyy} or later.",
                            new List<string> { "DateTooOld" }
                        );
                    }
                }

                // Create new stock entity
                var stock = new NthSnfStock
                {
                    OpeningStockBoxesMamta = ParseString(request.OpeningStockBoxesMamta),
                    ReceivedStockBoxesMamta = ParseString(request.ReceivedStockBoxesMamta),
                    DistributedBoxesMamta = ParseString(request.DistributedBoxesMamta),
                    ClosingStockBoxesMamta = ParseString(request.ClosingStockBoxesMamta),
                    Remarks = ParseString(request.Remarks),
                    EnteredBy = ParseString(request.EnteredBy),
                    SiteId = ParseString(request.SiteId),
                    OpeningStockSachetsMamta = ParseString(request.OpeningStockSachetsMamta),
                    ClosingStockSachetsMamta = ParseString(request.ClosingStockSachetsMamta),
                    DistributedSachetsMamta = ParseString(request.DistributedSachetsMamta),
                    OpeningStockSachetsWawa = ParseString(request.OpeningStockSachetsWawa),
                    ClosingStockSachetsWawa = ParseString(request.ClosingStockSachetsWawa),
                    DistributedSachetsWawa = ParseString(request.DistributedSachetsWawa),
                    OpeningStockBoxesWawa = ParseString(request.OpeningStockBoxesWawa),
                    ReceivedStockBoxesWawa = ParseString(request.ReceivedStockBoxesWawa),
                    DistributedBoxesWawa = ParseString(request.DistributedBoxesWawa),
                    ClosingStockBoxesWawa = ParseString(request.ClosingStockBoxesWawa),
                    Unit = ParseString(request.Unit),

                    // Latest additions
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

                    EnteredOn = enteredOn,
                    ActivityTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                    IsManualUpdate = "1"
                };

                _logger.LogDebug("Created stock entity for SiteId: {SiteId} with OpeningStockBoxesWawa: {OpeningStock}",
                    request.SiteId, stock.OpeningStockBoxesWawa);

                // Save to repository
                var created = await _foodStockRepository.AddAsync(stock);
                await _foodStockRepository.SaveChangesAsync();

                // Map to DTO
                var mappedItem = _mapper.Map<FoodStockDto>(created);

                _logger.LogInformation(
                    "Stock information saved successfully for Site: {SiteId} on Date: {EnteredOn} by User: {EnteredBy}",
                    request.SiteId,
                    enteredOn,
                    request.EnteredBy ?? "System"
                );

                return ApiResponse<FoodStockDto>.Success(
                    mappedItem,
                    "Stock Information saved successfully!"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock information for SiteId: {SiteId}, Date: {EnteredOn}",
                    request.SiteId ?? "null", request.EnteredOn ?? "null");
                return ApiResponse<FoodStockDto>.Failure(
                    $"Failed to save stock information: {ex.Message}"
                );
            }
        }

        
        /// Gets all food stocks for a specific site

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

        
        /// Gets missing stock dates for a specific site

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

                // Get all existing stock dates for this site
                var existingDates = await _foodStockRepository.GetExistingStockDatesAsync(siteId);
                _logger.LogDebug("Found {Count} existing stock dates for SiteId: {SiteId}",
                    existingDates.Count, siteId);

                var parsedDates = new List<DateTime>();

                foreach (var dateStr in existingDates)
                {
                    if (!string.IsNullOrEmpty(dateStr) &&
                        DateTime.TryParseExact(dateStr, "dd-MM-yyyy", null, DateTimeStyles.None, out var date))
                    {
                        parsedDates.Add(date);
                    }
                    else
                    {
                        _logger.LogDebug("Could not parse date string: {DateStr} for SiteId: {SiteId}",
                            dateStr ?? "null", siteId);
                    }
                }

                // Get the last 30 days
                var missingDates = new List<MissingStockDateDto>();
                var startDate = DateTime.Now.AddDays(-30);
                var endDate = DateTime.Now;

                // If no existing dates, check from start date
                if (parsedDates.Count == 0)
                {
                    _logger.LogDebug("No existing stock dates found for SiteId: {SiteId}, checking all dates in range",
                        siteId);
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        missingDates.Add(new MissingStockDateDto
                        {
                            Date = date.ToString("dd-MM-yyyy"),
                            IsMissing = true,
                            DisplayDate = date.ToString("dd-MMM-yyyy")
                        });
                    }
                }
                else
                {
                    // Check each date in the range
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        var dateStr = date.ToString("dd-MM-yyyy");
                        if (!parsedDates.Any(d => d.ToString("dd-MM-yyyy") == dateStr))
                        {
                            missingDates.Add(new MissingStockDateDto
                            {
                                Date = dateStr,
                                IsMissing = true,
                                DisplayDate = date.ToString("dd-MMM-yyyy")
                            });
                        }
                    }
                }

                // Sort missing dates (oldest first)
                missingDates = missingDates.OrderBy(d => DateTime.ParseExact(d.Date, "dd-MM-yyyy", null)).ToList();

                _logger.LogInformation("Found {MissingCount} missing stock dates for SiteId: {SiteId} out of {TotalDays} days",
                    missingDates.Count, siteId, (endDate - startDate).Days + 1);

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

        
        /// Validates if stock exists for a specific site and date

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

                return ApiResponse<bool>.Success(
                    stock != null,
                    stock != null ? "Stock exists" : "Stock does not exist"
                );
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

        
        /// Gets the next date that needs to be entered for a site

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
                        parsedExistingDates.Add(date);
                    }
                }

                var missingDates = new List<DateTime>();

                if (!parsedExistingDates.Any())
                {
                    // If no records exist, start from 30 days ago
                    var startDate = today.AddDays(-30);
                    for (var date = startDate; date < today; date = date.AddDays(1))
                    {
                        missingDates.Add(date);
                    }

                    // Add today if today is not yet saved
                    if (!parsedExistingDates.Any(d => d.Date == today))
                    {
                        missingDates.Add(today);
                    }
                }
                else
                {
                    var lastExistingDate = parsedExistingDates.Max();

                    // Check from the day after the last existing date to today
                    var nextDate = lastExistingDate.Date.AddDays(1);
                    for (var date = nextDate; date <= today; date = date.AddDays(1))
                    {
                        if (!parsedExistingDates.Any(d => d.Date == date.Date))
                        {
                            missingDates.Add(date);
                        }
                    }
                }

                // Sort missing dates (oldest first)
                missingDates = missingDates.OrderBy(d => d).ToList();

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

                _logger.LogInformation("Found {MissingCount} missing dates for SiteId: {SiteId}. Next required date: {NextDate}",
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

        #endregion

        #region Private Helper Methods

        
        /// Helper method to parse and clean string values

        private string? ParseString(string? value)
        {
            if (string.IsNullOrEmpty(value) || value == "null")
                return null;

            return value.Trim();
        }

        
        /// Helper method to parse string to decimal, returns null if empty

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
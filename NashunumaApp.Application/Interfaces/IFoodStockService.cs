// Application/Interfaces/IFoodStockService.cs

using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    /// <summary>
    /// Application service for food stock. Role-scoped methods require the
    /// current username so the repository can resolve site visibility:
    ///   - Admin      -> all active sites
    ///   - Non-admin  -> only the user's assigned site
    /// </summary>
    public interface IFoodStockService
    {
        /// <summary>
        /// Returns the latest stock row per scoped active site, paginated.
        /// </summary>
        Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(
            string username,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null);

        /// <summary>
        /// Returns a single food stock row by its primary key.
        /// </summary>
        Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id);

        /// <summary>
        /// Returns summary counters scoped to the user's role.
        /// </summary>
        Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync(string username);

        /// <summary>
        /// Returns the latest row per scoped active site as a file
        /// (Excel/JSON depending on the format argument).
        /// </summary>
        Task<ApiResponse<byte[]>> ExportFoodStocksAsync(
            string username,
            string? searchTerm,
            string? province,
            string? district,
            string? tehsil,
            string format);

        /// <summary>
        /// Deletes a stock row by primary key.
        /// </summary>
        Task<ApiResponse<bool>> DeleteFoodStockAsync(int id);

        /// <summary>
        /// Creates or updates a stock row for a specific site + date.
        /// </summary>
        Task<ApiResponse<FoodStockDto>> SaveStockInformationAsync(SaveStockInformationRequest request);

        /// <summary>
        /// Returns all stock rows for a specific site, newest first.
        /// </summary>
        Task<ApiResponse<List<FoodStockDto>>> GetFoodStocksBySiteIdAsync(string siteId);

        /// <summary>
        /// Returns the missing (unfilled) stock dates for a specific site.
        /// </summary>
        Task<ApiResponse<List<MissingStockDateDto>>> GetMissingStockDatesAsync(string siteId);

        /// <summary>
        /// Indicates whether a stock row exists for the given site + date.
        /// </summary>
        Task<ApiResponse<bool>> ValidateStockExistsForDateAsync(string siteId, string date);

        /// <summary>
        /// Returns the next date the user should enter stock for, plus any
        /// missing dates between the latest existing entry and today.
        /// </summary>
        Task<ApiResponse<NextRequiredDateDto>> GetNextRequiredDateAsync(string siteId);
    }
}
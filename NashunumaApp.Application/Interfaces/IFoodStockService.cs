// Application/Interfaces/IFoodStockService.cs

using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using NashunumaApp.Domain.Common.DTOs;

namespace NashunumaApp.Application.Interfaces
{
    public interface IFoodStockService
    {
        Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(
            string username, int pageNumber, int pageSize,
            string? searchTerm = null, string? siteId = null, string? province = null,
            string? district = null, string? tehsil = null, string? siteName = null,
            string? date = null, string? dateFrom = null, string? dateTo = null,
            string? enteredBy = null, string? headName = null);

        Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync(string username);

        Task<ApiResponse<byte[]>> ExportFoodStocksAsync(
            string username, string? searchTerm, string? siteId, string? siteName,
            string? province, string? district, string? tehsil,
            string? date, string? dateFrom, string? dateTo,
            string? enteredBy, string? headName, string format);

        Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id);
        Task<ApiResponse<bool>> DeleteFoodStockAsync(int id);
        Task<ApiResponse<FoodStockDto>> SaveStockInformationAsync(SaveStockInformationRequest request);

        Task<ApiResponse<List<FoodStockDto>>> GetFoodStocksBySiteIdAsync(string siteId);
        Task<ApiResponse<List<MissingStockDateDto>>> GetMissingStockDatesAsync(string siteId);
        Task<ApiResponse<bool>> ValidateStockExistsForDateAsync(string siteId, string date);
        Task<ApiResponse<NextRequiredDateDto>> GetNextRequiredDateAsync(string siteId);

    }
}
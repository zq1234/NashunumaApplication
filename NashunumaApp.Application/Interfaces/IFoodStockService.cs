using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.Stock;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface IFoodStockService
    {
        Task<ApiResponse<PaginatedResponse<FoodStockDto>>> GetPagedFoodStocksAsync(int pageNumber,int pageSize,string? searchTerm = null);
        Task<ApiResponse<FoodStockDto>> GetFoodStockByIdAsync(decimal id);
        Task<ApiResponse<Domain.Common.DTOs.FoodStockSummaryDto>> GetSummaryStatsAsync();

        Task<ApiResponse<byte[]>> ExportFoodStocksAsync(string? searchTerm, string format);

        Task<ApiResponse<bool>> DeleteFoodStockAsync(int id);

        Task<ApiResponse<FoodStockDto>> SaveStockInformationAsync(SaveStockInformationRequest request);
        Task<ApiResponse<List<FoodStockDto>>> GetFoodStocksBySiteIdAsync(string siteId);
        Task<ApiResponse<List<MissingStockDateDto>>> GetMissingStockDatesAsync(string siteId);
        Task<ApiResponse<bool>> ValidateStockExistsForDateAsync(string siteId, string date);
        Task<ApiResponse<NextRequiredDateDto>> GetNextRequiredDateAsync(string siteId);
    }
}
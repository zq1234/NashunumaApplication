using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
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
    }
}
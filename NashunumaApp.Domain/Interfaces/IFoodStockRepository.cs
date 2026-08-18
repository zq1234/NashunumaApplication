// Domain/Interfaces/IFoodStockRepository.cs
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Domain.Interfaces
{
    public interface IFoodStockRepository : IGenericRepository<NthSnfStock>
    {
        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null,
            string? province = null,
            string? district = null,
            string? tehsil = null);

        Task<FoodStockSummaryDto> GetSummaryStatsAsync();

        Task<List<FoodStockWithSiteDto>> GetExportDataAsync(
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null);

        Task<byte[]> GenerateExportFileAsync(List<FoodStockWithSiteDto> data, string format);

        Task<int> GetLatestStockCountBySiteAsync(string siteId);

        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10);
        Task<NthSnfStock> GetBySiteIdAndDateAsync(string siteId, string enteredOn);
        Task<List<NthSnfStock>> GetBySiteIdAsync(string siteId);
        Task<List<string>> GetExistingStockDatesAsync(string siteId);
    }
}
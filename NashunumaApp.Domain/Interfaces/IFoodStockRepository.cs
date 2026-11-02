// Domain/Interfaces/IFoodStockRepository.cs

using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Domain.Interfaces
{
    // Role-scoped: Admin = all rows on all active sites (no site/user filter); Non-admin = own active site + EnteredBy == username.
    public interface IFoodStockRepository : IGenericRepository<NthSnfStock>
    {
        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(string username, 
            int pageNumber, int pageSize, string? searchTerm = null, string? siteId = null,
            string? province = null, string? district = null, string? tehsil = null, 
            string? siteName = null, string? date = null, string? dateFrom = null, 
            string? dateTo = null, string? enteredBy = null, string? headName = null);
        Task<FoodStockSummaryDto> GetSummaryStatsAsync(string username);
        Task<List<FoodStockWithSiteDto>> GetExportDataAsync(string username, string? searchTerm = null, string? siteId = null, string? siteName = null, string? province = null, string? district = null, string? tehsil = null, string? date = null, string? dateFrom = null, string? dateTo = null, string? enteredBy = null, string? headName = null);
        Task<byte[]> GenerateExportFileAsync(List<FoodStockWithSiteDto> data, string format);
        Task<int> GetLatestStockCountBySiteAsync(string siteId);
        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(string username, string searchTerm, int pageNumber = 1, int pageSize = 10, string? siteId = null, string? siteName = null, string? province = null, string? district = null, string? tehsil = null, string? date = null, string? dateFrom = null, string? dateTo = null, string? enteredBy = null, string? headName = null);
        Task<NthSnfStock> GetBySiteIdAndDateAsync(string siteId, string enteredOn);
        Task<List<NthSnfStock>> GetBySiteIdAsync(string siteId);
        Task<List<string>> GetExistingStockDatesAsync(string siteId);
    }
}
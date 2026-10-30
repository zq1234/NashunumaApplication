// Domain/Interfaces/IFoodStockRepository.cs

using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Domain.Interfaces
{
    /// <summary>
    /// Repository contract for NthSnfStock (food stock).
    /// Role-scoped methods require a username:
    ///   - Admin (NthUsers.Isadmin == "True")  -> all active sites
    ///   - Non-admin                          -> only the user's assigned site
    /// </summary>
    public interface IFoodStockRepository : IGenericRepository<NthSnfStock>
    {
        /// <summary>
        /// Returns the latest stock row per active site (role-scoped),
        /// with optional search text and location filters.
        /// </summary>
        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(
            string username,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null,
            string? province = null,
            string? district = null,
            string? tehsil = null);

        /// <summary>
        /// Returns summary counters scoped to the user's role:
        /// TotalSites, NotUpdated (latest EnteredOn older than 2 days),
        /// LowStockSites (WAWA &lt; 50 OR MAMTA &lt; 50), StockByProvince, LastUpdated.
        /// </summary>
        Task<FoodStockSummaryDto> GetSummaryStatsAsync(string username);

        /// <summary>
        /// Returns the latest stock row per active site (role-scoped),
        /// with optional search text and location filters, for export.
        /// </summary>
        Task<List<FoodStockWithSiteDto>> GetExportDataAsync(
            string username,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null);

        /// <summary>
        /// Serializes the supplied data list to the requested file format.
        /// Does not touch the database, so no scoping is needed here.
        /// </summary>
        Task<byte[]> GenerateExportFileAsync(List<FoodStockWithSiteDto> data, string format);

        /// <summary>
        /// Returns the number of stock rows for a site (site-scoped by the caller).
        /// </summary>
        Task<int> GetLatestStockCountBySiteAsync(string siteId);

        /// <summary>
        /// Returns the latest stock row per active site (role-scoped) that matches
        /// the search term.
        /// </summary>
        Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(
            string username,
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Returns the single stock row for a specific site and date.
        /// Caller is responsible for authorizing the site.
        /// </summary>
        Task<NthSnfStock> GetBySiteIdAndDateAsync(string siteId, string enteredOn);

        /// <summary>
        /// Returns all stock rows for a specific site, newest first.
        /// Caller is responsible for authorizing the site.
        /// </summary>
        Task<List<NthSnfStock>> GetBySiteIdAsync(string siteId);

        /// <summary>
        /// Returns the distinct existing EnteredOn dates (dd-MM-yyyy) for a site,
        /// sorted newest first.
        /// </summary>
        Task<List<string>> GetExistingStockDatesAsync(string siteId);
    }
}
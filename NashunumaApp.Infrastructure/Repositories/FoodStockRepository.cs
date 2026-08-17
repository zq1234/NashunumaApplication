// Infrastructure/Repositories/FoodStockRepository.cs
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class FoodStockRepository : GenericRepository<NthSnfStock>, IFoodStockRepository
    {
        private readonly ApplicationDbContext _context;

        public FoodStockRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null)
        {
            try
            {
                var latestStockIds = _context.Foodstock
                    .GroupBy(x => x.SiteId)
                    .Select(g => g.Max(x => x.Id));

                // Build query with join first
                var baseQuery = from stock in _context.Foodstock
                                join site in _context.NthSiteLocations
                                    on stock.SiteId equals site.Id.ToString()
                                where site.IsActive == 1
                                    && latestStockIds.Contains(stock.Id)
                                select new { stock, site };

                // Apply simple search on stock/site fields
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    baseQuery = baseQuery.Where(x =>
                        (x.stock.SiteId != null && x.stock.SiteId.ToLower().Contains(searchTerm)) ||
                        (x.stock.Remarks != null && x.stock.Remarks.ToLower().Contains(searchTerm)) ||
                        (x.stock.EnteredBy != null && x.stock.EnteredBy.ToLower().Contains(searchTerm)) ||
                        (x.site.SiteName != null && x.site.SiteName.ToLower().Contains(searchTerm)) ||
                        (x.site.Address != null && x.site.Address.ToLower().Contains(searchTerm)) ||
                        (x.site.Province != null && x.site.Province.ToLower().Contains(searchTerm)) ||
                        (x.site.District != null && x.site.District.ToLower().Contains(searchTerm)) ||
                        (x.site.Tehsil != null && x.site.Tehsil.ToLower().Contains(searchTerm)) ||
                        (x.site.HeadName != null && x.site.HeadName.ToLower().Contains(searchTerm)) ||
                        (x.site.Contact != null && x.site.Contact.ToLower().Contains(searchTerm))
                    );
                }

                // Apply location filters
                if (!string.IsNullOrWhiteSpace(province))
                {
                    baseQuery = baseQuery.Where(x => x.site.Province == province);
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    baseQuery = baseQuery.Where(x => x.site.District == district);
                }

                if (!string.IsNullOrWhiteSpace(tehsil))
                {
                    baseQuery = baseQuery.Where(x => x.site.Tehsil == tehsil);
                }

                var totalCount = await baseQuery.CountAsync();

                // Project to DTO
                var items = await baseQuery
                    .OrderByDescending(x => x.stock.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new FoodStockWithSiteDto
                    {
                        // Stock fields
                        Id = x.stock.Id,
                        OpeningStockBoxesMamta = x.stock.OpeningStockBoxesMamta,
                        ReceivedStockBoxesMamta = x.stock.ReceivedStockBoxesMamta,
                        DistributedBoxesMamta = x.stock.DistributedBoxesMamta,
                        ClosingStockBoxesMamta = x.stock.ClosingStockBoxesMamta,
                        Remarks = x.stock.Remarks,
                        EnteredBy = x.stock.EnteredBy,
                        EnteredOn = x.stock.EnteredOn,
                        SiteId = x.stock.SiteId,
                        OpeningStockSachetsMamta = x.stock.OpeningStockSachetsMamta,
                        ClosingStockSachetsMamta = x.stock.ClosingStockSachetsMamta,
                        DistributedSachetsMamta = x.stock.DistributedSachetsMamta,
                        OpeningStockSachetsWawa = x.stock.OpeningStockSachetsWawa,
                        ClosingStockSachetsWawa = x.stock.ClosingStockSachetsWawa,
                        DistributedSachetsWawa = x.stock.DistributedSachetsWawa,
                        OpeningStockBoxesWawa = x.stock.OpeningStockBoxesWawa,
                        ReceivedStockBoxesWawa = x.stock.ReceivedStockBoxesWawa,
                        DistributedBoxesWawa = x.stock.DistributedBoxesWawa,
                        ClosingStockBoxesWawa = x.stock.ClosingStockBoxesWawa,
                        Unit = x.stock.Unit,
                        RutfReceived = x.stock.RutfReceived,
                        RutfOpening = x.stock.RutfOpening,
                        RutfDistributed = x.stock.RutfDistributed,
                        RutfClosing = x.stock.RutfClosing,
                        IfaReceived = x.stock.IfaReceived,
                        IfaOpening = x.stock.IfaOpening,
                        IfaDistributed = x.stock.IfaDistributed,
                        IfaClosing = x.stock.IfaClosing,
                        ActivityTime = x.stock.ActivityTime,
                        IsManualUpdate = x.stock.IsManualUpdate,
                        MmsReceived = x.stock.MmsReceived,
                        MmsOpening = x.stock.MmsOpening,
                        MmsDistributed = x.stock.MmsDistributed,
                        MmsClosing = x.stock.MmsClosing,

                        // Site Location fields
                        SiteName = x.site != null ? x.site.SiteName : null,
                        Address = x.site != null ? x.site.Address : null,
                        Contact = x.site != null ? x.site.Contact : null,
                        GeoLocation = x.site != null ? x.site.GeoLocation : null,
                        Province = x.site != null ? x.site.Province : null,
                        District = x.site != null ? x.site.District : null,
                        Tehsil = x.site != null ? x.site.Tehsil : null,
                        HeadName = x.site != null ? x.site.HeadName : null,
                        IsClosed = x.site != null ? x.site.IsClosed : null,
                        IsMobileSite = x.site != null ? x.site.IsMobileSite : null,
                        ProvinceNew = x.site != null ? x.site.ProvinceNew : null,
                        DistrictNew = x.site != null ? x.site.DistrictNew : null,
                        TehsilNew = x.site != null ? x.site.TehsilNew : null,
                        Latitude = x.site != null ? x.site.Latitude : null,
                        Longitude = x.site != null ? x.site.Longitude : null
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving food stocks with site details: {ex.Message}", ex);
            }
        }

        public async Task<FoodStockSummaryDto> GetSummaryStatsAsync()
        {
            try
            {
                // Get total sites
                var totalSites = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1)
                    .CountAsync();

                // Get all sites
                var allSites = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1)
                    .Select(s => s.Id)
                    .ToListAsync();

                // Get all food stocks (with date parsing in memory)
                var stocks = await _context.Foodstock
                    .Where(f => f.EnteredOn != null)
                    .Select(f => new { f.SiteId, f.EnteredOn, f.ClosingStockBoxesMamta })
                    .ToListAsync();

                // Process in memory using LINQ
                var sevenDaysAgo = DateTime.Now.AddDays(-7);

                // Get site IDs that were updated in last 7 days
                var updatedSiteIds = stocks
                    .Where(s => DateTime.TryParse(s.EnteredOn, out var date) && date >= sevenDaysAgo)
                    .Select(s => s.SiteId)
                    .Distinct()
                    .ToList();

                // Sites not updated
                var notUpdated = allSites.Count(s => !updatedSiteIds.Contains(s.ToString()));

                // Low stock sites
                var lowStockSites = stocks
                    .Where(s => s.ClosingStockBoxesMamta != null &&
                               int.TryParse(s.ClosingStockBoxesMamta, out var closing) && closing < 10)
                    .Select(s => s.SiteId)
                    .Distinct()
                    .Count();

                // Get stock by province (join with sites)
                var sitesWithProvince = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1)
                    .Select(s => new { s.Id, s.Province })
                    .ToListAsync();

                var stockByProvince = sitesWithProvince
                    .Join(stocks,
                        site => site.Id.ToString(),
                        stock => stock.SiteId,
                        (site, stock) => site.Province ?? "Unknown")
                    .GroupBy(p => p)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Get last updated time
                var lastUpdatedString = stocks
                    .OrderByDescending(s => s.EnteredOn)
                    .Select(s => s.EnteredOn)
                    .FirstOrDefault();

                DateTime? lastUpdated = null;
                if (lastUpdatedString != null && DateTime.TryParse(lastUpdatedString, out var parsed))
                {
                    lastUpdated = parsed;
                }

                return new FoodStockSummaryDto
                {
                    TotalSites = totalSites,
                    NotUpdated = notUpdated,
                    LowStockSites = lowStockSites,
                    StockByProvince = stockByProvince,
                    LastUpdated = lastUpdated
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving summary stats: {ex.Message}", ex);
            }
        }

        public async Task<List<FoodStockWithSiteDto>> GetExportDataAsync(
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null)
        {
            try
            {
                var query = from stock in _context.Foodstock
                            join site in _context.NthSiteLocations
                                on stock.SiteId equals site.Id.ToString()
                            where site.IsActive == 1
                            select new FoodStockWithSiteDto
                            {
                                Id = stock.Id,
                                SiteId = stock.SiteId,
                                SiteName = site.SiteName,
                                Province = site.Province,
                                District = site.District,
                                Tehsil = site.Tehsil,
                                Address = site.Address,
                                Contact = site.Contact,
                                HeadName = site.HeadName,
                                IsMobileSite = site.IsMobileSite,
                                OpeningStockBoxesMamta = stock.OpeningStockBoxesMamta,
                                ReceivedStockBoxesMamta = stock.ReceivedStockBoxesMamta,
                                DistributedBoxesMamta = stock.DistributedBoxesMamta,
                                ClosingStockBoxesMamta = stock.ClosingStockBoxesMamta,
                                OpeningStockBoxesWawa = stock.OpeningStockBoxesWawa,
                                ReceivedStockBoxesWawa = stock.ReceivedStockBoxesWawa,
                                DistributedBoxesWawa = stock.DistributedBoxesWawa,
                                ClosingStockBoxesWawa = stock.ClosingStockBoxesWawa,
                                RutfOpening = stock.RutfOpening,
                                RutfReceived = stock.RutfReceived,
                                RutfDistributed = stock.RutfDistributed,
                                RutfClosing = stock.RutfClosing,
                                EnteredBy = stock.EnteredBy,
                                EnteredOn = stock.EnteredOn,
                                ActivityTime = stock.ActivityTime,
                                Remarks = stock.Remarks
                            };

                // Apply simple search on export data
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(x =>
                        (x.SiteName != null && x.SiteName.ToLower().Contains(searchTerm)) ||
                        (x.Province != null && x.Province.ToLower().Contains(searchTerm)) ||
                        (x.District != null && x.District.ToLower().Contains(searchTerm)) ||
                        (x.Tehsil != null && x.Tehsil.ToLower().Contains(searchTerm)) ||
                        (x.Address != null && x.Address.ToLower().Contains(searchTerm)) ||
                        (x.HeadName != null && x.HeadName.ToLower().Contains(searchTerm)) ||
                        (x.Contact != null && x.Contact.ToLower().Contains(searchTerm)) ||
                        (x.Remarks != null && x.Remarks.ToLower().Contains(searchTerm))
                    );
                }

                // Apply location filters
                if (!string.IsNullOrWhiteSpace(province))
                {
                    query = query.Where(x => x.Province == province);
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    query = query.Where(x => x.District == district);
                }

                if (!string.IsNullOrWhiteSpace(tehsil))
                {
                    query = query.Where(x => x.Tehsil == tehsil);
                }

                return await query.OrderByDescending(x => x.EnteredOn).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving export data: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GenerateExportFileAsync(List<FoodStockWithSiteDto> data, string format)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating export file: {ex.Message}", ex);
            }
        }

        public async Task<int> GetLatestStockCountBySiteAsync(string siteId)
        {
            try
            {
                var count = await _context.Foodstock
                    .Where(f => f.SiteId == siteId)
                    .CountAsync();
                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting latest stock count: {ex.Message}", ex);
            }
        }

        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var query = from stock in _context.Foodstock
                            join site in _context.NthSiteLocations
                                on stock.SiteId equals site.Id.ToString()
                            where site.IsActive == 1
                            select new FoodStockWithSiteDto
                            {
                                Id = stock.Id,
                                SiteId = stock.SiteId,
                                SiteName = site.SiteName,
                                Province = site.Province,
                                District = site.District,
                                Tehsil = site.Tehsil,
                                Address = site.Address,
                                Contact = site.Contact,
                                HeadName = site.HeadName,
                                IsMobileSite = site.IsMobileSite,
                                OpeningStockBoxesMamta = stock.OpeningStockBoxesMamta,
                                ReceivedStockBoxesMamta = stock.ReceivedStockBoxesMamta,
                                DistributedBoxesMamta = stock.DistributedBoxesMamta,
                                ClosingStockBoxesMamta = stock.ClosingStockBoxesMamta,
                                OpeningStockBoxesWawa = stock.OpeningStockBoxesWawa,
                                ReceivedStockBoxesWawa = stock.ReceivedStockBoxesWawa,
                                DistributedBoxesWawa = stock.DistributedBoxesWawa,
                                ClosingStockBoxesWawa = stock.ClosingStockBoxesWawa,
                                RutfOpening = stock.RutfOpening,
                                RutfReceived = stock.RutfReceived,
                                RutfDistributed = stock.RutfDistributed,
                                RutfClosing = stock.RutfClosing,
                                EnteredBy = stock.EnteredBy,
                                EnteredOn = stock.EnteredOn,
                                ActivityTime = stock.ActivityTime,
                                Remarks = stock.Remarks
                            };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(x =>
                        (x.SiteName != null && x.SiteName.ToLower().Contains(searchTerm)) ||
                        (x.Province != null && x.Province.ToLower().Contains(searchTerm)) ||
                        (x.District != null && x.District.ToLower().Contains(searchTerm)) ||
                        (x.Tehsil != null && x.Tehsil.ToLower().Contains(searchTerm)) ||
                        (x.Address != null && x.Address.ToLower().Contains(searchTerm)) ||
                        (x.HeadName != null && x.HeadName.ToLower().Contains(searchTerm)) ||
                        (x.Contact != null && x.Contact.ToLower().Contains(searchTerm)) ||
                        (x.Remarks != null && x.Remarks.ToLower().Contains(searchTerm))
                    );
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.EnteredOn)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching food stocks: {ex.Message}", ex);
            }
        }
        
        /// Gets stock by site ID and date
        /// </summary>
        public async Task<NthSnfStock> GetBySiteIdAndDateAsync(string siteId, string enteredOn)
        {
            try
            {
                return await _context.Foodstock
                    .Where(f => f.SiteId == siteId && f.EnteredOn == enteredOn)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving stock by site and date: {ex.Message}", ex);
            }
        }

        
        /// Gets all stock records for a specific site
        /// </summary>
        public async Task<List<NthSnfStock>> GetBySiteIdAsync(string siteId)
        {
            try
            {
                return await _context.Foodstock
                    .Where(f => f.SiteId == siteId)
                    .OrderByDescending(f => f.EnteredOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving stocks by site: {ex.Message}", ex);
            }
        }

        /// Gets all existing stock dates for a specific site

        public async Task<List<string>> GetExistingStockDatesAsync(string siteId)
        {
            try
            {
                return await _context.Foodstock
                    .Where(f => f.SiteId == siteId && f.EnteredOn != null)
                    .Select(f => f.EnteredOn)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving existing stock dates: {ex.Message}", ex);
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NashunumaApp.Infrastructure.Repositories
{
    
    ///   - Admin      -> all active sites
    ///   - Non-admin  -> only NthUsers.SiteId
   
    public class FoodStockRepository : GenericRepository<NthSnfStock>, IFoodStockRepository
    {
        private readonly ApplicationDbContext _context;

        public FoodStockRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        
        /// Returns null if the user does not exist.
       
        private async Task<UserScope?> ResolveUserScopeAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));
            var userWithSite = await (
                from u in _context.NthUsers
                join s in _context.NthSiteLocations on u.SiteId equals s.Id
                where u.Username == username
                select new
                {
                    u.Userid,
                    u.Username,
                    u.Isadmin,         
                    SiteId = s.Id,
                    u.Isactive,
                    s.Province
                }
            ).FirstOrDefaultAsync();

            if (userWithSite == null)

                return null;

            // Only "True" (case-insensitive, trimmed) is treated as admin.
            // Anything else — "False", null, empty, malformed — is non-admin (fail-safe).
            var adminFlag = userWithSite.Isadmin?.Trim();
            bool isAdmin = adminFlag == "True" || adminFlag == "true";

            // Load the active sites the user is allowed to see.
            var activeSites = await _context.NthSiteLocations
                .Where(s => s.IsActive == 1)
                .Where(s => isAdmin || s.Id == userWithSite.SiteId)
                .Select(s => new SiteRef { Id = s.Id, Province = s.Province })
                .ToListAsync();

            return new UserScope
            {
                IsAdmin = isAdmin,
                UserSiteId = userWithSite.SiteId,
                ActiveSites = activeSites,
                SiteIdSet = activeSites
                                .Select(s => s.Id.ToString())
                                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            };
        }
        /// Returns an all-zero summary DTO for users that have no active sites.
        private static FoodStockSummaryDto EmptySummary() => new FoodStockSummaryDto
        {
            TotalSites = 0,
            NotUpdated = 0,
            LowStockSites = 0,
            StockByProvince = new Dictionary<string, int>(),
            LastUpdated = null
        };

        // Internal helper types
        private sealed class UserScope
        {
            public bool IsAdmin { get; set; }
            public decimal UserSiteId { get; set; }
            public List<SiteRef> ActiveSites { get; set; } = new();
            public HashSet<string> SiteIdSet { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class SiteRef
        {
            public decimal Id { get; set; }
            public string? Province { get; set; }
        }
        /// Returns the latest stock row per active site the user can see,
        /// with optional search text and location filters.
        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(
            string username,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null,
            string? province = null,
            string? district = null,
            string? tehsil = null)
        {
            try
            {
                // Resolve the user's allowed site set.
                var scope = await ResolveUserScopeAsync(username);
                if (scope == null || scope.ActiveSites.Count == 0)
                    return (new List<FoodStockWithSiteDto>(), 0);

                // Latest Foodstock.Id per SiteId; used to filter down to one row per site.
                var latestStockIds = _context.Foodstock
                    .GroupBy(x => x.SiteId)
                    .Select(g => g.Max(x => x.Id));

                // Base query: active site + belongs to user's scope + is the latest row.
                var baseQuery = from stock in _context.Foodstock
                                join site in _context.NthSiteLocations
                                    on stock.SiteId equals site.Id.ToString()
                                where site.IsActive == 1
                                      && scope.SiteIdSet.Contains(stock.SiteId)
                                      && latestStockIds.Contains(stock.Id)
                                select new { stock, site };

                // Free-text search across stock + site fields.
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

                // Explicit site filter (either stock.SiteId or site.Id may match).
                if (!string.IsNullOrWhiteSpace(siteId))
                {
                    baseQuery = baseQuery.Where(x =>
                        x.stock.SiteId == siteId || x.site.Id.ToString() == siteId);
                }

                // Location filters.
                if (!string.IsNullOrWhiteSpace(province))
                    baseQuery = baseQuery.Where(x => x.site.Province == province);

                if (!string.IsNullOrWhiteSpace(district))
                    baseQuery = baseQuery.Where(x => x.site.District == district);

                if (!string.IsNullOrWhiteSpace(tehsil))
                    baseQuery = baseQuery.Where(x => x.site.Tehsil == tehsil);

                // Total count for pagination.
                var totalCount = await baseQuery.CountAsync();

                // Materialise the page, projecting to the DTO.
                var items = await baseQuery
                    .OrderByDescending(x => x.stock.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new FoodStockWithSiteDto
                    {
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

                        SiteName = x.site.SiteName,
                        Address = x.site.Address,
                        Contact = x.site.Contact,
                        GeoLocation = x.site.GeoLocation,
                        Province = x.site.Province,
                        District = x.site.District,
                        Tehsil = x.site.Tehsil,
                        HeadName = x.site.HeadName,
                        IsClosed = x.site.IsClosed,
                        IsMobileSite = x.site.IsMobileSite,
                        ProvinceNew = x.site.ProvinceNew,
                        DistrictNew = x.site.DistrictNew,
                        TehsilNew = x.site.TehsilNew,
                        Latitude = x.site.Latitude,
                        Longitude = x.site.Longitude
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving food stocks with site details: {ex.Message}", ex);
            }
        }
        
        ///  - TotalSites     : number of active sites in scope
        ///  - NotUpdated     : sites whose latest EnteredOn is &gt; 2 days old
        ///  - LowStockSites  : sites with WAWA &lt; 50 OR MAMTA &lt; 50 on their latest row
        ///  - StockByProvince: latest-row count grouped by province
        ///  - LastUpdated    : most recent EnteredOn in scope
        public async Task<FoodStockSummaryDto> GetSummaryStatsAsync(string username)
        {
            try
            {
                var scope = await ResolveUserScopeAsync(username);
                if (scope == null || scope.ActiveSites.Count == 0)
                    return EmptySummary();

                var totalSites = scope.ActiveSites.Count;
                var siteIds = scope.SiteIdSet;

                // Load only the fields needed for the summary, for scoped sites.
                var stocks = await _context.Foodstock
                    .Where(f => f.EnteredOn != null
                                && f.SiteId != null
                                && siteIds.Contains(f.SiteId))
                    .Select(f => new
                    {
                        f.SiteId,
                        f.EnteredOn,
                        f.ClosingStockBoxesMamta,
                        f.ClosingStockBoxesWawa
                    })
                    .ToListAsync();

                // Parse dd-MM-yyyy into DateTime? and drop unparseable rows.
                var parsedStocks = stocks
                    .Select(s => new
                    {
                        s.SiteId,
                        s.ClosingStockBoxesMamta,
                        s.ClosingStockBoxesWawa,
                        EnteredOnDate = DateTime.TryParseExact(
                                            s.EnteredOn, "dd-MM-yyyy", null,
                                            DateTimeStyles.None, out var dt)
                                        ? dt
                                        : (DateTime?)null
                    })
                    .Where(s => s.EnteredOnDate.HasValue && !string.IsNullOrEmpty(s.SiteId))
                    .ToList();

                // Keep only the latest row per site.
                var latestPerSite = parsedStocks
                    .GroupBy(s => s.SiteId!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(s => s.EnteredOnDate!.Value).First())
                    .ToList();

                // Map siteId -> latest EnteredOn for O(1) lookups.
                var now = DateTime.Now;
                var latestBySite = latestPerSite
                    .ToDictionary(s => s.SiteId!, s => s.EnteredOnDate!.Value,
                                  StringComparer.OrdinalIgnoreCase);

                // Not Updated: no stock row at all OR latest row older than 2 days.
                var notUpdated = siteIds.Count(id =>
                {
                    if (!latestBySite.TryGetValue(id, out var enteredOn))
                        return true;
                    var daysPassed = Math.Abs((int)Math.Floor((enteredOn - now).TotalDays));
                    return daysPassed > 2;
                });

                // Low stock: WAWA < 50 OR MAMTA < 50 on the latest row.
                var lowStockSites = latestPerSite.Count(s =>
                    (int.TryParse(s.ClosingStockBoxesWawa, out var wawa) && wawa < 50) ||
                    (int.TryParse(s.ClosingStockBoxesMamta, out var mamta) && mamta < 50));

                // Group latest rows by province.
                var stockByProvince = scope.ActiveSites
                    .Join(latestPerSite,
                        site => site.Id.ToString(),
                        stock => stock.SiteId!,
                        (site, stock) => site.Province ?? "Unknown",
                        StringComparer.OrdinalIgnoreCase)
                    .GroupBy(p => p)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Latest EnteredOn across all scoped sites.
                var lastUpdated = latestPerSite
                    .OrderByDescending(s => s.EnteredOnDate!.Value)
                    .Select(s => (DateTime?)s.EnteredOnDate!.Value)
                    .FirstOrDefault();

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

        // Export

        /// Returns the latest stock row per scoped active site, for export.
        public async Task<List<FoodStockWithSiteDto>> GetExportDataAsync(
            string username,
            string? searchTerm = null,
            string? province = null,
            string? district = null,
            string? tehsil = null)
        {
            try
            {
                var scope = await ResolveUserScopeAsync(username);
                if (scope == null || scope.ActiveSites.Count == 0)
                    return new List<FoodStockWithSiteDto>();

                var latestStockIds = _context.Foodstock
                    .GroupBy(x => x.SiteId)
                    .Select(g => g.Max(x => x.Id));

                // Latest row per scoped active site, projected to the export DTO.
                var query = from stock in _context.Foodstock
                            join site in _context.NthSiteLocations
                                on stock.SiteId equals site.Id.ToString()
                            where site.IsActive == 1
                                  && scope.SiteIdSet.Contains(stock.SiteId)
                                  && latestStockIds.Contains(stock.Id)
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

                // Optional search text.
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

                // Optional location filters.
                if (!string.IsNullOrWhiteSpace(province))
                    query = query.Where(x => x.Province == province);

                if (!string.IsNullOrWhiteSpace(district))
                    query = query.Where(x => x.District == district);

                if (!string.IsNullOrWhiteSpace(tehsil))
                    query = query.Where(x => x.Tehsil == tehsil);

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

                return await Task.FromResult(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating export file: {ex.Message}", ex);
            }
        }

        /// Returns the number of stock rows for a given site.
        public async Task<int> GetLatestStockCountBySiteAsync(string siteId)
        {
            try
            {
                return await _context.Foodstock
                    .Where(f => f.SiteId == siteId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting latest stock count: {ex.Message}", ex);
            }
        }

        // Search
        /// Free-text search over the latest row per scoped active site.
        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(
            string username,
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var scope = await ResolveUserScopeAsync(username);
                if (scope == null || scope.ActiveSites.Count == 0)
                    return (new List<FoodStockWithSiteDto>(), 0);

                var latestStockIds = _context.Foodstock
                    .GroupBy(x => x.SiteId)
                    .Select(g => g.Max(x => x.Id));

                var query = from stock in _context.Foodstock
                            join site in _context.NthSiteLocations
                                on stock.SiteId equals site.Id.ToString()
                            where site.IsActive == 1
                                  && scope.SiteIdSet.Contains(stock.SiteId)
                                  && latestStockIds.Contains(stock.Id)
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

        // Site-scoped lookups (caller must authorize the site)

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

        public async Task<List<string>> GetExistingStockDatesAsync(string siteId)
        {
            try
            {
                var dates = await _context.Foodstock
                    .Where(f => f.SiteId == siteId && f.EnteredOn != null)
                    .Select(f => f.EnteredOn)
                    .Distinct()
                    .ToListAsync();

                // dd-MM-yyyy does NOT sort correctly as a string in SQL, so we
                // parse in memory and order by the real DateTime.
                return dates
                    .Select(d => new
                    {
                        Raw = d,
                        Parsed = DateTime.TryParseExact(d, "dd-MM-yyyy", null,
                                    DateTimeStyles.None, out var dt) ? dt : (DateTime?)null
                    })
                    .Where(x => x.Parsed.HasValue)
                    .OrderByDescending(x => x.Parsed.Value)
                    .Select(x => x.Raw)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving existing stock dates: {ex.Message}", ex);
            }
        }
    }
}
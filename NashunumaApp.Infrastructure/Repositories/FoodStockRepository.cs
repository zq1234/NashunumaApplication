using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;

namespace NashunumaApp.Infrastructure.Repositories
{
    /// <summary>
    /// FoodStock repository.
    ///   - Admin     -> NO scope filter: every row across every site (still IsActive).
    ///   - Non-admin -> only their assigned site AND only rows they entered.
    /// Caller filters (search/site/province/date/user/etc.) are applied on top.
    /// </summary>
    public class FoodStockRepository : GenericRepository<NthSnfStock>, IFoodStockRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserScopeService _scopeService;

        public FoodStockRepository(ApplicationDbContext context, IUserScopeService scopeService)
            : base(context)
        {
            _context = context;
            _scopeService = scopeService;
        }

        private static FoodStockSummaryDto EmptySummary() => new FoodStockSummaryDto
        {
            TotalSites = 0,
            NotUpdated = 0,
            LowStockSites = 0,
            StockByProvince = new Dictionary<string, int>(),
            LastUpdated = null
        };

        // ------------------------------------------------------------------
        // Shared query builder — apply role + caller filters once
        // ------------------------------------------------------------------
        private IQueryable<FoodStockQueryRow> BuildBaseQuery(UserScope scope)
        {
            // Admin: no SiteIdSet / EnteredBy restriction. Every active row.
            // Non-admin: only own site + only own rows.
            var baseQuery = from stock in _context.Foodstock
                            join site in _context.NthSiteLocations
                                on stock.SiteId equals site.Id.ToString()
                            where site.IsActive == 1
                                  && (scope.IsAdmin
                                      || (scope.SiteIdSet.Contains(stock.SiteId)
                                          && stock.EnteredBy == scope.Username))
                            select new FoodStockQueryRow { stock = stock, site = site };

            return baseQuery;
        }

        /// <summary>Anon type wrapper, so we can pass the joined row through helpers.</summary>
        private sealed class FoodStockQueryRow
        {
            public NthSnfStock stock { get; init; } = default!;
            public NthSiteLocation site { get; init; } = default!;
        }

        /// <summary>
        /// Applies all caller filters to the base query.
        /// Every clause is translatable to Oracle SQL (no client-eval).
        /// </summary>
        private static IQueryable<FoodStockQueryRow> ApplyFilters(
            IQueryable<FoodStockQueryRow> query,
            string? searchTerm,
            string? siteId,
            string? siteName,
            string? province,
            string? district,
            string? tehsil,
            string? date,
            string? dateFrom,
            string? dateTo,
            string? username,
            string? name,
            string? email)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.stock.SiteId != null && x.stock.SiteId.ToLower().Contains(s)) ||
                    (x.stock.Remarks != null && x.stock.Remarks.ToLower().Contains(s)) ||
                    (x.stock.EnteredBy != null && x.stock.EnteredBy.ToLower().Contains(s)) ||
                    (x.stock.EnteredOn != null && x.stock.EnteredOn.ToLower().Contains(s)) ||
                    (x.site.SiteName != null && x.site.SiteName.ToLower().Contains(s)) ||
                    (x.site.Address != null && x.site.Address.ToLower().Contains(s)) ||
                    (x.site.Province != null && x.site.Province.ToLower().Contains(s)) ||
                    (x.site.District != null && x.site.District.ToLower().Contains(s)) ||
                    (x.site.Tehsil != null && x.site.Tehsil.ToLower().Contains(s)) ||
                    (x.site.HeadName != null && x.site.HeadName.ToLower().Contains(s)) ||
                    (x.site.Contact != null && x.site.Contact.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(siteId))
            {
                var sid = siteId.Trim();
                query = query.Where(x => x.stock.SiteId == sid || x.site.Id.ToString() == sid);
            }

            if (!string.IsNullOrWhiteSpace(siteName))
            {
                var n = siteName.Trim().ToLower();
                query = query.Where(x => x.site.SiteName != null && x.site.SiteName.ToLower().Contains(n));
            }

            if (!string.IsNullOrWhiteSpace(province))
            {
                var p = province.Trim();
                query = query.Where(x => x.site.Province == p);
            }

            if (!string.IsNullOrWhiteSpace(district))
            {
                var d = district.Trim();
                query = query.Where(x => x.site.District == d);
            }

            if (!string.IsNullOrWhiteSpace(tehsil))
            {
                var t = tehsil.Trim();
                query = query.Where(x => x.site.Tehsil == t);
            }

            // Single date (dd-MM-yyyy) — exact match on EnteredOn.
            if (!string.IsNullOrWhiteSpace(date))
            {
                var d = date.Trim();
                query = query.Where(x => x.stock.EnteredOn == d);
            }

            // Inclusive date range — both strings must be dd-MM-yyyy
            // so lexical comparison is NOT used; convert via TO_DATE on the DB.
            if (!string.IsNullOrWhiteSpace(dateFrom))
            {
                var from = dateFrom.Trim();
                query = query.Where(x =>
                    x.stock.EnteredOn != null &&
                    string.Compare(x.stock.EnteredOn, from) >= 0);   // works because dd-MM-yyyy is fixed-width
            }

            if (!string.IsNullOrWhiteSpace(dateTo))
            {
                var to = dateTo.Trim();
                query = query.Where(x =>
                    x.stock.EnteredOn != null &&
                    string.Compare(x.stock.EnteredOn, to) <= 0);
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                var u = username.Trim();
                query = query.Where(x => x.stock.EnteredBy == u);
            }

            // "name" = site head name (exact or contains — here contains).
            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim().ToLower();
                query = query.Where(x => x.site.HeadName != null && x.site.HeadName.ToLower().Contains(n));
            }

            // "email" — if NthSiteLocations has an Email column, uncomment:
            // if (!string.IsNullOrWhiteSpace(email))
            // {
            //     var e = email.Trim().ToLower();
            //     query = query.Where(x => x.site.Email != null && x.site.Email.ToLower().Contains(e));
            // }

            return query;
        }

        // ------------------------------------------------------------------
        // Paged list
        // ------------------------------------------------------------------
        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> GetPagedFoodStocksWithSiteAsync(
            string username,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? siteId = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? siteName = null,
            string? date = null,
            string? dateFrom = null,
            string? dateTo = null,
            string? enteredBy = null,
            string? headName = null)
        {
            try
            {
                var scope = await _scopeService.ResolveAsync(username);
                if (scope == null)
                    return (new List<FoodStockWithSiteDto>(), 0);

                // Admin may have zero ActiveSites only if every site is inactive;
                // for admin we don't block on ActiveSites count.
                if (!scope.IsAdmin && scope.ActiveSites.Count == 0)
                    return (new List<FoodStockWithSiteDto>(), 0);

                var query = ApplyFilters(
                    BuildBaseQuery(scope),
                    searchTerm, siteId, siteName, province, district, tehsil,
                    date, dateFrom, dateTo, enteredBy, headName, email: null);

                var totalCount = await query.CountAsync();

                var items = await query
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

        // ------------------------------------------------------------------
        // Summary
        // ------------------------------------------------------------------
        public async Task<FoodStockSummaryDto> GetSummaryStatsAsync(string username)
        {
            try
            {
                var scope = await _scopeService.ResolveAsync(username);
                if (scope == null)
                    return EmptySummary();

                if (!scope.IsAdmin && scope.ActiveSites.Count == 0)
                    return EmptySummary();

                // For admin, "total sites" = all active sites.
                // For non-admin, it's just their own (scope.ActiveSites already restricts).
                var totalSites = scope.ActiveSites.Count;

                var stocksQuery = _context.Foodstock
                    .Where(f => f.EnteredOn != null
                                && f.SiteId != null
                                && (scope.IsAdmin
                                    || (scope.SiteIdSet.Contains(f.SiteId)
                                        && f.EnteredBy == scope.Username)));

                var stocks = await stocksQuery
                    .Select(f => new
                    {
                        f.SiteId,
                        f.EnteredOn,
                        f.ClosingStockBoxesMamta,
                        f.ClosingStockBoxesWawa
                    })
                    .ToListAsync();

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

                var latestPerSite = parsedStocks
                    .GroupBy(s => s.SiteId!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(s => s.EnteredOnDate!.Value).First())
                    .ToList();

                var now = DateTime.Now;
                var latestBySite = latestPerSite
                    .ToDictionary(s => s.SiteId!, s => s.EnteredOnDate!.Value,
                                  StringComparer.OrdinalIgnoreCase);

                // For admin, "not updated" is computed over the union of
                // active sites + sites present in the data.
                IEnumerable<string> siteIdsForNotUpdated = scope.IsAdmin
                    ? scope.SiteIdSet.Union(latestPerSite.Select(s => s.SiteId!), StringComparer.OrdinalIgnoreCase)
                    : scope.SiteIdSet;

                var notUpdated = siteIdsForNotUpdated.Count(id =>
                {
                    if (!latestBySite.TryGetValue(id, out var enteredOn))
                        return true;
                    var daysPassed = Math.Abs((int)Math.Floor((enteredOn - now).TotalDays));
                    return daysPassed > 2;
                });

                var lowStockSites = latestPerSite.Count(s =>
                    (int.TryParse(s.ClosingStockBoxesWawa, out var wawa) && wawa < 50) ||
                    (int.TryParse(s.ClosingStockBoxesMamta, out var mamta) && mamta < 50));

                var stockByProvince = scope.ActiveSites
                    .Join(latestPerSite,
                        site => site.Id.ToString(),
                        stock => stock.SiteId!,
                        (site, stock) => site.Province ?? "Unknown",
                        StringComparer.OrdinalIgnoreCase)
                    .GroupBy(p => p)
                    .ToDictionary(g => g.Key, g => g.Count());

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

        // ------------------------------------------------------------------
        // Export — full filter set; all matching rows (no latest-per-site)
        // ------------------------------------------------------------------
        public async Task<List<FoodStockWithSiteDto>> GetExportDataAsync(
            string username,
            string? searchTerm = null,
            string? siteId = null,
            string? siteName = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? date = null,
            string? dateFrom = null,
            string? dateTo = null,
            string? enteredBy = null,
            string? headName = null)
        {
            try
            {
                var scope = await _scopeService.ResolveAsync(username);
                if (scope == null) return new List<FoodStockWithSiteDto>();
                if (!scope.IsAdmin && scope.ActiveSites.Count == 0)
                    return new List<FoodStockWithSiteDto>();

                var query = ApplyFilters(
                    BuildBaseQuery(scope),
                    searchTerm, siteId, siteName, province, district, tehsil,
                    date, dateFrom, dateTo, enteredBy, headName, email: null);

                return await query
                    .OrderByDescending(x => x.stock.EnteredOn)
                    .ThenByDescending(x => x.stock.Id)
                    .Select(x => new FoodStockWithSiteDto
                    {
                        Id = x.stock.Id,
                        SiteId = x.stock.SiteId,
                        SiteName = x.site.SiteName,
                        Province = x.site.Province,
                        District = x.site.District,
                        Tehsil = x.site.Tehsil,
                        Address = x.site.Address,
                        Contact = x.site.Contact,
                        HeadName = x.site.HeadName,
                        IsMobileSite = x.site.IsMobileSite,

                        OpeningStockBoxesMamta = x.stock.OpeningStockBoxesMamta,
                        ReceivedStockBoxesMamta = x.stock.ReceivedStockBoxesMamta,
                        DistributedBoxesMamta = x.stock.DistributedBoxesMamta,
                        ClosingStockBoxesMamta = x.stock.ClosingStockBoxesMamta,
                        OpeningStockBoxesWawa = x.stock.OpeningStockBoxesWawa,
                        ReceivedStockBoxesWawa = x.stock.ReceivedStockBoxesWawa,
                        DistributedBoxesWawa = x.stock.DistributedBoxesWawa,
                        ClosingStockBoxesWawa = x.stock.ClosingStockBoxesWawa,
                        RutfOpening = x.stock.RutfOpening,
                        RutfReceived = x.stock.RutfReceived,
                        RutfDistributed = x.stock.RutfDistributed,
                        RutfClosing = x.stock.RutfClosing,

                        EnteredBy = x.stock.EnteredBy,
                        EnteredOn = x.stock.EnteredOn,
                        ActivityTime = x.stock.ActivityTime,
                        Remarks = x.stock.Remarks
                    })
                    .ToListAsync();
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

        // ------------------------------------------------------------------
        // Counts
        // ------------------------------------------------------------------
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

        // ------------------------------------------------------------------
        // Search — full filter set
        // ------------------------------------------------------------------
        public async Task<(List<FoodStockWithSiteDto> Items, int TotalCount)> SearchFoodStocksAsync(
            string username,
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10,
            string? siteId = null,
            string? siteName = null,
            string? province = null,
            string? district = null,
            string? tehsil = null,
            string? date = null,
            string? dateFrom = null,
            string? dateTo = null,
            string? enteredBy = null,
            string? headName = null)
        {
            try
            {
                var scope = await _scopeService.ResolveAsync(username);
                if (scope == null) return (new List<FoodStockWithSiteDto>(), 0);
                if (!scope.IsAdmin && scope.ActiveSites.Count == 0)
                    return (new List<FoodStockWithSiteDto>(), 0);

                var query = ApplyFilters(
                    BuildBaseQuery(scope),
                    searchTerm, siteId, siteName, province, district, tehsil,
                    date, dateFrom, dateTo, enteredBy, headName, email: null);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.stock.EnteredOn)
                    .ThenByDescending(x => x.stock.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new FoodStockWithSiteDto
                    {
                        Id = x.stock.Id,
                        SiteId = x.stock.SiteId,
                        SiteName = x.site.SiteName,
                        Province = x.site.Province,
                        District = x.site.District,
                        Tehsil = x.site.Tehsil,
                        Address = x.site.Address,
                        Contact = x.site.Contact,
                        HeadName = x.site.HeadName,
                        IsMobileSite = x.site.IsMobileSite,

                        OpeningStockBoxesMamta = x.stock.OpeningStockBoxesMamta,
                        ReceivedStockBoxesMamta = x.stock.ReceivedStockBoxesMamta,
                        DistributedBoxesMamta = x.stock.DistributedBoxesMamta,
                        ClosingStockBoxesMamta = x.stock.ClosingStockBoxesMamta,
                        OpeningStockBoxesWawa = x.stock.OpeningStockBoxesWawa,
                        ReceivedStockBoxesWawa = x.stock.ReceivedStockBoxesWawa,
                        DistributedBoxesWawa = x.stock.DistributedBoxesWawa,
                        ClosingStockBoxesWawa = x.stock.ClosingStockBoxesWawa,
                        RutfOpening = x.stock.RutfOpening,
                        RutfReceived = x.stock.RutfReceived,
                        RutfDistributed = x.stock.RutfDistributed,
                        RutfClosing = x.stock.RutfClosing,
                        EnteredBy = x.stock.EnteredBy,
                        EnteredOn = x.stock.EnteredOn,
                        ActivityTime = x.stock.ActivityTime,
                        Remarks = x.stock.Remarks
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching food stocks: {ex.Message}", ex);
            }
        }

        // ------------------------------------------------------------------
        // Site-scoped lookups (caller must authorize the site)
        // ------------------------------------------------------------------
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
                    .OrderByDescending(f => f.Id)
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
// Infrastructure/Repositories/SiteLocationRepository.cs
using Microsoft.EntityFrameworkCore;

using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.TempOracleEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class SiteLocationRepository : GenericRepository<NthSiteLocation>, ISiteLocationRepository
    {
        private readonly ApplicationDbContext _context;
        public SiteLocationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // -------------------- CRUD --------------------

        public async Task<(List<SiteLocationDto> Items, int TotalCount)> GetAllAsync(
            int pageNumber, int pageSize, string? search, CancellationToken ct = default)
        {
            var query = _context.NthSiteLocations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.SiteName!.Contains(s) ||
                    x.Address!.Contains(s) ||
                    x.Tehsil!.Contains(s) ||
                    x.District!.Contains(s) ||
                    x.Province!.Contains(s));
            }

            var total = await query.CountAsync(ct);
            var items = await query
               .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => Map(x))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<SiteLocationDto?> GetDtoByIdAsync(decimal id, CancellationToken ct = default)
        {
            var entity = await _context.NthSiteLocations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            return entity == null ? null : Map(entity);
        }
        public async Task<bool> ExistsByNameAsync(string siteName, decimal? excludeId = null, CancellationToken ct = default)
        {
            var q = _context.NthSiteLocations.AsNoTracking()
                .Where(x => x.SiteName != null && x.SiteName.ToLower() == siteName.ToLower());

            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);

            return await q.AnyAsync(ct);
        }

        // -------------------- Cascading lookups --------------------

        public async Task<List<ProvinceDto>> GetProvincesAsync(CancellationToken ct = default)
        {
            return await _context.NdepLibProvinces.AsNoTracking()
                .Where(p => p.PROVINCE != null)
                .OrderBy(p => p.PROVINCE)
                .Select(p => new ProvinceDto { Id = p.PROVCODE , Province = p.PROVINCE })
                .ToListAsync(ct);
        }

        public async Task<List<DistrictDto>> GetDistrictsByProvinceAsync(decimal provinceId, CancellationToken ct = default)
        {
            return await _context.NdepLibDistricts.AsNoTracking()
                .Where(d => d.PROVCODE == provinceId && d.DISTRICT != null)
                .OrderBy(d => d.DISTRICT)
                .Select(d => new DistrictDto
                {
                    Id = d.DISTCODE ,
                    District = d.DISTRICT,
                    ProvinceId = d.PROVCODE
                })
                .ToListAsync(ct);
        }

        public async Task<List<TehsilDto>> GetTehsilsByDistrictAsync(decimal districtId, CancellationToken ct = default)
        {
            return await _context.NdepLibTehsils.AsNoTracking()
                .Where(t => t.DISTCODE == districtId && t.TEHSIL != null)
                .OrderBy(t => t.TEHSIL)
                .Select(t => new TehsilDto
                {
                    Id = t.TEHSILCODE ,
                    Tehsil = t.TEHSIL,
                    DistrictId = t.DISTCODE
                })
                .ToListAsync(ct);
        }

        public async Task<List<UcDto>> GetUcsByTehsilAsync(decimal tehsilId, CancellationToken ct = default)
        {
            return await _context.NdepLibUCs.AsNoTracking()
                .Where(u => u.TEHSILCODE == tehsilId && u.UC != null)
                .OrderBy(u => u.UC)
                .Select(u => new UcDto
                {
                    Id = u.UCCODE ,
                    Uc = u.UC,
                    TehsilId = u.TEHSILCODE
                }).ToListAsync(ct);
        }

        // -------------------- Names resolution --------------------

        public async Task<(string? Province, string? District, string? Tehsil, string? Uc)> ResolveNamesAsync(decimal provinceId, decimal districtId, decimal tehsilId, decimal? ucId, CancellationToken ct = default)
        {
            var province = await _context.NdepLibProvinces.AsNoTracking()
                .Where(p => p.PROVCODE == provinceId)
                .Select(p => p.PROVINCE).FirstOrDefaultAsync(ct);

            var district = await _context.NdepLibDistricts.AsNoTracking()
                .Where(d => d.DISTCODE == districtId && d.PROVCODE == provinceId)
                .Select(d => d.DISTRICT).FirstOrDefaultAsync(ct);

            var tehsil = await _context.NdepLibTehsils.AsNoTracking()
                .Where(t => t.TEHSILCODE == tehsilId && t.DISTCODE == districtId)
                .Select(t => t.TEHSIL).FirstOrDefaultAsync(ct);
            var uc = await _context.NdepLibUCs.AsNoTracking()
                .Where(t => t.UCCODE == ucId && t.TEHSILCODE == tehsilId)
                .Select(t => t.UC).FirstOrDefaultAsync(ct);

            return (province?.ToLower(), district?.ToLower(), tehsil?.ToLower(), uc?.ToLower());
        }

        //public async Task<(string? Province, string? District, string? Tehsil, string? Uc)> ResolveNamesAsync(decimal provinceId,decimal districtId, decimal tehsilId,decimal? ucId,string provinceName,
        // string districtName,string tehsilName, string? ucName, CancellationToken ct = default)
        //{
        //    var rows = await _context.NthGeoLocations.AsNoTracking()
        //        .Where(g => g.PId == provinceId &&
        //                    g.DId == districtId &&
        //                    g.TId == tehsilId &&
        //                    (ucId == null || g.UcId == ucId.Value))
        //        .Select(g => new { g.Province, g.District, g.Tehsil, g.Uc })
        //        .ToListAsync(ct);

        //    var row = rows.FirstOrDefault(r =>
        //        string.Equals(r.Province, provinceName, StringComparison.OrdinalIgnoreCase) &&
        //        string.Equals(r.District, districtName, StringComparison.OrdinalIgnoreCase) &&
        //        string.Equals(r.Tehsil, tehsilName, StringComparison.OrdinalIgnoreCase) &&
        //        (ucId == null || string.Equals(r.Uc, ucName, StringComparison.OrdinalIgnoreCase)));

        //    return (row?.Province?.ToLower(),row?.District?.ToLower(),row?.Tehsil?.ToLower(),row?.Uc?.ToLower()
        //    );
        //}
        // -------------------- Dropdown Lookups --------------------

        public async Task<List<SiteDropdownDto>> GetSitesByFiltersAsync(decimal? provinceId = null,decimal? districtId = null,decimal? tehsilId = null, CancellationToken ct = default)
        {
            var query = _context.NthSiteLocations.AsNoTracking();

            if (provinceId.HasValue)
                query = query.Where(s => s.ProvinceId == provinceId.Value);

            if (districtId.HasValue)
                query = query.Where(s => s.DistrictId == districtId.Value);

            if (tehsilId.HasValue)
                query = query.Where(s => s.TehsilId == tehsilId.Value);

            return await query
                .Where(s => s.SiteName != null)
                .OrderBy(s => s.SiteName)
                .Select(s => new SiteDropdownDto { Id = s.Id, SiteName = s.SiteName })
                .ToListAsync(ct);
        }

        // -------------------- Geolocation Data Creation --------------------

        public async Task CreateGeolocationDataIfNotExistsAsync(string currentUser, decimal provinceId, decimal districtId,decimal tehsilId,decimal? ucId,string provinceName,
                string districtName,string tehsilName,string? ucName, CancellationToken ct = default)
        {
            var exists = await _context.NthGeoLocations.AsNoTracking()
                .AnyAsync(g =>
                    g.PId == provinceId &&
                    g.DId == districtId &&
                    g.TId == tehsilId &&
                    (ucId == null || g.UcId == ucId.Value) &&
                    g.Province != null && g.Province.ToLower() == provinceName.ToLower() &&
                    g.District != null && g.District.ToLower() == districtName.ToLower() &&
                    g.Tehsil != null && g.Tehsil.ToLower() == tehsilName.ToLower() &&
                    (ucId == null || (g.Uc != null && g.Uc.ToLower() == ucName!.ToLower())),
                    ct);

            if (exists)
                return;

            var country = await _context.NthGeoLocations.AsNoTracking()
                .Where(g => g.PId == provinceId && g.Country != null)
                .Select(g => g.Country)
                .FirstOrDefaultAsync(ct);

            var entity = new NthGeoLocation
            {
                Country = country ?? "PAKISTAN",
                Province = provinceName,
                PId = provinceId,

                District = districtName,
                DId = districtId,

                Tehsil = tehsilName,
                TId = tehsilId,

                Uc = ucId.HasValue ? ucName : null,
                UcId = ucId ?? 0m,
                Village = null,
                VId = 0m,
                Remarks = null,
                EnteredOn = DateTime.UtcNow,
                EnteredBy = currentUser
            };

            _context.NthGeoLocations.Add(entity);
            await _context.SaveChangesAsync(ct);
        }
        // -------------------- Mapping --------------------

        private static SiteLocationDto Map(NthSiteLocation x) => new()
        {
            Id = x.Id,
            SiteName = x.SiteName,
            Address = x.Address,
            Contact = x.Contact,
            GeoLocation = x.GeoLocation,
            HeadName = x.HeadName,
            SiteTarget = x.SiteTarget,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            IsActive = x.IsActive == 1,
            ProvinceId = x.ProvinceId,
            Province = x.Province,
            DistrictId = x.DistrictId,
            District = x.District,
            TehsilId = x.TehsilId,
            Tehsil = x.Tehsil,
            CreatedBy = x.CreatedBy,
            CreatedOn = x.CreatedOn,
            UpdatedBy = x.UpdatedBy,
            UpdatedOn = x.UpdatedOn,

        };
    }
}
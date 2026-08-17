// Infrastructure/Repositories/LocationRepository.cs
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Infrastructure.Data;

namespace NashunumaApp.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProvinceDto>> GetAllProvinces()
        {
            try
            {
                // Get distinct provinces from NthSiteLocation table
                var provinces = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.Province != null)
                    .GroupBy(s => new { s.ProvinceId, s.Province })
                    .Select(g => new ProvinceDto
                    {
                        Id = g.Key.ProvinceId ?? 0,
                        Province = g.Key.Province
                    })
                    .OrderBy(p => p.Province)
                    .ToListAsync();

                return provinces;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving provinces: {ex.Message}", ex);
            }
        }

        public async Task<List<DistrictDto>> GetDistrictsByProvinceName(string provinceName)
        {
            try
            {
                if (string.IsNullOrEmpty(provinceName))
                {
                    return new List<DistrictDto>();
                }

                // Get distinct districts for the province
                var districts = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.Province == provinceName && s.District != null)
                    .GroupBy(s => new { s.DistrictId, s.District, s.ProvinceId })
                    .Select(g => new DistrictDto
                    {
                        Id = g.Key.DistrictId ?? 0,
                        District = g.Key.District,
                        ProvinceId = g.Key.ProvinceId
                    })
                    .OrderBy(d => d.District)
                    .ToListAsync();

                return districts;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving districts for province {provinceName}: {ex.Message}", ex);
            }
        }

        public async Task<List<TehsilDto>> GetTehsilsByDistrictName(string districtName)
        {
            try
            {
                if (string.IsNullOrEmpty(districtName))
                {
                    return new List<TehsilDto>();
                }

                // Get distinct tehsils for the district
                var tehsils = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.District == districtName && s.Tehsil != null)
                    .GroupBy(s => new { s.TehsilId, s.Tehsil, s.DistrictId })
                    .Select(g => new TehsilDto
                    {
                        Id = g.Key.TehsilId ?? 0,
                        Tehsil = g.Key.Tehsil,
                        DistrictId = g.Key.DistrictId
                    })
                    .OrderBy(t => t.Tehsil)
                    .ToListAsync();

                return tehsils;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tehsils for district {districtName}: {ex.Message}", ex);
            }
        }

        public async Task<List<UcDto>> GetUcsByTehsilName(string tehsilName)
        {
            try
            {
                // Note: NthSiteLocation doesn't have UC information.
                // If UC data is needed, you'll need to join with a UC table.
                // For now, returning empty list.
                return await Task.FromResult(new List<UcDto>());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving UCs for tehsil {tehsilName}: {ex.Message}", ex);
            }
        }

        public async Task<ProvinceDto> GetProvinceByName(string provinceName)
        {
            try
            {
                var province = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.Province == provinceName && s.Province != null)
                    .GroupBy(s => new { s.ProvinceId, s.Province })
                    .Select(g => new ProvinceDto
                    {
                        Id = g.Key.ProvinceId ?? 0,
                        Province = g.Key.Province
                    })
                    .FirstOrDefaultAsync();

                return province;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving province {provinceName}: {ex.Message}", ex);
            }
        }

        public async Task<DistrictDto> GetDistrictByName(string districtName)
        {
            try
            {
                var district = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.District == districtName && s.District != null)
                    .GroupBy(s => new { s.DistrictId, s.District, s.ProvinceId })
                    .Select(g => new DistrictDto
                    {
                        Id = g.Key.DistrictId ?? 0,
                        District = g.Key.District,
                        ProvinceId = g.Key.ProvinceId
                    })
                    .FirstOrDefaultAsync();

                return district;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving district {districtName}: {ex.Message}", ex);
            }
        }

        public async Task<TehsilDto> GetTehsilByName(string tehsilName)
        {
            try
            {
                var tehsil = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1 && s.Tehsil == tehsilName && s.Tehsil != null)
                    .GroupBy(s => new { s.TehsilId, s.Tehsil, s.DistrictId })
                    .Select(g => new TehsilDto
                    {
                        Id = g.Key.TehsilId ?? 0,
                        Tehsil = g.Key.Tehsil,
                        DistrictId = g.Key.DistrictId
                    })
                    .FirstOrDefaultAsync();

                return tehsil;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tehsil {tehsilName}: {ex.Message}", ex);
            }
        }

        
        /// Get complete location hierarchy: Provinces -> Districts -> Tehsils
        /// </summary>
        public async Task<LocationHierarchyDto> GetLocationHierarchy()
        {
            try
            {
                var locations = await _context.NthSiteLocations
                    .Where(s => s.IsActive == 1)
                    .ToListAsync();

                var hierarchy = new LocationHierarchyDto
                {
                    Provinces = new List<ProvinceWithDistrictsDto>()
                };

                // Group by Province
                var provinceGroups = locations
                    .Where(s => s.Province != null)
                    .GroupBy(s => new { s.ProvinceId, s.Province });

                foreach (var provinceGroup in provinceGroups)
                {
                    var provinceDto = new ProvinceWithDistrictsDto
                    {
                        Id = provinceGroup.Key.ProvinceId ?? 0,
                        Province = provinceGroup.Key.Province,
                        Districts = new List<DistrictWithTehsilsDto>()
                    };

                    // Group by District within Province
                    var districtGroups = provinceGroup
                        .Where(s => s.District != null)
                        .GroupBy(s => new { s.DistrictId, s.District });

                    foreach (var districtGroup in districtGroups)
                    {
                        var districtDto = new DistrictWithTehsilsDto
                        {
                            Id = districtGroup.Key.DistrictId ?? 0,
                            District = districtGroup.Key.District,
                            Tehsils = new List<TehsilWithCountDto>()
                        };

                        // Group by Tehsil within District
                        var tehsilGroups = districtGroup
                            .Where(s => s.Tehsil != null)
                            .GroupBy(s => new { s.TehsilId, s.Tehsil });

                        foreach (var tehsilGroup in tehsilGroups)
                        {
                            var tehsilDto = new TehsilWithCountDto
                            {
                                Id = tehsilGroup.Key.TehsilId ?? 0,
                                Tehsil = tehsilGroup.Key.Tehsil,
                                SiteCount = tehsilGroup.Count()
                            };

                            districtDto.Tehsils.Add(tehsilDto);
                        }

                        provinceDto.Districts.Add(districtDto);
                    }

                    hierarchy.Provinces.Add(provinceDto);
                }

                return hierarchy;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving location hierarchy: {ex.Message}", ex);
            }
        }
    }
}
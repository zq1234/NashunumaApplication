// Infrastructure/Repositories/LocationRepository.cs
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Entities;
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

        public async Task<List<NthProvince>> GetAllProvincesAsync()
        {
            try
            {
                return await _context.NthProvinces
                    .OrderBy(p => p.Province)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving provinces: {ex.Message}", ex);
            }
        }

        public async Task<List<NthDistrict>> GetDistrictsByProvinceCodeAsync(decimal provinceCode)
        {
            try
            {
                return await _context.NthDistricts
                    .Where(d => d.Provcode == provinceCode)
                    .OrderBy(d => d.District)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving districts for province code {provinceCode}: {ex.Message}", ex);
            }
        }

        public async Task<List<NthTehsil>> GetTehsilsByDistrictCodeAsync(decimal districtCode)
        {
            try
            {
                return await _context.NthTehsils
                    .Where(t => t.Distcode == districtCode)
                    .OrderBy(t => t.Tehsil)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tehsils for district code {districtCode}: {ex.Message}", ex);
            }
        }

        public async Task<List<NthUc>> GetUcsByTehsilCodeAsync(decimal tehsilCode)
        {
            try
            {
                return await _context.NthUcs
                    .Where(u => u.Tehsilcode == tehsilCode)
                    .OrderBy(u => u.Uc)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving UCs for tehsil code {tehsilCode}: {ex.Message}", ex);
            }
        }

        public async Task<NthProvince> GetProvinceByCodeAsync(decimal provinceCode)
        {
            try
            {
                return await _context.NthProvinces
                    .FirstOrDefaultAsync(p => p.Provcode == provinceCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving province with code {provinceCode}: {ex.Message}", ex);
            }
        }

        public async Task<NthDistrict> GetDistrictByCodeAsync(decimal districtCode)
        {
            try
            {
                return await _context.NthDistricts
                    .FirstOrDefaultAsync(d => d.Distcode == districtCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving district with code {districtCode}: {ex.Message}", ex);
            }
        }

        public async Task<NthTehsil> GetTehsilByCodeAsync(decimal tehsilCode)
        {
            try
            {
                return await _context.NthTehsils
                    .FirstOrDefaultAsync(t => t.Tehsilcode == tehsilCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tehsil with code {tehsilCode}: {ex.Message}", ex);
            }
        }
    }
}
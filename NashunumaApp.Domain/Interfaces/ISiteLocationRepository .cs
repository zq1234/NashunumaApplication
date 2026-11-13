// Domain/Interfaces/ISiteLocationRepository.cs

using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Domain.Interfaces
{
    public interface ISiteLocationRepository : IGenericRepository<NthSiteLocation>
    {
        // CRUD
        Task<(List<SiteLocationDto> Items, int TotalCount)> GetAllAsync( int pageNumber, int pageSize, string? search, CancellationToken ct = default);
        Task<SiteLocationDto?> GetDtoByIdAsync(decimal id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string siteName, decimal? excludeId = null, CancellationToken ct = default);

        // Cascading lookups (Oracle temp entities)
        Task<List<ProvinceDto>> GetProvincesAsync(CancellationToken ct = default);
        Task<List<DistrictDto>> GetDistrictsByProvinceAsync(decimal provinceId, CancellationToken ct = default);
        Task<List<TehsilDto>> GetTehsilsByDistrictAsync(decimal districtId, CancellationToken ct = default);
        Task<List<UcDto>> GetUcsByTehsilAsync(decimal tehsilId, CancellationToken ct = default);
        Task<(string? Province, string? District, string? Tehsil)> ResolveNamesAsync(decimal provinceId, decimal districtId, decimal tehsilId, CancellationToken ct = default);
    }
}
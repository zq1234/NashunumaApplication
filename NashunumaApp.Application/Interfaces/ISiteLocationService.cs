// Application/Interfaces/ISiteLocationService.cs
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.SiteLocation;   // Create/Update requests
using NashunumaApp.Domain.Common.DTOs;              // ApiResponse, PaginatedResponse, SiteLocationDto, ProvinceDto, ...
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Interfaces
{
    public interface ISiteLocationService
    {
        Task<ApiResponse<PaginatedResponse<SiteLocationDto>>> GetAllAsync(
            int pageNumber = 1, int pageSize = 10, string? search = null, CancellationToken ct = default);

        Task<ApiResponse<SiteLocationDto>> GetByIdAsync(decimal id, CancellationToken ct = default);

        Task<ApiResponse<SiteLocationDto>> CreateAsync(
            CreateSiteLocationRequest request, string currentUser, CancellationToken ct = default);

        Task<ApiResponse<SiteLocationDto>> UpdateAsync(
            decimal id, UpdateSiteLocationRequest request, string currentUser, CancellationToken ct = default);

        Task<ApiResponse<bool>> DeleteAsync(decimal id, string currentUser, CancellationToken ct = default);

        Task<ApiResponse<List<ProvinceDto>>> GetProvincesAsync(CancellationToken ct = default);
        Task<ApiResponse<List<DistrictDto>>> GetDistrictsAsync(decimal provinceId, CancellationToken ct = default);
        Task<ApiResponse<List<TehsilDto>>> GetTehsilsAsync(decimal districtId, CancellationToken ct = default);
        Task<ApiResponse<List<UcDto>>> GetUcsAsync(decimal tehsilId, CancellationToken ct = default);

        // Dropdown for sites by hierarchical filters
        Task<ApiResponse<List<SiteDropdownDto>>> GetSitesByFiltersAsync(
            decimal? provinceId = null,
            decimal? districtId = null,
            decimal? tehsilId = null,
            CancellationToken ct = default);
    }
}
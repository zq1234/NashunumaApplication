// Application/Services/SiteLocationService.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.SiteLocation;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NashunumaApp.Application.Services
{
    public class SiteLocationService : ISiteLocationService
    {
        private readonly ISiteLocationRepository _repository;
        private readonly IMapper _mapper;

        public SiteLocationService(ISiteLocationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PaginatedResponse<SiteLocationDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null, CancellationToken ct = default)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Max(1, Math.Min(100, pageSize));

                var (items, total) = await _repository.GetAllAsync(pageNumber, pageSize, search, ct);

                var result = new PaginatedResponse<SiteLocationDto>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = total
                };

                return ApiResponse<PaginatedResponse<SiteLocationDto>>
                    .Success(result, "Site locations retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<SiteLocationDto>>
                    .Failure($"Failed to retrieve site locations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SiteLocationDto>> GetByIdAsync(decimal id, CancellationToken ct = default)
        {
            try
            {
                var dto = await _repository.GetDtoByIdAsync(id, ct);
                if (dto == null)
                    return ApiResponse<SiteLocationDto>.Failure("Site location not found", null, 404);

                return ApiResponse<SiteLocationDto>.Success(dto, "Site location retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SiteLocationDto>.Failure($"Failed to retrieve site location: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SiteLocationDto>> CreateAsync(CreateSiteLocationRequest request,string currentUser,CancellationToken ct = default)
        {
            try
            {
                if (await _repository.ExistsByNameAsync(request.SiteName, null, ct))
                    return ApiResponse<SiteLocationDto>.Failure(
                        $"A site named '{request.SiteName}' already exists");

                // 1. Ensure NthGeoLocation row exists, matching BOTH ids and names
               

                // 2. Resolve canonical names from the (now guaranteed) row
                var names = await _repository.ResolveNamesAsync(request.ProvinceId, request.DistrictId, request.TehsilId, request.UcId, ct);

                await _repository.CreateGeolocationDataIfNotExistsAsync(currentUser, request.ProvinceId, request.DistrictId, request.TehsilId, request.UcId, names.Province, names.District,
                    names.Tehsil, names.Uc, ct);

                if (names.Province == null)
                    return ApiResponse<SiteLocationDto>.Failure("Invalid province");

                if (names.District == null)
                    return ApiResponse<SiteLocationDto>.Failure(
                        "District does not belong to the selected province");

                if (names.Tehsil == null)
                    return ApiResponse<SiteLocationDto>.Failure(
                        "Tehsil does not belong to the selected district");

                var entity = new NthSiteLocation
                {
                    SiteName = request.SiteName.Trim().ToLower(),

                    ProvinceId = request.ProvinceId,
                    Province = names.Province,
                    DistrictId = request.DistrictId,
                    District = names.District,
                    TehsilId = request.TehsilId,
                    Tehsil = names.Tehsil,

                    ProvinceIdNew = request.ProvinceId,
                    ProvinceNew = names.Province,
                    DistrictIdNew = request.DistrictId,
                    DistrictNew = names.District,
                    TehsilIdNew = request.TehsilId,
                    TehsilNew = names.Tehsil,

                    Address = request.Address,
                    Contact = request.Contact,
                    GeoLocation = request.GeoLocation,
                    HeadName = request.HeadName,
                    SiteTarget = request.SiteTarget,
                    Latitude = null,
                    Longitude = null,

                    IsActive = 0,
                    IsClosed = null,
                    IsMobileSite = request.IsMobileSite,

                    MatchedP = "YES",
                    MatchedD = "YES",
                    MatchedT = "YES",
                    IsManualUpdate = "YES",

                    CreatedBy = currentUser,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = null,
                    UpdatedOn = null
                };

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync(ct);

                var dto = await _repository.GetDtoByIdAsync(entity.Id, ct);

                return ApiResponse<SiteLocationDto>.Success(
                    dto!,
                    "Site location created successfully",
                    201);
            }
            catch (Exception ex)
            {
                return ApiResponse<SiteLocationDto>.Failure(
                    $"Failed to create site location: {ex.Message}");
            }
        }
        public async Task<ApiResponse<SiteLocationDto>> UpdateAsync(decimal id, UpdateSiteLocationRequest request, string currentUser, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<SiteLocationDto>.Failure("Site location not found", null, 404);

                if (await _repository.ExistsByNameAsync(request.SiteName, id, ct))
                    return ApiResponse<SiteLocationDto>.Failure($"A site named '{request.SiteName}' already exists");

                var names = await _repository.ResolveNamesAsync(request.ProvinceId, request.DistrictId, request.TehsilId, request.UcId, ct);

                if (names.Province == null)
                    return ApiResponse<SiteLocationDto>.Failure("Invalid province");
                if (names.District == null)
                    return ApiResponse<SiteLocationDto>.Failure("District does not belong to the selected province");
                if (names.Tehsil == null)
                    return ApiResponse<SiteLocationDto>.Failure("Tehsil does not belong to the selected district");


                entity.SiteName = request.SiteName.Trim().ToLower();
                entity.ProvinceId = request.ProvinceId;
                entity.Province = names.Province;
                entity.DistrictId = request.DistrictId;
                entity.District = names.District;
                entity.TehsilId = request.TehsilId;
                entity.Tehsil = names.Tehsil;
                entity.Address = request.Address;
                entity.Contact = request.Contact;
                entity.GeoLocation = request.GeoLocation;
                entity.HeadName = request.HeadName;
                entity.SiteTarget = request.SiteTarget;
                entity.Latitude = request.Latitude;
                entity.Longitude = request.Longitude;
                entity.IsActive = request.IsActive ? 1 : 0;
                entity.UpdatedBy = currentUser;
                entity.UpdatedOn = DateTime.UtcNow;

                _repository.UpdateAsync(entity);
                await _repository.SaveChangesAsync(ct);

                var dto = await _repository.GetDtoByIdAsync(id, ct);
                return ApiResponse<SiteLocationDto>.Success(dto!, "Site location updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<SiteLocationDto>.Failure($"Failed to update site location: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(decimal id, string currentUser, CancellationToken ct = default)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<bool>.Failure("Site location not found", null, 404);

                entity.IsActive = 0;
                entity.UpdatedBy = currentUser;
                entity.UpdatedOn = DateTime.UtcNow;
                _repository.UpdateAsync(entity);
                await _repository.SaveChangesAsync(ct);

                return ApiResponse<bool>.Success(true, "Site location deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure($"Failed to delete site location: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProvinceDto>>> GetProvincesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _repository.GetProvincesAsync(ct);
                return ApiResponse<List<ProvinceDto>>.Success(list, "Provinces retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProvinceDto>>.Failure($"Failed to retrieve provinces: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DistrictDto>>> GetDistrictsAsync(decimal provinceId, CancellationToken ct = default)
        {
            try
            {
                var list = await _repository.GetDistrictsByProvinceAsync(provinceId, ct);
                return ApiResponse<List<DistrictDto>>.Success(list, "Districts retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DistrictDto>>.Failure($"Failed to retrieve districts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TehsilDto>>> GetTehsilsAsync(decimal districtId, CancellationToken ct = default)
        {
            try
            {
                var list = await _repository.GetTehsilsByDistrictAsync(districtId, ct);
                return ApiResponse<List<TehsilDto>>.Success(list, "Tehsils retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TehsilDto>>.Failure($"Failed to retrieve tehsils: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UcDto>>> GetUcsAsync(decimal tehsilId, CancellationToken ct = default)
        {
            try
            {
                var list = await _repository.GetUcsByTehsilAsync(tehsilId, ct);
                return ApiResponse<List<UcDto>>.Success(list, "UCs retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UcDto>>.Failure($"Failed to retrieve UCs: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SiteDropdownDto>>> GetSitesByFiltersAsync( decimal? provinceId = null, decimal? districtId = null, decimal? tehsilId = null,CancellationToken ct = default)
        {
            try
            {
                var list = await _repository.GetSitesByFiltersAsync(provinceId, districtId, tehsilId, ct);
                return ApiResponse<List<SiteDropdownDto>>.Success(list, "Sites retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SiteDropdownDto>>.Failure($"Failed to retrieve sites: {ex.Message}");
            }
        }
    }
}
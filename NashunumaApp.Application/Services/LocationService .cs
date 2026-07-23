// Application/Services/LocationService.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Interfaces;
using NashunumaApp.Domain.Common.DTOs;

namespace NashunumaApp.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;

        public LocationService(ILocationRepository locationRepository, IMapper mapper)
        {
            _locationRepository = locationRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ProvinceDto>>> GetAllProvinces()
        {
            try
            {
                var provinces = await _locationRepository.GetAllProvinces();

                return ApiResponse<List<ProvinceDto>>.Success(
                    provinces,
                    "Provinces retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProvinceDto>>.Failure(
                    $"Failed to retrieve provinces: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<List<DistrictDto>>> GetDistrictsByProvince(string provinceName)
        {
            try
            {
                if (string.IsNullOrEmpty(provinceName))
                {
                    return ApiResponse<List<DistrictDto>>.Failure("Province name is required");
                }

                var districts = await _locationRepository.GetDistrictsByProvinceName(provinceName);

                return ApiResponse<List<DistrictDto>>.Success(
                    districts,
                    "Districts retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DistrictDto>>.Failure(
                    $"Failed to retrieve districts: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<List<TehsilDto>>> GetTehsilsByDistrict(string districtName)
        {
            try
            {
                if (string.IsNullOrEmpty(districtName))
                {
                    return ApiResponse<List<TehsilDto>>.Failure("District name is required");
                }

                var tehsils = await _locationRepository.GetTehsilsByDistrictName(districtName);

                return ApiResponse<List<TehsilDto>>.Success(
                    tehsils,
                    "Tehsils retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TehsilDto>>.Failure(
                    $"Failed to retrieve tehsils: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<List<UcDto>>> GetUcsByTehsil(string tehsilName)
        {
            try
            {
                if (string.IsNullOrEmpty(tehsilName))
                {
                    return ApiResponse<List<UcDto>>.Failure("Tehsil name is required");
                }

                var ucs = await _locationRepository.GetUcsByTehsilName(tehsilName);

                return ApiResponse<List<UcDto>>.Success(
                    ucs,
                    "UCs retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UcDto>>.Failure(
                    $"Failed to retrieve UCs: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<LocationHierarchyDto>> GetFullLocationHierarchy()
        {
            try
            {
                var hierarchy = await _locationRepository.GetLocationHierarchy();

                return ApiResponse<LocationHierarchyDto>.Success(
                    hierarchy,
                    "Location hierarchy retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<LocationHierarchyDto>.Failure(
                    $"Failed to retrieve location hierarchy: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<ProvinceDto>> GetProvinceByName(string provinceName)
        {
            try
            {
                if (string.IsNullOrEmpty(provinceName))
                {
                    return ApiResponse<ProvinceDto>.Failure("Province name is required");
                }

                var province = await _locationRepository.GetProvinceByName(provinceName);

                if (province == null)
                {
                    return ApiResponse<ProvinceDto>.Failure($"Province '{provinceName}' not found");
                }

                return ApiResponse<ProvinceDto>.Success(province, "Province retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProvinceDto>.Failure($"Failed to retrieve province: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DistrictDto>> GetDistrictByName(string districtName)
        {
            try
            {
                if (string.IsNullOrEmpty(districtName))
                {
                    return ApiResponse<DistrictDto>.Failure("District name is required");
                }

                var district = await _locationRepository.GetDistrictByName(districtName);

                if (district == null)
                {
                    return ApiResponse<DistrictDto>.Failure($"District '{districtName}' not found");
                }

                return ApiResponse<DistrictDto>.Success(district, "District retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<DistrictDto>.Failure($"Failed to retrieve district: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TehsilDto>> GetTehsilByName(string tehsilName)
        {
            try
            {
                if (string.IsNullOrEmpty(tehsilName))
                {
                    return ApiResponse<TehsilDto>.Failure("Tehsil name is required");
                }

                var tehsil = await _locationRepository.GetTehsilByName(tehsilName);

                if (tehsil == null)
                {
                    return ApiResponse<TehsilDto>.Failure($"Tehsil '{tehsilName}' not found");
                }

                return ApiResponse<TehsilDto>.Success(tehsil, "Tehsil retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TehsilDto>.Failure($"Failed to retrieve tehsil: {ex.Message}");
            }
        }
    }
}
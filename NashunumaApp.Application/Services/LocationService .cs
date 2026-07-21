// Application/Services/LocationService.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.Interfaces;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Domain.Interfaces;

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

        public async Task<ApiResponse<List<ProvinceDto>>> GetAllProvincesAsync()
        {
            try
            {
                var provinces = await _locationRepository.GetAllProvincesAsync();
                var mappedData = _mapper.Map<List<ProvinceDto>>(provinces);

                return ApiResponse<List<ProvinceDto>>.Success(
                    mappedData,
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

        public async Task<ApiResponse<List<DistrictDto>>> GetDistrictsByProvinceAsync(decimal provinceCode)
        {
            try
            {
                if (provinceCode <= 0)
                {
                    return ApiResponse<List<DistrictDto>>.Failure("Invalid province code");
                }

                var districts = await _locationRepository.GetDistrictsByProvinceCodeAsync(provinceCode);
                var mappedData = _mapper.Map<List<DistrictDto>>(districts);

                return ApiResponse<List<DistrictDto>>.Success(
                    mappedData,
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

        public async Task<ApiResponse<List<TehsilDto>>> GetTehsilsByDistrictAsync(decimal districtCode)
        {
            try
            {
                if (districtCode <= 0)
                {
                    return ApiResponse<List<TehsilDto>>.Failure("Invalid district code");
                }

                var tehsils = await _locationRepository.GetTehsilsByDistrictCodeAsync(districtCode);
                var mappedData = _mapper.Map<List<TehsilDto>>(tehsils);

                return ApiResponse<List<TehsilDto>>.Success(
                    mappedData,
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

        public async Task<ApiResponse<List<UcDto>>> GetUcsByTehsilAsync(decimal tehsilCode)
        {
            try
            {
                if (tehsilCode <= 0)
                {
                    return ApiResponse<List<UcDto>>.Failure("Invalid tehsil code");
                }

                var ucs = await _locationRepository.GetUcsByTehsilCodeAsync(tehsilCode);
                var mappedData = _mapper.Map<List<UcDto>>(ucs);

                return ApiResponse<List<UcDto>>.Success(
                    mappedData,
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

        public async Task<ApiResponse<LocationHierarchyDto>> GetFullLocationHierarchyAsync()
        {
            try
            {
                var provinces = await _locationRepository.GetAllProvincesAsync();
                var hierarchy = new LocationHierarchyDto();

                foreach (var province in provinces)
                {
                    var provinceDto = new ProvinceWithDistrictsDto
                    {
                        Id = province.Id,
                        Provcode = province.Provcode,
                        Province = province.Province
                    };

                    if (province.Provcode.HasValue)
                    {
                        var districts = await _locationRepository.GetDistrictsByProvinceCodeAsync(province.Provcode.Value);

                        foreach (var district in districts)
                        {
                            var districtDto = new DistrictWithTehsilsDto
                            {
                                Id = district.Id,
                                Distcode = district.Distcode,
                                District = district.District,
                                Provcode = district.Provcode
                            };

                            if (district.Distcode.HasValue)
                            {
                                var tehsils = await _locationRepository.GetTehsilsByDistrictCodeAsync(district.Distcode.Value);

                                foreach (var tehsil in tehsils)
                                {
                                    var tehsilDto = new TehsilWithUcsDto
                                    {
                                        Id = tehsil.Id,
                                        Distcode = tehsil.Distcode,
                                        Tehsilcode = tehsil.Tehsilcode,
                                        Tehsil = tehsil.Tehsil
                                    };

                                    if (tehsil.Tehsilcode.HasValue)
                                    {
                                        var ucs = await _locationRepository.GetUcsByTehsilCodeAsync(tehsil.Tehsilcode.Value);
                                        tehsilDto.Ucs = _mapper.Map<List<UcDto>>(ucs);
                                    }

                                    districtDto.Tehsils.Add(tehsilDto);
                                }
                            }

                            provinceDto.Districts.Add(districtDto);
                        }
                    }

                    hierarchy.Provinces.Add(provinceDto);
                }

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

        public async Task<ApiResponse<ProvinceDto>> GetProvinceByCodeAsync(decimal provinceCode)
        {
            try
            {
                if (provinceCode <= 0)
                {
                    return ApiResponse<ProvinceDto>.Failure("Invalid province code");
                }

                var province = await _locationRepository.GetProvinceByCodeAsync(provinceCode);

                if (province == null)
                {
                    return ApiResponse<ProvinceDto>.Failure($"Province with code {provinceCode} not found");
                }

                var mappedData = _mapper.Map<ProvinceDto>(province);
                return ApiResponse<ProvinceDto>.Success(mappedData, "Province retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProvinceDto>.Failure($"Failed to retrieve province: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DistrictDto>> GetDistrictByCodeAsync(decimal districtCode)
        {
            try
            {
                if (districtCode <= 0)
                {
                    return ApiResponse<DistrictDto>.Failure("Invalid district code");
                }

                var district = await _locationRepository.GetDistrictByCodeAsync(districtCode);

                if (district == null)
                {
                    return ApiResponse<DistrictDto>.Failure($"District with code {districtCode} not found");
                }

                var mappedData = _mapper.Map<DistrictDto>(district);
                return ApiResponse<DistrictDto>.Success(mappedData, "District retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<DistrictDto>.Failure($"Failed to retrieve district: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TehsilDto>> GetTehsilByCodeAsync(decimal tehsilCode)
        {
            try
            {
                if (tehsilCode <= 0)
                {
                    return ApiResponse<TehsilDto>.Failure("Invalid tehsil code");
                }

                var tehsil = await _locationRepository.GetTehsilByCodeAsync(tehsilCode);

                if (tehsil == null)
                {
                    return ApiResponse<TehsilDto>.Failure($"Tehsil with code {tehsilCode} not found");
                }

                var mappedData = _mapper.Map<TehsilDto>(tehsil);
                return ApiResponse<TehsilDto>.Success(mappedData, "Tehsil retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TehsilDto>.Failure($"Failed to retrieve tehsil: {ex.Message}");
            }
        }
    }
}
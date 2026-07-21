using NashunumaApp.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Application.Interfaces
{
    public interface ILocationService
    {
        Task<ApiResponse<List<ProvinceDto>>> GetAllProvincesAsync();
        Task<ApiResponse<List<DistrictDto>>> GetDistrictsByProvinceAsync(decimal provinceCode);
        Task<ApiResponse<List<TehsilDto>>> GetTehsilsByDistrictAsync(decimal districtCode);
        Task<ApiResponse<List<UcDto>>> GetUcsByTehsilAsync(decimal tehsilCode);
        Task<ApiResponse<LocationHierarchyDto>> GetFullLocationHierarchyAsync();
        Task<ApiResponse<ProvinceDto>> GetProvinceByCodeAsync(decimal provinceCode);
        Task<ApiResponse<DistrictDto>> GetDistrictByCodeAsync(decimal districtCode);
        Task<ApiResponse<TehsilDto>> GetTehsilByCodeAsync(decimal tehsilCode);
    }
}

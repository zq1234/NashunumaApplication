using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Domain.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Application.Interfaces
{
    public interface ILocationService
    {
        Task<ApiResponse<List<ProvinceDto>>> GetAllProvinces();
        Task<ApiResponse<List<DistrictDto>>> GetDistrictsByProvince(string provinceName);
        Task<ApiResponse<List<TehsilDto>>> GetTehsilsByDistrict(string districtName);
        Task<ApiResponse<List<UcDto>>> GetUcsByTehsil(string tehsilName);
        Task<ApiResponse<LocationHierarchyDto>> GetFullLocationHierarchy();
        Task<ApiResponse<ProvinceDto>> GetProvinceByName(string provinceName);
        Task<ApiResponse<DistrictDto>> GetDistrictByName(string districtName);
        Task<ApiResponse<TehsilDto>> GetTehsilByName(string tehsilName);
    }
}

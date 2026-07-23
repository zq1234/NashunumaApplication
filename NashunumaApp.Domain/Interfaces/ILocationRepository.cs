using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Domain.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<ProvinceDto>> GetAllProvinces();
        Task<List<DistrictDto>> GetDistrictsByProvinceName(string provinceName);
        Task<List<TehsilDto>> GetTehsilsByDistrictName(string districtName);
        Task<List<UcDto>> GetUcsByTehsilName(string tehsilName);
        Task<ProvinceDto> GetProvinceByName(string provinceName);
        Task<DistrictDto> GetDistrictByName(string districtName);
        Task<TehsilDto> GetTehsilByName(string tehsilName);
        Task<LocationHierarchyDto> GetLocationHierarchy();
    }
}

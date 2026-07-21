using NashunumaApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NashunumaApp.Domain.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<NthProvince>> GetAllProvincesAsync();
        Task<List<NthDistrict>> GetDistrictsByProvinceCodeAsync(decimal provinceCode);
        Task<List<NthTehsil>> GetTehsilsByDistrictCodeAsync(decimal districtCode);
        Task<List<NthUc>> GetUcsByTehsilCodeAsync(decimal tehsilCode);
        Task<NthProvince> GetProvinceByCodeAsync(decimal provinceCode);
        Task<NthDistrict> GetDistrictByCodeAsync(decimal districtCode);
        Task<NthTehsil> GetTehsilByCodeAsync(decimal tehsilCode);
    }
}

// Application/Mappings/MappingProfile.cs
using AutoMapper;
using NashunumaApp.Application.DTOs.Common;
using NashunumaApp.Application.DTOs.FoodStock;
using NashunumaApp.Application.DTOs.User;
using NashunumaApp.Domain.Common.DTOs;
using NashunumaApp.Domain.Entities;

namespace NashunumaApp.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FoodStockWithSiteDto, FoodStockDto>();
            CreateMap<UserWithLocationDto, UserDto>();
            CreateMap<NthSnfStock, FoodStockDto>();
            CreateMap<NthProvince, ProvinceDto>();
            CreateMap<NthDistrict, DistrictDto>();
            CreateMap<NthTehsil, TehsilDto>();
            CreateMap<NthUc, UcDto>();
        }
    }
}
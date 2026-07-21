// Application/DTOs/Common/LocationDto.cs
namespace NashunumaApp.Application.DTOs.Common
{
    public class ProvinceDto
    {
        public decimal Id { get; set; }
        public decimal? Provcode { get; set; }
        public string? Province { get; set; }
    }

    public class DistrictDto
    {
        public decimal Id { get; set; }
        public decimal? Distcode { get; set; }
        public string? District { get; set; }
        public decimal? Provcode { get; set; }
    }

    public class TehsilDto
    {
        public decimal Id { get; set; }
        public decimal? Distcode { get; set; }
        public decimal? Tehsilcode { get; set; }
        public string? Tehsil { get; set; }
    }

    public class UcDto
    {
        public decimal Id { get; set; }
        public decimal Tehsilcode { get; set; }
        public decimal Uccode { get; set; }
        public string? Uctype { get; set; }
        public decimal? Ucno { get; set; }
        public string? Uc { get; set; }
    }

    public class LocationHierarchyDto
    {
        public List<ProvinceWithDistrictsDto> Provinces { get; set; } = new();
    }

    public class ProvinceWithDistrictsDto
    {
        public decimal Id { get; set; }
        public decimal? Provcode { get; set; }
        public string? Province { get; set; }
        public List<DistrictWithTehsilsDto> Districts { get; set; } = new();
    }

    public class DistrictWithTehsilsDto
    {
        public decimal Id { get; set; }
        public decimal? Distcode { get; set; }
        public string? District { get; set; }
        public decimal? Provcode { get; set; }
        public List<TehsilWithUcsDto> Tehsils { get; set; } = new();
    }

    public class TehsilWithUcsDto
    {
        public decimal Id { get; set; }
        public decimal? Distcode { get; set; }
        public decimal? Tehsilcode { get; set; }
        public string? Tehsil { get; set; }
        public List<UcDto> Ucs { get; set; } = new();
    }
}
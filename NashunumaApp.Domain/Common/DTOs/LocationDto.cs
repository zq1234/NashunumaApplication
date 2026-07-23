// Application/DTOs/Common/LocationDto.cs
namespace NashunumaApp.Domain.Common.DTOs;

public class ProvinceDto
{
    public decimal Id { get; set; }
    public string? Province { get; set; }
}

public class DistrictDto
{
    public decimal Id { get; set; }
    public string? District { get; set; }
    public decimal? ProvinceId { get; set; }
}

public class TehsilDto
{
    public decimal Id { get; set; }
    public string? Tehsil { get; set; }
    public decimal? DistrictId { get; set; }
}

public class UcDto
{
    public decimal Id { get; set; }
    public string? Uc { get; set; }
    public decimal? TehsilId { get; set; }
}

public class ProvinceWithDistrictsDto
{
    public decimal Id { get; set; }
    public string? Province { get; set; }
    public List<DistrictWithTehsilsDto> Districts { get; set; } = new();
}

public class DistrictWithTehsilsDto
{
    public decimal Id { get; set; }
    public string? District { get; set; }
    public List<TehsilWithCountDto> Tehsils { get; set; } = new();
}

public class TehsilWithCountDto
{
    public decimal Id { get; set; }
    public string? Tehsil { get; set; }
    public int SiteCount { get; set; }
}

public class LocationHierarchyDto
{
    public List<ProvinceWithDistrictsDto> Provinces { get; set; } = new();
}
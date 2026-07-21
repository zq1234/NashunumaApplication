// DTOs/User/UpdateUserLocationDto.cs
namespace NashunumaApp.Application.DTOs.User
{
    public class UpdateUserLocationDto
    {
        public string Province { get; set; }
        public decimal? ProvinceId { get; set; }
        public string District { get; set; }
        public decimal? DistrictId { get; set; }
        public string Tehsil { get; set; }
        public decimal? TehsilId { get; set; }
        public decimal? SiteId { get; set; }
        public string SiteName { get; set; }
    }
}
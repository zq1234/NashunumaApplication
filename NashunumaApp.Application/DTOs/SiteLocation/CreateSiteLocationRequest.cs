// Application/DTOs/SiteLocation/CreateSiteLocationRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NashunumaApp.Application.DTOs.SiteLocation
{
    public class CreateSiteLocationRequest
    {
        [Required, MaxLength(200)]
        public string SiteName { get; set; } = default!;

        [Required]
        public decimal ProvinceId { get; set; }

        [Required]
        public decimal DistrictId { get; set; }

        [Required]
        public decimal TehsilId { get; set; }
        public decimal? UcId { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? GeoLocation { get; set; }
        public string? HeadName { get; set; }
        public decimal? SiteTarget { get; set; }
        public string? IsMobileSite { get; set; }
    }
}
using System;

namespace NashunumaApp.Domain.Common.DTOs
{
    public class SiteLocationDto
    {
        public decimal Id { get; set; }
        public string? SiteName { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? GeoLocation { get; set; }
        public string? HeadName { get; set; }
        public decimal? SiteTarget { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsActive { get; set; }

        public decimal? ProvinceId { get; set; }
        public string? Province { get; set; }
        public decimal? DistrictId { get; set; }
        public string? District { get; set; }
        public decimal? TehsilId { get; set; }
        public string? Tehsil { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    //public class ProvinceDto
    //{
    //    public decimal Id { get; set; }
    //    public string? Province { get; set; }
    //}

    //public class DistrictDto
    //{
    //    public decimal Id { get; set; }
    //    public string? District { get; set; }
    //    public decimal? ProvinceId { get; set; }
    //}

    //public class TehsilDto
    //{
    //    public decimal Id { get; set; }
    //    public string? Tehsil { get; set; }
    //    public decimal? DistrictId { get; set; }
    //}

    //public class UcDto
    //{
    //    public decimal Id { get; set; }
    //    public string? Uc { get; set; }
    //    public decimal? TehsilId { get; set; }
    //}
}
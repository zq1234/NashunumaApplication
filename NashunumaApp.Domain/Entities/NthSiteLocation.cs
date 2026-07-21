using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthSiteLocation
{
    public decimal Id { get; set; }

    public string? SiteName { get; set; }

    public decimal? TehsilId { get; set; }

    public decimal? DistrictId { get; set; }

    public decimal? ProvinceId { get; set; }

    public decimal? IsActive { get; set; }

    public string? Address { get; set; }

    public string? Contact { get; set; }

    public string? GeoLocation { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Tehsil { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? HeadName { get; set; }

    public decimal? SiteTarget { get; set; }

    public string? IsClosed { get; set; }

    public string? IsMobileSite { get; set; }

    public decimal? TehsilIdNew { get; set; }

    public decimal? DistrictIdNew { get; set; }

    public decimal? ProvinceIdNew { get; set; }

    public string? TehsilNew { get; set; }

    public string? DistrictNew { get; set; }

    public string? ProvinceNew { get; set; }

    public string? MatchedD { get; set; }

    public string? MatchedT { get; set; }

    public string? MatchedP { get; set; }

    public string? IsManualUpdate { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}

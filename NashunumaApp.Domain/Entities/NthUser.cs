using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthUser
{
    public decimal? Userid { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Tehsil { get; set; }

    public string? Personname { get; set; }

    public string? Designation { get; set; }

    public string? Email { get; set; }

    public string? Mobilenumber { get; set; }

    public string? Imeino { get; set; }

    public string? Macaddress { get; set; }

    public string? Gpscoordinates { get; set; }

    public string? Usertype { get; set; }

    public string? Isactive { get; set; }

    public string? Isadmin { get; set; }

    public string? Requestdatetime { get; set; }

    public string? Activedatetime { get; set; }

    public string? Activedby { get; set; }

    public string? Lastlogindatetime { get; set; }

    public string? Lastotpcode { get; set; }

    public string? Lastotpcodedatetime { get; set; }

    public string? Hospitalname { get; set; }

    public string? Changemobile { get; set; }

    public string? Appversion { get; set; }

    public decimal? SiteId { get; set; }

    public string? SiteName { get; set; }

    public decimal? ProvinceId { get; set; }

    public decimal? DistrictId { get; set; }

    public decimal? TehsilId { get; set; }

    public decimal? DesignationId { get; set; }

    public string? DesignationName { get; set; }

    public decimal? EditProfile { get; set; }

    public decimal? Istransferred { get; set; }

    public string? ChangeType { get; set; }

    public string? ImeiTagCount { get; set; }

    public string? Matched { get; set; }

    public string? IsManualUpdated { get; set; }
}

// Domain/Common/DTOs/UserWithLocationDto.cs
namespace NashunumaApp.Domain.Common.DTOs
{
    public class UserWithLocationDto
    {
         
        public decimal? UserId { get; set; }
        public string? Username { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public string? Mobilenumber { get; set; }
        public string? Designation { get; set; }
        public string? Usertype { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Tehsil { get; set; }
        public decimal? SiteId { get; set; }
        public string? SiteName { get; set; }
        public string? Isactive { get; set; }
        public string? Isadmin { get; set; }
        public string? Lastlogindatetime { get; set; }
        public string? Imeino { get; set; }
        public string? Macaddress { get; set; }
        public string? Requestdatetime { get; set; }
        public string? Activedatetime { get; set; }
        public string? Activedby { get; set; }
        public string? ChangeType { get; set; }
        public decimal? Istransferred { get; set; }
        public string? Updatedby { get; set; }
        public string? Updatedon { get; set; }

        // Site Location fields - matching NthSiteLocation entity
        public string? SiteAddress { get; set; }
        public string? SiteContact { get; set; }
        public string? SiteGeoLocation { get; set; }
        public string? SiteProvince { get; set; }
        public string? SiteDistrict { get; set; }
        public string? SiteTehsil { get; set; }
        public string? SiteHeadName { get; set; }
        public string? SiteIsClosed { get; set; }
        public string? SiteIsMobileSite { get; set; }
        public string? SiteProvinceNew { get; set; }
        public string? SiteDistrictNew { get; set; }
        public string? SiteTehsilNew { get; set; }
        public decimal? SiteLatitude { get; set; }
        public decimal? SiteLongitude { get; set; }
    }
}
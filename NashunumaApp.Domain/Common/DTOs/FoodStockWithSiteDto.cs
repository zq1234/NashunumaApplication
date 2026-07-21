// Domain/Common/DTOs/FoodStockWithSiteDto.cs
namespace NashunumaApp.Domain.Common.DTOs
{
    public class FoodStockWithSiteDto
    {
         
        public decimal Id { get; set; }
        public string? OpeningStockBoxesMamta { get; set; }
        public string? ReceivedStockBoxesMamta { get; set; }
        public string? DistributedBoxesMamta { get; set; }
        public string? ClosingStockBoxesMamta { get; set; }
        public string? Remarks { get; set; }
        public string? EnteredBy { get; set; }
        public string? EnteredOn { get; set; }
        public string? SiteId { get; set; }
        public string? OpeningStockSachetsMamta { get; set; }
        public string? ClosingStockSachetsMamta { get; set; }
        public string? DistributedSachetsMamta { get; set; }
        public string? OpeningStockSachetsWawa { get; set; }
        public string? ClosingStockSachetsWawa { get; set; }
        public string? DistributedSachetsWawa { get; set; }
        public string? OpeningStockBoxesWawa { get; set; }
        public string? ReceivedStockBoxesWawa { get; set; }
        public string? DistributedBoxesWawa { get; set; }
        public string? ClosingStockBoxesWawa { get; set; }
        public string? Unit { get; set; }
        public string? RutfReceived { get; set; }
        public string? RutfOpening { get; set; }
        public string? RutfDistributed { get; set; }
        public string? RutfClosing { get; set; }
        public string? IfaReceived { get; set; }
        public string? IfaOpening { get; set; }
        public string? IfaDistributed { get; set; }
        public string? IfaClosing { get; set; }
        public string? ActivityTime { get; set; }
        public string? IsManualUpdate { get; set; }
        public string? MmsReceived { get; set; }
        public string? MmsOpening { get; set; }
        public string? MmsDistributed { get; set; }
        public string? MmsClosing { get; set; }

         
        public string? SiteName { get; set; }
        public string? Address { get; set; }
        public string? Contact { get; set; }
        public string? GeoLocation { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Tehsil { get; set; }
        public string? HeadName { get; set; }
        public string? IsClosed { get; set; }
        public string? IsMobileSite { get; set; }
        public string? ProvinceNew { get; set; }
        public string? DistrictNew { get; set; }
        public string? TehsilNew { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
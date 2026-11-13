namespace NashunumaApp.Domain.Entities;

public class UserLocationTransferAudit
{
    public decimal Id { get; set; }
    public decimal? UserId { get; set; }
    public string? Username { get; set; }
    public string? ModifiedBy { get; set; }

    public string? FromProvince { get; set; }
    public decimal? FromProvinceId { get; set; }
    public string? FromDistrict { get; set; }
    public decimal? FromDistrictId { get; set; }
    public string? FromTehsil { get; set; }
    public decimal? FromTehsilId { get; set; }
    public decimal? FromSiteId { get; set; }
    public string? FromSiteName { get; set; }

    public string? ToProvince { get; set; }
    public decimal? ToProvinceId { get; set; }
    public string? ToDistrict { get; set; }
    public decimal? ToDistrictId { get; set; }
    public string? ToTehsil { get; set; }
    public decimal? ToTehsilId { get; set; }
    public decimal? ToSiteId { get; set; }
    public string? ToSiteName { get; set; }

    public string SystemName { get; set; } = "NashunumaApp";
    public DateTime TransferredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public decimal TotalTransferCount { get; set; }
}

using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthSnfStock
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
}

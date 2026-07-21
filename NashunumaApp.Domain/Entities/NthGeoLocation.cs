using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthGeoLocation
{
    public decimal Id { get; set; }

    public string Country { get; set; }

    public string Province { get; set; }

    public decimal PId { get; set; }

    public string District { get; set; }

    public decimal DId { get; set; }

    public string Tehsil { get; set; }

    public decimal TId { get; set; }

    public string Uc { get; set; }

    public decimal UcId { get; set; }

    public string Village { get; set; }

    public decimal VId { get; set; }

    public string Remarks { get; set; }
}

using System;
using System.Collections.Generic;

namespace NashunumaApp.Infrastructure.TempOracleEntities;

public partial class NdepLibDistrict
{
    public decimal ID { get; set; }

    public decimal? DIVCODE { get; set; }

    public decimal? DISTCODE { get; set; }

    public string? DISTRICT { get; set; }

    public decimal? NSER_CLUSTER { get; set; }

    public decimal? PROVCODE { get; set; }

    public decimal? BANK_CLUSTER { get; set; }

    public decimal? BANK { get; set; }

    public decimal? PMT_ZONE { get; set; }

    public byte? ZONECODE { get; set; }
}

using System;
using System.Collections.Generic;

namespace NashunumaApp.Infrastructure.TempOracleEntities;

public partial class NdepLibTehsil
{
    public decimal ID { get; set; }

    public decimal? DISTCODE { get; set; }

    public decimal? TEHSILCODE { get; set; }

    public string? TEHSIL { get; set; }
}

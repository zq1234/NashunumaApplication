using System;
using System.Collections.Generic;

namespace NashunumaApp.Infrastructure.TempOracleEntities;

public partial class NdepLibLocality
{
    public decimal ID { get; set; }

    public decimal? TEHSILCODE { get; set; }

    public decimal? UCCODE { get; set; }

    public decimal? LOCALITYCODE { get; set; }

    public string? LOCALITYTYPE { get; set; }

    public string? LOCALITY { get; set; }
}

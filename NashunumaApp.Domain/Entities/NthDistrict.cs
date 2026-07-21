using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthDistrict
{
    public decimal Id { get; set; }

    public decimal? Distcode { get; set; }

    public string? District { get; set; }

    public decimal? Provcode { get; set; }
}

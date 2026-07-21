using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthTehsil
{
    public decimal Id { get; set; }

    public decimal? Distcode { get; set; }

    public decimal? Tehsilcode { get; set; }

    public string? Tehsil { get; set; }
}

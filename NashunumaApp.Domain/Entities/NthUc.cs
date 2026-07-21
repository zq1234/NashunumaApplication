using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthUc
{
    public decimal Id { get; set; }

    public decimal? Tehsilcode { get; set; }

    public decimal? Uccode { get; set; }

    public string? Uctype { get; set; }

    public decimal? Ucno { get; set; }

    public string? Uc { get; set; }
}

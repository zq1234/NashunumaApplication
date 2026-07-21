using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthNadraChildVerification
{
    public decimal Childid { get; set; }

    public string Bformno { get; set; }

    public string Nameeng { get; set; }

    public string Nameur { get; set; }

    public string Gender { get; set; }

    public string Childdob { get; set; }

    public decimal? Childage { get; set; }

    public string Mothercnic { get; set; }

    public decimal? Verificationcount { get; set; }

    public string Isadolscent { get; set; }

    public string ApiStatusCodeNadra { get; set; }

    public DateTime? VerifiedOn { get; set; }

    public string NadraResponse { get; set; }
}

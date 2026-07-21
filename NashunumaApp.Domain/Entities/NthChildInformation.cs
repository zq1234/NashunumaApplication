using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthChildInformation
{
    public decimal Childid { get; set; }

    public string Bformno { get; set; }

    public string Nameeng { get; set; }

    public string Nameur { get; set; }

    public string Gender { get; set; }

    public string Childdob { get; set; }

    public string Childage { get; set; }

    public string Enteredby { get; set; }

    public string Enteredon { get; set; }

    public string Nadraverified { get; set; }

    public string Isdisable { get; set; }

    public string Disabilitytype { get; set; }

    public string Mothercnic { get; set; }

    public string Visible { get; set; }

    public string Updatedby { get; set; }

    public string Updatedon { get; set; }

    public string Exitstatus { get; set; }

    public string Exitdate { get; set; }

    public string Exitedby { get; set; }

    public string Exitedon { get; set; }

    public string Childdobbymother { get; set; }

    public string Genderbymother { get; set; }

    public decimal? SiteId { get; set; }

    public string SiteName { get; set; }

    public decimal? Registeredquarter { get; set; }

    public string RegistrationBookNo { get; set; }

    public string ReferredFrom { get; set; }

    public decimal? ChangeType { get; set; }

    public string IsAdolescent { get; set; }

    public string DbUser { get; set; }

    public string ApiReturnCode { get; set; }

    public string BeneTypeForBank { get; set; }

    public string IsPostDelivered { get; set; }

    public string VcId { get; set; }

    public string Education { get; set; }

    public string ConsectivePayment { get; set; }

    public decimal? DuplicateKey { get; set; }

    public string IsManualUpdate { get; set; }

    public string MmsEligible { get; set; }

    public DateTime? DobFixed { get; set; }
}

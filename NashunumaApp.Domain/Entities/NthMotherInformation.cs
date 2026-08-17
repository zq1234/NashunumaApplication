using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthMotherInformation
{
    public decimal Womenid { get; set; }

    public string Womencnic { get; set; }

    public string Womenname { get; set; }

    public string Husbandname { get; set; }

    public string Ishusbandalive { get; set; }

    public string Dob { get; set; }

    public string Age { get; set; }

    public string Province { get; set; }

    public string District { get; set; }

    public string Tehsil { get; set; }

    public string Uc { get; set; }

    public string Village { get; set; }

    public string Address { get; set; }

    public string Phoneno { get; set; }

    public string Firsttime { get; set; }

    public string Enteredby { get; set; }

    public string Enteredon { get; set; }

    public string Visitdate { get; set; }

    public string Ispregnant { get; set; }

    public string Pregstartmonth { get; set; }

    public string Trimister { get; set; }

    public string Expecteddeliverymonth { get; set; }

    public string Lastdeliverydate { get; set; }

    
    /// 1 = miscarriage, 2 = abortion, 3= death. 4=still birth, 5=mother death
    /// </summary>
    public string Deliverystatus { get; set; }

    public string Healthcaretype { get; set; }

    public string Doctorname { get; set; }

    public string Verificationdate { get; set; }

    public string Havechildren { get; set; }

    public decimal? Noofchildren { get; set; }

    public string Visible { get; set; }

    public string Updatedby { get; set; }

    public string Updatedon { get; set; }

    public decimal? SiteId { get; set; }

    public string SiteName { get; set; }

    public string Pregstartdate { get; set; }

    public string Expecteddeliverydate { get; set; }

    public decimal? Pregnancycode { get; set; }

    public string RegistrationBookNo { get; set; }

    public string ReferredFrom { get; set; }

    public decimal? ApiStatusCode { get; set; }

    public decimal? IsFloodAf { get; set; }

    public string Ba { get; set; }

    public string BeneTypeForBank { get; set; }

    public string PmtRecorded { get; set; }

    public string NutritionBeneType { get; set; }

    public string AccountCreatedWithEnrollment { get; set; }

    public string NextPregrency { get; set; }

    public string Matched { get; set; }

    public string ConsectivePayment { get; set; }

    public string IsManualUpdate { get; set; }

    public decimal? DuplicateKey { get; set; }

    public string MmsEligible { get; set; }

    public string MmsFalseDeliveryMark { get; set; }

    public decimal? CycNo { get; set; }

    public string PregTermBy { get; set; }

    public string PregTermOn { get; set; }

    public string CalledVia { get; set; }

    public string LastFacilitatedId { get; set; }

    public decimal? CurrentPregnancyCode { get; set; }
}

using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthAdolescenceChildVisitDetail
{
    public decimal Id { get; set; }

    public string Bformno { get; set; }

    public string Visitdate { get; set; }

    public string Quarterno { get; set; }

    public string Quartercode { get; set; }

    public string IfaUtilized { get; set; }

    public string IfaDistributed { get; set; }

    public string AwarenessSession { get; set; }

    public string FirstDoseAtFc { get; set; }

    public string MotherAccompinied { get; set; }

    public string Compliant { get; set; }

    public string Status { get; set; }

    public string Pictureurl { get; set; }

    public string Visible { get; set; }

    public string Enteredby { get; set; }

    public string Enteredon { get; set; }

    public string Updatedby { get; set; }

    public string Updatedon { get; set; }

    public string Mothercnic { get; set; }

    public decimal? Childid { get; set; }

    public string Firstvisitdate { get; set; }

    public string Lockedafterpayment { get; set; }

    public string Paymentdatetime { get; set; }

    public string Paymentby { get; set; }

    public string Batchnumber { get; set; }

    public string Lockedmessage { get; set; }

    public string Lockedmessageactual { get; set; }

    public decimal? SiteId { get; set; }

    public string SiteName { get; set; }

    public string CompliantDate { get; set; }

    public string RegistrationBookNo { get; set; }

    public string PhoneNo { get; set; }

    public string ApiStatusCode { get; set; }

    public string IfaEnabled { get; set; }

    public string Treatmentstatus { get; set; }

    public string Weight { get; set; }

    public string Height { get; set; }

    public string Muac { get; set; }

    public string Muacstatus { get; set; }

    public string Bmi { get; set; }

    public string Bmistatus { get; set; }

    public decimal? AmToBe { get; set; }

    public string ContAfterMarriage { get; set; }

    public string PendingStatus { get; set; }

    public string IsManualUpdate { get; set; }

    public decimal? DuplicateKey { get; set; }

    public string PeriodFromApp { get; set; }

    public string Version { get; set; }

    public string TranchEnroll { get; set; }

    public decimal? ReportingCode { get; set; }

    public string FaaMarkForP { get; set; }
}

using System;
using System.Collections.Generic;

namespace NashunumaApp.Domain.Entities;

public partial class NthPamentInformation
{
    public string Paymentid { get; set; }

    public decimal Beneficiaryid { get; set; }

    public decimal Motherid { get; set; }

    public decimal? Childid { get; set; }

    public decimal Quarter { get; set; }

    public decimal Installement { get; set; }

    public decimal Amount { get; set; }

    public DateTime Generationdate { get; set; }

    public decimal Bankid { get; set; }

    public decimal Clusterid { get; set; }

    public string Paymenttype { get; set; }

    public bool Throwtobank { get; set; }

    public DateTime Throwdate { get; set; }

    public string Bankstatus { get; set; }

    public decimal Cardmode { get; set; }

    public string Payedby { get; set; }

    public string Paidon { get; set; }

    public string Remarks { get; set; }

    public decimal? Id { get; set; }

    public string Lockedmessage { get; set; }

    public string Bankmessage { get; set; }

    public decimal? SiteId { get; set; }

    public string SiteName { get; set; }

    public string RegistrationBookNo { get; set; }

    public decimal? QuarterId { get; set; }

    public string CycleNo { get; set; }

    public string FundType { get; set; }

    public string BeneType { get; set; }

    public string Bankstatusaftermanualpayment { get; set; }

    public decimal? PFlag { get; set; }

    public decimal? VisitId { get; set; }

    /// <summary>
    /// Mother,Child,Adolescent
    /// </summary>
    public string PaymentType { get; set; }

    public string ReversalReporting { get; set; }

    public string OldPaymentId { get; set; }

    public string BankPaymentId { get; set; }

    public string MarkSuccess { get; set; }

    public decimal? MarkSuccessClusterId { get; set; }

    public string MarkSuccess2 { get; set; }
}

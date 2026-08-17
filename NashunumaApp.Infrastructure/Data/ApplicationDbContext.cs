using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NashunumaApp.Domain.Entities;
using NashunumaApp.Infrastructure.Identity;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<NthUser> NthUsers { get; set; }
    //public virtual DbSet<NthMotherInformation> NthMotherInformations { get; set; }
    //public virtual DbSet<NthMotherTrimister> NthMotherTrimisters { get; set; }
    //public virtual DbSet<NthChildInformation> NthChildInformations { get; set; }
    //public virtual DbSet<NthChildVisitDetail> NthChildVisitDetails { get; set; }
    //public virtual DbSet<NthGeoLocation> NthGeoLocations { get; set; }
    //public virtual DbSet<NthAdolescenceChildVisitDetail> NthAdolescenceChildVisitDetails { get; set; }
    //public virtual DbSet<NthNadraChildVerification> NthNadraChildVerifications { get; set; }
    public virtual DbSet<NthProvince> NthProvinces { get; set; }
    public virtual DbSet<NthDistrict> NthDistricts { get; set; }
    public virtual DbSet<NthTehsil> NthTehsils { get; set; }
    public virtual DbSet<NthUc> NthUcs { get; set; }
    public virtual DbSet<NthSiteLocation> NthSiteLocations { get; set; }
    //public virtual DbSet<NthPamentInformation> NthPamentInformations { get; set; }
    public virtual DbSet<NthSnfStock> Foodstock { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required for Identity
        base.OnModelCreating(modelBuilder);

        //modelBuilder
        //    .HasDefaultSchema("BISPADMIN")
        //    .UseCollation("USING_NLS_COMP");
   
        modelBuilder
            .HasDefaultSchema("MZAHID")
            .UseCollation("USING_NLS_COMP");
        modelBuilder.Entity<NthSiteLocation>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NTH_SITE_LOCATION", "BISP_NASHUNUMA");

            entity.HasIndex(e => e.Id, "IDX_ID_");

            entity.HasIndex(e => new { e.Id, e.SiteName }, "IDX_ID_SITE_NAME");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Contact)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CONTACT");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CREATED_BY");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("DATE")
                .HasColumnName("CREATED_ON");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.DistrictId)
                .HasColumnType("NUMBER")
                .HasColumnName("DISTRICT_ID");
            entity.Property(e => e.DistrictIdNew)
                .HasColumnType("NUMBER")
                .HasColumnName("DISTRICT_ID_NEW");
            entity.Property(e => e.DistrictNew)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DISTRICT_NEW");
            entity.Property(e => e.GeoLocation)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GEO_LOCATION");
            entity.Property(e => e.HeadName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("HEAD_NAME");
            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.IsActive)
                .HasColumnType("NUMBER")
                .HasColumnName("IS_ACTIVE");
            entity.Property(e => e.IsClosed)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IS_CLOSED");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.IsMobileSite)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IS_MOBILE_SITE");
            entity.Property(e => e.Latitude)
                .HasColumnType("NUMBER")
                .HasColumnName("LATITUDE");
            entity.Property(e => e.Longitude)
                .HasColumnType("NUMBER")
                .HasColumnName("LONGITUDE");
            entity.Property(e => e.MatchedD)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MATCHED_D");
            entity.Property(e => e.MatchedP)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MATCHED_P");
            entity.Property(e => e.MatchedT)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MATCHED_T");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PROVINCE");
            entity.Property(e => e.ProvinceId)
                .HasColumnType("NUMBER")
                .HasColumnName("PROVINCE_ID");
            entity.Property(e => e.ProvinceIdNew)
                .HasColumnType("NUMBER")
                .HasColumnName("PROVINCE_ID_NEW");
            entity.Property(e => e.ProvinceNew)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PROVINCE_NEW");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.SiteTarget)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_TARGET");
            entity.Property(e => e.Tehsil)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TEHSIL");
            entity.Property(e => e.TehsilId)
                .HasColumnType("NUMBER")
                .HasColumnName("TEHSIL_ID");
            entity.Property(e => e.TehsilIdNew)
                .HasColumnType("NUMBER")
                .HasColumnName("TEHSIL_ID_NEW");
            entity.Property(e => e.TehsilNew)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TEHSIL_NEW");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("UPDATED_BY");
            entity.Property(e => e.UpdatedOn)
                .HasColumnType("DATE")
                .HasColumnName("UPDATED_ON");
        });
        modelBuilder.Entity<NthSnfStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("STOCK_PK");

            entity.ToTable("NTH_SNF_STOCK", "BISP_NASHUNUMA");

            entity.HasIndex(e => e.EnteredOn, "IDX_NTH_SNF_STOCK_ENTEREDON");

            entity.HasIndex(e => e.SiteId, "IDX_NTH_SNF_STOCK_SITE");

            entity.HasIndex(e => new { e.SiteId, e.EnteredOn }, "IDX_NTH_SNF_STOCK_SITE_DATE");

            entity.HasIndex(e => new { e.SiteId, e.ActivityTime }, "IDX_STOCK_SITE_ACTIVITY");

            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.ActivityTime)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ACTIVITY_TIME");
            entity.Property(e => e.ClosingStockBoxesMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CLOSING_STOCK_BOXES_MAMTA");
            entity.Property(e => e.ClosingStockBoxesWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CLOSING_STOCK_BOXES_WAWA");
            entity.Property(e => e.ClosingStockSachetsMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CLOSING_STOCK_SACHETS_MAMTA");
            entity.Property(e => e.ClosingStockSachetsWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CLOSING_STOCK_SACHETS_WAWA");
            entity.Property(e => e.DistributedBoxesMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DISTRIBUTED_BOXES_MAMTA");
            entity.Property(e => e.DistributedBoxesWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DISTRIBUTED_BOXES_WAWA");
            entity.Property(e => e.DistributedSachetsMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DISTRIBUTED_SACHETS_MAMTA");
            entity.Property(e => e.DistributedSachetsWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DISTRIBUTED_SACHETS_WAWA");
            entity.Property(e => e.EnteredBy)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ENTERED_BY");
            entity.Property(e => e.EnteredOn)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ENTERED_ON");
            entity.Property(e => e.IfaClosing)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IFA_CLOSING");
            entity.Property(e => e.IfaDistributed)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IFA_DISTRIBUTED");
            entity.Property(e => e.IfaOpening)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IFA_OPENING");
            entity.Property(e => e.IfaReceived)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IFA_RECEIVED");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.MmsClosing)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MMS_CLOSING");
            entity.Property(e => e.MmsDistributed)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MMS_DISTRIBUTED");
            entity.Property(e => e.MmsOpening)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MMS_OPENING");
            entity.Property(e => e.MmsReceived)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MMS_RECEIVED");
            entity.Property(e => e.OpeningStockBoxesMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("OPENING_STOCK_BOXES_MAMTA");
            entity.Property(e => e.OpeningStockBoxesWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("OPENING_STOCK_BOXES_WAWA");
            entity.Property(e => e.OpeningStockSachetsMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("OPENING_STOCK_SACHETS_MAMTA");
            entity.Property(e => e.OpeningStockSachetsWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("OPENING_STOCK_SACHETS_WAWA");
            entity.Property(e => e.ReceivedStockBoxesMamta)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RECEIVED_STOCK_BOXES_MAMTA");
            entity.Property(e => e.ReceivedStockBoxesWawa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RECEIVED_STOCK_BOXES_WAWA");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("REMARKS");
            entity.Property(e => e.RutfClosing)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RUTF_CLOSING");
            entity.Property(e => e.RutfDistributed)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RUTF_DISTRIBUTED");
            entity.Property(e => e.RutfOpening)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RUTF_OPENING");
            entity.Property(e => e.RutfReceived)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RUTF_RECEIVED");
            entity.Property(e => e.SiteId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SITE_ID");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("UNIT");
        });

        modelBuilder.Entity<NthUser>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("SYS_C0082241");
            entity.ToTable("NTH_USERS", "BISP_NASHUNUMA");
            entity.Property(e => e.Username)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("USERNAME");
            entity.Property(e => e.Activedatetime)
                .HasMaxLength(50)
                .HasColumnName("ACTIVEDATETIME");
            entity.Property(e => e.Activedby)
                .HasMaxLength(50)
                .HasColumnName("ACTIVEDBY");
            entity.Property(e => e.Appversion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("APPVERSION");
            entity.Property(e => e.ChangeType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CHANGE_TYPE");
            entity.Property(e => e.Changemobile)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("CHANGEMOBILE");
            entity.Property(e => e.Designation)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("DESIGNATION");
            entity.Property(e => e.DesignationId)
                .HasColumnType("NUMBER")
                .HasColumnName("DESIGNATION_ID");
            entity.Property(e => e.DesignationName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DESIGNATION_NAME");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.DistrictId)
                .HasColumnType("NUMBER")
                .HasColumnName("DISTRICT_ID");
            entity.Property(e => e.EditProfile)
                .HasColumnType("NUMBER")
                .HasColumnName("EDIT_PROFILE");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gpscoordinates)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GPSCOORDINATES");
            entity.Property(e => e.Hospitalname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HOSPITALNAME");
            entity.Property(e => e.ImeiTagCount)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IMEI_TAG_COUNT");
            entity.Property(e => e.Imeino)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("IMEINO");
            entity.Property(e => e.IsManualUpdated)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATED");
            entity.Property(e => e.Isactive)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISACTIVE");
            entity.Property(e => e.Isadmin)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISADMIN");
            entity.Property(e => e.Istransferred)
                .HasColumnType("NUMBER")
                .HasColumnName("ISTRANSFERRED");
            entity.Property(e => e.TransferCountForSite)
                .HasColumnType("NUMBER")
                .HasColumnName("TRANSFER_COUNT_FOR_SITE");
            entity.Property(e => e.Lastlogindatetime)
                .HasMaxLength(50)
                .HasColumnName("LASTLOGINDATETIME");
            entity.Property(e => e.Lastotpcode)
                .HasMaxLength(50)
                .HasColumnName("LASTOTPCODE");
            entity.Property(e => e.Lastotpcodedatetime)
                .HasMaxLength(50)
                .HasColumnName("LASTOTPCODEDATETIME");
            entity.Property(e => e.Macaddress)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("MACADDRESS");
            entity.Property(e => e.Matched)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MATCHED");
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("MOBILENUMBER");
            entity.Property(e => e.Password)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Personname)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PERSONNAME");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PROVINCE");
            entity.Property(e => e.ProvinceId)
                .HasColumnType("NUMBER")
                .HasColumnName("PROVINCE_ID");
            entity.Property(e => e.Requestdatetime)
                .HasMaxLength(50)
                .HasColumnName("REQUESTDATETIME");
            entity.Property(e => e.SiteId)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_ID");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.Tehsil)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TEHSIL");
            entity.Property(e => e.TehsilId)
                .HasColumnType("NUMBER")
                .HasColumnName("TEHSIL_ID");
            entity.Property(e => e.Userid)
                .HasColumnType("NUMBER")
                .HasColumnName("USERID");
            entity.Property(e => e.Usertype)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("USERTYPE");
        });

        // NthMotherInformation
        modelBuilder.Entity<NthMotherInformation>(entity =>
        {
            entity.HasKey(e => e.Womenid).HasName("SYS_C00764971");
            entity.ToTable("NTH_MOTHER_INFORMATION", "BISP_NASHUNUMA");
            entity.HasIndex(e => new { e.SiteId, e.Womencnic, e.District, e.Enteredon }, "IDX$$_00010005");
            entity.HasIndex(e => new { e.SiteId, e.Trimister, e.Womencnic }, "IDX$$_0001000A");
            entity.HasIndex(e => new { e.SiteId, e.Womenid }, "IDX$$_0001000B");
            entity.HasIndex(e => new { e.Tehsil, e.SiteName, e.Province, e.District, e.Womencnic, e.Enteredon }, "IDX$$_00010019");
            entity.HasIndex(e => e.Womencnic, "UNIQ_WOMENCNIC").IsUnique();
            entity.Property(e => e.Womenid)
                .HasColumnType("NUMBER")
                .HasColumnName("WOMENID");
            entity.Property(e => e.AccountCreatedWithEnrollment)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ACCOUNT_CREATED_WITH_ENROLLMENT");
            entity.Property(e => e.Address)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("AGE");
            entity.Property(e => e.ApiStatusCode)
                .HasColumnType("NUMBER")
                .HasColumnName("API_STATUS_CODE");
            entity.Property(e => e.Ba)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BA");
            entity.Property(e => e.BeneTypeForBank)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BENE_TYPE_FOR_BANK");
            entity.Property(e => e.CalledVia)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CALLED_VIA");
            entity.Property(e => e.ConsectivePayment)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("CONSECTIVE_PAYMENT");
            entity.Property(e => e.CurrentPregnancyCode)
                .HasColumnType("NUMBER")
                .HasColumnName("CURRENT_PREGNANCY_CODE");
            entity.Property(e => e.CycNo)
                .HasColumnType("NUMBER")
                .HasColumnName("CYC_NO");
            entity.Property(e => e.Deliverystatus)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasComment("1 = miscarriage, 2 = abortion, 3= death. 4=still birth, 5=mother death")
                .HasColumnName("DELIVERYSTATUS");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Dob)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("DOB");
            entity.Property(e => e.Doctorname)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("DOCTORNAME");
            entity.Property(e => e.DuplicateKey)
                .HasDefaultValueSql("1")
                .HasColumnType("NUMBER")
                .HasColumnName("DUPLICATE_KEY");
            entity.Property(e => e.Enteredby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDBY");
            entity.Property(e => e.Enteredon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDON");
            entity.Property(e => e.Expecteddeliverydate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EXPECTEDDELIVERYDATE");
            entity.Property(e => e.Expecteddeliverymonth)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("EXPECTEDDELIVERYMONTH");
            entity.Property(e => e.Firsttime)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("FIRSTTIME");
            entity.Property(e => e.Havechildren)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("HAVECHILDREN");
            entity.Property(e => e.Healthcaretype)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("HEALTHCARETYPE");
            entity.Property(e => e.Husbandname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HUSBANDNAME");
            entity.Property(e => e.IsFloodAf)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("IS_FLOOD_AF");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.Ishusbandalive)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISHUSBANDALIVE");
            entity.Property(e => e.Ispregnant)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISPREGNANT");
            entity.Property(e => e.LastFacilitatedId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("LAST_FACILITATED_ID");
            entity.Property(e => e.Lastdeliverydate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("LASTDELIVERYDATE");
            entity.Property(e => e.Matched)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MATCHED");
            entity.Property(e => e.MmsEligible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("MMS_ELIGIBLE");
            entity.Property(e => e.MmsFalseDeliveryMark)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("MMS_FALSE_DELIVERY_MARK");
            entity.Property(e => e.NextPregrency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("NEXT_PREGRENCY");
            entity.Property(e => e.Noofchildren)
                .HasColumnType("NUMBER")
                .HasColumnName("NOOFCHILDREN");
            entity.Property(e => e.NutritionBeneType)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("NUTRITION_BENE_TYPE");
            entity.Property(e => e.Phoneno)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("PHONENO");
            entity.Property(e => e.PmtRecorded)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PMT_RECORDED");
            entity.Property(e => e.PregTermBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PREG_TERM_BY");
            entity.Property(e => e.PregTermOn)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PREG_TERM_ON");
            entity.Property(e => e.Pregnancycode)
                .HasColumnType("NUMBER")
                .HasColumnName("PREGNANCYCODE");
            entity.Property(e => e.Pregstartdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PREGSTARTDATE");
            entity.Property(e => e.Pregstartmonth)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PREGSTARTMONTH");
            entity.Property(e => e.Province)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PROVINCE");
            entity.Property(e => e.ReferredFrom)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("REFERRED_FROM");
            entity.Property(e => e.RegistrationBookNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REGISTRATION_BOOK_NO");
            entity.Property(e => e.SiteId)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_ID");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.Tehsil)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TEHSIL");
            entity.Property(e => e.Trimister)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("TRIMISTER");
            entity.Property(e => e.Uc)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("UC");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDBY");
            entity.Property(e => e.Updatedon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDON");
            entity.Property(e => e.Verificationdate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("VERIFICATIONDATE");
            entity.Property(e => e.Village)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("VILLAGE");
            entity.Property(e => e.Visible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("VISIBLE");
            entity.Property(e => e.Visitdate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("VISITDATE");
            entity.Property(e => e.Womencnic)
                .IsRequired()
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("WOMENCNIC");
            entity.Property(e => e.Womenname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("WOMENNAME");
        });

        // NthMotherTrimister
        modelBuilder.Entity<NthMotherTrimister>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C00764973");
            entity.ToTable("NTH_MOTHER_TRIMISTER", "BISP_NASHUNUMA");
            entity.HasIndex(e => new { e.Mothercnic, e.Trimistercode, e.Snfdistributed, e.Visible }, "IDX$$_623A80001");
            entity.HasIndex(e => e.Mothercnic, "INDEX1");
            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.AmToBe)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("AM_TO_BE");
            entity.Property(e => e.AmountReasons)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AMOUNT_REASONS");
            entity.Property(e => e.AmountReceived)
                .HasColumnType("NUMBER")
                .HasColumnName("AMOUNT_RECEIVED");
            entity.Property(e => e.Anc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ANC");
            entity.Property(e => e.ApiStatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("API_STATUS_CODE");
            entity.Property(e => e.Awareness)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("AWARENESS");
            entity.Property(e => e.BatchFromScanner)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BATCH_FROM_SCANNER");
            entity.Property(e => e.Batchnumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BATCHNUMBER");
            entity.Property(e => e.BeneTypeAcountBank)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BENE_TYPE_ACOUNT_BANK");
            entity.Property(e => e.Bmi)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("BMI");
            entity.Property(e => e.Bmistatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BMISTATUS");
            entity.Property(e => e.Compliant)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("COMPLIANT");
            entity.Property(e => e.CompliantDate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("COMPLIANT_DATE");
            entity.Property(e => e.CompliedStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("COMPLIED_STATUS");
            entity.Property(e => e.CycleNo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CYCLE_NO");
            entity.Property(e => e.DeleteFlag)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("'N'")
                .IsFixedLength()
                .HasColumnName("DELETE_FLAG");
            entity.Property(e => e.DuplicateKey)
                .HasColumnType("NUMBER")
                .HasColumnName("DUPLICATE_KEY");
            entity.Property(e => e.Endserial)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ENDSERIAL");
            entity.Property(e => e.Enteredby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDBY");
            entity.Property(e => e.Enteredon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDON");
            entity.Property(e => e.FaaMarkForP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("FAA_MARK_FOR_P");
            entity.Property(e => e.Firstvisitdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FIRSTVISITDATE");
            entity.Property(e => e.FoodQuantityFromOther)
                .HasColumnType("NUMBER")
                .HasColumnName("FOOD_QUANTITY_FROM_OTHER");
            entity.Property(e => e.Height)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("HEIGHT");
            entity.Property(e => e.IsAnemic)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("IS_ANEMIC");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.Iseligibleforcmam)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISELIGIBLEFORCMAM");
            entity.Property(e => e.Lockedafterpayment)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("LOCKEDAFTERPAYMENT");
            entity.Property(e => e.Lockedmessage)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("LOCKEDMESSAGE");
            entity.Property(e => e.Lockedmessageactual)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("LOCKEDMESSAGEACTUAL");
            entity.Property(e => e.Mappedtrimistercode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MAPPEDTRIMISTERCODE");
            entity.Property(e => e.MmsSource)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MMS_SOURCE");
            entity.Property(e => e.Mothercnic)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("MOTHERCNIC");
            entity.Property(e => e.Muac)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MUAC");
            entity.Property(e => e.Muacstatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MUACSTATUS");
            entity.Property(e => e.PaymentStatusBefore)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PAYMENT_STATUS_BEFORE");
            entity.Property(e => e.Paymentby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PAYMENTBY");
            entity.Property(e => e.Paymentdatetime)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PAYMENTDATETIME");
            entity.Property(e => e.PeriodFromApp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PERIOD_FROM_APP");
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PHONE_NO");
            entity.Property(e => e.Pictureurl)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("PICTUREURL");
            entity.Property(e => e.PregnancyCode)
                .HasColumnType("NUMBER")
                .HasColumnName("PREGNANCY_CODE");
            entity.Property(e => e.Pregnancycode)
                .HasColumnType("NUMBER")
                .HasColumnName("PREGNANCYCODE");
            entity.Property(e => e.Quarterstatus)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("QUARTERSTATUS");
            entity.Property(e => e.RegistrationBookNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REGISTRATION_BOOK_NO");
            entity.Property(e => e.ReportingCode)
                .HasColumnType("NUMBER")
                .HasColumnName("REPORTING_CODE");
            entity.Property(e => e.Selfimmunization)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("SELFIMMUNIZATION");
            entity.Property(e => e.SiteId)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_ID");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.Skippedmissed)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("SKIPPEDMISSED");
            entity.Property(e => e.SnfType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SNF_TYPE");
            entity.Property(e => e.Snfdistributed)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("SNFDISTRIBUTED");
            entity.Property(e => e.Snfutilized)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("SNFUTILIZED");
            entity.Property(e => e.Startserial)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("STARTSERIAL");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("STATUS");
            entity.Property(e => e.TranchEnroll)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("TRANCH_ENROLL");
            entity.Property(e => e.Treatementstartdate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("TREATEMENTSTARTDATE");
            entity.Property(e => e.Treatmentstarted)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("TREATMENTSTARTED");
            entity.Property(e => e.Treatmentstatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TREATMENTSTATUS");
            entity.Property(e => e.Trimistercode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TRIMISTERCODE");
            entity.Property(e => e.Trimisterno)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TRIMISTERNO");
            entity.Property(e => e.Tt1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("TT1");
            entity.Property(e => e.Tt2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("TT2");
            entity.Property(e => e.Tt3)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("TT3");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDBY");
            entity.Property(e => e.Updatedon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDON");
            entity.Property(e => e.Version)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("VERSION");
            entity.Property(e => e.Visible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("VISIBLE");
            entity.Property(e => e.Visitdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("VISITDATE");
            entity.Property(e => e.Weight)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("WEIGHT");
        });

        // NthChildInformation
        modelBuilder.Entity<NthChildInformation>(entity =>
        {
            entity.HasKey(e => e.Childid).HasName("SYS_C008215");
            entity.ToTable("NTH_CHILD_INFORMATION", "BISP_NASHUNUMA");
            entity.HasIndex(e => new { e.Nameeng, e.Gender, e.Childdob, e.Mothercnic, e.DuplicateKey }, "CHILD_INFO_UNIQUE_KEY").IsUnique();
            entity.HasIndex(e => new { e.Gender, e.Childid }, "IDX$$_00010007");
            entity.HasIndex(e => e.Mothercnic, "IDX$$_00010016");
            entity.HasIndex(e => new { e.Gender, e.Mothercnic }, "IDX$$_00010017");
            entity.Property(e => e.Childid)
                .HasColumnType("NUMBER")
                .HasColumnName("CHILDID");
            entity.Property(e => e.ApiReturnCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("API_RETURN_CODE");
            entity.Property(e => e.BeneTypeForBank)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BENE_TYPE_FOR_BANK");
            entity.Property(e => e.Bformno)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("BFORMNO");
            entity.Property(e => e.ChangeType)
                .HasColumnType("NUMBER")
                .HasColumnName("CHANGE_TYPE");
            entity.Property(e => e.Childage)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("CHILDAGE");
            entity.Property(e => e.Childdob)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("CHILDDOB");
            entity.Property(e => e.Childdobbymother)
                .IsRequired()
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("CHILDDOBBYMOTHER");
            entity.Property(e => e.ConsectivePayment)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("CONSECTIVE_PAYMENT");
            entity.Property(e => e.DbUser)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DB_USER");
            entity.Property(e => e.Disabilitytype)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("DISABILITYTYPE");
            entity.Property(e => e.DobFixed)
                .HasColumnType("DATE")
                .HasColumnName("DOB_FIXED");
            entity.Property(e => e.DuplicateKey)
                .HasColumnType("NUMBER")
                .HasColumnName("DUPLICATE_KEY");
            entity.Property(e => e.Education)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Enteredby)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ENTEREDBY");
            entity.Property(e => e.Enteredon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDON");
            entity.Property(e => e.Exitdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EXITDATE");
            entity.Property(e => e.Exitedby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("EXITEDBY");
            entity.Property(e => e.Exitedon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("EXITEDON");
            entity.Property(e => e.Exitstatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EXITSTATUS");
            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .HasColumnName("GENDER");
            entity.Property(e => e.Genderbymother)
                .IsRequired()
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("GENDERBYMOTHER");
            entity.Property(e => e.IsAdolescent)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IS_ADOLESCENT");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.IsPostDelivered)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IS_POST_DELIVERED");
            entity.Property(e => e.Isdisable)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISDISABLE");
            entity.Property(e => e.MmsEligible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("MMS_ELIGIBLE");
            entity.Property(e => e.Mothercnic)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("MOTHERCNIC");
            entity.Property(e => e.Nadraverified)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("NADRAVERIFIED");
            entity.Property(e => e.Nameeng)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NAMEENG");
            entity.Property(e => e.Nameur)
                .HasMaxLength(50)
                .HasColumnName("NAMEUR");
            entity.Property(e => e.ReferredFrom)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("REFERRED_FROM");
            entity.Property(e => e.Registeredquarter)
                .HasColumnType("NUMBER")
                .HasColumnName("REGISTEREDQUARTER");
            entity.Property(e => e.RegistrationBookNo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("REGISTRATION_BOOK_NO");
            entity.Property(e => e.SiteId)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_ID");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDBY");
            entity.Property(e => e.Updatedon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDON");
            entity.Property(e => e.VcId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("VC_ID");
            entity.Property(e => e.Visible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("VISIBLE");
        });

        // NthChildVisitDetail
        modelBuilder.Entity<NthChildVisitDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SYS_C008216");
            entity.ToTable("NTH_CHILD_VISIT_DETAIL", "BISP_NASHUNUMA");
            entity.HasIndex(e => e.Mothercnic, "IDX$$_0001001A");
            entity.HasIndex(e => e.Childid, "IDX$$_608000001");
            entity.HasIndex(e => e.Bformno, "IDX_BFORMNO");
            entity.HasIndex(e => new { e.Childid, e.Mothercnic }, "IDX_COMPOSITE_CHILDVISIT");
            entity.HasIndex(e => new { e.Childid, e.Mothercnic, e.Bformno, e.Quarterno }, "IDX_COMPOSITE_CHILDVISIT_ALL");
            entity.HasIndex(e => new { e.Childid, e.Quarterno, e.DuplicateKey }, "UQ_NTH_CHILD_VISIT").IsUnique();
            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.AccountTypeForBank)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ACCOUNT_TYPE_FOR_BANK");
            entity.Property(e => e.AmToBe)
                .HasColumnType("NUMBER")
                .HasColumnName("AM_TO_BE");
            entity.Property(e => e.AmountReasons)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AMOUNT_REASONS");
            entity.Property(e => e.AmountReceived)
                .HasColumnType("NUMBER")
                .HasColumnName("AMOUNT_RECEIVED");
            entity.Property(e => e.ApiStatusCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("API_STATUS_CODE");
            entity.Property(e => e.AppetitieTest)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("APPETITIE_TEST");
            entity.Property(e => e.Awareness)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("AWARENESS");
            entity.Property(e => e.BatchFromScanner)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BATCH_FROM_SCANNER");
            entity.Property(e => e.Batchnumber)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("BATCHNUMBER");
            entity.Property(e => e.Bcg)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("BCG");
            entity.Property(e => e.Bformno)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("BFORMNO");
            entity.Property(e => e.Breastfeeding)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BREASTFEEDING");
            entity.Property(e => e.Childid)
                .HasColumnType("NUMBER")
                .HasColumnName("CHILDID");
            entity.Property(e => e.Compliant)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("COMPLIANT");
            entity.Property(e => e.CompliantDate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("COMPLIANT_DATE");
            entity.Property(e => e.Complication)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("COMPLICATION");
            entity.Property(e => e.CompliedStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("COMPLIED_STATUS");
            entity.Property(e => e.DoctorReferral)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("DOCTOR_REFERRAL");
            entity.Property(e => e.DoctorRemarks)
                .HasMaxLength(2000)
                .IsUnicode(false)
                .HasColumnName("DOCTOR_REMARKS");
            entity.Property(e => e.DuplicateKey)
                .HasDefaultValueSql("1 ")
                .HasColumnType("NUMBER")
                .HasColumnName("DUPLICATE_KEY");
            entity.Property(e => e.Endserial)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ENDSERIAL");
            entity.Property(e => e.Enteredby)
                .IsRequired()
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDBY");
            entity.Property(e => e.Enteredon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ENTEREDON");
            entity.Property(e => e.FaaMarkForP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("FAA_MARK_FOR_P");
            entity.Property(e => e.Firstvisitdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FIRSTVISITDATE");
            entity.Property(e => e.FoodQuantityFromOther)
                .HasColumnType("NUMBER")
                .HasColumnName("FOOD_QUANTITY_FROM_OTHER");
            entity.Property(e => e.Height)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("HEIGHT");
            entity.Property(e => e.Ipv)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IPV");
            entity.Property(e => e.IsAnemic)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("IS_ANEMIC");
            entity.Property(e => e.IsManualUpdate)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("IS_MANUAL_UPDATE");
            entity.Property(e => e.Iseligibleforcmam)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ISELIGIBLEFORCMAM");
            entity.Property(e => e.Lockedafterpayment)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("LOCKEDAFTERPAYMENT");
            entity.Property(e => e.Lockedmessage)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("LOCKEDMESSAGE");
            entity.Property(e => e.Lockedmessageactual)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("LOCKEDMESSAGEACTUAL");
            entity.Property(e => e.Measles1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("MEASLES1");
            entity.Property(e => e.Measles2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("MEASLES2");
            entity.Property(e => e.MmsSoruce)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MMS_SORUCE");
            entity.Property(e => e.Mothercnic)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("MOTHERCNIC");
            entity.Property(e => e.Muac)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MUAC");
            entity.Property(e => e.Muacstatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MUACSTATUS");
            entity.Property(e => e.Odema)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("ODEMA");
            entity.Property(e => e.OdemaGrade)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ODEMA_GRADE");
            entity.Property(e => e.Opv0)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("OPV0");
            entity.Property(e => e.Opv1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("OPV1");
            entity.Property(e => e.Opv2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("OPV2");
            entity.Property(e => e.Opv3)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("OPV3");
            entity.Property(e => e.PaymentStatusBefore)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PAYMENT_STATUS_BEFORE");
            entity.Property(e => e.Paymentby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PAYMENTBY");
            entity.Property(e => e.Paymentdatetime)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("PAYMENTDATETIME");
            entity.Property(e => e.Penta1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PENTA1");
            entity.Property(e => e.Penta2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PENTA2");
            entity.Property(e => e.Penta3)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PENTA3");
            entity.Property(e => e.PeriodFromApp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PERIOD_FROM_APP");
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("PHONE_NO");
            entity.Property(e => e.Pictureurl)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("PICTUREURL");
            entity.Property(e => e.Pnc)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PNC");
            entity.Property(e => e.Pneumo1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PNEUMO1");
            entity.Property(e => e.Pneumo2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PNEUMO2");
            entity.Property(e => e.Pneumo3)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("PNEUMO3");
            entity.Property(e => e.Quartercode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("QUARTERCODE");
            entity.Property(e => e.Quarterno)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("QUARTERNO");
            entity.Property(e => e.ReferralReason)
                .HasColumnType("NUMBER")
                .HasColumnName("REFERRAL_REASON");
            entity.Property(e => e.RegistrationBookNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REGISTRATION_BOOK_NO");
            entity.Property(e => e.ReportingCode)
                .HasColumnType("NUMBER")
                .HasColumnName("REPORTING_CODE");
            entity.Property(e => e.SamCaseReferredTo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SAM_CASE_REFERRED_TO");
            entity.Property(e => e.SamCaseType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SAM_CASE_TYPE");
            entity.Property(e => e.Screening)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("SCREENING");
            entity.Property(e => e.Selfimmunization)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("SELFIMMUNIZATION");
            entity.Property(e => e.SiteId)
                .HasColumnType("NUMBER")
                .HasColumnName("SITE_ID");
            entity.Property(e => e.SiteName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SITE_NAME");
            entity.Property(e => e.SnfType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SNF_TYPE");
            entity.Property(e => e.Snfdistributed)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SNFDISTRIBUTED");
            entity.Property(e => e.Snfutilized)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SNFUTILIZED");
            entity.Property(e => e.Startserial)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("STARTSERIAL");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("STATUS");
            entity.Property(e => e.TranchEnroll)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("TRANCH_ENROLL");
            entity.Property(e => e.Treatementstartdate)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("TREATEMENTSTARTDATE");
            entity.Property(e => e.Treatmentstarted)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("TREATMENTSTARTED");
            entity.Property(e => e.Treatmentstatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TREATMENTSTATUS");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDBY");
            entity.Property(e => e.Updatedon)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("UPDATEDON");
            entity.Property(e => e.Version)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("VERSION");
            entity.Property(e => e.Visible)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("VISIBLE");
            entity.Property(e => e.Visitdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("VISITDATE");
            entity.Property(e => e.Weight)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("WEIGHT");
            entity.Property(e => e.Womenheight)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("WOMENHEIGHT");
            entity.Property(e => e.Womenmuac)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("WOMENMUAC");
            entity.Property(e => e.Womenweight)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("WOMENWEIGHT");
        });


        modelBuilder.Entity<NthProvince>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NTH_PROVINCES", "BISP_NASHUNUMA");

            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Provcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("PROVCODE");
            entity.Property(e => e.Province)
                .HasMaxLength(26)
                .IsUnicode(false)
                .HasColumnName("PROVINCE");
        });
        modelBuilder.Entity<NthDistrict>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NTH_DISTRICTS", "BISP_NASHUNUMA");

            entity.Property(e => e.Distcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("DISTCODE");
            entity.Property(e => e.District)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Provcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("PROVCODE");
        });
        modelBuilder.Entity<NthTehsil>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NTH_TEHSILS", "BISP_NASHUNUMA");

            entity.Property(e => e.Distcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("DISTCODE");
            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Tehsil)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("TEHSIL");
            entity.Property(e => e.Tehsilcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("TEHSILCODE");
        });
        modelBuilder.Entity<NthUc>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NTH_UCS", "BISP_NASHUNUMA");

            entity.Property(e => e.Id)
                .HasColumnType("NUMBER")
                .HasColumnName("ID");
            entity.Property(e => e.Tehsilcode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("TEHSILCODE");
            entity.Property(e => e.Uc)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("UC");
            entity.Property(e => e.Uccode)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("UCCODE");
            entity.Property(e => e.Ucno)
                .HasColumnType("NUMBER(38)")
                .HasColumnName("UCNO");
            entity.Property(e => e.Uctype)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("UCTYPE");
        });

        OnModelCreatingPartial(modelBuilder);

       

    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
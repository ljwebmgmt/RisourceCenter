using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;

namespace newrisourcecenter.Models
{
    public class RisourceCenterContext : IdentityDbContext<ApplicationUser>
    {
        public RisourceCenterContext()
            : base("name=DefaultConnection")
        {
            this.Database.CommandTimeout = 5 * 60;
        }

        public virtual DbSet<Nav1ViewModel> Nav1ViewModel { get; set; }
        public virtual DbSet<Nav2ViewModel> Nav2ViewModel { get; set; }
        public virtual DbSet<UserViewModel> UserViewModels { get; set; }
        public virtual DbSet<UserTheme> UserThemes { get; set; }
        public virtual DbSet<partnerTypeViewModel> partnerTypeViewModels { get; set; }
        public virtual DbSet<partnerIndustryViewModel> partnerIndustryViewModels { get; set; }
        public virtual DbSet<partnerProductsViewModel> partnerProductsViewModels { get; set; }
        public virtual DbSet<partnerCompanyViewModel> partnerCompanyViewModels { get; set; }
        public virtual DbSet<partnerLocationViewModel> partnerLocationViewModels { get; set; }
        public virtual DbSet<Nav3ViewModel> Nav3ViewModel { get; set; }
        public virtual DbSet<RiSourcesViewModel> RiSourcesViewModels { get; set; }
        public virtual DbSet<risourcesCatViewModel> risourcesCatViewModels { get; set; }
        public virtual DbSet<LitRequestedViewModel> LitRequestedViewModels { get; set; }
        public virtual DbSet<risourcesTypeViewModel> risourcesTypeViewModels { get; set; }
        public virtual DbSet<SalesCommViewModel> SalesCommViewModels { get; set; }
        public virtual DbSet<LiteratureViewModel> LiteratureViewModels { get; set; }
        public virtual DbSet<AspNetRoleViewModel> AspNetRoleViewModels { get; set; }
        public virtual DbSet<partnerStockCheckViewModel> partnerStockCheckViewModels { get; set; }
        public virtual DbSet<LocalizationModel> LocalizationModels { get; set; }
        public virtual DbSet<LabelsModel> LabelsModels { get; set; }
        public virtual DbSet<RittalUniversityViewModels> rittalUniversityViewModels { get; set; }
        public virtual DbSet<ReturnTools> returnTools { get; set; }
        public virtual DbSet<ReturnToolExtentions> returnToolExtentions { get; set; }
        public virtual DbSet<ReturnToolFiles> returnToolFile { get; set; }
        public virtual DbSet<ReturnToolActionLogs> returnToolActionLog { get; set; }
        public virtual DbSet<PartnerApplicationViewModel> PartnerApplicationViewModels { get; set; }
        public virtual DbSet<Announcement_logViewModel> announcement_logViewModels { get; set; }
        public virtual DbSet<AnnouncementsViewModel> AnnouncementsViewModels { get; set; }
        public virtual DbSet<RFQNewViewModel> RFQNewViewModels { get; set; }
        public virtual DbSet<RFQNewViewModelExt> RFQNewViewModelExts { get; set; }
        public virtual DbSet<RFQ_New_File> RFQNewFiles { get; set; }
        public virtual DbSet<RFQ_New_Action_LogViewModel> RFQ_New_Action_LogViewModels { get; set; }
        public virtual DbSet<RFQViewModel> RFQViewModels { get; set; }
        public virtual DbSet<RFQViewModelExt> RFQViewModelExts { get; set; }
        public virtual DbSet<RFQ_File> RFQFiles { get; set; }
        public virtual DbSet<RFQ_Action_LogViewModel> RFQ_Action_LogViewModels { get; set; }
        public virtual DbSet<RFQ_Parts_InstalledViewModel> RFQ_Parts_InstalledViewModels { get; set; }
        public virtual DbSet<RFQ_Parts_ShippedViewModel> RFQ_Parts_ShippedViewModels { get; set; }
        public virtual DbSet<SiteApprovers> SiteApprovers { get; set; }
        public virtual DbSet<MDFViewModel> MDFViewModels { get; set; }
        public virtual DbSet<MDFfiles> mdfFiles { get; set; }
        public virtual DbSet<MDFPinnacleViewModel> MDFPinnacleViewModels { get; set; }
        public virtual DbSet<MDFPinnaclefiles> mdfPinnacleFiles { get; set; }
        public virtual DbSet<SRPViewModel> SRPViewModels { get; set; }
        public virtual DbSet<salesRequestFile> SalesRequestFiles { get; set; }
        public virtual DbSet<salesRequestApprovers> salesRequestApprovers { get; set; }
        public virtual DbSet<salesRequestAdditionalInfo> salesRequestAdditionalInfos { get; set; }
        public virtual DbSet<salesRequestActionLog> salesRequestActionLogs { get; set; }
        public virtual DbSet<emailtrackerViewModel> emailtrackerViewModels { get; set; }
        public virtual DbSet<SPAViewModels> SPAViewModels { get; set; }
        public virtual DbSet<SPAItemViewModel> SPAItemViewModels { get; set; }
        public virtual DbSet<SPASalesRepsViewModel> SPA_SalesRepsViewModels { get; set; }
        public virtual DbSet<SPA_FIlesViewModel> SPA_FIlesViewModels { get; set; }
        public virtual DbSet<Large_enclosureFmd> Large_enclosureFmds { get; set; }
        public virtual DbSet<Large_enclosurePricing> Large_enclosurePricings { get; set; }
        public virtual DbSet<Large_enclosureTs8> Large_enclosureTs8s { get; set; }
        public virtual DbSet<LargeEnclosureMyaccessory> LargeEnclosureMyaccessories { get; set; }
        public virtual DbSet<Config> LargeEnclosureMyconfigs { get; set; }
        public virtual DbSet<countriesViewModel> countriesViewModels { get; set; }
        public virtual DbSet<datastateViewModel> datastateViewModels { get; set; }
        public virtual DbSet<SPAAccountManager> SPAAccountManagers { get; set; }
        public virtual DbSet<SPATerritoryCode> SPATerritoryCodes { get; set; }
        public virtual DbSet<SkusViewModel> SkusViewModels { get; set; }
        public virtual DbSet<SPAMaterialMasterViewModel> SPAMaterialMasterViewModels { get; set; }
        public virtual DbSet<SPAIntostockMultiplierViewModel> SPAIntostockMultiplierViewModels { get; set; }
        public virtual DbSet<SPANotesViewModels> SPANotesViewModels { get; set; }
        public virtual DbSet<WebshopConnectViewModel> WebshopConnectViewModels { get; set; }
        public virtual DbSet<SPARebatesViewModel> SPARebatesViewModels { get; set; }
        public virtual DbSet<SPARebatesItemsViewModel> SPARebatesItemsViewModels { get; set; }
        public virtual DbSet<RiSourcesCarts> RiSourcesCarts { get; set; }
        public virtual DbSet<RFQRASViewModel> RFQRASViewModels { get; set; }
        public virtual DbSet<PMQuoteViewModel> PMQuoteViewModels { get; set; }
    }
}
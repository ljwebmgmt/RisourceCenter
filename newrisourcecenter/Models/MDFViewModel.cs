using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    [Table("mdf_main")]
    public class MDFViewModel
    {
        [Display(Name = "Request Number")]
        [Key]
        public long mdf_ID { get; set; }
        [Display(Name = "Submitter")]
        public Nullable<long> mdf_user { get; set; }
        [Display(Name = "SAP Account")]
        public Nullable<long> mdf_SAP { get; set; }
        [Display(Name = "Company Name ")]
        public Nullable<long> mdf_comp { get; set; }
        [Display(Name = "Activity Title")]
        public string mdf_title { get; set; }
        [Display(Name = "Activity Description")]
        public string mdf_desc { get; set; }
        [Display(Name = "Location")]
        public Nullable<long> mdf_loc { get; set; }
        [Display(Name = "Estimated Cost of Activity")]
        public Nullable<double> mdf_totalCost { get; set; }
        [Display(Name = "MDF Amount Requested")]
        public Nullable<double> mdf_mdfCost { get; set; }
        [Display(Name = "Expected Activity Date")]
        public Nullable<System.DateTime> mdf_date { get; set; }
        [Display(Name = "Request Type")]
        public Nullable<int> mdf_type { get; set; }
        [Display(Name = "Status")]
        public Nullable<int> mdf_status { get; set; }
        [Display(Name = "Approval Comments")]
        public string mdf_comments { get; set; }
        [Display(Name = "Completion Comments")]
        public string mdf_comments2 { get; set; }
        [Display(Name = "User Comments")]
        public string mdf_comments3 { get; set; }
        [Display(Name = "Request Date")]
        public Nullable<System.DateTime> mdf_requestDate { get; set; }
        [Display(Name = "Review Date")]
        public Nullable<System.DateTime> mdf_reviewDate { get; set; }
        [Display(Name = "Validation Date")]
        public Nullable<System.DateTime> mdf_validationDate { get; set; }
        [Display(Name = "Credit Issue Date")]
        public Nullable<System.DateTime> mdf_creditIssueDate { get; set; }
        [Display(Name = "Approved Amount (MDF)")]
        public Nullable<double> mdf_approvedAmt { get; set; }
        [Display(Name = "Approved Amount (MKT)")]
        public Nullable<double> mdf_approvedAmt_mkt { get; set; }
        [Display(Name = "Completed Amount (MDF)")]
        public Nullable<double> mdf_validatedAmt { get; set; }
        [Display(Name = "Completed Amount (MKT)")]
        public Nullable<double> mdf_validatedAmt_mkt { get; set; }
        [Display(Name = "Credit Memo Number")]
        public string mdf_creditMemoNum { get; set; }
        [Display(Name = "Accounting Comments")]
        public string mdf_accountingInstruct { get; set; }
        [Display(Name = "Archive Year")]
        public Nullable<int> archive_year { get; set; }
        [Display(Name = "Request Type")]        public string mdf_requestType { get; set; }
        [NotMapped]
        public IQueryable<partnerLocationViewModel> partner_loc { get; set; }
        [NotMapped]
        public string company { get; set; }
        [NotMapped]
        public string topdesc { get; set; }
        [NotMapped]
        public IList<MdfParts> mdf_parts { get; set; }
        [NotMapped]
        public double? comp_MDF_amount { get; set; }
        [NotMapped]
        public string password { get; set; }
        [NotMapped]
        public string usr_fName { get; set; }
        [NotMapped]
        public string usr_lName { get; set; }
        [NotMapped]
        public string usr_phone { get; set; }
        [NotMapped]
        [Display(Name = "User Email")]
        public string usr_email { get; set; }
        [NotMapped]
        public IEnumerable<MDFViewModel> mdf_main { get; set; }
        [NotMapped]
        public string request_type { get; set; }
        [NotMapped]
        public string requester { get; set; }
        [NotMapped]
        [Display(Name = "Attach files: ")]
        public IEnumerable<HttpPostedFileBase> fileupload { get; set; }
        [NotMapped]
        public IEnumerable<MDFfiles> list_mdf_files { get; set; }
        [NotMapped]
        public int mdf_file_type { get; set; }
        [NotMapped]
        public List<partnerCompanyViewModel> list_comp { get; set; }
        [NotMapped]
        public List<MdfParts> company_metrics { get; set; }
        [NotMapped]
        public List<MdfParts> company_metrics_mkt { get; set; }
        [NotMapped]
        public string region { get; set; }
        [NotMapped]
        public string usr_city { get; set; }
        [NotMapped]
        public string usr_add1 { get; set; }
        [NotMapped]
        public string usr_zip { get; set; }
        [NotMapped]
        public string usr_state { get; set; }
        [Display(Name = "Cost Center")]
        public string cost_center { get; set; }
        [NotMapped]
        public int old_status { get; set; }
        [Display(Name = "Assigned RCM")]
        public string rcm {  get; set; }
        [NotMapped]
        public List<string> rcmUsers { get; set; }
        [NotMapped]
        public Dictionary<string,string> rcmNames { get; set; }
        [NotMapped]
        [Display(Name = "Total MDF Funds Before Request Approved")]
        public double mdf_remainingAmt { get; set; }
        [NotMapped]
        [Display(Name = "Total MDF Funds After Request Approved")]
        public double mdf_remainingAmtWithPending { get; set; }
        [NotMapped]
        [Display(Name = "Total MKT Funds Before Request Approved")]
        public double mkt_remainingAmt { get; set; }
        [NotMapped]
        [Display(Name = "Total MKT Funds After Request Approved")]
        public double mkt_remainingAmtWithPending { get; set; }
    }

    [Table("mdf_pinnacle_main")]
    public class MDFPinnacleViewModel
    {
        [Display(Name = "Request Number")]
        [Key]
        public long mdf_ID { get; set; }
        [Display(Name = "Submitter")]
        public Nullable<long> mdf_user { get; set; }
        [Display(Name = "SAP Account")]
        public Nullable<long> mdf_SAP { get; set; }
        [Display(Name = "Company Name ")]
        public Nullable<long> mdf_comp { get; set; }
        [Display(Name = "Title")]
        public string mdf_title { get; set; }
        [Display(Name = "Description")]
        public string mdf_desc { get; set; }
        [Display(Name = "Location")]
        public Nullable<long> mdf_loc { get; set; }
        [Display(Name = "Total Cost")]
        public Nullable<double> mdf_totalCost { get; set; }
        [Display(Name = "MDF Amount")]
        public Nullable<double> mdf_mdfCost { get; set; }
        [Display(Name = "Expected Date")]
        public Nullable<System.DateTime> mdf_date { get; set; }
        [Display(Name = "Request Type")]
        public Nullable<int> mdf_type { get; set; }
        [Display(Name = "Status")]
        public Nullable<int> mdf_status { get; set; }
        [Display(Name = "Approval Comments")]
        public string mdf_comments { get; set; }
        [Display(Name = "Completion Comments")]
        public string mdf_comments2 { get; set; }
        [Display(Name = "User Comments")]
        public string mdf_comments3 { get; set; }
        [Display(Name = "Request Date")]
        public Nullable<System.DateTime> mdf_requestDate { get; set; }
        [Display(Name = "Review Date")]
        public Nullable<System.DateTime> mdf_reviewDate { get; set; }
        [Display(Name = "Validation Date")]
        public Nullable<System.DateTime> mdf_validationDate { get; set; }
        [Display(Name = "Credit Issue Date")]
        public Nullable<System.DateTime> mdf_creditIssueDate { get; set; }
        [Display(Name = "Approved Amount")]
        public Nullable<double> mdf_approvedAmt { get; set; }
        [Display(Name = "Completed Amount")]
        public Nullable<double> mdf_validatedAmt { get; set; }
        [Display(Name = "Credit Memo Number")]
        public string mdf_creditMemoNum { get; set; }
        [Display(Name = "Accounting Comments")]
        public string mdf_accountingInstruct { get; set; }
        [Display(Name = "Archive Year")]
        public Nullable<int> archive_year { get; set; }
        [Display(Name = "Request Type")]        public string mdf_requestType { get; set; }
        [NotMapped]
        public IQueryable<partnerLocationViewModel> partner_loc { get; set; }
        [NotMapped]
        public string company { get; set; }
        [NotMapped]
        public string topdesc { get; set; }
        [NotMapped]
        public IList<MdfParts> mdf_parts { get; set; }
        [NotMapped]
        public double? comp_MDF_amount { get; set; }
        [NotMapped]
        public string password { get; set; }
        [NotMapped]
        public string usr_fName { get; set; }
        [NotMapped]
        public string usr_lName { get; set; }
        [NotMapped]
        public string usr_phone { get; set; }
        [NotMapped]
        [Display(Name = "User Email")]
        public string usr_email { get; set; }
        [NotMapped]
        public IEnumerable<MDFPinnacleViewModel> mdf_main { get; set; }
        [NotMapped]
        public string request_type { get; set; }
        [NotMapped]
        public string requester { get; set; }
        [NotMapped]
        [Display(Name = "Attach files: ")]
        public IEnumerable<HttpPostedFileBase> fileupload { get; set; }
        [NotMapped]
        public IEnumerable<MDFPinnaclefiles> list_mdf_files { get; set; }
        [NotMapped]
        public int mdf_file_type { get; set; }
        [NotMapped]
        public List<partnerCompanyViewModel> list_comp { get; set; }
        [NotMapped]
        public List<MdfParts> company_metrics { get; set; }
        [NotMapped]
        public string region { get; set; }
        [NotMapped]
        public string usr_city { get; set; }
        [NotMapped]
        public string usr_add1 { get; set; }
        [NotMapped]
        public string usr_zip { get; set; }
        [NotMapped]
        public string usr_state { get; set; }
    }

    public class MdfParts
    {
        //Training Parts
        public Nullable<double> Pending_Training_total { get; set; }
        public Nullable<double> Approved_Training_total { get; set; }
        public Nullable<double> Completed_Training_total { get; set; }
        public Nullable<double> SpentTraining { get; set; }
        public Nullable<double> Credit_Training_total { get; set; }
        //Promotional Events Parts
        public Nullable<double> Pending_PromoEv_total { get; set; }
        public Nullable<double> Approved_PromoEv_total { get; set; }
        public Nullable<double> Completed_PromoEv_total { get; set; }
        public Nullable<double> SpentPromoEv { get; set; }
        public Nullable<double> Credit_PromoEv_total { get; set; }
        //Promotional Activities Parts
        public Nullable<double> Pending_PromoAc_total { get; set; }
        public Nullable<double> Approved_PromoAc_total { get; set; }
        public Nullable<double> Completed_PromoAc_total { get; set; }
        public Nullable<double> SpentPromoAc { get; set; }
        public Nullable<double> Credit_PromoAc_total { get; set; }
        //Merchandise Parts
        public Nullable<double> Pending_Merchandise_total { get; set; }
        public Nullable<double> Approved_Merchandise_total { get; set; }
        public Nullable<double> Completed_Merchandise_total { get; set; }
        public Nullable<double> SpentMerchandise { get; set; }
        public Nullable<double> Credit_Merchandise_total { get; set; }
        //Display Products Parts
        public Nullable<double> Pending_DP_total { get; set; }
        public Nullable<double> Approved_DP_total { get; set; }
        public Nullable<double> Completed_DP_total { get; set; }
        public Nullable<double> SpentDP { get; set; }
        public Nullable<double> Credit_DP_total { get; set; }
        //Other Products Parts
        public Nullable<double> Pending_OP_total { get; set; }
        public Nullable<double> Approved_OP_total { get; set; }
        public Nullable<double> Completed_OP_total { get; set; }
        public Nullable<double> SpentOP { get; set; }
        public Nullable<double> Credit_OP_total { get; set; }
        //RAS Parts
        public Nullable<double> Pending_RAS_total { get; set; }
        public Nullable<double> Approved_RAS_total { get; set; }
        public Nullable<double> Completed_RAS_total { get; set; }
        public Nullable<double> SpentRAS { get; set; }
        public Nullable<double> Credit_RAS_total { get; set; }
        //Tradeshow Parts
        public Nullable<double> Pending_Tradeshow_total { get; set; }
        public Nullable<double> Approved_Tradeshow_total { get; set; }
        public Nullable<double> Completed_Tradeshow_total { get; set; }
        public Nullable<double> SpentTradeshow { get; set; }
        public Nullable<double> Credit_Tradeshow_total { get; set; }
        //SPIF Parts
        public Nullable<double> Pending_SPIF_total { get; set; }
        public Nullable<double> Approved_SPIF_total { get; set; }
        public Nullable<double> Completed_SPIF_total { get; set; }
        public Nullable<double> SpentSPIF { get; set; }
        public Nullable<double> Credit_SPIF_total { get; set; }
        //Total Available    
        public Nullable<double> totalTrainingAva { get; set; }
        public Nullable<double> totalPromoeAva { get; set; }
        public Nullable<double> totalPromoaAva { get; set; }
        public Nullable<double> totalMerchAva { get; set; }
        public Nullable<double> totalDpAva { get; set; }
        public Nullable<double> totalOtherAva { get; set; }
        public Nullable<double> totalRASAva { get; set; }
        public Nullable<double> totalTradeshowAva { get; set; }
        public Nullable<double> totalSPIFAva { get; set; }
        //Calculating total Pending, Approved, Complete
        public Nullable<double> totalPending { get; set; }
        public Nullable<double> totalApproved { get; set; }
        public Nullable<double> totalComplete { get; set; }
        public Nullable<double> totalCredit { get; set; }
        //Total Utilized and Available
        public Nullable<double> totalMDFUtilized { get; set; }
        public Nullable<double> PercentageUtilized { get; set; }
        public Nullable<double> totalMDFAva { get; set; }
        public Nullable<double> totalMDFAvaWithPending { get; set; }
        public Nullable<double> PercentageTotalMDFAva { get; set; }
        //Percentage Training
        public Nullable<double> PercentageTrainingUsed { get; set; }
        public Nullable<double> PercentageTraining { get; set; }
        public Nullable<double> PercentageTrainingValid { get; set; }
        //Percentage Promotional Events
        public Nullable<double> PercentagePromotionalEventUsed { get; set; }
        public Nullable<double> PercentagePromotionalEvent { get; set; }
        public Nullable<double> PercentagePromotionalValid { get; set; }
        //Percentage Promotional Activities
        public Nullable<double> PercentagePromotionalActivityUsed { get; set; }
        public Nullable<double> PercentagePromotionalActivity { get; set; }
        public Nullable<double> PercentagePromotionalActivityValid { get; set; }
        //Percentage Mechandise
        public Nullable<double> PercentageMerchandiseUsed { get; set; }
        public Nullable<double> PercentageMerchandise { get; set; }
        public Nullable<double> PercentageMerchandiseValid { get; set; }
        //Percentage Display Products
        public Nullable<double> PercentageDisplayProductsUsed { get; set; }
        public Nullable<double> PercentageDisplayProducts { get; set; }
        public Nullable<double> PercentageDisplayProductsValid { get; set; }
        //Percentage Other Products
        public Nullable<double> PercentageOtherProductsUsed { get; set; }
        public Nullable<double> PercentageOtherProducts { get; set; }
        public Nullable<double> PercentageOtherProductsValid { get; set; }
        //Percentage RAS
        public Nullable<double> PercentageRASUsed { get; set; }
        public Nullable<double> PercentageRAS { get; set; }
        public Nullable<double> PercentageRASValid { get; set; }
        //Percentage Tradeshow
        public Nullable<double> PercentageTradeshowUsed { get; set; }
        public Nullable<double> PercentageTradeshow { get; set; }
        public Nullable<double> PercentageTradeshowValid { get; set; }
        //Percentage SPIF
        public Nullable<double> PercentageSPIFUsed { get; set; }
        public Nullable<double> PercentageSPIF { get; set; }
        public Nullable<double> PercentageSPIFValid { get; set; }
    }

    //List Training Models
    public class PendingTraining
    {
        public double? value { get; set; }
    }

    public class ApprovedTraining
    {
        public double? value { get; set; }
    }

    public class CompletedTraining
    {
        public double? value { get; set; }
    }

    public class CreditTraining
    {
        public double? value { get; set; }
    }

    //List Promotional Events
    public class PendingPromEvents
    {
        public double? value { get; set; }
    }

    public class ApprovedPromEvents
    {
        public double? value { get; set; }
    }

    public class CompletedPromEvents
    {
        public double? value { get; set; }
    }

    public class CreditPromEvents
    {
        public double? value { get; set; }
    }

    //List Promotional Activities
    public class PendingPromAcitivies
    {
        public double? value { get; set; }
    }

    public class ApprovedPromAcitivies
    {
        public double? value { get; set; }
    }

    public class CompletedPromAcitivies
    {
        public double? value { get; set; }
    }

    public class CreditPromAcitivies
    {
        public double? value { get; set; }
    }

    //List Merchandise
    public class PendingMerchandise
    {
        public double? value { get; set; }
    }

    public class ApprovedMerchandise
    {
        public double? value { get; set; }
    }

    public class CompletedMerchandise
    {
        public double? value { get; set; }
    }

    public class CreditMerchandise
    {
        public double? value { get; set; }
    }

    //List Display Product
    public class PendingDisplayProduct
    {
        public double? value { get; set; }
    }

    public class ApprovedDisplayProduct
    {
        public double? value { get; set; }
    }

    public class CompletedDisplayProduct
    {
        public double? value { get; set; }
    }

    public class CreditDisplayProduct
    {
        public double? value { get; set; }
    }

    //List Other Product
    public class PendingOtherProduct
    {
        public double? value { get; set; }
    }

    public class ApprovedOtherProduct
    {
        public double? value { get; set; }
    }

    public class CompletedOtherProduct
    {
        public double? value { get; set; }
    }

    public class CreditOtherProduct
    {
        public double? value { get; set; }
    }

    public class PendingActivity
    {
        public double? value { get; set; }
    }

    public class ApprovedActivity
    {
        public double? value { get; set; }
    }

    public class CompletedActivity
    {
        public double? value { get; set; }
    }

    public class CreditActivity
    {
        public double? value { get; set; }
    }
    [Table("mdf_file")]
    public class MDFfiles
    {
        [Key]
        public long mdf_file_ID { get; set; }
        public Nullable<long> mdf_ID { get; set; }
        public string mdf_file_name { get; set; }
        public Nullable<byte> mdf_file_type { get; set; }
    }

    [Table("mdf_pinnacle_file")]
    public class MDFPinnaclefiles
    {
        [Key]
        public long mdf_file_ID { get; set; }
        public Nullable<long> mdf_ID { get; set; }
        public string mdf_file_name { get; set; }
        public Nullable<byte> mdf_file_type { get; set; }
    }

    public class MDFUsers
    {
        public int usr_ID { get; set; }
        public long? comp_ID { get; set; }
        public string fullName { get; set; }
        public string company { get; set; }
        public string userstatus { get; set; }
        public string adminstatus { get; set; }
    }


}
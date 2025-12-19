using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    [Table("partnerType")]
    public class partnerTypeViewModel
    {
        [Key]
        public int pt_ID { get; set; }
        [Display(Name = "Partner Type")]
        public string pt_type { get; set; }
    }

    [Table("partnerIndustry")]
    public class partnerIndustryViewModel
    {
        [Key]
        public int pi_ID { get; set; }
        [Display(Name = "Partner Industry")]
        public string pi_industry { get; set; }
    }

    [Table("partnerProducts")]
    public class partnerProductsViewModel
    {
        [Key]
        public int pp_ID { get; set; }
        [Display(Name = "Partner Products")]
        public string pp_product { get; set; }
    }

    [Table("partnerCompany")]
    public class partnerCompanyViewModel
    {
        [Key]
        public long comp_ID { get; set; }
        [Display(Name = "Company Name")]
        public string comp_name { get; set; }
        [Display(Name = "Company Industry")]
        public Nullable<byte> comp_industry { get; set; }
        public IEnumerable<partnerIndustry> list_industry { get; set; }
        [Display(Name = "Company Type")]
        public Nullable<byte> comp_type { get; set; }
        public IEnumerable<partnerType> list_Type { get; set; }
        [Display(Name = "Company Level")]
        public Nullable<byte> comp_level { get; set; }
        [Display(Name = "Company Products")]
        public string comp_products { get; set; }
        public IEnumerable<partnerProduct> list_products { get; set; }
        [Display(Name = "Stock Availability & Order Status:")]
        public Nullable<byte> comp_SAP { get; set; }
        [Display(Name = "Point of Sales")]
        public Nullable<byte> comp_POS { get; set; }
        [Display(Name = "Special Pricing Agreement")]
        public Nullable<byte> comp_SPA { get; set; }
        [Display(Name = "Project Registration")]
        public Nullable<byte> comp_project_reg { get; set; }
        [Display(Name = "Market Development Fund Program")]
        public Nullable<byte> comp_MDF { get; set; }
        [Display(Name = "Total MDF Funds")]
        public Nullable<double> comp_MDF_amount { get; set; }
        [Display(Name = "Total MKT Funds")]
        public Nullable<double> comp_MKT_Limit { get; set; }
        [Display(Name = "Funds For Training")]
        public Nullable<double> comp_MDF_tLimit { get; set; }
        [Display(Name = "Funds For Promotional Activity")]
        public Nullable<double> comp_MDF_aLimit { get; set; }
        [Display(Name = "Funds For Other Activity")]
        public Nullable<double> comp_MDF_oLimit { get; set; }
        [Display(Name = "Funds For Promotional Events")]
        public Nullable<double> comp_MDF_eLimit { get; set; }
        [Display(Name = "Funds For Display Products")]
        public Nullable<double> comp_MDF_dLimit { get; set; }
        [Display(Name = "Funds For Mechandise")]
        public Nullable<double> comp_MDF_mLimit { get; set; }
        [Display(Name = "FileExpress")]
        public Nullable<byte> comp_FX { get; set; }
        [Display(Name = "Company Active")]
        public Nullable<byte> comp_active { get; set; }
        [Display(Name = "Date Created")]
        public Nullable<System.DateTime> comp_dateCreated { get; set; }
        [Display(Name = "Date Updated")]
        public Nullable<System.DateTime> comp_dateUpdated { get; set; }
        [Display(Name = "Created By")]
        public Nullable<long> comp_createdBy { get; set; }
        [Display(Name = "Updated By")]
        public Nullable<long> comp_updatedBy { get; set; }
        [Display(Name = "Old ID")]
        public Nullable<long> old_ID { get; set; }
        [Display(Name = "RiCRM")]
        public Nullable<int> comp_RiCRM { get; set; }
        [Display(Name = "Company Region")]
        public string comp_region { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_regions { get; set; }
        [NotMapped]
        public int? count_locations { get; set; }
        [NotMapped]
        public int? count_users { get; set; }
        [NotMapped]
        public int? count_jigsaw { get; set; }
        [NotMapped]
        public string full_name { get; set; }
        [NotMapped]
        [Display(Name = "Total MDF Funds Remaining")]
        public string MDF_remaining { get; set; }
        [Display(Name = "Available for Deal Registration?")]
        public Nullable<byte> bid_registration { get; set; }
        [Display(Name = "IT Territory Manager")]
        public string it_territory_manager { get; set; }
        [Display(Name = "IT Director")]
        public string it_director { get; set; }
        [Display(Name = "General Manager")]
        public string general_manager { get; set; }
        [NotMapped]
        [Display(Name = "Total MKT Funds Remaining")]
        public string MKT_remaining { get; set; }
    }

    [Table("partnerLocation")]
    public partial class partnerLocationViewModel
    {
        [Key]
        public long loc_ID { get; set; }
        [Display(Name = "Partner Company")]
        public Nullable<long> comp_ID { get; set; }
        [Display(Name = "Location Name")]
        public string loc_name { get; set; }
        [Display(Name = "Address 1")]
        public string loc_add1 { get; set; }
        [Display(Name = "Address 2")]
        public string loc_add2 { get; set; }
        [Display(Name = "City")]
        public string loc_city { get; set; }
        [Display(Name = "State")]
        public string loc_state { get; set; }
        [Display(Name = "Zip")]
        public string loc_zip { get; set; }
        [Display(Name = "Phone")]
        public string loc_phone { get; set; }
        [Display(Name = "Fax")]
        public string loc_fax { get; set; }
        [Display(Name = "Website")]
        public string loc_web { get; set; }
        [Display(Name = "Email")]
        public string loc_email { get; set; }
        [Display(Name = "Lat")]
        public Nullable<double> loc_lat { get; set; }
        [Display(Name = "Lng")]
        public Nullable<double> loc_lon { get; set; }
        [Display(Name = "Dealer Status")]
        public Nullable<byte> loc_dealor_status { get; set; }
        [Display(Name = "Show Address")]
        public Nullable<byte> loc_show_address { get; set; }
        [Display(Name = "SAP Account")]
        public Nullable<long> loc_SAP_account { get; set; }
        [Display(Name = "SAP Password")]
        public string loc_SAP_password { get; set; }
        [Display(Name = "Online Shop Account")]
        public string loc_Webshop_account { get; set; }
        [Display(Name = "Online Shop Password")]
        public string loc_Webshop_password { get; set; }
        [Display(Name = "Date Created")]
        public Nullable<System.DateTime> loc_dateCreated { get; set; }
        [Display(Name = "Date Updated")]
        public Nullable<System.DateTime> loc_dateUpdated { get; set; }
        [Display(Name = "Created By")]
        public Nullable<long> loc_createdBy { get; set; }
        [Display(Name = "Updated By")]
        public Nullable<long> loc_updatedBy { get; set; }
        [Display(Name = "Old Location ID")]
        public Nullable<long> old_locID { get; set; }
        [Display(Name = "Logo")]
        [NotMapped]
        public HttpPostedFileBase attachment { get; set; }
        [Display(Name = "Logo")]
        public string loc_logo { get; set; }
        [Display(Name = "Pricing Group")]
        public string price_group { get; set; }
    }

    public class CompData
    {
        public string comp_name { get; set; }
        public long comp_ID { get; set; }
    }

    [Table("partnerStockCheck")]
    public class partnerStockCheckViewModel
    {
        [Key]
        public long ps_ID { get; set; }
        [Display(Name = "SAP Account")]
        public Nullable<long> ps_account { get; set; }
        [Display(Name = "User Name")]
        public int usr_user { get; set; }
        [Display(Name ="Location Name")]
        public Nullable<long> loc_id { get; set; }
        [NotMapped]
        [Display(Name = "Company Name")]
        public string company_name { get; set; }
    }

    [Table("Webshop_connect")]
    public class WebshopConnectViewModel
    {
        [Key]
        public int ws_ID { get; set; }
        [Display(Name = "Online Shop Account")]
        public string ws_account { get; set; }
        [Display(Name = "User Name")]
        public int usr_user { get; set; }
        [Display(Name = "Location Name")]
        public long loc_id { get; set; }
        [NotMapped]
        [Display(Name = "Company Name")]
        public string company_name { get; set; }
        [NotMapped]
        [Display(Name = "Location Name")]
        public string location_name { get; set; }
    }

    public class SAPdata{
        public long? sap_account { get; set; }
        public long loc_ID { get; set; }
        public string sap_password { get; set; }
        public string loc_name { get; set; }
        public string comp_name { get; set; }
        public string loc_city { get; set; }
    }

    public class WSdata
    {
        public string ws_account { get; set; }
        public long loc_ID { get; set; }
        public string ws_password { get; set; }
        public string loc_name { get; set; }
        public string comp_name { get; set; }
        public string loc_city { get; set; }
    }

    [Table("PartnerApplication")]
    public class PartnerApplicationViewModel
    {
        [Key]
        [Display(Name = "Id")]
        public int appli_id { get; set; }
        [Display(Name = "Application Name")]
        public string appli_name { get; set; }
        [Display(Name = "Order")]
        public string order { get; set; }
    }

}
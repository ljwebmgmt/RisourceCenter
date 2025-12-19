using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class HomeViewModel
    {
        //User Personal Information
        [Required]
        [Display(Name = "Name")]
        public string usr_full_name { get; set; }
        public string[] name_label { get; set; }
        [Required]
        [Display(Name = "Address")]
        public string usr_address { get; set; }
        public string[] address_label { get; set; }
        [Required]
        [Display(Name = "Job Title")]
        public string usr_title { get; set; }
        public string[] title_label { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public string usr_phone { get; set; }
        public string[] phone_label { get; set; }
        [Required]
        [Display(Name = "Website")]
        public string  usr_website { get; set; }
        public string[] website_label { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string usr_email { get; set; }
        public string[] email_label { get; set; }
        //User Company Information
        [Required]
        [Display(Name = "Company Name")]
        public string company_name { get; set; }
        public string[] company_name_label { get; set; }
        [Required]
        [Display(Name = "Account Type")]
        public int? company_type { get; set; }
        public string[] company_type_label { get; set; }
        [Display(Name = "Company Industry")]
        public int? company_industry { get; set; }
        public string[] company_industry_label { get; set; }
        [Display(Name = "Location")]
        public string Location { get; set; }
        public string[] Location_label { get; set; }
        [Required]
        [Display(Name = "Language")]
        public string usr_language { get; set; }
        public string[] language_label { get; set; }
        [Display(Name = "Company Logo")]
        public string company_logo { get; set; }
        public string[] company_logo_label { get; set; }
        [Display(Name = "Comapny Products")]
        public string company_prod { get; set; }
        public IEnumerable<partnerProduct> list_products { get; set; }
        public string[] company_prod_label { get; set; }
        [Display(Name = "Comapny Phone")]
        public string loc_phone { get; set; }
        public string[] loc_phone_label { get; set; }
        [Display(Name = "Company Email")]
        public string loc_email { get; set; }
        public string[] loc_email_label { get; set; }
        [Display(Name = "Company Address")]
        public string loc_address { get; set; }
        public string[] loc_address_label { get; set; }
        //other labels
        public string[] personal_info_heading { get; set; }
        public string[] price_availability_heading { get; set; }
        public string[] company_info_heading { get; set; }
        public string[] edit_label { get; set; }
        public double? mdf_Amount { get; set; }
        public int? comp_MDF { get; set; }
        public string password { get; set; }
        public string usr_fName { get; set; }
        public string usr_lName { get; set; }
        public string usr_city { get; set; }
        public string usr_state { get; set; }
        public string usr_zip { get; set; }
        public string usr_add1 { get; set; }
        public long? comp_ID { get; set; }
    }

    public class StockChecks
    {
        public long? ps_account { get; set; }
        public long? usr_user { get; set; }
        public long? ps_ID { get; set; }
        public string comp_name { get; set; }
        public string sap_password { get; set; }
        public string loc_city { get; set; }
        public string usr_fullName { get; set; }
        public string loc_state { get; set; }
        public string comp_industry { get; set; }
        public string comp_audience { get; set; }
    }

    public class ListStockChecks
    {
        public List<StockChecks> Distinct_stockPrice { get; set; }
        public List<StockChecks> stockPrice { get; set; }
    }

    public class Webshops
    {
        public string ws_account { get; set; }
        public long? usr_user { get; set; }
        public string ws_user { get; set; }
        public long? ws_ID { get; set; }
        public string comp_name { get; set; }
        public string ws_password { get; set; }
        public string loc_city { get; set; }
        public string usr_fullName { get; set; }
        public long? sap_account { get; set; }
        public string loc_state { get; set; }
        public string comp_industry { get; set; }
        public string comp_audience { get; set; }
    }

    public class ListWebshops
    {
        public List<Webshops> Distinct_stockPrice { get; set; }
        public List<Webshops> stockPrice { get; set; }
    }

    [Table("country")]
    public class countriesViewModel
    {
        [Key]
        public int country_id { get; set; }
        public string country_abbr { get; set; }
        public string country_long { get; set; }
        public string Language { get; set; }
    }

    [Table("data_state")]
    public partial class datastateViewModel
    {
        [Key]
        public int stateid { get; set; }
        public string state_abbr { get; set; }
        public string state_long { get; set; }
        public string state_country { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("salesComm")]
    public class SalesCommViewModel
    {
        [Key]
        public long scID { get; set; }
        [Display(Name = "Category")]
        public Nullable<long> n2ID { get; set; }
        public IEnumerable<Nav1List> list_n2ID { get; set; }
        [Display(Name = "Attach a RiSource")]
        public IEnumerable<Nav1List> risource_menu { get; set; }
        [Display(Name = "Level 3 ")]
        public Nullable<long> n3ID { get; set; }
        [Display(Name = "Status *")]
        public Nullable<int> sc_status { get; set; }
        public IEnumerable<Nav1List> list_status { get; set; }
        [Display(Name = "Headline")]
        public string sc_headline { get; set; }
        [AllowHtml]
        [Display(Name = "Teaser")]
        public string sc_teaser { get; set; }
        [AllowHtml]
        [Display(Name = "Body")]
        public string sc_body { get; set; }
        [Display(Name = "Keyword")]
        public string sc_keywords { get; set; }
        [Display(Name = "Products")]
        public string sc_products { get; set; }
        public IEnumerable<partnerProduct> list_products { get; set; }
        [Display(Name = "Audience")]
        public string sc_usrTypes { get; set; }
        public IEnumerable<partnerType> list_Type { get; set; }
        [Display(Name = "Start Date *")]
        public Nullable<System.DateTime> sc_startDate { get; set; }
        [Display(Name = "End Date")]
        public Nullable<System.DateTime> sc_endDate { get; set; }
        [Display(Name = "Owner")]
        public Nullable<long> sc_owner { get; set; }
        [Display(Name = "Industry")]
        public string sc_industry { get; set; }
        public IEnumerable<partnerIndustry> list_industry { get; set; }
        [Display(Name = "Old ID")]
        public Nullable<long> old_scid { get; set; }
        [Display(Name = "Attach RiSource")]
        public string attach_risource { get; set; }
        [Display(Name = "List Of Attachments")]
        public IEnumerable<Nav1List> list_attachments { get; set; }
        [Display(Name = "Publish to Countries *")]
        public string countries { get; set; }
        [Display(Name = "Default Language *")]
        public Nullable<int> default_lang { get; set; }
        [Display(Name = "Translate to Language *")]
        public string languages { get; set; }
        [Display(Name = "Submission Date *")]
        public DateTime submission_date { get; set; }
        [NotMapped]
        public string startDate { get; set; }
        [NotMapped]
        public string endDate { get; set; }
    }

    public partial class SalesCommunicationsViewModel
    {
        public long scID { get; set; }
        public Nullable<long> n2ID { get; set; }
        public Nullable<long> n3ID { get; set; }
        public Nullable<int> sc_status { get; set; }
        public string sc_headline { get; set; }
        public string sc_teaser { get; set; }
        public string sc_body { get; set; }
        public string sc_keywords { get; set; }
        public string sc_products { get; set; }
        public string sc_usrTypes { get; set; }
        public Nullable<System.DateTime> sc_startDate { get; set; }
        public Nullable<System.DateTime> sc_endDate { get; set; }
        public Nullable<long> sc_owner { get; set; }
        public string sc_industry { get; set; }
        public Nullable<long> old_scid { get; set; }
        //Additional files
        public Nullable<long> n1ID { get; set; }
        public string nav2_longName { get; set; }
        public string nav3_longName { get; set; }
        public string attach_risource { get; set; }
        public IEnumerable<Nav1List> list_attachments { get; set; }
        public IEnumerable<SalesMenu> sales_menu { get; set; }
        public IEnumerable<SalesMenu> sales_submenu { get; set; }
        public string countries { get; set; }
        public Nullable<int> default_lang { get; set; }
        public string languages { get; set; }
    }

    public class SalesMenu
    {
        public long? n2Id { get; set; }
        public long n3Id { get; set; }
        public string n2_longName { get; set; }
        public string n3_longName { get; set; }
    }

    public class WeeklyEmail
    {
        public int usr_ID { get; set; }
        public string usr_fName { get; set; }
        public string usr_lName { get; set; }
        public string usr_email { get; set; }
        public long comp_ID { get; set; }
        public string comp_name { get; set; }
        public Nullable<byte> comp_industry { get; set; }
        public Nullable<byte> comp_type { get; set; }
        public string comp_products { get; set; }
        public long scID { get; set; }
        public string sc_headline { get; set; }
        public string sc_teaser { get; set; }
        public string sc_industry { get; set; }
        public string sc_products { get; set; }
        public string sc_usrTypes { get; set; }
        public long? n3Id { get; set; }
        public DateTime send_date { get; set; }
    }
}
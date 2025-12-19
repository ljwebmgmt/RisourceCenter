using newrisourcecenter.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("RiSources")]
    public class RiSourcesViewModel
    {
        [Key]
        public int ris_ID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_section { get; set; }
        [Display(Name = "Select Level 2")]
        public long n2ID { get; set; }
        [Display(Name = "Select Level 3")]
        public long n3ID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_n3ID { get; set; }
        [Display(Name = "RiSource Headline")]
        public string ris_headline { get; set; }
        [AllowHtml]
        [Display(Name = "RiSource teaser")]
        public string ris_teaser { get; set; }
        [AllowHtml]
        [Display(Name = "RiSource Body")]
        public string ris_body { get; set; }
        [Display(Name = "RiSource Status")]
        public string ris_status { get; set; }
        [Display(Name = "RiSource Keywords")]
        public string ris_keywords { get; set; }
        [Display(Name = "RiSource Products")]
        public string ris_products { get; set; }
        public IEnumerable<partnerProduct> list_products { get; set; }
        [Display(Name = "RiSource Categories")]
        public string ris_industry { get; set; }
        public IEnumerable<partnerIndustry> list_industry { get; set; }
        [Display(Name = "RiSource User Types")]
        public string ris_usrTypes { get; set; }
        public IEnumerable<partnerType> list_Type { get; set; }
        [Display(Name = "RiSource Industries")]
        public string ris_categories { get; set; }
        public IEnumerable<risourcesCategory> list_categories { get; set; }
        [Display(Name = "RiSource Application")]
        public string ris_partnerApp { get; set; }
        public IEnumerable<PartnerApplicationViewModel> list_partnerApp { get; set; }
        [Display(Name = "RiSource Edited By")]
        public string ris_editedBy { get; set; }
        [Display(Name = "RiSource Created By")]
        public string ris_owner { get; set; }
        [Display(Name = "RiSource link")]
        public string ris_link { get; set; }
        [Display(Name = "File Size")]
        public string file_size { get; set; }
        [Display(Name = "File type")]
        public string file_type { get; set; }
        [Display(Name = "RiSource Order")]
        public Nullable<int> ris_order { get; set; }
        [Display(Name = "RiSource Start Date")]
        public Nullable<System.DateTime> ris_startDate { get; set; }
        [Display(Name = "RiSource End Date")]
        public Nullable<System.DateTime> ris_endDate { get; set; }
        [Display(Name = "Date Created")]
        public Nullable<System.DateTime> dateCreated { get; set; }
        [Display(Name = "Display Image")]
        public string displayimage { get; set; }
        public IEnumerable<risourcesType_image> list_displayimage { get; set; }
        [Display(Name = "Attachment")]
        [NotMapped]
        public HttpPostedFileBase attachment { get; set; }
    }

    public class RisourcesReportViewModel
    {
        public List<RiSourcesViewModel> resources { get; set; }
        public Dictionary<int,RisourceActivity> resourceActivities { get; set; }
    }

    public class ExportRisourceReportModel
    {
        public string name { get; set; }
        public string type { get;set; }
        public int number_downloads { get; set; }
        public int number_selects { get; set; }
    }

    public class ExportRisourceActivityReportModel
    {
        public string name { get; set; }
        public string type { get; set; }
        public string user {  get; set; }
        public string action { get; set; }
        public DateTime action_time { get; set; }
    }

    public class RisourceActivity
    {
        public int ris_ID { get; set; }
        public string name { get; set; }
        public long type { get; set; }
        public string link { get; set; }
        public int number_downloads { get; set; }
        public int number_selects { get; set; }
    }

    public class RisourceActivityLog
    {
        public int Form_ID { get; set; }
        public int Usr_ID { get; set; }
        public string Action { get; set; }
        public DateTime Action_Time { get; set; }
    }

    public class RisourceAction
    {
        public int user_ID { get; set; }
        public string username { get; set; }
        public string action { get; set; }
        public DateTime action_time { get; set;}
    }

    public class Nav2List
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class RiSourcesModel
    {
        [Key]
        public int ris_ID { get; set; }
        public long n2ID { get; set; }
        public long n3ID { get; set; }
        public string ris_headline { get; set; }
        public string ris_teaser { get; set; }
        public string ris_body { get; set; }
        public string ris_status { get; set; }
        public string ris_keywords { get; set; }
        public string ris_products { get; set; }
        public string ris_industry { get; set; }
        public string ris_usrTypes { get; set; }
        public string ris_categories { get; set; }
        public string ris_editedBy { get; set; }
        public string ris_owner { get; set; }
        public string ris_link { get; set; }
        public string file_size { get; set; }
        public string file_type { get; set; }
        public Nullable<int> ris_order { get; set; }
        public Nullable<System.DateTime> ris_startDate { get; set; }
        public Nullable<System.DateTime> ris_endDate { get; set; }
        public Nullable<System.DateTime> dateCreated { get; set; }
        //Other values
        public string nav2_longName { get; set; }
        public string nav3_longName { get; set; }
        public int n1ID { get; set; }
        public string n2_headerImg { get; set; }
        public string displayimage { get; set; }
        public string ris_partnerApp { get; set; }
        [NotMapped]
        public string status { get; set; }
        [NotMapped]
        public string Date_created { get; set; }
        [NotMapped]
        public string selected { get; set; }
        public IEnumerable<int> listRisources { get; set; }
    }

    public class ListRiSources
    {
        public string list { get; set; }
    }

    [Table("risourcesCategories")]
    public class risourcesCatViewModel
    {
        [Key]
        public int cat_id { get; set; }
        [Display(Name = "RiSource Categories")]
        public string ris_categories { get; set; }
    }

    [Table("risourcesType_image")]
    public class risourcesTypeViewModel
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "Image Link")]
        public string type_link { get; set; }
        [Display(Name = "File Type Name")]
        public string type_name { get; set; }
        [Display(Name = "File Order")]
        public Nullable<int> type_order { get; set; }
        [Display(Name = "Attachment")]
        [NotMapped]
        public HttpPostedFileBase attachment { get; set; }
    }

    public class getValues
    {
       public string ris_links { get; set; }
    }

    [Table("RiSourceCart")]
    public class RiSourcesCarts
    {
        [Key]
        public int ID { get; set; }
        public int ris_ID { get; set; }
        public int user_id { get; set; }
        [NotMapped]
        public string get_headlines { get; set; }
    }
}
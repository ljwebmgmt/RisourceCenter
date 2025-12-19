using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("Nav3")]
    public class Nav3ViewModel
    {
        [Key]
        public long n3ID { get; set; }
        [Display(Name = "Level 2 Nav")]
        public Nullable<long> n2ID { get; set; }
        [Display(Name = "Section")]
        public IEnumerable<System.Web.Mvc.SelectListItem> list_n2ID { get; set; }
        [Display(Name = "Nav Order")]
        public Nullable<int> n3order { get; set; }
        [Display(Name = "Short Name")]
        public string n3_nameShort { get; set; }
        [Display(Name = "Long Name")]
        public string n3_nameLong { get; set; }
        [Display(Name = "Short Description")]
        public string n3_descShort { get; set; }
        [AllowHtml]
        [Display(Name = "Long Description")]
        public string n3_descLong { get; set; }
        [Display(Name = "Nav Status")]
        public Nullable<int> n3_active { get; set; }
        [Display(Name = "Products")]
        public string n3_products { get; set; }
        public IEnumerable<partnerProduct> list_products { get; set; }
        [Display(Name = "Audience")]
        public string n3_usrTypes { get; set; }
        public IEnumerable<partnerType> list_Type { get; set; }
        [Display(Name = "Edited By")]
        public Nullable<long> n3_editBy { get; set; }
        public Nullable<System.DateTime> n3_editDate { get; set; }
        [Display(Name = "Redirect")]
        public string n3_redirect { get; set; }
        [Display(Name = "Keywords")]
        public string n3_keywords { get; set; }
        [Display(Name = "Industry")]
        public string n3_industry { get; set; }
        public IEnumerable<partnerIndustry> list_industry { get; set; }
        [Display(Name = "Old Level 3 ID")]
        public Nullable<long> old_n3id { get; set; }
        [Display(Name = "Old Level 2 ID")]
        public Nullable<long> old_n2id { get; set; }
        public string file_name { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_n1ID { get; set; }
        [Display(Name = "Attachment")]
        [NotMapped]
        public HttpPostedFileBase attachment { get; set; }
        public List<Nav3Viewlabels> nav3labels { get; set; }
    }

    public class Nav1List
    {
        public long? id { get; set; }
        public string name { get; set; }
        public long? n2id { get; set; }
        public string n2name { get; set; }
        public string img { get; set; }
        public long? n3id { get; set; }
        public string n3name { get; set; }
        public Nullable<int> n3order { get; set; }
        public Nullable<long> n2_IE_approver { get; set; }
        public Nullable<long> n2_IT_approver { get; set; }
        public string ie_approver { get; set; }
        public string it_approver { get; set; }
    }

    public class Nav3Viewlabels
    {
        [Key]
        public long n3ID { get; set; }
        public string[] n2ID_label { get; set; }
        public string[] list_n2ID_label { get; set; }
        public string[] n3order_label { get; set; }
        public string[] n3_nameShort_label { get; set; }
        public string[] n3_nameLong_label { get; set; }
        public string[] n3_descShort_label { get; set; }
        public string[] n3_descLong_label { get; set; }
        public string[] n3_active_label { get; set; }
        public string[] n3_products_label { get; set; }
        public string[] list_products_label { get; set; }
        public string[] n3_usrTypes_label { get; set; }
        public string[] list_Type_label { get; set; }
        public string[] n3_editBy_label { get; set; }
        public string[] n3_editDate_label { get; set; }
        public string[] n3_redirect_label { get; set; }
        public string[] n3_industry_label { get; set; }
        public string[] old_n3id_label { get; set; }
        public string[] old_n2id_label { get; set; }
        public string[] list_n1ID_label { get; set; }
        public string[] add_link_label { get; set; }
        public string[] filter_link_label { get; set; }
        public string[] edit_label { get; set; }
        public string[] delete_label { get; set; }
    }

}
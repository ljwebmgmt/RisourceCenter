using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{

    [Table("Localization")]
    public partial class LocalizationModel
    {
        [Key]
        public int localization_id { get; set; }
        [Display(Name = "Table Name")]
        public string table_name { get; set; }
        [Display(Name = "Parent ID")]
        public Nullable<int> parent_id { get; set; }
        [Display(Name = "Table Column Name")]
        public string column_name { get; set; }
        [Display(Name = "Origional Message")]
        [AllowHtml]
        public string message_original { get; set; }
        [Display(Name = "Translated Version")]
        [AllowHtml]
        public string message_translated { get; set; }
        [Display(Name = "Translate to language")]
        public Nullable<int> language { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_country_ids { get; set; }
        [Display(Name = "Edit Translated Language")]
        [NotMapped]
        public Nullable<int> edit_language { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> edit_lang_ids { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> date { get; set; }
    }

    [Table("Labels")]
    public partial class LabelsModel
    {
        [Key]
        public int label_id { get; set; }
        [Display(Name = "Label Name")]
        public string label_name { get; set; }
        [Display(Name = "Controller Name")]
        public string controller_name { get; set; }
        [Display(Name = "Page Name")]
        public string page_name { get; set; }
        [Display(Name = "Translated Label")]
        public string translated_label { get; set; }
        [Display(Name = "Language")]
        public Nullable<int> language { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_country_ids { get; set; }
    }

}
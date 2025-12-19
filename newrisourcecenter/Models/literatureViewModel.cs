using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace newrisourcecenter.Models
{
    public class literatureViewModel
    {
    }
    
    [Table("literature_requested")]
    public partial class LitRequestedViewModel
    {
        [Key]
        public int rlit_ID { get; set; }
        [Display(Name = "Literature")]
        public string rlit_info { get; set; }
        public IEnumerable<LiteratureViewModel> lit_name { get; set; }
        [Display(Name = "Requester")]
        public Nullable<int> usr_ID { get; set; }
        [Display(Name = "Date Requested")]
        public Nullable<System.DateTime> date_created { get; set; }
        [Display(Name = "Status")]
        public Nullable<int> status { get; set; }
        public IEnumerable<AddedLits> added_lits { get; set; }
    }


    [Table("literature")]
    public partial class LiteratureViewModel
    {
        [Key]
        public int lit_ID { get; set; }
        [Display(Name = "Literature Name")]
        public string lit_name { get; set; }
        [Display(Name = "Created By")]
        public Nullable<long> created_by { get; set; }
        [Display(Name = "Date Created")]
        public Nullable<System.DateTime> date_created { get; set; }
        [Display(Name = "Date Updated")]
        public Nullable<System.DateTime> date_updated { get; set; }
        [Display(Name = "Updated By")]
        public Nullable<long> updated_by { get; set; }
        [Display(Name = "RiSources")]
        public string risource { get; set; }
        public IEnumerable<Nav1List> list_attachments { get; set; }
        [Display(Name = "RiSources")]
        public IEnumerable<Nav1List> risource_menu { get; set; }
    }

    public partial class LitRequestedModel
    {
        public int? rlit_ID { get; set; }
        [Display(Name = "Literature")]
        public Nullable<int> lit_ID { get; set; }
        [Display(Name = "Enter Quantity")]
        public Nullable<int> lit_quantity { get; set; }
        [Display(Name = "Requester")]
        public Nullable<int> usr_ID { get; set; }
        [Display(Name = "Date Requested")]
        public Nullable<System.DateTime> date_created { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        [Display(Name = "Literature")]
        public string lit_name { get; set; }
        public string userName { get; set; }
    }

    public class AddedLits
    {
        public string lit_list { get; set; }
    }
}
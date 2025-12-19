using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{

    public class ReturnToolViewModel
    {
        [Display(Name ="Form ID")]
        public Nullable<int> form_id { get; set; }
        [Display(Name = "User ID")]
        public Nullable<int> user_id { get; set; }
        [Display(Name = "Name")]
        public string name { get; set; }
        [Display(Name = "Company Name")]
        public string company { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        [Display(Name = "Phone")]
        public string phone { get; set; }
        [Display(Name = "Admin ID")]
        public Nullable<int> admin_Id { get; set; }
        [Display(Name = "SAP Account")]
        public string sap_num { get; set; }
        [Display(Name = "Purchase Order")]
        public string po_num { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        [Display(Name = "Location")]
        public string location { get; set; }
        [Display(Name = "Request Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? request_date { get; set; }
        [Display(Name = "Dated Submitted")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? submission_date { get; set; }
        [Display(Name = "Date Completed")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? completion_date { get; set; }
        [Display(Name = "Admin Notes")]
        public string admin_notes { get; set; }
        [Display(Name = "Exteneded ID")]
        public Nullable<int> ext_id { get; set; }
        [Display(Name = "Return Type")]
        public string return_type { get; set; }
        [Display(Name = "Part Number")]
        public string part_num { get; set; }
        [Display(Name = "Quantity")]
        public Nullable<int> quantity { get; set; }
        [Display(Name = "Reason")]
        [AllowHtml]
        public string return_reason { get; set; }
        [Display(Name = "Upload Template")]
        public HttpPostedFileBase attachments_file { get; set; }
        [Display(Name = "Attach Images of the UL or Serial Number")]
        public string attachments_ul { get; set; }
        [Display(Name = "Attach pictures of the damages and pictures of the bill of Lading")]
        public string attachments_lading { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_return_types { get; set; }
        public List<ReturnToolExtentions> returnToolExtentions { get; set; }
        public int? file_id { get; set; }
        public int count { get; set; }
        [NotMapped]
        [Display(Name ="Off Set PO #")]
        public string offset_po { get; set; }
        [NotMapped]
        [Display(Name = "Auth Number #")]
        public string authonumber { get; set; } 
        public string warranty { get; set; }
    }

    [Table("ReturnTools")]
    public class ReturnTools
    {
        [Display(Name = "Form ID")]
        [Key]
        public Nullable<int> form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> admin_id { get; set; }
        public string sap_num { get; set; }
        public string po_num { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        public string location { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? request_date { get; set; }
        [Display(Name = "Date Submitted")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? submission_date { get; set; }
        [Display(Name = "Date Completed")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? completion_date { get; set; }
        public string admin_notes { get; set; }
        [Display(Name = "Return Type")]
        public string return_type { get; set; }
        [Display(Name = "Read the Warranty")]
        public string warranty { get; set; }
    }

    [Table("ReturnToolExtentions")]
    public class ReturnToolExtentions
    {
        [Key]
        public Nullable<int> ext_id { get; set; }
        public int? form_id { get; set; }
        public string return_type { get; set; }
        public string part_num { get; set; }
        public Nullable<int> quantity { get; set; }
        public string quote_num { get; set; }
        [AllowHtml]
        public string return_reason { get; set; }
        public string reasoncheckbox { get; set; }
        public string partpo_num { get; set; }
    }

    [Table("ReturnToolFiles")]
    public class ReturnToolFiles
    {
        [Key]
        public Nullable<int> file_id { get; set; }
        public Nullable<int> form_id { get; set; }
        public string identifier { get; set; }
        public string return_type { get; set; }
        public string file_name { get; set; }
        public string offset_po { get; set; }
    }

    [Table("ReturnToolActionLogs")]
    public class ReturnToolActionLogs
    {
        [Key]
        public Nullable<int> log_id { get; set; }
        public Nullable<int> form_id { get; set; }
        public long user_id { get; set; }
        public string action { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? action_time { get; set; }
        public string notes { get; set; }
        public string authNumber { get; set; }
    }
}
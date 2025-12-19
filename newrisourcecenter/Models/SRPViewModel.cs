using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("salesRequest")]
    public partial class SRPViewModel
    {
        [Key]
        public int FormID { get; set; }
        [Display(Name = "Sales Rep ID: *")]
        public Nullable<long> SalesRepID { get; set; }
        [Display(Name = "Method of Payment: *")]
        public string paymentMethod { get; set; }
        public IEnumerable<SelectListItem> list_paymentMethod { get; set; }
        [Display(Name ="Department: *")]
        public string department { get; set; }
        public IEnumerable<SelectListItem> list_departments { get; set; }
        [Display(Name = "Type Of Request: *")]
        public string requestType { get; set; }
        public IEnumerable<SelectListItem> list_requestType { get; set; }
        [Display(Name = "Sales Region: *")]
        public string region { get; set; }
        public IEnumerable<SelectListItem> list_regionalManagers { get; set; }
        [Display(Name = "Supplier: *")]
        public string supplier { get; set; }
        [Display(Name = "Supplier (SAP)#: ")]
        public string supplierNumber { get; set; }
        [Display(Name = "Ship To: *")]
        public string shipTo { get; set; }
        [Display(Name = "Ship to Attn: *")]
        public string shiptoAttn { get; set; }
        [NotMapped]
        [Display(Name = "Attach Supporting Documents: ")]
        public IEnumerable<HttpPostedFileBase> fileupload { get; set; }
        [NotMapped]
        [Display(Name = "Upload Invoice: ")]
        public IEnumerable<HttpPostedFileBase> uploadinvoice { get; set; }
        [Display(Name = "Status: ")]
        public string status { get; set; }
        [Display(Name = "Title: ")]
        public string title { get; set; }
        [Display(Name = "Description: ")]
        public string description { get; set; }
        [Display(Name = "Estimated Cost: ")]
        public string estimatedCost { get; set; }
        [Display(Name = "Activity Date: ")]
        public Nullable<DateTime> activitydate { get; set; }
        [Display(Name = "Date Created: ")]
        public Nullable<DateTime> dateCreated { get; set; }
        [Display(Name = "Date Completed: ")]
        public Nullable<DateTime> dateCompleted { get; set; }
        [Display(Name = "Date Update: ")]
        public Nullable<DateTime> dateUpdated { get; set; }
        public IQueryable<salesRequestFile> list_srp_files { get; set; }
        public IQueryable<salesRequestFile> list_srp_invoice { get; set; }
        public IQueryable<salesRequestAdditionalInfo> list_additional_info { get; set; }
        [Display(Name = "Chart of Accounts: *")]
        [NotMapped]
        public string achType { get; set; }
        public IEnumerable<SelectListItem> list_achType { get; set; }
        [Display(Name = "Cost Center: *")]
        [NotMapped]
        public string cccType { get; set; }
        public IEnumerable<SelectListItem> list_cccType { get; set; }
        [Display(Name ="PO Number: ")]
        public string ponumber { get; set; }
        [NotMapped]
        public string topdesc { get; set; }
        [NotMapped]
        public string usr_fname { get; set; }
        [NotMapped]
        public string usr_lname { get; set; }
        [NotMapped]
        public string usr_phone { get; set; }
        [NotMapped]
        public string usr_email { get; set; }
        [NotMapped]
        public string fullname { get; set; }
        public int compID { get; set; }
        public int InvoiceAmountIsEqual { get; set; }
    }

    [Table("salesRequest_Additional_Info")]
    public partial class salesRequestAdditionalInfo
    {
        [Display(Name = "ID: *")]
        public int ID { get; set; }
        [Display(Name = "Form ID: *")]
        public Nullable<int> Form_ID { get; set; }
        [Display(Name = "Quantity: *")]
        public string quantity { get; set; }
        [Display(Name = "Unit Price: *")]
        public string unitPrice { get; set; }
        [Display(Name = "Total Price: *")]
        public string totalPrice { get; set; }
        [Display(Name = "PartNumber/Description: *")]
        public string partNumberOrdescription { get; set; }
        [Display(Name = "Delivery Date: *")]
        public string deliveryDate { get; set; }
        [Display(Name = "Chart of Accounts: *")]
        public string achType { get; set; }
        public IEnumerable<SelectListItem> list_achType { get; set; }
        [Display(Name = "Cost Center: *")]
        public string cccType { get; set; }
        public IEnumerable<SelectListItem> list_cccType { get; set; }
    }

    [Table("salesRequest_Action_Log")]
    public partial class salesRequestActionLog
    {
        [Key]
        public int ID { get; set; }
        public int Form_ID { get; set; }
        public string Action { get; set; }
        public Nullable<System.DateTime> Action_Time { get; set; }
        public string Notes { get; set; }
        public string Usr_ID { get; set; }
        [NotMapped]
        public string name { get; set; }
    }

    [Table("salesRequest_File")]
    public partial class salesRequestFile
    {
        [Key]
        public int ID { get; set; }
        public int FormID { get; set; }
        public string FileName { get; set; }
        public int AttachmentType { get; set; }
    }

    [Table("salesRequest_Approvers")]
    public partial class salesRequestApprovers
    {
        public int ID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string RegionName { get; set; }
        public Nullable<int> Status { get; set; }
        public string Department { get; set; }
        [NotMapped]
        [Display(Name ="Approver")]
        public string fullName { get; set; }
        [NotMapped]
        [Display(Name = "Status")]
        public string StatusName { get; set; }
        public IEnumerable<SelectListItem> list_users { get; set; }
        public IEnumerable<SelectListItem> list_departments { get; set; }
        public IEnumerable<SelectListItem> list_regions { get; set; }
        public IEnumerable<SelectListItem> list_status { get; set; }
    }

    public class SRPCommon
    {
        public List<SelectListItem> department { get;  set; }
        public List<SelectListItem> requestType { get; set; }
        public List<SelectListItem> paymentMethod { get; set; }
        public List<SelectListItem> regionalManager { get; set; }
        public List<SelectListItem> achType { get; set; }
        public List<SelectListItem> cccType { get; set; }
    }
}
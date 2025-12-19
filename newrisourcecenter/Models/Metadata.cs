using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class ChangeCommissionMetadata
    {
        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "Credit Sale")]
        public int credit_sale { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "To Representative")]
        public string to_representive { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "From Representative")]
        public string from_representive { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "Account Number")]
        public string account_number { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "Account Name")]
        public string account_name { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "Sales Document")]
        public Nullable<int> sales_doc_number { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "PO Number")]
        public string po_number { get; set; }

        [Required(ErrorMessage ="This is required field")]
        [Display(Name = "Invoice Date")]
        public System.DateTime invoice_date { get; set; }

        [Required(ErrorMessage ="This is required field")]
        //[MaxLength(10)]
        //[MinLength(1)]
        [RegularExpression(@"^\d{0,6}(\.\d{1,3})?$", ErrorMessage = "Not valid number. Amount must be numeric b/w 6 decimal and 3 percision numbers")]
        [Display(Name = "Amount")]
        public decimal amount { get; set; }

        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Assigned Rep:		")]
        public Nullable<int> assigned_rep { get; set; }

        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Assigned Manger:		")]
        public Nullable<int> assigned_manager { get; set; }

        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Requesting Manager:		")]
        public Nullable<int> request_manager { get; set; }

        [Display(Name = "Initial Approval ")]
        public Nullable<int> initialApproval { get; set; }

        [Display(Name = "Final Approval")]
        public string FinalApproval { get; set; }



    }


    public class CommissionAdminMetadata
    {
        [Required(ErrorMessage = "This is required field")]
        public string Req_Manager { get; set; }

        [Required(ErrorMessage = "This is required field")]
        public string Assigned_Manager { get; set; }

    }

    public class BidRegistrationMetadata
    {
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Partner Name")]
        public string contact_name { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Partner Email")]
        public string contact_email { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Partner Phone")]
        public string contact_phone { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Customer Name")]
        public string customer_name { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Decision Maker?")]
        public bool decision_maker { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Company Name")]
        public string company_name { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Cpmpany HQ Location")]
        public string company_location { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Company Project Location")]
        public string project_location { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Project Name")]
        public string project_name { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Estimated Start Date")]
        public Nullable<System.DateTime> estimated_start_date { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Ball-park Value")]
        public Nullable<decimal> project_value { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Rittal products being recommended")]
        public string products { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Fulfilling this order through an IT distributor?")]
        public bool IT_distributor { get; set; }
        [Display(Name = "Distributor Name")]
        public string distributor_name { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Project Type")]
        public string project_type { get; set; }
        [Display(Name = "Competition/Incumbent")]
        public string competition { get; set; }
        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Actions Required")]
        public string actions_required { get; set; }
        [Display(Name = "IT Channel/Territory Manager Approval")]
        public string channel_manager_approval { get; set; }
        [Display(Name = "IT Director of Channel Sales Approval")]
        public string director_approval { get; set; }
        [Display(Name = "GM Approval")]
        public string gm_approval { get; set; }
        [Display(Name = "Files")]
        public string files { get; set; }
        [Display(Name = "Created By")]
        public Nullable<int> created_by { get; set; }
    }
}
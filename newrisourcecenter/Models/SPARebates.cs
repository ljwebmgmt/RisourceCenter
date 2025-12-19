using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("SPA_Rebates")]
    public partial class SPARebatesViewModel
    {
        [Key]
        [Display(Name="Rebate ID")]
        public int rebate_ID { get; set; }
        [Display(Name = "Contract ID")]
        public int contract_ID { get; set; }
        [Display(Name = "Total Rebate")]
        public string rebate_total_amount { get; set; }
        [Display(Name = "Credit Memo")]
        public string credit_memo { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; }
        [Display(Name = "Submit Date")]
        public DateTime submit_date { get; set; }
        [Display(Name = "Memo Date")]
        public DateTime memo_date { get; set; }
        [NotMapped]
        [Display(Name = "Select Distributor")]
        public List<SelectListItem> list_comp { get; set; }
        [NotMapped]
        public string comp { get; set; }
        [NotMapped]
        public List<SPARebatesItemsViewModel> spa_rebates_items { get; set; }
    }

    [Table("SPA_RebatesItems")]
    public partial class SPARebatesItemsViewModel
    {
        [Key]
        public int rebateItem_ID { get; set; }
        public int rebate_ID { get; set; }
        public int contract_ID { get; set; }
        public string sku { get; set; }
        public double last_price {get;set;}
        public double spa_price { get; set; }
        public int quantity_rebated { get; set; }
        public int quantity_requested { get; set; }
        public string capping_status { get; set; }
        public string current_rebate { get; set; }
        public string distributor_requests { get; set; }
        public string original_difference { get; set; }
        public DateTime invoice_date { get; set; }
        public string customer_invoice_number { get; set; }
        public string rittal_invoice_number { get; set; }
        public string rebate_amount { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        [NotMapped]
        public string invoicedate { get; set; }
    }

    public class ItemsList
    {
        public string sku { get; set; }
        public int quantity_requested { get; set; }
        public string invoice_date { get; set; }
        public string customer_invoice_num { get; set; }
        public string rittal_invoice_num { get; set; }
        public string position { get; set; }
        public int contract_id { get; set; }
        public string rebate_id { get; set; }
        public string rebateItem_id { get; set; }
    }

    public class List_Results
    {
        public string position { get; set; }
        public string msg { get; set; }
    }

    public class ListPriceAmount
    {
        public double amount { get; set; }
        public string list_price { get; set; }
    }

}
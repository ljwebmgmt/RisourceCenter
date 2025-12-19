using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("SPAs")]
    public class SPAViewModels
    {
        [Key]
        [Display(Name = "Form ID")]
        public int Spa_id { get; set; }
        [Display(Name = "User ID")]
        public Nullable<int> Usr_id { get; set; }
        [Display(Name = "Distributor ID")]
        public Nullable<int> Comp_id { get; set; }
        [Display(Name = "Company Name")]
        public string Customer_company_name { get; set; }
        [Display(Name = "Contract Id")]
        public string Contract_id { get; set; }
        [Display(Name = "Contact Name")]
        public string Customer_name { get; set; }
        [Display(Name = "Customer Address1")]
        public string Customer_address1 { get; set; }
        [Display(Name = "Customer Address2")]
        public string Customer_address2 { get; set; }
        [Display(Name = "Customer City")]
        public string Customer_city { get; set; }
        [Display(Name = "Customer State")]
        public string Customer_state { get; set; }
        [Display(Name = "Customer Zip")]
        public string Customer_zip { get; set; }
        [Display(Name = "Customer Phone")]
        public string Customer_phone { get; set; }
        [Display(Name = "Contact Email")]
        public string Customer_email { get; set; }
        [Display(Name = "Contact Title")]
        public string Customer_title { get; set; }
        [Display(Name = "Contract Start Date")]
        public DateTime? Start_date { get; set; }
        [Display(Name = "Contract End Date")]
        public DateTime? End_date { get; set; }
        [Display(Name = "Date Updated")]
        public DateTime? Updated_date { get; set; }
        [Display(Name = "Date Approved")]
        public DateTime? Approved_date { get; set; }
        [Display(Name = "Updated by")]
        public Nullable<int> Updated_by { get; set; }
        [Display(Name = "Projected Sales ($)")]
        public string Projected_sales { get; set; }
        [Display(Name = "Competition")]
        public string Competition { get; set; }
        [Display(Name = "Territory Code")]
        public string Territory_code { get; set; }
        [Display(Name = "Activity Status")]
        public string Activity_status { get; set; }
        [Display(Name = "Contract Status")]
        public string Status { get; set; }
        [Display(Name = "Contract Type")]
        public string Contract_type { get; set; }
        [Display(Name = "Market Segment")]
        public string Market_segement { get; set; }
        [Display(Name = "Account Type")]
        public string Account_type { get; set; }
        [Display(Name = "Additional Information")]
        public string Additional_information { get; set; }
        [Display(Name = "Distributor Location")]
        public string Distributor_location { get; set; }
        public IEnumerable<SelectListItem> List_dist_locations { get; set; }
        [Display(Name = "Sales Rep User")]
        public string Sales_rep_user { get; set; }
        public IEnumerable<SelectListItem> List_sales_rep_user { get; set; }
        public IEnumerable<SPASalesRepsViewModel> List_sales_reps { get; set; }
        [NotMapped]
        [Display(Name = "Contract Creator")]
        public string UserName { get; set; }
        [NotMapped]
        [Display(Name = "Distributor Name")]
        public string CompanyName { get; set; }
        [NotMapped]
        public string Updated_By_Name { get; set; }
        [NotMapped]
        public List<SPAbreakdown> SPAbreakdown { get; set; }
        [NotMapped]
        public string Sales_rep_user_name { get; set; }
        [NotMapped]
        public int count { get; set; }
        [NotMapped]
        public List<SPAItemViewModel> List_SPA_items { get; set; }
    }

    public class SPAbreakdown
    {
        public List<SPAViewModels> SPACurrent { get; set; }
        public List<SPAViewModels> SPAAwaitingApproval { get; set; }
        public List<SPAViewModels> SPAUpcoming { get; set; }
        public List<SPAViewModels> SPADeclined { get; set; }
        public List<SPAViewModels> SPAExpired { get; set; }
        public List<SPAViewModels> SPADraft { get; set; }
    }

    public class SPAExpired
    {
        public int Spa_id { get; set; }
        public int? Usr_id { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string Customer_name { get; set; }
    }

    public class CountCompanies
    {
        public long Comp_id { get; set; }
        public string CompanyName { get; set; }
        public string CountSPAs { get; set; }
        public int Comp_SPA { get; set; }
        public int Comp_POS { get; set; }
    }

    public class CountUsers
    {
        public int Id { get; set; }
        public int? Usr_id { get; set; }
        public string UserName { get; set; }
        public string CountSPAs { get; set; }
    }

    [Table("SPAItems")]
    public class SPAItemViewModel
    {
        [Key]
        [Display(Name = "Item Id")]
        public int Item_id { get; set; }
        [Display(Name = "Item Name")]
        public string Product_name { get; set; }
        [Display(Name = "Form Id")]
        public int Form_id { get; set; }
        [Display(Name = "Quantity")]
        public string Quantity { get; set; }
        [Display(Name = "SKU")]
        public string Sku { get; set; }
        [Display(Name = "Requested Price")]
        public string Requested_price { get; set; }
        [Display(Name = "Requested Multiplier")]
        public string Requested_multiplier { get; set; }
        [Display(Name = "Target Price")]
        public string Target_price { get; set; }
        [Display(Name = "Status")]
        public int Item_Status { get; set; }
        [Display(Name = "Updated By")]
        public int Updated_by { get; set; }
        [Display(Name ="Date Created")]
        public DateTime? Date_Created { get; set; }
        [Display(Name ="Date Updated")]
        public DateTime? Date_Updated { get; set; }
        [NotMapped]
        public IEnumerable<SPAMaterialMasterViewModel> List_skus { get; set; }
        [NotMapped]
        [Display(Name = "Select a SKU")]
        public string Sku_id { get; set; }
        [NotMapped]
        public string Created_Update { get; set; }
        [NotMapped]
        public string Update_date { get; set; }
        [NotMapped]
        public float? Instock_price { get; set; }
        [NotMapped]
        public float? List_price { get; set; }
        [NotMapped]
        public float? Instock_multiplier { get; set; }
        [NotMapped]
        public float? Cost { get; set; }
    }

    [Table("SPA_Material_Master")]
    public partial class SPAMaterialMasterViewModel
    {
        [Key]
        [Display(Name ="ID")]
        public int ID { get; set; }
        [Display(Name = "Material")]
        public string material { get; set; }
        [Display(Name = "Material Description")]
        public string material_description { get; set; }
        [Display(Name = "MPG")]
        public string mpg { get; set; }
        [Display(Name = "MPG Description")]
        public string mpg_description { get; set; }
        [Display(Name = "Cost")]
        public string cost { get; set; }
        [Display(Name = "List Price")]
        public string list_price { get; set; }
    }

    [Table("SPA_Intostock_Multiplier")]
    public partial class SPAIntostockMultiplierViewModel
    {
        public int ID { get; set; }
        public string GG { get; set; }
        public string PG { get; set; }
        public string CC { get; set; }
        public string AA { get; set; }
    }

    [Table("SPASkus")]
    public class SkusViewModel
    {
        [Key]
        public int Sku_id { get; set; }
        public string Sku_code { get; set; }
        public string Description { get; set; }
        public float? ListPrice { get; set; }
        public float? IntoStockPrice { get; set; }
        public float? Cost { get; set; }
        public float? IntoStockMultiplier { get; set; }
    }

    [Table("SPA_SalesReps")]
    public class SPASalesRepsViewModel
    {
        [Key]
        public int Rep_id { get; set; }
        public string Usr_id { get; set; }
        public int? Form_id { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        [NotMapped]
        public string Email { get; set; }
        [NotMapped]
        public string Roles { get; set; }
    }

    [Table("SPA_FIles")]
    public class SPA_FIlesViewModel
    {
        [Key]
        public int File_id { get; set; }
        public int? Form_id { get; set; }
        public string File_name { get; set; }
        public string File_ext { get; set; }
    }

    public class SPADetailsModel{
        public SPAViewModels Get_SPAs { get; set; }
        public IEnumerable<SPAItemViewModel> List_SPA_Items { get; set; }
        public IEnumerable<SPASalesRepsViewModel> List_SPA_Participants { get; set; }
        public IEnumerable<SPA_FIlesViewModel> List_SPA_Files { get; set; }
        public IEnumerable<SPANotesViewModels> List_Notes { get; set; }

        public SAPDistributor List_SPA_Distributor { get; set; }
    }

    public class SAPDistributor
    {
        [Display(Name = "Distributor Name")]
        public string company_name { get; set; }
        [Display(Name ="Distributor Address")]
        public string address { get; set; }
        [Display(Name = "Distributor City")]
        public string city { get; set; }
        [Display(Name ="Distributor State")]
        public string state { get; set; }
        [Display(Name = "Distributor Zip")]
        public string zip { get; set; }
        [Display(Name = "Distributor Phone")]
        public string phone { get; set; }
    }

    public class FailedItems
    {
        public string SKU { get; set; }
    }

    [Table("SPA_Territory_Codes")]
    public class SPATerritoryCode
    {
        public int ID { get; set; }
        public string zip_region { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string county { get; set; }
        public string zip_type { get; set; }
        public string ie_sales_office { get; set; }
        public string ie_manager { get; set; }
        public string territory_code { get; set; }
    }
    [Table("SPA_Account_Manager")]
    public partial class SPAAccountManager
    {
        public int ID { get; set; }
        [Display(Name = "Contact Name")]
        public string contact_name { get; set; }
        [Display(Name = "Contact Type")]
        public string contact_type { get; set; }
        [Display(Name = "Title")]
        public string title { get; set; }
        [Display(Name = "Zip")]
        public string zip { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        [Display(Name = "Territory Code")]
        public string territory_code { get; set; }
        [NotMapped]
        public string count { get; set; }
    }

    public class Compids
    {
        public string comp_ids { get; set; }
        public string email { get; set; }
    }

    [Table("SPA_Notes")]
    public class SPANotesViewModels
    {
        public int ID { get; set; }
        public bool Note_Type { get; set; }
        public int Form_ID { get; set; }
        public string Action { get; set; }
        public DateTime Action_Time { get; set; }
        public string Note { get; set; }
        public int User_ID { get; set; }
        [NotMapped]
        public string Action_date { get; set; }
        [NotMapped]
        public bool Can_delete { get; set; }
    }
}
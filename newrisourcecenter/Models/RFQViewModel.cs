using newrisourcecenter.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("RFQ_New_Data")]
    public class RFQNewViewModel
    {
        [Key]
        public int ID { get; set; }
        [Display(Name ="Requested By: *")]
        [Required]
        public string sales_engineer { get; set; }
        [Display(Name = "Regional General Manager: *")]
        public string regional_director { get; set; }
        [Display(Name = "Rittal Account Manager: ")]
        public string account_manager { get; set; }
        [Display(Name = "Your Contact Number: ")]
        public string cell_phone { get; set; }
        [Display(Name = "Your Email: ")]
        public string email { get; set; }
        [Display(Name = "Date Submitted: *")]
        [Required]
        public DateTime? submission_date { get; set; }
        [Display(Name = "Completion Date: *")]
        public DateTime? completion_date { get; set; }
        [Display(Name = "New quote or quote revision: ")]
        public string updated_quote { get; set; }
        [Display(Name = "Sold to Party Name: ")]
        public string sold_to_party { get; set; }
        [Display(Name = "Quote Number / Line Item updating: *")]
        public string qte_num { get; set; }
        [Display(Name = "SAP Account Number: *")]
        public string sap_account_num { get; set; }
        [Display(Name = "Location: ")]
        public string location { get; set; }
        [Display(Name = "Sold to Contact:")]
        public string end_contact { get; set; }
        [Display(Name = "Opportunity Number: *")]
        public string opportunity_num { get; set; }
        [Display(Name = "Project Name: *")]
        [Required]
        public string qte_ref { get; set; }
        [Display(Name = "End User Name: *")]
        public string end_user { get; set; }
        [Display(Name = "Special Instructions: ")]
        public string qte_description { get; set; }
        [Display(Name = "Drawing Number: ")]
        public string draw_num { get; set; }
        [Display(Name = "Total Qty/EAU: *")]
        public string total_qty { get; set; }
        [Display(Name = "Release Qty: *")]
        public string release_qty { get; set; }
        [Display(Name = "Competition: ")]
        public string competition { get; set; }
        [Display(Name = "Target Price: ")]
        public string target_price { get; set; }
        [Display(Name = "Scaled/Volume Pricing: ")]
        public string scale_volume { get; set; }
        [Display(Name = "SPA Contract Number: ")]
        public string spa_contract_num { get; set; }
        [Display(Name = "SPA Mult: ")]
        public string spa_mult { get; set; }
        [Display(Name = "Drawing for Approval: ")]
        public Nullable<short> drawing_approval { get; set; }
        [Display(Name = "Check if a Project: ")]
        public Nullable<short> check_project { get; set; }
        [Display(Name = "Product Category: *")]
        public string product_category { get; set; }
        public IEnumerable<SelectListItem> listSAP { get; set; }
        public Dictionary<long,string> listCompanies { get; set; }
        public Dictionary<long,Dictionary<long,Dictionary<string,string>>> listLocations { get; set; }
        public IEnumerable<SelectListItem> list_prod_cat { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_data { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_non_data { get; set; }
        [Display(Name = "Enclosure Type: ")]
        public string enclosure_type_it { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_it { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_it { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_it { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_it { get; set; }
        [Display(Name = "Color: ")]
        public string color_it { get; set; }
        [Display(Name = "Sidewall Style: ")]
        public string sidewall_style_it { get; set; }
        [Display(Name = "Sidewall location: ")]
        public string sidewall_location_it { get; set; }
        [Display(Name = "Castors: ")]
        public string castors_it { get; set; }
        [Display(Name = "Leveling feet: ")]
        public string Leveling_feet_it { get; set; }
        [Display(Name = "Front: ")]
        public string front_it { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_it { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_it { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_it { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_it { get; set; }
        [Display(Name = "Partition Wall: ")]
        public string partition_wall_it { get; set; }
        [Display(Name = "Baffles: ")]
        public string baffles_it { get; set; }
        [Display(Name = "Baying Brackets: ")]
        public string bsaying_brackets_it { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_datacenter { get; set; }
        [Display(Name = "Intelligence: ")]
        public string intell_data { get; set; }
        [Display(Name = "Voltage: ")]
        public string voltage_data { get; set; }
        [Display(Name = "Amperage: ")]
        public string amp_data { get; set; }
        [Display(Name = "Outlet Type: ")]
        public string outlet_it { get; set; }
        [Display(Name = "Quantity of Type: ")]
        public string quantity_type_data { get; set; }
        [Display(Name = "Input Cord Plug Type: ")]
        public string input_cord_it { get; set; }
        [Display(Name = "Expansion Unit (Check For Yes): ")]
        public string expansion_it { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_ie { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_ie { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_ie { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_ie { get; set; }
        [Display(Name = "Material: ")]
        public string material_ie { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_ie { get; set; }
        [Display(Name = "Sidewalls: ")]
        public string sidewall_ie { get; set; }
        [Display(Name = "Front: ")]
        public string front_ie { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_ie { get; set; }
        [Display(Name = "Plinths: ")]
        public string plinths_ie { get; set; }
        [Display(Name = "Plinths Type: ")]
        public string plinths_type_ie { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_ie { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_ie { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_ie { get; set; }
        [Display(Name = "19 inch Rails: ")]
        public string Rails { get; set; }
        [Display(Name = "Suited?: ")]
        public string Suited { get; set; }
        [Display(Name = "If suited, which bay is this? Lt to Rt: ")]
        public string suited_bay_ie { get; set; }
        [Display(Name = "Door Modified?: ")]
        public string door_ie { get; set; }
        [Display(Name = "Roof Modified?: ")]
        public string roof_ie { get; set; }
        [Display(Name = "Rear wall modified?: ")]
        public string rear_wall_ie { get; set; }
        [Display(Name = "Sidewalls modified?: ")]
        public string sidewall_mod_ie { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_mod_ie { get; set; }
        [Display(Name = "Special Paint Types: ")]
        public string special_paint_ie { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_ie_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_ie { get; set; }
        [Display(Name = "UL/NEMA Rating: ")]
        public string ul_nema_other_ie { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_ie { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_WM_AE_JB { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_WM_AE_JB { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_WM_AE_JB { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_WM_AE_JB { get; set; }
        [Display(Name = "Material: ")]
        public string material_WM_AE_JB { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_WM_AE_JB { get; set; }
        [Display(Name = "Latching: ")]
        public string latching_wm { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_wm { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_wm { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_wm { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_wm { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_wm_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_WM_AE_JB { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_WM_AE_JB { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_WM_AE_JB { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_other_1 { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_other_1 { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_other { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_other { get; set; }
        [Display(Name = "Prod Type: ")]
        public string producttype_other_1 { get; set; }
        [Display(Name = "Material: ")]
        public string material_other_1 { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_other_1 { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_other_1 { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_other_1 { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_other_1 { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string specialpaint_other { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_other  { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_other_1 { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_other_1 { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_footer { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_3 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_3 { get; set; }
        public string send { get; set; }
        public string admin_status { get; set; }
        [NotMapped]
        [Display(Name = "Document Upload: ")]
        public IEnumerable<HttpPostedFileBase> fileupload { get; set; }
        public int? user_id { get; set; }
        public string save { get; set; }
        [Display(Name = "Contact Person Name: *")]
        [Required]
        public string distro_name { get; set; }
        [Display(Name = "Company/Location Name: *")]
        [Required]
        public string distro_company { get; set; }
        [NotMapped]
        public string form_id { get; set; }
        [NotMapped]
        [Display(Name = "Project Drawing Upload: ")]
        public string Image_Name { get; set; }
        [NotMapped]
        public List<RFQNewViewModelExtPart> RFQExt { get; set; }
        [NotMapped]
        public List<RFQ_New_File> list_RFQ_files { get; set; }
        [NotMapped]
        public string admin_notes { get; set; }
        [NotMapped]
        public List<RFQ_New_Action_LogViewModel> list_RFQ_logs { get; set; }
        public IQueryable<RFQ_Parts_Installed> list_installed_parts { get; set; }
        public IQueryable<RFQ_Parts_Shipped> list_shipped_parts { get; set; }
        [Display(Name = "Quote Number: ")]
        public string Quote_Num { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
        [NotMapped]
        public string requestor { get; set; }
        [NotMapped]
        public string comp_name { get; set; }
        [Display(Name = "Deal Registration Number: ")]
        public string deal_registration { get; set; }
        [Display(Name = "No. of Enclosures: ")]
        public Nullable<short> suited_enclosures_it { get; set; }
        [Display(Name = "No. of Enclosures: ")]
        public Nullable<short> suited_enclosures_ie { get; set; }
        [Display(Name = "Enclosure 1 (Part number HxWxD): ")]
        public string enclosure_1_it { get; set; }
        [Display(Name = "Enclosure 2 (Part number HxWxD): ")]
        public string enclosure_2_it { get; set; }
        [Display(Name = "Enclosure 3 (Part number HxWxD): ")]
        public string enclosure_3_it { get; set; }
        [Display(Name = "Enclosure 4 (Part number HxWxD): ")]
        public string enclosure_4_it { get; set; }
        [Display(Name = "Enclosure 5 (Part number HxWxD): ")]
        public string enclosure_5_it { get; set; }
        [Display(Name = "Enclosure 1 (Part number HxWxD): ")]
        public string enclosure_1_ie { get; set; }
        [Display(Name = "Enclosure 2 (Part number HxWxD): ")]
        public string enclosure_2_ie { get; set; }
        [Display(Name = "Enclosure 3 (Part number HxWxD): ")]
        public string enclosure_3_ie { get; set; }
        [Display(Name = "Enclosure 4 (Part number HxWxD): ")]
        public string enclosure_4_ie { get; set; }
        [Display(Name = "Enclosure 5 (Part number HxWxD): ")]
        public string enclosure_5_ie { get; set; }
        [Display(Name = "Does Enclosure need to be bayed?: ")]
        public string part_type_it { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_ie { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_WM_AE_JB { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_other { get; set; }
        [NotMapped]
        public string other_sap_account_num { get; set; }
        [Display(Name = "End User Name: *")]
        public string end_user_name { get; set; }
        [Display(Name = "End User Location (City/State):")]
        public string end_user_location { get; set; }
        [NotMapped]
        public List<string> products { get; set; }
        [NotMapped]
        public List<string> productMaterials { get; set; }
        [NotMapped]
        public List<string> productFamilies { get; set; }
        [NotMapped]
        public List<string> productRatings { get; set; }
        [NotMapped]
        public List<string> productEnclosureTypes { get; set; }
        [NotMapped]
        public List<string> productTypes { get; set; }
        [NotMapped]
        public Dictionary<string, List<string>> productAccessories { get; set; }
        [Display(Name = "Accessories:")]
        public string accessories_it { get; set; }
        [Display(Name = "Surface(s) to Modify:")]
        public string surface_to_modify { get; set; }
        [NotMapped]
        public string[] accessories { get; set; }
        [NotMapped]
        public string[] surfaces { get; set; }
        [NotMapped]
        public string[] mods { get; set; }
        public string enclosures { get; set; }
        [NotMapped]
        public Dictionary<string,List<string>> accessoryFilters { get; set; }
        public Nullable<short> default_baying_accessories { get; set; }
        public string baying_accessories { get; set; }
        [NotMapped]
        public Dictionary<string,string> enclosureAccessories { get; set; }
        [NotMapped]
        public Dictionary<string,Dictionary<string, string>> climateParts { get; set; }
        public string climate_spare { get; set; }
        public string product_industry { get; set; }
        [Display(Name = "Notes: *")]
        public string notes { get; set; }
        public short? variant { get; set; }
    }

    [Table("RFQ_New_Data_Extend")]
    public class RFQNewViewModelExt
    {
        public int id { get; set; }
        [Display(Name = "RFQ ID: *")]
        public string form_id { get; set; }
        [Display(Name = "Product Number: *")]
        public Nullable<int> prod_id { get; set; }
        [Display(Name = "Total Qty: *")]
        public string total_qty { get; set; }
        [Display(Name = "Release Qty: *")]
        public string release_qty { get; set; }
        [Display(Name = "Target Price: ")]
        public string target_price { get; set; }
        [Display(Name = "Product Category: *")]
        public string product_category { get; set; }
        public IEnumerable<SelectListItem> list_prod_cat { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_data { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_non_data { get; set; }
        [Display(Name = "Enclosure Type: ")]
        public string enclosure_type_it { get; set; }
        [Display(Name = "Part No: ")]
        public string part_num_it { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_it { get; set; }
        [Display(Name = "Color: ")]
        public string color_it { get; set; }
        [Display(Name = "Sidewall Style: ")]
        public string sidewall_style_it { get; set; }
        [Display(Name = "Sidewall location: ")]
        public string sidewall_location_it { get; set; }
        [Display(Name = "Castors: ")]
        public string castors_it { get; set; }
        [Display(Name = "Leveling feet: ")]
        public string Leveling_feet_it { get; set; }
        [Display(Name = "Front: ")]
        public string front_it { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_it { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_it { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_it { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_it { get; set; }
        [Display(Name = "Partition Wall: ")]
        public string partition_wall_it { get; set; }
        [Display(Name = "Baffles: ")]
        public string baffles_it { get; set; }
        [Display(Name = "Baying Brackets: ")]
        public string bsaying_brackets_it { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_datacenter { get; set; }
        [Display(Name = "Intelligence: ")]
        public string intell_data { get; set; }
        [Display(Name = "Voltage: ")]
        public string voltage_data { get; set; }
        [Display(Name = "Amperage: ")]
        public string amp_data { get; set; }
        [Display(Name = "Outlet Type: ")]
        public string outlet_it { get; set; }
        [Display(Name = "Quantity of Type: ")]
        public string quantity_type_data { get; set; }
        [Display(Name = "Input Cord Plug Type: ")]
        public string input_cord_it { get; set; }
        [Display(Name = "Expansion Unit (Check For Yes): ")]
        public string expansion_it { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_ie { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_ie { get; set; }
        [Display(Name = "Material: ")]
        public string material_ie { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_ie { get; set; }
        [Display(Name = "Sidewalls: ")]
        public string sidewall_ie { get; set; }
        [Display(Name = "Front: ")]
        public string front_ie { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_ie { get; set; }
        [Display(Name = "Plinths: ")]
        public string plinths_ie { get; set; }
        [Display(Name = "Plinths Type: ")]
        public string plinths_type_ie { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_ie { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_ie { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_ie { get; set; }
        [Display(Name = "19 inch Rails: ")]
        public string Rails { get; set; }
        [Display(Name = "Suited?: ")]
        public string Suited { get; set; }
        [Display(Name = "If suited, which bay is this? Lt to Rt: ")]
        public string suited_bay_ie { get; set; }
        [Display(Name = "Door Modified?: ")]
        public string door_ie { get; set; }
        [Display(Name = "Roof Modified?: ")]
        public string roof_ie { get; set; }
        [Display(Name = "Rear wall modified?: ")]
        public string rear_wall_ie { get; set; }
        [Display(Name = "Sidewalls modified?: ")]
        public string sidewall_mod_ie { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_mod_ie { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_ie { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_ie_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_ie { get; set; }
        [Display(Name = "UL/NEMA Rating: ")]
        public string ul_nema_other_ie { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_ie { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_WM_AE_JB { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_WM_AE_JB { get; set; }
        [Display(Name = "Material: ")]
        public string material_WM_AE_JB { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_WM_AE_JB { get; set; }
        [Display(Name = "Latching: ")]
        public string latching_wm { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_wm { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_wm { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_wm { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_wm { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_wm_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_WM_AE_JB { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_WM_AE_JB { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_WM_AE_JB { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_other_1 { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_other_1 { get; set; }
        [Display(Name = "Prod Type: ")]
        public string producttype_other_1 { get; set; }
        [Display(Name = "Material: ")]
        public string material_other_1 { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_other_1 { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_other_1 { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_other_1 { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_other_1 { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string specialpaint_other { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_other { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_other_1 { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_other_1 { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_footer { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_3 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_3 { get; set; }
        public string send { get; set; }
        [Display(Name = "Project Drawing Upload: ")]
        public string Image_Name { get; set; }
        [NotMapped]
        public List<RFQNewViewModelExtPart> RFQExt { get; set; }
        [NotMapped]
        public RFQNewViewModel mainModel { get; set; }
        [NotMapped]
        public string save { get; set; }
        [NotMapped]
        public List<ProductCategories> list_product_cats { get; set; }
        [NotMapped]
        public List<RFQ_New_File> list_RFQ_files { get; set; }
        public IQueryable<RFQ_Parts_Installed> list_installed_parts { get; set; }
        public IQueryable<RFQ_Parts_Shipped> list_shipped_parts { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
    }

    public class RFQNewViewModelExtPart
    {
        public string rfqid { get; set; }
        public int rfqidExt { get; set; }
        public string product_categories { get; set; }
        public string total_quantity { get; set; }   
        public RFQNewViewModelExt extModel { get; set; }
    }

    [Table("RFQ_New_Action_Log")]
    public partial class RFQ_New_Action_LogViewModel
    {
        [Key]
        public int ID { get; set; }
        public int Form_ID { get; set; }
        public string Action { get; set; }
        public Nullable<System.DateTime> Action_Time { get; set; }
        public string Notes { get; set; }
        public string Usr_ID { get; set; }
        public string Admin_ID { get; set; }
        [NotMapped]
        public string fullName { get; set; }
        [NotMapped]
        public string AdminfullName { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
    }

    public class ProductCategories
    {
        public string cat_name { get; set; }
        public string value { get; set; }
    }

    [Table("RFQ_New_Files")]
    public partial class RFQ_New_File
    {
        [Key]
        public Nullable<int> file_id { get; set; }
        public Nullable<int> form_id { get; set; }
        public Nullable<int> ext_form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
    }

    [Table("RFQ_Parts_Installed")]
    public partial class RFQ_Parts_InstalledViewModel
    {
        [Key]
        public Nullable<int> id { get; set; }
        public Nullable<int> form_id { get; set; }
        public Nullable<int> ext_form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public string qty_installed { get; set; }
        public string part_number_installed { get; set; }
        public string description_installed { get; set; }
    }

    [Table("RFQ_Parts_Shipped")]
    public partial class RFQ_Parts_ShippedViewModel
    {
        [Key]
        public Nullable<int> ID { get; set; }
        public Nullable<int> form_id { get; set; }
        public Nullable<int> ext_form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public string qty_shipped { get; set; }
        public string part_number_shipped { get; set; }
        public string description_shipped { get; set; }
    }

    public class CloneParts
    {
        public int new_id { get; set; }
    }

    public class RFQReportModel
    {
        public int form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> comp_type { get; set; }
        public string quote_num { get; set; }
        public string requestor { get; set; }
        public string comp_name { get; set; }
        public string qte_ref { get; set; }
        public string end_user { get; set; }
        public DateTime? submission_date { get; set; }
        public DateTime? completion_date { get; set; }
        public string action { get; set; }
        public string adminfullName { get; set; }
        public bool IsCloned { get; set; }
        public bool IsReturned { get; set; }
        public double total_time { get; set; }
        public string admin_notes { get; set; }
        public string admin_status { get; set; }
        public string send { get; set; }
        public short check_project { get; set; }
        public List<RFQ_Action_Log> logs { get; set; }
        public double completion_time { get; set; }
    }

    public class RFQNewReportModel
    {
        public int form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> comp_type { get; set; }
        public string quote_num { get; set; }
        public string requestor { get; set; }
        public string comp_name { get; set; }
        public string qte_ref { get; set; }
        public string end_user { get; set; }
        public DateTime? submission_date { get; set; }
        public DateTime? completion_date { get; set; }
        public string action { get; set; }
        public string adminfullName { get; set; }
        public bool IsCloned { get; set; }
        public bool IsReturned { get; set; }
        public double total_time { get; set; }
        public string admin_notes { get; set; }
        public string admin_status { get; set; }
        public string send { get; set; }
        public short check_project { get; set; }
        public List<RFQ_New_Action_Log> logs { get; set; }
        public double completion_time { get; set; }
    }

    public class RFQViewModelReport
    {
        public string company { get; set; }
        public IEnumerable<SelectListItem> list_comps { get; set; }
        public string users { get; set; }
        public IEnumerable<SelectListItem> list_users { get; set; }
        public IEnumerable<RFQViewModel> rfq_model { get; set; }
        public ICollection<RFQReportModel> list_reports_model { get; set; }
        public int totalReturnedforms { get; set; }
        public int averageHoursCalc { get; set; }
        public int averageHoursCalcWithoutWait { get; set; }
        public double averageUnder24HoursCalc { get; set; }
        public double averageUnder24HoursCalcWithoutWait { get; set; }
        public int percUnder24HoursCalc { get; set; }
        public int percUnder24HoursCalcWithoutWait { get; set; }
        public double hoursUnder24 { get; set; }
        public double hoursUnder24WithoutWait { get; set; }
        public double totalForms { get; set; }
        public int countUnder24 { get; set; }
        public int totalUnder24 { get; set; }
        public int totalUnder48 { get; set; }

        public Dictionary<string,double> waitStatusTimes { get; set; }
    }

    public class RFQNewViewModelReport
    {
        public string company { get; set; }
        public IEnumerable<SelectListItem> list_comps { get; set; }
        public string users { get; set; }
        public IEnumerable<SelectListItem> list_users { get; set; }
        public IEnumerable<RFQNewViewModel> rfq_model { get; set; }
        public ICollection<RFQNewReportModel> list_reports_model { get; set; }
        public int totalReturnedforms { get; set; }
        public int averageHoursCalc { get; set; }
        public int averageHoursCalcWithoutWait { get; set; }
        public double averageUnder24HoursCalc { get; set; }
        public double averageUnder24HoursCalcWithoutWait { get; set; }
        public int percUnder24HoursCalc { get; set; }
        public int percUnder24HoursCalcWithoutWait { get; set; }
        public double hoursUnder24 { get; set; }
        public double hoursUnder24WithoutWait { get; set; }
        public double totalForms { get; set; }
        public int countUnder24 { get; set; }
        public int totalUnder24 { get; set; }
        public int totalUnder48 { get; set; }

        public Dictionary<string, double> waitStatusTimes { get; set; }
    }

    public class Yearly_holidays
    {
        public string thanksgivingEve { get; set; }
        public string thanksgivingDay { get; set; }
        public string christmasEve { get; set; }
        public string christmasDay { get; set; }
        public string NewYearsDay { get; set; }
        public string BoxingDay { get; set; }
        public string TwentyNith { get; set; }
        public string memorialDay { get; set; }
        public string independenceDay { get; set; }
        public string labourDay { get; set; }

    }

    [Table("RFQ_RAS_Data")]
    public class RFQRASViewModel
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "Account Manager: *")]
        [Required]
        public string account_manager { get; set; }
        [Display(Name = "Regional Manager: *")]
        [Required]
        public string regional_manager { get; set; }
        [Display(Name = "Machine Type: *")]
        [Required]
        public string machine_type { get; set; }
        [Display(Name = "Competitor: *")]
        [Required]
        public string competitor { get; set; }
        [Display(Name = "Opportunity ID: *")]
        public int opportunity_id { get; set; }
        [Display(Name = "ERP ID: *")]
        [Required]
        public int erp_id { get; set; }
        [Display(Name = "Company Name: *")]
        [Required]
        public string company { get; set; }
        [Display(Name = "Contact Name: *")]
        [Required]
        public string contact_name { get; set; }
        [Display(Name = "Phone Number: *")]
        [Required]
        public string phone_number { get; set; }
        [Display(Name = "Email: *")]
        [Required]
        public string email { get; set; }
        [Display(Name = "Street Address: *")]
        [Required]
        public string street_address { get; set; }
        [Display(Name = "City: *")]
        [Required]
        public string city { get; set; }
        [Display(Name = "State: *")]
        [Required]
        public string state { get; set; }
        [Display(Name = "Zip Code: *")]
        [Required]
        public string zipcode { get; set; }
        [Display(Name = "Delivery Type: *")]
        [Required]
        public string delivery_type { get; set; }
        [Display(Name = "Transformer Voltage: *")]
        [Required]
        public string transformer_voltage { get; set; }
        [Display(Name = "Equipment Options:")]
        public string equipment_options { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> listMachineTypes { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> listDeliveryTypes { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> listVoltage { get; set; }
        public object this[string propertyName]
        {
            get
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                return property.GetValue(this, null);
            }
            set
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                property.SetValue(this, value, null);
            }
        }
    }

    public class RFQSearchProduct
    {
        public string part_number { get; set; }
        public string enclosure_type { get; set; }
        public string material { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int depth { get; set; }
        public string family { get; set; }
        public string rating { get; set; }
        public string product_type { get; set; }
        public string industry { get; set; }
        public bool return_filters { get; set; }
        public string dimension_type { get; set; }
        public string height_IT { get; set; }
        public int width_IT { get; set; }
        public int depth_IT { get; set; }
    }

    public class RFQSearchAccessory
    {
        public string part_number { get; set; }
        public string product_type { get; set; }
        public string product_family { get; set; }
        public string group_1 { get; set; }
        public string group_2 { get; set; }
        public string description_1 { get; set; }
        public string description_2 { get; set; }
        public string description_3 { get; set; }
        public string color { get; set; }
        public string material { get; set; }
    }

    public class RFQSearchClimate
    {
        public string type { get; set; }
        public string voltage { get; set; }
        public string cfm { get; set; }
        public string color { get; set; }
        public string btu { get; set; }
        public string version { get; set; }
        public string material { get; set; }
        public string ul_nema { get; set; }
        public string kw { get; set; }
        public string phase { get; set; }
        public string part_number { get; set; }
    }

    public class RFQEditEnclosure
    {
        public int rfqID { get; set; }
        public string enclosures { get; set; }
        public short? variant { get; set; }
    }

    [Table("RFQ_Data")]
    public class RFQViewModel
    {
        [Key]
        public int ID { get; set; }
        [NotMapped]
        public string rfq_type { get; set; }
        [Display(Name = "Requested By: *")]
        [Required]
        public string sales_engineer { get; set; }
        [Display(Name = "Regional General Manager: *")]
        public string regional_director { get; set; }
        [Display(Name = "Rittal Account Manager: *")]
        [Required]
        public string account_manager { get; set; }
        [Display(Name = "Your Contact Number: ")]
        public string cell_phone { get; set; }
        [Display(Name = "Your Email: ")]
        public string email { get; set; }
        [Display(Name = "Date Submitted: *")]
        [Required]
        public DateTime? submission_date { get; set; }
        [Display(Name = "Completion Date: *")]
        public DateTime? completion_date { get; set; }
        [Display(Name = "New quote or quote revision: *")]
        [Required]
        public string updated_quote { get; set; }
        [Display(Name = "Sold to Party Name: ")]
        public string sold_to_party { get; set; }
        [Display(Name = "Quote Number / Line Item updating: *")]
        public string qte_num { get; set; }
        [Display(Name = "SAP Account Number: *")]
        [Required]
        public string sap_account_num { get; set; }
        [Display(Name = "Location: ")]
        public string location { get; set; }
        [Display(Name = "Sold to Contact:")]
        public string end_contact { get; set; }
        [Display(Name = "Opportunity Number: ")]
        public string opportunity_num { get; set; }
        [Display(Name = "Project Name: *")]
        public string qte_ref { get; set; }
        [Display(Name = "End User:")]
        public string end_user { get; set; }
        [Display(Name = "Quote Reference Description: ")]
        public string qte_description { get; set; }
        [Display(Name = "Drawing Number: ")]
        public string draw_num { get; set; }
        [Display(Name = "Total Qty/EAU: *")]
        public string total_qty { get; set; }
        [Display(Name = "Release Qty: *")]
        public string release_qty { get; set; }
        [Display(Name = "Competition: ")]
        public string competition { get; set; }
        [Display(Name = "Target Price: ")]
        public string target_price { get; set; }
        [Display(Name = "Scaled/Volume Pricing: ")]
        public string scale_volume { get; set; }
        [Display(Name = "SPA Contract Number: ")]
        public string spa_contract_num { get; set; }
        [Display(Name = "SPA Mult: ")]
        public string spa_mult { get; set; }
        [Display(Name = "Drawing for Approval: ")]
        public Nullable<short> drawing_approval { get; set; }
        [Display(Name = "Check if a Project: ")]
        public Nullable<short> check_project { get; set; }
        [Display(Name = "Product Category: *")]
        public string product_category { get; set; }
        public IEnumerable<SelectListItem> listSAP { get; set; }
        public IEnumerable<SelectListItem> list_prod_cat { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_data { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_non_data { get; set; }
        [Display(Name = "Enclosure Type: ")]
        public string enclosure_type_it { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_it { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_it { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_it { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_it { get; set; }
        [Display(Name = "Color: ")]
        public string color_it { get; set; }
        [Display(Name = "Sidewall Style: ")]
        public string sidewall_style_it { get; set; }
        [Display(Name = "Sidewall location: ")]
        public string sidewall_location_it { get; set; }
        [Display(Name = "Castors: ")]
        public string castors_it { get; set; }
        [Display(Name = "Leveling feet: ")]
        public string Leveling_feet_it { get; set; }
        [Display(Name = "Front: ")]
        public string front_it { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_it { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_it { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_it { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_it { get; set; }
        [Display(Name = "Partition Wall: ")]
        public string partition_wall_it { get; set; }
        [Display(Name = "Baffles: ")]
        public string baffles_it { get; set; }
        [Display(Name = "Baying Brackets: ")]
        public string bsaying_brackets_it { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_datacenter { get; set; }
        [Display(Name = "Intelligence: ")]
        public string intell_data { get; set; }
        [Display(Name = "Voltage: ")]
        public string voltage_data { get; set; }
        [Display(Name = "Amperage: ")]
        public string amp_data { get; set; }
        [Display(Name = "Outlet Type: ")]
        public string outlet_it { get; set; }
        [Display(Name = "Quantity of Type: ")]
        public string quantity_type_data { get; set; }
        [Display(Name = "Input Cord Plug Type: ")]
        public string input_cord_it { get; set; }
        [Display(Name = "Expansion Unit (Check For Yes): ")]
        public string expansion_it { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_ie { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_ie { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_ie { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_ie { get; set; }
        [Display(Name = "Material: ")]
        public string material_ie { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_ie { get; set; }
        [Display(Name = "Sidewalls: ")]
        public string sidewall_ie { get; set; }
        [Display(Name = "Front: ")]
        public string front_ie { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_ie { get; set; }
        [Display(Name = "Plinths: ")]
        public string plinths_ie { get; set; }
        [Display(Name = "Plinths Type: ")]
        public string plinths_type_ie { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_ie { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_ie { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_ie { get; set; }
        [Display(Name = "19 inch Rails: ")]
        public string Rails { get; set; }
        [Display(Name = "Suited?: ")]
        public string Suited { get; set; }
        [Display(Name = "If suited, which bay is this? Lt to Rt: ")]
        public string suited_bay_ie { get; set; }
        [Display(Name = "Door Modified?: ")]
        public string door_ie { get; set; }
        [Display(Name = "Roof Modified?: ")]
        public string roof_ie { get; set; }
        [Display(Name = "Rear wall modified?: ")]
        public string rear_wall_ie { get; set; }
        [Display(Name = "Sidewalls modified?: ")]
        public string sidewall_mod_ie { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_mod_ie { get; set; }
        [Display(Name = "Special Paint Types: ")]
        public string special_paint_ie { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_ie_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_ie { get; set; }
        [Display(Name = "UL/NEMA Rating: ")]
        public string ul_nema_other_ie { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_ie { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_WM_AE_JB { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_WM_AE_JB { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_WM_AE_JB { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_WM_AE_JB { get; set; }
        [Display(Name = "Material: ")]
        public string material_WM_AE_JB { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_WM_AE_JB { get; set; }
        [Display(Name = "Latching: ")]
        public string latching_wm { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_wm { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_wm { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_wm { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_wm { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_wm_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_WM_AE_JB { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_WM_AE_JB { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_WM_AE_JB { get; set; }
        [Display(Name = "Part No. being modified: ")]
        public string part_num_other_1 { get; set; }
        [Display(Name = "Dimensions of product being modified: ")]
        public string size_hxwxd_other_1 { get; set; }
        [Display(Name = "Is this enclosure a special dimension?: ")]
        public Nullable<short> special_dimension_other { get; set; }
        [Display(Name = "How are we to modify?: ")]
        public string mods_other { get; set; }
        [Display(Name = "Prod Type: ")]
        public string producttype_other_1 { get; set; }
        [Display(Name = "Material: ")]
        public string material_other_1 { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_other_1 { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_other_1 { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_other_1 { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_other_1 { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string specialpaint_other { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_other { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_other_1 { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_other_1 { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_footer { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_3 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_3 { get; set; }
        public string send { get; set; }
        public string admin_status { get; set; }
        [NotMapped]
        [Display(Name = "Document Upload: ")]
        public IEnumerable<HttpPostedFileBase> fileupload { get; set; }
        public int? user_id { get; set; }
        public string save { get; set; }
        [Display(Name = "Contact Person Name: *")]
        public string distro_name { get; set; }
        [Display(Name = "Company/Location Name: *")]
        public string distro_company { get; set; }
        [NotMapped]
        public string form_id { get; set; }
        [NotMapped]
        [Display(Name = "Project Drawing Upload: ")]
        public string Image_Name { get; set; }
        [NotMapped]
        public List<RFQViewModelExtPart> RFQExt { get; set; }
        [NotMapped]
        public List<RFQ_File> list_RFQ_files { get; set; }
        [NotMapped]
        public string admin_notes { get; set; }
        [NotMapped]
        public List<RFQ_Action_LogViewModel> list_RFQ_logs { get; set; }
        public IQueryable<RFQ_Parts_Installed> list_installed_parts { get; set; }
        public IQueryable<RFQ_Parts_Shipped> list_shipped_parts { get; set; }
        [Display(Name = "Quote Number: ")]
        public string Quote_Num { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
        [NotMapped]
        public string requestor { get; set; }
        [NotMapped]
        public string comp_name { get; set; }
        [Display(Name = "Deal Registration Number: ")]
        public string deal_registration { get; set; }
        [Display(Name = "No. of Enclosures: ")]
        public Nullable<short> suited_enclosures_it { get; set; }
        [Display(Name = "No. of Enclosures: ")]
        public Nullable<short> suited_enclosures_ie { get; set; }
        [Display(Name = "Enclosure 1 (Part number HxWxD): ")]
        public string enclosure_1_it { get; set; }
        [Display(Name = "Enclosure 2 (Part number HxWxD): ")]
        public string enclosure_2_it { get; set; }
        [Display(Name = "Enclosure 3 (Part number HxWxD): ")]
        public string enclosure_3_it { get; set; }
        [Display(Name = "Enclosure 4 (Part number HxWxD): ")]
        public string enclosure_4_it { get; set; }
        [Display(Name = "Enclosure 5 (Part number HxWxD): ")]
        public string enclosure_5_it { get; set; }
        [Display(Name = "Enclosure 1 (Part number HxWxD): ")]
        public string enclosure_1_ie { get; set; }
        [Display(Name = "Enclosure 2 (Part number HxWxD): ")]
        public string enclosure_2_ie { get; set; }
        [Display(Name = "Enclosure 3 (Part number HxWxD): ")]
        public string enclosure_3_ie { get; set; }
        [Display(Name = "Enclosure 4 (Part number HxWxD): ")]
        public string enclosure_4_ie { get; set; }
        [Display(Name = "Enclosure 5 (Part number HxWxD): ")]
        public string enclosure_5_ie { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_it { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_ie { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_WM_AE_JB { get; set; }
        [Display(Name = "Part Type: ")]
        public string part_type_other { get; set; }
        [NotMapped]
        public string other_sap_account_num { get; set; }
        [Display(Name = "End User Name: *")]
        public string end_user_name { get; set; }
        [Display(Name = "End User Location (City/State):")]
        public string end_user_location { get; set; }
        [NotMapped]
        public string company_region { get; set; }
    }

    [Table("RFQ_Data_Extend")]
    public class RFQViewModelExt
    {
        public int id { get; set; }
        [Display(Name = "RFQ ID: *")]
        public string form_id { get; set; }
        [Display(Name = "Product Number: *")]
        public Nullable<int> prod_id { get; set; }
        [Display(Name = "Total Qty: *")]
        public string total_qty { get; set; }
        [Display(Name = "Release Qty: *")]
        public string release_qty { get; set; }
        [Display(Name = "Target Price: ")]
        public string target_price { get; set; }
        [Display(Name = "Product Category: *")]
        [Required]
        public string product_category { get; set; }
        public IEnumerable<SelectListItem> list_prod_cat { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_data { get; set; }
        [Display(Name = "Xpress Mod/Paint: *")]
        public string xpress_mod_non_data { get; set; }
        [Display(Name = "Enclosure Type: ")]
        public string enclosure_type_it { get; set; }
        [Display(Name = "Part No: ")]
        public string part_num_it { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_it { get; set; }
        [Display(Name = "Color: ")]
        public string color_it { get; set; }
        [Display(Name = "Sidewall Style: ")]
        public string sidewall_style_it { get; set; }
        [Display(Name = "Sidewall location: ")]
        public string sidewall_location_it { get; set; }
        [Display(Name = "Castors: ")]
        public string castors_it { get; set; }
        [Display(Name = "Leveling feet: ")]
        public string Leveling_feet_it { get; set; }
        [Display(Name = "Front: ")]
        public string front_it { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_it { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_it { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_it { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_it { get; set; }
        [Display(Name = "Partition Wall: ")]
        public string partition_wall_it { get; set; }
        [Display(Name = "Baffles: ")]
        public string baffles_it { get; set; }
        [Display(Name = "Baying Brackets: ")]
        public string bsaying_brackets_it { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_datacenter { get; set; }
        [Display(Name = "Intelligence: ")]
        public string intell_data { get; set; }
        [Display(Name = "Voltage: ")]
        public string voltage_data { get; set; }
        [Display(Name = "Amperage: ")]
        public string amp_data { get; set; }
        [Display(Name = "Outlet Type: ")]
        public string outlet_it { get; set; }
        [Display(Name = "Quantity of Type: ")]
        public string quantity_type_data { get; set; }
        [Display(Name = "Input Cord Plug Type: ")]
        public string input_cord_it { get; set; }
        [Display(Name = "Expansion Unit (Check For Yes): ")]
        public string expansion_it { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_ie { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_ie { get; set; }
        [Display(Name = "Material: ")]
        public string material_ie { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_ie { get; set; }
        [Display(Name = "Sidewalls: ")]
        public string sidewall_ie { get; set; }
        [Display(Name = "Front: ")]
        public string front_ie { get; set; }
        [Display(Name = "Rear: ")]
        public string rear_ie { get; set; }
        [Display(Name = "Plinths: ")]
        public string plinths_ie { get; set; }
        [Display(Name = "Plinths Type: ")]
        public string plinths_type_ie { get; set; }
        [Display(Name = "Cable Entry: ")]
        public string cable_ie { get; set; }
        [Display(Name = "Handles: ")]
        public string handles_ie { get; set; }
        [Display(Name = "Inserts: ")]
        public string inserts_ie { get; set; }
        [Display(Name = "19 inch Rails: ")]
        public string Rails { get; set; }
        [Display(Name = "Suited?: ")]
        public string Suited { get; set; }
        [Display(Name = "If suited, which bay is this? Lt to Rt: ")]
        public string suited_bay_ie { get; set; }
        [Display(Name = "Door Modified?: ")]
        public string door_ie { get; set; }
        [Display(Name = "Roof Modified?: ")]
        public string roof_ie { get; set; }
        [Display(Name = "Rear wall modified?: ")]
        public string rear_wall_ie { get; set; }
        [Display(Name = "Sidewalls modified?: ")]
        public string sidewall_mod_ie { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_mod_ie { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_ie { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_ie_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_ie { get; set; }
        [Display(Name = "UL/NEMA Rating: ")]
        public string ul_nema_other_ie { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_ie { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_WM_AE_JB { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_WM_AE_JB { get; set; }
        [Display(Name = "Material: ")]
        public string material_WM_AE_JB { get; set; }
        [Display(Name = "Mpl: ")]
        public string mpl_WM_AE_JB { get; set; }
        [Display(Name = "Latching: ")]
        public string latching_wm { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_wm { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_wm { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_wm { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string special_paint_wm { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_wm_1 { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_WM_AE_JB { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_WM_AE_JB { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_WM_AE_JB { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_num_other_1 { get; set; }
        [Display(Name = "Size HxWxD: ")]
        public string size_hxwxd_other_1 { get; set; }
        [Display(Name = "Prod Type: ")]
        public string producttype_other_1 { get; set; }
        [Display(Name = "Material: ")]
        public string material_other_1 { get; set; }
        [Display(Name = "Body Modified?: ")]
        public string body_modified_other_1 { get; set; }
        [Display(Name = "Door modified?: ")]
        public string door_modified_other_1 { get; set; }
        [Display(Name = "MPL Modified?: ")]
        public string mpl_modified_other_1 { get; set; }
        [Display(Name = "Special Paint?: ")]
        public string specialpaint_other_1 { get; set; }
        [Display(Name = "Special Paint Type: ")]
        public string specialpaint_other { get; set; }
        [Display(Name = "28 color selection: ")]
        public string color_mod_other { get; set; }
        [Display(Name = "UL/NEMA: ")]
        public string ul_nema_other_1 { get; set; }
        [Display(Name = "Rating: ")]
        public string rating_other_1 { get; set; }
        [Display(Name = "Additional information: ")]
        public string additional_info_footer { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_installed_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_installed_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_installeed_3 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_1 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_1 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_1 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_2 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_2 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_2 { get; set; }
        [Display(Name = "Qty: ")]
        public string qty_shipped_3 { get; set; }
        [Display(Name = "Part No.: ")]
        public string part_number_shipped_3 { get; set; }
        [Display(Name = "Description: ")]
        public string description_shipped_3 { get; set; }
        public string send { get; set; }
        [Display(Name = "Project Drawing Upload: ")]
        public string Image_Name { get; set; }
        [NotMapped]
        public List<RFQViewModelExtPart> RFQExt { get; set; }
        [NotMapped]
        public RFQViewModel mainModel { get; set; }
        [NotMapped]
        public string save { get; set; }
        [NotMapped]
        public List<ProductCategories> list_product_cats { get; set; }
        [NotMapped]
        public List<RFQ_File> list_RFQ_files { get; set; }
        public IQueryable<RFQ_Parts_Installed> list_installed_parts { get; set; }
        public IQueryable<RFQ_Parts_Shipped> list_shipped_parts { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
    }

    public class RFQViewModelExtPart
    {
        public string rfqid { get; set; }
        public int rfqidExt { get; set; }
        public string product_categories { get; set; }
        public string total_quantity { get; set; }
        public RFQViewModelExt extModel { get; set; }
    }

    [Table("RFQ_Action_Log")]
    public partial class RFQ_Action_LogViewModel
    {
        [Key]
        public int ID { get; set; }
        public int Form_ID { get; set; }
        public string Action { get; set; }
        public Nullable<System.DateTime> Action_Time { get; set; }
        public string Notes { get; set; }
        public string Usr_ID { get; set; }
        public string Admin_ID { get; set; }
        [NotMapped]
        public string fullName { get; set; }
        [NotMapped]
        public string AdminfullName { get; set; }
        [NotMapped]
        public bool IsCloned { get; set; }
    }

    [Table("RFQ_Files")]
    public partial class RFQ_File
    {
        [Key]
        public Nullable<int> file_id { get; set; }
        public Nullable<int> form_id { get; set; }
        public Nullable<int> ext_form_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
    }

}
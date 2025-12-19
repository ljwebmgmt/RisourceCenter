using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("usr_user")]
    public class UserViewModel
    {
        [Key]
        public int usr_ID { get; set; }
        [Display(Name = "First Name")]
        public string usr_fName { get; set; }
        [Display(Name = "Last Name")]
        public string usr_lName { get; set; }
        public string usr_password { get; set; }
        [Display(Name = "Email")]
        public string usr_email { get; set; }
        [Display(Name = "Title")]
        public string usr_title { get; set; }
        [Display(Name = "Address 1")]
        public string usr_add1 { get; set; }
        [Display(Name = "Address 2")]
        public string usr_add2 { get; set; }
        [Display(Name = "City")]
        public string usr_city { get; set; }
        [Required]
        [Display(Name = "State")]
        public Nullable<int> usr_state { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_state_ids { get; set; }
        [Required]
        [Display(Name = "Country")]
        public Nullable<int> usr_country { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_country_ids { get; set; }

        [Display(Name = "Language")]
        public Nullable<int> usr_language { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_language_ids { get; set; }
        [Display(Name = "Zip")]
        public string usr_zip { get; set; }
        [Display(Name = "Phone")]
        public string usr_phone { get; set; }
        [Display(Name = "Fax")]
        public string usr_fax { get; set; }
        [Display(Name = "Website")]
        public string usr_web { get; set; }
        [Display(Name = "Theme")]
        public Nullable<int> admin_theme { get; set; }
        [Display(Name = "Company ID")]
        public Nullable<long> comp_ID { get; set; }
        public IEnumerable<Nav1List> list_comp_id { get; set; }
        [Display(Name = "Location ID")]
        public Nullable<long> comp_loc_ID { get; set; }
        public IEnumerable<Nav1List> list_loc_id { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_theme_name { get; set; }
        public string system_ID { get; set; }
        [Display(Name = "User Pages")]
        public string usr_pages { get; set; }
        [Display(Name = "User Role")]
        [NotMapped]
        public string role { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_roles { get; set; }
        public IEnumerable<Nav1List> admin_navs { get; set; }
        public IEnumerable<Nav1List> special_navs { get; set; }
        public List<Userlabels> userlabels { get; set; }
        public Nullable<byte> usr_role { get; set; }
        public Nullable<byte> usr_SAP { get; set; }
        [Display(Name = "Regional Director SPA/POS")]
        public Nullable<byte> usr_SPA { get; set; }
        [Display(Name = "Account Manager SPA/POS")]
        public Nullable<byte> usr_POS { get; set; }
        [Display(Name = "Rittal Sales")]
        public Nullable<int> usr_sales { get; set; }
        public Nullable<byte> usr_project_reg { get; set; }
        public Nullable<byte> usr_MDF { get; set; }
        public Nullable<byte> usr_FX { get; set; }
        public Nullable<byte> usr_siteRole { get; set; }
        public Nullable<System.DateTime> usr_dateCreated { get; set; }
        public Nullable<System.DateTime> usr_dateUpdated { get; set; }
        public Nullable<byte> wN { get; set; }
        public Nullable<long> old_usr_id { get; set; }
        public Nullable<long> old_UserID { get; set; }
        public Nullable<System.DateTime> usr_lastLogin { get; set; }
        [Display(Name = "Login Count")]
        public Nullable<long> usr_countLogin { get; set; }
        [Display(Name = "Jigsaw Password")]
        public string usr_jigsaw_password { get; set; }
        [Display(Name = "Jigsaw Login")]
        public string usr_jigsaw_login { get; set; }
        [Display(Name = "MDF Login")]
        public string usr_MDF_login { get; set; }
        [Display(Name = "MDF Password")]
        public string usr_MDF_password { get; set; }
        public Nullable<int> usr_rfq { get; set; }
        public Nullable<int> usr_RiCRM { get; set; }
        public Nullable<int> show_message { get; set; }
        public Nullable<int> usr_status { get; set; }
        [NotMapped]
        public bool usr_statuss { get; set; }
        [NotMapped]
        public string user_role { get; set; }

        [System.Web.Mvc.AllowHtml]
        [Display(Name = "Requested Access To:")]
        public string access_request { get; set; }
        public Nullable<Boolean> interlynx_user { get; set; }
        public byte? email_sent { get; set; }
        [NotMapped]
        public string str_status { get; set; }
        [NotMapped]
        public List<SelectListItem> Approver_listings { get; set; }
        [NotMapped]
        public List<long> approver_companyIds { get; set; }
        [Display(Name = "Region Approver:")]
        public string region_approver { get; set; }
        [NotMapped]
        public string old_approver { get; set; }
        [NotMapped]
        public string region_approver_name {  get; set; }
        public bool deleted { get; set; }
        public bool inactive { get; set; }
    }

    public class Userlabels
    {
        [Key]
        public int usr_ID { get; set; }
        public string[] usr_fName_label { get; set; }
        public string[] usr_lName_label { get; set; }
        public string[] usr_email_label { get; set; }
        public string[] usr_title_label { get; set; }
        public string[] usr_add1_label { get; set; }
        public string[] usr_add2_label { get; set; }
        public string[] usr_city_label { get; set; }
        public string[] usr_state_label { get; set; }
        public string[] usr_country_label { get; set; }
        public string[] usr_language_label { get; set; }
        public string[] usr_zip_label { get; set; }
        public string[] usr_phone_label { get; set; }
        public string[] usr_fax_label { get; set; }
        public string[] usr_web_label { get; set; }
        public string[] admin_theme_label { get; set; }
        public string[] comp_ID_label { get; set; }
        public string[] comp_loc_ID_label { get; set; }
        public string[] usr_pages_label { get; set; }
        public string[] role_label { get; set; }
        public string[] admin_navs_label { get; set; }
        public string[] special_navs_label { get; set; }
        public string[] select_role_label { get; set; }
        public string[] invoke_label { get; set; }
        public string[] password_label { get; set; }
        public string[] usr_sap_label { get; set; }
        public string[] usr_spa_label { get; set; }
        public string[] usr_pos_label { get; set; }
        public string[] usr_countLogin_label { get; set; }
        public string[] usr_jigsaw_password_label { get; set; }
        public string[] usr_jigsaw_login_label { get; set; }
        public string[] usr_MDF_login_label { get; set; }
        public string[] usr_MDF_password_label { get; set; }
        public string[] usr_wN_label { get; set; }
    }

    [Table("themes")]
    public class UserTheme
    {
        [Key]
        public int? theme_id { get; set; }
        [Display(Name = "Theme Name")]
        public string theme_name { get; set; }
    }

    public partial class AspNetUserRoles
    {
        [Display(Name = "User ID")]
        public long UserId { get; set; }
        [Display(Name = "Role ID")]
        public long RoleId { get; set; }
    }

    public partial class AspNetRoleViewModel
    {
        [Display(Name = "Role ID")]
        public string Id { get; set; }
        [Display(Name = "Role Name")]
        public string Name { get; set; }
    }

    public class Role
    {
        public string[] name { get; set; }
    }

    public class UserIds
    {
        public int ids { get; set; }
    }

    [Table("SiteApprovers")]
    public class SiteApprovers
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int CompType { get; set; }
        [NotMapped]
        public List<partnerType> list_Type { get; set; }
        [NotMapped]
        public List<Nav1List> country { get; set; }
    }

    public class LastLoginReportViewModel
    {
        public List<UserViewModel> users { get; set; }
        public Dictionary<long, string> companyNames { get; set; }
    }
}
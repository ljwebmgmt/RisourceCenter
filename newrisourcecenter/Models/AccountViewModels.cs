using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace newrisourcecenter.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

    }

    public class LoginPartnerPortalViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "First Name *")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 0)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name *")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Address Line 1: *")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2:")]
        public string Address2 { get; set; }

        [Required]
        [Display(Name = "City: *")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 0)]
        public string City { get; set; }

        [Required]
        [Display(Name = "State/Territory: *")]
        public int State { get; set; }
        //create a list for the company data in the RegisterViewModel
        public IEnumerable<System.Web.Mvc.SelectListItem> List_states { get; set; }

        [Required]
        [Display(Name = "Country *")]
        public int Country { get; set; }
        //create a list for the company data in the RegisterViewModel
        public IEnumerable<System.Web.Mvc.SelectListItem> List_countries { get; set; }

        [Display(Name = "Language *")]
        public int Language { get; set; }
        //create a list for the company data in the RegisterViewModel
        public IEnumerable<System.Web.Mvc.SelectListItem> List_languages { get; set; }

        [Required]
        [Display(Name = "Zip / Postal Code: *")]
        [RegularExpression("([a-zA-Z0-9]+)", ErrorMessage = "Only alphanumeric characters allowed in zipcode field")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        public string Zip { get; set; }

        [Display(Name = "Phone:")]
        public string Phone { get; set; }

        [Display(Name = "Fax:")]
        public string Fax { get; set; }

        [Display(Name = "Website:")]
        public string Website { get; set; }

        [Display(Name = "Weekly Newsletter:")]
        public bool Newsletter { get; set; }

        [Display(Name = "Company: *")]
        public long Company { get; set; }
        //create a list for the company data in the RegisterViewModel
        public IEnumerable<System.Web.Mvc.SelectListItem> Company_listings { get; set; }
        [Required]
        [Display(Name = "Location: *")]
        public long? Comp_loc_ID { get; set; }
        [NotMapped]
        [Display(Name = "If you know your SAP account number, please include it below: ")]
        public string Sap_numb { get; set; }

        public string Username { get; set; }

        [Display(Name = "SAP Webviewer (check price & availability, view order history and order tracking)")]
        public bool Usr_SAP { get; set; }

        [Display(Name = "Online Shop (online ordering, check price & availability, view quotation and order history)")]
        public bool Usr_POS { get; set; }

        [Display(Name = "SPA contracts and rebates")]
        public bool Usr_SPA { get; set; }

        [Display(Name = "Marketing development funds (MDF)")]
        public bool Usr_MDF { get; set; }

        [Display(Name = "Region Approver:")]
        public string Region_Approver { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Approver_listings { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class PreRegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    [Table("CountLoggedin")]
    public class CountLogged
    {
        public int ID { get; set; }
        public string  SystemID { get; set; }
    }
}


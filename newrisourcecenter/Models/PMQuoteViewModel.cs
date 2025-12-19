using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("PMQuote")]
    public class PMQuoteViewModel
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "Company Name: *")]
        [Required]
        public string company_name { get; set; }
        [Display(Name = "Contact Name: *")]
        [Required]
        public string contact_name { get; set; }
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
        [Display(Name = "Account Manager Name: *")]
        [Required]
        public string manager_name { get; set; }
        [Display(Name = "Title: *")]
        [Required]
        public string title { get; set; }
        [Display(Name = "Email: *")]
        [Required]
        public string email { get; set; }
        [Display(Name = "Heat Exchanger: ")]
        public short heat_exchanger { get; set; }
        [Display(Name = "A/C: ")]
        public short ac { get; set; }
        [Display(Name = "LCP-CW: ")]
        public short lcp_cw { get; set; }
        [Display(Name = "Chiller: ")]
        public short chiller { get; set; }
        [Display(Name = "LCP-DX: ")]
        public short lcp_dx { get; set; }
        [Display(Name = "LCP-DX Condenser: ")]
        public short lcp_dx_condenser { get; set; }
        [Display(Name = "DET-AC: ")]
        public short det_ac { get; set; }
        [Display(Name = "Enviromental Conditions: ")]
        public string enviromental_conditions { get; set; }
        [Display(Name = "Submited On: ")]
        public DateTime submitted_on { get; set; }
        [Display(Name = "Submited By: ")]
        public int submitted_by { get; set; }
        public string file_url { get; set; }
        [NotMapped]
        public int quote_number { get; set; }
        [NotMapped]
        public string generated_by { get; set; }
        [NotMapped]
        public string[] conditions { get; set; }
        [Display(Name = "Type of Service: *")]
        [Required]
        public string service_type { get; set; }
    }
}
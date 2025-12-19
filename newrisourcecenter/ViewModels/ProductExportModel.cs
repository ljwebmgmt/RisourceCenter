using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace newrisourcecenter.ViewModels
{
    public class ProductExportInputModel
    {
        [Required]
        public string category { get; set; }
        public string subcategory { get; set; }
    }

    public class ProductExportResponse
    {
        public string fileUrl { get; set; }
        public string fileName { get; set; }
        public string error { get; set; }
    }

    public class ProductExportModel
    {
        public Category[] categories { get; set; }
    }

    public class Category
    {
        public string code { get; set; }
        public string name { get; set; }
        public SubCategory[] subcategories { get; set; }
    }

    public class SubCategory
    {
        public string code { get; set; }
        public string name { get; set; }
    }
}
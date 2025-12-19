using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class NonPIMViewModel
    {
        public string partNumber { get; set; }
        public Dictionary<string,object> product { get; set; }
        public Dictionary<string, object> literature { get; set; }
        public DiscontinuedProduct discontinuedProduct { get; set; }
        public List<ImageFile> imageFiles { get; set; }
        public List<TextFeatures> textFeatures { get; set; }
        public Dimensions dimension { get; set; }
        public List<AdditionalSpecification> additionalSpecifications { get; set; }
        public List<Rating> ratings { get; set; }
        public List<Accessory> accessories { get; set; }
        public List<Dictionary<string,object>> uploads { get;set; }
        public Dictionary<string,List<Dictionary<string,object>>> catalogs { get; set; }
        public List<string> pdfList { get; set; }
        public List<string> dwfList { get; set; }
        public List<string> zipList { get; set; }
    }

    public class DiscontinuedProduct
    {
        public string disc_partnum { get; set; }
        public string prod_discdescription { get; set; }
        public string prod_disclookup { get; set; }
        public DateTime prod_discdate { get; set; }
        public string disc_desc { get; set; }
    }

    public class ImageFile
    {
        public string image_file { get; set; }
        public string image_desc { get; set; }
        public int image_order { get; set; }
    }

    public class TextFeatures
    {
        public string txt_marketing { get; set; }
        public string txt_type { get; set; }
        public string text_relate { get; set; }
    }

    public class Dimensions
    {
        public decimal prod_height { get; set; }
        public decimal prod_width { get; set; }
        public decimal prod_depth { get; set; }
    }

    public class AdditionalSpecification
    {
        public int as_ID { get; set; }
        public string asv_value { get; set; }
        public string as_desc { get; set; }
    }

    public class Rating
    {
        public string srate_desc { get; set; }
        public string prod_ratevalue { get; set; }
    }

    public class Accessory
    {
        public int aId { get; set; }
        public int titleID { get; set; }
        public string titleName { get; set; }
        public List<Description> descriptions { get; set; }
    }

    public class Description
    {
        public string prod_description { get; set; }
        public string use_prod_descsap { get; set; }
        public string prod_descsap { get; set; }
        public string specs_partnum { get; set; }

    }
}
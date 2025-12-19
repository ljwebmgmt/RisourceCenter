using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class LargeEnclosureViewModel
    {
        [Key]
        public int ID { get; set; }
        public string TS_Articelnummer { get; set; }
        public string Accessory_partnumber { get; set; }
        public string Doors { get; set; }
        public string Allocation_1 { get; set; }
        public string Allocation_1_1 { get; set; }
        public string Quantity_Per_Pack { get; set; }
        public string Number_of_Packs { get; set; }
        public string Description { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
        public string Unit_Cost { get; set; }
        public string Username { get; set; }
        public string Code { get; set; }
    }

    [Table("Large_enclosure_fmd")]
    public partial class Large_enclosureFmd
    {
        public int ID { get; set; }
        public string FMD_partnumber { get; set; }
        public string Accessory_partnumber { get; set; }
        public string Doors { get; set; }
        public string Allocation_1 { get; set; }
        public string Allocation_1_1 { get; set; }
        public string Quantity_Per_Pack { get; set; }
        public string Number_of_Packs { get; set; }
        public string Description { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
    }

    [Table("Large_enclosure_pricing")]
    public partial class Large_enclosurePricing
    {
        public int ID { get; set; }
        public string Doors { get; set; }
        public string Part_Number { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Part_Of_Selector { get; set; }
        public string Type { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
    }

    [Table("Large_enclosure_ts8")]
    public partial class Large_enclosureTs8
    {
        public int ID { get; set; }
        public string TS8_partnumber { get; set; }
        public string Accessory_partnumber { get; set; }
        public string Doors { get; set; }
        public string Allocation_1 { get; set; }
        public string Allocation_1_1 { get; set; }
        public string Quantity_Per_Pack { get; set; }
        public string Number_of_Packs { get; set; }
        public string Description { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
    }

    [Table("Large_enclosure_myaccessories")]
    public partial class LargeEnclosureMyaccessory
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Code { get; set; }
        public string Config_name { get; set; }
        public string Part_Number { get; set; }
        public string Accessory_Number { get; set; }
        public string Description { get; set; }
        public string Quantity_Per_Pack { get; set; }
        public string Number_of_Packs { get; set; }
        public string Unit_Cost { get; set; }
        public string Total_Cost { get; set; }
        public string Baying_NotBaying { get; set; }
        public string Type { get; set; }
        public string Doors { get; set; }
        public string Height { get; set; }
    }

    public class Heights
    {
        public string Height { get; set; }
    }

    public class Depths
    {
        public string Depth { get; set; }
    }

    public class Widths
    {
        public string Width { get; set; }
    }

    public class BayingNotBaying {
        public string TS_Articelnummer { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
        public string Doors { get; set; }
    }

    [Table("Large_enclosure_myconfig")]
    public class Config
    {
       [Key]
        public int ID { get; set; }
        public string Username { get; set; }
        public string Config_name { get; set; }
        public string Unique_id { get; set; }
        public string Materials { get; set; }
        public DateTime Date_created { get; set; }
        [NotMapped]
        public string DateCreated { get; set; }
    }

}
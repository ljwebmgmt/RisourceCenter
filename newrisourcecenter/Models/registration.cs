namespace newrisourcecenter.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("registration")]
    public partial class registration
    {
        [Key]
        public long reg_ID { get; set; }

        public int? reg_phase { get; set; }

        [StringLength(100)]
        public string comp_name { get; set; }

        public byte? comp_industry { get; set; }

        [StringLength(100)]
        public string loc_name { get; set; }

        [StringLength(300)]
        public string loc_add1 { get; set; }

        [StringLength(300)]
        public string loc_add2 { get; set; }

        [StringLength(50)]
        public string loc_city { get; set; }

        public int? loc_state { get; set; }

        [StringLength(10)]
        public string loc_zip { get; set; }

        [StringLength(20)]
        public string loc_phone { get; set; }

        [StringLength(20)]
        public string loc_fax { get; set; }

        [StringLength(300)]
        public string loc_web { get; set; }

        [StringLength(100)]
        public string loc_email { get; set; }

        [StringLength(50)]
        public string usr_fName { get; set; }

        [StringLength(50)]
        public string usr_lName { get; set; }

        [StringLength(50)]
        public string usr_position { get; set; }

        [StringLength(300)]
        public string usr_add1 { get; set; }

        [StringLength(300)]
        public string usr_add2 { get; set; }

        [StringLength(50)]
        public string usr_city { get; set; }

        public int? usr_state { get; set; }

        [StringLength(10)]
        public string usr_zip { get; set; }

        [StringLength(20)]
        public string usr_phone { get; set; }

        [StringLength(20)]
        public string usr_fax { get; set; }

        [StringLength(50)]
        public string usr_email { get; set; }

        [StringLength(20)]
        public string usr_password { get; set; }

        public DateTime? reg_dateSubmited { get; set; }

        public DateTime? reg_dateUpdated { get; set; }

        public long? reg_updateBy { get; set; }

        [StringLength(50)]
        public string usr_title { get; set; }
    }
}

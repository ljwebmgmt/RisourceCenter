using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace newrisourcecenter.Models
{
    public class SupportToolsModels
    {
        public string n1Id { get; set; }
        public string n1_descLong { get; set; }
        public List<nav2> list_n2_data { get; set; }
    }

    [Table("email_tracker")]
    public class emailtrackerViewModel
    {
        [Key]
        public int ID { get; set; }
        [Display(Name = "User")]
        public Nullable<int> usr_ID { get; set; }
        [Display(Name = "Campaign Type")]
        public string email_type { get; set; }
        [Display(Name = "Actions")]
        public string msg_action { get; set; }
        [Display(Name = "Links")]
        public string url_tracked { get; set; }
        [Display(Name = "Date Sent")]
        public Nullable<DateTime> date_sent { get; set; }
        [Display(Name = "Date Opened")]
        public Nullable<DateTime> date_opened { get; set; }
        [NotMapped]
        public string full_name { get; set; }
        [NotMapped]
        public int? count { get; set; }
    }

    public class GroupCount
    {
        public int? usr_ID { get; set; }
        public int? count { get; set; }
        public string  url { get; set; }
        public DateTime? sent { get; set; }
        public DateTime? opened { get; set; }
    }
}
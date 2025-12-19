using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    [Table("Announcements")]
    public class AnnouncementsViewModel
    {
        [Key]
        public int ID { get; set; }
        [AllowHtml]
        [Display(Name ="Message")]
        public string message { get; set; }
        [Display(Name ="Pages")]
        public string pages { get; set; }
        public string adminID { get; set; }
        [Display(Name ="Status")]
        public string status { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> list_status { get; set; }
        [Display(Name ="Start Date")]
        public Nullable<System.DateTime> startDate { get; set; }
        [Display(Name ="End Date")]
        public Nullable<System.DateTime> endDate { get; set; } 
        [NotMapped]
        public string hide { get; set; }
    }

    [Table("Announcement_logs")]
    public class Announcement_logViewModel
    {
        public int ID { get; set; }
        public Nullable<int> announcementID { get; set; }
        public string userID { get; set; }
        public Nullable<System.DateTime> Time_Seen { get; set; }
    }



}
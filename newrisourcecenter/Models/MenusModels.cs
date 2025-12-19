using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class MenuItems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
        public string PageName { get; set; }
        public string LinkID { get; set; }
        public int? ParentId { get; set; }
        public long ChildId { get; set; }
        public string Usrgroup { get; set; }
        public string redirect { get; set; }
        public string n2_redirectJS { get; set; }      
        public int? Order { get; set; }
    }

    public class SectionsCounts
    {
        public int nav_settings { get; set; }
        public int user_settings { get; set; }
        public int partner_settings { get; set; }
        public int support_settings { get; set; }
        public int rittal_uni { get; set; }
        public int communications { get; set; }
        public int risource { get; set; }
    }
}
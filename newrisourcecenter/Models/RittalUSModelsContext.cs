using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class RittalUSModelsContext : IdentityDbContext<ApplicationUser>
    {
        public RittalUSModelsContext()
            : base("name=RittalUSEntities")
        {

        }
        public virtual DbSet<tbl_MRK_Zipcode> tbl_MRK_Zipcodes { get; set; }
    }


    public class mrk_sales
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string group { get; set; }
    }

}
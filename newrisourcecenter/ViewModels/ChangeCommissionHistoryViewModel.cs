using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using newrisourcecenter.Models;

namespace newrisourcecenter.ViewModels
{
    public class ChangeCommissionHistoryViewModel:ChangeCommissionHistory
    {
        public usr_user usr_User { get; set; }

        public string statusName { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using newrisourcecenter.Models;

namespace newrisourcecenter.ViewModels
{
    public class ChangeCommissionViewModel:ChangeCommission
    {
        public ChangeCommissionRoles roles { get; set; }

        public string username { get; set; }

        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public decimal amount { get; set; }
    }
}
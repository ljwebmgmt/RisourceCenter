using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.ViewModels
{
    public class ChangeCommissionRoles
    {
        public string RoleType { get; set; }

        public bool IsCreator { get; set; }

        public bool IsEditable { get; set; }

        public bool IsApprovable { get; set; }

        public bool IsLocked { get; set; }

        public bool IsViewable { get; set; }

        public bool IsAdmin { get; set; }
    }
}
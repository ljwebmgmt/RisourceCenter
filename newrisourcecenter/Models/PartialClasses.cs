using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    [MetadataType(typeof(ChangeCommissionMetadata))]
    public partial class ChangeCommission
    {
        
    }

    [MetadataType(typeof(CommissionAdminMetadata))]
    public partial class CommissionAdmin
    {

    }

    [MetadataType(typeof(BidRegistrationMetadata))]
    public partial class BidRegistration
    {
        public string username { get; set; }
        public string companyName { get; set; }
        public string approver_type { get; set; }
        public string status { get; set; }
    }

    //public partial class ChangeCommissionHistory
    //{
    //    [NotMapped]
    //    public usr_user usr { get; set; }
    //}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class SearchViewModel
    {
        public IEnumerable<UserViewModel> user_results { get; set; }
        public IEnumerable<SalesCommunicationsViewModel> salesComm_results { get; set; }
        public IEnumerable<RiSourcesModel> risources_results { get; set; }
        public IEnumerable<partnerCompanyViewModel> pComp_results { get; set; }
        public IEnumerable<partnerLocationViewModel> pLoc_results { get; set; }
        public countResults count_results { get; set; }
        public string search_term { get; set; }
        public IEnumerable<Nav3ViewModel> list_classes { get; set; }
        public IEnumerable<RFQViewModel> list_rfq { get; set; }
        public IEnumerable<ReturnTools> list_returns { get; set; }
        public IEnumerable<Catalog_search> search_cat { get; set; }
        public IEnumerable<partNumber_search> search_part { get; set; }
    }

    public class countResults
    {
        public string user_count { get; set; }
        public string salesComm_count { get; set; }
        public string risources_count { get; set; }
        public string pComp_count { get; set; }
        public string pLoc_count { get; set; }
        public string classes_count { get; set; }
        public string rfq_count { get; set; }
        public string returns_count { get; set; }
        public string cat_count { get; set; }
        public string part_count { get; set; }
    }

    public class Catalog_search
    {
        public string partnumber { get; set; }
    }

    public class partNumber_search
    {
        public string partnumber { get; set; }
    }
}
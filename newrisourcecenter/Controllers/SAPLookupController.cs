using newrisourcecenter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    public class SAPLookupController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        // GET: SAPLookup
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            } 
            ViewBag.sapNumbers = db.partnerLocationViewModels.Where(x => x.loc_SAP_account.HasValue && x.loc_SAP_account.Value > 0).Select(x => x.loc_SAP_account.Value).ToList();
            return View();
        }

        public async Task<String> lookup(string sap)
        {
            try
            {
                long sapNumber = Convert.ToInt64(sap);
                var locdata = db.partnerLocationViewModels.Join(db.partnerCompanyViewModels,comp => comp.comp_ID,loc => loc.comp_ID,(loc, comp) => new { loc, comp }).Where(a => a.comp.comp_active != 0 && a.loc.loc_SAP_account.HasValue && a.loc.loc_SAP_account == sapNumber);
                List<WSdata> loc_list = new List<WSdata>();
                List<string> companyNames = new List<string>();
                foreach (var item in locdata)
                {
                    if (!companyNames.Contains(item.comp.comp_name))
                        companyNames.Add(item.comp.comp_name);
                    loc_list.Add(new WSdata { loc_name = item.loc.loc_name, comp_name = item.comp.comp_name, loc_city = item.loc.loc_city });
                }
                return JsonConvert.SerializeObject(new { locations = loc_list, company_names = String.Join(", ",companyNames), status = "OK" });
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { error = "An error occurred while processing your request, please try again", status = "OK" });
            }
        }
    }
}
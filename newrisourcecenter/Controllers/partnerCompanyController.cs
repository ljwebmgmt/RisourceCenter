using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Controllers;

namespace newrisourcecenter.Models
{
    [Authorize]
    public class partnerCompanyController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: partnerCompany
        public ActionResult Index()
        {

            long companyId = Convert.ToInt64(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCompaniesPaged(int start, int limit, string search = "")
        {
            long companyId = Convert.ToInt64(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0) return Json(new { error = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            var query = db.partnerCompanyViewModels.AsQueryable();
            if (!(User.IsInRole("Super Admin") || (User.IsInRole("Local Admin") && User.IsInRole("Rittal User")) || User.IsInRole("Global Admin")))
            {
                query = query.Where(a => a.comp_ID == companyId);
            }
            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int compIdSearch))
                    query = query.Where(a => a.comp_ID == compIdSearch || a.comp_name.Contains(search));
                else
                    query = query.Where(a => a.comp_name.Contains(search));
            }
            int totalCount = await query.CountAsync();
            var rows = await query.OrderBy(a => a.comp_name).Skip(start).Take(limit).ToListAsync();
            return Json(new { rows, total = totalCount }, JsonRequestBehavior.AllowGet);
        }

        // GET: Labels
        [HttpGet]
        public ActionResult searchCompany()
        {
            return View();
        }

        [HttpPost]
        public ActionResult searchCompany(string form_value = null)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }
                var compdata = db.partnerCompanyViewModels;
                double Num;
                bool isNum = double.TryParse(form_value, out Num);

                if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") && User.IsInRole("Rittal User") || User.IsInRole("Global Admin"))
                {
                    if (isNum)
                    {
                        int compid = Convert.ToInt32(form_value);
                        return View(compdata.Where(a => a.comp_ID == compid));
                    }
                    else
                    {
                        return View(compdata.Where(a => a.comp_name.Contains(form_value)));
                    }
                }

                return Json("You do not have any more companies");


                //compdata.Take(10));
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
        }

        // GET: partnerCompany/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var usrs = dbEntity.usr_user;

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            //Create the list for the dropdown on the return type
            List<SelectListItem> list_regions = new List<SelectListItem>();
            list_regions.Add(new SelectListItem { Text = "Select a Return Type", Value = "select", Selected = true });//default value for select dropdown- WEST, SOUTH, NORTH, CENTRAL, NORTHEAST, SOUTHEAST
            list_regions.Add(new SelectListItem { Text = "CENTRAL", Value = "CENTRAL" });
            list_regions.Add(new SelectListItem { Text = "NORTH", Value = "NORTH" });
            list_regions.Add(new SelectListItem { Text = "NORTHEAST", Value = "NORTHEAST" });
            list_regions.Add(new SelectListItem { Text = "SOUTH", Value = "SOUTH" });
            list_regions.Add(new SelectListItem { Text = "SOUTHEAST", Value = "SOUTHEAST" });
            list_regions.Add(new SelectListItem { Text = "WEST", Value = "WEST" });
            list_regions.Add(new SelectListItem { Text = "GLOBAL KEY ACCOUNTS", Value = "GLOBAL KEY ACCOUNTS" });

            var partnerComp = new partnerCompanyViewModel();
            {
                partnerComp.list_Type = list_types;
                partnerComp.list_industry = list_industry;
                partnerComp.list_products = list_products;
                partnerComp.list_regions = list_regions;
            };

            return View(partnerComp);
        }

        // POST: partnerCompany/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "comp_ID,comp_name,comp_industry,comp_type,comp_level,comp_products,comp_SAP,comp_POS,comp_SPA,comp_project_reg,comp_MDF,comp_MDF_amount,comp_MDF_tLimit,comp_MDF_aLimit,comp_MDF_mLimit,comp_FX,comp_active,comp_dateCreated,comp_dateUpdated,comp_createdBy,comp_updatedBy,old_ID,comp_RiCRM,comp_region,bid_registration,it_territory_manager,general_manager,comp_MKT_Limit")] partnerCompanyViewModel partnerCompanyViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                partnerCompanyViewModel.comp_products = Request.Form["comp_products"];
                partnerCompanyViewModel.comp_type = Convert.ToByte(Request.Form["comp_type"]);
                partnerCompanyViewModel.comp_industry = Convert.ToByte(Request.Form["comp_industry"]);
                partnerCompanyViewModel.comp_active = Convert.ToByte(Request.Form["comp_active"]);
                partnerCompanyViewModel.comp_dateCreated = DateTime.Now;
                partnerCompanyViewModel.comp_createdBy = userId;

                partnerCompanyViewModel.comp_MDF_aLimit = partnerCompanyViewModel.comp_MDF_amount;
                partnerCompanyViewModel.comp_MDF_oLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_tLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_eLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_mLimit = partnerCompanyViewModel.comp_MDF_amount * 0.10;
                partnerCompanyViewModel.comp_MDF_dLimit = partnerCompanyViewModel.comp_MDF_amount * 0.10;

                db.partnerCompanyViewModels.Add(partnerCompanyViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerCompanyViewModel.comp_ID), "PartnerCompany", DateTime.Now, " Partner Company was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"] });
            }

            return View(partnerCompanyViewModel);
        }

        // GET: partnerCompany/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var comm = new CommonController();
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(id);
            double? mdfRemaining = 0;
            double? mktRemaining = 0;

            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var users = dbEntity.usr_user;
            int count_jigsaw = dbEntity.usr_user.Where(a => a.comp_ID == id && a.usr_SPA == 1 || a.comp_ID == id && a.usr_POS == 1).Count();
            int count_locations = dbEntity.partnerLocations.Where(a => a.comp_ID == id).Count();
            //Count users
            int count_users = users.Where(a => a.comp_ID == id).Count();

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            if (partnerCompanyViewModel == null)
            {
                return HttpNotFound();
            }

            if (partnerCompanyViewModel.comp_MDF_amount > 0)
            {
                //Get The MDF information
                List<mdf_main> mdf_main = await dbEntity.mdf_main.Where(a=>a.mdf_comp==partnerCompanyViewModel.comp_ID && a.archive_year==null && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();
                List<MdfParts> prom = comm.MDFsActivities(mdf_main, partnerCompanyViewModel);
                mdfRemaining = prom.FirstOrDefault().totalMDFAva;
            }

            if (partnerCompanyViewModel.comp_MKT_Limit > 0)
            {
                //Get The MDF information
                List<mdf_main> mdf_main = await dbEntity.mdf_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == null && (a.cost_center.Contains("MKT") || a.cost_center == "Split")).ToListAsync();
                List<MdfParts> prom = comm.MKTsActivities(mdf_main, partnerCompanyViewModel);
                mktRemaining = prom.FirstOrDefault().totalMDFAva;
            }

            //Create the list for the dropdown on the return type
            List<SelectListItem> list_regions = new List<SelectListItem>();
            list_regions.Add(new SelectListItem { Text = "Select a Return Type", Value = "select", Selected = true });//default value for select dropdown- WEST, SOUTH, NORTH, CENTRAL, NORTHEAST, SOUTHEAST
            list_regions.Add(new SelectListItem { Text = "CENTRAL", Value = "CENTRAL" });
            list_regions.Add(new SelectListItem { Text = "NORTH", Value = "NORTH" });
            list_regions.Add(new SelectListItem { Text = "NORTHEAST", Value = "NORTHEAST" });
            list_regions.Add(new SelectListItem { Text = "SOUTH", Value = "SOUTH" });
            list_regions.Add(new SelectListItem { Text = "SOUTHEAST", Value = "SOUTHEAST" });
            list_regions.Add(new SelectListItem { Text = "WEST", Value = "WEST" });
            list_regions.Add(new SelectListItem { Text = "GLOBAL KEY ACCOUNTS", Value = "GLOBAL KEY ACCOUNTS" });

            partnerCompanyViewModel.list_products = list_products;
            partnerCompanyViewModel.list_industry = list_industry;
            partnerCompanyViewModel.list_Type = list_types;
            partnerCompanyViewModel.list_regions = list_regions;
            partnerCompanyViewModel.count_locations = count_locations;
            partnerCompanyViewModel.count_users = count_users;
            partnerCompanyViewModel.count_jigsaw = count_jigsaw;
            if (!string.IsNullOrEmpty(partnerCompanyViewModel.comp_updatedBy.ToString()))
            {
                //Get who edited last
                var updatedBy_data = dbEntity.usr_user.Where(a => a.usr_ID == partnerCompanyViewModel.comp_updatedBy);
                if (updatedBy_data.Count()>0)
                {
                    partnerCompanyViewModel.full_name = updatedBy_data.FirstOrDefault().usr_fName + " " + updatedBy_data.FirstOrDefault().usr_lName;
                }
            }
            partnerCompanyViewModel.MDF_remaining = mdfRemaining.ToString();
            partnerCompanyViewModel.MKT_remaining = mktRemaining.ToString();

            return View(partnerCompanyViewModel);
        }

        // POST: partnerCompany/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "comp_ID,comp_name,comp_industry,comp_type,comp_level,comp_products,comp_SAP,comp_POS,comp_SPA,comp_project_reg,comp_MDF,comp_MDF_amount,comp_MDF_tLimit,comp_MDF_aLimit,comp_MDF_mLimit,comp_FX,comp_active,comp_dateCreated,comp_dateUpdated,comp_createdBy,comp_updatedBy,old_ID,comp_RiCRM,comp_region,bid_registration,it_territory_manager,general_manager,comp_MKT_Limit")] partnerCompanyViewModel partnerCompanyViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                partnerCompanyViewModel.comp_products = Request.Form["comp_products"];
                partnerCompanyViewModel.comp_type = Convert.ToByte(Request.Form["comp_type"]);
                partnerCompanyViewModel.comp_industry = Convert.ToByte(Request.Form["comp_industry"]);
                partnerCompanyViewModel.comp_active = Convert.ToByte(Request.Form["comp_active"]);
                partnerCompanyViewModel.comp_updatedBy = userId;
                partnerCompanyViewModel.comp_dateUpdated = DateTime.Now;

                partnerCompanyViewModel.comp_MDF_aLimit = partnerCompanyViewModel.comp_MDF_amount;
                partnerCompanyViewModel.comp_MDF_oLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_tLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_eLimit = partnerCompanyViewModel.comp_MDF_amount * 0.15;
                partnerCompanyViewModel.comp_MDF_mLimit = partnerCompanyViewModel.comp_MDF_amount * 0.10;
                partnerCompanyViewModel.comp_MDF_dLimit = partnerCompanyViewModel.comp_MDF_amount * 0.10;

                db.Entry(partnerCompanyViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerCompanyViewModel.comp_ID), "PartnerCompany", DateTime.Now, " Partner Company was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                if (!string.IsNullOrEmpty(Request.Form["isSPA"]) && Request.Form["isSPA"]=="yes")
                {
                    return RedirectToAction("SPAadmin","SPA", new { n1_name = "Support Tools", n2_name = "SPA Controls Admin", msg ="SPA Admin",comp_id = partnerCompanyViewModel.comp_ID });
                }

                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(partnerCompanyViewModel);
        }

        // GET: partnerCompany/Delete/5
        public async Task<JsonResult> Delete(long? id)
        {
            try
            {
                if (id == null)
                {
                    throw new Exception("An error occurred while processing your request.");
                }
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    throw new Exception("Please Login. Login has timed out");
                }
                partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(id);
                if (partnerCompanyViewModel == null)
                {
                    throw new Exception("Company not found");
                }
                db.partnerCompanyViewModels.Remove(partnerCompanyViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerCompanyViewModel.comp_ID), "PartnerCompany", DateTime.Now, " Partner Company was deleted by user " + userId, "Delete", Convert.ToInt32(userId));
                return Json("OK");
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(e.Message);
            }
        }

        // POST: partnerCompany/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(id);
            db.partnerCompanyViewModels.Remove(partnerCompanyViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

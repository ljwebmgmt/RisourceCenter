using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using newrisourcecenter.Controllers;

namespace newrisourcecenter.Models
{
    [Authorize]
    public class partnerLocationController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: partnerLocation
        public async Task<ActionResult> Index(int comp_id=0)
        {
            long companyId = Convert.ToInt64(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") && User.IsInRole("Rittal User") || User.IsInRole("Global Admin"))
            {
                //select the company list from the partnerCompanies database
                var companies = from list in db.partnerCompanyViewModels where list.comp_active != 0 orderby list.comp_name ascending select list;                
                //Add the data to the comp_listing object
                List<CompData> comp_listing = new List<CompData>();
                foreach (var item in companies)//iterate the add function
                {
                    comp_listing.Add(new CompData { comp_name=item.comp_name,comp_ID=item.comp_ID });
                }
                ViewBag.partnerComp = comp_listing;
                if (comp_id==0)
                {
                    return View(await db.partnerLocationViewModels.Where(a => a.comp_ID == companyId).OrderByDescending(a => a.loc_ID).ToListAsync());
                }
                else
                {
                    return View(await db.partnerLocationViewModels.Where(a => a.comp_ID == comp_id).OrderByDescending(a => a.loc_ID).ToListAsync());
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                //select the company list from the partnerCompanies database
                var companies = from list in db.partnerCompanyViewModels where list.comp_active != 0 where list.comp_ID == companyId orderby list.comp_name ascending select list;
               
                //Add the data to the comp_listing object
                List<CompData> comp_listing = new List<CompData>();
                foreach (var item in companies)//iterate the add function
                {
                    comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
                }
                ViewBag.partnerComp = comp_listing;

                return View(await db.partnerLocationViewModels.Where(a => a.comp_ID == companyId).ToListAsync());
            }
            else
            {
                return View();
            }

        }

        // GET: partnerLocation/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerLocationViewModel partnerLocationViewModel = await db.partnerLocationViewModels.FindAsync(id);
            if (partnerLocationViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerLocationViewModel);
        }

        // GET: partnerLocation/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //select the states data from the data_stat table
            var states = from state in dbEntity.data_state where state.state_country == "US" || state.state_country == "CA" orderby state.state_abbr ascending select state;
            //Add the data to the list_state object
            List<CompData> list_states = new List<CompData>();
            foreach (var item in states)//iterate the add function
            {
                list_states.Add(new CompData { comp_name = item.state_long, comp_ID = item.stateid });
            }

            ViewBag.StateData = list_states;
            return View();
        }

        // POST: partnerLocation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "loc_ID,comp_ID,loc_name,loc_add1,loc_add2,loc_city,loc_state,loc_zip,loc_phone,loc_fax,loc_web,loc_email,loc_logo,loc_lat,loc_lon,loc_dealor_status,loc_show_address,loc_SAP_account,loc_SAP_password,loc_dateCreated,loc_dateUpdated,loc_createdBy,loc_updatedBy,old_locID,attachment,price_group,loc_Webshop_account,loc_Webshop_password")] partnerLocationViewModel partnerLocationViewModel, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                //attach a file to the risources
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    var guid = Guid.NewGuid().ToString();
                    var file = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/company/partners/logos"), file);
                    partnerLocationViewModel.loc_logo = file;
                    attachment.SaveAs(path);
                }

                db.partnerLocationViewModels.Add(partnerLocationViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerLocationViewModel.loc_ID), "PartnerLocation", DateTime.Now, " Partner location was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { comp_id = partnerLocationViewModel.comp_ID, n1_name=Request.Form["n1_name"], n2_name = Request.Form["n2_name"] });
            }

            return View(partnerLocationViewModel);
        }

        // GET: partnerLocation/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            long companyId = Convert.ToInt64(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<CompData> comp_listing = new List<CompData>();
            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") && User.IsInRole("Rittal User") || User.IsInRole("Global Admin"))
            {
                //select the company list from the partnerCompanies database
                var companies = from list in db.partnerCompanyViewModels where list.comp_active != 0 orderby list.comp_name ascending select list;
                //Add the data to the comp_listing object
                
                foreach (var item in companies)//iterate the add function
                {
                    comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                //select the company list from the partnerCompanies database
                var companies = from list in db.partnerCompanyViewModels where list.comp_active != 0 where list.comp_ID == companyId orderby list.comp_name ascending select list;
                //Add the data to the comp_listing object
                foreach (var item in companies)//iterate the add function
                {
                    comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
                }
            }
            else
            {
                ViewBag.partnerComp = comp_listing;
                return View();
            }
            ViewBag.partnerComp = comp_listing;
            //select the states data from the data_stat table
            var states = from state in dbEntity.data_state where state.state_country == "US" || state.state_country == "CA" orderby state.state_abbr ascending select state;
            //Add the data to the list_state object
            List<CompData> list_states = new List<CompData>();
            foreach (var item in states)//iterate the add function
            {
                list_states.Add(new CompData { comp_name = item.state_long, comp_ID = item.stateid });
            }
            ViewBag.StateData = list_states;

            partnerLocationViewModel partnerLocationViewModel = await db.partnerLocationViewModels.FindAsync(id);
            if (partnerLocationViewModel == null)
            {
                return HttpNotFound();
            }

            return View(partnerLocationViewModel);
        }

        // POST: partnerLocation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "loc_ID,comp_ID,loc_name,loc_add1,loc_add2,loc_city,loc_state,loc_zip,loc_phone,loc_fax,loc_web,loc_email,loc_logo,loc_lat,loc_lon,loc_dealor_status,loc_show_address,loc_SAP_account,loc_SAP_password,loc_dateCreated,loc_dateUpdated,loc_createdBy,loc_updatedBy,old_locID,attachment,price_group,loc_Webshop_account,loc_Webshop_password")] partnerLocationViewModel partnerLocationViewModel, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                //attach a file to the risources
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    var guid = Guid.NewGuid().ToString();
                    var file = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/company/partners/logos"), file);
                    partnerLocationViewModel.loc_logo = file;
                    attachment.SaveAs(path);
                }

                db.Entry(partnerLocationViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerLocationViewModel.loc_ID), "PartnerLocation", DateTime.Now, " Partner location was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { comp_id=partnerLocationViewModel.comp_ID, n1_name=Request.Form["n1_name"], n2_name = Request.Form["n2_name"] });
            }

            return View(partnerLocationViewModel);
        }

        public string sendwebshopRequest(string useremail=null,string company=null,string location=null)
        {
            var From_req = "webmaster@rittal.us";
            var To_req = "henderson.r@rittal.us";
            var Subject_req = "Online Shop login request";
            var Body_req = "<h1>Online Shop Login Request</h1><table><tr><th style=\"text-align:left;\"  >User Email:</th><td>" + useremail+ "</td></tr><tr><th style=\"text-align:left;\" >Company Name:</th><td>" + company+ "</td></tr><tr><th style=\"text-align:left;\" >Location Name:</th><td>" + location+"</td></tr></table>";

            locController.email(From_req, To_req, Subject_req, Body_req); // call the email function

            return "OK";
        }

        // GET: partnerLocation/Delete/5
        public async Task<ActionResult> Delete(long? id)
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
            partnerLocationViewModel partnerLocationViewModel = await db.partnerLocationViewModels.FindAsync(id);
            if (partnerLocationViewModel == null)
            {
                return HttpNotFound();
            }

            db.partnerLocationViewModels.Remove(partnerLocationViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(partnerLocationViewModel.loc_ID), "PartnerLocation", DateTime.Now, " Partner location was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { comp_id = Request.QueryString["comp_id"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"] });

            //return View(partnerLocationViewModel);
        }

        // POST: partnerLocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            partnerLocationViewModel partnerLocationViewModel = await db.partnerLocationViewModels.FindAsync(id);
            db.partnerLocationViewModels.Remove(partnerLocationViewModel);
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

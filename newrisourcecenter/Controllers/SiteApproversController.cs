using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public class SiteApproversController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: SiteApprovers
        public async Task<ActionResult> Index()
        {
            return View(await db.SiteApprovers.ToListAsync());
        }

        // GET: SiteApprovers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiteApprovers siteApprovers = await db.SiteApprovers.FindAsync(id);
            if (siteApprovers == null)
            {
                return HttpNotFound();
            }
            return View(siteApprovers);
        }

        // GET: SiteApprovers/Create
        public ActionResult Create()
        {
            SiteApprovers siteApprovers = new SiteApprovers();
            var partnerType = dbEntity.partnerTypes;
            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }
            siteApprovers.list_Type = list_types;


            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries.OrderBy(a => a.country_id))
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { name = country.country_long, id = country.country_id });
                }
            }
            siteApprovers.country = UserCountries;

            return View(siteApprovers);
        }

        // POST: SiteApprovers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,CountryId,CompType,FullName,Email")] SiteApprovers siteApprovers)
        {

            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                db.SiteApprovers.Add(siteApprovers);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(siteApprovers.Id), "Nav1ViewModel", DateTime.Now, " SiteApprovers was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"] });
            }
            return View(siteApprovers);
        }

        // GET: SiteApprovers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiteApprovers siteApprovers = await db.SiteApprovers.FindAsync(id);
            if (siteApprovers == null)
            {
                return HttpNotFound();
            }
            var partnerType = dbEntity.partnerTypes;
            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }
            siteApprovers.list_Type = list_types;

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries.OrderBy(a => a.country_id))
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { name = country.country_long, id = country.country_id });
                }
            }
            siteApprovers.country = UserCountries;

            return View(siteApprovers);
        }

        // POST: SiteApprovers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CountryId,CompType,FullName,Email")] SiteApprovers siteApprovers)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.Entry(siteApprovers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(siteApprovers.Id), "SiteApprovers", DateTime.Now, " a siteApprovers was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"] });
            }


            return View(siteApprovers);
        }

        // GET: SiteApprovers/Delete/5
        public async Task<ActionResult> Delete(int? id)
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
            SiteApprovers siteApprovers = await db.SiteApprovers.FindAsync(id);
            if (siteApprovers == null)
            {
                return HttpNotFound();
            }
            db.SiteApprovers.Remove(siteApprovers);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(siteApprovers.Id), "SiteApprovers", DateTime.Now, " a Site Approver was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });

           //return View(siteApprovers);
        }

        // POST: SiteApprovers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SiteApprovers siteApprovers = await db.SiteApprovers.FindAsync(id);
            db.SiteApprovers.Remove(siteApprovers);
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

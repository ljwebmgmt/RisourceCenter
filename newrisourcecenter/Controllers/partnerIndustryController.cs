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
    [Authorize(Roles = "Super Admin")]
    public class partnerIndustryController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: partnerIndustry
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(await db.partnerIndustryViewModels.ToListAsync());
        }

        // GET: partnerIndustry/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerIndustryViewModel partnerIndustryViewModel = await db.partnerIndustryViewModels.FindAsync(id);
            if (partnerIndustryViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerIndustryViewModel);
        }

        // GET: partnerIndustry/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: partnerIndustry/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "pi_ID,pi_industry")] partnerIndustryViewModel partnerIndustryViewModel)
        {
            if (ModelState.IsValid)
            {
                db.partnerIndustryViewModels.Add(partnerIndustryViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(partnerIndustryViewModel);
        }

        // GET: partnerIndustry/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerIndustryViewModel partnerIndustryViewModel = await db.partnerIndustryViewModels.FindAsync(id);
            if (partnerIndustryViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerIndustryViewModel);
        }

        // POST: partnerIndustry/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "pi_ID,pi_industry")] partnerIndustryViewModel partnerIndustryViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(partnerIndustryViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(partnerIndustryViewModel);
        }

        // GET: partnerIndustry/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerIndustryViewModel partnerIndustryViewModel = await db.partnerIndustryViewModels.FindAsync(id);
            if (partnerIndustryViewModel == null)
            {
                return HttpNotFound();
            }

            db.partnerIndustryViewModels.Remove(partnerIndustryViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: partnerIndustry/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            partnerIndustryViewModel partnerIndustryViewModel = await db.partnerIndustryViewModels.FindAsync(id);
            db.partnerIndustryViewModels.Remove(partnerIndustryViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
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

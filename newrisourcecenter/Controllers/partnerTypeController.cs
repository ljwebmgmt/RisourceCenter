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
    [Authorize(Roles = "Super Admin,Rittal User")]
    public class partnerTypeController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: partnerType
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await db.partnerTypeViewModels.ToListAsync());
        }

        // GET: partnerType/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerTypeViewModel partnerTypeViewModel = await db.partnerTypeViewModels.FindAsync(id);
            if (partnerTypeViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerTypeViewModel);
        }

        // GET: partnerType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: partnerType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "pt_ID,pt_type")] partnerTypeViewModel partnerTypeViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.partnerTypeViewModels.Add(partnerTypeViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(partnerTypeViewModel);
        }

        // GET: partnerType/Edit/5
        public async Task<ActionResult> Edit(int? id)
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
            partnerTypeViewModel partnerTypeViewModel = await db.partnerTypeViewModels.FindAsync(id);
            if (partnerTypeViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerTypeViewModel);
        }

        // POST: partnerType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "pt_ID,pt_type")] partnerTypeViewModel partnerTypeViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.Entry(partnerTypeViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(partnerTypeViewModel);
        }

        // GET: partnerType/Delete/5
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
            partnerTypeViewModel partnerTypeViewModel = await db.partnerTypeViewModels.FindAsync(id);
            if (partnerTypeViewModel == null)
            {
                return HttpNotFound();
            }
            db.partnerTypeViewModels.Remove(partnerTypeViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: partnerType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            partnerTypeViewModel partnerTypeViewModel = await db.partnerTypeViewModels.FindAsync(id);
            db.partnerTypeViewModels.Remove(partnerTypeViewModel);
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

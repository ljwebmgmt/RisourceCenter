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
    public class partnerProductsController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: partnerProducts
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await db.partnerProductsViewModels.ToListAsync());
        }

        // GET: partnerProducts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerProductsViewModel partnerProductsViewModel = await db.partnerProductsViewModels.FindAsync(id);
            if (partnerProductsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerProductsViewModel);
        }

        // GET: partnerProducts/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: partnerProducts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "pp_ID,pp_product")] partnerProductsViewModel partnerProductsViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.partnerProductsViewModels.Add(partnerProductsViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(partnerProductsViewModel);
        }

        // GET: partnerProducts/Edit/5
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
            partnerProductsViewModel partnerProductsViewModel = await db.partnerProductsViewModels.FindAsync(id);
            if (partnerProductsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerProductsViewModel);
        }

        // POST: partnerProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "pp_ID,pp_product")] partnerProductsViewModel partnerProductsViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.Entry(partnerProductsViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(partnerProductsViewModel);
        }

        // GET: partnerProducts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerProductsViewModel partnerProductsViewModel = await db.partnerProductsViewModels.FindAsync(id);
            if (partnerProductsViewModel == null)
            {
                return HttpNotFound();
            }

            db.partnerProductsViewModels.Remove(partnerProductsViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: partnerProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            partnerProductsViewModel partnerProductsViewModel = await db.partnerProductsViewModels.FindAsync(id);
            db.partnerProductsViewModels.Remove(partnerProductsViewModel);
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

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
    public class risourcesCatController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: risourcesCat
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await db.risourcesCatViewModels.ToListAsync());
        }

        // GET: risourcesCat/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            risourcesCatViewModel risourcesCatViewModel = await db.risourcesCatViewModels.FindAsync(id);
            if (risourcesCatViewModel == null)
            {
                return HttpNotFound();
            }
            return View(risourcesCatViewModel);
        }

        // GET: risourcesCat/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: risourcesCat/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "cat_id,ris_categories")] risourcesCatViewModel risourcesCatViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.risourcesCatViewModels.Add(risourcesCatViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(risourcesCatViewModel);
        }

        // GET: risourcesCat/Edit/5
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
            risourcesCatViewModel risourcesCatViewModel = await db.risourcesCatViewModels.FindAsync(id);
            if (risourcesCatViewModel == null)
            {
                return HttpNotFound();
            }
            return View(risourcesCatViewModel);
        }

        // POST: risourcesCat/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "cat_id,ris_categories")] risourcesCatViewModel risourcesCatViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.Entry(risourcesCatViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index",new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(risourcesCatViewModel);
        }

        // GET: risourcesCat/Delete/5
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
            risourcesCatViewModel risourcesCatViewModel = await db.risourcesCatViewModels.FindAsync(id);
            if (risourcesCatViewModel == null)
            {
                return HttpNotFound();
            }

            db.risourcesCatViewModels.Remove(risourcesCatViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index",new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: risourcesCat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            risourcesCatViewModel risourcesCatViewModel = await db.risourcesCatViewModels.FindAsync(id);
            db.risourcesCatViewModels.Remove(risourcesCatViewModel);
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

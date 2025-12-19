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
using System.IO;

namespace newrisourcecenter.Controllers
{
    [Authorize(Roles = "Super Admin,Rittal User")]
    public class risourcesTypeController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: risourcesType
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await db.risourcesTypeViewModels.OrderByDescending(a=>a.ID).ToListAsync());
        }

        // GET: risourcesType/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            risourcesTypeViewModel risourcesTypeViewModel = await db.risourcesTypeViewModels.FindAsync(id);
            if (risourcesTypeViewModel == null)
            {
                return HttpNotFound();
            }
            return View(risourcesTypeViewModel);
        }

        // GET: risourcesType/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: risourcesType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,type_link,type_name,type_order")] risourcesTypeViewModel risourcesTypeViewModel, HttpPostedFileBase attachment)
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
                    var path = Path.Combine(Server.MapPath("~/attachments/risources/displayImage"), file);
                    risourcesTypeViewModel.type_link = file;
                    attachment.SaveAs(path);
                }

                db.risourcesTypeViewModels.Add(risourcesTypeViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"] });
            }

            return View(risourcesTypeViewModel);
        }

        // GET: risourcesType/Edit/5
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
            risourcesTypeViewModel risourcesTypeViewModel = await db.risourcesTypeViewModels.FindAsync(id);
            if (risourcesTypeViewModel == null)
            {
                return HttpNotFound();
            }
            return View(risourcesTypeViewModel);
        }

        // POST: risourcesType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,type_link,type_name,type_order")] risourcesTypeViewModel risourcesTypeViewModel, HttpPostedFileBase attachment)
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
                    var path = Path.Combine(Server.MapPath("~/attachments/risources/displayImage"), file);
                    risourcesTypeViewModel.type_link = file;
                    attachment.SaveAs(path);
                }

                db.Entry(risourcesTypeViewModel).State = EntityState.Modified;      
                await db.SaveChangesAsync();
                return RedirectToAction("Index",new { n1_name = Request.Form["n1_name"] });
            }
            return View(risourcesTypeViewModel);
        }

        // GET: risourcesType/Delete/5
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
            risourcesTypeViewModel risourcesTypeViewModel = await db.risourcesTypeViewModels.FindAsync(id);
            if (risourcesTypeViewModel == null)
            {
                return HttpNotFound();
            }
            db.risourcesTypeViewModels.Remove(risourcesTypeViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index",new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: risourcesType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            risourcesTypeViewModel risourcesTypeViewModel = await db.risourcesTypeViewModels.FindAsync(id);
            db.risourcesTypeViewModels.Remove(risourcesTypeViewModel);
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

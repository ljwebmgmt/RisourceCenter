using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class UserThemesController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: UserThemes
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await db.UserThemes.ToListAsync());
        }

        // GET: UserThemes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserTheme userTheme = await db.UserThemes.FindAsync(id);
            if (userTheme == null)
            {
                return HttpNotFound();
            }
            return View(userTheme);
        }

        // GET: UserThemes/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: UserThemes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "theme_id,theme_name")] UserTheme userTheme)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.UserThemes.Add(userTheme);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(userTheme);
        }

        // GET: UserThemes/Edit/5
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
            UserTheme userTheme = await db.UserThemes.FindAsync(id);
            if (userTheme == null)
            {
                return HttpNotFound();
            }
            return View(userTheme);
        }

        // POST: UserThemes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "theme_id,theme_name")] UserTheme userTheme)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                db.Entry(userTheme).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            }
            return View(userTheme);
        }

        // GET: UserThemes/Delete/5
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
            UserTheme userTheme = await db.UserThemes.FindAsync(id);
            if (userTheme == null)
            {
                return HttpNotFound();
            }

            db.UserThemes.Remove(userTheme);
            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { n1_name=Request.QueryString["n1_name"] });
        }

        // POST: UserThemes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UserTheme userTheme = await db.UserThemes.FindAsync(id);
            db.UserThemes.Remove(userTheme);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public List<UserTheme> themdata(string email)
        {
            List<UserTheme> ustheme = new List<UserTheme>();

            if (!string.IsNullOrEmpty(email))
            {
                RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
                var them = db.usr_user.Where(a => a.usr_email==email).DefaultIfEmpty().ToList();

                foreach (var intem in them)
                {
                    ustheme.Add(new UserTheme { theme_id = intem.admin_theme });
                }
            }
            else
            {
                ustheme.Add(new UserTheme { theme_id = 1 });
            }

            return ustheme;
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

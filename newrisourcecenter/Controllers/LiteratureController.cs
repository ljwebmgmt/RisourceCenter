using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    public class LiteratureController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: Literature
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(await db.LiteratureViewModels.ToListAsync());
        }

        // GET: Literature/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LiteratureViewModel literatureViewModel = await db.LiteratureViewModels.FindAsync(id);
            if (literatureViewModel == null)
            {
                return HttpNotFound();
            }
            return View(literatureViewModel);
        }

        // GET: Literature/Create
        public async Task<ActionResult> Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var literatureViewModel = await db.LiteratureViewModels.ToListAsync();

            //This is the dropdown for the risources
            var level2 = db.Nav2ViewModel.Where(a => a.n1ID == 4);
            List<Nav1List> list_level2 = new List<Nav1List>();
            list_level2.Add(new Nav1List { id = 0, name = "Select Risource Type" });
            foreach (var items in level2.OrderBy(a=>a.n2_nameLong))
            {
                if ( items.n2ID != 46 )
                {
                    if ( items.n2ID != 49 )
                    {
                        list_level2.Add(new Nav1List { id = items.n2ID, name = items.n2_nameLong });
                    }
                }
            }
            ViewBag.RiSourceMenu = list_level2;

            return View(literatureViewModel);
        }

        // POST: Literature/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "lit_ID,lit_name,created_by,date_created,date_updated,updated_by,risource,attach_risource")] LiteratureViewModel literatureViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                literatureViewModel.risource = Request.Form["attach_risource"];
                db.LiteratureViewModels.Add(literatureViewModel);
                await db.SaveChangesAsync();

                return RedirectToAction("Create", new { n1_name=Request.Form["n1_name"] });
            }

            return View(literatureViewModel);
        }

        // GET: Literature/Edit/5
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
            LiteratureViewModel literatureViewModel = await db.LiteratureViewModels.FindAsync(id);
            if (literatureViewModel == null)
            {
                return HttpNotFound();
            }

            //This is the dropdown for the risources
            var level2 = db.Nav2ViewModel.Where(a => a.n1ID == 4);
            List<Nav1List> list_level2 = new List<Nav1List>();
            list_level2.Add(new Nav1List { id = 0, name = "Select Risource Type" });
            foreach (var items in level2.OrderBy(a => a.n2_nameLong))
            {
                if ( items.n2ID != 46 )
                {
                    if ( items.n2ID != 49 )
                    {
                        list_level2.Add(new Nav1List { id = items.n2ID, name = items.n2_nameLong });
                    }
                }
            }
            ViewBag.RiSourceMenu = list_level2;

            //Add attachment list to the Edit page
            List<Nav1List> list_attachments = new List<Nav1List>();
            var arrayOfAttachments = literatureViewModel.risource;
            if (arrayOfAttachments != null)
            {
                int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);

                foreach (int item in nums)
                {
                    var risour = db.RiSourcesViewModels.Where(a => a.ris_ID == item);
                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                }
            }
            literatureViewModel.list_attachments = list_attachments;

            return View(literatureViewModel);
        }

        // POST: Literature/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "lit_ID,lit_name,created_by,date_created,date_updated,updated_by,risource,attach_risource")] LiteratureViewModel literatureViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                literatureViewModel.risource = Request.Form["attach_risource"];
                db.Entry(literatureViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Create", new { n1_name = Request.QueryString["n1_name"] });
            }

            return View(literatureViewModel);
        }

        // GET: Literature/Delete/5
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
            LiteratureViewModel literatureViewModel = await db.LiteratureViewModels.FindAsync(id);
            if (literatureViewModel == null)
            {
                return HttpNotFound();
            }
            db.LiteratureViewModels.Remove(literatureViewModel);
            await db.SaveChangesAsync();

            return RedirectToAction("Create", new { n1_name = Request.QueryString["n1_name"] });
        }

        // POST: Literature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            LiteratureViewModel literatureViewModel = await db.LiteratureViewModels.FindAsync(id);
            db.LiteratureViewModels.Remove(literatureViewModel);
            await db.SaveChangesAsync();

            return RedirectToAction("Create", new { n1_name = Request.QueryString["n1_name"] });

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

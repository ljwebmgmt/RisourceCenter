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
    public class AnnouncementsController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: Announcements
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(await db.AnnouncementsViewModels.OrderByDescending(a=>a.ID).ToListAsync());
        }

        // GET: Announcements/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnouncementsViewModel announcementsViewModel = await db.AnnouncementsViewModels.FindAsync(id);
            if (announcementsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(announcementsViewModel);
        }

        // GET: Announcements/Create
        public ActionResult Create()
        {
            AnnouncementsViewModel announcementsViewModel = new AnnouncementsViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //Add status dropdown
            List<SelectListItem> list_status = new List<SelectListItem>();
            list_status.Add(new SelectListItem { Text = "Select Status", Value = "", Selected = true });//default value for select dropdown
            list_status.Add(new SelectListItem { Text = "Activate", Value = "1" });//default value for select dropdown
            list_status.Add(new SelectListItem { Text = "DeActivate", Value = "0" });//default value for select dropdown
            announcementsViewModel.list_status = list_status;

            return View(announcementsViewModel);
        }

        // POST: Announcements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,message,pages,adminID,status,startDate,endDate")] AnnouncementsViewModel announcementsViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                announcementsViewModel.adminID = userId.ToString();
                db.AnnouncementsViewModels.Add(announcementsViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { childId = Request.Form["childId"], n1_name = Request.Form["n1_name"] });
            }

            return View(announcementsViewModel);
        }

        // GET: Announcements/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnouncementsViewModel announcementsViewModel = await db.AnnouncementsViewModels.FindAsync(id);
            if (announcementsViewModel == null)
            {
                return HttpNotFound();
            }
            //Add status dropdown
            List<SelectListItem> list_status = new List<SelectListItem>();
            list_status.Add(new SelectListItem { Text = "Select Status", Value = "", Selected = true });//default value for select dropdown
            list_status.Add(new SelectListItem { Text = "Activate", Value = "1" });//default value for select dropdown
            list_status.Add(new SelectListItem { Text = "DeActivate", Value = "0" });//default value for select dropdown
            announcementsViewModel.list_status = list_status;

            return View(announcementsViewModel);
        }

        // POST: Announcements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,message,pages,adminID,status,startDate,endDate")] AnnouncementsViewModel announcementsViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.Entry(announcementsViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"] });
            }
            return View(announcementsViewModel);
        }

        [HttpPost]
        public async Task<JsonResult> getAnnouncement(int announcementID = 0, int preview=0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return Json("Login");
            }
            try
            {
                if (announcementID != 0)
                {
                    Announcement_logViewModel announce_log = new Announcement_logViewModel();
                    announce_log.announcementID = announcementID;
                    announce_log.userID = userId.ToString();
                    announce_log.Time_Seen = DateTime.Now;
                    db.announcement_logViewModels.Add(announce_log);
                    await db.SaveChangesAsync();
                    return Json("hideAnnouncement");
                }
                else
                {
                    AnnouncementsViewModel announce = new AnnouncementsViewModel();

                    if (preview==1)
                    {
                        AnnouncementsViewModel announcement_data = db.AnnouncementsViewModels.Where(x => x.startDate <= DateTime.Today).OrderByDescending(a => a.startDate).FirstOrDefault();

                        announce.message = announcement_data.message;
                        announce.ID = announcement_data.ID;
                        announce.hide = "show";
                    }
                    else
                    {
                        AnnouncementsViewModel announcement_data = db.AnnouncementsViewModels.Where(a => a.status == "1" && a.startDate <= DateTime.Today).OrderByDescending(a => a.startDate).FirstOrDefault();
                        var announce_log = db.announcement_logViewModels.Where(a => a.userID == userId.ToString() && a.announcementID == announcement_data.ID);

                        if (announce_log.Count() == 0)
                        {
                            if (announcement_data.startDate < DateTime.Today || announcement_data.startDate==null)
                            {
                                if (announcement_data.endDate >= DateTime.Today || announcement_data.endDate == null)
                                {
                                    announce.pages = announcement_data.pages;
                                    announce.message = announcement_data.message;
                                    announce.ID = announcement_data.ID;
                                    announce.hide = "show";
                                }
                            }
                        }
                        else
                        {
                            announce.hide = "hide";
                        }
                    }

                    return Json(announce);
                }
            }
            catch (Exception e)
            {
                return Json("bad" + e);
            }
        }


        // GET: Announcements/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnnouncementsViewModel announcementsViewModel = await db.AnnouncementsViewModels.FindAsync(id);
            if (announcementsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(announcementsViewModel);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            AnnouncementsViewModel announcementsViewModel = await db.AnnouncementsViewModels.FindAsync(id);
            db.AnnouncementsViewModels.Remove(announcementsViewModel);
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

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
    public class WebshopController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: partnerStockCheck
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long compaId = Convert.ToInt64(Session["companyId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                List<WebshopConnectViewModel> webshopViewModel = new List<WebshopConnectViewModel>();
                IList<partnerCompanyViewModel> compdata;
                if (!User.IsInRole("Channel User"))
                {
                    compdata = await db.partnerCompanyViewModels.ToListAsync();
                }
                else
                {
                    compdata = await db.partnerCompanyViewModels.Where(a => a.comp_ID == compaId).ToListAsync();
                }

                // Add to the companies list
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata)
                {
                    var webshop_data = dbEntity.partnerLocations.Where(b => b.comp_ID == item.comp_ID && b.loc_Webshop_account != null);
                    int count_webshopCounts = webshop_data.Count();
                    complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name, n2id = count_webshopCounts });
                }
                ViewBag.compData = complist.OrderBy(a => a.name);

                //Add to the model
                long? usrID = Convert.ToInt64(Request.QueryString["user_ID"]);
                long companyId = Convert.ToInt64(Session["companyId"]);

                var webshopView = await db.WebshopConnectViewModels.Where(a => a.usr_user == usrID).ToListAsync();
                foreach (var item in webshopView)
                {
                    var locdata = db.partnerLocationViewModels.Join(
                        db.partnerCompanyViewModels,
                        comp => comp.comp_ID,
                        loc => loc.comp_ID,
                        (loc, comp) => new { loc, comp }
                        ).Where(a => a.loc.loc_Webshop_account == item.ws_account && a.loc.loc_ID==item.loc_id);

                    if (locdata.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(locdata.FirstOrDefault().loc.loc_name))
                        {
                            webshopViewModel.Add(new WebshopConnectViewModel
                            {
                                ws_ID = item.ws_ID,
                                ws_account = item.ws_account,
                                company_name = locdata.FirstOrDefault().comp.comp_name + "-" + locdata.FirstOrDefault().loc.loc_name,
                            });
                        }
                        else
                        {
                            webshopViewModel.Add(new WebshopConnectViewModel
                            {
                                ws_ID = item.ws_ID,
                                ws_account = item.ws_account,
                                company_name = locdata.FirstOrDefault().comp.comp_name + "-" + locdata.FirstOrDefault().loc.loc_city,
                            });
                        }
                    }
                    else
                    {
                        webshopViewModel.Add(new WebshopConnectViewModel
                        {
                            ws_ID = item.ws_ID,
                            ws_account = item.ws_account,
                            company_name = "Can't find Company data",
                        });
                    }

                }
                return View(webshopViewModel);

            }
            return View();

        }

        public ActionResult WSLocations(int compId = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var locdata = db.partnerLocationViewModels.Join(
                db.partnerCompanyViewModels,
                comp => comp.comp_ID,
                loc => loc.comp_ID,
                (loc, comp) => new { loc, comp }
                ).Where(a => a.comp.comp_ID == compId);

            List<WSdata> loc_list = new List<WSdata>();
            foreach (var item in locdata)
            {
                loc_list.Add(new WSdata { loc_ID = item.loc.loc_ID, ws_account = item.loc.loc_Webshop_account, ws_password = item.loc.loc_Webshop_password, loc_name = item.loc.loc_name, comp_name = item.comp.comp_name, loc_city = item.loc.loc_city });
            }

            return View(loc_list);
        }

        public async Task<ActionResult> SaveWSAccount(string ws_account = null, int userId = 0, int locid = 0)
        {
            long userIds = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userIds == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var webshopViewModel = new WebshopConnectViewModel();
            webshopViewModel.ws_account = ws_account;
            webshopViewModel.usr_user = userId;
            webshopViewModel.loc_id = locid;
            db.WebshopConnectViewModels.Add(webshopViewModel);
            await db.SaveChangesAsync();

            //return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["userId"], compid = Request.QueryString["dcompid"] });
            var location = await db.partnerLocationViewModels.FindAsync(locid);
            webshopViewModel.company_name = Request.QueryString["compName"];
            webshopViewModel.location_name = location.loc_name;

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(webshopViewModel.ws_ID), "Webshop", DateTime.Now, " WS account " + ws_account + " was added to user " + userId + " by user " + userIds, "WS_Saved", Convert.ToInt32(userIds));

            return View(webshopViewModel);
        }

        // GET: Webshop
        //public async Task<ActionResult> Index()
        //{
        //    return View(await db.WebshopConnectViewModels.ToListAsync());
        //}

        // GET: Webshop/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WebshopConnectViewModel webshopConnectViewModel = await db.WebshopConnectViewModels.FindAsync(id);
            if (webshopConnectViewModel == null)
            {
                return HttpNotFound();
            }
            return View(webshopConnectViewModel);
        }

        // GET: Webshop/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Webshop/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ws_ID,ws_account,usr_user,loc_id")] WebshopConnectViewModel webshopConnectViewModel)
        {
            if (ModelState.IsValid)
            {
                db.WebshopConnectViewModels.Add(webshopConnectViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(webshopConnectViewModel);
        }

        // GET: Webshop/Edit/5
        public async Task<ActionResult> Edit(long? id)
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
            WebshopConnectViewModel webshopConnectViewModel = await db.WebshopConnectViewModels.FindAsync(id);
            if (webshopConnectViewModel == null)
            {
                return HttpNotFound();
            }

            return View(webshopConnectViewModel);
        }

        // POST: Webshop/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ws_ID,ws_account,usr_user,loc_id")] WebshopConnectViewModel webshopConnectViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin"))
                {
                    var compdata = db.partnerCompanyViewModels;
                    List<Nav1List> complist = new List<Nav1List>();
                    foreach (var item in compdata)
                    {
                        complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name });
                    }
                    ViewBag.compData = complist;
                }

                db.Entry(webshopConnectViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(webshopConnectViewModel.ws_ID), "Webshop", DateTime.Now, " Webshop was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["user_ID"], compid = Request.QueryString["compid"], back = Request.QueryString["back"] });
            }
            return View(webshopConnectViewModel);
        }

        //public async Task<ActionResult> Edit([Bind(Include = "ws_ID,ws_account,usr_user,loc_id")] WebshopConnectViewModel webshopConnectViewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(webshopConnectViewModel).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(webshopConnectViewModel);
        //}

        // GET: Webshop/Delete/5
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
            WebshopConnectViewModel webshopConnectViewModel = await db.WebshopConnectViewModels.FindAsync(id);
            if (webshopConnectViewModel == null)
            {
                return HttpNotFound();
            }

            db.WebshopConnectViewModels.Remove(webshopConnectViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(webshopConnectViewModel.ws_ID), "Webshop", DateTime.Now, " user id " + webshopConnectViewModel.usr_user + " with Webshop account " + webshopConnectViewModel.ws_account + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["user_ID"], compid = Request.QueryString["compid"], back = Request.QueryString["back"] });
        }

        //public async Task<ActionResult> Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    WebshopConnectViewModel webshopConnectViewModel = await db.WebshopConnectViewModels.FindAsync(id);
        //    if (webshopConnectViewModel == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(webshopConnectViewModel);
        //}


        // POST: Webshop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            WebshopConnectViewModel webshopConnectViewModel = await db.WebshopConnectViewModels.FindAsync(id);
            db.WebshopConnectViewModels.Remove(webshopConnectViewModel);
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

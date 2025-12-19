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
    public class partnerStockCheckController : Controller
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
                List<partnerStockCheckViewModel> partnerStockCheckModel = new List<partnerStockCheckViewModel>();
                IList<partnerCompanyViewModel> compdata ;
                if (!User.IsInRole("Channel User"))
                {
                    compdata = await db.partnerCompanyViewModels.ToListAsync();
                }
                else
                {
                    compdata = await db.partnerCompanyViewModels.Where(a=>a.comp_ID==compaId).ToListAsync();
                }

                // Add to the companies list
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata)
                {
                   var sap_data = dbEntity.partnerLocations.Where(b=>b.comp_ID==item.comp_ID && b.loc_SAP_account!=0 && b.loc_SAP_account != null);
                   int count_sapCounts = sap_data.Count();
                   complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name,n2id= count_sapCounts });
                }
                ViewBag.compData = complist.OrderBy(a=>a.name);

                //Add to the model
                long? usrID = Convert.ToInt64(Request.QueryString["user_ID"]);
                long companyId = Convert.ToInt64(Session["companyId"]);

                var partnerStockCheckView = await db.partnerStockCheckViewModels.Where(a => a.usr_user == usrID).ToListAsync();
                foreach (var item in partnerStockCheckView)
                {
                    var locdata = db.partnerLocationViewModels.Join(
                        db.partnerCompanyViewModels,
                        comp => comp.comp_ID,
                        loc => loc.comp_ID,
                        (loc, comp) => new { loc, comp }
                        ).Where(a => a.loc.loc_SAP_account == item.ps_account);

                    if (locdata.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(locdata.FirstOrDefault().loc.loc_name))
                        {
                            partnerStockCheckModel.Add(new partnerStockCheckViewModel
                            {
                                ps_ID = item.ps_ID,
                                ps_account = item.ps_account,
                                company_name = locdata.FirstOrDefault().comp.comp_name + "-" + locdata.FirstOrDefault().loc.loc_name,
                            });
                        }
                        else
                        {
                            partnerStockCheckModel.Add(new partnerStockCheckViewModel
                            {
                                ps_ID = item.ps_ID,
                                ps_account = item.ps_account,
                                company_name = locdata.FirstOrDefault().comp.comp_name + "-" + locdata.FirstOrDefault().loc.loc_city,
                            });
                        }
                    }
                    else
                    {
                        partnerStockCheckModel.Add(new partnerStockCheckViewModel
                        {
                            ps_ID = item.ps_ID,
                            ps_account = item.ps_account,
                            company_name = "Can't find Company data",
                        });
                    }

                }
                return View(partnerStockCheckModel);

            }
            return View();

        }

        // GET: partnerStockCheck/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            partnerStockCheckViewModel partnerStockCheckViewModel = await db.partnerStockCheckViewModels.FindAsync(id);
            if (partnerStockCheckViewModel == null)
            {
                return HttpNotFound();
            }
            return View(partnerStockCheckViewModel);
        }
     
        public ActionResult SAPLocations(int compId=0)
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
                
            List<SAPdata> loc_list = new List<SAPdata>();
            foreach (var item in locdata)
            {
                loc_list.Add(new SAPdata { loc_ID = item.loc.loc_ID, sap_account = item.loc.loc_SAP_account,sap_password=item.loc.loc_SAP_password,loc_name=item.loc.loc_name,comp_name=item.comp.comp_name,loc_city=item.loc.loc_city });
            }

            return View(loc_list);
        }

        public async Task<ActionResult> SaveSAPAccount(int sap_account = 0, int userId=0,int locid=0)
        {
            long userIds = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userIds == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var partnerStockCheckViewModel = new partnerStockCheckViewModel();
            partnerStockCheckViewModel.ps_account = sap_account;
            partnerStockCheckViewModel.usr_user = userId;
            partnerStockCheckViewModel.loc_id = locid;
            db.partnerStockCheckViewModels.Add(partnerStockCheckViewModel);
            await db.SaveChangesAsync();

            //return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["userId"], compid = Request.QueryString["dcompid"] });

            partnerStockCheckViewModel.company_name = Request.QueryString["compName"];


            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(partnerStockCheckViewModel.ps_ID), "PartnerStockCheck", DateTime.Now, " SAP account " + sap_account + " was added to user "+userId+" by user " + userIds, "SAP_Saved", Convert.ToInt32(userIds));

            return View(partnerStockCheckViewModel);
        }

        // GET: partnerStockCheck/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: partnerStockCheck/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ps_ID,ps_account,usr_user")] partnerStockCheckViewModel partnerStockCheckViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.partnerStockCheckViewModels.Add(partnerStockCheckViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerStockCheckViewModel.ps_ID), "PartnerStockCheck", DateTime.Now, " Partner Stock Check was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index");
            }

            return View(partnerStockCheckViewModel);
        }

        // GET: partnerStockCheck/Edit/5
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
            partnerStockCheckViewModel partnerStockCheckViewModel = await db.partnerStockCheckViewModels.FindAsync(id);
            if (partnerStockCheckViewModel == null)
            {
                return HttpNotFound();
            }

            return View(partnerStockCheckViewModel);
        }

        // POST: partnerStockCheck/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ps_ID,ps_account,usr_user")] partnerStockCheckViewModel partnerStockCheckViewModel)
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

                db.Entry(partnerStockCheckViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(partnerStockCheckViewModel.ps_ID), "PartnerStockCheck", DateTime.Now, " Partner Stock Check was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["user_ID"], compid = Request.QueryString["compid"], back = Request.QueryString["back"] });
            }
            return View(partnerStockCheckViewModel);
        }

        // GET: partnerStockCheck/Delete/5
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
            partnerStockCheckViewModel partnerStockCheckViewModel = await db.partnerStockCheckViewModels.FindAsync(id);
            if (partnerStockCheckViewModel == null)
            {
                return HttpNotFound();
            }

            db.partnerStockCheckViewModels.Remove(partnerStockCheckViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(partnerStockCheckViewModel.ps_ID), "PartnerStockCheck", DateTime.Now, " user id "+ partnerStockCheckViewModel.usr_user + " with SAP account "+ partnerStockCheckViewModel.ps_account + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n1_name = @Request.QueryString["n1_name"], n2_name = @Request.QueryString["n2_name"], msg = @Request.QueryString["msg"], user_ID = @Request.QueryString["user_ID"], compid = Request.QueryString["compid"], back = Request.QueryString["back"] });
        }

        // POST: partnerStockCheck/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            partnerStockCheckViewModel partnerStockCheckViewModel = await db.partnerStockCheckViewModels.FindAsync(id);
            db.partnerStockCheckViewModels.Remove(partnerStockCheckViewModel);
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

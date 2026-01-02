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
    public class Nav3ViewModelController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: Nav3ViewModel
        public async Task<ActionResult> Index(int parentID = 0, int n2ID = 14, string n1_name = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (parentID == 0)
            {
                ViewBag.n1_name = "Dashboard/Home";
            }
            else
            {
                ViewBag.n1_name = n1_name;
            }
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            var locController = new CommonController();

            //Add n1ID to the list of n1IDs for the drop down
            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1);
            List<Nav1List> list_n1ID = new List<Nav1List>();
            foreach (var n11dsitems in n1ids)
            {
                var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == n11dsitems.n1ID && a.n2ID != 12 && a.n2ID != 13 && a.n2ID != 16 && a.n2ID != 64 && a.n2ID != 65);

                foreach (var n12dsitems in n2ids)
                {
                    int Converted_n2id = Convert.ToInt32(n12dsitems.n2ID);
                    list_n1ID.Add(new Nav1List {
                        id = n11dsitems.n1ID,
                        name = locController.localization("nav1", "n1_nameLong", n11dsitems.n1_nameLong, n11dsitems.n1ID, languageId),
                        n2id = n12dsitems.n2ID,
                        n2name = locController.localization("nav2", "n2_nameLong", n12dsitems.n2_nameLong, Converted_n2id, languageId)
                    });
                }
            }
            ViewBag.list_n1ID = list_n1ID;

            var nav3data = await db.Nav3ViewModel.Where(a => a.n2ID == n2ID).ToListAsync();

            List<Nav3ViewModel> nav3ViewModel = new List<Nav3ViewModel>();
            List<Nav3Viewlabels> nav3labels = new List<Nav3Viewlabels>();
            if (nav3data.Count() > 0)
            {
                foreach (var items in nav3data)
                {
                    nav3labels.Add(new Nav3Viewlabels
                    {
                        n3order_label = locController.GetLable("Nav Order", "Nav3ViewModel", "Index", languageId),
                        n3_descLong_label = locController.GetLable("Long Description", "Nav3ViewModel", "Index", languageId),
                        n3_nameLong_label = locController.GetLable("Long Name", "Nav3ViewModel", "Index", languageId),
                        filter_link_label = locController.GetLable("Filter Level 3 Menu", "Nav3ViewModel", "Index", languageId),
                        add_link_label = locController.GetLable("Add Level 3 Menu Item", "Nav3ViewModel", "Index", languageId),
                        edit_label = locController.GetLable("Edit", "Nav3ViewModel", "Index", languageId),
                        delete_label = locController.GetLable("Delete", "Nav3ViewModel", "Index", languageId)
                    });

                    int n3id = Convert.ToInt32(items.n3ID);
                    nav3ViewModel.Add(new Nav3ViewModel
                    {
                        n3ID = items.n3ID,
                        n2ID = items.n2ID,
                        list_n2ID = items.list_n2ID,
                        n3order = items.n3order,
                        n3_nameShort = items.n3_nameShort,
                        n3_nameLong = locController.localization("nav3", "n3_nameLong", items.n3_nameLong, n3id, languageId),
                        n3_descShort = items.n3_descShort,
                        n3_descLong = locController.localization("nav3", "n3_descLong", items.n3_descLong, n3id, languageId),
                        n3_active = items.n3_active,
                        n3_products = items.n3_products,
                        list_products = items.list_products,
                        n3_usrTypes = items.n3_usrTypes,
                        n3_editBy = items.n3_editBy,
                        n3_redirect = items.n3_redirect,
                        n3_keywords = items.n3_keywords,
                        n3_industry = items.n3_industry,
                        old_n3id = items.old_n3id,
                        old_n2id = items.old_n2id,
                        nav3labels = nav3labels
                    });
                }
            }
            else
            {
                nav3labels.Add(new Nav3Viewlabels
                {
                    n3order_label = locController.GetLable("Nav Order", "Nav3ViewModel", "Index", languageId),
                    n3_descLong_label = locController.GetLable("Long Description", "Nav3ViewModel", "Index", languageId),
                    n3_nameLong_label = locController.GetLable("Long Name", "Nav3ViewModel", "Index", languageId),
                    filter_link_label = locController.GetLable("Filter Level 3 Menu", "Nav3ViewModel", "Index", languageId),
                    add_link_label = locController.GetLable("Add Level 3 Menu Item", "Nav3ViewModel", "Index", languageId),
                    edit_label = locController.GetLable("Edit", "Nav3ViewModel", "Index", languageId),
                    delete_label = locController.GetLable("Delete", "Nav3ViewModel", "Index", languageId)
                });

                //int n3id = Convert.ToInt32(items.n3ID);
                nav3ViewModel.Add(new Nav3ViewModel
                {
                    //n3ID = items.n3ID,
                    //n2ID = items.n2ID,
                    //list_n2ID = items.list_n2ID,
                    //n3order = items.n3order,
                    //n3_nameShort = items.n3_nameShort,
                    //n3_nameLong = locController.localization("nav3", "n3_nameLong", items.n3_nameLong, n3id, languageId),
                    //n3_descShort = items.n3_descShort,
                    //n3_descLong = locController.localization("nav3", "n3_descLong", items.n3_descLong, n3id, languageId),
                    //n3_active = items.n3_active,
                    //n3_products = items.n3_products,
                    //list_products = items.list_products,
                    //n3_usrTypes = items.n3_usrTypes,
                    //n3_editBy = items.n3_editBy,
                    //n3_redirect = items.n3_redirect,
                    //n3_keywords = items.n3_keywords,
                    //n3_industry = items.n3_industry,
                    //old_n3id = items.old_n3id,
                    //old_n2id = items.old_n2id,
                    nav3labels = nav3labels
                });
            }
            IOrderedEnumerable<Nav3ViewModel> listNav3 = nav3ViewModel.OrderByDescending(a => a.n3ID);
            if (Request.IsAjaxRequest())
            {
                return PartialView(listNav3);
            }
            return View(listNav3);
        }

        // GET: Nav3ViewModel/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);

            if (nav3ViewModel == null)
            {
                return HttpNotFound();
            }
            return View(nav3ViewModel);
        }

        // GET: Nav3ViewModel/Create
        public ActionResult Create(int n1id,int n2id, string n1_name)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.n1_name = n1_name;

            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            var Nav3ViewModel = new Nav3ViewModel();
            {
                Nav3ViewModel.n2ID = n2id;
                Nav3ViewModel.list_Type = list_types;
                Nav3ViewModel.list_industry = list_industry;
                Nav3ViewModel.list_products = list_products;
            };

            return View(Nav3ViewModel);
        }

        // POST: Nav3ViewModel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "n3ID,n2ID,n3order,n3_nameShort,n3_nameLong,n3_descShort,n3_descLong,n3_active,n3_products,n3_usrTypes,n3_editBy,n3_editDate,n3_redirect,n3_keywords,n3_industry,old_n3id,old_n2id")] Nav3ViewModel nav3ViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                nav3ViewModel.n3_products = Request.Form["n3_products"];
                nav3ViewModel.n3_usrTypes = Request.Form["n3_usrTypes"];
                nav3ViewModel.n3_industry = Request.Form["n3_industry"];

                db.Nav3ViewModel.Add(nav3ViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav3ViewModel.n3ID), "Nav3ViewModel", DateTime.Now, " Nav3 was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg=Request.QueryString["msg"] });
            }

            return View(nav3ViewModel);
        }

        // GET: Nav3ViewModel/Edit/5
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

            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);

            long? n2ID = nav3ViewModel.n2ID;
            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1);

            //Get the name of the sub menu
            var n2data = n2ids.Where(a => a.n2ID == n2ID);
            ViewBag.n2_name = n2data.FirstOrDefault().n2_nameLong;

            //Get n1ID of Item
            int? n1ID = n2data.FirstOrDefault().n1ID;

            //Get the name of the top menu
            var n1data = dbEntity.nav1.Where(a => a.n1_active == 1).Where(a=>a.n1ID== n2data.FirstOrDefault().n1ID);
            ViewBag.n1_name = n1data.FirstOrDefault().n1_nameLong;

            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;

            //Add n1ID to the list of n1IDs for the drop down
            List<SelectListItem> list_n2ID = new List<SelectListItem>();
            list_n2ID.Add(new SelectListItem { Text = "Select Top Nav", Value = "select", Selected = true });
            
            foreach (var items in n2ids.Where(a => a.n1ID == n1ID))
            {
                list_n2ID.Add(new SelectListItem { Text = items.n2_nameLong, Value = items.n2ID.ToString() });
            }

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            nav3ViewModel.list_industry = list_industry;
            nav3ViewModel.list_products = list_products;
            nav3ViewModel.list_Type = list_types;
            nav3ViewModel.list_n2ID = list_n2ID;

            if (nav3ViewModel == null)
            {
                return HttpNotFound();
            }

            return View(nav3ViewModel);
        }

        // POST: Nav3ViewModel/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "n3ID,n2ID,n3order,n3_nameShort,n3_nameLong,n3_descShort,n3_descLong,n3_active,n3_products,n3_usrTypes,n3_editBy,n3_editDate,n3_redirect,n3_keywords,n3_industry,old_n3id,old_n2id")] Nav3ViewModel nav3ViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                nav3ViewModel.n3_products = Request.Form["n3_products"];
                nav3ViewModel.n3_usrTypes = Request.Form["n3_usrTypes"];
                nav3ViewModel.n3_industry = Request.Form["n3_industry"];

                db.Entry(nav3ViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav3ViewModel.n3ID), "Nav3ViewModel", DateTime.Now, " Nav3 was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"] });
            }

            return View(nav3ViewModel);
        }

        // GET: Nav3ViewModel/Delete/5
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
            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);
            if (nav3ViewModel == null)
            {
                return HttpNotFound();
            }
            db.Nav3ViewModel.Remove(nav3ViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(nav3ViewModel.n3ID), "Nav3ViewModel", DateTime.Now, " Nav3 was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n2Id = Request.QueryString["n2Id"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"] });
        }

        // POST: Nav3ViewModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);
            db.Nav3ViewModel.Remove(nav3ViewModel);
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

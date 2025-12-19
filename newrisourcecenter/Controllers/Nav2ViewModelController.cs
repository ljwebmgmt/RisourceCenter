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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace newrisourcecenter.Controllers
{
    [Authorize(Roles = "Super Admin,Rittal User")]
    public class Nav2ViewModelController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: Nav2ViewModel
        public async Task<ActionResult> Index(int parentID = 0, string n1_name = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            var locController = new CommonController();

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (parentID == 0)
            {
                ViewBag.n1_name = locController.localization("nav1", "n1_nameLong", "Dashboard/Home", 0, languageId);
            }
            else
            {
                ViewBag.n1_name = locController.localization("nav1", "n1_nameLong", "Dashboard/Home", parentID, languageId);
            }

            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1);
            //Add n1ID to the list of n1IDs for the drop down
            List<Nav1List> list_n1ID = new List<Nav1List>();
            List<Nav2ViewModel> nav2Model = new List<Nav2ViewModel>();
            foreach (var items in n1ids)
            {
                list_n1ID.Add(new Nav1List {
                    id = items.n1ID,
                    name = locController.localization("nav1", "n1_nameLong", items.n1_nameLong, items.n1ID, languageId)   
                });
            }
            ViewBag.list_n1ID = list_n1ID;

            var nav2Data = await db.Nav2ViewModel.Where(a => a.n1ID == parentID).OrderByDescending(a => a.n2ID).ToListAsync();

            List<Nav2Viewlabels> nav2labels = new List<Nav2Viewlabels>();

            foreach (var item in nav2Data)
            {
                nav2labels.Add(new Nav2Viewlabels
                {
                    n2order_label = locController.GetLable("Nav Order", "Nav2ViewModel", "Index", languageId),
                    n2_descLong_label = locController.GetLable("Long Description", "Nav2ViewModel", "Index", languageId),
                    n2_nameLong_label = locController.GetLable("Long Name", "Nav2ViewModel", "Index", languageId),
                    filter_link_label = locController.GetLable("Filter Level 2 Menu", "Nav2ViewModel", "Index", languageId),
                    add_link_label = locController.GetLable("Add a Level 2 Menu Item", "Nav2ViewModel", "Index", languageId),
                    edit_label = locController.GetLable("Edit", "Nav2ViewModel", "Index", languageId),
                    delete_label = locController.GetLable("Delete", "Nav2ViewModel", "Index", languageId)
                });

                int n2id = Convert.ToInt32(item.n2ID);
                nav2Model.Add(new Nav2ViewModel
                {
                    n2ID = item.n2ID,
                    n2order = item.n2order,
                    n2_nameLong = locController.localization("nav2", "n2_nameLong", item.n2_nameLong, n2id, languageId),
                    n2_descLong = locController.localization("nav2", "n2_descLong", item.n2_descLong, n2id, languageId),
                    nav2labels = nav2labels
                });
            }  

            return View(nav2Model);
        }

        // GET: Nav2ViewModel/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Nav2ViewModel nav2ViewModel = await db.Nav2ViewModel.FindAsync(id);

            if (nav2ViewModel == null)
            {
                return HttpNotFound();
            }
            return View(nav2ViewModel);
        }

        // GET: Nav2ViewModel/Create
        public ActionResult Create()
        {
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1);
            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var usrs = dbEntity.usr_user;
            var risou_icon = db.risourcesTypeViewModels;

            //Add n1ID to the list of n1IDs for the drop down
            List<SelectListItem> list_n1ID = new List<SelectListItem>();
            list_n1ID.Add(new SelectListItem { Text = "Select Top Nav", Value = "select", Selected = true });
            list_n1ID.Add(new SelectListItem { Text = "Dashboard", Value = "0" });

            foreach (var items in n1ids)
            {
                list_n1ID.Add(new SelectListItem { Text = items.n1_nameLong, Value = items.n1ID.ToString() });
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

            //Get user for the IT and IE approval list
            List<usr_user> list_user = new List<usr_user>();
            foreach (var items in usrs.OrderBy(a => a.usr_fName))
            {
                list_user.Add(new usr_user { usr_ID = items.usr_ID, usr_fName = items.usr_fName, usr_lName = items.usr_lName });
            }

            //push the roles into a list
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var roles = roleManager.Roles;
            List<rolesList> list_roles = new List<rolesList>();
            foreach (var item in roles)
            {
                list_roles.Add(new rolesList { id = item.Id, name = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId) });
            }

            var Nav2ViewModel = new Nav2ViewModel();
            {
                Nav2ViewModel.risource_icon = risou_icon;//Add risource Images to the list to be used
                Nav2ViewModel.list_n1ID = list_n1ID;
                Nav2ViewModel.list_Type = list_types;
                Nav2ViewModel.list_industry = list_industry;
                Nav2ViewModel.list_products = list_products;
                Nav2ViewModel.list_user = list_user;
                Nav2ViewModel.user_roles = list_roles;
            };

            return View(Nav2ViewModel);
        }

        // POST: Nav2ViewModel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "n2ID,n1ID,n2order,n2_nameShort,n2_nameLong,n2_descShort,n2_descLong,n2_descLongAlt,n2_active,n2_products,n2_usrTypes,n2_editBy,n2_editDate,n2_headerImg,n2_redirect,n2_redirectJS,n2_keywords,old_n3id,old_n2id,n2_industry,n2_IT_approver,n2_IE_approver,PageName,Controller,usr_group")] Nav2ViewModel nav2ViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                nav2ViewModel.n2_products = Request.Form["n2_products"];
                nav2ViewModel.n2_usrTypes = Request.Form["n2_usrTypes"];
                nav2ViewModel.n2_industry = Request.Form["n2_industry"];
                nav2ViewModel.usr_group = Request.Form["usr_group"];

                var ID = Request.Form["n1ID"];

                db.Nav2ViewModel.Add(nav2ViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav2ViewModel.n2ID), "Nav2ViewModel", DateTime.Now, " Nav2 was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { parentID = ID, n1_name = Request.QueryString["n1_name"], msg = Request.QueryString["msg"] });
            }

            return View(nav2ViewModel);
        }

        // GET: Nav2ViewModel/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Nav2ViewModel nav2ViewModel = await db.Nav2ViewModel.FindAsync(id);

            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1);
            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var usrs = dbEntity.usr_user;
            var risou_icon = db.risourcesTypeViewModels;

            //Add n1ID to the list of n1IDs for the drop down
            List<SelectListItem> list_n1ID = new List<SelectListItem>();
            list_n1ID.Add(new SelectListItem { Text = "Select Top Nav", Value = "select", Selected = true });
            list_n1ID.Add(new SelectListItem { Text = "Dashboard", Value = "0" });

            foreach (var items in n1ids)
            {
                list_n1ID.Add(new SelectListItem { Text = items.n1_nameLong, Value = items.n1ID.ToString() });
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

            //Get user for the IT and IE approval list
            List<usr_user> list_user = new List<usr_user>();
            foreach (var items in usrs.OrderBy(a => a.usr_fName))
            {
                list_user.Add(new usr_user { usr_ID = items.usr_ID, usr_fName = items.usr_fName, usr_lName = items.usr_lName });
            }

            //push the roles into a list
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var roles = roleManager.Roles;
            List<rolesList> list_roles = new List<rolesList>();
            foreach (var item in roles)
            {
                list_roles.Add(new rolesList { id = item.Id, name = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId) });
            }      

            nav2ViewModel.risource_icon = risou_icon;//Add risource Images to the list to be used
            nav2ViewModel.list_user = list_user;
            nav2ViewModel.list_industry = list_industry;
            nav2ViewModel.list_products = list_products;
            nav2ViewModel.list_Type = list_types;
            nav2ViewModel.list_n1ID = list_n1ID;
            nav2ViewModel.user_roles = list_roles;

            if (nav2ViewModel == null)
            {
                return HttpNotFound();
            }

            return View(nav2ViewModel);
        }

        // POST: Nav2ViewModel/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "n2ID,n1ID,n2order,n2_nameShort,n2_nameLong,n2_descShort,n2_descLong,n2_descLongAlt,n2_active,n2_products,n2_usrTypes,n2_editBy,n2_editDate,n2_headerImg,n2_redirect,n2_redirectJS,n2_keywords,old_n3id,old_n2id,n2_industry,n2_IT_approver,n2_IE_approver,PageName,Controller,usr_group")] Nav2ViewModel nav2ViewModel, HttpPostedFileBase attachment)
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
                    var path = Path.Combine(Server.MapPath("~/attachments/risources"), file);
                    nav2ViewModel.n2_headerImg = file;
                    attachment.SaveAs(path);
                }

                nav2ViewModel.n2_products = Request.Form["n2_products"];
                nav2ViewModel.n2_usrTypes = Request.Form["n2_usrTypes"];
                nav2ViewModel.n2_industry = Request.Form["n2_industry"];
                nav2ViewModel.usr_group = Request.Form["usr_group"];

                var ID = Request.Form["n1ID"];

                db.Entry(nav2ViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav2ViewModel.n2ID), "Nav2ViewModel", DateTime.Now, " Nav2 was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { parentid = ID, n1_name = Request.QueryString["n1_name"], msg = Request.QueryString["msg"] });
            }

            return View(nav2ViewModel);
        }

        // GET: Nav2ViewModel/Delete/5
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
            Nav2ViewModel nav2ViewModel = await db.Nav2ViewModel.FindAsync(id);
            if (nav2ViewModel == null)
            {
                return HttpNotFound();
            }

            db.Nav2ViewModel.Remove(nav2ViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(nav2ViewModel.n2ID), "Nav2ViewModel", DateTime.Now, " Nav2 was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index",new { parentid=Request.QueryString["parentid"], n1_name=Request.QueryString["n1_name"], msg = Request.QueryString["msg"] });
        }

        // POST: Nav2ViewModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Nav2ViewModel nav2ViewModel = await db.Nav2ViewModel.FindAsync(id);

            db.Nav2ViewModel.Remove(nav2ViewModel);
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

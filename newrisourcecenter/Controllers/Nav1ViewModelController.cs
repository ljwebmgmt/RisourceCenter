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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace newrisourcecenter.Controllers
{
    [Authorize(Roles = "Super Admin,Rittal User")]
    public class Nav1ViewModelController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        // GET: Nav1ViewModel
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var locController = new CommonController();
            List<Nav1ViewModel> nav1Model = new List<Nav1ViewModel>();

            var nav1Data = await db.Nav1ViewModel.ToListAsync();
            List<Nav1Viewlabels> nav1labels = new List<Nav1Viewlabels>();

            foreach (var item in nav1Data)
            {
                nav1labels.Add(new Nav1Viewlabels
                {
                    n1order_label = locController.GetLable("Link Order", "Nav1ViewModel", "Index", languageId),
                    n1_descLong_label = locController.GetLable("Long Description", "Nav1ViewModel", "Index", languageId),
                    n1_nameLong_label = locController.GetLable("Long Name", "Nav1ViewModel", "Index", languageId),
                    add_link_label = locController.GetLable("Add a Top Menu Item", "Nav1ViewModel", "Index", languageId),
                    edit_label = locController.GetLable("Edit", "Nav1ViewModel", "Index", languageId),
                    delete_label = locController.GetLable("Delete", "Nav1ViewModel", "Index", languageId)
                });

                nav1Model.Add(new Nav1ViewModel {
                    n1ID=item.n1ID,
                    n1order=item.n1order,
                    n1_nameLong = locController.localization( "nav1", "n1_nameLong", item.n1_nameLong,item.n1ID, languageId),
                    n1_descLong= locController.localization("nav1", "n1_descLong", item.n1_descLong, item.n1ID, languageId),
                    default_language=item.default_language,
                    list_nav1_labels=nav1labels
                });
            }
                        
            return View(nav1Model);
        }

        // GET: Nav1ViewModel/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nav1ViewModel nav1ViewModel = await db.Nav1ViewModel.FindAsync(id);
            if (nav1ViewModel == null)
            {
                return HttpNotFound();
            }
            return View(nav1ViewModel);
        }

        // GET: Nav1ViewModel/Create
        public ActionResult Create()
        {
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            var locController = new CommonController();

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //push the roles into a list
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var roles = roleManager.Roles;
            List<rolesList> list_roles = new List<rolesList>();
            Nav1ViewModel nav1ViewModel = new Nav1ViewModel();

            foreach (var item in roles)
            {
                list_roles.Add(new rolesList { id = item.Id, name = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId) });
            }

            //select language from the countries table
            var language = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> list_languages = new List<SelectListItem>();
            list_languages.Add(new SelectListItem {Text = locController.GetLable("Select your Language", "Nav1ViewModel", "Index", languageId)[0],Value = "select", Selected = true});//default value for select dropdown
            foreach (var item in language.OrderBy(a => a.Language))//iterate the add function
            {
                if (item.country_id != 38)
                {
                    list_languages.Add(new SelectListItem {
                        Text = locController.GetLable(item.Language, "Nav1ViewModel", "Index", languageId)[0],
                        Value = item.country_id.ToString()
                    });
                }
            }

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries.OrderBy(a => a.country_id))
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { name = locController.GetLable(country.country_long, "Nav1ViewModel", "Index", languageId)[0], id = country.country_id });
                }
            }

            //Apply the page lables to the edit page
            List<Nav1Viewlabels> nav1labels = new List<Nav1Viewlabels>();
            nav1labels.Add(new Nav1Viewlabels
            {
                usr_group_label = locController.GetLable("Roles User Role", "Nav1ViewModel", "Index", languageId),
                pageName_label = locController.GetLable("Nav. Page Name", "Nav1ViewModel", "Index", languageId),
                n1_headerImg_label = locController.GetLable("Header Image", "Nav1ViewModel", "Index", languageId),
                n1_active_label = locController.GetLable("Nav. Status", "Nav1ViewModel", "Index", languageId),
                controller_label = locController.GetLable("Nav. Controller Name", "Nav1ViewModel", "Index", languageId),
                linkId_label = locController.GetLable("Link hidden Name", "Nav1ViewModel", "Index", languageId),
                default_language = locController.GetLable("Language", "Nav1ViewModel", "Index", languageId),
                location = locController.GetLable("Location", "Nav1ViewModel", "Index", languageId),
                n1order_label = locController.GetLable("Link Order", "Nav1ViewModel", "Index", languageId),
                n1_descLong_label = locController.GetLable("Long Description", "Nav1ViewModel", "Index", languageId),
                n1_descShort_label = locController.GetLable("Short Description", "Nav1ViewModel", "Index", languageId),
                n1_nameshort_label = locController.GetLable("Short Name", "Nav1ViewModel", "Index", languageId),
                n1_nameLong_label = locController.GetLable("Long Name", "Nav1ViewModel", "Index", languageId),
                add_link_label = locController.GetLable("Add a Top Menu Item", "Nav1ViewModel", "Index", languageId),
                edit_label = locController.GetLable("Edit", "Nav1ViewModel", "Index", languageId),
                delete_label = locController.GetLable("Delete", "Nav1ViewModel", "Index", languageId),
                active_label = locController.GetLable("Active", "Nav1ViewModel", "Index", languageId),
                not_active_label = locController.GetLable("Not Active", "Nav1ViewModel", "Index", languageId),
                select_status_label= locController.GetLable("Select A Status", "Nav1ViewModel", "Index", languageId)
            });

            nav1ViewModel.nav1labels = nav1labels;
            nav1ViewModel.user_roles = list_roles;
            nav1ViewModel.list_language_ids = list_languages;
            nav1ViewModel.list_location_ids = UserCountries;

            return View(nav1ViewModel);
        }

        // POST: Nav1ViewModel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "n1ID,n1order,n1_nameShort,n1_nameLong,linkId,pageName,controller,n1_descShort,n1_descLong,n1_editBy,n1_editDate,n1_headerImg,usr_group,n1_active,default_language,locations")] Nav1ViewModel nav1ViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                nav1ViewModel.usr_group = Request.Form["usr_group"];
                nav1ViewModel.locations = Request.Form["locations"];

                db.Nav1ViewModel.Add(nav1ViewModel);
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav1ViewModel.n1ID), "Nav1ViewModel", DateTime.Now, " Nav1 was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Index",new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"] });
            }

            return View(nav1ViewModel);
        }

        // GET: Nav1ViewModel/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            var locController = new CommonController();

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nav1ViewModel nav1ViewModel = new Nav1ViewModel();
            var nav1Model = await db.Nav1ViewModel.FindAsync(id);
            if (nav1ViewModel == null)
            {
                return HttpNotFound();
            }
            nav1ViewModel.n1ID = nav1Model.n1ID;
            nav1ViewModel.n1order = nav1Model.n1order;
            nav1ViewModel.n1_nameShort = nav1Model.n1_nameShort;
            nav1ViewModel.n1_nameLong = nav1Model.n1_nameLong;
            nav1ViewModel.linkId = nav1Model.linkId;
            nav1ViewModel.pageName = nav1Model.pageName;
            nav1ViewModel.controller = nav1Model.controller;
            nav1ViewModel.n1_descLong = nav1Model.n1_descLong;
            nav1ViewModel.n1_descShort = nav1Model.n1_descShort;
            nav1ViewModel.n1_active = nav1Model.n1_active;
            nav1ViewModel.n1_headerImg = nav1Model.n1_headerImg;
            nav1ViewModel.usr_group = nav1Model.usr_group;
            nav1ViewModel.default_language = nav1Model.default_language;
            nav1ViewModel.locations = nav1Model.locations;

            //select language from the countries table
            var language = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> list_languages = new List<SelectListItem>();
            list_languages.Add(new SelectListItem { Text = locController.GetLable("Select your Language", "Nav1ViewModel", "Index", languageId)[0], Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in language.OrderBy(a => a.Language))//iterate the add function
            {
                if (item.country_id != 38)
                {
                    list_languages.Add(new SelectListItem
                    {
                        Text = locController.GetLable(item.Language, "Nav1ViewModel", "Index", languageId)[0],
                        Value = item.country_id.ToString()
                    });
                }
            }
            //select language from the countries table
            var userlanguage = language.Where(a => a.country_id== nav1Model.default_language);
            ViewBag.language = userlanguage.FirstOrDefault().Language;

            //push the roles into a list
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var roles = roleManager.Roles;
            List<rolesList> list_roles = new List<rolesList>();
            foreach (var item in roles)
            {
                list_roles.Add(new rolesList { id = item.Id, name = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId) });
            }

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries.OrderBy(a => a.country_id))
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { name = locController.GetLable(country.country_long, "Nav1ViewModel", "Index", languageId)[0], id = country.country_id });
                }
            }

            //Apply the page lables to the edit page
            List<Nav1Viewlabels> nav1labels = new List<Nav1Viewlabels>();
            nav1labels.Add(new Nav1Viewlabels
            {
                usr_group_label = locController.GetLable("Roles User Role", "Nav1ViewModel", "Index", languageId),
                pageName_label = locController.GetLable("Nav. Page Name", "Nav1ViewModel", "Index", languageId),
                n1_headerImg_label = locController.GetLable("Header Image", "Nav1ViewModel", "Index", languageId),
                n1_active_label = locController.GetLable("Nav. Status", "Nav1ViewModel", "Index", languageId),
                controller_label = locController.GetLable("Nav. Controller Name", "Nav1ViewModel", "Index", languageId),
                linkId_label = locController.GetLable("Link hidden Name", "Nav1ViewModel", "Index", languageId),
                default_language = locController.GetLable("Language", "Nav1ViewModel", "Index", languageId),
                location = locController.GetLable("Location", "Nav1ViewModel", "Index", languageId),
                n1order_label = locController.GetLable("Link Order", "Nav1ViewModel", "Index", languageId),
                n1_descLong_label = locController.GetLable("Long Description", "Nav1ViewModel", "Index", languageId),
                n1_descShort_label = locController.GetLable("Short Description", "Nav1ViewModel", "Index", languageId),
                n1_nameshort_label = locController.GetLable("Short Name", "Nav1ViewModel", "Index", languageId),
                n1_nameLong_label = locController.GetLable("Long Name", "Nav1ViewModel", "Index", languageId),
                add_link_label = locController.GetLable("Add a Top Menu Item", "Nav1ViewModel", "Index", languageId),
                edit_label = locController.GetLable("Edit", "Nav1ViewModel", "Index", languageId),
                delete_label = locController.GetLable("Delete", "Nav1ViewModel", "Index", languageId),
                active_label = locController.GetLable("Active", "Nav1ViewModel", "Index", languageId),
                not_active_label = locController.GetLable("Not Active", "Nav1ViewModel", "Index", languageId)
            });
         
            nav1ViewModel.nav1labels = nav1labels;
            nav1ViewModel.user_roles = list_roles;
            nav1ViewModel.list_language_ids = list_languages;
            nav1ViewModel.list_location_ids = UserCountries;

            return View(nav1ViewModel);
        }

        // POST: Nav1ViewModel/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "n1ID,n1order,n1_nameShort,n1_nameLong,linkId,pageName,controller,n1_descShort,n1_descLong,n1_editBy,n1_editDate,n1_headerImg,usr_group,n1_active,default_language,locations")] Nav1ViewModel nav1ViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                nav1ViewModel.usr_group = Request.Form["usr_group"];
                nav1ViewModel.locations = Request.Form["locations"];

                db.Entry(nav1ViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(nav1ViewModel.n1ID), "Nav1ViewModel", DateTime.Now, " Nav1 was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"] });
            }
            return View(nav1ViewModel);
        }

        // GET: Nav1ViewModel/Delete/5
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
            Nav1ViewModel nav1ViewModel = await db.Nav1ViewModel.FindAsync(id);
            if (nav1ViewModel == null)
            {
                return HttpNotFound();
            }
            db.Nav1ViewModel.Remove(nav1ViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(nav1ViewModel.n1ID), "Nav1ViewModel", DateTime.Now, " Nav1 was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"] });
            //return View(nav1ViewModel);
        }

        // POST: Nav1ViewModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Nav1ViewModel nav1ViewModel = await db.Nav1ViewModel.FindAsync(id);
            db.Nav1ViewModel.Remove(nav1ViewModel);
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

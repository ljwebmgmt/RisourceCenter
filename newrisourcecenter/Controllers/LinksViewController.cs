using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public class LinksViewController : Controller
    {
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();

        // GET: LinksView
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            LinksViewModel linksViewModel = new LinksViewModel();

            var n1_descLong = db.nav1.Where(a => a.n1ID == 6).FirstOrDefault().n1_descLong;
            string industry = Convert.ToString(Session["userIndustry"]);
            string usrType = Convert.ToString(Session["companyType"]);
            string products = Convert.ToString(Session["userProducts"]);
            string siteRole = "";
            int id = 6;
            if (User.IsInRole("Super Admin"))
            {
                siteRole = "1";
            }

            CommonController commonController = new CommonController();
            List<nav2> nav2 = commonController.SubmenFilter(industry, usrType, products, siteRole, id);

            linksViewModel.n1_descLong = n1_descLong;
            linksViewModel.list_n2_data = nav2;

            return View(linksViewModel);

        }

        // GET: LinksView/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LinksView/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LinksView/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: LinksView/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LinksView/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: LinksView/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LinksView/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

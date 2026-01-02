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
using Newtonsoft.Json;
using Ionic.Zip;
using newrisourcecenter.ViewModels;
using CsvHelper;
using System.Globalization;

namespace newrisourcecenter.Controllers
{
    public class RiSourcesController : Controller
    {
       // private RisourceCenterContext db = new RisourceCenterContext();
        //private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        //CommonController locController = new CommonController();

        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        private RisourceCenterContext db;
        private CommonController locController = new CommonController();

        public RiSourcesController() : this(new RisourceCenterContext())
        {

        }

        public RiSourcesController(RisourceCenterContext _db)
        {
            db = _db;
        }

        // GET: RiSources
        [Authorize]
        public ActionResult Index(int childid=0, int n3id=0, string n1_name=null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ViewBag.n1_name==null)
            {
                ViewBag.n1_name = "RiSources";
            }
            else
            {
                ViewBag.n1_name = n1_name;
            }

            var riSourcesCart = db.RiSourcesCarts.Where(a => a.user_id == userId).ToList();
            ViewBag.riousrceID = riSourcesCart.Count();

            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);

            var partnerProducts = dbEntity.partnerProducts;
            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }
            ViewBag.list_products = list_products.OrderBy(a=>a.pp_product);

            //Add partner industry to the list of types for the drop down
            var partnerIndustry = dbEntity.partnerIndustries;
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }
            ViewBag.list_industry = list_industry.OrderBy(a => a.pi_industry);

            //Add n1ID to the list of n1IDs for the drop down
            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4);
            List<Nav1List> list_n2ID = new List<Nav1List>();
            foreach (var n2dsitems in n2ids)
            {
                if ( n2dsitems.n2ID != 46 )
                {
                    if ( n2dsitems.n2ID != 49 && n2dsitems.n2ID!=42 && n2dsitems.n2ID!=78)
                    {
                        list_n2ID.Add(new Nav1List { id = n2dsitems.n2ID, name = n2dsitems.n2_nameLong,n3order=n2dsitems.n2order });
                    }
                }
            }
            ViewBag.list_n2ID = list_n2ID.OrderBy(a=>a.n3order);

            //Add partner type to the list of types for the drop down
            var risouCategories = dbEntity.risourcesCategories;
            List<risourcesCategory> list_cat = new List<risourcesCategory>();
            foreach (var items in risouCategories)
            {
                list_cat.Add(new risourcesCategory { cat_id = items.cat_id, ris_categories = items.ris_categories });
            }
            ViewBag.list_cat = list_cat.OrderBy(a => a.ris_categories);

            //Add partner type to the list of types for the drop down
            var risourPartnerApp = db.PartnerApplicationViewModels;
            List<PartnerApplicationViewModel> list_partnerApp = new List<PartnerApplicationViewModel>();
            foreach (var items in risourPartnerApp)
            {
                list_partnerApp.Add(new PartnerApplicationViewModel { appli_id= items.appli_id, appli_name = items.appli_name });
            }
            ViewBag.list_partnerApp =list_partnerApp.OrderBy(a => a.appli_name);

            var riSourcesData = db.RiSourcesViewModels.Join(
                     db.Nav2ViewModel,
                     risou => risou.n2ID,
                     nav2 => nav2.n2ID,
                     (risou, nav2) => new { risou, nav2 }
                 ).Where(a => a.risou.ris_status == "1");

            List<RiSourcesModel> risources = new List<RiSourcesModel>();

            if (userIndustry != "3")
            {
                if (childid != 0 && n3id == 0)
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID,ris_categories= items.risou.ris_categories, ris_link = items.risou.ris_link,n2_headerImg=items.nav2.n2_headerImg,file_size=items.risou.file_size,displayimage=items.risou.displayimage,file_type=items.risou.file_type,dateCreated=items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }
                }
                else if (childid != 0 && n3id != 0)
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.n3ID == n3id).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link,n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }
                }
                else
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link,n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }

                }
            }
            else
            {
                if (childid != 0 && n3id == 0)
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link,n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }
                }
                else if (childid != 0 && n3id != 0)
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.n3ID == n3id).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link,n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }
                }
                else
                {
                    foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                    {
                        risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories,ris_link = items.risou.ris_link,n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated, listRisources = riSourcesCart.Select(a => a.ris_ID) });
                    }

                }
            }

            return View(risources);
        }

        // GET: RiSources
        [Authorize(Roles = "Super Admin,Local Admin,Rittal User")]
        public async Task<ActionResult> Allrisouces(int parentID = 2, int n2id=0, string n1_name = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.n1_name = n1_name;

            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4);

            //Add n1ID to the list of n1IDs for the drop down
            Dictionary<long,Nav1List> list_n2ID = new Dictionary<long,Nav1List>();
            List<long> excludedIds = new List<long>() { 49, 46, 42, 78, 30177 };
            foreach (var n2dsitems in n2ids)
            {
                if (excludedIds.Contains(n2dsitems.n2ID))
                    continue;
                list_n2ID.Add(n2dsitems.n2ID, new Nav1List { id = n2dsitems.n2ID, name = n2dsitems.n2_nameLong, img = n2dsitems.n2_headerImg, n3order = n2dsitems.n2order });    
            }
            ViewBag.list_n2ID = list_n2ID.OrderBy(a => a.Value.n3order);
            List<RiSourcesViewModel> listRisources = null;
            if (n2id == 0) {
                listRisources = await db.RiSourcesViewModels.OrderByDescending(a => a.ris_ID).ToListAsync();
            }
            else
            {
                listRisources = await db.RiSourcesViewModels.OrderByDescending(a => a.ris_ID).Where(a => a.n2ID == n2id).ToListAsync();
            }
            if(Request.IsAjaxRequest())
            {
                return PartialView("_RisourcesTable", listRisources);
            }
            return View(listRisources);
        }

        // GET: RiSources
        public ActionResult List(int childid = 0,string ind_id=null,string prod_id = null,string cat_id = null, string app_id=null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (childid == 0 && prod_id == "" && cat_id == "" && ind_id=="" && app_id=="")//No filters
            {
              return  RedirectToAction("Index","RiSources", new { grid = Request.QueryString["grid"], n1_name = Request.QueryString["n1_name"] });
            }
            else if (childid != 0 && prod_id == "" && cat_id == "" && ind_id == "" && app_id=="")
            {//No filters
                return RedirectToAction("Index", "RiSources",new { childid = childid , n1_name=Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], grid = Request.QueryString["grid"] });
            }

            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);

            var partnerProducts = dbEntity.partnerProducts;
            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }
            ViewBag.list_products = list_products.OrderBy(a => a.pp_product);

            //Add partner industry to the list of types for the drop down
            var partnerIndustry = dbEntity.partnerIndustries;
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }
            ViewBag.list_industry = list_industry.OrderBy(a => a.pi_industry);

            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4);
            //Add n1ID to the list of n1IDs for the drop down
            List<Nav1List> list_n2ID = new List<Nav1List>();
            foreach (var n2dsitems in n2ids)
            {
                if ( n2dsitems.n2ID != 46 ) {
                    if ( n2dsitems.n2ID != 49 && n2dsitems.n2ID != 42 && n2dsitems.n2ID != 78)
                    {
                        list_n2ID.Add(new Nav1List { id = n2dsitems.n2ID, name = n2dsitems.n2_nameLong,n3order=n2dsitems.n2order });
                    }
                }
            }
            ViewBag.list_n2ID = list_n2ID.OrderBy(a=>a.n3order);

            //Add partner type to the list of types for the drop down
            var risourPartnerApp = db.PartnerApplicationViewModels;
            List<PartnerApplicationViewModel> list_partnerApp = new List<PartnerApplicationViewModel>();
            foreach (var items in risourPartnerApp)
            {
                list_partnerApp.Add(new PartnerApplicationViewModel { appli_id = items.appli_id, appli_name = items.appli_name });
            }
            ViewBag.list_partnerApp = list_partnerApp.OrderBy(a => a.appli_name);

            var risouCategories = dbEntity.risourcesCategories;
            //Add partner type to the list of types for the drop down
            List<risourcesCategory> list_cat = new List<risourcesCategory>();
            foreach (var items in risouCategories)
            {
                list_cat.Add(new risourcesCategory { cat_id = items.cat_id, ris_categories = items.ris_categories });
            }
            ViewBag.list_cat = list_cat.OrderBy(a => a.ris_categories);

            var riSourcesData = db.RiSourcesViewModels.Join(
                     db.Nav2ViewModel,
                     risou => risou.n2ID,
                     nav2 => nav2.n2ID,
                     (risou, nav2) => new { risou, nav2 }
                 ).Where(a => a.risou.ris_status == "1");

            List<RiSourcesModel> risources = new List<RiSourcesModel>();

            if (userIndustry != "3")
            {
                /*
                    Note: Categories was switched to industry and Applications become industry later
                    after the system was built and then we added Applications as a different filter
                */
                //Use this part if company id is not equal to 3 which is it is either IT or Industry
                if (childid == 0)
                {//use this when not filtered by file type
                    if (prod_id == "" && cat_id != "" && app_id == "")//filter by only category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_industry.Contains(userIndustry)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id == "" && app_id == "")//filter by only products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && cat_id == "" && app_id != "")//filter by only application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id != "" && app_id=="")//filter by both category and products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && cat_id != "" && app_id != "")//filter by both category and application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id == "" && app_id != "")//filter by both product and application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id != "" && app_id != "")//filter by application,  category and products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }
                }
                else
                {//use this when filtered by file type
                    if (prod_id == "" && cat_id != "" && app_id=="")//filter by only category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_industry.Contains(userIndustry)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id == "" && app_id=="")//filter by only products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && cat_id == "" && app_id != "")//filter by only application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id != "" && app_id=="")//filter by both category and products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && cat_id != "" && app_id != "")//filter by both category and application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id == "" && app_id != "")//filter by both product and application
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && cat_id != "" && app_id != "")//filter by application,  category and products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                }
            }
            else
            {
                //Use this part if company id is equal to 3 for both industry and IT
                if (childid == 0)
                {//use this when not filtered by file type
                    if (prod_id == "" && ind_id == "" && cat_id != "" && app_id=="")//filter by only category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id == "" && app_id=="")//filter by only products type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id == "" && cat_id == "" && app_id != "")//filter by only application type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id == "" && app_id=="")//filter by only industry type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id != "" &&  app_id=="")//product type and category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id == "" && cat_id != "" && app_id != "")//application type and category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }


                    if (prod_id != "" && ind_id == "" && cat_id == "" && app_id != "")//application type and product type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id != "" && app_id=="")//industry type and category type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id == "" && app_id != "")//industry type and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(cat_id)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(cat_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id == "" && app_id=="")//industry type and product type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id != "" && app_id=="")//industry type and Category and Product type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id == "" && app_id != "")//industry type and product and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id != "" && app_id != "")//industry type and Category and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id != "" && app_id != "")//product type and Category type and application type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }                   
                    }

                    if (prod_id != "" && ind_id != "" && cat_id != "" && app_id != "")//product type and Category type and application type
                    {
                        if (ind_id !="3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                }
                else
                {
                    //use this when filtered by file type
                    if (prod_id == "" && ind_id == "" && cat_id != "" && app_id=="")//filter by file type and only category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id == "" && app_id=="")//filter by file type and only products type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id == "" && app_id=="")//filter by file type and only industry type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id == "" && ind_id == "" && cat_id == "" && app_id != "")//filter by file type and only application type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id == "" && cat_id != "" && app_id != "")//filter by file type and application type and category type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id == "" && app_id != "")//filter by file type and application type and product type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id == "" && app_id != "")//filter by file type and industry type and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id != "" && app_id=="")//filter by file type,category type and products
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg,file_size = items.risou.file_size,displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id == "" && app_id=="")//filter by file type,products type and industry type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_industry.Contains(ind_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id != "" && app_id=="")//filter by file type, industry type and Category
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id != "" && app_id=="")//filter by file type, industry type and Category and Product type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id == "" && app_id != "")//filter by file type,industry type and product and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id == "" && ind_id != "" && cat_id != "" && app_id != "")//filter by file type,industry type and Category and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }

                    if (prod_id != "" && ind_id == "" && cat_id != "" && app_id != "")//filter by file type,product type and Category type and application type
                    {
                        foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_partnerApp.Contains(app_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                        }
                    }

                    if (prod_id != "" && ind_id != "" && cat_id != "" && app_id != "")//filter by file type,product type and Category type and application type
                    {
                        if (ind_id != "3")
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_industry.Contains(ind_id)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a=>a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                        else
                        {
                            foreach (var items in riSourcesData.Where(a => a.risou.n2ID == childid).Where(a => a.risou.ris_usrTypes.Contains(companyType)).Where(a => a.risou.ris_categories.Contains(cat_id)).Where(a => a.risou.ris_products.Contains(prod_id)).Where(a => a.risou.ris_partnerApp.Contains(app_id)))
                            {
                                risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = items.risou.file_size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, dateCreated = items.risou.dateCreated });
                            }
                        }
                    }
                }

            }

            return View(risources);

        }

        // GET: RiSources/Level3Menu/5
        public ActionResult Level3Menu(int? n3ID)
        {
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (n3ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var n3ids = dbEntity.nav3.Where(a => a.n3_active == 1).Where(a => a.n2ID == n3ID);

            //Add n2ID to the list of n2IDs for the drop down
            List<SelectListItem> list_n3ID = new List<SelectListItem>();
            list_n3ID.Add(new SelectListItem { Text = "Select A Level 3 Item", Value = "select", Selected = true });

            foreach (var items in n3ids)
            {
                list_n3ID.Add(new SelectListItem { Text = items.n3_nameLong, Value = items.n3ID.ToString() });
            }

            RiSourcesViewModel n3IDModel = new RiSourcesViewModel();

            n3IDModel.list_n3ID = list_n3ID;

            return View(n3IDModel);
        }

        // GET: RiSources/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiSourcesViewModel riSourcesViewModel = await db.RiSourcesViewModels.FindAsync(id);
            if (riSourcesViewModel == null)
            {
                return HttpNotFound();
            }
            return View(riSourcesViewModel);
        }

        // GET: RiSources/Create
        public ActionResult Create(int n2id, string n1_name)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.n1_name = n1_name;

            var n3ids = dbEntity.nav3.Where(a => a.n3_active == 1).Where(a => a.n2ID == n2id);
            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var risouCategories = dbEntity.risourcesCategories;
            var risouDisplayImage = dbEntity.risourcesType_image;
            var partnerApps = db.PartnerApplicationViewModels;

            //Add partner type to the list of types for the drop down
            List<risourcesCategory> list_cat = new List<risourcesCategory>();
            foreach (var items in risouCategories)
            {
                list_cat.Add(new risourcesCategory { cat_id = items.cat_id, ris_categories = items.ris_categories });
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

            //Add partner industry to the list of types for the drop down
            List<risourcesType_image> list_displayimage = new List<risourcesType_image>();
            foreach (var items in risouDisplayImage)
            {
                list_displayimage.Add(new risourcesType_image { ID = items.ID, type_link = items.type_link, type_name = items.type_name });
            }

            //Add partner application to the list of types for the drop down
            List<PartnerApplicationViewModel> list_partnerApps = new List<PartnerApplicationViewModel>();
            foreach (var items in partnerApps)
            {
               list_partnerApps.Add(new PartnerApplicationViewModel { appli_id=items.appli_id, appli_name=items.appli_name, order = items.order });
            }

            var RiSourcesViewModel = new RiSourcesViewModel();
            {
                RiSourcesViewModel.list_displayimage = list_displayimage;
                RiSourcesViewModel.list_categories = list_cat;
                RiSourcesViewModel.list_Type = list_types;
                RiSourcesViewModel.list_industry = list_industry;
                RiSourcesViewModel.list_products = list_products;
                RiSourcesViewModel.list_partnerApp = list_partnerApps;
            };

            return View(RiSourcesViewModel);
        }

        // POST: RiSources/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ris_ID,n2ID,n3ID,ris_headline,ris_teaser,ris_body,ris_status,ris_keywords,ris_products,ris_industry,ris_usrTypes,ris_categories,ris_editedBy,ris_owner,ris_link,ris_order,ris_startDate,ris_endDate,dateCreated,attachment,file_size,displayimage,ris_parterApp")] RiSourcesViewModel riSourcesViewModel, HttpPostedFileBase attachment)
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
                    //var guid = Guid.NewGuid().ToString();
                    var file = fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/risources"), file);
                    riSourcesViewModel.ris_link = file;
                    riSourcesViewModel.file_size = attachment.ContentLength.ToString();
                    riSourcesViewModel.file_type = Path.GetExtension(attachment.FileName);
                    attachment.SaveAs(path);
                }

                riSourcesViewModel.ris_products = Request.Form["ris_products"];
                riSourcesViewModel.ris_usrTypes = Request.Form["ris_usrTypes"];
                riSourcesViewModel.ris_industry = Request.Form["ris_industry"];
                riSourcesViewModel.ris_categories = Request.Form["ris_categories"];
                riSourcesViewModel.ris_partnerApp = Request.Form["ris_parterApp"];

                db.RiSourcesViewModels.Add(riSourcesViewModel);
                await db.SaveChangesAsync();


                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(riSourcesViewModel.ris_ID), "RiSources", DateTime.Now, " RiSources was created by user " + userId, "Create", Convert.ToInt32(userId));

                return RedirectToAction("Allrisouces", new { n2Id=Request.Form["n2ID"], n1_name=Request.Form["n1_name"], n2_name = Request.Form["n2_name"], img =Request.Form["img"] });
            }

            return View(riSourcesViewModel);
        }

        // GET: RiSources/Edit/5
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

            RiSourcesViewModel riSourcesViewModel = await db.RiSourcesViewModels.FindAsync(id);
            //ViewBag.n1_name = n1_name;
            int n2ID = Convert.ToInt32(riSourcesViewModel.n2ID);
            var n3ids = dbEntity.nav3.Where(a => a.n3_active == 1).Where(a => a.n2ID == n2ID);

            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;
            var risouCategories = dbEntity.risourcesCategories;
            var risouDisplayImage = dbEntity.risourcesType_image;
            var partnerApps = db.PartnerApplicationViewModels;

            //Add n1ID to the list of n1IDs for the drop down
            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4);
            List<Nav1List> list_n2ID = new List<Nav1List>();
            foreach (var n2dsitems in n2ids)
            {
                if ( n2dsitems.n2ID != 46 )
                {
                    if ( n2dsitems.n2ID != 49 && n2dsitems.n2ID != 42 && n2dsitems.n2ID != 78)
                    {
                        list_n2ID.Add(new Nav1List { id = n2dsitems.n2ID, name = n2dsitems.n2_nameLong,n3order=n2dsitems.n2order });
                    }
                }
            }
            ViewBag.list_n2ID = list_n2ID.OrderBy(a=>a.n3order);

            //Add partner type to the list of types for the drop down
            List<risourcesCategory> list_cat = new List<risourcesCategory>();
            foreach (var items in risouCategories)
            {
                list_cat.Add(new risourcesCategory { cat_id = items.cat_id, ris_categories = items.ris_categories });
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

            //Add partner industry to the list of types for the drop down
            List<risourcesType_image> list_displayimage = new List<risourcesType_image>();
            foreach (var items in risouDisplayImage)
            {
                list_displayimage.Add(new risourcesType_image { ID = items.ID, type_link = items.type_link,type_name=items.type_name });
            }

            //Add partner application to the list of types for the drop down
            List<PartnerApplicationViewModel> list_partnerApps = new List<PartnerApplicationViewModel>();
            foreach (var items in partnerApps)
            {
                list_partnerApps.Add(new PartnerApplicationViewModel { appli_id = items.appli_id, appli_name = items.appli_name, order = items.order });
            }

            riSourcesViewModel.list_displayimage = list_displayimage;
            riSourcesViewModel.list_industry = list_industry;
            riSourcesViewModel.list_products = list_products;
            riSourcesViewModel.list_Type = list_types;
            riSourcesViewModel.list_categories = list_cat;
            riSourcesViewModel.list_partnerApp = list_partnerApps;

            if (riSourcesViewModel == null)
            {
                return HttpNotFound();
            }
            return View(riSourcesViewModel);
        }

        // POST: RiSources/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ris_ID,n2ID,n3ID,ris_headline,ris_teaser,ris_body,ris_status,ris_keywords,ris_products,ris_industry,ris_usrTypes,ris_categories,ris_editedBy,ris_owner,ris_link,ris_order,ris_startDate,ris_endDate,dateCreated,attachment,file_size,displayimage,ris_partnerApp")] RiSourcesViewModel riSourcesViewModel, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                //update the attached file to the risources
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    //var guid = Guid.NewGuid().ToString();
                    var file = fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/risources"), file);
                    //assign model values from upload 
                    riSourcesViewModel.ris_link = file;
                    riSourcesViewModel.file_size = attachment.ContentLength.ToString();
                    riSourcesViewModel.file_type = Path.GetExtension(attachment.FileName);
                    attachment.SaveAs(path);
                }

                riSourcesViewModel.ris_products = Request.Form["ris_products"];
                riSourcesViewModel.ris_usrTypes = Request.Form["ris_usrTypes"];
                riSourcesViewModel.ris_industry = Request.Form["ris_industry"];
                riSourcesViewModel.ris_categories = Request.Form["ris_categories"];
                riSourcesViewModel.ris_partnerApp = Request.Form["ris_partnerApp"];
                riSourcesViewModel.dateCreated = DateTime.Now;

                if (Request.Form["displayimage"]=="default")
                {
                    //get default image for the Nav2 Id for the RiSource
                    var n2ID = Convert.ToInt32(Request.Form["n2ID"]);
                    var n2image = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4 && a.n2ID==n2ID);
                    if (n2image.Count()!=0)
                    {
                        riSourcesViewModel.displayimage = n2image.FirstOrDefault().n2_headerImg;
                    }
                }
                else
                {
                    riSourcesViewModel.displayimage = Request.Form["displayimage"];
                }

                db.Entry(riSourcesViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(riSourcesViewModel.ris_ID), "RiSources", DateTime.Now, " RiSources was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("Allrisouces", new { n2Id=Request.Form["n2ID"], n1_name=Request.Form["n1_name"], n2_name = Request.Form["n2_name"], img=Request.Form["img"] });
            }

            return View(riSourcesViewModel);
        }

        #region Resources Search
        [HttpPost]
        public async Task<string> SearchRiSources()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "it did not wor";
            }
            var form_value = Request.Form["form_value"];
            var RiSourcesData = await db.RiSourcesViewModels.Where(model => model.ris_headline.Contains(form_value) || model.ris_keywords.Contains(form_value)).ToListAsync();
            List<RiSourcesViewModel> List_RiSources = new List<RiSourcesViewModel>();
            foreach (var item in RiSourcesData)
            {
                List_RiSources.Add(new RiSourcesViewModel { ris_ID = item.ris_ID, ris_headline = item.ris_headline, ris_teaser=item.ris_teaser, n2ID = item.n2ID, ris_link = item.ris_link });
            }

            var restunedString = JsonConvert.SerializeObject(new { RiSourcesData = List_RiSources, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public async Task<string> SearchRiSourcesWithActivity()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "it did not wor";
            }
            string form_value = Request.Form["form_value"];
            DateTime? startDate = null;
            if(Request.Form.AllKeys.Contains<string>("start_date") && !string.IsNullOrEmpty(Request.Form["start_date"]))
                startDate = Convert.ToDateTime(Request.Form["start_date"]);
            DateTime? endDate = null;
            if(Request.Form.AllKeys.Contains<string>("end_date") && !string.IsNullOrEmpty(Request.Form["end_date"]))
                endDate = Convert.ToDateTime(Request.Form["end_date"]);
            long n2ID = !string.IsNullOrEmpty(Request.Form["n2ID"]) ? Convert.ToInt64(Request.Form["n2ID"]) : 0;
            var risources = dbEntity.RiSources.Join(dbEntity.RiSources_Action_Log,risource => risource.ris_ID,action => action.Form_ID,(risource,action) => new { risource, action}).Where(x => true);
            if(n2ID > 0)
            {
                risources = risources.Where(x => x.risource.n2ID == n2ID);
            }
            if (!string.IsNullOrEmpty(form_value))
            {
                risources = risources.Where(x => x.risource.ris_headline.Contains(form_value));
            }
            if(startDate != null && startDate.HasValue)
            {
                risources = risources.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time >= startDate.Value);
            }
            if (endDate != null && endDate.HasValue)
            {
                risources = risources.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time <= endDate.Value);
            }
            Dictionary<int,RisourceActivity> RiSourcesData = await risources.GroupBy(x => x.risource.ris_ID).OrderByDescending(x => x.FirstOrDefault().risource.ris_ID).ToDictionaryAsync(x => x.FirstOrDefault().risource.ris_ID, x => new RisourceActivity()
            {
                ris_ID = x.FirstOrDefault().risource.ris_ID,
                name = x.FirstOrDefault().risource.ris_headline,
                type = x.FirstOrDefault().risource.n2ID.Value,
                link = x.FirstOrDefault().risource.ris_link,
                number_downloads = x.Where(y => y.action.Action == "download").Count(),
                number_selects = x.Where(y => y.action.Action == "select").Count()
            });
            var restunedString = JsonConvert.SerializeObject(new { RiSourcesData = RiSourcesData.Values.ToList(), status = "OK" });
            return restunedString;
        }

        public ActionResult SearchReSources()
        {
            return View();
        }

        public string AddReSourcesFilesToCart(int risourceID,string testing=null)
        {
            int userId = 18127;//This is for testing

            if (testing == null)
            {
                userId = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    RedirectToAction("Login", "Account");
                }
            }
            List<RiSourcesCarts> risourceModels = db.RiSourcesCarts.ToList();
            RiSourcesCarts risourceModel = risourceModels.Where(a => a.ris_ID == risourceID && a.user_id==userId).FirstOrDefault();
            if (risourceModel==null)
            {
                RiSourcesCarts risourceCart = new RiSourcesCarts
                {
                    ris_ID = risourceID,
                    user_id = userId
                };

                db.RiSourcesCarts.Add(risourceCart);
                db.SaveChanges();
            }

            var restunedString = JsonConvert.SerializeObject(new { risourcesModel=GetCartItems(), status = "OK" });

            return restunedString;
        }

        public string ShowCart()
        {
           var restunedString = JsonConvert.SerializeObject(new { risourcesModel=GetCartItems(), status = "OK" });

            return restunedString;
        }

        public List<RiSourcesCarts> GetCartItems()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                RedirectToAction("Login", "Account");
            }

            List<RiSourcesCarts> risourceModels = db.RiSourcesCarts.Where(a => a.user_id == userId).ToList();
            List<RiSourcesCarts> newRiSources = new List<RiSourcesCarts>();
            foreach (var item in risourceModels)
            {
                var item_string = db.RiSourcesViewModels.Find(item.ris_ID);
                if(item_string == null)
                {
                    continue;
                }
                newRiSources.Add(new RiSourcesCarts { ID = item.ID, ris_ID = item.ris_ID, get_headlines = item_string.ris_headline });
            }

            return newRiSources;
        }

        [HttpPost]
        public string DeleteReSourcesFilesToCart(int risourceID, string testing = null)
        {
            if (testing == null)
            {
                int userId = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    RedirectToAction("Login", "Account");
                }
            }
            var get_resources = db.RiSourcesCarts.ToList();

            var risourceCart = get_resources.Where(a=>a.ID == risourceID).FirstOrDefault();
            db.RiSourcesCarts.Remove(risourceCart);
            db.SaveChanges();

            var restunedString = JsonConvert.SerializeObject(new { risourcesModel=GetCartItems(), status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public ActionResult getReSourcesFiles()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            List<RiSourcesCarts> riousrceID = db.RiSourcesCarts.Where(a=>a.user_id==userId).ToList();
            // List<getValues> getva = new List<getValues>();
            var os = new MemoryStream();
            var zip = new ZipFile();
            List<RiSourcesViewModel> risourceModel  = db.RiSourcesViewModels.ToList();
            foreach (var item in riousrceID)
            {
                int id = Convert.ToInt32(item.ris_ID);
                RiSourcesViewModel obj = risourceModel.Where(a => a.ris_ID == id).FirstOrDefault();
                if(obj == null)
                {
                    continue;
                }
                var get_file = obj.ris_link;
                if (System.IO.File.Exists(Server.MapPath("~/attachments/risources/" + get_file)))
                {
                    zip.AddFile(Server.MapPath("~/attachments/risources/" + get_file), "");
                }
            }

            zip.Save(os);
            os.Position = 0;

            return File(os, "application/zip", "RiSourceCenter_Downloads.zip");
        }

        public string getSearchResources(string search=null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "login";
            }
            //check if file has been added to downloads
            var riSourcesCart = db.RiSourcesCarts.Where(a => a.user_id == userId).ToList();
            var countRiSources = riSourcesCart.Count();

            List<SalesCommunicationsViewModel> salesCommunications = new List<SalesCommunicationsViewModel>();
            SearchViewModel searchModel = new SearchViewModel();
            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);
            var riSourcesData = db.RiSourcesViewModels.Join(
                     db.Nav2ViewModel,
                     risou => risou.n2ID,
                     nav2 => nav2.n2ID,
                     (risou, nav2) => new { risou, nav2 }
                 ).Where(a => a.risou.ris_status == "1");

            if (search!="")
            {
                riSourcesData = riSourcesData.Where(a => a.risou.ris_status == "1" && (a.risou.ris_headline.Contains(search) || a.risou.ris_teaser.Contains(search) || a.risou.ris_keywords.Contains(search)));
            }

            List<RiSourcesModel> risources = new List<RiSourcesModel>();

            if (userIndustry != "3")
            {
                foreach (var items in riSourcesData.Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                {
                    string dateCreated = @String.Format("{0:d}", items.risou.dateCreated);
                    string size = @String.Format("{0:(0,0)}", Convert.ToDouble(@items.risou.file_size));
                    Uri uriResult;
                    bool result = Uri.TryCreate(@items.risou.ris_link, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps);

                    if (result)
                    {
                        if (riSourcesCart.Select(a=>a.ris_ID).Contains(items.risou.ris_ID))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "link" });
                        }
                        else
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "link" });
                        }
                    }
                    else
                    {
                        if (riSourcesCart.Select(a => a.ris_ID).Contains(items.risou.ris_ID))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "download",selected="yes" });
                        }
                        else
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "download", selected = "no" });
                        }
                    }
                }
            }
            else
            {
                foreach (var items in riSourcesData.Where(a => a.risou.ris_usrTypes.Contains(companyType)))
                {
                    string dateCreated = @String.Format("{0:d}", items.risou.dateCreated);
                    string size = @String.Format("{0:(0,0)}", Convert.ToDouble(items.risou.file_size));
                    Uri uriResult;
                    bool result = Uri.TryCreate(@items.risou.ris_link, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps);

                    if (result)
                    {
                        if (riSourcesCart.Select(a => a.ris_ID).Contains(items.risou.ris_ID))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "link" });
                        }
                        else
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "link" });
                        }
                    }
                    else
                    {
                        if (riSourcesCart.Select(a => a.ris_ID).Contains(items.risou.ris_ID))
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "download", selected = "yes" });
                        }
                        else
                        {
                            risources.Add(new RiSourcesModel { ris_ID = items.risou.ris_ID, ris_headline = items.risou.ris_headline, ris_body = items.risou.ris_body, ris_teaser = items.risou.ris_teaser, ris_products = items.risou.ris_products, nav2_longName = items.nav2.n2_nameLong, n1ID = 4, n2ID = items.risou.n2ID, n3ID = items.risou.n3ID, ris_categories = items.risou.ris_categories, ris_link = items.risou.ris_link, n2_headerImg = items.nav2.n2_headerImg, file_size = size, displayimage = items.risou.displayimage, file_type = items.risou.file_type, Date_created = dateCreated, status = "download", selected = "no" });
                        }
                    }
                }
            }

            var restunedString = JsonConvert.SerializeObject(new { resourcesData = risources.OrderByDescending(a => a.ris_ID), status = "OK", countRiSources = countRiSources });
            return restunedString;
        }
        #endregion

        // GET: RiSources/Delete/5
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
            RiSourcesViewModel riSourcesViewModel = await db.RiSourcesViewModels.FindAsync(id);
            if (riSourcesViewModel == null)
            {
                return HttpNotFound();
            }
            db.RiSourcesViewModels.Remove(riSourcesViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(riSourcesViewModel.ris_ID), "RiSources", DateTime.Now, " RiSources was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Allrisouces", new { n2Id = Request.QueryString["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], img = Request.QueryString["img"] });

        }

        // POST: RiSources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RiSourcesViewModel riSourcesViewModel = await db.RiSourcesViewModels.FindAsync(id);
            db.RiSourcesViewModels.Remove(riSourcesViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Allrisouces");
        }

        [HttpPost]
        public async Task<JsonResult> TrackDownload(int id, string action)
        {
            dbEntity.RiSources_Action_Log.Add(new RiSources_Action_Log()
            {
                Form_ID = id,
                Action = action,
                Action_Time = DateTime.Now,
                Usr_ID = Session["userId"].ToString()
            });
            await dbEntity.SaveChangesAsync();
            return Json(true);
        }

        // GET: RiSources
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> RisourcesReport(int parentID = 2, int n2id = 0, string n1_name = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.n1_name = n1_name;
            RisourcesReportViewModel viewModel = new RisourcesReportViewModel();
            viewModel.resourceActivities = await dbEntity.RiSources_Action_Log.GroupBy(x => x.Form_ID).ToDictionaryAsync(x => x.FirstOrDefault().Form_ID.Value, x => new RisourceActivity()
            {
                ris_ID = x.FirstOrDefault().Form_ID.Value,
                number_downloads = x.Where(y => y.Action == "download").Count(),
                number_selects = x.Where(y => y.Action == "select").Count()
            });
            List<int> risourceIds = viewModel.resourceActivities.Keys.ToList();
            var risources = db.RiSourcesViewModels.Where(x => risourceIds.Contains(x.ris_ID));
            List<long> risourceTypes = await risources.Select(x => x.n2ID).ToListAsync();
            if (n2id > 0)
            {
                risources = risources.Where(a => a.n2ID == n2id);
            }
            viewModel.resources = await risources.OrderByDescending(a => a.ris_ID).ToListAsync();
            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1 && risourceTypes.Contains(a.n2ID) && a.n1ID == 4);

            //Add n1ID to the list of n1IDs for the drop down
            Dictionary<long, Nav1List> list_n2ID = new Dictionary<long, Nav1List>();
            foreach (var n2dsitems in n2ids)
            {
                list_n2ID.Add(n2dsitems.n2ID, new Nav1List { id = n2dsitems.n2ID, name = n2dsitems.n2_nameLong, img = n2dsitems.n2_headerImg, n3order = n2dsitems.n2order });
            }
            ViewBag.list_n2ID = list_n2ID.OrderBy(a => a.Value.n3order);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<string> GetActivity(int id)
        {
            long loggedUserId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || loggedUserId == 0)
            {
                return "it did not work";
            }

            var RiSourcesData = await dbEntity.RiSources_Action_Log.Where(model => model.Form_ID == id).ToListAsync();
            List<int> userIds = RiSourcesData.Select(x => Convert.ToInt32(x.Usr_ID)).Distinct().ToList();
            Dictionary<int, string> userNames = await dbEntity.usr_user.Where(x => userIds.Contains(x.usr_ID)).ToDictionaryAsync(x => x.usr_ID, x => (x.usr_fName + " " + x.usr_lName).Trim());
            List<RisourceAction> List_RiSources = new List<RisourceAction>();
            foreach (var item in RiSourcesData)
            {
                int userId = Convert.ToInt32(item.Usr_ID);
                List_RiSources.Add(new RisourceAction { user_ID =  userId, username = (userNames.ContainsKey(userId) ? userNames[userId] : ""), action = item.Action, action_time = item.Action_Time.Value });
            }

            var restunedString = JsonConvert.SerializeObject(new { details = List_RiSources, status = "OK" });

            return restunedString;
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<FileStreamResult> ExportRisourcesReport()
        {
            string form_value = Request.QueryString["form_value"];
            long n2id = !string.IsNullOrEmpty(Request.QueryString["n2ID"]) ? Convert.ToInt64(Request.QueryString["n2ID"]) : 0;
            DateTime? startDate = null;
            if (Request.QueryString.AllKeys.Contains<string>("start_date") && !string.IsNullOrEmpty(Request.QueryString["start_date"]))
                startDate = Convert.ToDateTime(Request.QueryString["start_date"]);
            DateTime? endDate = null;
            if (Request.QueryString.AllKeys.Contains<string>("end_date") && !string.IsNullOrEmpty(Request.QueryString["end_date"]))
                endDate = Convert.ToDateTime(Request.QueryString["end_date"]);
            var query = dbEntity.RiSources_Action_Log.Join(dbEntity.RiSources, action => action.Form_ID, risource => risource.ris_ID, (action, risource) => new { action, risource });
            if(n2id > 0)
            {
                query = query.Where(x => x.risource.n2ID == n2id);
            }
            if (!string.IsNullOrEmpty(form_value))
            {
                query = query.Where(x => x.risource.ris_headline.Contains(form_value));
            }
            if (startDate != null && startDate.HasValue)
            {
                query = query.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time >= startDate.Value);
            }
            if (endDate != null && endDate.HasValue)
            {
                query = query.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time <= endDate.Value);
            }
            query = query.OrderByDescending(a => a.risource.ris_ID);
            Dictionary<long, string> types = await dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4).ToDictionaryAsync(x => x.n2ID, x => x.n2_nameLong);
            Dictionary<int, ExportRisourceReportModel> activities = await query.GroupBy(x => x.action.Form_ID).ToDictionaryAsync(x => x.FirstOrDefault().action.Form_ID.Value, x => new ExportRisourceReportModel()
            {
                name = x.FirstOrDefault().risource.ris_headline,
                type = (x.FirstOrDefault().risource.n2ID.HasValue && types.ContainsKey(x.FirstOrDefault().risource.n2ID.Value) ? types[x.FirstOrDefault().risource.n2ID.Value] : ""),
                number_downloads = x.Where(y => y.action.Action == "download").Count(),
                number_selects = x.Where(y => y.action.Action == "select").Count()
            });
            
            var result = WriteCsvToMemory(activities.Values.ToList());
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "risources_summary_report_export.csv" };
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<FileStreamResult> ExportRisourcesActivityReport()
        {
            string form_value = Request.QueryString["form_value"];
            long n2id = !string.IsNullOrEmpty(Request.QueryString["n2ID"]) ? Convert.ToInt64(Request.QueryString["n2ID"]) : 0;
            DateTime? startDate = null;
            if (Request.QueryString.AllKeys.Contains<string>("start_date") && !string.IsNullOrEmpty(Request.QueryString["start_date"]))
                startDate = Convert.ToDateTime(Request.QueryString["start_date"]);
            DateTime? endDate = null;
            if (Request.QueryString.AllKeys.Contains<string>("end_date") && !string.IsNullOrEmpty(Request.QueryString["end_date"]))
                endDate = Convert.ToDateTime(Request.QueryString["end_date"]);
            var query = dbEntity.RiSources_Action_Log.Join(dbEntity.RiSources, action => action.Form_ID, risource => risource.ris_ID, (action, risource) => new { action, risource }).Join(dbEntity.usr_user, a => a.action.Usr_ID, user => user.usr_ID.ToString(), (a, user) => new { a.action, a.risource, user });
            if (n2id > 0)
            {
                query = query.Where(x => x.risource.n2ID == n2id);
            }
            if (!string.IsNullOrEmpty(form_value))
            {
                query = query.Where(x => x.risource.ris_headline.Contains(form_value));
            }
            if (startDate != null && startDate.HasValue)
            {
                query = query.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time >= startDate.Value);
            }
            if (endDate != null && endDate.HasValue)
            {
                query = query.Where(x => x.action.Action_Time.HasValue && x.action.Action_Time <= endDate.Value);
            }
            query = query.OrderByDescending(a => a.risource.ris_ID).OrderBy(a => a.action.Action_Time);
            Dictionary<long, string> types = await dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == 4).ToDictionaryAsync(x => x.n2ID, x => x.n2_nameLong);
            var items = await query.ToListAsync();
            List<ExportRisourceActivityReportModel> activities = new List<ExportRisourceActivityReportModel>();
            foreach (var item in items)
            {
                activities.Add(new ExportRisourceActivityReportModel()
                {
                    name = item.risource.ris_headline,
                    type = (item.risource.n2ID.HasValue && types.ContainsKey(item.risource.n2ID.Value) ? types[item.risource.n2ID.Value] : ""),
                    user = (item.user.usr_fName + " " + item.user.usr_lName).Trim(),
                    action = item.action.Action,
                    action_time = item.action.Action_Time.Value
                });
            }

            var result = WriteCsvToMemory(activities);
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "risources_activity_report_export.csv" };
        }

        public byte[] WriteCsvToMemory(dynamic records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
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

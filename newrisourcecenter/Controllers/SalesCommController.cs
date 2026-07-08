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
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Configuration;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public partial class SalesCommController : Controller
    {
       private RisourceCenterContext db;
       private RisourceCenterMexicoEntities dbEntity;
        CommonController locController = new CommonController();

        public SalesCommController() : this(new RisourceCenterContext(), new RisourceCenterMexicoEntities())
        {

        }

        public SalesCommController(RisourceCenterContext _db, RisourceCenterMexicoEntities _dbEntity)
        {
            db = _db;
            dbEntity = _dbEntity;
        }

        #region Index
        // GET: SalesCommunications/Index
        public ActionResult Index(int childId = 0, int n3id = 0, string submenu=null, string querystring=null, string recent = "latest")
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId==0)
            {
                return RedirectToAction("Login", "Account");
            }

            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);
            List<SalesCommunicationsViewModel> salesCommunications = new List<SalesCommunicationsViewModel>();

            //Get submenu data
            var level2menudata = dbEntity.nav2.Where(a => a.n1ID == 1 && a.n2_active==1);
            List<SalesMenu> level2Menu = new List<SalesMenu>();
            foreach (var item in level2menudata)
            {
                //Exclude the admin link
                if (item.n2ID!=45) {
                    level2Menu.Add(new SalesMenu { n2Id = item.n2ID, n2_longName = item.n2_nameLong });
                }
            }
            salesCommunications.Add(new SalesCommunicationsViewModel { sales_submenu = level2Menu });

            //Get top menu
            if (n3id==0 && childId==0) {
                var menuInfo = dbEntity.nav2.Join(
                           dbEntity.nav3,
                           nav3 => nav3.n2ID,
                           nav2 => nav2.n2ID,
                           (nav2, nav3) => new { nav2, nav3 }
                       ).Where(a => a.nav2.n1ID == 1 && a.nav2.n2_active == 1 && a.nav3.n3_active == 1);

                List<SalesMenu> salesMenu = new List<SalesMenu>();
                foreach (var item in menuInfo)
                {
                    salesMenu.Add(new SalesMenu { n2Id = item.nav2.n2ID, n2_longName = item.nav2.n2_nameLong, n3Id = item.nav3.n3ID, n3_longName = item.nav3.n3_nameLong });
                }
                salesCommunications.Add(new SalesCommunicationsViewModel { sales_menu = salesMenu });
            }
            else if (n3id != 0 && submenu == "yes")
            {
                var menuInfo = dbEntity.nav2.Join(
                           dbEntity.nav3,
                           nav3 => nav3.n2ID,
                           nav2 => nav2.n2ID,
                           (nav2, nav3) => new { nav2, nav3 }
                       ).Where(a => a.nav2.n1ID == 1 && a.nav2.n2_active == 1 && a.nav3.n3_active == 1).Where(a => a.nav3.n3ID ==n3id);

                List<SalesMenu> salesMenu = new List<SalesMenu>();
                foreach (var item in menuInfo)
                {
                    salesMenu.Add(new SalesMenu { n2Id = item.nav2.n2ID, n2_longName = item.nav2.n2_nameLong, n3Id = item.nav3.n3ID, n3_longName = item.nav3.n3_nameLong });
                }
                salesCommunications.Add(new SalesCommunicationsViewModel { sales_menu = salesMenu });
            }
            else
            {
                var menuInfo = dbEntity.nav2.Join(
                           dbEntity.nav3,
                           nav3 => nav3.n2ID,
                           nav2 => nav2.n2ID,
                           (nav2, nav3) => new { nav2, nav3 }
                       ).Where(a => a.nav2.n1ID == 1 && a.nav2.n2_active == 1 && a.nav3.n3_active == 1).Where(a=>a.nav2.n2ID== childId);

                List<SalesMenu> salesMenu = new List<SalesMenu>();
                foreach (var item in menuInfo)
                {
                    salesMenu.Add(new SalesMenu { n2Id = item.nav2.n2ID, n2_longName = item.nav2.n2_nameLong, n3Id = item.nav3.n3ID, n3_longName = item.nav3.n3_nameLong });
                }
                salesCommunications.Add(new SalesCommunicationsViewModel { sales_menu = salesMenu });
            }

            //Get top menu
            if (childId!=0 && childId == 27 && submenu ==null)
            {
                IQueryable<nav2> navInfo = dbEntity.nav2.Where(a => a.n2ID == 27);
                ViewBag.n2Name = navInfo.FirstOrDefault().n2_nameLong;
                return View(childIdis27(salesCommunications, companyType, n3id, querystring, recent));
            }
            else if(childId != 0 && childId != 27 )
            {
                IQueryable<nav2> navInfo = dbEntity.nav2.Where(a => a.n2ID == childId);
                ViewBag.n2Name = navInfo.FirstOrDefault().n2_nameLong;
                return View(childIdisNot27(salesCommunications, childId, companyType, userIndustry, n3id, querystring, recent));
            }else if (n3id!=0 && submenu=="yes")
            {
                return View(onlyN3Id(salesCommunications, companyType, userIndustry, n3id, querystring, recent));
            }
            else
            {
                //IQueryable<nav2> navInfo = dbEntity.nav2;
                //ViewBag.n2Name = navInfo.FirstOrDefault().n2_nameLong;
                return View(defaultNoChildId(salesCommunications, companyType, userIndustry, n3id, querystring, recent));
            }
        }
        #endregion

        #region All Communications
        // GET: SalesComm
        public async Task<ActionResult> AllSalesComms(int parentID = 2, int n2id = 0, string n2_name = null, int n3id = 0, string n3_name = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.n2_name = n2_name;
            ViewBag.n3_name = n3_name;

            //Add n2ID to the list of n2IDs for the dropdown
            var menuFilter = dbEntity.nav2.Join(
                      dbEntity.nav3,
                      nav3 => nav3.n2ID,
                      nav2 => nav2.n2ID,
                      (nav2,nav3) => new { nav2, nav3 }
                  );
            List<Nav1List> nav2_Menu = new List<Nav1List>();
            foreach (var n2dsitems in menuFilter.Where(a=>a.nav2.n1ID==1).OrderByDescending(a=>a.nav3.n2ID))
            {
                nav2_Menu.Add(new Nav1List { id = n2dsitems.nav2.n2ID, name = n2dsitems.nav2.n2_nameLong, img = n2dsitems.nav2.n2_headerImg, n3id=n2dsitems.nav3.n3ID, n3name=n2dsitems.nav3.n3_nameLong});  
            }
            ViewBag.nav_Menu = nav2_Menu;

            if (n3id == 0)
            {
                return View(await db.SalesCommViewModels.OrderByDescending(a => a.scID).ToListAsync());
            }
            else
            {
                return View(await db.SalesCommViewModels.OrderByDescending(a => a.scID).Where(a => a.n3ID == n3id).ToListAsync());
            }
        }
        #endregion

        #region Search Communications
        [HttpPost]
        public async Task<string> SearchComm()
        {
            var form_value = Request.Form["form_value"];
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "it did not wor";
            }

            var communicationsData = await db.SalesCommViewModels.Where(model=>model.sc_headline.Contains(form_value) || model.sc_keywords.Contains(form_value)).ToListAsync();
            List<SalesCommViewModel> List_Comm = new List<SalesCommViewModel>();
            foreach (var item in communicationsData)
            {
                string startDate = string.Format("{0:d}", item.sc_startDate);
                string endDate = string.Format("{0:d}", item.sc_endDate);

                List_Comm.Add(new SalesCommViewModel { scID=item.scID,sc_headline = item.sc_headline,startDate=startDate, endDate=endDate });
            }

            var restunedString = JsonConvert.SerializeObject(new { commData = List_Comm, status = "OK" });

            return restunedString;
        }

        //[HttpPost]
        //public async Task<string> SearchCommuications(string querystring=null,string usrProducts=null,string industry=null,string audience=null, string testing=null)
        //{
        //    string restunedString = "";
        //    DateTime dateStart = Convert.ToDateTime("2015-01-01 00:00:00.000");//format the date for the table request

        //    if (testing==null) {
        //        long userId = Convert.ToInt64(Session["userId"]);
        //        if (!Request.IsAuthenticated || userId == 0)
        //        {
        //            return "it did not wor";
        //        }
        //    }
 
        //    List<SalesCommViewModel> List_Comm = new List<SalesCommViewModel>();
        //    List<SalesCommViewModel> new_communication = new List<SalesCommViewModel>();
        //    List<SalesCommViewModel> communicationsData = db.SalesCommViewModels.ToList();

        //    if (industry == "3")
        //    {
        //        bool islist = usrProducts.Any(m => m.ToString().Contains(","));//Check if products is a list
        //        if (islist)
        //        {
        //            var list_usrProducts = usrProducts.Split(',');
        //            foreach (var item in communicationsData.Where(model => model.sc_headline.Contains(querystring) && model.sc_usrTypes.Contains(audience) || model.sc_keywords.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_startDate > dateStart))
        //            {
        //                foreach (var items in list_usrProducts)
        //                {
        //                    if (item.sc_products.Contains(items))
        //                    {
        //                        new_communication.Add(new SalesCommViewModel {scID=item.scID,sc_headline=item.sc_headline,sc_body=item.sc_body, });
        //                        //communicationsData.Where(model => model.sc_headline.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_products.Contains(items) || model.sc_keywords.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_products.Contains(items) && model.sc_startDate > dateStart).ToList();

        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            communicationsData = communicationsData.Where(model => model.sc_headline.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_products.Contains(usrProducts) || model.sc_keywords.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_products.Contains(usrProducts) && model.sc_startDate > dateStart).ToList();
        //        }
        //    }
        //    else
        //    {
        //        bool islist = usrProducts.Any(m => m.ToString().Contains(","));//Check if products is a list
        //        if (islist)
        //        {
        //            var list_usrProducts = usrProducts.Split(',');
        //            foreach (var item in communicationsData)
        //            {
        //                foreach (var items in list_usrProducts)
        //                {
        //                    if (item.sc_products.Contains(items))
        //                    {
        //                        communicationsData = communicationsData.Where(model => model.sc_headline.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_industry.Contains(industry) && model.sc_products.Contains(items) || model.sc_keywords.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_industry.Contains(industry) && model.sc_products.Contains(items) && model.sc_startDate > dateStart).ToList();

        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            communicationsData = communicationsData.Where(model => model.sc_headline.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_industry.Contains(industry) && model.sc_products.Contains(usrProducts) || model.sc_keywords.Contains(querystring) && model.sc_usrTypes.Contains(audience) && model.sc_industry.Contains(industry) && model.sc_products.Contains(usrProducts) && model.sc_startDate > dateStart).ToList();
        //        }
        //    }

        //    foreach (var item in communicationsData)
        //    {
        //        string startDate = string.Format("{0:d}", item.sc_startDate);
        //        string endDate = string.Format("{0:d}", item.sc_endDate);

        //        List_Comm.Add(new SalesCommViewModel { scID = item.scID, sc_headline = item.sc_headline, startDate = startDate, endDate = endDate });
        //    }

        //    restunedString = JsonConvert.SerializeObject(new { commData = List_Comm, status = "OK" });

        //    return restunedString;
        //}
        #endregion

        #region Get Communications By ID
        //Read sales communication by click through
        public ActionResult getSaleComById(int scID = 0, int n3id=0)
        {
            List<SalesCommunicationsViewModel> salesData = new List<SalesCommunicationsViewModel>();
            DateTime date = DateTime.Today.AddDays(-7);
            ViewBag.WeeklyEmail = "This is the weekly email";

            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //Get submenu data
            var submenu = dbEntity.nav2.Where(a=>a.n1ID==1 && a.n2_active==1);
            List<SalesMenu> level2Menu = new List<SalesMenu>();
            foreach (var item in submenu)
            {
                //Exclude the admin link
                if (item.n2ID != 45)
                {
                    level2Menu.Add(new SalesMenu { n2Id = item.n2ID, n2_longName = item.n2_nameLong });
                }
            }

            //Get sales communications
            var salescom = dbEntity.salesComms.Join(
                   dbEntity.nav3,
                    sales => sales.n3ID,
                    nav3 => nav3.n3ID,
                   (sales, nav3) => new { sales, nav3 }
               )
               .Join(
                   dbEntity.nav2,
                   nav3 => nav3.nav3.n2ID,
                   nav2 => nav2.n2ID,
                   (nav3, nav2) => new { nav3, nav2 }
               ).Where(a => a.nav3.sales.scID == scID);

            //Get level3 menu data
            var menuInfo = dbEntity.nav2.Join(
                       dbEntity.nav3,
                       nav3 => nav3.n2ID,
                       nav2 => nav2.n2ID,
                       (nav2, nav3) => new { nav2, nav3 }
                   ).Where(a => a.nav2.n1ID == 1 && a.nav2.n2_active == 1 && a.nav3.n3_active == 1);
            List<SalesMenu> level3Menu = new List<SalesMenu>();
            foreach (var item in menuInfo)
            {
                level3Menu.Add(new SalesMenu { n2Id = item.nav2.n2ID, n2_longName = item.nav2.n2_nameLong, n3Id = item.nav3.n3ID, n3_longName = item.nav3.n3_nameLong });
            }

            //if childId or n2Id is 27, filter by n2ID==27,n2ID==1,n2ID==3 with the n3ID while ignoring whether rittal user or not
            foreach (var comm in salescom)
            {
                //Add attachment list to the Edit page
                var arrayOfAttachments = comm.nav3.sales.attach_risource;
                List<Nav1List> list_attachments = new List<Nav1List>();
                if (arrayOfAttachments != null)
                {
                    int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);

                    foreach (int item in nums)
                    {
                        var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                        if (risour.Count() > 0)
                        {
                            list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                        }
                    }
                }

                salesData.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products,sales_menu= level3Menu, sales_submenu = level2Menu });
            }

            return View(salesData);

        }
        #endregion

        #region Communications Details
        // GET: SalesComm/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesCommViewModel salesCommViewModel = await db.SalesCommViewModels.FindAsync(id);
            if (salesCommViewModel == null)
            {
                return HttpNotFound();
            }
            return View(salesCommViewModel);
        }
        #endregion

        #region Create Communications
        // GET: SalesComm/Create
        public ActionResult Create(int n2id = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            SalesCommViewModel salesCommViewModel = new SalesCommViewModel();
            //Get IT approver email
            List<string> approverEmails = new List<string>();
            var get_it_approver_email = dbEntity.nav2.Join(
                      dbEntity.usr_user,
                      nav2 => nav2.n2_IT_approver,
                      usr => usr.usr_ID,
                      (nav2, usr) => new { nav2, usr }
                  ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == n2id );

            foreach (var item in get_it_approver_email)
            {
                ViewBag.it_name = item.usr.usr_fName + " " + item.usr.usr_lName;
                ViewBag.it_email = item.usr.usr_email;
                approverEmails.Add(item.usr.usr_email);
            }

            //Get IE approver email
            var get_ie_approver_email = dbEntity.nav2.Join(
                      dbEntity.usr_user,
                      nav2 => nav2.n2_IE_approver,
                      usr => usr.usr_ID,
                      (nav2, usr) => new { nav2, usr }
                  ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == n2id);
            foreach (var item in get_ie_approver_email)
            {
                ViewBag.ie_name = item.usr.usr_fName + " " + item.usr.usr_lName;
                ViewBag.ie_email = item.usr.usr_email;
                approverEmails.Add(item.usr.usr_email);
            }
            string additionalApprovers = ConfigurationManager.AppSettings["additionalCommApprover"];
            if(!string.IsNullOrEmpty(additionalApprovers))
            {
                approverEmails.AddRange(additionalApprovers.Split(','));
            }
            
            string email = string.Join(",", approverEmails);

            //Add partner producst to the list of types for the drop down
            var partnerProducts = dbEntity.partnerProducts;
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            var partnerIndustry = dbEntity.partnerIndustries;
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            //Add partner type to the list of types for the drop down
            var partnerType = dbEntity.partnerTypes;
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //This is the dropdown for the risources
            var level2 = dbEntity.nav2.Where(a=>a.n1ID==4 && a.n2_active==1);
            List<Nav1List> list_level2 = new List<Nav1List>();
            list_level2.Add(new Nav1List { id = 0, name = "Select Risource Type" });
            foreach (var items in level2.OrderBy(a => a.n2_nameLong))
            {
                if ( items.n2ID != 46 )
                {
                    if ( items.n2ID != 49 ) { 
                        list_level2.Add(new Nav1List { id = items.n2ID, name = items.n2_nameLong });
                    }
                }
            }

            List<Nav1List> status = new List<Nav1List>();
            status.Add(new Nav1List { id = 1, name = "Request Approval/Staging" });
            if (email.Contains(User.Identity.GetUserName()))
            {
                status.Add(new Nav1List { id = 2, name = "Live" });
            }
            status.Add(new Nav1List { id = 3, name = "Inactive" });

            //Add attachment list to the Edit page
            List<Nav1List> list_attachments = new List<Nav1List>();
            var arrayOfAttachments = salesCommViewModel.attach_risource;
            if (arrayOfAttachments != null)
            {
                int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);

                foreach (int item in nums)
                {
                    var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                }
            }

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries)
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { id = country.country_id, name = country.country_long, n2name = country.Language });
                }
            }
            ViewBag.countries = UserCountries;

            //set the users country
            List<Nav1List> Userlanguage = new List<Nav1List>();
            foreach (var country in countries)
            {
                if (country.country_id != 38)
                {
                    Userlanguage.Add(new Nav1List { id = country.country_id, name = country.country_long, n2name = country.Language });
                }
            }
            ViewBag.language = Userlanguage;

            salesCommViewModel.list_attachments = list_attachments;
            salesCommViewModel.list_products = list_products;
            salesCommViewModel.list_industry = list_industry;
            salesCommViewModel.list_Type = list_types;
            salesCommViewModel.risource_menu = list_level2;
            salesCommViewModel.list_status = status;

            return View(salesCommViewModel);
        }

        public async Task<ActionResult> GetRiSources(int risource_id=0, int scID=0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (scID != 0) {
                SalesCommViewModel salesCommViewModel = await db.SalesCommViewModels.FindAsync(scID);
                ViewBag.attached_risources = salesCommViewModel.attach_risource;
            }

            IQueryable<RiSource> risour = dbEntity.RiSources.Where(a => a.n2ID == risource_id);
            return View(risour);
        }

        // POST: SalesComm/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "scID,n2ID,n3ID,sc_status,sc_headline,sc_teaser,sc_body,sc_keywords,sc_products,sc_usrTypes,sc_startDate,sc_endDate,sc_owner,sc_industry,old_scid,attach_risource,countries,default_lang,languages")] SalesCommViewModel salesCommViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                salesCommViewModel.sc_products = Request.Form["sc_products"];
                salesCommViewModel.sc_usrTypes = Request.Form["sc_usrTypes"];
                salesCommViewModel.sc_industry = Request.Form["sc_industry"];
                salesCommViewModel.attach_risource = Request.Form["attach_risource"];
                string languages = Request.Form["languages"];
                salesCommViewModel.countries = Request.Form["countries"];
                salesCommViewModel.languages = languages;
                salesCommViewModel.submission_date = DateTime.Today;

                db.SalesCommViewModels.Add(salesCommViewModel);
                await db.SaveChangesAsync();

                //get the identity after the insert
                long id = salesCommViewModel.scID;
                var locali =  new LocalizationModel();
                string[] langs = Request.Form["languages"].Split(',');
                foreach (var item in langs)
                { 
                    locali.language = Convert.ToInt32(item);
                    locali.table_name = "salesComm";
                    locali.column_name = "sc_body";
                    locali.parent_id = Convert.ToInt32(id);
                    locali.message_original = Request.Unvalidated.Form["sc_body"];
                    db.LocalizationModels.Add(locali);
                    await db.SaveChangesAsync();

                    locali.language = Convert.ToInt32(item);
                    locali.table_name = "salesComm";
                    locali.column_name = "sc_headline";
                    locali.parent_id = Convert.ToInt32(id);
                    locali.message_original = Request.Unvalidated.Form["sc_headline"];
                    db.LocalizationModels.Add(locali);
                    await db.SaveChangesAsync();

                    locali.language = Convert.ToInt32(item);
                    locali.table_name = "salesComm";
                    locali.column_name = "sc_teaser";
                    locali.parent_id = Convert.ToInt32(id);
                    locali.message_original = Request.Unvalidated.Form["sc_teaser"];
                    db.LocalizationModels.Add(locali);
                    await db.SaveChangesAsync();
                }

                //start the email function
                emailfunction(salesCommViewModel,"created");

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(id), "SalesComm", DateTime.Now, User.Identity.Name + " Communications was created by user " + userId, "Created", Convert.ToInt32(userId));


                return RedirectToAction("AllSalesComms", new { img = Request.Form["img"], n3id = Request.Form["n3id"], n2id = Request.Form["n2id"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], n3_name = Request.Form["n3_name"] });
            }

            return View(salesCommViewModel);
        }
        #endregion

        #region Edit Communications
        // GET: SalesComm/Edit/5
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
            SalesCommViewModel salesCommViewModel = await db.SalesCommViewModels.FindAsync(id);
            List<string> approverEmails = new List<string>();
            //Get IT approver email
            var get_it_approver_email = dbEntity.nav2.Join(
                      dbEntity.usr_user,
                      nav2 => nav2.n2_IT_approver,
                      usr => usr.usr_ID,
                      (nav2, usr) => new { nav2, usr }
                  ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID==salesCommViewModel.n2ID);

            foreach (var item in get_it_approver_email) {
                ViewBag.it_name = item.usr.usr_fName + " " + item.usr.usr_lName;
                ViewBag.it_email = item.usr.usr_email;
                approverEmails.Add(item.usr.usr_email);
            }

            //Get IE approver email
            var get_ie_approver_email = dbEntity.nav2.Join(
                      dbEntity.usr_user,
                      nav2 => nav2.n2_IE_approver,
                      usr => usr.usr_ID,
                      (nav2, usr) => new { nav2, usr }
                  ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == salesCommViewModel.n2ID);
            foreach (var item in get_ie_approver_email)
            {
                ViewBag.ie_name = item.usr.usr_fName + " " + item.usr.usr_lName;
                ViewBag.ie_email = item.usr.usr_email;
                approverEmails.Add(item.usr.usr_email);
            }
            string additionalApprovers = ConfigurationManager.AppSettings["additionalCommApprover"];
            if (!string.IsNullOrEmpty(additionalApprovers))
            {
                approverEmails.AddRange(additionalApprovers.Split(','));
            }

            string email = string.Join(",", approverEmails);

            //Add partner producst to the list of types for the drop down
            var partnerProducts = dbEntity.partnerProducts;
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            var partnerIndustry = dbEntity.partnerIndustries;
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            //Add partner type to the list of types for the drop down
            var partnerType = dbEntity.partnerTypes;
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add n2ID to the list of n2IDs for the dropdown
            var menuFilter = dbEntity.nav2.Join(
                      dbEntity.nav3,
                      nav3 => nav3.n2ID,
                      nav2 => nav2.n2ID,
                      (nav2, nav3) => new { nav2, nav3 }
                  ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n1ID == 1).Where(a => a.nav3.n3_active == 1);
            List<Nav1List> nav2_Menu = new List<Nav1List>();
            foreach (var n2dsitems in menuFilter)
            {
                if (n2dsitems.nav2.n2_IT_approver!=null && n2dsitems.nav2.n2_IE_approver!=null) {
                    usr_user ie_approver = await dbEntity.usr_user.FindAsync(n2dsitems.nav2.n2_IE_approver);
                    usr_user it_approver = await dbEntity.usr_user.FindAsync(n2dsitems.nav2.n2_IT_approver);

                    nav2_Menu.Add(new Nav1List { id = n2dsitems.nav2.n2ID, name = n2dsitems.nav2.n2_nameLong, img = n2dsitems.nav2.n2_headerImg, n3id = n2dsitems.nav3.n3ID, n3name = n2dsitems.nav3.n3_nameLong, n2_IE_approver = n2dsitems.nav2.n2_IE_approver, n2_IT_approver = n2dsitems.nav2.n2_IT_approver,ie_approver=ie_approver.usr_fName+" "+ie_approver.usr_lName,it_approver= it_approver.usr_fName + " " + it_approver.usr_lName });
                }
                else
                {
                    nav2_Menu.Add(new Nav1List { id = n2dsitems.nav2.n2ID, name = n2dsitems.nav2.n2_nameLong, img = n2dsitems.nav2.n2_headerImg, n3id = n2dsitems.nav3.n3ID, n3name = n2dsitems.nav3.n3_nameLong, n2_IE_approver = n2dsitems.nav2.n2_IE_approver, n2_IT_approver = n2dsitems.nav2.n2_IT_approver });
                }
            }

            List<Nav1List> status = new List<Nav1List>();
            status.Add(new Nav1List { id = 1, name = "Request Approval/Staging" });
            status.Add(new Nav1List { id = 2, name = "Live" });
            status.Add(new Nav1List { id = 3, name = "Inactive" });

            //Add risources menu type to the list of types for the dropdown
            var level2 = dbEntity.nav2.Where(a => a.n1ID == 4 && a.n2_active == 1);
            List<Nav1List> list_level2 = new List<Nav1List>();
            list_level2.Add(new Nav1List { id = 0, name ="Select Risource Type"});
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
            
            //Add attachment list to the Edit page
            List<Nav1List> list_attachments = new List<Nav1List>();
            var arrayOfAttachments = salesCommViewModel.attach_risource;
            if (arrayOfAttachments!=null) {
                int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);

                foreach (int item in nums)
                {
                    var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                    if (risour.Count() > 0)
                    {
                        list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                    }
                }
            }

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<Nav1List> UserCountries = new List<Nav1List>();
            foreach (var country in countries)
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new Nav1List { id = country.country_id, name = country.country_long, n2name = country.Language });
                }
            }
            ViewBag.countries = UserCountries;

            //set the users country
            List<Nav1List> Userlanguage = new List<Nav1List>();
            foreach (var country in countries)
            {
                if (country.country_id != 38)
                {
                    Userlanguage.Add(new Nav1List { id = country.country_id, name = country.country_long, n2name = country.Language });
                }
            }
            ViewBag.language = Userlanguage;

            salesCommViewModel.list_attachments = list_attachments;
            salesCommViewModel.list_n2ID = nav2_Menu;
            salesCommViewModel.list_industry = list_industry;
            salesCommViewModel.list_products = list_products;
            salesCommViewModel.list_Type = list_types;
            salesCommViewModel.list_status = status;
            salesCommViewModel.risource_menu = list_level2;

            if (salesCommViewModel == null)
            {
                return HttpNotFound();
            }

            return View(salesCommViewModel);
        }

        // POST: SalesComm/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "scID,n2ID,n3ID,sc_status,sc_headline,sc_teaser,sc_body,sc_keywords,sc_products,sc_usrTypes,sc_startDate,sc_endDate,sc_owner,sc_industry,old_scid,attach_risource,countries,default_lang,languages,submission_date")] SalesCommViewModel salesCommViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                salesCommViewModel.sc_products = Request.Form["sc_products"];
                salesCommViewModel.sc_usrTypes = Request.Form["sc_usrTypes"];
                salesCommViewModel.sc_industry = Request.Form["sc_industry"];
                string languages = Request.Form["languages"];
                salesCommViewModel.countries = Request.Form["countries"];
                salesCommViewModel.languages = languages;
                salesCommViewModel.attach_risource = Request.Form["attach_risource"];

                db.Entry(salesCommViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //get the identity after the insert
                long id = salesCommViewModel.scID;
                string[] langs = languages.Split(',');

                foreach (var item in langs)
                {
                    var lang = Convert.ToByte(item);
                    var locali_sc_body = db.LocalizationModels.Where(a => a.parent_id == id && a.language==lang && a.column_name== "sc_body");
                    if (locali_sc_body.Count() == 0)
                    {
                        var local = new LocalizationModel();
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_body";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_body"];
                        db.LocalizationModels.Add(local);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var local = dbEntity.Localizations.Find(locali_sc_body.FirstOrDefault().localization_id);
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_body";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_body"];
                        await dbEntity.SaveChangesAsync();
                    }

                    var locali_sc_headline = db.LocalizationModels.Where(a => a.parent_id == id && a.language == lang && a.column_name == "sc_headline");
                    if (locali_sc_headline.Count() == 0)
                    {
                        var local = new LocalizationModel();
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_headline";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_headline"];
                        db.LocalizationModels.Add(local);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var local = dbEntity.Localizations.Find(locali_sc_headline.FirstOrDefault().localization_id);
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_headline";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_headline"];
                        await dbEntity.SaveChangesAsync();
                    }

                    var locali_sc_teaser = db.LocalizationModels.Where(a => a.parent_id == id && a.language == lang && a.column_name == "sc_teaser");
                    if (locali_sc_teaser.Count() == 0)
                    {
                        var local = new LocalizationModel();
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_teaser";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_teaser"];
                        db.LocalizationModels.Add(local);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var local = dbEntity.Localizations.Find(locali_sc_teaser.FirstOrDefault().localization_id);
                        local.language = Convert.ToInt32(item);
                        local.table_name = "salesComm";
                        local.column_name = "sc_teaser";
                        local.parent_id = Convert.ToInt32(id);
                        local.message_original = Request.Unvalidated.Form["sc_teaser"];
                        await dbEntity.SaveChangesAsync();
                    }

                }

                //start the email function
                emailfunction(salesCommViewModel,"edited");

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(salesCommViewModel.scID), "SalesComm", DateTime.Now, User.Identity.Name + " Communications was edited by user " + userId, "Edit", Convert.ToInt32(userId));

                return RedirectToAction("AllSalesComms", new { img=Request.QueryString["img"], n3id=Request.QueryString["n3id"], n2id=Request.QueryString["n2id"], n1_name=Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], n3_name =Request.QueryString["n3_name"] });
            }
            return View(salesCommViewModel);
        }
        #endregion

        #region Delete Communications
        // GET: SalesComm/Delete/5
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
            SalesCommViewModel salesCommViewModel = await db.SalesCommViewModels.FindAsync(id);
            if (salesCommViewModel == null)
            {
                return HttpNotFound();
            }
            db.SalesCommViewModels.Remove(salesCommViewModel);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(id), "SalesComm", DateTime.Now, User.Identity.Name + " Communications was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("AllSalesComms", new { img = Request.QueryString["img"], n3id = Request.QueryString["n3id"], n2id = Request.QueryString["n2id"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], n3_name = Request.QueryString["n3_name"] });
        }

        // POST: SalesComm/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            SalesCommViewModel salesCommViewModel = await db.SalesCommViewModels.FindAsync(id);
            db.SalesCommViewModels.Remove(salesCommViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("AllSalesComms");
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                dbEntity.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

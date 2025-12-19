using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace newrisourcecenter.Controllers
{
    public class SearchController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();

        // GET: SalesCommSearch
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            List<SalesCommunicationsViewModel> salesCommunications = new List<SalesCommunicationsViewModel>();
            SearchViewModel searchModel = new SearchViewModel();
            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);
            string search = Request.QueryString["msg"];

            if (User.IsInRole("Super Admin"))
            {
                var userData = db.UserViewModels.Where(a=>a.usr_fName.Contains(search) || a.usr_lName.Contains(search) || a.usr_email.Contains(search) || a.usr_phone.Contains(search) || a.usr_add1.Contains(search));
                List<UserViewModel> userList = new List<UserViewModel>();
                foreach (var item in userData)
                {
                    userList.Add(new UserViewModel { usr_ID=item.usr_ID, usr_fName=item.usr_fName, usr_lName=item.usr_lName });
                }
                searchModel.user_results = userList;

                var pCompData = db.partnerCompanyViewModels.Where(a => a.comp_name.Contains(search));
                List<partnerCompanyViewModel> pCompList = new List<partnerCompanyViewModel>();
                foreach (var item in pCompData)
                {
                    pCompList.Add(new partnerCompanyViewModel {comp_ID =item.comp_ID, comp_name=item.comp_name });
                }
                searchModel.pComp_results = pCompList;

                var pLocData = db.partnerLocationViewModels.Where(a => a.loc_name.Contains(search) || a.loc_phone.Contains(search) || a.loc_add1.Contains(search) || a.loc_email.Contains(search) || a.loc_web.Contains(search));
                List<partnerLocationViewModel> pLocList = new List<partnerLocationViewModel>();
                foreach (var item in pLocData)
                {
                    pLocList.Add(new partnerLocationViewModel { loc_ID=item.loc_ID,  loc_name = item.loc_name });
                }
                searchModel.pLoc_results = pLocList;
            }

            /*var risourcesData = db.RiSourcesViewModels;
            List<RiSourcesViewModel> risourcesList = new List<RiSourcesViewModel>();
            foreach (var item in risourcesData)
            {
                risourcesList.Add(new RiSourcesViewModel { ris_ID=item.ris_ID, ris_headline =item.ris_headline, ris_link=item.ris_link,file_size=item.file_size,file_type=item.file_type,dateCreated=item.dateCreated });
            }
            */
            var riSourcesData = db.RiSourcesViewModels.Join(
                     db.Nav2ViewModel,
                     risou => risou.n2ID,
                     nav2 => nav2.n2ID,
                     (risou, nav2) => new { risou, nav2 }
                 ).Where(a => a.risou.ris_status == "1" && a.risou.ris_headline.Contains(search) || a.risou.ris_teaser.Contains(search));
            List<RiSourcesModel> risources = new List<RiSourcesModel>();

            if (userIndustry != "3")
            {
                foreach (var items in riSourcesData.Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
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

            searchModel.risources_results = risources;
            searchModel.salesComm_results = defaultNoChildId(salesCommunications, companyType, userIndustry, search);

            return View(searchModel);
        }

        public List<SalesCommunicationsViewModel> defaultNoChildId(List<SalesCommunicationsViewModel> salesCommunications, string companyType, string userIndustry, string search)
        {
            //Default function without any filters
            DateTime date = Convert.ToDateTime("1900-01-01 00:00:00.000");//format the date for the table request
            DateTime dateStart = Convert.ToDateTime("2015-01-01 00:00:00.000");//format the date for the table request

            var salescom = dbEntity.salesComms.Join(
                      dbEntity.nav3,
                       nav3 => nav3.n3ID,
                       sales => sales.n3ID,
                      (sales, nav3) => new { sales, nav3 }
                  )
                  .Join(
                      dbEntity.nav2,
                      nav3 => nav3.nav3.n2ID,
                      nav2 => nav2.n2ID,
                      (nav3, nav2) => new { nav3, nav2 }
                  ).Where(a => a.nav3.sales.sc_status == 2)
                  .Where(a => a.nav3.sales.sc_startDate <= DateTime.Today && a.nav3.sales.sc_usrTypes.Contains(companyType) && a.nav3.sales.sc_headline.Contains(search))
                  .OrderByDescending(a => a.nav3.sales.scID);

            if (userIndustry != "3")
            {
                //Get to those who are not rittal users
                foreach (var comm in salescom.Where(a => a.nav3.sales.sc_industry.Contains(userIndustry) && a.nav3.sales.sc_startDate > dateStart))
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

                    salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                }
            }
            else
            {
                //Get to those who are rittal users
                foreach (var comm in salescom.Where(a => a.nav3.sales.sc_industry.Contains("1") || a.nav3.sales.sc_industry.Contains("2")).Where(m=>m.nav3.sales.sc_startDate > dateStart))
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
                            if (risour.Count()>0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                }
            }

            return salesCommunications;
        }

        // GET: SalesCommSearch
        public async Task<string> Autosearch()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "you need to login";
            }

            List<SalesCommunicationsViewModel> salesCommunications = new List<SalesCommunicationsViewModel>();
            SearchViewModel searchModel = new SearchViewModel();
            countResults count_results = new countResults();
            List<RFQViewModel> myRFQs;
            List<partNumber_search> partnum = new List<partNumber_search>();
            List<Catalog_search> cat_log = new List<Catalog_search>();
            IQueryable<ReturnTools> returnRequest;

            string companyType = Convert.ToString(Session["companyType"]);
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string userProducts = Convert.ToString(Session["userProducts"]);
            string search = Request.QueryString["msg"];
            int id = 0;
            //Check if string is made up of integer characters
            if (search.All(char.IsDigit))
            {
                id = Convert.ToInt32(Request.QueryString["msg"]);
            }

            if (User.IsInRole("Super Admin"))
            {
                var userData = db.UserViewModels.Where(a => a.usr_fName.Contains(search) || a.usr_lName.Contains(search) || a.usr_email.Contains(search) || a.usr_phone.Contains(search) || a.usr_add1.Contains(search));
                List<UserViewModel> userList = new List<UserViewModel>();
                foreach (var item in userData)
                {
                    userList.Add(new UserViewModel { usr_ID = item.usr_ID, usr_fName = item.usr_fName, usr_lName = item.usr_lName });
                }
                searchModel.user_results = userList;
                count_results.user_count = userList.Count().ToString();

                var pCompData = db.partnerCompanyViewModels.Where(a => a.comp_name.Contains(search));
                List<partnerCompanyViewModel> pCompList = new List<partnerCompanyViewModel>();
                foreach (var item in pCompData)
                {
                    pCompList.Add(new partnerCompanyViewModel { comp_ID = item.comp_ID, comp_name = item.comp_name });
                }
                searchModel.pComp_results = pCompList;
                count_results.pComp_count = pCompList.Count().ToString();

                var pLocData = db.partnerLocationViewModels.Where(a => a.loc_name.Contains(search) || a.loc_phone.Contains(search) || a.loc_add1.Contains(search) || a.loc_email.Contains(search) || a.loc_web.Contains(search));
                List<partnerLocationViewModel> pLocList = new List<partnerLocationViewModel>();
                foreach (var item in pLocData)
                {
                    pLocList.Add(new partnerLocationViewModel { loc_ID = item.loc_ID, loc_name = item.loc_name });
                }
                searchModel.pLoc_results = pLocList;
                count_results.pLoc_count = pLocList.Count().ToString();
            }

            //Search results for RFQ
            var user_rfq = db.UserViewModels.Where(a => a.usr_pages.Contains("20121")).Select(a => a.usr_ID);
            if (user_rfq.ToList().Contains(Convert.ToInt32(userId)))
            {
                myRFQs = db.RFQViewModels.Where(a=>a.sold_to_party==search || a.ID==id || a.distro_company==search || a.distro_name==search || a.end_contact == search).OrderByDescending(a => a.ID).ToList();
            }
            else
            {
                myRFQs = db.RFQViewModels.Where(b => b.user_id == userId).Where(a=>a.sold_to_party == search || a.ID == id || a.distro_company == search || a.distro_name == search || a.end_contact==search).OrderByDescending(a => a.ID).ToList();
            }
            searchModel.list_rfq = myRFQs;
            count_results.rfq_count = myRFQs.Count().ToString();

            //Search results for returns
            //var user_return = db.UserViewModels.Where(a => a.usr_pages.Contains("76")).Select(a => a.usr_ID);
            //if (user_return.ToList().Contains(Convert.ToInt32(userId)))
            //{
            //    returnRequest = db.returnTools.Where(a=>a.form_id==id).OrderByDescending(a => a.form_id);
            //}
            //else
            //{
            //    returnRequest = db.returnTools.Where(a => a.user_id == userId).OrderByDescending(a => a.form_id);
            //}
            //searchModel.list_returns = returnRequest;

            //Access Passive Part Numbers
            string catalog_parts = Server.MapPath("~/attachments/catalog-parts.txt");
            StreamReader catalog_data = System.IO.File.OpenText(catalog_parts);
            string line = "";
            while ((line = catalog_data.ReadLine())!=null)
            {
                var firstpart = line.Split(',');
                if (firstpart[0].Contains(search))
                {
                    cat_log.Add(new Catalog_search { partnumber =  firstpart[0]});
                }
            }
            searchModel.search_cat = cat_log;
            count_results.cat_count = cat_log.Count().ToString();

            //Access Passive Part Numbers
            string non_pim_part_ids = Server.MapPath("~/attachments/non-pim-part-ids.txt");
            StreamReader part_data = System.IO.File.OpenText(non_pim_part_ids);
            string part_line = "";
            while ((part_line = part_data.ReadLine()) != null)
            {
                if (part_line.Contains(search))
                {
                    partnum.Add(new partNumber_search { partnumber = part_line });
                }
            }
            searchModel.search_part = partnum;
            count_results.part_count = partnum.Count().ToString();

            //Get all courses
            var myClassDataSet = dbEntity.nav2.Join(
                                          dbEntity.nav3,
                                          nav2data => nav2data.n2ID,
                                          nav3data => nav3data.n2ID,
                                          (nav2data, nav3data) => new { nav2data, nav3data }
                                      ).Where(a => a.nav2data.n1ID == 2 && a.nav2data.n2_active == 1 && a.nav2data.n2_usrTypes.Contains(companyType) && a.nav2data.n2ID != 64 && a.nav2data.n2ID != 65 && a.nav2data.n2ID != 12 && a.nav2data.n2ID != 13 && a.nav3data.file_name.Contains(search));
            //List of courses
            List<Nav2ViewModel> courses_list = new List<Nav2ViewModel>();
            //List classes
            List<Nav3ViewModel> list_classes = new List<Nav3ViewModel>();

            if (userIndustry != "3")
            {
                //List of classes
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav3data.n3ID, a.nav3data.n3_nameLong, a.nav2data.n2ID, a.nav3data.file_name, a.nav3data.n3_active, a.nav3data.n3_industry, a.nav3data.n3_usrTypes, a.nav3data.n3_products, a.nav3data.n3order }).Where(a => a.Key.n3_active == 1 && a.Key.n3_usrTypes.Contains(companyType) && a.Key.n3_industry.Contains(userIndustry)).OrderBy(a => a.Key.n3order))
                {
                    //filter by product type
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n3_products.Contains(products))
                            {
                                list_classes.Add(new Nav3ViewModel { n2ID = item.Key.n2ID, n3ID = item.Key.n3ID, n3_nameLong = item.Key.n3_nameLong, file_name = item.Key.file_name });
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                //List of classes
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav3data.n3ID, a.nav3data.n3_nameLong, a.nav2data.n2ID, a.nav3data.file_name, a.nav3data.n3_active, a.nav3data.n3_industry, a.nav3data.n3_usrTypes, a.nav3data.n3_products, a.nav3data.n3order }).Where(a => a.Key.n3_active == 1 && a.Key.n3_usrTypes.Contains(companyType)).OrderBy(a => a.Key.n3order))
                {
                    //filter by product type
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n3_products.Contains(products))
                            {
                                list_classes.Add(new Nav3ViewModel { n2ID = item.Key.n2ID, n3ID = item.Key.n3ID, n3_nameLong = item.Key.n3_nameLong, file_name = item.Key.file_name });
                                break;
                            }
                        }
                    }
                }
            }
            searchModel.list_classes = list_classes;
            count_results.classes_count = list_classes.Count().ToString();

            var riSourcesData = db.RiSourcesViewModels.Join(
                     db.Nav2ViewModel,
                     risou => risou.n2ID,
                     nav2 => nav2.n2ID,
                     (risou, nav2) => new { risou, nav2 }
                 ).Where(a => a.risou.ris_status == "1" && a.risou.ris_headline.Contains(search) || a.risou.ris_teaser.Contains(search));
            List<RiSourcesModel> risources = new List<RiSourcesModel>();

            if (userIndustry != "3")
            {
                foreach (var items in riSourcesData.Where(a => a.risou.ris_industry.Contains(userIndustry)).Where(a => a.risou.ris_usrTypes.Contains(companyType)))
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
            searchModel.risources_results = risources;
            count_results.risources_count = risources.Count().ToString();

            searchModel.salesComm_results = defaultNoChildId(salesCommunications, companyType, userIndustry, search);
            count_results.salesComm_count = searchModel.salesComm_results.Count().ToString();
            searchModel.count_results = count_results;
            searchModel.search_term = search;

            string search_model = JsonConvert.SerializeObject(new { searchModel = searchModel });

            return search_model;
        }

        private string SearchRiSources()
        {
            return "this is RiSources";
        }

        private string SearchUsers()
        {
            return "this is Users";
        }
        

    }
}
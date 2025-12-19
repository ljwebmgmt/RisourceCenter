using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using newrisourcecenter.Models;
using Newtonsoft.Json;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private RisourceCenterMexicoEntities db;

        public HomeController():this(new RisourceCenterMexicoEntities())
        {

        }

        public HomeController(RisourceCenterMexicoEntities _db)
        {
            db = _db;
        }

        [Authorize]
        public ActionResult Index()
        {
            string usr_state = "";
            string usr_country = "";
            string usr_language = "";
            long userId = Convert.ToInt64(Session["userId"]);
            long userLanguageId = Convert.ToInt64(Session["userLanguageId"]);
            long countryId = Convert.ToInt64(Session["userCountryId"]);
            var locController = new CommonController();
            var partnerProducts = db.partnerProducts;

            //Set MDF USRL
            string industry = Convert.ToString(Session["userIndustry"]);
            string usrType = Convert.ToString(Session["companyType"]);
            string products = Convert.ToString(Session["userProducts"]);
            string siteRole = "";
            int id = 3;
            if (User.IsInRole("Super Admin"))
            {
                siteRole = "1";
            }
            List<nav2> nav2 = locController.SubmenFilter(industry, usrType, products, siteRole, id);
            ViewBag.list_n2_data = nav2;
            //Links link list
            List<nav2> nav2_links = locController.SubmenFilter(industry, usrType, products, siteRole, 6);
            ViewBag.list_n2_data_links = nav2_links;
            bool interlynx = Convert.ToBoolean(Session["interlynx_user"]);
            string interlynxToken = "";
            if(interlynx)
            {
                interlynxToken = CommonController.CreateMD5("43t3cs@34&3~" + Convert.ToString(Session["userEmail"]).ToLower());
            }
            string dealInterlynx = "";
            string[] pages = Session["userPages"] != null ? Convert.ToString(Session["userPages"]).Split(',') : new string[] { };
            if (pages.Contains(ConfigurationManager.AppSettings["BidRegistrationNav"]))
            {
                dealInterlynx = CommonController.CreateMD5("43dt3cse@34a&l3~" + Convert.ToString(Session["userEmail"]).ToLower());
            }

            ViewBag.interlynxtoken = interlynxToken;
            ViewBag.dealInterlynxToken = dealInterlynx;
            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            if (!Request.IsAuthenticated || userId==0)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var email = User.Identity.GetUserName();
                var usr_data = from a in db.usr_user join b in db.partnerCompanies on a.comp_ID equals b.comp_ID join d in db.partnerLocations on a.comp_loc_ID equals d.loc_ID into e
                               from c in e.DefaultIfEmpty()
                               where (a.usr_email == email )
                               select new
                               {
                                   usr_zip=a.usr_zip,
                                   usr_add1=a.usr_add1,
                                   usr_add2=a.usr_add2,
                                   usr_city =a.usr_city,
                                   usr_phone = a.usr_phone,
                                   usr_website = a.usr_web,
                                   usr_name = a.usr_fName+ " "+ a.usr_lName,
                                   usr_fName = a.usr_fName,
                                   usr_lName = a.usr_lName,
                                   usr_title = a.usr_title,
                                   usr_email = a.usr_email,
                                   comp_ID= a.comp_ID,
                                   company_name = b.comp_name,
                                   company_logo = c.loc_logo,
                                   company_loc = c.loc_name,
                                   company_industry = b.comp_industry,
                                   company_prod = b.comp_products,
                                   loc_phone = c.loc_phone,
                                   loc_add1=c.loc_add1,
                                   loc_add2=c.loc_add2,
                                   loc_state=c.loc_state,
                                   loc_zip = c.loc_zip,
                                   loc_email = c.loc_email,
                                   company_type = b.comp_type,
                                   state = a.usr_state,
                                   country =a.usr_country,
                                   language=a.usr_language,
                                   loc_city =c.loc_city,
                                   mdf_amount = b.comp_MDF_amount,
                                   com_MDF = b.comp_MDF,
                                   usr_password=a.usr_password
                               };

                var  homeModel = new HomeViewModel();   
                foreach (var item in usr_data)
                {
                    //Get the users Country data
                    var countryData = db.countries.Where(a => a.country_id == item.country);
                    if (countryData.Count() == 0)
                    {
                        usr_country = "United States";
                    }
                    else
                    {
                        usr_country = countryData.FirstOrDefault().country_long;
                    }
                    //Get the users language data
                    var languageData = db.countries.Where(a => a.country_id==item.language);
                    if (languageData.Count()==0)
                    {
                        usr_language = "English";
                    }
                    else
                    {
                        usr_language = languageData.FirstOrDefault().Language;
                    }
                    //Get the users state data
                    if (item.loc_state != null)
                    {
                        int locstate = Convert.ToInt32(item.loc_state);
                        var loc_stateData = db.data_state;
                        string loc_state = loc_stateData.Where(a => a.stateid == locstate).FirstOrDefault().state_long;
                        //location address
                        homeModel.loc_address = item.loc_add1 + "," + item.loc_add2 + "<br />" + item.loc_city + ",<br />" + loc_state + ",<br />" + usr_country + ",<br />" + item.loc_zip + "<br />";
                        homeModel.loc_address_label = locController.GetLable("Company Address", "Home", "Index", userLanguageId);
                    }
                    //Get the users country data
                    var stateData =db.data_state.Where(a=>a.stateid==item.state);
                    if (stateData.Count() > 0)
                    {
                        usr_state = stateData.FirstOrDefault().state_long;
                    }
                    //user address
                    homeModel.usr_address = item.usr_add1 + "," + item.usr_add2 + "<br />" + item.usr_city + ",<br />" + usr_state + ",<br />" + usr_country + ",<br />" + item.usr_zip + "<br />";
                    homeModel.address_label = locController.GetLable("Address", "Home", "Index", userLanguageId);
                    homeModel.usr_language = usr_language;
                    homeModel.language_label = locController.GetLable("Language", "Home", "Index", userLanguageId);
                    homeModel.usr_full_name = item.usr_name;
                    homeModel.name_label = locController.GetLable("Name","Home","Index", userLanguageId);
                    homeModel.usr_phone = item.usr_phone;
                    homeModel.phone_label = locController.GetLable("Phone", "Home", "Index", userLanguageId);
                    homeModel.usr_website = item.usr_website;
                    homeModel.website_label = locController.GetLable("Website", "Home", "Index", userLanguageId);
                    homeModel.usr_title = item.usr_title;
                    homeModel.title_label = locController.GetLable("Title", "Home", "Index", userLanguageId);
                    homeModel.usr_email = item.usr_email;
                    homeModel.email_label = locController.GetLable("Email", "Home", "Index", userLanguageId);
                    homeModel.company_name = item.company_name;
                    homeModel.company_name_label = locController.GetLable("Company Name", "Home", "Index", userLanguageId);
                    homeModel.company_logo = item.company_logo;
                    homeModel.company_logo_label = locController.GetLable("Company Logo", "Home", "Index", userLanguageId);
                    homeModel.Location = item.company_loc;
                    homeModel.Location_label = locController.GetLable("My Location", "Home", "Index", userLanguageId);
                    homeModel.company_prod = item.company_prod;
                    homeModel.company_prod_label = locController.GetLable("Company Products", "Home", "Index", userLanguageId);
                    homeModel.loc_phone = item.loc_phone;
                    homeModel.loc_phone_label = locController.GetLable("Company Phone", "Home", "Index", userLanguageId);
                    homeModel.loc_email = item.loc_email;
                    homeModel.loc_email_label = locController.GetLable("Company Email", "Home", "Index", userLanguageId);
                    homeModel.company_type = item.company_type;
                    homeModel.company_type_label = locController.GetLable("Company Type", "Home", "Index", userLanguageId);
                    homeModel.company_industry = item.company_industry;
                    homeModel.company_industry_label = locController.GetLable("Company Industry", "Home", "Index", userLanguageId);
                    homeModel.personal_info_heading = locController.GetLable("My Personal Information", "Home", "Index", userLanguageId);
                    homeModel.company_info_heading = locController.GetLable("My Company Information", "Home", "Index", userLanguageId);
                    homeModel.price_availability_heading = locController.GetLable("My Price and Availability", "Home", "Index", userLanguageId);
                    homeModel.edit_label = locController.GetLable("Edit", "Home", "Index", userLanguageId);
                    homeModel.comp_MDF = item.com_MDF;
                    homeModel.mdf_Amount = item.mdf_amount;
                    homeModel.password = item.usr_password;
                    homeModel.usr_fName = item.usr_fName;
                    homeModel.usr_lName = item.usr_lName;
                    homeModel.list_products = list_products;
                    homeModel.comp_ID = item.comp_ID;
                    homeModel.usr_add1 = item.usr_add1;
                    homeModel.usr_city = item.usr_city;
                    homeModel.usr_zip = item.usr_zip;
                    homeModel.usr_state = usr_state;
                }

                return View(homeModel);
            }
        }

        public ActionResult Stockcheck()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            ViewBag.userId = Convert.ToInt64(Session["userId"]);
            ViewBag.companyId = Convert.ToInt64(Session["companyId"]);
            ViewBag.fullname = Convert.ToString(Session["firstName"])+" "+Convert.ToString(Session["lastName"]);
            
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (Session["userId"] == null)
            {
                return RedirectToAction("login", "Account");
            }

            int usr_id = int.Parse("" + Session["userId"] + "");
            Dictionary<int, string> companyIndustries = db.partnerIndustries.ToDictionary(x => x.pi_ID, x => x.pi_industry);
            Dictionary<int, string> companyAudiences = db.partnerTypes.ToDictionary(x => x.pt_ID, x => x.pt_type);
            var usr_data = from a in db.partnerStockChecks
                           join b in db.partnerLocations on a.ps_account equals b.loc_SAP_account
                           join ds in db.data_state on b.loc_state equals ds.stateid.ToString()
                           join c in db.partnerCompanies on b.comp_ID equals c.comp_ID
                           join d in db.usr_user on a.usr_user equals d.usr_ID
                           where (a.usr_user == usr_id) //(a.usr_user == usr_id) test cases usr_ID = 18127,usr_ID = 3 has different passwords,usr_ID = 1 has no sap active accounts
                           select new
                           {
                               ps_account = a.ps_account,
                               usr_user = a.usr_user,
                               ps_ID = a.ps_ID,
                               comp_name = c.comp_name,
                               sap_password = b.loc_SAP_password,
                               loc_city = b.loc_city,
                               loc_state = ds.state_long,
                               comp_industry = c.comp_industry,
                               comp_type = c.comp_type
                           };

            if (usr_data.Distinct().Count() == 0)
            {
                ViewBag.Status = "The SAP account can not be accessed";
                return View();
            }

            List<StockChecks> stockPrice = new List<StockChecks>();
            foreach (var sap_user in usr_data)
            {
                if (sap_user.ps_account != null && sap_user.sap_password != null)
                {
                    stockPrice.Add(new StockChecks
                    {
                        usr_user = sap_user.usr_user,
                        ps_account = sap_user.ps_account,
                        ps_ID = sap_user.ps_ID,
                        comp_name = sap_user.comp_name,
                        sap_password = sap_user.sap_password,
                        loc_city = sap_user.loc_city,
                        loc_state = sap_user.loc_state
                    });
                }
            }

            List<StockChecks> Distinct_stockPrice = new List<StockChecks>();
            foreach (var sap_user in usr_data.GroupBy(group => new { group.ps_account, group.sap_password, group.ps_ID, group.comp_name, group.usr_user, group.comp_industry, group.comp_type }).OrderByDescending(a => a.Key.ps_account))
            {
                if (sap_user.Key.ps_account != null && sap_user.Key.sap_password != null)
                {
                    Distinct_stockPrice.Add(new StockChecks
                    {
                        usr_user = sap_user.Key.usr_user,
                        ps_account = sap_user.Key.ps_account,
                        ps_ID = sap_user.Key.ps_ID,
                        comp_name = sap_user.Key.comp_name,
                        sap_password = sap_user.Key.sap_password,
                        comp_industry = (companyIndustries.ContainsKey(sap_user.Key.comp_industry.Value) ? companyIndustries[sap_user.Key.comp_industry.Value] : ""),
                        comp_audience = (companyAudiences.ContainsKey(sap_user.Key.comp_type.Value) ? companyAudiences[sap_user.Key.comp_type.Value] : "")
                    });
                }
            }

            var ListStockChecks = new ListStockChecks
            {
                Distinct_stockPrice = Distinct_stockPrice,
                stockPrice = stockPrice
            };

            ViewBag.stockPrice = Distinct_stockPrice;

            return View(ListStockChecks);
        }

        public ActionResult StockcheckAll()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            ViewBag.userId = Convert.ToInt64(Session["userId"]);
            ViewBag.companyId = Convert.ToInt64(Session["companyId"]);
            ViewBag.fullname = Convert.ToString(Session["firstName"]) + " " + Convert.ToString(Session["lastName"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (Session["userId"] == null)
            {
                return RedirectToAction("login", "Account");
            }

            int usr_id = int.Parse("" + Session["userId"] + "");

            var usr_data = from a in db.partnerStockChecks
                           join b in db.partnerLocations on a.ps_account equals b.loc_SAP_account
                           join c in db.partnerCompanies on b.comp_ID equals c.comp_ID
                           join d in db.usr_user on a.usr_user equals d.usr_ID
                           select new
                           {
                               ps_account = a.ps_account,
                               usr_user = a.usr_user,
                               usr_fullName = d.usr_fName+" "+d.usr_lName,
                               ps_ID = a.ps_ID,
                               comp_name = c.comp_name,
                               sap_password = b.loc_SAP_password,
                               loc_city = b.loc_city
                           };

            if (usr_data.Distinct().Count() == 0)
            {
                ViewBag.Status = "The SAP account cannot be accessed";
                return View();
            }

            List<StockChecks> Distinct_stockPrice = new List<StockChecks>();

            foreach (var sap_user in usr_data.GroupBy(group => new { group.ps_account, group.sap_password, group.ps_ID, group.comp_name, group.usr_user, group.loc_city,group.usr_fullName }).OrderByDescending(a => a.Key.ps_account))
            {
                if (sap_user.Key.ps_account != null && sap_user.Key.sap_password != null)
                {
                    Distinct_stockPrice.Add(new StockChecks
                    {
                        usr_user = sap_user.Key.usr_user,
                        ps_account = sap_user.Key.ps_account,
                        ps_ID = sap_user.Key.ps_ID,
                        comp_name = sap_user.Key.comp_name,
                        sap_password = sap_user.Key.sap_password,
                        loc_city = sap_user.Key.loc_city,
                        usr_fullName = sap_user.Key.usr_fullName
                    });
                }
            }

            var ListStockChecks = new ListStockChecks
            {
                Distinct_stockPrice = Distinct_stockPrice,
            };

            ViewBag.stockPrice = Distinct_stockPrice;

            return View(ListStockChecks);
        }

        public ActionResult Webshop()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            ViewBag.userId = Convert.ToInt64(Session["userId"]);
            ViewBag.companyId = Convert.ToInt64(Session["companyId"]);
            ViewBag.fullname = Convert.ToString(Session["firstName"]) + " " + Convert.ToString(Session["lastName"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (Session["userId"] == null)
            {
                return RedirectToAction("login", "Account");
            }

            int usr_id = int.Parse("" + Session["userId"] + "");
            Dictionary<int, string> companyIndustries = db.partnerIndustries.ToDictionary(x => x.pi_ID, x => x.pi_industry);
            Dictionary<int, string> companyAudiences = db.partnerTypes.ToDictionary(x => x.pt_ID, x => x.pt_type);
            var usr_data = from a in db.Webshop_connect
                           join b in db.partnerLocations on a.loc_id equals b.loc_ID
                           join ds in db.data_state on b.loc_state equals ds.stateid.ToString()
                           join c in db.partnerCompanies on b.comp_ID equals c.comp_ID
                           join d in db.usr_user on a.usr_user equals d.usr_ID
                           where (a.usr_user == usr_id) //(a.usr_user == usr_id) test cases usr_ID = 18127,usr_ID = 3 has different passwords,usr_ID = 1 has no sap active accounts
                           select new
                           {
                               ws_account = a.ws_account,
                               ws_user = b.loc_Webshop_account,
                               usr_user = a.usr_user,
                               ws_ID = a.ws_ID,
                               comp_name = c.comp_name,
                               ws_password = b.loc_Webshop_password,
                               loc_city = b.loc_city,
                               sap_account = b.loc_SAP_account,
                               loc_state = ds.state_long,
                               comp_industry = c.comp_industry,
                               comp_type = c.comp_type
                           };

            if (usr_data.Distinct().Count() == 0)
            {
                ViewBag.Status = "The Online Shop account cannot be accessed";
                return View();
            }

            List<Webshops> stockPrice = new List<Webshops>();
            foreach (var ws_data in usr_data)
            {
                if (ws_data.ws_account != null && ws_data.ws_password != null)
                {
                    stockPrice.Add(new Webshops
                    {
                        usr_user = ws_data.usr_user,
                        ws_account = ws_data.ws_account,
                        ws_user = ws_data.ws_user,
                        ws_ID = ws_data.ws_ID,
                        comp_name = ws_data.comp_name,
                        ws_password = ws_data.ws_password,
                        loc_city = ws_data.loc_city,
                        sap_account = ws_data.sap_account,
                        loc_state = ws_data.loc_state
                    });
                }
            }

            List<Webshops> Distinct_stockPrice = new List<Webshops>();
            foreach (var ws_user in usr_data.GroupBy(group => new { group.ws_account, group.ws_user, group.ws_password, group.ws_ID, group.comp_name, group.usr_user, group.loc_city, group.sap_account, group.comp_industry, group.comp_type }).OrderByDescending(a => a.Key.ws_account))
            {
                if (ws_user.Key.ws_account != null && ws_user.Key.ws_password != null)
                {
                    Distinct_stockPrice.Add(new Webshops
                    {
                        usr_user = ws_user.Key.usr_user,
                        ws_account = ws_user.Key.ws_account,
                        ws_ID = ws_user.Key.ws_ID,
                        ws_user = ws_user.Key.ws_user,
                        comp_name = ws_user.Key.comp_name,
                        ws_password = ws_user.Key.ws_password,
                        loc_city = ws_user.Key.loc_city,
                        sap_account = ws_user.Key.sap_account,
                        comp_industry = (companyIndustries.ContainsKey(ws_user.Key.comp_industry.Value) ? companyIndustries[ws_user.Key.comp_industry.Value] : ""),
                        comp_audience = (companyAudiences.ContainsKey(ws_user.Key.comp_type.Value) ? companyAudiences[ws_user.Key.comp_type.Value] : "")
                    });
                }
            }

            var ListWebshops = new ListWebshops
            {
                Distinct_stockPrice = Distinct_stockPrice,
                stockPrice = stockPrice
            };

            ViewBag.stockPrice = Distinct_stockPrice;

            return View(ListWebshops);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
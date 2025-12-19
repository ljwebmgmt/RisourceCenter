using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using iTextSharp.text.pdf.qrcode;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using newrisourcecenter.Models;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using static iTextSharp.text.pdf.AcroFields;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
        CommonController commCtl = new CommonController();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> PreRegister()
        {
            UserViewModel preRegister = new UserViewModel();
            var usr_data = db.usr_user;

           // int i = 0;
            foreach (var data in usr_data)
            {
                preRegister.usr_email = data.usr_email;
                preRegister.usr_password = data.usr_password;
                if (data.usr_status==1 )
                {
                    preRegister.usr_statuss = true;
                }
                else
                {
                    preRegister.usr_statuss = false;
                }

                var user = new ApplicationUser { UserName = preRegister.usr_email, Email = preRegister.usr_email,EmailConfirmed=preRegister.usr_statuss };
                var result = await UserManager.CreateAsync(user, preRegister.usr_password);

                var the_user = db.usr_user.Where(a => a.usr_email == preRegister.usr_email);
                if (result.Succeeded)
                {
                    foreach(var item in the_user)
                    {
                        var compa = db.partnerLocations.Where(a=>a.comp_ID==item.comp_ID);

                        item.system_ID = user.Id;
                        item.usr_country = 228;
                        item.usr_language = 228;
                        item.admin_theme = 1;
                        if (compa.Count() == 0)
                        {
                            item.comp_loc_ID = 909;
                        }
                        else
                        {
                            item.comp_loc_ID = compa.FirstOrDefault().loc_ID;
                        }
                    }
                }
                /*
                i++;
                if (i==8825)
                {
                    break;
                }
                */
            }

            db.SaveChanges();

            return View();
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //Uncomment this line for maintenance
            //return RedirectToAction("Maintenance", "Manage");
            ViewBag.Title = "Rittal RiSource Center";
            ViewBag.ReturnUrl = returnUrl;
            commCtl.FileLog("Logging In");
            
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                Session.Clear();
                return RedirectToAction("Index", "Home",new { n1_name="Home"});
            }
            Session["IsPartnerPortal"] = false;
            return View();
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult LoginPartnerPortal(string username, string returnUrl)
        {
            //Uncomment this line for maintenance
            //return RedirectToAction("Maintenance", "Manage");
            ViewBag.Title = "Rittal RiSource Center";
            
            ViewBag.username = username;
            commCtl.FileLog("Logging In");
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                Session.Clear();
                return RedirectToAction("Index", "Home", new { n1_name = "Home" });
            }
            Session["IsPartnerPortal"] = true;
            return View();
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Maintenance(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                Session.Clear();
                return RedirectToAction("Index", "Manage");
            }

            ViewBag.Title = "Rittal RiSource Center";
            ViewBag.ReturnUrl = returnUrl;
            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginPartnerPortal(LoginPartnerPortalViewModel loginModel, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }
            var url = "https://www.rittal.com/de_de/partner-portal/en/default/api/auth";
            HttpClient client = new HttpClient();

            LoginViewModel model = new LoginViewModel();
           // string apiData = "{'username': '"+loginModel.Username+"','Password':'"+ loginModel.Password + "'}";
            //HttpContent content = new StringContent(apiData, UTF8Encoding.UTF8, "application/json");

            HttpContent content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("username",loginModel.Username),
                new KeyValuePair<string, string>("password",loginModel.Password),
            });
            using (HttpResponseMessage response = client.PostAsync(url, content).Result)
            {
                //HttpResponseMessage response = await client.PostAsJsonAsync(url, loginModel);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError("", "Invalid username or password. Please try again.");
                    return View(loginModel);
                }
                if (response.IsSuccessStatusCode)
                {
                    string apiResult = response.Content.ReadAsStringAsync().Result;//.ReadAsAsync<Dictionary<string, dynamic>>();
                    Dictionary<string,string> partnerPortalUser = JObject.Parse(apiResult).ToObject<Dictionary<string,string>>();
                    Session["PortalUser"] = partnerPortalUser;
                    model.Email = partnerPortalUser["email"];
                    model.Password = loginModel.Password;
                    model.RememberMe = loginModel.RememberMe;

                }
                var data = db.AspNetUsers.Where(m => m.Email == model.Email);
                //Set the user session variables
                var usr = from usr_data in db.usr_user where (usr_data.usr_email == model.Email) || (usr_data.pp_username == loginModel.Username) select new { usr_data };
                var userData = usr.FirstOrDefault();
                if (userData == null)
                {
                    ModelState.AddModelError("", "Your account does not exits in Rittal RiSource Center. Please register.");
                    return RedirectToLocal("/Account/RegisterTemp");

                }
                // var usrContext = new usr_user();
                // var current_user = db.usr_user.Where(u => u.usr_email ==)
                var company = db.partnerCompanies.Where(a => a.comp_ID == userData.usr_data.comp_ID).FirstOrDefault();

                if (company == null)
                {
                    ModelState.AddModelError("", "Your account or company account is not activated. Please contact the system administrator.");
                    return View(loginModel);
                }

                if (company.comp_active == 0)
                {
                    ModelState.AddModelError("", "Your company is not activated yet. Please contact the system administrator.");
                    return View(loginModel);
                }

                foreach (var item in data)
                {
                    if (!item.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Your account has not been activated yet. Please contact the system administrator.");
                        return View(loginModel);
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(userData.usr_data.usr_email, userData.usr_data.usr_password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:

                        //var usr = from usr_data in db.usr_user where usr_data.usr_email == model.Email select new {  usr_data };
                        //set the host name
                        if (Request.Url.Port != 443)
                        {
                            Session["host"] = Request.Url.Host + ":" + Request.Url.Port;
                        }
                        else
                        {
                            Session["host"] = Request.Url.Host;
                        }
                        Session["userName"] = User.Identity.GetUserName();//Get the system username
                        
                        var item = userData;
                        //Temp data for coldfusion sites 
                        Response.Cookies["RISOURCEUSRID"].Value = item.usr_data.usr_ID.ToString();
                        Response.Cookies["RISOURCESITEROLE"].Value = item.usr_data.usr_role.ToString();
                        Response.Cookies["RISOURCEUSERNAME"].Value = HttpUtility.UrlEncode(item.usr_data.usr_email);
                        Response.Cookies["FULLNAME"].Value = item.usr_data.usr_fName.ToString() + " " + item.usr_data.usr_lName.ToString();
                        Response.Cookies["RISOURCEUSERROLE"].Value = item.usr_data.usr_role.ToString();

                        //User session variables
                        Session["firstName"] = item.usr_data.usr_fName;
                        Session["lastName"] = item.usr_data.usr_lName;
                        Session["userId"] = item.usr_data.usr_ID;
                        Session["userPassword"] = item.usr_data.usr_password;
                        Session["userRole"] = item.usr_data.usr_role;
                        Session["system_ID"] = item.usr_data.system_ID;
                        Session["userPages"] = item.usr_data.usr_pages;
                        Session["userCountryId"] = item.usr_data.usr_country;
                        Session["userLanguageId"] = item.usr_data.usr_language;
                        Session["phone"] = item.usr_data.usr_phone;
                        Session["userStateId"] = item.usr_data.usr_state;
                        Session["comp_MDF"] = item.usr_data.usr_MDF;
                        Session["usr_SPA"] = item.usr_data.usr_SPA;
                        Session["usr_POS"] = item.usr_data.usr_POS;
                        Session["city"] = item.usr_data.usr_city;
                        Session["zip"] = item.usr_data.usr_zip;
                        Session["add1"] = item.usr_data.usr_add1;
                        Session["userEmail"] = item.usr_data.usr_email;
                        //Get the users country data
                        if (item.usr_data.usr_state != null)
                        {
                            var stateData = db.data_state.Where(a => a.stateid == item.usr_data.usr_state);
                            if (stateData.Count() > 0)
                            {
                                Session["state"] = stateData.FirstOrDefault().state_long;
                            }
                        }

                        //Get the rest of the session variables from the partnerCompany table
                        if (!String.IsNullOrEmpty(item.usr_data.comp_ID.ToString()))
                        {
                            //Temp data for coldfusion sites 
                            Response.Cookies["RISOURCECOMPANYID"].Value = company.comp_ID.ToString();
                            Response.Cookies["RISOURCEINDUSTRY"].Value = company.comp_industry.ToString();
                            Response.Cookies["RISOURCEPRODUCTS"].Value = company.comp_products != null ? company.comp_products.ToString(): "";
                            //Response.Cookies["RISOURCEINDUSTRY"].Value = company.comp_industry.ToString();

                            Session["companyId"] = company.comp_ID;
                            Session["userIndustry"] = company.comp_industry;
                            Session["userProducts"] = company.comp_products;
                            Session["companyType"] = company.comp_type;
                            Session["comp_SPA"] = company.comp_SPA;
                            Session["comp_POS"] = company.comp_POS;
                            Session["comp_region"] = company.comp_region;
                            Session["comp_name"] = company.comp_name;
                            var location = db.partnerLocations.Where(a => a.loc_ID == item.usr_data.comp_loc_ID);//Get company data
                            Session["location_name"] = location.FirstOrDefault() != null ? location.FirstOrDefault().loc_name : "";
                        }

                        await CountLoggedin("loggedin");//Add users to the loggin list

                        return RedirectToLocal("/Home");

                    //return Redirect("~/Home?n1_name=Home");

                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(loginModel);
                }
            }

        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //else if (!ModelState.IsValid)
            //{
            //    var locController = new CommonController();
            //    //check model state errors
            //    var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            //    locController.emailErrors(message);//send errors by email
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            //}

            //string sqls = "SELECT * FROM AspNetUsers WHERE Email ='antwi.s@rittal.us'";
            var data = db.AspNetUsers.Where(m => m.Email == model.Email);
            //Set the user session variables
            var usr = from usr_data in db.usr_user where usr_data.usr_email == model.Email && !usr_data.deleted && !usr_data.inactive select new { usr_data };
           // var usrContext = new usr_user();
           // var current_user = db.usr_user.Where(u => u.usr_email ==)
            var company = db.partnerCompanies.Where(a => a.comp_ID == usr.FirstOrDefault().usr_data.comp_ID).FirstOrDefault();

            if (usr == null)
            {
                ModelState.AddModelError("", "Your account does not exist or is disabled. Please contact the system administrator.");
                return View(model);
            }

            if (company == null)
            {
                ModelState.AddModelError("", "Your account or company account is not activated. Please contact the system administrator.");
                return View(model);
            }

            if (company.comp_active == 0)
            {
                ModelState.AddModelError("", "Your company is not activated yet. Please contact the system administrator.");
                return View(model);
            }

            foreach (var item in data)
            {
                if (!item.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Your account has not been activated yet. Please contact the system administrator.");
                    return View(model);
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            bool loginSuccess = false;
            switch (result)
            {
                case SignInStatus.Success:

                    //var usr = from usr_data in db.usr_user where usr_data.usr_email == model.Email select new {  usr_data };
                    //set the host name
                    if (Request.Url.Port != 443)
                    {
                        Session["host"] = Request.Url.Host + ":" + Request.Url.Port;
                    }
                    else
                    {
                        Session["host"] = Request.Url.Host;
                    }
                    Session["userName"] = User.Identity.GetUserName();//Get the system username
                                      
                    foreach (var item in usr)
                    {
                        //Temp data for coldfusion sites 
                        Response.Cookies["RISOURCEUSRID"].Value = item.usr_data.usr_ID.ToString();
                        Response.Cookies["RISOURCESITEROLE"].Value = item.usr_data.usr_role.ToString();
                        Response.Cookies["RISOURCEUSERNAME"].Value = HttpUtility.UrlEncode(item.usr_data.usr_email);
                        Response.Cookies["FULLNAME"].Value = item.usr_data.usr_fName.ToString()+" "+ item.usr_data.usr_lName.ToString();
                        Response.Cookies["RISOURCEUSERROLE"].Value = item.usr_data.usr_role.ToString();

                        //User session variables
                        Session["firstName"] = item.usr_data.usr_fName;
                        Session["lastName"] = item.usr_data.usr_lName;
                        Session["userId"] = item.usr_data.usr_ID;
                        Session["userPassword"] = item.usr_data.usr_password;
                        Session["userRole"] = item.usr_data.usr_role;
                        Session["system_ID"] = item.usr_data.system_ID;
                        Session["userPages"] = item.usr_data.usr_pages;
                        Session["userCountryId"] = item.usr_data.usr_country;
                        Session["userLanguageId"] = item.usr_data.usr_language;
                        Session["phone"] = item.usr_data.usr_phone;
                        Session["userStateId"] = item.usr_data.usr_state;
                        Session["comp_MDF"] = item.usr_data.usr_MDF;
                        Session["usr_SPA"] = item.usr_data.usr_SPA;
                        Session["usr_POS"] = item.usr_data.usr_POS;
                        Session["city"] = item.usr_data.usr_city;
                        Session["zip"] = item.usr_data.usr_zip;
                        Session["add1"] = item.usr_data.usr_add1;
                        Session["interlynx_user"] = item.usr_data.interlynx_user.HasValue ? item.usr_data.interlynx_user.Value : false;
                        Session["userEmail"] = item.usr_data.usr_email;
                        var companyType = db.partnerTypes.Where(x => x.pt_ID == company.comp_type).FirstOrDefault();
                        bool isPinnacle = companyType != null && companyType.pt_type.ToLower().Contains("pinnacle automation");
                        Session["isPinnacleUser"] = isPinnacle;
                        if (isPinnacle)
                        {
                            returnUrl = "~/MDFViewPinnacle";
                        }
                        //Get the users country data
                        if (item.usr_data.usr_state!=null) {
                            var stateData = db.data_state.Where(a => a.stateid == item.usr_data.usr_state);
                            if (stateData.Count() > 0)
                            {
                                Session["state"] = stateData.FirstOrDefault().state_long;
                            }
                        }

                        //Get the rest of the session variables from the partnerCompany table
                        if (!String.IsNullOrEmpty(item.usr_data.comp_ID.ToString()))
                        {
                            //Temp data for coldfusion sites 
                            Response.Cookies["RISOURCECOMPANYID"].Value = company.comp_ID.ToString();
                            Response.Cookies["RISOURCEINDUSTRY"].Value = company.comp_industry.ToString();
                            Response.Cookies["RISOURCEPRODUCTS"].Value = company.comp_products!=null?company.comp_products.ToString():"";
                            //Response.Cookies["RISOURCEINDUSTRY"].Value = company.comp_industry.ToString();

                            Session["companyId"]    = company.comp_ID;
                            Session["userIndustry"] = company.comp_industry;
                            Session["userProducts"] = company.comp_products;
                            Session["companyType"] = company.comp_type;
                            Session["comp_SPA"] = company.comp_SPA;
                            Session["comp_POS"] = company.comp_POS;
                            Session["comp_region"] = company.comp_region;
                            Session["comp_name"] = company.comp_name;
                            var location = db.partnerLocations.Where(a => a.loc_ID == item.usr_data.comp_loc_ID);//Get company data
                            Session["location_name"] = location.FirstOrDefault() != null ? location.FirstOrDefault().loc_name : "";
                        }
                    }
                    await updateUserLoggedIn(usr.First().usr_data);
                    await CountLoggedin("loggedin");//Add users to the loggin list

                    /*if (Request.Url.Host != "10.38.0.114")
                    {
                        return RedirectToAction("loginauto.cfm", "account",new { username = ""+model.Email+"" });
                    }
                    else
                    {
                        return Redirect("~/Home?n1_name=Home");
                    }
                    */

                    //var url = returnUrl;
                    return RedirectToLocal(returnUrl);

                    //return Redirect("~/Home?n1_name=Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public async Task updateUserLoggedIn(usr_user user)
        {
            user.usr_lastLogin = DateTime.Now;
            user.last_warning = null;
            var usrContext = db.Entry(user);
            usrContext.Property(p => p.usr_lastLogin).IsModified = true;
            usrContext.Property(p => p.last_warning).IsModified = true;
            await db.SaveChangesAsync();
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult GetLocation(int comp_id=0)
        {
            var loc_data = db.partnerLocations.Where(a=>a.comp_ID==comp_id);
            List<partnerLocation> loc = new List<partnerLocation>();
            foreach(var item in loc_data)
            {
                loc.Add(new partnerLocation { loc_ID = item.loc_ID,loc_city=item.loc_city,loc_name=item.loc_city});
            }
            return View(loc);
        }

        // GET: /Account/AddUser
        [AllowAnonymous]
        public ActionResult AddUser()
        {
            ViewBag.Title = "Request an account";

            //select the company list from the partnerCompanies database
            var companies = from list in db.partnerCompanies where list.comp_active != 0 orderby list.comp_name ascending select list;
            //Add the data to the comp_listing object
            List<SelectListItem> comp_listing = new List<SelectListItem>();
            comp_listing.Add(new SelectListItem { Text = "Select your Company", Value = "select", Selected = true });//default value for select dropdown
            List<long> companyIds = new List<long>();
            foreach (var item in companies)//iterate the add function
            {
                comp_listing.Add(new SelectListItem { Text = item.comp_name, Value = item.comp_ID.ToString() });
                if(item.comp_type == 3 || item.comp_name.ToLower() == "rittal united states")
                {
                    companyIds.Add(item.comp_ID);
                }
            }

            //select the states data from the data_stat table
            var states = from state in db.data_state where state.state_country == "US" || state.state_country == "CA" orderby state.state_abbr ascending select state;
            //Add the data to the list_state object
            List<SelectListItem> list_states = new List<SelectListItem>();
            list_states.Add(new SelectListItem { Text = "Select your State", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in states)//iterate the add function
            {
                list_states.Add(new SelectListItem { Text = item.state_long, Value = item.stateid.ToString() });
            }

            //select countries from the countries table
            var countries = db.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            //Add the data to the list_countries list object
            List<SelectListItem> list_countries = new List<SelectListItem>();
            list_countries.Add(new SelectListItem { Text = "Select your Country", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in countries.OrderBy(a => a.country_id))//iterate the add function
            {
                if (item.country_id != 242)
                {
                    list_countries.Add(new SelectListItem { Text = item.country_long, Value = item.country_id.ToString() });
                }
            }

            //select language from the countries table
            var language = db.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            //Add the data to the list_languages list object
            List<SelectListItem> list_languages = new List<SelectListItem>();
            list_languages.Add(new SelectListItem { Text = "Select your Language", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in language.OrderBy(a => a.Language))//iterate the add function
            {
                if (item.country_id != 38)
                {
                    list_languages.Add(new SelectListItem { Text = item.Language, Value = item.country_id.ToString() });
                }
            }

            var approvers = from list in db.RegionApprovers orderby list.name ascending select list;
            List<SelectListItem> approver_listing = new List<SelectListItem>();
            approver_listing.Add(new SelectListItem { Text = "Select your Approver", Value = "", Selected = true });//default value for select dropdown
            foreach (var item in approvers)//iterate the add function
            {
                approver_listing.Add(new SelectListItem { Text = item.name, Value = item.code });
            }
            approver_listing.Add(new SelectListItem { Text = "None of the above", Value = "" });

            /*
            Instantiate the RegisterViewModel and assign the list of companies 
            from the database to the RegisterViewModel field company_listings
            */
            var RegisterViewModelData = new RegisterViewModel
            {
                Company_listings = comp_listing,
                List_states = list_states,
                List_countries = list_countries,
                List_languages = list_languages,
                Approver_listings = approver_listing
            };
            ViewBag.approverCompanyIds = companyIds;
            return View(RegisterViewModelData);//pass the data to the Register View
        }

        // GET: /Account/RegisterTemp
        [AllowAnonymous]
        public ActionResult RegisterTemp()
        {
            ViewBag.Title = "Request an account";

            //select the company list from the partnerCompanies database
            var companies = from list in db.partnerCompanies where list.comp_active != 0 orderby list.comp_name ascending select list;
            //Add the data to the comp_listing object
            List<SelectListItem> comp_listing = new List<SelectListItem>();
            comp_listing.Add(new SelectListItem { Text = "Select your Company", Value = "select", Selected = true });//default value for select dropdown
            List<long> companyIds = new List<long>();
            foreach (var item in companies)//iterate the add function
            {
                comp_listing.Add(new SelectListItem { Text = item.comp_name, Value = item.comp_ID.ToString() });
                if (item.comp_type == 3 || item.comp_name.ToLower() == "rittal united states")
                {
                    companyIds.Add(item.comp_ID);
                }
            }

            //select the states data from the data_stat table
            var states = from state in db.data_state where state.state_country == "US" || state.state_country == "CA" orderby state.state_abbr ascending select state;
            //Add the data to the list_state object
            List<SelectListItem> list_states = new List<SelectListItem>();
            list_states.Add(new SelectListItem { Text = "Select your State", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in states)//iterate the add function
            {
                list_states.Add(new SelectListItem { Text = item.state_long, Value = item.stateid.ToString() });
            }

            //select countries from the countries table
            var countries = db.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            //Add the data to the list_countries list object
            List<SelectListItem> list_countries = new List<SelectListItem>();
            list_countries.Add(new SelectListItem { Text = "Select your Country", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in countries.OrderBy(a => a.country_id))//iterate the add function
            {
                if (item.country_id != 242)
                {
                    list_countries.Add(new SelectListItem { Text = item.country_long, Value = item.country_id.ToString() });
                }
            }

            //select language from the countries table
            var language = db.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            //Add the data to the list_languages list object
            List<SelectListItem> list_languages = new List<SelectListItem>();
            list_languages.Add(new SelectListItem { Text = "Select your Language", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in language.OrderBy(a => a.Language))//iterate the add function
            {
                if (item.country_id != 38)
                {
                    list_languages.Add(new SelectListItem { Text = item.Language, Value = item.country_id.ToString() });
                }
            }

            var approvers = from list in db.RegionApprovers orderby list.name ascending select list;
            List<SelectListItem> approver_listing = new List<SelectListItem>();
            approver_listing.Add(new SelectListItem { Text = "Select your Approver", Value = "", Selected = true });//default value for select dropdown
            foreach (var item in approvers)//iterate the add function
            {
                approver_listing.Add(new SelectListItem { Text = item.name, Value = item.code });
            }
            approver_listing.Add(new SelectListItem { Text = "None of the above", Value = "" });

            /*
            Instantiate the RegisterViewModel and assign the list of companies 
            from the database to the RegisterViewModel field company_listings
            */
            var RegisterViewModelData = new RegisterViewModel
            {
                Company_listings = comp_listing,
                List_states = list_states,
                List_countries = list_countries,
                List_languages = list_languages,
                Approver_listings = approver_listing
            };

            if(Session.Count > 0 && Session["PortalUser"] != null)
            {
                var PortalUser = Session["PortalUser"] as Dictionary<string,string>;
                RegisterViewModelData.FirstName = PortalUser["contactFirstname"];
                RegisterViewModelData.LastName = PortalUser["contactLastname"];
                RegisterViewModelData.Address1 = PortalUser["address"];
                RegisterViewModelData.City = PortalUser["city"];
                var country = RegisterViewModelData.List_countries.Where(x => x.Text == PortalUser["country"]).FirstOrDefault();
                RegisterViewModelData.Country = Convert.ToInt32(country!=null?country.Value:"0");
                RegisterViewModelData.Zip = PortalUser["zipcode"];
                RegisterViewModelData.Email = PortalUser["email"];
                RegisterViewModelData.Phone = PortalUser["phone"];
                RegisterViewModelData.Username = PortalUser["username"];
            }
            ViewBag.approverCompanyIds = companyIds;
            return View(RegisterViewModelData);//pass the data to the Register View
        }

        // POST: /Account/RegisterTemp
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTemp(RegisterViewModel model)
        {
            try
            {
                var checkuser_temp = db.usr_user_temp.Where(a => a.usr_email == model.Email);
                var checkuser = db.usr_user.Where(a => a.usr_email == model.Email);
                if (checkuser.Count() != 0 || checkuser_temp.Count() != 0 || UserManager.FindByEmail(model.Email) != null)
                {
                    return RedirectToAction("RegisterTemp", new { msg = "The email address " + model.Email + " already exist" });
                }
                if (ModelState.IsValid)
                {
                    //determine the link
                    string link = "";
                    if (Request.Url.Port != 443)
                    {
                        link = "http://" + Request.Url.Host + ":" + Request.Url.Port;
                    }
                    else
                    {
                        link = "https://" + Request.Url.Host;
                    }

                    //Get state data
                    var state = db.data_state.Where(a => a.stateid == model.State);//Get state data
                    var stateName = state.FirstOrDefault().state_long;
                    //Get country data
                    var country = db.countries.Where(a => a.country_id == model.Country);//Get country data
                    var countryName = country.FirstOrDefault().country_long;
                    int Company = Convert.ToInt32(model.Company);
                    //Get company data
                    var company = db.partnerCompanies.Where(a => a.comp_ID == Company);//Get company data
                    var compName = company.FirstOrDefault().comp_name;
                    //Get Location data
                    var location = db.partnerLocations.Where(a => a.loc_ID == model.Comp_loc_ID);//Get company data
                    var locName = location.FirstOrDefault().loc_name;

                    usr_user_temp UserData = new usr_user_temp(); //Instantiate the usr_details model
                                                                  //Set the field values for usr_details model to be inserted into the usr_details model
                    UserData.usr_fName = model.FirstName;
                    UserData.usr_lName = model.LastName;
                    UserData.usr_email = model.Email;
                    UserData.usr_add1 = model.Address1;
                    UserData.usr_add2 = model.Address2;
                    UserData.usr_city = model.City;
                    UserData.usr_state = model.State;
                    UserData.usr_country = model.Country;
                    UserData.usr_language = model.Language;
                    UserData.usr_zip = model.Zip;
                    UserData.usr_phone = model.Phone;
                    UserData.usr_fax = model.Fax;
                    UserData.usr_web = model.Website;
                    UserData.comp_ID = Convert.ToInt32(model.Company);
                    UserData.comp_loc_ID = model.Comp_loc_ID;
                    UserData.sap_numb = model.Sap_numb;
                    var wN = " ";

                    if (model.Newsletter)
                    {
                        UserData.wN = 1;
                        wN = "Yes";
                    }
                    else
                    {
                        UserData.wN = 0;
                        wN = "Yes";
                    }
                    UserData.usr_password = model.Password;
                    UserData.usr_title = model.Title;
                    UserData.usr_dateCreated = DateTime.Now;
                    UserData.usr_role = 1;
                    UserData.usr_SAP = 0;
                    UserData.usr_POS = 0;
                    UserData.usr_SPA = 0;
                    UserData.usr_MDF = 0;
                    if (model.Usr_SAP)
                    {
                        UserData.usr_SAP = 1;
                    }
                    if (model.Usr_POS)
                    {
                        UserData.usr_POS = 1;
                    }
                    if (model.Usr_SPA)
                    {
                        UserData.usr_SPA = 1;
                    }
                    if (model.Usr_MDF)
                    {
                        UserData.usr_MDF = 1;
                    }
                    UserData.usr_project_reg = 0;

                    UserData.usr_FX = 0;
                    UserData.usr_siteRole = 0;
                    UserData.admin_theme = 1;
                    if (Session.Count > 0 && Session["PortalUser"] != null)
                    {
                        var PortalData = Session["PortalUser"] as Dictionary<string, string>;
                        UserData.pp_username = PortalData["username"];
                        Session["PortalUser"] = null;
                    }
                    UserData.region_approver = model.Region_Approver;
                    db.usr_user_temp.Add(UserData); // pass InsertUserData to the database
                    await db.SaveChangesAsync(); //Save the data into the database 
                    long id = UserData.usr_ID;
                    byte[] b = ASCIIEncoding.ASCII.GetBytes(id + "");
                    var From_req = "webmaster@rittal.us";
                    var To_req = "" + model.Email + "";//change it to the email address of the person in Mexico
                    var Subject_req = "Action Required: Rittal RiSourceCenter Account Request";
                    var Body_req = "Dear " + model.FirstName + " " + model.LastName + ", \n" +
                    "<br /><br />" +
                    "Before we process your request for RiSource Center access, please click <a href=\"" + link + "/Account/RegisterVerify?id=" + Convert.ToBase64String(b) + "\">this link</a> to open a new tab in your browser. This tab will contain an additional link that must be clicked in order for your request to be processed." +
                    "<br /><br />" +
                    "<b>Please note:</b> Email address verification is a required step! ";

                    commCtl.email(From_req, To_req, Subject_req, Body_req, "yes", true, "henderson.r@rittal.us"); // call the email function

                    //Log the action by the user
                    await commCtl.siteActionLog(0, "Account", DateTime.Now, To_req + " New Account request and email was sent.", "RegisterTemp", 0);

                    return RedirectToAction("RegisterTempConfirm", "Account");
                }
                else if (!ModelState.IsValid)
                {
                    var locController = new CommonController();
                    //check model state errors
                    var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    locController.emailErrors(message);//send errors by email
                    return RedirectToAction("RegisterTemp", new { msg = message });
                }

                //select the company list from the partnerCompanies database
                var companies = from list in db.partnerCompanies where list.comp_active != 0 orderby list.comp_name ascending select list;
                //Add the data to the comp_listing object
                List<SelectListItem> comp_listing = new List<SelectListItem>();
                comp_listing.Add(new SelectListItem { Text = "Select your Company", Value = "select", Selected = true });//default value for select dropdown
                List<long> companyIds = new List<long>();
                foreach (var item in companies)//iterate the add function
                {
                    comp_listing.Add(new SelectListItem { Text = item.comp_name, Value = item.comp_ID.ToString() });
                    if (item.comp_type == 3 || item.comp_name.ToLower() == "rittal united states")
                    {
                        companyIds.Add(item.comp_ID);
                    }
                }

                //select the states data from the data_stat table
                var states = from state in db.data_state where state.state_country == "US" || state.state_country == "CA" orderby state.state_abbr ascending select state;
                //Add the data to the list_state object
                List<SelectListItem> list_states = new List<SelectListItem>();
                list_states.Add(new SelectListItem { Text = "Select your State", Value = "select", Selected = true });//default value for select dropdown
                foreach (var item in states)//iterate the add function
                {
                    list_states.Add(new SelectListItem { Text = item.state_long, Value = item.stateid.ToString() });
                }

                //select countries from the countries table
                var countries = db.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
                //Add the data to the list_countries list object
                List<SelectListItem> list_countries = new List<SelectListItem>();
                list_countries.Add(new SelectListItem { Text = "Select your Country", Value = "select", Selected = true });//default value for select dropdown
                foreach (var item in countries.OrderBy(a => a.country_id))//iterate the add function
                {
                    if (item.country_id != 242)
                    {
                        list_countries.Add(new SelectListItem { Text = item.country_long, Value = item.country_id.ToString() });
                    }
                }

                var approvers = from list in db.RegionApprovers orderby list.name ascending select list;
                List<SelectListItem> approver_listing = new List<SelectListItem>();
                approver_listing.Add(new SelectListItem { Text = "Select your Approver", Value = "", Selected = true });//default value for select dropdown
                foreach (var item in approvers)//iterate the add function
                {
                    approver_listing.Add(new SelectListItem { Text = item.name, Value = item.code });
                }
                approver_listing.Add(new SelectListItem { Text = "None of the above", Value = "" });

                model.List_states = list_states;
                model.Company_listings = comp_listing;
                model.List_countries = list_countries;
                model.Approver_listings = approver_listing;
                // If we got this far, something failed, redisplay form
                ViewBag.approverCompanyIds = companyIds;
                return View(model);
            }
            catch(DbEntityValidationException ex)
            {
                return RedirectToAction("RegisterTemp", new { msg = ex.Message });
            }
            catch(Exception ex)
            {
                return RedirectToAction("RegisterTemp", new { msg = ex.Message });
            }
        }

        // POST: /Account/Register
        //[HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpGet]
        public ActionResult RegisterVerify( string id="" )
        {
            byte[] b = Convert.FromBase64String(id);
            int userid = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(b));
            ViewBag.userId = userid;
            ViewBag.heading = "Account Verification";
            var  model = db.usr_user_temp.Where(a=>a.usr_ID==userid).FirstOrDefault();
            if(model == null)
            {
                ModelState.AddModelError("", "Your account does not exits in Rittal RiSource Center. Please register.");
                return RedirectToAction("RegisterTemp", "Account");
            }
            var confirmedUser = db.usr_user.Where(x => x.usr_email == model.usr_email).FirstOrDefault();
            if (confirmedUser != null)
                return RedirectToAction("RegisterConfirm", "Account");
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> VerifyEmail(int userid = 0)
        {
            ViewBag.userId = userid;
            var model = db.usr_user_temp.Where(a => a.usr_ID == userid).FirstOrDefault();
            var user = new ApplicationUser { UserName = model.usr_email, Email = model.usr_email };
            var result = await UserManager.CreateAsync(user, model.usr_password);
            if (result.Succeeded)
            {
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                code = code.Replace("[", "{b}").Replace("]", "{s}").Replace("/", "[-]").Replace("&", "[n]").Replace("\\", "[b]").Replace("=", "[e]").Replace("%", "[p]").Replace("#", "[h]").Replace("@", "[a]");
                var accept = Url.Action("ConfirmDeny", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //var deny = Url.Action("DenyEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //Get state data
                var state = db.data_state.Where(a => a.stateid == model.usr_state);//Get state data
                var stateName = state.FirstOrDefault().state_long;
                //Get country data
                var country = db.countries.Where(a => a.country_id==model.usr_country);//Get country data
                var countryName = country.FirstOrDefault().country_long;
                //Get company data
                var company = db.partnerCompanies.Where(a => a.comp_ID == model.comp_ID).FirstOrDefault();//Get company data
                var compName = company.comp_name;
                Dictionary<int, string> companyIndustries = db.partnerIndustries.ToDictionary(x => x.pi_ID, x => x.pi_industry);
                Dictionary<int, string> companyAudiences = db.partnerTypes.ToDictionary(x => x.pt_ID, x => x.pt_type);

                //Get Location data
                var location = db.partnerLocations.Where(a => a.loc_ID == model.comp_loc_ID);//Get company data
                var locName = location.FirstOrDefault().loc_name;

                //await UserManager.SendEmailAsync(user.Id, "New RiSourceCenter Account", "Please confirm or deny the account by clicking <a href=\"" + accept + "\">here</a>");

                //usr_details InsertUserData = new usr_details(); //Instantiate the usr_details model
                usr_user UserData = new usr_user(); //Instantiate the usr_details model

                //Set the field values for usr_details model to be inserted into the usr_details model
                UserData.usr_fName = model.usr_fName;
                UserData.usr_lName = model.usr_lName;
                UserData.usr_email = model.usr_email;
                UserData.usr_add1  = model.usr_add1;
                UserData.usr_add2  = model.usr_add2;
                UserData.usr_city  = model.usr_city;
                UserData.usr_state = model.usr_state;
                UserData.usr_country = model.usr_country;
                UserData.usr_language = model.usr_language;
                UserData.usr_zip   = model.usr_zip;
                UserData.usr_phone = model.usr_phone;
                UserData.usr_fax   = model.usr_fax;
                UserData.usr_web   = model.usr_web;
                UserData.comp_ID   = model.comp_ID;
                UserData.comp_loc_ID = model.comp_loc_ID;

                var wN = " ";

                if (model.wN==1)
                {
                    UserData.wN = 1;
                    wN = "Yes";
                }
                else
                {
                    UserData.wN = 0;
                    wN = "No";
                }
                UserData.usr_password     = model.usr_password;
                UserData.usr_title        = model.usr_title;
                UserData.usr_dateCreated  = DateTime.Now;
                UserData.usr_role         = 1;
                UserData.usr_SAP          = 0;
                UserData.usr_POS          = 0;
                UserData.usr_SPA          = 0;
                UserData.usr_project_reg  = 0;
                UserData.usr_MDF          = 0;
                UserData.usr_FX           = 0;
                UserData.usr_siteRole     = 0;
                UserData.admin_theme = 1;
                UserData.pp_username = model.pp_username;
                List<string> accessRequest = new List<string>();
                if(model.usr_POS == 1)
                {
                    accessRequest.Add("Online Shop (online ordering, check price & availability, view quotation and order history)");
                }
                if (model.usr_SAP == 1)
                {
                    accessRequest.Add("SAP Webviewer (check price & availability, view order history and order tracking)");
                }
                if(model.usr_SPA == 1)
                {
                    accessRequest.Add("SPA contracts and rebates");
                }
                if (model.usr_MDF == 1)
                {
                    accessRequest.Add("Marketing development funds (MDF)");
                }
                UserData.access_request = string.Join("<br/>", accessRequest);
                UserData.region_approver = model.region_approver;
                UserData.inactive = false;
                UserData.deleted = false;
                db.usr_user.Add(UserData); // pass InsertUserData to the database
                db.SaveChanges(); //Save the data into the database 
                                   
                var From = "webmaster@rittal.us";
                var To = "";
                StringBuilder AddtoEmail = new StringBuilder();
                if (UserData.usr_country == 228 && !string.IsNullOrEmpty(UserData.usr_zip) && company != null && company.comp_type == 1)
                {
                    List<string> companies = ConfigurationManager.AppSettings["NationalManagerCompanies"].Split(',').ToList();
                    List<string> companyLocations = ConfigurationManager.AppSettings["NationalManagerCompanyLocations"].Split(',').ToList();
                    if(companies.Contains(company.comp_ID.ToString()) || companyLocations.Contains(company.comp_ID + ":" + model.comp_loc_ID))
                    {
                        AddtoEmail.Append(ConfigurationManager.AppSettings["NationalChannelManagerEmail"]);
                    }
                    else
                    {
                        string zip = UserData.usr_zip.TrimStart('0');
                        List<string> approvers = db.RCMContacts.Where(a => a.zipcode == zip && !string.IsNullOrEmpty(a.email)).Select(x => x.email).ToList();
                        if (approvers.Count() > 0)
                        {
                            AddtoEmail.Append(string.Join(",", approvers));
                        }
                        else
                        {
                            AddtoEmail.Append(ConfigurationManager.AppSettings["NationalChannelManagerEmail"]);
                        }
                    }
                }
                if(AddtoEmail.Length == 0)
                {
                    var getsiteapprovers = db.SiteApprovers.Where(a => a.CountryId == UserData.usr_country);
                    if (getsiteapprovers.Count() > 0)//Has a approver
                    {
                        int x = 0;
                        foreach (var item in getsiteapprovers)
                        {
                            if (UserData.usr_country == 228)//It is a US company
                            {
                                if (company.comp_type == 5)//it is a Rittal Company
                                {
                                    if (x < getsiteapprovers.Count() - 1)
                                    {
                                        AddtoEmail.Append(item.Email + ",");
                                    }
                                    else
                                    {
                                        AddtoEmail.Append(item.Email);
                                    }
                                }
                                else
                                {
                                    AddtoEmail.Append("customerservice@rittal.us,anderson.a@rittal.us");//change it to the email address of the person who is admin
                                    break;
                                }
                            }
                            else //It is not a US company
                            {
                                if (x < getsiteapprovers.Count() - 1)
                                {
                                    AddtoEmail.Append(item.Email + ",");
                                }
                                else
                                {
                                    AddtoEmail.Append(item.Email);
                                }
                            }
                            x++;
                        }
                    }
                    else
                    {
                        AddtoEmail.Append("customerservice@rittal.us,anderson.a@rittal.us");//change it to the email address of the person who is admin
                    }
                }
                To = AddtoEmail.ToString();
                var Subject = "New RiSourceCenter Account";
                var Body = "Dear " + To + ", <br /><br /> " + model.usr_fName +" "+ model.usr_lName + " has just requested RisourceCenter Access:" +
                    "<br /><br /><strong>First Name</strong> :" + model.usr_fName +
                    "<br /><strong>Last Name </strong> :" + model.usr_lName +
                    "<br /><strong>Email </strong> :" + model.usr_email +
                    "<br /><strong>Address 1 </strong> :" + model.usr_add1 +
                    "<br /><strong>Address 2 </strong> :" + model.usr_add2 +
                    "<br /><strong>City </strong> :" + model.usr_city +
                    "<br /><strong>State </strong> :" + stateName +
                    "<br /><strong>Country </strong> :" + countryName +
                    "<br /><strong>Company </strong> :" + compName +
                    "<br /><strong>Company Region </strong> :" + company.comp_region +
                    "<br /><strong>Company Industry </strong> :" + (companyIndustries.ContainsKey(company.comp_industry.Value) ? companyIndustries[company.comp_industry.Value] : "") +
                    "<br /><strong>Company Audience </strong> :" + (companyIndustries.ContainsKey(company.comp_industry.Value) ? companyIndustries[company.comp_industry.Value] : "") +
                    "<br /><strong>Location </strong> :" + locName +
                    "<br /><strong>SAP Number </strong> :" + model.sap_numb +
                    "<br /><strong>Zip </strong> :" + model.usr_zip +
                    "<br /><strong>Phone </strong> :" + model.usr_phone +
                    "<br /><strong>Fax </strong> :" + model.usr_fax +
                    "<br /><strong>Website </strong> :" + model.usr_web +
                    "<br /><strong>Need Access To</strong>: <br />" + String.Join("<br />", accessRequest) +
                    "<br /><br /> Please confirm or deny the account by clicking <a href=\"" + accept + "\">here</a>";

                commCtl.email(From, To, Subject, Body, "yes", true, "henderson.r@rittal.us"); // call the email function

                var From_req = "webmaster@rittal.us";
                var To_req = ""+model.usr_email+"";//change it to the email address of the person requestiong access
                var Subject_req = "Rittal RiSourceCenter Account Request";
                var Body_req = "Dear " + model.usr_fName + " "+ model.usr_lName + ", <br /><br /> " +
                    "Thank you for your interest in RiSource Center. Below is the information you submitted. Your request is currently under review by a Rittal representative - once a decision has been made, you will be notified via email along with the necessary instructions." +
                    "<br /><br /><strong>Name</strong>: " + model.usr_fName + " " + model.usr_lName +
                    "<br /><strong>SAP Number</strong>: " + model.sap_numb +
                    "<br /><strong>Company</strong>: " + compName +
                    "<br /><strong>Location</strong>: " + locName +
                    "<br /><strong>Address</strong>: " + model.usr_add1 + ", " + model.usr_add2 +
                    "<br />" + model.usr_city + "," + stateName + " - " + model.usr_zip +
                    "<br />" + countryName +
                    "<br /><strong>Email</strong>: " + model.usr_email +
                    "<br /><strong>Phone</strong>: " + model.usr_phone +
                    "<br /><strong>Fax</strong>: " + model.usr_fax +
                    "<br /><strong>Website</strong>: " + model.usr_web +
                    "<br /><strong>Need Access To</strong>: <br />" + String.Join("<br />",accessRequest) + 
                    "<br /><br />";

                commCtl.email(From_req, To_req, Subject_req, Body_req); // call the email function

                db.usr_user_temp.Remove(model);
                await db.SaveChangesAsync();

                //return RedirectToAction("registerauto.cfm", "account", new { userid = "" + model.usr_ID + "" });
                await commCtl.siteActionLog(0, "Account", DateTime.Now, "Account Verification - User ID: " + userid + " - Email: " + model.usr_email, "Register", 0);
                return RedirectToAction("RegisterConfirm", "Account");
            }
            ViewBag.heading = "Error Verifying Your Email";
            ModelState.AddModelError("", String.Join("<br/>",result.Errors));
            return RedirectToAction("RegisterTemp", "Account");
        }

        //GET: /Account/RegisterConfirm
        [AllowAnonymous]
        public ActionResult RegisterTempConfirm()
            {
            return View();
            }


        //GET: /Account/RegisterConfirm
        [AllowAnonymous]
        public ActionResult RegisterConfirm()
            {
            return View();
            }

        [Authorize]
        [HttpGet]
        public ActionResult ConfirmDeny(string code, string userId = null)
        {
            string redirect = "ConfirmEmail";
            try
            {
                if (userId == null || code == null)
                    throw new Exception("Error");
                var getUserByID = db.AspNetUsers.Single(m => m.Id == userId);
                var usr = db.usr_user.Where(a => a.usr_email == getUserByID.Email).FirstOrDefault();
                if(usr.email_sent == 1)
                {
                    if (getUserByID.EmailConfirmed)
                        throw new Exception("error");
                    else
                    {
                        redirect = "DenyEmail";
                        throw new Exception("Error");
                    }
                }
                ViewBag.userId = userId;
                ViewBag.code = code;
                ViewBag.heading = "Account Confirmation (Approval / Denial)";  
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction(redirect, "Account", new { code = code, userId = userId });
            }
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string code, string userId = null)
        {
            ViewBag.Title = "Account confirmation";
            var results = new Dictionary<string, string>();
            try
            {
                if (userId == null || code == null)
                {
                    throw new Exception("An error occurred while processing your request");
                }
                code = code.Replace("[-]", "/").Replace("[n]", "&").Replace("[b]", "\\").Replace("[e]", "=").Replace("[p]", "%").Replace("[h]", "#").Replace("[a]", "@").Replace("{s}", "]").Replace("{b}", "[");
                var result = await UserManager.ConfirmEmailAsync(userId, code); // currently not working
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.FirstOrDefault().ToString());
                }
                await this.ConfirmUser(userId);
                results.Add("Success", "ConfirmEmail");
                ViewBag.heading = "Account Activated";
            }
            catch (Exception ex)
            {
                results.Add("Error", ex.Message.ToString());
                ViewBag.heading = "Error Activating Account";
            }

            return View(results); // return message to the view
        }

        public async Task<bool> ConfirmUser(string userId)
        {
            var getUserByID = db.AspNetUsers.Single(m => m.Id == userId); // connect to the database and select user with the the userId field
            if (getUserByID == null)
            {
                throw new Exception("No user found");
            }
            var Update_usr = db.usr_user.Where(a => a.usr_email == getUserByID.Email).FirstOrDefault();//get the user ID and move it to the usr_user table
            var user = Update_usr;
            ViewBag.email = getUserByID.Email; //Send the Email to the confirmation page
            ViewBag.userID = Update_usr.usr_ID;
            ViewBag.companyID = Update_usr.comp_ID;
            if (string.IsNullOrEmpty(Update_usr.system_ID) || Update_usr.system_ID != userId || Update_usr.email_sent == null || Update_usr.email_sent == 0)
            {
                var company = db.partnerCompanies.Where(a => a.comp_ID == Update_usr.comp_ID).FirstOrDefault();
                Update_usr.system_ID = userId;
                Update_usr.email_sent = 1;
                await db.SaveChangesAsync();
                if (Update_usr.comp_loc_ID > 0)
                {
                    var loc = db.partnerLocations.Where(x => x.loc_ID == Update_usr.comp_loc_ID).FirstOrDefault();
                    if(loc != null)
                    {
                        bool accountAdded = false;
                        if(!string.IsNullOrEmpty(loc.loc_Webshop_account))
                        {
                            accountAdded = true;
                            db.Webshop_connect.Add(new Webshop_connect()
                            {
                                ws_account = loc.loc_Webshop_account,
                                usr_user = Update_usr.usr_ID,
                                loc_id = Update_usr.comp_loc_ID.Value
                            });
                        }
                        if(loc.loc_SAP_account.HasValue && loc.loc_SAP_account.Value > 0)
                        {
                            accountAdded = true;
                            db.partnerStockChecks.Add(new partnerStockCheck()
                            {
                                ps_account = loc.loc_SAP_account.Value,
                                usr_user = Update_usr.usr_ID,
                                loc_id = Update_usr.comp_loc_ID.Value
                            });
                        }
                        if (accountAdded)
                            await db.SaveChangesAsync();
                    }
                }
                
                //Set the global variables for the email function
                string From = "webmaster@rittal.us";
                string To = getUserByID.Email;
                var Subject = "Rittal RiSourceCenter Account Approved";
                var callbackUrl = "https://www.risourcecenter.com/Account/Login";
                var ForgetUrl = "https://www.risourcecenter.com/Account/ForgotPassword";
                string Body = string.Format("Dear " + user.usr_fName + " " + user.usr_lName + ",<br /><br /> Your request to access Rittal RiSourceCenter is <b>Approved!</b><br /><br />" +
                    "Please login using the link below along with the email address and password you provided in the initial registration form." +
                    "<br /><br />Link: <a href=\"" + callbackUrl + "\">" + callbackUrl + "</a>" +
                    "<br />Email: " + To +
                    "<br /><br />If you've forgotten your password, please click <a href=\"" + ForgetUrl + "\"><b>this link</b></a> which will open a tab in your default browser. You will be prompted to provide your email address and you will then receive an email from <a href='mailto:webmaster@rittal.us'>webmaster@rittal.us</a> that will enable you to change your password.");
                if (Update_usr.usr_country == 228)
                {
                    Body += "<br /><br />You may have questions once you begin using RiSource Center. ";
                    if (company != null)
                    {
                        if (company.comp_type == 5)
                            Body += "If so, please contact <a href='mailto:Henderson.R@rittal.us'>Rick Henderson</a>, Digital Marketing Manager for Rittal North America.<b> Please Note: If you are part of the Rittal sales team, you should receive training on the RiSource Center as part of your on boarding process.</b>";
                        else if (company.comp_industry == 1)
                            Body += "If so, please contact <a href='mailto:czlonka.n@rittal.us?subject=Inquiry%20from%20RiSource%20Center%20user'>Nancy Czlonka</a>, Director of IT Channel Sales or alternatively use the <a href='https://www.risourcecenter.com/Home/contact?parentId=6&childId=116&n1_name=Links&n2_name=Contact%20a%20Rittal%20Rep&submenu=yes'>Contact page</a> on RiSource Center for additional contact information.";
                        else
                            Body += "If so, please contact the Regional Channel Manager (RCM) in your area. If you don't know who your RCM is, please contact <a href='mailto:barth.j@rittal.us'>Jim Barth</a>, National Channel Manager.";
                    }
                }
                commCtl.email(From, To, Subject, Body); // call the email function
                await commCtl.siteActionLog(0, "Account", DateTime.Now, "RiSourceCenter Account Approved - User ID: " + userId + " - Email: " + getUserByID.Email, "ConfirmEmail", 0);
                if (!string.IsNullOrEmpty(Update_usr.region_approver))
                {
                    await this.CreateStoreFrontUser(Update_usr, (company != null ? company.comp_name : ""));
                }
            }
            return true;
        }

        //GET:Account/DenyEmail
        [Authorize]
        public async Task<ActionResult> DenyEmail(string userId, string code)
        {
            ViewBag.Title = "Account Denial";

            if (userId == null || code == null)
            {
                return View("Error");
            }
            await this.DenyUser(userId);
            return View(); // return message to the view
        }

        public async Task<bool> DenyUser(string userId)
        {
            var getUserByID = db.AspNetUsers.Single(m => m.Id == userId); // connect to the database and select user with the the userId field
            var Update_usr = db.usr_user.Where(a => a.usr_email == getUserByID.Email).FirstOrDefault();
            ViewBag.email = getUserByID.Email;
            if (string.IsNullOrEmpty(Update_usr.system_ID) && (Update_usr.email_sent == null || Update_usr.email_sent == 0))
            {
                //Set the global variables for the email function
                string From = "webmaster@rittal.us";
                string To = getUserByID.Email;
                var Subject = "RiSourceCenter Account Denied";
                var Body = string.Format("Dear " + To + ", <br /><br />Your request for access into the RiSourceCenter has been denied. Please contact customer service at <a href=\"mailto: customerservice@rittal.us ? Subject = RiSourceCenter % 20access % 20request\" target=\"_top\">customerservice@rittal.us</a> with any questions or concerns.");
                await commCtl.siteActionLog(0, "Account", DateTime.Now, Update_usr.usr_email + " was denied", "DenyEmail", 0);
                commCtl.email(From, To, Subject, Body, "yes", true); // call the email function
                Update_usr.email_sent = 1;
                await db.SaveChangesAsync();
            }
            return true;
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    //Log the action by the user
                    await commCtl.siteActionLog(0, "Account", DateTime.Now, model.Email + " forgot password and email was not sent. User does not exist ", "ForgotPassword", 0);

                    var Fromi = "webmaster@rittal.us";
                    var Toi = "presswala.z@rittal.us";
                    var Subjecti = "RiSourceCenter reset password Email not sent";
                    var Bodyi = model.Email + " forgot password and email was not sent. User does not exist using page ForgotPassword. Check the user table for the user";
                    commCtl.email(Fromi, Toi, Subjecti, Bodyi); // call the email function

                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                var From = "webmaster@rittal.us";
                var To = model.Email;
                var Subject = "RiSourceCenter reset password";
                var Body = "Dear "+ To + ",<br /><br />Please click <a href=\"" + callbackUrl + "\">here</a> to reset your password for the RiSourceCenter.";

                commCtl.email(From, To, Subject, Body); // call the email function

                //Log the action by the user
                await commCtl.siteActionLog(0, "Account", DateTime.Now, To + " forgot password and email was sent", "ForgotPassword", 0);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email

                //Log the action by the user
                await commCtl.siteActionLog(0, "Account", DateTime.Now, model.Email + " forgot password and email was not sent. Validation failed ", "ForgotPassword", 0);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                //get the user ID and move it to the usr_user table
                var email = model.Email;
                var Update_usr = db.usr_user.Where(a => a.usr_email == email).FirstOrDefault();
                Update_usr.usr_password = model.Password;
                await db.SaveChangesAsync();
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            await CountLoggedin("loggedout");//remove users to the loggin list
            Session.Clear();
            HttpContext.Response.Cookies.Set(new HttpCookie("RISOURCEUSRID") { Value = string.Empty });

            var returnUrl = Request.UrlReferrer.PathAndQuery;
            if (returnUrl!=null)
            {
                return RedirectToAction("Login", "Account", new { ReturnUrl = returnUrl});
                //Response.Redirect(returnUrl);
                //http://localhost:58419/Account/Login?ReturnUrl=%2FSalesComm%2FgetSaleComById%2F%3FscID%3D33828%26n3id%3D20223%26n1_name%3DSales%2520Communications%26sent%3D5%2F16%2F2018%252012%3A00%3A00%2520AM
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            db.Dispose();
            base.Dispose(disposing);
        }

        #region count logined users
        public async Task CountLoggedin(string action)
        {
            //Delete the user
            string SystemID = Convert.ToString(Session["system_ID"]);
            var logins = db.CountLoggedins.Where(a => a.SystemID == SystemID);
            if (logins.Count()!=0)
            {
                db.CountLoggedins.RemoveRange(logins);
                await db.SaveChangesAsync();
            }

            if (action == "loggedin")
            {
                var loginNew = db.CountLoggedins;
                var newlogin = new CountLoggedin
                {
                    SystemID = SystemID
                };
                loginNew.Add(newlogin);
                await db.SaveChangesAsync();           
            }
        }

        [HttpPost]
        public JsonResult LoggedinCount()
        {
            var countLogins = db.CountLoggedins.Count();
            return Json(countLogins);
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion

        #region StoreFront API
        public async Task<bool> CreateStoreFrontUser(usr_user user, string companyName)
        {
            var state = db.data_state.Where(x => x.stateid == user.usr_state).FirstOrDefault();
            var country = db.countries.Where(x => x.country_id == user.usr_country).FirstOrDefault();
            if(string.IsNullOrEmpty(companyName))
            {
                var company = db.partnerCompanies.Where(x => x.comp_ID == user.comp_ID) .FirstOrDefault();
                if(company != null)
                {
                    companyName = company.comp_name;
                }
            }
            string soapString = CreateSoapEnvelope(user, companyName, (state != null ? state.state_abbr : ""), (country != null ? country.country_long : ""));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpResponseMessage response = await PostXmlRequest("https://print1step.mypresswise.com/r/wsdl-order2.php", soapString);
            string content = await response.Content.ReadAsStringAsync();
            return response.StatusCode == HttpStatusCode.OK;
        }
        public static async Task<HttpResponseMessage> PostXmlRequest(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "#POST");
                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }

        public static string CreateSoapEnvelope(usr_user user, string compName, string state, string country)
        {
            return $@"<SOAP-ENV:Envelope
            xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'
            xmlns:SOAP-ENC='http://schemas.xmlsoap.org/soap/encoding/'
            xmlns:xsd='http://www.w3.org/2001/XMLSchema'
            xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' SOAP-ENV:encodingStyle='http://schemas.xmlsoap.org/soap/encoding/'>
            <SOAP-ENV:Body>
            <create_user>
            <auth>
            <user>print1step.mypresswise.com</user>
            <pass>5e796c3f5cb5f</pass>
            <code>E8428AED70BB5928142D0EB6AE933D6E</code>
            </auth>
            <request>
            <customerID>{user.region_approver.Replace("C", "").Trim()}</customerID>
            <user>
            <loginID>{user.usr_email}</loginID>
            <loginPass>{user.usr_password}</loginPass>
            <contactName>{(user.usr_fName + " " + user.usr_lName).Trim()}</contactName>
            <companyName>{compName}</companyName>
            <contactEmail>{user.usr_email}</contactEmail>
            <contactTelePhone>{user.usr_phone}</contactTelePhone>
            <contactExt></contactExt>
            <contactFax>{user.usr_fax}</contactFax>
            <contactAddress1>{user.usr_add1}</contactAddress1>
            <contactAddress2>{user.usr_add2}</contactAddress2>
            <contactCity>{user.usr_city}</contactCity>
            <contactState>{state}</contactState>
            <contactZip>{user.usr_zip}</contactZip>
            <contactCountry>{country}</contactCountry>
            <AllowOrder>Y</AllowOrder>
            <AllowApproval></AllowApproval>
            <Admin></Admin>
            </user>
            </request>
            </create_user>
            </SOAP-ENV:Body>
            </SOAP-ENV:Envelope>";
        }
        #endregion
    }
}
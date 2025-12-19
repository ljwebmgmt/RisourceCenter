using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace newrisourcecenter.Controllers
{
    public class SPAController : Controller
    {
        private RisourceCenterMexicoEntities Entitydb = new RisourceCenterMexicoEntities();
        private RisourceCenterContext db;
        private CommonController comm = new CommonController();

        public SPAController() : this(new RisourceCenterContext())
        {

        }

        public SPAController(RisourceCenterContext _db)
        {
            db = _db;
        }

        #region Index
        // GET: SPA
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<SPAViewModels> SPAViewModels = new List<SPAViewModels>();
            List<SPAViewModels> SPACurrent = new List<SPAViewModels>();
            List<SPAViewModels> SPAExpired = new List<SPAViewModels>();
            List<SPAViewModels> SPAUpcoming = new List<SPAViewModels>();
            List<SPAViewModels> SPADraft = new List<SPAViewModels>();
            List<SPAViewModels> SPADeclined = new List<SPAViewModels>();
            List<SPAViewModels> SPAAwaitingApproval = new List<SPAViewModels>();

            var spamodel = db.SPAViewModels.Where(a => a.Usr_id == userId).ToList();
            //Get contracts from only the company
            await Loopdata(spamodel.Where(a => a.Activity_status == "1").ToList(), string.Empty, SPACurrent);
            await Loopdata(spamodel.Where(a => a.Activity_status == "4").ToList(), string.Empty, SPAExpired);
            await Loopdata(spamodel.Where(a => a.Activity_status == "5").ToList(), string.Empty, SPAUpcoming);
            await Loopdata(spamodel.Where(a => a.Activity_status == "3").ToList(), string.Empty, SPADraft);
            await Loopdata(spamodel.Where(a => a.Activity_status == "2").ToList(), string.Empty, SPADeclined);
            await Loopdata(spamodel.Where(a => a.Activity_status == "6").ToList(), string.Empty, SPAAwaitingApproval);
            SPAbreakdown SPAbreakdown = new SPAbreakdown { SPACurrent = SPACurrent.OrderBy(a => a.Spa_id).ToList(),
                SPAAwaitingApproval = SPAAwaitingApproval.OrderBy(a => a.Spa_id).ToList(),
                SPADeclined = SPADeclined.OrderBy(a => a.Spa_id).ToList(),
                SPAExpired = SPAExpired.OrderBy(a => a.Spa_id).ToList(),
                SPAUpcoming = SPAUpcoming.OrderBy(a => a.Spa_id).ToList(),
                SPADraft = SPADraft.OrderBy(a => a.Spa_id).ToList()
            };

            return View(SPAbreakdown);
        }
        #endregion

        #region SPAAdmin
        // GET: SPA
        public async Task<ActionResult> SPAAdmin(int comp_id = 0, int usr_id = 0)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int usr_SPA = Convert.ToInt32(Session["usr_SPA"]);//Regional Manager
            int usr_POS = Convert.ToInt32(Session["usr_POS"]);//Account Manager
            string region = Convert.ToString(Session["comp_region"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            List<SPAViewModels> SPAViewModels = new List<SPAViewModels>();
            List<SPAViewModels> SPACurrent = new List<SPAViewModels>();
            List<SPAViewModels> SPAExpired = new List<SPAViewModels>();
            List<SPAViewModels> SPAUpcoming = new List<SPAViewModels>();
            List<SPAViewModels> SPADraft = new List<SPAViewModels>();
            List<SPAViewModels> SPADeclined = new List<SPAViewModels>();
            List<SPAViewModels> SPAAwaitingApproval = new List<SPAViewModels>();

            IEnumerable<SPAViewModels> spa_users = new List<SPAViewModels>();
            var spamodel = await db.SPAViewModels.ToListAsync();
            List<CountCompanies> countCompanies = new List<CountCompanies>();
            List<CountUsers> countUsersName = new List<CountUsers>();
            List<partnerCompanyViewModel> companies = await GetDistributors(region);
            //Get company name
            if (comp_id!=0)
            {
                var companyName = await db.partnerCompanyViewModels.FindAsync(comp_id);
                ViewBag.comp_name = companyName.comp_name;
            }

            if (usr_id!=0) {
                var userName = await db.UserViewModels.FindAsync(usr_id);
                ViewBag.usr_name = userName.usr_fName + "" + userName.usr_lName;
            }

            foreach (var item in companies)
            {
                string countContracts = "";
                var contracts = spamodel.Where(a => a.Comp_id == item.comp_ID);//Get SPAs with the company ID
                if (contracts.Count() != 0)
                {
                    countContracts = contracts.Count().ToString();// Count Contracts by company
                    //Get SPA users
                    if (comp_id != 0)
                    {
                        spa_users = contracts.Where(a => a.Comp_id == comp_id);
                        //Get contracts from only the company if the company id is not zero
                        await Loopdata(spa_users.Where(a => a.Activity_status == "1").ToList(), item.comp_name, SPACurrent);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "4").ToList(), item.comp_name, SPAExpired);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "5").ToList(), item.comp_name, SPAUpcoming);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "3").ToList(), item.comp_name, SPADraft);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "2").ToList(), item.comp_name, SPADeclined);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "6").ToList(), item.comp_name, SPAAwaitingApproval);
                    }
                    else if (usr_id != 0)
                    {
                        spa_users = contracts;
                        //Get contracts from only the users if the user id is not zero
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "1").ToList(), item.comp_name, SPACurrent);
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "4").ToList(), item.comp_name, SPAExpired);
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "5").ToList(), item.comp_name, SPAUpcoming);
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "3").ToList(), item.comp_name, SPADraft);
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "2").ToList(), item.comp_name, SPADeclined);
                        await Loopdata(spa_users.Where(a => a.Usr_id == usr_id && a.Activity_status == "6").ToList(), item.comp_name, SPAAwaitingApproval);
                    }
                    else
                    {
                        spa_users = contracts;
                        //Get contracts from the list of all the companies
                        await Loopdata(spa_users.Where(a => a.Activity_status == "1").ToList(), item.comp_name, SPACurrent);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "4").ToList(), item.comp_name, SPAExpired);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "5").ToList(), item.comp_name, SPAUpcoming);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "3").ToList(), item.comp_name, SPADraft);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "2").ToList(), item.comp_name, SPADeclined);
                        await Loopdata(spa_users.Where(a => a.Activity_status == "6").ToList(), item.comp_name, SPAAwaitingApproval);
                    }

                    foreach (var item_user in spa_users.GroupBy(a => new { a.Usr_id }))//Group contracts by users
                    {
                        string countContracts_user = "";
                        var contracts_user = spa_users.Where(a => a.Usr_id == item_user.Key.Usr_id);
                        if (contracts_user.Count() != 0)
                        {
                            var user = await comm.GetfullName(Convert.ToInt32(item_user.Key.Usr_id));
                            countContracts_user = contracts_user.Count().ToString();
                            //get the contract and usr information
                            countUsersName.Add(new CountUsers { UserName = user["fullName"], CountSPAs = countContracts_user, Usr_id = item_user.Key.Usr_id });
                        }
                    }
                }
                //get the contract and usr information
                countCompanies.Add(new CountCompanies { Comp_id = item.comp_ID, CompanyName = item.comp_name, CountSPAs = countContracts, Comp_POS = Convert.ToInt32(item.comp_POS), Comp_SPA = Convert.ToInt32(item.comp_SPA) });
            }

            if (countUsersName.Count() != 0)
            {
                ViewBag.List_countUsers = countUsersName;
            }

            ViewBag.List_distributors = countCompanies.OrderBy(a => a.CompanyName);

            SPAbreakdown SPAbreakdown = new SPAbreakdown
            {
                SPACurrent = SPACurrent.OrderBy(a => a.Spa_id).ToList(),
                SPAAwaitingApproval = SPAAwaitingApproval.OrderBy(a => a.Spa_id).ToList(),
                SPADeclined = SPADeclined.OrderBy(a => a.Spa_id).ToList(),
                SPAExpired = SPAExpired.OrderBy(a => a.Spa_id).ToList(),
                SPAUpcoming = SPAUpcoming.OrderBy(a => a.Spa_id).ToList(),
                SPADraft = SPADraft.OrderBy(a => a.Spa_id).ToList()
            };

            return View(SPAbreakdown);
        }

        //Loop function
        public async Task<List<SPAViewModels>> Loopdata(List<SPAViewModels> data, string comp_name, List<SPAViewModels> SPAlist)
        {
            foreach (var contract in data)
            {
                var user = await comm.GetfullName(Convert.ToInt32(contract.Usr_id));
                SPAlist.Add(new SPAViewModels
                {
                    Spa_id = contract.Spa_id,
                    Usr_id = contract.Usr_id,
                    UserName = user["fullName"],
                    CompanyName = comp_name,
                    Customer_name = contract.Customer_name
                });
            }

            return SPAlist;
        }

        public async Task<ActionResult> SubmitRequest(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var contracts = await db.SPAViewModels.Where(a => a.Spa_id == id).FirstOrDefaultAsync();//Get SPAs with the company ID
            //contracts.Start_date = DateTime.Now;
            contracts.Activity_status = "6";
            await db.SaveChangesAsync();

            var user = await comm.GetfullName(userId);
            await LogChanges("SubmitRequest", "The SPA has been submitted by " + user["fullName"], contracts.Spa_id, 0);

            //isAdmin = no & submenu = yes
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], isAdmin = "no", submenu = "yes", success = "Your request has been submitted" });
        }

        public async Task Checkdate()
        {
            var contracts = await db.SPAViewModels.ToListAsync();//Get SPAs with the company ID

            //Set the current contracts daily
            foreach (var item in contracts.Where(a => a.Activity_status == "1" || a.Activity_status == "5"))
            {
                DateTime date1 = Convert.ToDateTime(item.End_date);
                if (date1 >= DateTime.Today)
                {
                    item.Activity_status = "1";
                }
            }

            //Set the expired contracts daily
            foreach (var item in contracts.Where(a => a.Activity_status == "1"))
            {
                DateTime date1 = Convert.ToDateTime(item.End_date);
                if (date1 < DateTime.Today)
                {
                    item.Activity_status = "4";
                    await UpdateSPAItems(item.Spa_id, 4);
                }
            }

            //Set the pending contracts daily
            foreach (var item in contracts.Where(a => a.Activity_status == "1"))
            {
                DateTime date2 = Convert.ToDateTime(item.Start_date);
                if (date2 > DateTime.Today)
                {
                    item.Activity_status = "5";
                }
            }
        }

        [HttpPost]
        public async Task UpdateContractStatus(int id = 0, string status = null)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var SPAContract = db.SPAViewModels.Find(id);
            SPAContract.Activity_status = status;
            if (status == "1")
            {
                SPAContract.Approved_date = DateTime.Today;
            }

            await db.SaveChangesAsync();

            var user = await comm.GetfullName(userId);
            await LogChanges("UpdateContractStatus", user["fullName"] + " has updated  a SKU on the SPA", SPAContract.Spa_id, 0);
        }

        public async Task UpdateSPAItems(int spa_id, int status)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var SPAitems = await db.SPAItemViewModels.Where(a => a.Form_id == spa_id).ToListAsync();
            foreach (var spaitem in SPAitems)
            {
                spaitem.Date_Updated = DateTime.Today;
            }

            await db.SaveChangesAsync();

            var user = await comm.GetfullName(userId);
            await LogChanges("UpdateSPAItems", user["fullName"] + " has updated a SKU on the SPA", spa_id, 0);
        }
        #endregion

        #region Manage Assign
        [HttpGet]
        public async Task<ActionResult> Manageassign(string usr_email=null)
        {
            List<Compids> list_comp_us = new List<Compids>();
            List<SPASalesRepsViewModel> Sales_reps = new List<SPASalesRepsViewModel>();
            //List US companies
            list_comp_us.Add(new Compids { comp_ids = "312" });
            list_comp_us.Add(new Compids { comp_ids = "407" });
            list_comp_us.Add(new Compids { comp_ids = "408" });
            list_comp_us.Add(new Compids { comp_ids = "409" });
            list_comp_us.Add(new Compids { comp_ids = "410" });
            list_comp_us.Add(new Compids { comp_ids = "411" });
            list_comp_us.Add(new Compids { comp_ids = "412" });
            list_comp_us.Add(new Compids { comp_ids = "413" });
            list_comp_us.Add(new Compids { comp_ids = "477" });
            list_comp_us.Add(new Compids { comp_ids = "30629" });
            list_comp_us.Add(new Compids { comp_ids = "30630" });
            //Get Users of the RiSouce Center
            var user_data = db.UserViewModels.ToList();

            List<SPAAccountManager> SPAAccountManage = new List<SPAAccountManager>();
            List<SPAAccountManager> SPAAccountManageAssign = new List<SPAAccountManager>();
            List<SPAViewModels> spaviewmodels = new List<SPAViewModels>();
            var spamodels = await db.SPAViewModels.ToListAsync();

            //Get sales reps for  left
            var accountmanagers = db.SPAAccountManagers;
            foreach (var item in user_data)
            {
                if (list_comp_us.Select(a => a.comp_ids).Contains(item.comp_ID.ToString()))
                {
                    int count = spamodels.Where(a => a.Sales_rep_user == item.usr_email).Count();
                    SPAAccountManage.Add(new SPAAccountManager { email = item.usr_email, contact_name = item.usr_fName + " " + item.usr_lName, count = count.ToString() });
                }
            }
            foreach (var item in accountmanagers.GroupBy(a => new { a.contact_name, a.email }))
            {
                if (!SPAAccountManage.Select(a => a.email).Contains(item.Key.email))
                {
                    int count = spamodels.Where(a => a.Sales_rep_user == item.Key.email).Count();
                    SPAAccountManage.Add(new SPAAccountManager { email = item.Key.email, contact_name = item.Key.contact_name, count = count.ToString() });
                }
            }
            ViewBag.salesreps = SPAAccountManage.OrderBy(a=>a.contact_name);

            //Get sales reps for right 
            foreach (var item in user_data)
            {
                if (list_comp_us.Select(a => a.comp_ids).Contains(item.comp_ID.ToString()))
                {
                    SPAAccountManageAssign.Add(new SPAAccountManager { email = item.usr_email, contact_name = item.usr_fName + " " + item.usr_lName });
                }
            }
            foreach (var item in accountmanagers.GroupBy(a => new { a.contact_name, a.email }))
            {
                if (!SPAAccountManageAssign.Select(a => a.email).Contains(item.Key.email))
                {
                    SPAAccountManageAssign.Add(new SPAAccountManager { email = item.Key.email, contact_name = item.Key.contact_name });
                }
            }
            ViewBag.salesrepsAssign = SPAAccountManageAssign.OrderBy(a => a.contact_name);
           
            //SPA Model
            foreach (var item in spamodels.Where(a => a.Sales_rep_user == usr_email))
            {
                var companyName = await db.partnerCompanyViewModels.Where(b=>b.comp_ID==item.Comp_id).FirstOrDefaultAsync();
                var user = await comm.GetfullName(Convert.ToInt32(item.Usr_id));
                spaviewmodels.Add(new SPAViewModels { Spa_id=item.Spa_id, CompanyName=companyName.comp_name,Customer_name = item.Customer_name,Sales_rep_user_name=user["fullName"]});
            }

            return View(spaviewmodels);
        }

        [HttpPost]
        public async Task<ActionResult> Manageassign()
        {
            var useremail = Request.Form["selectUser"];
            var spa_ids = Request.Form["spa_ids"];

            if (spa_ids!=null && useremail!="") {
                var spamodels = await db.SPAViewModels.ToListAsync();

                foreach (var item in spa_ids.Split(','))
                {
                    int? id = Convert.ToInt32(item);
                    var list = spamodels.Where(a => a.Spa_id == id).FirstOrDefault();
                    list.Sales_rep_user = useremail;
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("manageassign",new {n1_name=Request.Form["n1_name"], n2_name=Request.Form["n2_name"], usr_email=useremail,msg=Request.Form["msg"],isAdmin="yes"});
        }
        #endregion

        #region Manage Participants
        [HttpGet]
        public async Task<ActionResult> Manageparticipant(int comp_id = 0)
        {
            List<partnerCompanyViewModel> partnerComp = new List<partnerCompanyViewModel>();
            List<SPAAccountManager> SPAAccountManage = new List<SPAAccountManager>();
            List<SPAViewModels> spaviewmodels = new List<SPAViewModels>();
            var spamodels = await db.SPAViewModels.ToListAsync();

            var getCompanies = await db.partnerCompanyViewModels.Where(a=>a.comp_type==1).ToListAsync();
            foreach (var item in getCompanies)
            {
                int count = spamodels.Where(a => a.Comp_id == item.comp_ID).Count();
                partnerComp.Add(new partnerCompanyViewModel { comp_ID=item.comp_ID, comp_name=item.comp_name,count_users = count });
            }
            ViewBag.partnerComp = partnerComp;

            //Get sales reps
            List<Compids> list_comp = new List<Compids>();
            List<Compids> list_comp_us = new List<Compids>();
            List<UserViewModel> getuser_data;

            //List US companies
            list_comp_us.Add(new Compids { comp_ids = "312" });
            list_comp_us.Add(new Compids { comp_ids = "407" });
            list_comp_us.Add(new Compids { comp_ids = "408" });
            list_comp_us.Add(new Compids { comp_ids = "409" });
            list_comp_us.Add(new Compids { comp_ids = "410" });
            list_comp_us.Add(new Compids { comp_ids = "411" });
            list_comp_us.Add(new Compids { comp_ids = "412" });
            list_comp_us.Add(new Compids { comp_ids = "413" });
            list_comp_us.Add(new Compids { comp_ids = "477" });
            list_comp_us.Add(new Compids { comp_ids = "30629" });
            list_comp_us.Add(new Compids { comp_ids = "30630" });
            
            //Get Rittal and Man Rep Companies
            List<partnerCompanyViewModel> company_ids = db.partnerCompanyViewModels.Where(a => a.comp_type == 5 || a.comp_type == 3).ToList();
            //Get users
            getuser_data = db.UserViewModels.ToList();         

            foreach (var items in company_ids)
            {
                if (list_comp_us.Select(a => a.comp_ids).Contains(items.comp_ID.ToString()))
                {
                    list_comp.Add(new Compids { comp_ids = items.comp_ID.ToString() });
                }
            }

            foreach (var user in getuser_data)
            {
                if (list_comp.Select(a => a.comp_ids).Contains(user.comp_ID.ToString()))
                {
                    SPAAccountManage.Add(new SPAAccountManager { email = user.usr_email, contact_name = user.usr_fName + " " + user.usr_lName });
                }
            }

            ViewBag.salesreps = SPAAccountManage.OrderBy(a => a.contact_name);

            //Get Users of the RiSouce Center
            var user_data = db.UserViewModels.ToList();
            foreach (var item in spamodels.Where(a => a.Comp_id == comp_id))
            {
                List<SPASalesRepsViewModel> newsalesrep = new List<SPASalesRepsViewModel>();
                var user = await comm.GetfullName(Convert.ToInt32(item.Usr_id));
                //Get Participants
                var particpants = db.SPA_SalesRepsViewModels.Where(a => a.Form_id == item.Spa_id);
                foreach (var items in particpants)
                {
                    var get_user = user_data.Where(a => a.usr_email == items.Usr_id).FirstOrDefault();
                    if (get_user!=null) {
                        newsalesrep.Add(new SPASalesRepsViewModel { Usr_id = items.Usr_id, FullName = get_user.usr_fName + " " + get_user.usr_lName });
                    }
                }
                spaviewmodels.Add(new SPAViewModels { Spa_id = item.Spa_id, Customer_name = item.Customer_name, Sales_rep_user_name = user["fullName"], List_sales_reps = newsalesrep });
            }

            return View(spaviewmodels);
        }

        [HttpGet]
        public async Task<string> Selectparticipant(string querystring = null)
        {
            List<partnerCompanyViewModel> partnerComp = new List<partnerCompanyViewModel>();
            List<SPAAccountManager> SPAAccountManage = new List<SPAAccountManager>();
            List<SPAViewModels> spaviewmodels = new List<SPAViewModels>();
            var spamodels = await db.SPAViewModels.ToListAsync();

            //Get sales reps
            List<Compids> list_comp = new List<Compids>();
            List<Compids> list_comp_us = new List<Compids>();
            List<UserViewModel> getuser_data;

            //List US companies
            list_comp_us.Add(new Compids { comp_ids = "312" });
            list_comp_us.Add(new Compids { comp_ids = "407" });
            list_comp_us.Add(new Compids { comp_ids = "408" });
            list_comp_us.Add(new Compids { comp_ids = "409" });
            list_comp_us.Add(new Compids { comp_ids = "410" });
            list_comp_us.Add(new Compids { comp_ids = "411" });
            list_comp_us.Add(new Compids { comp_ids = "412" });
            list_comp_us.Add(new Compids { comp_ids = "413" });
            list_comp_us.Add(new Compids { comp_ids = "477" });
            list_comp_us.Add(new Compids { comp_ids = "30629" });
            list_comp_us.Add(new Compids { comp_ids = "30630" });

            //Get Rittal and Man Rep Companies
            List<partnerCompanyViewModel> company_ids = db.partnerCompanyViewModels.Where(a => a.comp_type == 5 || a.comp_type == 3).ToList();
            //Get users
            if (querystring == null)
            {
                getuser_data = db.UserViewModels.ToList();
            }
            else
            {
                getuser_data = db.UserViewModels.Where(a => a.usr_email.Contains(querystring) || a.usr_fName.Contains(querystring) || a.usr_lName.Contains(querystring)).ToList();
            }

            foreach (var items in company_ids)
            {
                if (list_comp_us.Select(a => a.comp_ids).Contains(items.comp_ID.ToString()))
                {
                    list_comp.Add(new Compids { comp_ids = items.comp_ID.ToString() });
                }
            }

            foreach (var user in getuser_data)
            {
                if (list_comp.Select(a => a.comp_ids).Contains(user.comp_ID.ToString()))
                {
                    SPAAccountManage.Add(new SPAAccountManager { email = user.usr_email, contact_name = user.usr_fName + " " + user.usr_lName });
                }
            }

            var restunedString = JsonConvert.SerializeObject(new { spaaccountmanager = SPAAccountManage.OrderBy(a => a.contact_name), querystring = querystring, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public async Task<ActionResult> Manageparticipant()
        {
            string email = "";
            var spa_ids = Request.Form["spa_ids"];

            if (!string.IsNullOrEmpty(Request.Form["enderedUser"]))
            {
                email = Request.Form["enderedUser"];
            }
            else if(!string.IsNullOrEmpty(Request.Form["selectUser"]))
            {
                email = Request.Form["selectUser"];
            }
           
            if (spa_ids != null && email != "")
            {
                foreach (var item in spa_ids.Split(','))
                {
                    int? id = Convert.ToInt32(item);
                    var checkParticipant = await db.SPA_SalesRepsViewModels.Where(a=>a.Form_id==id && a.Usr_id==email).FirstOrDefaultAsync();
                    if (checkParticipant==null)
                    {
                        db.SPA_SalesRepsViewModels.Add(new SPASalesRepsViewModel { Form_id = id, Usr_id = email });
                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("Manageparticipant", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], comp_id = Request.Form["comp_id"], msg = Request.Form["msg"], isAdmin = "yes" });
        }
        #endregion

        #region Get Distributors
        private async Task<List<partnerCompanyViewModel>> GetDistributors(string region = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                RedirectToAction("Login", "Account");
            }

            if (User.IsInRole("Global Admin") || User.IsInRole("Super Admin"))
            {
                List<partnerCompanyViewModel> distributors = await db.partnerCompanyViewModels.ToListAsync();
                return distributors;
            }
            else
            {
                List<partnerCompanyViewModel> distributors = await db.partnerCompanyViewModels.Where(a => a.comp_region == region).ToListAsync();
                return distributors;
            }
        }
        #endregion

        #region Contract Actions
        public async Task<ActionResult> CreateCustomerinfo()
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int usr_SPA = Convert.ToInt32(Session["usr_SPA"]);//Regional Manager
            int usr_POS = Convert.ToInt32(Session["usr_POS"]);//Account Manager
            string region = Convert.ToString(Session["comp_region"]);
            string comp_name = Convert.ToString(Session["comp_name"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            SPAViewModels spamodels = new SPAViewModels();
            List<partnerCompanyViewModel> companies = await GetDistributors(region);
            List<partnerLocationViewModel> locations = await db.partnerLocationViewModels.ToListAsync();
            List<SelectListItem> newlist = new List<SelectListItem>();
            newlist.Add(new SelectListItem { Value = "", Text = "---Please select a distributor---" });

            if (User.IsInRole("Super Admin") || User.IsInRole("Global Admin"))
            {
                //If Super Admin get all locations
                foreach (var company in companies)
                {
                    var comp_locations = locations.Where(a => a.comp_ID == company.comp_ID);
                    foreach (var loc in comp_locations)
                    {
                        if (!string.IsNullOrEmpty(loc.loc_name))
                        {
                            newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = company.comp_name + "-" + loc.loc_name + "(" + loc.loc_SAP_account + ")" });
                        }
                        else
                        {
                            newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = company.comp_name + "-" + loc.loc_city + "(" + loc.loc_SAP_account + ")" });
                        }
                    }
                }
            }
            else
            {
                //if not super admin or global admin select only location under company
                var comp_locations = locations.Where(a => a.comp_ID == companyId);
                foreach (var loc in comp_locations)
                {
                    if (!string.IsNullOrEmpty(loc.loc_name))
                    {
                        newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = comp_name + "-" + loc.loc_name + "(" + loc.loc_SAP_account + ")" });
                    }
                    else
                    {
                        newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = comp_name + "-" + loc.loc_city + "(" + loc.loc_SAP_account + ")" });
                    }
                }
            }

            spamodels.List_dist_locations = newlist.OrderBy(a => a.Text);

            return View(spamodels);
        }

        [HttpPost]
        public async Task<string> CreateContract(SPAViewModels sPAViewModels)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "Login";
            }
            sPAViewModels.Activity_status = "3";
            db.SPAViewModels.Add(sPAViewModels);
            db.SaveChanges();

            var user = await comm.GetfullName(userId);
            await LogChanges("CreateContract", user["fullName"] + " has created a new SPA", sPAViewModels.Spa_id, 0);

            var restunedString = new JavaScriptSerializer().Serialize(new { id = sPAViewModels.Spa_id, status = "OK" });
            return restunedString;
        }

        // GET: SPA/Edit/5
        [HttpGet]
        public string GetContract(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "Login";
            }
            if (id == null)
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Get contract information
            SPAViewModels sPAViewModels = db.SPAViewModels.Find(id);

            if (sPAViewModels == null)
            {
                HttpNotFound();
            }
            //Get the name of the sales rep
            sPAViewModels.Sales_rep_user_name = db.SPAAccountManagers.Where(a=>a.email==sPAViewModels.Sales_rep_user).FirstOrDefault().contact_name;

            //Get the products for this contract
            IEnumerable<SPAItemViewModel> sPAItemViewModel = db.SPAItemViewModels.Where(a => a.Form_id == id).OrderByDescending(a => a.Item_id);

            var restunedString = JsonConvert.SerializeObject(new { contractData = sPAViewModels, items = sPAItemViewModel, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<string> UpdateContract(SPAViewModels sPAViewModels)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                RedirectToAction("Login", "Account");
            }

            sPAViewModels.Updated_date = DateTime.Today;
            sPAViewModels.Updated_by = userId;

            db.Entry(sPAViewModels).State = EntityState.Modified;
            db.SaveChanges();

            var user = await comm.GetfullName(userId);
            await LogChanges("UpdateContract", user["fullName"] + " has updated the SPA ", sPAViewModels.Spa_id, 0);

            var restunedString = new JavaScriptSerializer().Serialize(new { id = sPAViewModels.Spa_id, status = "OK" });
            return restunedString;
        }

        [HttpGet]
        public async Task<string> CheckContractId(string ContractId = null, int spaid = 0)
        {
            var restunedString = "";
            long? companyId = Convert.ToInt32(Session["companyId"]);

            var sPAViewModels = await db.SPAViewModels.Where(model => model.Contract_id == ContractId && model.Comp_id == companyId).FirstOrDefaultAsync();
            if (sPAViewModels != null)
            {
                if (spaid != 0)
                {
                    var original_spa_id = await db.SPAViewModels.Where(a => a.Spa_id == spaid).FirstOrDefaultAsync();
                    restunedString = new JavaScriptSerializer().Serialize(new { id = original_spa_id.Contract_id, status = "The contract ID already exists in the system" });
                }
                else
                {
                    restunedString = new JavaScriptSerializer().Serialize(new { status = "The contract ID already exists in the system" });
                }
            }
            else
            {
                restunedString = new JavaScriptSerializer().Serialize(new { id = ContractId, status = "OK" });
            }

            return restunedString;
        }
        #endregion

        #region Add Products Action
        public ActionResult CreateProductinfo()
        {
            ViewBag.Title = "CreateProductinfo";
            List<SPAMaterialMasterViewModel> Spa_skus = db.SPAMaterialMasterViewModels.ToList();
            SPAItemViewModel SPAItemViewModel = new SPAItemViewModel { List_skus = Spa_skus };

            return View(SPAItemViewModel);
        }

        [HttpPost]
        public async Task<string> CreateProduct(SPAItemViewModel sPAItemViewModel)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var restunedString = string.Empty;
            string skus = sPAItemViewModel.Sku;
            string status = "OK";
            var form_id = sPAItemViewModel.Form_id;
            //check for duplicate parts
            var dup_sku = db.SPAItemViewModels.Where(a => a.Sku == skus && a.Form_id == form_id);
            if (dup_sku.Count() != 0)
            {
                status = "duplicate";
            }
            else
            {
                sPAItemViewModel.Date_Created = DateTime.Today;
                sPAItemViewModel.Requested_price = Math.Round(Convert.ToDouble(sPAItemViewModel.Requested_price), 2).ToString();
                sPAItemViewModel.Requested_multiplier = Math.Round(Convert.ToDouble(sPAItemViewModel.Requested_multiplier), 2).ToString();
                db.SPAItemViewModels.Add(sPAItemViewModel);
                db.SaveChanges();

                var user = await comm.GetfullName(userId);
                await LogChanges("CreateProduct", user["fullName"] + " has added a new SKU to the SPA ", sPAItemViewModel.Form_id, 0);

            }

            IEnumerable<SPAItemViewModel> sPAItemViewModels = db.SPAItemViewModels.Where(a => a.Form_id == sPAItemViewModel.Form_id).OrderByDescending(a => a.Item_id);
            restunedString = new JavaScriptSerializer().Serialize(new { id = sPAItemViewModel.Item_id, items = sPAItemViewModels, status = status });

            return restunedString;
        }

        [HttpPost]
        public async Task<string> BulkUploadProducts(int id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var formdata = Request.Files;
            string returnString = "";
            List<FailedItems> failed_skus = new List<FailedItems>();
            List<FailedItems> duplicate_skus = new List<FailedItems>();

            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file);
                    var path = Path.Combine(Server.MapPath("~/attachments/spa/files"), fileName);
                    string ext = Path.GetExtension(fileName);
                    fileContent.SaveAs(path);

                    if (ext == ".csv")
                    {
                        if (System.IO.File.Exists(path))
                        {
                            double req_price = 0.0;
                            double req_multiplier = 0.0;
                            string target_price = string.Empty;
                            //string quantity = string.Empty;
                            var reader = new StreamReader(path);
                            while (!reader.EndOfStream)
                            {
                                if (fileName.Contains("fixed"))
                                {
                                    //Parse Open Ended file
                                    var line = reader.ReadLine();
                                    var columns = line.Split(',');
                                    if (columns[0] != "SKU")
                                    {
                                        string skus = columns[0];
                                        string quantities = columns[1];
                                        string request_multiplier = columns[2];
                                        string request_price = columns[3];
                                        string target_prices = columns[4];
                                        var dup_sku = db.SPAItemViewModels.Where(a => a.Sku == skus && a.Form_id == id);
                                        if (dup_sku.Count() == 0)
                                        {
                                            var spasku = db.SPAMaterialMasterViewModels.Where(m => m.material.Equals(skus)).FirstOrDefault();//Get the SKUs from the sku table
                                            if (spasku != null)
                                            {
                                                if (request_multiplier != "")
                                                {
                                                    //Calculate the requested price if the requested multiplier is given
                                                    req_price = Math.Round(Convert.ToDouble(request_multiplier) * Convert.ToDouble(spasku.list_price), 2);
                                                    req_multiplier = Convert.ToDouble(request_multiplier);
                                                }
                                                else if (request_price != "")
                                                {
                                                    //Calculate the requested multiplier if the requested price is given
                                                    req_multiplier = Math.Round(Convert.ToDouble(request_price) / Convert.ToDouble(spasku.list_price), 2);
                                                    req_price = Convert.ToDouble(request_price);
                                                }

                                                if (target_prices != "")
                                                {
                                                    target_price = target_prices;
                                                }
                                                SPAItemViewModel spa = new SPAItemViewModel();
                                                spa.Product_name = spasku.material_description;
                                                spa.Form_id = id;
                                                spa.Quantity = quantities;
                                                spa.Sku = skus;
                                                spa.Requested_price = req_price.ToString();
                                                spa.Date_Created = DateTime.Today;
                                                spa.Requested_multiplier = req_multiplier.ToString();
                                                spa.Target_price = target_price;

                                                db.SPAItemViewModels.Add(spa);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                failed_skus.Add(new FailedItems { SKU = skus });//Add the skus that were not inserted
                                            }
                                        }
                                        else
                                        {
                                            duplicate_skus.Add(new FailedItems { SKU = skus });//Add the skus that were not inserted for duplicate
                                        }
                                    }
                                }
                                else
                                {
                                    //Parse Fixed Quanity file
                                    var line = reader.ReadLine();
                                    var columns = line.Split(',');
                                    string request_multiplier = columns[1];
                                    string request_price = columns[2];
                                    string target_prices = columns[3];
                                    if (columns[0] != "SKU")
                                    {
                                        string skus = columns[0];
                                        var dup_sku = db.SPAItemViewModels.Where(a => a.Sku == skus && a.Form_id == id);
                                        if (dup_sku.Count() == 0)
                                        {
                                            var spasku = db.SPAMaterialMasterViewModels.Where(m => m.material.Equals(skus)).FirstOrDefault();//Get the SKUs from the sku table
                                            if (spasku != null)
                                            {
                                                if (request_multiplier != "")
                                                {
                                                    //Calculate the requested price if the requested multiplier is given
                                                    req_price = Math.Round(Convert.ToDouble(request_multiplier) * Convert.ToDouble(spasku.list_price), 2);
                                                    req_multiplier = Convert.ToDouble(request_multiplier);
                                                }
                                                else if (request_price != "")
                                                {
                                                    //Calculate the requested multiplier if the requested price is given
                                                    req_multiplier = Math.Round(Convert.ToDouble(request_price) / Convert.ToDouble(spasku.list_price), 2);
                                                    req_price = Convert.ToDouble(request_price);
                                                }

                                                if (target_prices != "")
                                                {
                                                    target_price = target_prices.ToString();
                                                }

                                                SPAItemViewModel spa = new SPAItemViewModel();
                                                spa.Product_name = spasku.material_description;
                                                spa.Form_id = id;
                                                spa.Sku = skus;
                                                spa.Requested_price = req_price.ToString();
                                                spa.Date_Created = DateTime.Today;
                                                spa.Requested_multiplier = req_multiplier.ToString();
                                                spa.Target_price = target_price;

                                                db.SPAItemViewModels.Add(spa);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                failed_skus.Add(new FailedItems { SKU = skus });//Add the skus that were not inserted
                                            }
                                        }
                                        else
                                        {
                                            duplicate_skus.Add(new FailedItems { SKU = skus });//Add the skus that were not inserted for duplicate
                                        }
                                    }
                                }
                            }
                            reader.Close();
                            //Get the products for this contract
                            IEnumerable<SPAItemViewModel> sPAItemViewModel = db.SPAItemViewModels.Where(a => a.Form_id == id).OrderByDescending(a => a.Item_id);
                            returnString = JsonConvert.SerializeObject(new { items = sPAItemViewModel, failedItems = failed_skus, duplicates = duplicate_skus, status = "OK" });

                        }
                        else
                        {
                            returnString = "could not find file try again";
                        }
                    }
                    else
                    {
                        returnString = "Please download and use the template";
                    }
                }
            }

            var user = await comm.GetfullName(userId);
            await LogChanges("BulkUploadProducts", user["fullName"] + " has uploaded multiple SKUs to the SPA", id, 0);

            return returnString;
        }

        // GET: SPA/Edit/5
        public async Task<string> GetProduts(int? id)
        {
            if (id == null)
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAItemViewModel sPAItemViewModels = db.SPAItemViewModels.Find(id);
            if (sPAItemViewModels == null)
            {
                HttpNotFound();
            }
            string sku_code = sPAItemViewModels.Sku;
            int spa_id = sPAItemViewModels.Form_id;
            SPAMaterialMasterViewModel material_master = await db.SPAMaterialMasterViewModels.Where(m => m.material == sku_code).FirstOrDefaultAsync();
            //Get the pricing group
            var distributor_location = db.SPAViewModels.Find(spa_id).Distributor_location;
            int dis_loc = Convert.ToInt32(distributor_location);
            var pricing_group = db.partnerLocationViewModels.Find(dis_loc).price_group;
            //Get into Stock multiplier
            var getmultiplier = await GetMultiplier(pricing_group, material_master.mpg);
            string IntoStockMultiplier = getmultiplier.Split(',')[1];
            //Calculate the Into Stock Price
            Single IntoStockPrice = getIntoStockPrice(Convert.ToDouble(IntoStockMultiplier), Convert.ToDouble(material_master.list_price));

            SkusViewModel skusViewModels = new SkusViewModel();
            skusViewModels.Sku_code = material_master.material;
            skusViewModels.Description = material_master.material_description;
            skusViewModels.ListPrice = Convert.ToSingle(material_master.list_price);
            skusViewModels.Cost = Convert.ToSingle(material_master.cost);
            skusViewModels.IntoStockMultiplier = Convert.ToSingle(IntoStockMultiplier);
            skusViewModels.IntoStockPrice = IntoStockPrice;

            var restunedString = JsonConvert.SerializeObject(new { item = sPAItemViewModels, sku = skusViewModels, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public async Task<string> UpdateProduct(SPAItemViewModel sPAItemViewModel)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            sPAItemViewModel.Requested_price = Math.Round(Convert.ToDouble(sPAItemViewModel.Requested_price), 2).ToString();
            sPAItemViewModel.Requested_multiplier = Math.Round(Convert.ToDouble(sPAItemViewModel.Requested_multiplier), 2).ToString();
            db.Entry(sPAItemViewModel).State = EntityState.Modified;
            db.SaveChanges();

            var user = await comm.GetfullName(userId);
            await LogChanges("UpdateProduct", user["fullName"] + " has updated the sku " + sPAItemViewModel.Sku + " attached to the SPA ", sPAItemViewModel.Form_id, 0);

            IEnumerable<SPAItemViewModel> sPAItemViewModels = db.SPAItemViewModels.Where(a => a.Form_id == sPAItemViewModel.Form_id).OrderByDescending(a => a.Item_id);
            var restunedString = new JavaScriptSerializer().Serialize(new { id = sPAItemViewModel.Item_id, items = sPAItemViewModels, status = "OK" });

            return restunedString;
        }

        public async Task<string> DeleteProduct(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var restunedString = "";
            if (id == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "bad request" });
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAItemViewModel sPAItemViewModels = db.SPAItemViewModels.Find(id);
            if (sPAItemViewModels == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "Not found" });
                var notfound = HttpNotFound();
            }

            db.SPAItemViewModels.Remove(sPAItemViewModels);
            db.SaveChanges();

            var user = await comm.GetfullName(userId);
            await LogChanges("DeleteProduct", user["fullName"] + " has deteled the sku " + sPAItemViewModels.Sku + " from the SPA ", sPAItemViewModels.Form_id, 0);

            return restunedString = JsonConvert.SerializeObject(new { status = "OK" });
        }
        #endregion

        #region Get Sku Data
        // GET: SPA/Edit/5
        [HttpGet]
        public async Task<string> GetSKUData(int? id = 0, int spa_id = 0)
        {
            if (id == null || id == 0)
            {
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Get the pricing group
            var distributor_location = db.SPAViewModels.Find(spa_id).Distributor_location;
            int dis_loc = Convert.ToInt32(distributor_location);
            var pricing_group = db.partnerLocationViewModels.Find(dis_loc).price_group;
            SPAMaterialMasterViewModel material_master = db.SPAMaterialMasterViewModels.Find(id);
            var getmultiplier = await GetMultiplier(pricing_group, material_master.mpg);
            string IntoStockMultiplier = getmultiplier.Split(',')[1];
            //Calculate the Into Stock Price
            Single IntoStockPrice = getIntoStockPrice(Convert.ToDouble(IntoStockMultiplier), Convert.ToDouble(material_master.list_price));

            SkusViewModel skusViewModels = new SkusViewModel();
            skusViewModels.Sku_code = material_master.material;
            skusViewModels.Description = material_master.material_description;
            skusViewModels.ListPrice = Convert.ToSingle(material_master.list_price);
            skusViewModels.Cost = Convert.ToSingle(material_master.cost);
            skusViewModels.IntoStockMultiplier = Convert.ToSingle(IntoStockMultiplier);
            skusViewModels.IntoStockPrice = IntoStockPrice;

            var restunedString = JsonConvert.SerializeObject(new { skuData = skusViewModels, status = ModelState });
            return restunedString;
        }

        // GET: SPA/Edit/5
        [HttpGet]
        public async Task<string> searchGetSKUData(string id = null)
        {
            List<SPAMaterialMasterViewModel> Spa_skus = await db.SPAMaterialMasterViewModels.Where(a=>a.material.Contains(id)).ToListAsync();

            var restunedString = JsonConvert.SerializeObject(new { Spa_skus = Spa_skus, status = ModelState });
            return restunedString;
        }

        public Single getIntoStockPrice(double IntoStockMultiplier, double list_price)
        {
            Single intostockprice = Convert.ToSingle(Math.Round(IntoStockMultiplier * list_price, 2));

            return intostockprice;
        }

        public async Task<string> GetMultiplier(string pricing_group = null, string mpg = null)
        {
            string multiplier = string.Empty;
            if (pricing_group == "GG")
            {
                var GetMultiplier = await db.SPAIntostockMultiplierViewModels.Where(m => m.GG.Contains(mpg)).FirstOrDefaultAsync();
                multiplier = GetMultiplier.GG;
            }
            else if (pricing_group == "PG")
            {
                var GetMultiplier = await db.SPAIntostockMultiplierViewModels.Where(m => m.PG.Contains(mpg)).FirstOrDefaultAsync();
                multiplier = GetMultiplier.PG;
            }
            else if (pricing_group == "AA")
            {
                var GetMultiplier = await db.SPAIntostockMultiplierViewModels.Where(m => m.AA.Contains(mpg)).FirstOrDefaultAsync();
                multiplier = GetMultiplier.AA;
            }
            else if (pricing_group == "CC")
            {
                var GetMultiplier = await db.SPAIntostockMultiplierViewModels.Where(m => m.CC.Contains(mpg)).FirstOrDefaultAsync();
                multiplier = GetMultiplier.CC;
            }

            return multiplier;
        }
        #endregion

        #region Contract Participants Action
        public async Task<ActionResult> CreateParticipantsinfo()
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int usr_SPA = Convert.ToInt32(Session["usr_SPA"]);//Regional Manager
            int usr_POS = Convert.ToInt32(Session["usr_POS"]);//Account Manager
            string region = Convert.ToString(Session["comp_region"]);
            string comp_name = Convert.ToString(Session["comp_name"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            SPAViewModels spamodels = new SPAViewModels();
            List<partnerCompanyViewModel> companies = await GetDistributors(region);
            List<partnerLocationViewModel> locations = await db.partnerLocationViewModels.ToListAsync();
            List<SelectListItem> newlist = new List<SelectListItem>();
            newlist.Add(new SelectListItem { Value = "", Text = "---Please select a distributor---" });

            if (User.IsInRole("Super Admin") || User.IsInRole("Global Admin"))
            {
                foreach (var company in companies)
                {
                    var comp_locations = locations.Where(a => a.comp_ID == company.comp_ID);
                    foreach (var loc in comp_locations)
                    {
                        if (!string.IsNullOrEmpty(loc.loc_name))
                        {
                            newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = company.comp_name + "-" + loc.loc_name + "(" + loc.loc_SAP_account + ")" });
                        }
                        else
                        {
                            newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = company.comp_name + "-" + loc.loc_city + "(" + loc.loc_SAP_account + ")" });
                        }
                    }
                }
            }
            else
            {
                var comp_locations = locations.Where(a => a.comp_ID == companyId);
                foreach (var loc in comp_locations)
                {
                    if (!string.IsNullOrEmpty(loc.loc_name))
                    {
                        newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = comp_name + "-" + loc.loc_name + "(" + loc.loc_SAP_account + ")" });
                    }
                    else
                    {
                        newlist.Add(new SelectListItem { Value = loc.loc_ID.ToString(), Text = comp_name + "-" + loc.loc_city + "(" + loc.loc_SAP_account + ")" });
                    }
                }
            }

            spamodels.List_dist_locations = newlist;

            return View(spamodels);
        }

        [HttpGet]
        public async Task<string> GetParticipants(int? id)
        {
            var restunedString = "no results";
            string Roles = "Sales Rep";
            List<SPASalesRepsViewModel> Sales_reps = new List<SPASalesRepsViewModel>();
            SPAViewModels sPAViewModels = db.SPAViewModels.Find(id);
            if (sPAViewModels == null)
            {
                return restunedString;
            }

            //Get Owner Data
            var ownerUser = await comm.GetfullName(Convert.ToInt32(sPAViewModels.Usr_id));
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = ownerUser["fullName"], Email = ownerUser["email"], Roles = "Owner" });

            //Get the name of the default sales rep
            var Sales_rep_user_name = db.SPAAccountManagers.ToList();
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = Sales_rep_user_name.Where(a => a.email == sPAViewModels.Sales_rep_user).FirstOrDefault().contact_name, Email = sPAViewModels.Sales_rep_user, Roles = "Default Participant" });
            //Get Users of the RiSouce Center
            var user_data = db.UserViewModels.ToList();
            //List Participants from SPA_SalesReps Table
            var Spa_sales_reps = db.SPA_SalesRepsViewModels.Where(a => a.Form_id == id);
            foreach (var reps in Spa_sales_reps)
            {
                var get_user = user_data.Where(a => a.usr_email == reps.Usr_id).FirstOrDefault();
                Sales_reps.Add(new SPASalesRepsViewModel { Rep_id = reps.Rep_id, FullName = get_user.usr_fName + " " + get_user.usr_lName, Email = reps.Usr_id, Roles = "Added Participant" });
            }

            restunedString = new JavaScriptSerializer().Serialize(new { id = sPAViewModels.Spa_id, zip = sPAViewModels.Customer_zip, reps = Sales_reps, status = "OK" });

            return restunedString;
        }

        public string GetSalesReps(string zip, string testing = null)
        {
            if (testing == null)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    RedirectToAction("Login", "Account");
                }
            }
            string territory_code = "";
            List<SPAAccountManager> SPAAccountManage = new List<SPAAccountManager>();
            List<SPATerritoryCode> spaterritorycode = db.SPATerritoryCodes.Where(a => a.zip == zip).ToList();
            if (spaterritorycode.Count() != 0)
            {
                territory_code = spaterritorycode.FirstOrDefault().territory_code;
                var accountmanagers = db.SPAAccountManagers.Where(a => a.territory_code == territory_code);
                foreach (var item in accountmanagers.GroupBy(a => new { a.contact_name, a.email }))
                {
                    SPAAccountManage.Add(new SPAAccountManager { email = item.Key.email, contact_name = item.Key.contact_name });
                }
            }

            var restunedString = JsonConvert.SerializeObject(new { territory_code = territory_code, spaaccountmanager = SPAAccountManage, status = "OK" });

            return restunedString;
        }

        public string GetSalesRepsParticipants(string querystring = null, int spaid = 0, string testing = null)
        {
            List<Compids> list_comp = new List<Compids>();
            List<Compids> list_comp_us = new List<Compids>();
            List<SPAAccountManager> SPAAccountManage = new List<SPAAccountManager>();
            List<UserViewModel> user_data;
            if (testing == null)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    RedirectToAction("Login", "Account");
                }
            }

            //List US companies
            list_comp_us.Add(new Compids{ comp_ids ="312"});
            list_comp_us.Add(new Compids { comp_ids = "407" });
            list_comp_us.Add(new Compids { comp_ids = "408" });
            list_comp_us.Add(new Compids { comp_ids = "409" });
            list_comp_us.Add(new Compids { comp_ids = "410" });
            list_comp_us.Add(new Compids { comp_ids = "411" });
            list_comp_us.Add(new Compids { comp_ids = "412" });
            list_comp_us.Add(new Compids { comp_ids = "413" });
            list_comp_us.Add(new Compids { comp_ids = "477" });
            list_comp_us.Add(new Compids { comp_ids = "30629" });
            list_comp_us.Add(new Compids { comp_ids = "30630" });

            //Get the company id of distributor
            List<SPAViewModels> GetSPA = db.SPAViewModels.Where(a => a.Spa_id == spaid).ToList();
            int comp_id = Convert.ToInt32(GetSPA.FirstOrDefault().Comp_id);
            list_comp.Add(new Compids { comp_ids = comp_id.ToString() });
            //Get Rittal and Man Rep Companies
            List<partnerCompanyViewModel> company_ids = db.partnerCompanyViewModels.Where(a => a.comp_type == 5 || a.comp_type == 3).ToList();
            //Get users
            if (querystring == null)
            {
                user_data = db.UserViewModels.ToList();
            }
            else
            {
                user_data = db.UserViewModels.Where(a => a.usr_email.Contains(querystring) || a.usr_fName.Contains(querystring) || a.usr_lName.Contains(querystring)).ToList();
            }

            foreach (var items in company_ids)
            {
                if (list_comp_us.Select(a => a.comp_ids).Contains(items.comp_ID.ToString()))
                {
                    list_comp.Add(new Compids { comp_ids = items.comp_ID.ToString() });
                }
            }

            foreach (var user in user_data)
            {
                if (list_comp.Select(a => a.comp_ids).Contains(user.comp_ID.ToString()))
                {
                    SPAAccountManage.Add(new SPAAccountManager { email = user.usr_email, contact_name = user.usr_fName + " " + user.usr_lName });
                }
            }

            var restunedString = JsonConvert.SerializeObject(new { spaaccountmanager = SPAAccountManage.OrderBy(a => a.contact_name), querystring = querystring, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public async Task<string> AddParticipant(SPASalesRepsViewModel salesrep)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            db.SPA_SalesRepsViewModels.Add(salesrep);
            db.SaveChanges();

            string Roles = string.Empty;
            var restunedString = "no results";
            List<SPASalesRepsViewModel> Sales_reps = new List<SPASalesRepsViewModel>();
            SPAViewModels sPAViewModels = db.SPAViewModels.Find(salesrep.Form_id);
            if (sPAViewModels == null)
            {
                return restunedString;
            }

            //Get Owner Data
            var ownerUser = await comm.GetfullName(Convert.ToInt32(sPAViewModels.Usr_id));
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = ownerUser["fullName"], Email = ownerUser["email"], Roles = "Owner" });

            //Get the name of the sales rep
            var Sales_rep_user_name = db.SPAAccountManagers.ToList();
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = Sales_rep_user_name.Where(a => a.email == sPAViewModels.Sales_rep_user).FirstOrDefault().contact_name, Email = sPAViewModels.Sales_rep_user, Roles = "Default Participant" });

            //Get Users of the RiSouce Center
            var user_data = db.UserViewModels.ToList();
            //List Participants from SPA_SalesReps Table
            var Spa_sales_reps = db.SPA_SalesRepsViewModels.Where(a => a.Form_id == salesrep.Form_id);
            foreach (var reps in Spa_sales_reps)
            {
                var get_user = user_data.Where(a => a.usr_email == reps.Usr_id).FirstOrDefault();
                Sales_reps.Add(new SPASalesRepsViewModel { Rep_id = reps.Rep_id, FullName = get_user.usr_fName + " " + get_user.usr_lName, Email = reps.Usr_id, Roles = "Added Participant" });
            }

            var user = await comm.GetfullName(userId);
            await LogChanges("AddParticipant", user["fullName"] + " has added a participant to the SPA ", Convert.ToInt32(salesrep.Form_id), 0);

            restunedString = JsonConvert.SerializeObject(new { id = salesrep.Rep_id, zip = sPAViewModels.Customer_zip, salesrepData = Sales_reps, status = "OK" });

            return restunedString;
        }

        public async Task<string> DeleteParticipant(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var restunedString = "";
            if (id == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "bad request" });
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPASalesRepsViewModel sPASalesViewModels = db.SPA_SalesRepsViewModels.Find(id);
            if (sPASalesViewModels == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "Not found" });
                var notfound = HttpNotFound();
            }
            db.SPA_SalesRepsViewModels.Remove(sPASalesViewModels);
            db.SaveChanges();

            var user = await comm.GetfullName(userId);
            await LogChanges("DeleteParticipant", user["fullName"] + " has deleted a participant SPA ", Convert.ToInt32(sPASalesViewModels.Form_id), 0);

            return restunedString = JsonConvert.SerializeObject(new { status = "OK" });
        }
        #endregion

        #region Files Upload
        public ActionResult UploadFiles()
        {
            return View();
        }

        public async Task<string> GetSPAFiles(int? id)
        {
            IEnumerable<SPA_FIlesViewModel> SPA_FIles = await db.SPA_FIlesViewModels.Where(a => a.Form_id == id).ToListAsync();

            string returnString = new JavaScriptSerializer().Serialize(new { spafiles = SPA_FIles.OrderByDescending(a => a.File_id), status = "OK" });

            return returnString;
        }

        [HttpPost]
        public async Task<string> AddSPAFiles(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var formdata = Request.Files;
            List<SPA_FIlesViewModel> SPA_FIles = new List<SPA_FIlesViewModel>();

            foreach (string file in Request.Files)
            {

                var fileContent = Request.Files[file];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file);
                    var path = Path.Combine(Server.MapPath("~/attachments/spa/contracts"), fileName);
                    string ext = Path.GetExtension(fileName);
                    fileContent.SaveAs(path);

                    //if file_id is not set or the entry is empty
                    SPA_FIles.Add(new SPA_FIlesViewModel
                    {
                        Form_id = id,
                        File_name = file,
                        File_ext = ext
                    });
                }
            }

            db.SPA_FIlesViewModels.AddRange(SPA_FIles);
            await db.SaveChangesAsync();

            IQueryable<SPA_FIlesViewModel> SPA_FIlesViewModel = db.SPA_FIlesViewModels.Where(model => model.Form_id == id);


            var user = await comm.GetfullName(userId);
            await LogChanges("AddSPAFiles", user["fullName"] + " has attached a file(s) to the SPA", Convert.ToInt32(id), 0);

            string returnString = new JavaScriptSerializer().Serialize(new { spafiles = SPA_FIlesViewModel.OrderByDescending(a => a.File_id), status = "OK" });

            return returnString;
        }

        public async Task<string> DeleteSPAFile(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            var restunedString = "";
            if (id == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "bad request" });
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPA_FIlesViewModel SPA_FIlesViewModels = db.SPA_FIlesViewModels.Find(id);
            if (SPA_FIlesViewModels == null)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "Not found" });
                var notfound = HttpNotFound();
            }

            db.SPA_FIlesViewModels.Remove(SPA_FIlesViewModels);
            db.SaveChanges();

            //Delete the file from the system 
            string sourcePath = string.Format("~/attachments/spa/contracts/{0}", @SPA_FIlesViewModels.File_name);
            if (System.IO.File.Exists(Server.MapPath(sourcePath)))
            {
                var path = Server.MapPath(sourcePath);
                System.IO.File.Delete(path);
            }

            var user = await comm.GetfullName(userId);
            await LogChanges("DeleteSPAFile", user["fullName"] + " has deleted a file from the SPA", Convert.ToInt32(id), 0);

            return restunedString = JsonConvert.SerializeObject(new { status = "OK" });
        }
        #endregion

        #region Create actions
        // GET: SPA/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            long? companyId = Convert.ToInt32(Session["companyId"]);
            int usr_SPA = Convert.ToInt32(Session["usr_SPA"]);//Regional Manager
            int usr_POS = Convert.ToInt32(Session["usr_POS"]);//Account Manager
            string region = Convert.ToString(Session["comp_region"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        #endregion

        #region Details Action
        public async Task<ActionResult> Details(int id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            string Roles = string.Empty;
            List<SPASalesRepsViewModel> Sales_reps = new List<SPASalesRepsViewModel>();
            List<SPANotesViewModels> getNotes = new List<SPANotesViewModels>();
            string companyId = Convert.ToString(Session["companyId"]);
            string username = User.Identity.Name;

            SPAViewModels GetSPA = db.SPAViewModels.Find(id);
            if (GetSPA.Contract_type == "1")
            {
                GetSPA.Contract_type = "Fixed Quantity";
            }
            else
            {
                GetSPA.Contract_type = "Open Ended";
            }
            int loc_id = Convert.ToInt32(GetSPA.Distributor_location);
            //Get Distributor location data
            var locationData = await db.partnerLocationViewModels.Join(
                                    db.partnerCompanyViewModels,
                                    loc => loc.comp_ID,
                                    comp => comp.comp_ID,
                                    (loc, comp) => new { loc, comp }
                                  ).Where(a => a.loc.loc_ID == loc_id).FirstOrDefaultAsync();

            if (locationData.comp.comp_name == "")
            {
                GetSPA.Distributor_location = locationData.loc.loc_city + "-(" + locationData.loc.loc_SAP_account + ")";
            }
            else
            {
                GetSPA.Distributor_location = locationData.comp.comp_name + "-" + locationData.loc.loc_city + "-(" + locationData.loc.loc_SAP_account + ")";
            }

            int stateid = Convert.ToInt32(locationData.loc.loc_state);
            var state = await Entitydb.data_state.Where(a => a.stateid == stateid).FirstOrDefaultAsync();
            SAPDistributor get_distributors = new SAPDistributor
            {
                company_name = locationData.comp.comp_name,
                address = locationData.loc.loc_add1,
                phone = locationData.loc.loc_phone,
                city = locationData.loc.loc_city,
                state = state.state_long,
                zip = locationData.loc.loc_zip
            };

            //Get Updated by
            if (GetSPA.Updated_by != null)
            {
                var UpdatedUser = await comm.GetfullName(Convert.ToInt32(GetSPA.Updated_by));
                GetSPA.Updated_By_Name = UpdatedUser["fullName"];
            }

            //Get Owner Data
            var ownerUser = await comm.GetfullName(Convert.ToInt32(GetSPA.Usr_id));
            ViewBag.email = ownerUser["email"];
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = ownerUser["fullName"], Email = ownerUser["email"], Roles = "Owner" });

            //Get the name of the sales rep
            var Sales_rep_user_name = db.SPAAccountManagers.ToList();
            Sales_reps.Add(new SPASalesRepsViewModel { FullName = Sales_rep_user_name.Where(a => a.email == GetSPA.Sales_rep_user).FirstOrDefault().contact_name, Email = GetSPA.Sales_rep_user, Roles = "Default Participant" });
            //Get user
            var user_data = db.UserViewModels.ToList();
            //List Participants from SPA_SalesReps Table
            var Spa_sales_reps = db.SPA_SalesRepsViewModels.Where(a => a.Form_id == id);
            foreach (var reps in Spa_sales_reps)
            {
                var get_user = user_data.Where(a => a.usr_email == reps.Usr_id).FirstOrDefault();
                Sales_reps.Add(new SPASalesRepsViewModel { Rep_id = reps.Rep_id, FullName = get_user.usr_fName + " " + get_user.usr_lName, Email = reps.Usr_id, Roles = "Added Participant" });
            }

            IEnumerable<SPAItemViewModel> GetSPA_Items = await db.SPAItemViewModels.Where(model => model.Form_id == id).ToListAsync();
            ViewBag.expireditems = GetSPA_Items.Where(model => model.Item_Status == 0 && model.Date_Updated != null).Count();
            ViewBag.current = GetSPA_Items.Where(model => model.Item_Status == 1).Count();
            ViewBag.declined = GetSPA_Items.Where(model => model.Item_Status == 2).Count();
            ViewBag.pending = GetSPA_Items.Where(model => model.Item_Status == 0 && model.Date_Updated == null).Count();

            IEnumerable<SPA_FIlesViewModel> GetSPAFiles = await db.SPA_FIlesViewModels.Where(model => model.Form_id == id).ToListAsync();

            var note_type = InnternalExternal(id, companyId, username);

            //Determine if user is external or internal
            if (note_type == "true")
            {
                //user is internal
                getNotes = await db.SPANotesViewModels.Where(a => a.Form_ID == id).ToListAsync();
                foreach (var item in getNotes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                }

                ViewBag.filter = "1";
            }
            else if (note_type == "false")
            {
                //user is external
                getNotes = await db.SPANotesViewModels.Where(a => a.Note_Type == false && a.Form_ID == id).ToListAsync();
                foreach (var item in getNotes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                }

                ViewBag.filter = "0";
            }

           var SPADetailsModel = new SPADetailsModel()
            {
                Get_SPAs = GetSPA,
                List_SPA_Participants = Sales_reps,
                List_SPA_Files = GetSPAFiles,
                List_SPA_Distributor = get_distributors,
                List_Notes = getNotes.OrderByDescending(a => a.Action_date)
           };

            return View(SPADetailsModel);
        }

        [HttpPost]
        public async Task<string> GetItems(int type = 0, int id = 0, string status = null)
        {
            var restunedString = "";
            List<SPAItemViewModel> List_items = new List<SPAItemViewModel>();
            if (id == 0)
            {
                restunedString = JsonConvert.SerializeObject(new { status = "bad request" });
                new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Get the SPA
            SPAViewModels GetSPA = db.SPAViewModels.Find(id);
            int loc_id = Convert.ToInt32(GetSPA.Distributor_location);
            //Get Distributor location data
            var locationData = await db.partnerLocationViewModels.Join(
                                    db.partnerCompanyViewModels,
                                    loc => loc.comp_ID,
                                    comp => comp.comp_ID,
                                    (loc, comp) => new { loc, comp }
                                  ).Where(a => a.loc.loc_ID == loc_id).FirstOrDefaultAsync();

            var price_group = locationData.loc.price_group;

            if (status == "expired")
            {
                var List_item = await db.SPAItemViewModels.Where(model => model.Form_id == id && model.Date_Updated != null).ToListAsync();
                foreach (var item in List_item)
                {
                    var getSku = await db.SPAMaterialMasterViewModels.Where(a => a.material == item.Sku).FirstOrDefaultAsync();
                    var getmultiplier = await GetMultiplier(price_group, getSku.mpg);
                    string intostockmultiplier = getmultiplier.Split(',')[1];
                    var intostockprice = getIntoStockPrice(Convert.ToDouble(intostockmultiplier), Convert.ToDouble(getSku.list_price));
                    string dateCreated = string.Format("{0:MM/dd/yyyy}", item.Date_Created);
                    string dateupdated = string.Format("{0:MM/dd/yyyy}", item.Date_Updated);
                    var Cost = Convert.ToSingle(getSku.cost);
                    List_items.Add(new SPAItemViewModel { List_price = Convert.ToSingle(getSku.list_price), Instock_multiplier = Convert.ToSingle(intostockmultiplier), Instock_price = Convert.ToSingle(intostockprice), Item_id = item.Item_id, Form_id = item.Form_id, Sku = item.Sku, Product_name = item.Product_name, Requested_multiplier = item.Requested_multiplier, Requested_price = item.Requested_price, Update_date = dateupdated, Created_Update = dateCreated, Quantity = item.Quantity, Target_price = item.Target_price,Cost = Cost });
                }
            }
            else
            {
                var List_item = await db.SPAItemViewModels.Where(model => model.Form_id == id && model.Item_Status == type && model.Date_Updated == null).ToListAsync();
                foreach (var item in List_item)
                {
                    var getSku = await db.SPAMaterialMasterViewModels.Where(a => a.material == item.Sku).FirstOrDefaultAsync();
                    var getmultiplier = await GetMultiplier(price_group, getSku.mpg);
                    string intostockmultiplier = getmultiplier.Split(',')[1];
                    var intostockprice = getIntoStockPrice(Convert.ToDouble(intostockmultiplier), Convert.ToDouble(getSku.list_price));
                    string dateCreated = string.Format("{0:MM/dd/yyyy}", item.Date_Created);
                    string dateupdated = string.Format("{0:MM/dd/yyyy}", item.Date_Updated);
                    var Cost = Convert.ToSingle(getSku.cost);
                    List_items.Add(new SPAItemViewModel { List_price = Convert.ToSingle(getSku.list_price), Instock_multiplier = Convert.ToSingle(intostockmultiplier), Instock_price = Convert.ToSingle(intostockprice), Item_id = item.Item_id, Form_id = item.Form_id, Sku = item.Sku, Product_name = item.Product_name, Requested_multiplier = item.Requested_multiplier, Requested_price = item.Requested_price, Update_date = dateupdated, Created_Update = dateCreated, Quantity = item.Quantity, Target_price = item.Target_price,Cost = Cost });
                }
            }

            return restunedString = JsonConvert.SerializeObject(new { listitems = List_items, status = "OK" });
        }

        public async Task UpdateItems(int item_id, int status = 0, string quantity = null, string requested_price = null, string requested_multiplier = null, string target_price = null)
        {
            int userId = Convert.ToInt32(Session["userId"]);

            var List_item = await db.SPAItemViewModels.Where(model => model.Item_id == item_id).FirstOrDefaultAsync();

            List_item.Quantity = quantity;
            List_item.Requested_price = Math.Round(Convert.ToDouble(requested_price), 2).ToString();
            List_item.Requested_multiplier = Math.Round(Convert.ToDouble(requested_multiplier), 2).ToString();
            List_item.Item_Status = status;
            List_item.Target_price = target_price;

            await db.SaveChangesAsync();

            var user = await comm.GetfullName(userId);
            await LogChanges("UpdateItemsDetails", user["fullName"] + " has updated the sku data on the details page", 0, 0);
        }
        #endregion

        #region SPA Notes
        [HttpGet]
        public async Task<ActionResult> SpaNotes()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public async Task<string> GetSpaNotes(int spaid = 0)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            List<SPANotesViewModels> getNotes = new List<SPANotesViewModels>();
            string companyId = Convert.ToString(Session["companyId"]);
            string username = User.Identity.Name;
            var note_type = InnternalExternal(spaid, companyId, username);

            //Determine if user is external or internal
            if (note_type == "true")
            {
                //user is internal
                getNotes = await db.SPANotesViewModels.Where(a => a.Form_ID == spaid).ToListAsync();
                foreach (var item in getNotes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID == userId)
                    {
                        item.Can_delete = true;
                    }
                }
            }
            else if (note_type == "false")
            {
                //user is external
                getNotes = await db.SPANotesViewModels.Where(a => a.Note_Type == false && a.Form_ID == spaid).ToListAsync();
                foreach (var item in getNotes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID == userId)
                    {
                        item.Can_delete = true;
                    }
                }
            }

            string restunedString = new JavaScriptSerializer().Serialize(new { id = spaid, getNotes = getNotes.OrderByDescending(a => a.ID), notetype = note_type, status = "OK" });

            return restunedString;
        }

        public string InnternalExternal (int spaid = 0, string companyId = null, string username = null)
        {
            string internal_user="";//default user is external user      
            List<Compids> list_participants_email = new List<Compids>();
            List<Compids> list_comp_rittal = new List<Compids>();

            //Get the participants from spa owner and default participant 
            List<SPAViewModels> GetSPA = db.SPAViewModels.Where(a => a.Spa_id == spaid).ToList();
            string default_participant_email = GetSPA.FirstOrDefault().Sales_rep_user;//default participant
            list_participants_email.Add(new Compids { email=default_participant_email });
            int user_ID = Convert.ToInt32(GetSPA.FirstOrDefault().Usr_id);//owner of spa user id
            List<UserViewModel> users = db.UserViewModels.Where(a=>a.usr_ID==user_ID).ToList();
            var usr_email = users.FirstOrDefault().usr_email;//get spa owners email
            list_participants_email.Add(new Compids { email = usr_email });
            //Get Rittal company ids
            List<partnerCompanyViewModel> company_ids = db.partnerCompanyViewModels.Where(a => a.comp_type == 5).ToList();
            foreach (var items in company_ids)
            {
                list_comp_rittal.Add(new Compids { comp_ids = items.comp_ID.ToString() });
            }

            //Determine if a user is a participant            
            var participants = db.SPA_SalesRepsViewModels.Where(a => a.Usr_id == username);
            foreach (var participant in participants)
            {
                list_participants_email.Add(new Compids { email = participant.Usr_id });
            }

            //Determine if users can edit notes
            if (list_comp_rittal.Select(a => a.comp_ids).Contains(companyId) || list_participants_email.Select(a => a.email).Contains(username))
            {
                //Determine if user is external or internal
                if (list_comp_rittal.Select(a => a.comp_ids).Contains(companyId))
                {
                    //user is internal
                    internal_user = "true";
                }
                else
                {
                    //user is external
                    internal_user = "false";
                }
            }

            return internal_user;
        }

        public async Task<string> SaveNotes(SPANotesViewModels spa_notesviewmodels)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                RedirectToAction("Login", "Account");
            }
            var user = await comm.GetfullName(userId);
            List<SPANotesViewModels> notes = null;
            spa_notesviewmodels.Action_Time = DateTime.Today;
            spa_notesviewmodels.User_ID = userId;

            db.SPANotesViewModels.Add(spa_notesviewmodels);
            db.SaveChanges();

            var spaid = spa_notesviewmodels.Form_ID;

            if (spa_notesviewmodels.Note_Type == true)
            {
                notes = await db.SPANotesViewModels.Where(a => a.Form_ID == spaid).ToListAsync();
                foreach (var item in notes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID==userId)
                    {
                        item.Can_delete = true;
                    }
                }

                await LogChanges("SaveNotes", user["fullName"] + " has written an internal note on the SPA",spaid,0);
            }
            else if (spa_notesviewmodels.Note_Type == false)
            {
                notes = await db.SPANotesViewModels.Where(a => a.Note_Type == false && a.Form_ID==spaid).ToListAsync();
                foreach (var item in notes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID == userId)
                    {
                        item.Can_delete = true;
                    }
                }

                await LogChanges("SaveNotes", user["fullName"] + " has written an external note on the SPA", spaid, 0);
            }

            var restunedString = new JavaScriptSerializer().Serialize(new { spaid = spa_notesviewmodels.Form_ID,notes = notes.OrderByDescending(a=>a.ID), status = "OK" });

            return restunedString;
        }

        async Task LogChanges(string action = null, string note = null, int spaid=0, int user=0)
        {
            SPANotesViewModels notes = new SPANotesViewModels();
            notes.Action = action;
            notes.Form_ID = spaid;
            notes.User_ID = user;
            notes.Note = note;
            notes.Note_Type = true;
            notes.Action_Time = DateTime.Today;

            db.SPANotesViewModels.Add(notes);
            await db.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<string> filterNotes(string filter, int spaid)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            List<SPANotesViewModels> notes = null;

            if (filter == "1")
            {
                notes = await db.SPANotesViewModels.Where(a => a.Note_Type == true && a.Form_ID == spaid).ToListAsync();

                foreach (var item in notes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID == userId)
                    {
                        item.Can_delete = true;
                    }
                }
            }
            else if (filter == "0")
            {
                notes = await db.SPANotesViewModels.Where(a => a.Note_Type == false && a.Form_ID == spaid).ToListAsync();
                foreach (var item in notes)
                {
                    item.Action_date = string.Format("{0:MM/dd/yyyy}", item.Action_Time);
                    if (item.User_ID == userId)
                    {
                        item.Can_delete = true;
                    }
                }
            }

            var restunedString = new JavaScriptSerializer().Serialize(new { spaid = spaid, notes = notes.OrderByDescending(a => a.ID), status = "OK" });

            return restunedString;

        }

        public async Task<string> DeleteSPANote(int id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return "not loggedin";
            }

            SPANotesViewModels spanote = db.SPANotesViewModels.Find(id);
            if (spanote!=null)
            {
                db.SPANotesViewModels.Remove(spanote);
                db.SaveChanges();
            }

            var user = await comm.GetfullName(userId);
            await LogChanges("DeleteSPANote", user["fullName"] + " has deleteed a note from the SPA ", spanote.Form_ID, 0);

            return "deleted";
        }
        #endregion

        #region Delete Actions
        // GET: SPA/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Get the installed parts data
            var removeSPAs = db.SPAItemViewModels.Where(a => a.Form_id == id);
            if (removeSPAs != null)
            {
                db.SPAItemViewModels.RemoveRange(removeSPAs);
            }

            SPAViewModels sPAViewModels = db.SPAViewModels.Find(id);
            db.SPAViewModels.Remove(sPAViewModels);
            db.SaveChanges();


            var user = await comm.GetfullName(userId);
            await LogChanges("Delete", user["fullName"] + " has deleteed a note from the SPA", id, 0);

            if (Request.QueryString["isAdmin"]=="yes")
            {
                return RedirectToAction("SPAadmin", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"],msg = Request.QueryString["msg"], success = "The SPA has been deleted" });
            }

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The SPA has been deleted" });
        }

        // POST: SPA/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            SPAViewModels sPAViewModels = db.SPAViewModels.Find(id);
            db.SPAViewModels.Remove(sPAViewModels);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion

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

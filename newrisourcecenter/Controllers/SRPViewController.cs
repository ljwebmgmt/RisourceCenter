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
using System.Text;

namespace newrisourcecenter.Controllers
{
    public partial class SRPViewController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private CommonController comm = new CommonController();

        #region SRP index
        // GET: SRPView
        public async Task<ActionResult> Index()
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            SRPViewModel srp = new SRPViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 30123 || a.n2ID == 30123).FirstOrDefaultAsync();// Get page text
            var usr_data = db.UserViewModels.Where(a => a.usr_ID == userId).FirstOrDefault();

            srp.topdesc = topdesc != null ? topdesc.n2_descLong : "";
            srp.usr_phone = usr_data.usr_phone;
            srp.usr_fname = usr_data.usr_fName;
            srp.usr_lname = usr_data.usr_lName;
            srp.usr_email = usr_data.usr_email;

            return View(srp);
        }
        #endregion

        #region Manage SRP
        // GET: SRPView
        public async Task<ActionResult> Manage()
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //Get Regional Managers
            //var regionalMan = db.salesRequestApprovers;
            IEnumerable<SRPViewModel> srp_model;
            //Get Regional Managers
            var regionalMan = db.salesRequestApprovers.Where(a=> a.Status == 1);
            activateApprovers(regionalMan);//check user permissions

            if (User.IsInRole("Super Admin"))
            {
                srp_model = await db.SRPViewModels.ToListAsync();
            }
            else
            {
                srp_model = await db.SRPViewModels.Where(a => a.SalesRepID==userId).ToListAsync();
            }

            foreach (var item in srp_model)
            {
                //get rep name
                var fullname = await comm.GetfullName(Convert.ToInt32(item.SalesRepID));
                item.fullname = fullname["fullName"];
                //Text for department
                int department_position = Convert.ToInt32(item.department);
                item.department = FuncCommonSRP(regionalMan).department[department_position].Text;
                //Text for payment method
                int paymentMethod_position = Convert.ToInt32(item.paymentMethod);
                item.paymentMethod = FuncCommonSRP(regionalMan).paymentMethod[paymentMethod_position].Text;
                //Text for request type
                int requestType_position = Convert.ToInt32(item.requestType);
                item.requestType = FuncCommonSRP(regionalMan).requestType[requestType_position].Text;
            }
        
            return View(srp_model);
        }
        #endregion

        #region Admin SRP
        // GET: SRPView
        public async Task<ActionResult> Admin(string region=null)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            IEnumerable<SRPViewModel> srp_model;
            var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
            activateApprovers(regionalMan);//check user permissions
                                           //Get the combined data for user and form
            var srpRequest = db.SRPViewModels.Join(
                                                    db.UserViewModels,
                                                    srp => srp.SalesRepID,
                                                    usr => usr.usr_ID,
                                                    (srp, usr) => new { srp, usr }
                                                ).Join(
                                                    db.partnerCompanyViewModels,
                                                    comp => comp.usr.comp_ID,
                                                    usSRP => usSRP.comp_ID,
                                                    (usSRP, comp) => new { usSRP, comp }
                                                ).OrderByDescending(a => a.usSRP.srp.FormID);

            if (User.IsInRole("Super Admin") || ViewBag.isbuyer==1 || ViewBag.isaccountspayables==1)
            {
                srp_model = await db.SRPViewModels.ToListAsync();
            }
            else
            {
                srp_model = await db.SRPViewModels.Where(a => a.region == region && a.status != "Draft").ToListAsync();
            }

            foreach (var item in srp_model)
            {
                //get rep name
                var fullname = await comm.GetfullName(Convert.ToInt32(item.SalesRepID));
                item.fullname = fullname["fullName"];
                //Text for department
                int department_position = Convert.ToInt32(item.department);
                item.department = FuncCommonSRP(regionalMan).department[department_position].Text;
                //Text for payment method
                int paymentMethod_position = Convert.ToInt32(item.paymentMethod);
                item.paymentMethod = FuncCommonSRP(regionalMan).paymentMethod[paymentMethod_position].Text;
                //Text for request type
                int requestType_position = Convert.ToInt32(item.requestType);
                item.requestType = FuncCommonSRP(regionalMan).requestType[requestType_position].Text;
            }

            //Add company types to the list
            StringBuilder user_strings = new StringBuilder();
            List<Nav1List> list_comp = new List<Nav1List>();
            foreach (var item in srpRequest.GroupBy(a => new { a.usSRP.usr.usr_ID, a.usSRP.usr.usr_lName, a.usSRP.usr.usr_fName, a.usSRP.usr.comp_ID }).OrderBy(a => a.Key.usr_fName))
            {
                user_strings.Append(string.Format("\"<option value={0}>{1}</option>\"+", item.Key.usr_ID, item.Key.usr_fName + " " + item.Key.usr_lName));
                list_comp.Add(new Nav1List { id = item.Key.comp_ID });
            }
            ViewBag.list_users = user_strings;

            //Add company types to the list
            StringBuilder company_strings = new StringBuilder();
            foreach (var item in list_comp.GroupBy(m => new { m.id }))
            {
                var companyNames = db.partnerCompanyViewModels.Where(m => m.comp_ID == item.Key.id);
                company_strings.Append(string.Format("\"<option value={0}>{1}</option>\"+", item.Key.id, companyNames.FirstOrDefault().comp_name));
            }
            ViewBag.list_companys = company_strings;

            return View(srp_model);
        }
        #endregion

        #region Admin Filter
        [HttpGet]
        public ActionResult AdminFilter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminFilter(string filterType = null)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }
                var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
                //Get the combined data for user and form
                var srpRequest = db.SRPViewModels.Join(
                                                        db.UserViewModels,
                                                        srp => srp.SalesRepID,
                                                        usr => usr.usr_ID,
                                                        (srp, usr) => new { srp, usr }
                                                    ).Join(
                                                        db.partnerCompanyViewModels,
                                                        comp => comp.usr.comp_ID,
                                                        usSRP => usSRP.comp_ID,
                                                        (usSRP, comp) => new { usSRP, comp }
                                                    ).OrderByDescending(a => a.usSRP.srp.FormID);

                List<SRPViewModel> srpTools = new List<SRPViewModel>();
                //Get the log data
                //var srplog = db.salesRequestActionLogs.ToList();
                List<SelectListItem> departments = new List<SelectListItem>();
                departments = FuncCommonSRP(regionalMan).department;

                if (filterType == "formdata")
                {
                    int? formID = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.FormID == formID))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {                                    
                            if (@values.Value==item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber,dateCreated=item.usSRP.srp.dateCreated,dateCompleted=item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "po")
                {
                    string PO = Request.QueryString["form_value"];
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.ponumber==PO))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "companyName")
                {
                    int? compID = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.compID==compID))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "department")
                {
                    string department = Request.QueryString["form_value"];
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.department == department))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "status")
                {
                    string status = Request.QueryString["form_value"];
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.status.Contains(status)))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "requester")
                {
                    long? requester = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.SalesRepID==requester))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "request_date")
                {
                    DateTime start_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value"]));
                    DateTime end_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value1"]));
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.dateCreated >= start_date && a.usSRP.srp.dateCreated <= end_date))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }
                else if (filterType == "date_completed")
                {
                    DateTime start_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value"]));
                    DateTime end_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value1"]));
                    foreach (var item in srpRequest.Where(a => a.usSRP.srp.dateCompleted >= start_date && a.usSRP.srp.dateCompleted <= end_date))
                    {
                        int form_id = Convert.ToInt32(item.usSRP.srp.FormID);
                        //Text for department
                        int m = 0;
                        string depart = "";
                        int department_position = Convert.ToInt32(item.usSRP.srp.department);
                        foreach (var values in departments)
                        {
                            if (@values.Value == item.usSRP.srp.department)
                            {
                                depart = @values.Text;
                                break;
                            }
                            m++;
                        }
                        srpTools.Add(new SRPViewModel { FormID = item.usSRP.srp.FormID, fullname = item.usSRP.usr.usr_fName + " " + item.usSRP.usr.usr_lName, status = item.usSRP.srp.status, department = depart, ponumber = item.usSRP.srp.ponumber, dateCreated = item.usSRP.srp.dateCreated, dateCompleted = item.usSRP.srp.dateCompleted });
                    }
                }

                return View(srpTools);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
        }
        #endregion

        #region View SRP Details
        // GET: SRPView/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(id);
            if (sRPViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sRPViewModel);
        }
        #endregion

        #region Create SRP
        // GET: SRPView/Create
        public ActionResult Create()
        {
            //Get Regional Managers
            var regionalMan = db.salesRequestApprovers.Where(a =>a.Status==1 && a.Department == "0");
            SRPViewModel srp = new SRPViewModel();
            //List departments          
            srp.list_departments = FuncCommonSRP(regionalMan).department.OrderBy(a => a.Text);
            //List departments
            List<SelectListItem> requestType = new List<SelectListItem>();
            srp.list_requestType = FuncCommonSRP(regionalMan).requestType.OrderBy(a => a.Text);
            //List departments
            List<SelectListItem> paymentMethod = new List<SelectListItem>();
            srp.list_paymentMethod = FuncCommonSRP(regionalMan).paymentMethod.OrderBy(a => a.Text);
            //Test for ACH/Check
            srp.list_achType = FuncCommonSRP(regionalMan).achType.OrderBy(a => a.Text);
            //Text for Cost Center
            srp.list_cccType = FuncCommonSRP(regionalMan).cccType.OrderBy(a => a.Text);

            return View(srp);
        }

        // POST: SRPView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "formID,SalesRepID,paymentMethod,department,requestType,fileupload,uploadinvoice,status,dateCreated,dateUpdated,region,supplier,supplierNumber,shipTo,shiptoAttn,compID,title,description,estimatedCost,activitydate")] SRPViewModel sRPViewModel, IEnumerable<HttpPostedFileBase> fileupload, IEnumerable<HttpPostedFileBase> uploadinvoice)
        {
            if (ModelState.IsValid)
            {
                long? companyId = Convert.ToInt32(Session["companyId"]);
                long userId = Convert.ToInt64(Session["userId"]);
                int languageId = Convert.ToInt32(Session["userLanguageId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                sRPViewModel.dateCreated = DateTime.Today;
                if (!string.IsNullOrEmpty(Request.Form["save"]))
                {
                    sRPViewModel.status = "Draft";
                    db.SRPViewModels.Add(sRPViewModel);
                    await db.SaveChangesAsync();
                    //Add files
                    salesRequestFile srp_files = new salesRequestFile();
                    await SaveFiles(fileupload, sRPViewModel.FormID, srp_files);
                    await UploadInvoice(uploadinvoice, sRPViewModel.FormID, srp_files);
                    //Save items
                    await saveItems(sRPViewModel, "create");
                    //save action into log
                    await logAction(sRPViewModel.FormID, "Draft", "A new form with Id="+ sRPViewModel.FormID + " has been created", userId.ToString());
                }
                else
                {
                    //Get Regional Managers
                    var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
                    int department = Convert.ToInt32(sRPViewModel.department);
                    int region = Convert.ToInt32(sRPViewModel.region);
                    var regionalApproval = regionalMan.Where(x => x.ID == region).FirstOrDefault();
                    var approver = await comm.GetfullName(Convert.ToInt32(regionalApproval.UserID));
                    var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));

                    string supervisorEmailBody = "A new " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                    string requesterMessage = "<br />" +
                    "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted to " + approver["fullName"] + " for approval." +
                    "<br />" +
                    "Be on the lookout for an email when an action is taken on your request." +
                    "<br /><br />";

                    //Set the status
                    sRPViewModel.status = "Pending";
                    db.SRPViewModels.Add(sRPViewModel);
                    await db.SaveChangesAsync();
                    //Add files
                    salesRequestFile srp_files = new salesRequestFile();
                    await SaveFiles(fileupload, sRPViewModel.FormID, srp_files);
                    await UploadInvoice(uploadinvoice, sRPViewModel.FormID, srp_files);
                    //Save items
                    await saveItems(sRPViewModel, "create");
                    //save action into log
                    await logAction(sRPViewModel.FormID, "Submitted", "A new form with Id=" + sRPViewModel.FormID + " has been created", userId.ToString());
                    //Get the list of files
                    IQueryable<salesRequestFile> get_srp_files = db.SalesRequestFiles.Where(a => a.FormID == sRPViewModel.FormID);
                    sRPViewModel.list_srp_files = get_srp_files.Where(a => a.AttachmentType == 1);
                    sRPViewModel.list_srp_invoice = get_srp_files.Where(a => a.AttachmentType == 2);//attach invoice
                    //send email to requestor and approver
                    await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", requesterMessage);
                    await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request",supervisorEmailBody);
                }

                return RedirectToAction("Manage", new { n1_name= Request.Form["n1_name"], n2_name=Request.Form["n2_name"],msg = "Manage Sales Request", success="Your request has been submitted" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(sRPViewModel);
        }
        #endregion

        #region Show Log table
        public async Task<ActionResult> LogTable(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //List Log of information
            List<salesRequestActionLog> logs = new List<salesRequestActionLog>();
            var srp_logs = db.salesRequestActionLogs.Where(a => a.Form_ID == id);
            foreach (var item in srp_logs)
            {
                string name = "";
                if (item.Usr_ID == "-1")
                {
                    name = "Approver/Supervisor";
                }
                else if (item.Usr_ID == "-2")
                {
                    name = "Procurement/Rittal Buyer";
                }
                else if (item.Usr_ID == "-3")
                {
                    name = "Accounts Payable";
                }
                else
                {
                    var approver = await comm.GetfullName(Convert.ToInt32(item.Usr_ID));
                    if (!string.IsNullOrEmpty(approver["fullName"]))
                    {
                        name = approver["fullName"];
                    }
                }

                logs.Add(new salesRequestActionLog
                {
                    ID = item.ID,
                    Form_ID = item.Form_ID,
                    Action_Time = item.Action_Time,
                    Notes = item.Notes,
                    Action = item.Action,
                    name = name
                });
            }

            return View(logs);
        }
        #endregion

        #region Edit SRP
        // GET: SRPView/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(id);
            if (sRPViewModel == null)
            {
                return HttpNotFound();
            }
            //Get Regional Managers
            var regionalMans = db.salesRequestApprovers.Where(a => a.Status == 1);
            activateApprovers(regionalMans);//check user permissions
            var regionalMan = regionalMans.Where(a => a.Department == "0");

            //List files
            IQueryable<salesRequestFile> srp_files = db.SalesRequestFiles.Where(a=>a.FormID == sRPViewModel.FormID); 
            //get rep name
            var fullname = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            sRPViewModel.fullname = fullname["fullName"];
            //Text for department
            sRPViewModel.list_departments = FuncCommonSRP(regionalMan).department.OrderBy(a => a.Text);
            //Text for payment method
            sRPViewModel.list_paymentMethod = FuncCommonSRP(regionalMan).paymentMethod.OrderBy(a => a.Text);
            //Text for request type
            sRPViewModel.list_requestType = FuncCommonSRP(regionalMan).requestType.OrderBy(a => a.Text);
            sRPViewModel.list_srp_files = srp_files.Where(a=>a.AttachmentType==1);
            sRPViewModel.list_srp_invoice = srp_files.Where(a => a.AttachmentType == 2);//attach invoice
            //List departments
            List<SelectListItem> regionalManager = new List<SelectListItem>();
            sRPViewModel.list_regionalManagers = FuncCommonSRP(regionalMan).regionalManager.OrderBy(a => a.Text);
            //Text for ACH/Check
            sRPViewModel.list_achType = FuncCommonSRP(regionalMan).achType.OrderBy(a => a.Text);
            //Text for Cost Center
            sRPViewModel.list_cccType = FuncCommonSRP(regionalMan).cccType.OrderBy(a => a.Text);
            //Additional info
            var srp_items = db.salesRequestAdditionalInfos.Where(a => a.Form_ID == id);    
            sRPViewModel.list_additional_info = srp_items;

            return View(sRPViewModel);
        }

        // POST: SRPView/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "formID,SalesRepID,paymentMethod,department,requestType,fileupload,uploadinvoice,status,dateCreated,dateUpdated,region,supplier,supplierNumber,shipTo,shiptoAttn,ponumber,compID,title,description,estimatedCost,activitydate,InvoiceAmountIsEqual")] SRPViewModel sRPViewModel, IEnumerable<HttpPostedFileBase> fileupload,IEnumerable<HttpPostedFileBase> uploadinvoice)
        {
            if (ModelState.IsValid)
            {
                long? companyId = Convert.ToInt32(Session["companyId"]);
                long userId = Convert.ToInt64(Session["userId"]);
                int languageId = Convert.ToInt32(Session["userLanguageId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!string.IsNullOrEmpty(Request.Form["save"]))
                {
                    if (sRPViewModel.status != "Completed")
                    {
                        sRPViewModel.status = "Draft";
                    }
                    db.Entry(sRPViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Add files
                    salesRequestFile srp_files = new salesRequestFile();
                    await SaveFiles(fileupload, sRPViewModel.FormID, srp_files);
                    await UploadInvoice(uploadinvoice, sRPViewModel.FormID, srp_files);
                    //Save items
                    await saveItems(sRPViewModel, "edit");
                    //save action into log
                    await logAction(sRPViewModel.FormID, "Draft", "A new form with Id=" + sRPViewModel.FormID + " has been created", userId.ToString());
                }
                else
                {
                    //Get Regional Managers
                    var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
                    int department = Convert.ToInt32(sRPViewModel.department);
                    int region = Convert.ToInt32(sRPViewModel.region);
                    var regionalApproval = regionalMan.Where(x => x.ID == region).FirstOrDefault();
                    var approver = await comm.GetfullName(Convert.ToInt32(regionalApproval.UserID));
                    var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));

                    db.Entry(sRPViewModel).State = EntityState.Modified;
                    //Add files
                    salesRequestFile srp_files = new salesRequestFile();
                    await SaveFiles(fileupload, sRPViewModel.FormID, srp_files);
                    await UploadInvoice(uploadinvoice, sRPViewModel.FormID, srp_files);
                    //Save items
                    await saveItems(sRPViewModel, "edit");
                    //Get the list of files
                    IQueryable<salesRequestFile> get_srp_files = db.SalesRequestFiles.Where(a => a.FormID == sRPViewModel.FormID);
                    sRPViewModel.list_srp_files = get_srp_files.Where(a => a.AttachmentType == 1);
                    sRPViewModel.list_srp_invoice = get_srp_files.Where(a => a.AttachmentType == 2);//attach invoice
                    if (sRPViewModel.status=="Invoice Needed" )
                    {
                        if (sRPViewModel.InvoiceAmountIsEqual == 1) {
                            //The only time requestor emails accounting is when the invoice Needed action is performed               
                            string accountingEmailBody = "The invoice requested for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                            string requesterMessage = "<br />" +
                            "Your invoice for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted to accounts payable for it to be completed." +
                            "<br /><br />" +
                            "Your status should be showing that you submitted the invoice and when you are reimbursed, your status would show completed." +
                            "<br /><br />" +
                            "Be on the lookout for an email when an action is taken on your request." +
                            "<br /><br />";
                            //save action into log
                            await logAction(sRPViewModel.FormID, "Invoice Submiited", "Invoice Submitted to Accounting for the form with Id=" + sRPViewModel.FormID + " ", userId.ToString());
                            sRPViewModel.status = "Invoice Submitted";
                            await db.SaveChangesAsync();
                            //send email to requestor and approver
                            await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Invoice has been submitted", requesterMessage);
                            await EmailAccounting(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", accountingEmailBody);
                        }
                        else
                        {
                            string supervisorEmailBody = "The " + FuncCommonSRP(regionalMan).department[department].Text + " request was updated and resubmitted by " + salesrep["fullName"] + " because the purchase request amount does not match the invoice amount.";
                            string requesterMessage = "<br />" +
                            "Your resubmitted " + FuncCommonSRP(regionalMan).department[department].Text + " request has been sent to " + approver["fullName"] + " for approval." +
                            "<br />" +
                            "Be on the lookout for an email when an action is taken on your request." +
                            "<br /><br />";
                            //save action into log
                            await logAction(sRPViewModel.FormID, "Resubmitted", "The resubmitted invoice and PO request amount do not match for the form with Id=" + sRPViewModel.FormID + "", userId.ToString());
                            sRPViewModel.status = "Amount Updated";
                            await db.SaveChangesAsync();
                            //send email to requestor and approver
                            await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Resubmitted Sales/Channel Request", requesterMessage);
                            await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Resubmitted Sales/Channel Request", supervisorEmailBody);
                        }
                    }
                    else if (sRPViewModel.status=="Additional Information-Procurement")
                    {
                        string supervisorEmailBody = "The additional information for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                        string requesterMessage = "<br />" +
                        "Your additional information for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted to " + approver["fullName"] + " for review and it will be forwarded to the buyer if approved." +
                        "<br />" +
                        "Be on the lookout for an email when an action is taken on your request." +
                        "<br /><br />";
                        //save action into log
                        await logAction(sRPViewModel.FormID, "Submitted Additional Information-Procurement", "Additional Information has been submitted to Procurement for the form with Id=" + sRPViewModel.FormID + " ", userId.ToString());
                        sRPViewModel.status = "Pending";
                        await db.SaveChangesAsync();
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Additional Information has been submitted", requesterMessage);
                        await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Additional Information for Sales/Channel Request", supervisorEmailBody);
                    }
                    else if (sRPViewModel.status == "Additional Information-Accounting")
                    {
                        string accountingEmailBody = "The additional information for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                        string requesterMessage = "<br />" +
                        "Your additional information for the " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted to accounts payable for it to be completed." +
                        "<br />" +
                        "Be on the lookout for an email when an action is taken on your request." +
                        "<br /><br />";
                        //save action into log
                        await logAction(sRPViewModel.FormID, "Submitted Additional Information-Accounting", "Additional Information has been submitted to Accounting for the form with Id=" + sRPViewModel.FormID + " ", userId.ToString());
                        await db.SaveChangesAsync();
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Additional Information has been submitted", requesterMessage);
                        await EmailAccounting(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Additinal Information has been submitted", accountingEmailBody);
                    }
                    else
                    {
                        if (sRPViewModel.status != "Completed")
                        {
                            sRPViewModel.status = "Pending";
                        }
                        string supervisorEmailBody = "A new " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".And has been approved by the procument department.";
                        string requesterMessage = "<br />" +
                        "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted to " + approver["fullName"] + " for approval." +
                        "<br />" +
                        "Be on the lookout for an email when an action is taken on your request." +
                        "<br /><br />";
                        //save action into log
                        await logAction(sRPViewModel.FormID, "Submitted", "A new form with Id=" + sRPViewModel.FormID + " has been editted", userId.ToString());
                        await db.SaveChangesAsync();
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", requesterMessage);
                        await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", supervisorEmailBody);
                    }
                }

                return RedirectToAction("Manage", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = "Manage Sales Request", success = "Your request has been updated" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(sRPViewModel);
        }
        #endregion

        #region Update Status Supervisor
        public async Task<ActionResult> StatusSupervisor(int formid, string status,bool postComment)
        {
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(formid);
            //Get Regional Managers
            var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
            int department = Convert.ToInt32(sRPViewModel.department);
            int region = Convert.ToInt32(sRPViewModel.region);
            var regionalApproval = regionalMan.Where(x => x.ID == region).FirstOrDefault();
            var approver = await comm.GetfullName(Convert.ToInt32(regionalApproval.UserID));
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            //Get the list of files
            IQueryable<salesRequestFile> get_srp_files = db.SalesRequestFiles.Where(a => a.FormID == sRPViewModel.FormID);
            sRPViewModel.list_srp_files = get_srp_files.Where(a => a.AttachmentType == 1);
            sRPViewModel.list_srp_invoice = get_srp_files.Where(a => a.AttachmentType == 2);//attach invoice

            long userId = Convert.ToInt64(Session["userId"]);
            if (userId == 0)
            {
                userId = -1;
            }
            ViewBag.status_data = "The requestor will be notified of the status change to " + status;

            if (status == "Approved") //if it is approved
            {
                //show the status on page and place to enter PO number to purchaser
                if (sRPViewModel.paymentMethod == "0")//Will Expense in Concur. Email requestor to expense in concur
                {
                    string requesterMessage = "<br />" +
                    "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been approved by " + approver["fullName"] + " . You may login to verify your status." +
                    "<br />" +
                    "The next step is to submit your request in concur as indicated on your request." +
                    "<br /><br />";
                    //send email to requestor
                    await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Will Expense in Concur", requesterMessage);
                }
                else if (sRPViewModel.paymentMethod == "1")//send procument email and tell the requestor it has been forwarded to procurment if check or ACH and mark as review
                {
                    if (sRPViewModel.status == "Amount Updated")
                    {
                        string requesterMessage = "<br />" +
                        "Your resubmitted " + FuncCommonSRP(regionalMan).department[department].Text + " request has been approved by " + approver["fullName"] + " and forwarded to the procurement department for review." +
                        "<br />" +
                        "Be on the lookout for an email when an action is taken on your request." +
                        "<br /><br />";
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Resubmitted Sales/Channel Request", requesterMessage);
                        //Check if PO Number is set. If PO is set by pass purchasing
                        string procurementMessage = "The " + FuncCommonSRP(regionalMan).department[department].Text + " request has been resubmitted by " + salesrep["fullName"] + " because the invoice amount does not match the orignal estimated amount.";
                        await EmailProcurement(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Resubmitted Sales/Channel Request", procurementMessage);
                    }
                    else
                    {
                        string requesterMessage = "<br />" +
                        "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been approved by " + approver["fullName"] + " and forwarded to the procurement department for review." +
                        "<br />" +
                        "Be on the lookout for an email when an action is taken on your request." +
                        "<br /><br />";
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", requesterMessage);
                        //Check if PO Number is set. If PO is set by pass purchasing
                        string procurementMessage = "A new " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                        await EmailProcurement(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request", procurementMessage);
                    }            
                }

                //Update status to Approved
                sRPViewModel.status = status;
                await db.SaveChangesAsync();
                //save action into log
                await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Approved by supervisor", userId.ToString());
            }
            else if (status == "Denied")//if it is denied
            {
                //send requestor an email telling them it has been denied
                string requesterMessage = "<br />" +
                "Based on the information provided, unfortunately your request cannot be approved. You may resubmit another request in the feature." +
                "<br /><br />";

                //Update status to deny
                sRPViewModel.status = status;
                await db.SaveChangesAsync();
                //send email to requestor
                await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Will Expense in Concur", requesterMessage);

                //save action into log
                await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Denied by supervisor", userId.ToString());
            }
            else if (status == "Additional Information") //Additional informaton is required
            {
                if (!postComment)
                {
                    ViewBag.formID = formid;
                    ViewBag.status = status;
                    ViewBag.postComment = postComment;
                }
                else
                {
                    //email requestor for additional information
                    string requesterMessage = "<br />" +
                    "It was difficult to make a decision on your request." +
                    "<br /><br />" +
                    "Please <a href=\"https://www.risourcecenter.com/SRPView/Edit/"+ sRPViewModel.FormID + "?n1_name=Support%20Tools&n2_name=Sales%20Request%20Portal\">login</a> to the RiSourceCenter and submit additional information as requested below:" +
                    "<br /><br />" +
                    "<table style=\"background-color:silver;\">" +
                        "<tr>" +
                            "<td>" +
                            "<br />" +
                            Request.QueryString["comment"] +
                            "<br /><br />" +
                            "</td>" +
                        "</tr>" +
                    "</table>" +
                    "<br /><br />";

                    //Update status to Additional Information
                    sRPViewModel.status = status;
                    await db.SaveChangesAsync();

                    //send email to requestor
                    await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Will Expense in Concur", requesterMessage);

                    //save action into log
                    await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Additional Information by supervisor", userId.ToString());
                }
            }

            return View();
        }
        #endregion

        #region Update Status Procurement
        public async Task<ActionResult> StatusProcurment(int formid, string status, bool postComment=false)
        {
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(formid);
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            //Get the list of files
            IQueryable<salesRequestFile> get_srp_files = db.SalesRequestFiles.Where(a => a.FormID == sRPViewModel.FormID);
            sRPViewModel.list_srp_files = get_srp_files.Where(a => a.AttachmentType == 1);
            sRPViewModel.list_srp_invoice = get_srp_files.Where(a => a.AttachmentType == 2);//attach invoice

            //Get Regional Managers
            var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
            int department = Convert.ToInt32(sRPViewModel.department);
            long userId = Convert.ToInt64(Session["userId"]);
            if (userId == 0)
            {
                userId = -2;
            }
            ViewBag.status_data = "The requestor will be notified of the status change to " + status;

            if (status == "Approved") //if it is approved and send email to accounting and requestor
            {
                if (!postComment)
                {
                    ViewBag.status_data = "Enter PO number to be sent to the accounting department";
                    ViewBag.formID = formid;
                    ViewBag.status = status;
                    ViewBag.postComment = postComment;
                    ViewBag.type = 1;
                }
                else
                {
                    //Update the ponumber
                    sRPViewModel.ponumber = Request.QueryString["comment"];
                    await db.SaveChangesAsync();
                    //invoice is attached send to account else send to requestor
                    if (sRPViewModel.list_srp_invoice.Count() > 0)
                    {
                        //Update status to Approved
                        sRPViewModel.status = "PO Assigned";
                        string accountingEmailBody = "A new " + FuncCommonSRP(regionalMan).department[department].Text + " request has been submitted by " + salesrep["fullName"] + ".";
                        //email requestor for additional information
                        string requesterMessage = "<br />" +
                                "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been approved by the Rittal Buyer or Procurement department. You may log in to verify your status." +
                                "<br /><br />" +
                                "Also your request has been forwarded to the Accounting department for the next step." +
                                "<br /><br />" +
                                "Your status should be showing that a PO number has been assigned to your request now and when you are reimbursed, your status would show completed." +
                                "<br /><br />";
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " PO has been Assigned", requesterMessage);
                        await EmailAccounting(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " New Sales/Channel Request PO #" + Request.QueryString["comment"], accountingEmailBody);
                    }
                    else
                    {
                        //Update status to Approved
                        sRPViewModel.status = "Invoice Needed";
                        //email requestor for invoice
                        string requesterMessage = "<br />" +
                                "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been approved by the Rittal Buyer or Procurement department. But you need to <a href=\"https://www.risourcecenter.com/SRPView/Edit/" + sRPViewModel.FormID + "?n1_name=Support%20Tools&n2_name=Sales%20Request%20Portal\">login</a> and attach an invoice for the transaction to be completed by accounting." +
                                "<br /><br />" +
                                "Please login to attach the invoice." +
                                "<br /><br />";
                        //send email to requestor and approver
                        await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Invoice Is Required", requesterMessage);
                    }

                    //save action into log
                    await logAction(sRPViewModel.FormID, sRPViewModel.status, "The form with Id=" + sRPViewModel.FormID + " has been assigned a PO number= "+ sRPViewModel.ponumber +" by the buyer but invoice is needed", userId.ToString());
                }
            }
            else if (status == "Denied")//if it is denied, email requestor and supervisor
            {
                //send requestor an email telling them it has been denied
                string requesterMessage = "<br />" +
                "Based on the information provided, unfortunately your request cannot be approved. You may resubmit another request in the feature." +
                "<br /><br />";

                //Update status to Denied
                sRPViewModel.status = "Procurement-Denied";
                await db.SaveChangesAsync();
                //send email to requestor
                await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Will Expense in Concur", requesterMessage);

                //save action into log
                await logAction(sRPViewModel.FormID, sRPViewModel.status, "The form with Id=" + sRPViewModel.FormID + " has been denied a PO number by the buyer", userId.ToString());
            }
            else if (status == "Additional Information") //Additional informaton is required
            {
                if (!postComment)
                {
                    ViewBag.formID = formid;
                    ViewBag.status = status;
                    ViewBag.postComment = postComment;
                    ViewBag.type = 2;
                }
                else
                {
                    //email requestor for additional information
                    string requesterMessage = "<br />" +
                    "It was difficult to make a decision on your request." +
                    "<br /><br />" +
                    "Please <a href=\"https://www.risourcecenter.com/SRPView/Edit/" + sRPViewModel.FormID + "?n1_name=Support%20Tools&n2_name=Sales%20Request%20Portal\">login</a> to the RiSourceCenter and submit the additional information as requested below:" +
                    "<br /><br />" +
                    "<table style=\"background-color:silver;\">" +
                        "<tr>" +
                            "<td>" +
                            "<br />" +
                            Request.QueryString["comment"] +
                            "<br /><br />" +
                            "</td>" +
                        "</tr>" +
                    "</table>" +
                    "<br /><br />";

                    //Update status to Additional Information
                    sRPViewModel.status = "Additional Information-Procurement";
                    await db.SaveChangesAsync();

                    //send email to requestor
                    await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Will Expense in Concur", requesterMessage);

                    //save action into log
                    await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Additional Information by supervisor", userId.ToString());
                }
            }

            return View();
        }
        #endregion

        #region Update Status Accounting Department
        public async Task<ActionResult> StatusAccounting(int formid, string status, bool postComment = false)
        {
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(formid);
            //Get Regional Managers
            var regionalMan = db.salesRequestApprovers.Where(a => a.Status == 1);
            int department = Convert.ToInt32(sRPViewModel.department);
            int region = Convert.ToInt32(sRPViewModel.region);
            var regionalApproval = regionalMan.Where(x => x.ID == region).FirstOrDefault();
            var approver = await comm.GetfullName(Convert.ToInt32(regionalApproval.UserID));
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            long userId = Convert.ToInt64(Session["userId"]);
            //Get the list of files
            IQueryable<salesRequestFile> get_srp_files = db.SalesRequestFiles.Where(a => a.FormID == sRPViewModel.FormID);
            sRPViewModel.list_srp_files = get_srp_files.Where(a => a.AttachmentType == 1);
            sRPViewModel.list_srp_invoice = get_srp_files.Where(a => a.AttachmentType == 2);//attach invoice

            if (userId == 0)
            {
                userId = -3;
            }
            ViewBag.status_data = "The requestor will be notified of the status change to " + status;

            if (status == "Completed") //if it is approved
            {
                string requesterMessage = "<br />" +
                            "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request has been marked as complete. You may log in to verify your status."+
                            "<br /><br />"+
                            "Contact the Accounting department if you have any questions."+
                            "<br /><br />";

                //Update status to completed
                sRPViewModel.status = status;
                sRPViewModel.dateCompleted = DateTime.Now;

                await db.SaveChangesAsync();

                //show the status of on the page 
                ViewBag.status_data = "The requestor will be notified of the status change to " + status;
                await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Has Been Completed ", requesterMessage);

                //save action into log
                await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Completed by the Accounting Department", userId.ToString());
            }
            else if (status == "Additional Information")
            {
                if (!postComment)
                {
                    ViewBag.status_data = "Enter the information you would like the requestor to send";
                    ViewBag.formID = formid;
                    ViewBag.status = status;
                    ViewBag.postComment = postComment;
                }
                else
                {
                    string requesterMessage = "<br />" +
                                "Your " + FuncCommonSRP(regionalMan).department[department].Text + " request was approved by your supervisor " + approver["fullName"] + " and the purchasing department assigned a PO number but unfortunately it is difficult for us to make a decision on your request in the accounting department." +
                                "<br /><br />" +
                                "Please <a href=\"https://www.risourcecenter.com/SRPView/Edit/" + sRPViewModel.FormID + "?n1_name=Support%20Tools&n2_name=Sales%20Request%20Portal\">login</a> to the RiSourceCenter and provide the additional information as stated below." +
                                "<br /><br />This would help us make a better decision on your request.<br /><br />" +
                                "<table style=\"background-color:silver;\">" +
                                "<tr>" +
                                    "<td>" +
                                    "<br />" +
                                    Request.QueryString["comment"] +
                                    "<br /><br />" +
                                    "</td>" +
                                "</tr>" +
                                "</table>" +
                                "<br /><br />";

                    string supervisorEmailBody = "The sales request number " + sRPViewModel.FormID + " submitted by " + salesrep["fullName"] + " could not be completed by Accounting. Additional Information has been requested as indicated below:" +
                                "<br /><br />" +
                                "<table style=\"background-color:silver;\">" +
                                "<tr>" +
                                    "<td>" +
                                    "<br />" +
                                    Request.QueryString["comment"] +
                                    "<br /><br />" +
                                    "</td>" +
                                "</tr>" +
                                "</table>" +
                                "<br /><br />";

                    //Update status to Additional Information
                    sRPViewModel.status = "Additional Information-Accounting";
                    await db.SaveChangesAsync();

                    await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Additional Information Requested by Accounting ", requesterMessage);
                    await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Additional Information Requested by Accounting", supervisorEmailBody);

                    //save action into log
                    await logAction(sRPViewModel.FormID, sRPViewModel.status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Additional Information by the Accounting Department", userId.ToString());
                }
            }
            else  if (status == "Denied")//if it is denied
            {
                //send requestor an email telling them it has been denied
                string requesterMessage = "<br />" +
                "Based on the information provided, unfortunately your request cannot be completed. You may resubmit another request in the feature." +
                "<br /><br />";
                string supervisorEmailBody = "The sales request number "+ sRPViewModel.FormID +" submitted by " + salesrep["fullName"] + ". Has been denied by the accounting department.";

                //Update status to Denied
                sRPViewModel.status = "Accounting-Denied";
                await db.SaveChangesAsync();

                await EmailRequestor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + "  Has been denied by Accounting", requesterMessage);
                await EmailSupervisor(regionalMan, sRPViewModel, "REQ No. " + sRPViewModel.FormID + " Has been denied by Accounting",supervisorEmailBody);

                //save action into log
                await logAction(sRPViewModel.FormID, status, "The status of the form with Id=" + sRPViewModel.FormID + " has been changed to Denied by the Accounting Department", userId.ToString());
            }

            return View();
        }
        #endregion

        #region SRP files
        async Task SaveFiles(IEnumerable<HttpPostedFileBase> fileupload, int srp_id, salesRequestFile srp_files)
        {
            //Process the attached images
            foreach (HttpPostedFileBase file in fileupload)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file.FileName);
                    var NewFileName = srp_id + "_" + fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/srp/files"), NewFileName);

                    file.SaveAs(path);
                    //if file_id is not set or the entry is empty
                    srp_files = new salesRequestFile
                    {
                        FormID=srp_id,
                        FileName=NewFileName,
                        AttachmentType = 1             
                    };
                    db.SalesRequestFiles.Add(srp_files);
                    await db.SaveChangesAsync();
                }
            }
        }

        async Task UploadInvoice(IEnumerable<HttpPostedFileBase> uploadInvoice, int srp_id, salesRequestFile srp_files)
        {
            //Process the attached images
            foreach (HttpPostedFileBase file in uploadInvoice)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file.FileName);
                    var NewFileName = srp_id + "_" + fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/srp/files"), NewFileName);

                    file.SaveAs(path);
                    //if file_id is not set or the entry is empty
                    srp_files = new salesRequestFile
                    {
                        FormID = srp_id,
                        FileName = NewFileName,
                        AttachmentType = 2
                    };
                    db.SalesRequestFiles.Add(srp_files);
                    await db.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region Dropdown Settings
        private SRPCommon FuncCommonSRP(IQueryable<salesRequestApprovers> regionalMan)
        {
            //List departments
            List<SelectListItem> departments = new List<SelectListItem>();
            departments.Add(new SelectListItem { Text = "IE Sales", Value = "0" });
            departments.Add(new SelectListItem { Text = "IT Sales", Value = "1" });
            departments.Add(new SelectListItem { Text = "Channel", Value = "2" });

            //List departments
            List<SelectListItem> requestType = new List<SelectListItem>();
            requestType.Add(new SelectListItem { Text = "Sales Meeting/Conference", Value = "0" });
            requestType.Add(new SelectListItem { Text = "Tradeshow/Event Participation", Value = "1" });
            requestType.Add(new SelectListItem { Text = "Sport Event", Value = "2" });
            requestType.Add(new SelectListItem { Text = "Golf Outing", Value = "3" });
            requestType.Add(new SelectListItem { Text = "Other", Value = "4" });

            //List departments
            List<SelectListItem> paymentMethod = new List<SelectListItem>();
            paymentMethod.Add(new SelectListItem { Text = "Will Expense in Concur", Value = "0" });
            paymentMethod.Add(new SelectListItem { Text = "ACH/Check (PO Required)", Value = "1" });

            //List Regional Managers
            List<SelectListItem> regionalManager = new List<SelectListItem>();
            foreach (var regManager in regionalMan)
            {
                regionalManager.Add(new SelectListItem { Text = regManager.RegionName, Value = regManager.ID.ToString() });
            }

            //List Ach/Check
            List<SelectListItem> achType = new List<SelectListItem>();
            achType.Add(new SelectListItem { Text = "097100 - Prepaid rent expense", Value = "0" });
            achType.Add(new SelectListItem { Text = "098100 - Prepaid insurance", Value = "1" });
            achType.Add(new SelectListItem { Text = "098200 - Prepaid advertising and tradeshow costs", Value = "2" });
            achType.Add(new SelectListItem { Text = "451300 - Maintenance/repair to vehicles", Value = "3" });
            achType.Add(new SelectListItem { Text = "451400 - Repair to and modification of tools", Value = "4" });
            achType.Add(new SelectListItem { Text = "453000 - Other rental and leasing costs", Value = "5" });
            achType.Add(new SelectListItem { Text = "462000 - Seminars/training", Value = "6" });
            achType.Add(new SelectListItem { Text = "465700 - Company entertainment", Value = "7" });
            achType.Add(new SelectListItem { Text = "465800 - Accommodation expenses", Value = "8" });
            achType.Add(new SelectListItem { Text = "465900 - Training expenses and costs of representation", Value = "9" });
            achType.Add(new SelectListItem { Text = "466300 - Travel expenses", Value = "10" });
            achType.Add(new SelectListItem { Text = "480000 - Advertising costs", Value = "11" });
            achType.Add(new SelectListItem { Text = "466400 - Guest travel related expenses", Value = "12" });
            achType.Add(new SelectListItem { Text = "467000 - Subscriptions", Value = "13" });
            achType.Add(new SelectListItem { Text = "467200 - Dues/Memberships", Value = "14" });
            achType.Add(new SelectListItem { Text = "467900 - Management travel expenses", Value = "15" });
            achType.Add(new SelectListItem { Text = "470000 - Management expenses", Value = "16" });
            achType.Add(new SelectListItem { Text = "480000 - Advertising costs", Value = "17" });
            achType.Add(new SelectListItem { Text = "480200 - Gifts/promotions", Value = "18" });
            achType.Add(new SelectListItem { Text = "481000 - Exhibition costs, trade shows", Value = "19" });
            achType.Add(new SelectListItem { Text = "482300 - Conference costs", Value = "20" });
            achType.Add(new SelectListItem { Text = "482600 - Information", Value = "21" });
            achType.Add(new SelectListItem { Text = "483000 - Misc sales expenses", Value = "22" });

            //List Cost Center
            List<SelectListItem> cccType = new List<SelectListItem>();
            cccType.Add(new SelectListItem { Text = "7200 - Channel Management", Value = "0" });
            cccType.Add(new SelectListItem { Text = "7105 - IT Sales Development", Value = "1" });
            cccType.Add(new SelectListItem { Text = "7110 - Northeast Sales Development", Value = "2" });
            cccType.Add(new SelectListItem { Text = "7115 - Central Sales Development", Value = "3" });
            cccType.Add(new SelectListItem { Text = "7120 - North Sales Development", Value = "4" });
            cccType.Add(new SelectListItem { Text = "7130 - South Sales Development", Value = "5" });
            cccType.Add(new SelectListItem { Text = "7140 - Southeast Sales Development", Value = "6" });
            cccType.Add(new SelectListItem { Text = "7150 - West Sales Development", Value = "7" });

            SRPCommon srp_common = new SRPCommon
            {
                department = departments,
                requestType = requestType,
                paymentMethod = paymentMethod,
                regionalManager = regionalManager,
                achType = achType,
                cccType = cccType                
            };

            return srp_common;
        }

        [HttpPost]
        public JsonResult GetRigionalManager(string id = null)
        {
            var countLogins = db.salesRequestApprovers.Where(a =>a.Status==1 && a.Department == id);
            return Json(countLogins);
        }
        #endregion

        #region log Action
        async Task logAction(int form_id, string action, string notes, string user_id)
        {
            salesRequestActionLog srpActionLog = new salesRequestActionLog();
            srpActionLog = new salesRequestActionLog
            {
                Form_ID = form_id,
                Action = action,
                Action_Time = DateTime.Now,
                Notes = notes,
                Usr_ID = user_id,
            };

            db.salesRequestActionLogs.Add(srpActionLog);
            await db.SaveChangesAsync();
        }
        #endregion

        #region Save Items
        async Task saveItems(SRPViewModel srpviewmodel,string action=null)
        {
            if (!string.IsNullOrEmpty(Request.Form["partNumberOrdescription"]))
            {
                string[] partNumberOrdescription = Request.Form["partNumberOrdescription"].Split(',');
                var srp_items = db.salesRequestAdditionalInfos.Where(a => a.Form_ID == srpviewmodel.FormID);
                if ( partNumberOrdescription.Count() > 0 && !string.IsNullOrEmpty(action) && action == "edit" && srp_items.Count() > 0)
                {
                    //insert installed parts
                    srp_items.FirstOrDefault().partNumberOrdescription = Request.Form["partNumberOrdescription"];
                    srp_items.FirstOrDefault().quantity = Request.Form["quantity"];
                    srp_items.FirstOrDefault().unitPrice = Request.Form["unitPrice"];
                    srp_items.FirstOrDefault().totalPrice = Request.Form["totalPrice"];
                    srp_items.FirstOrDefault().deliveryDate = Request.Form["deliveryDate"];
                    srp_items.FirstOrDefault().achType = Request.Form["achType"];
                    srp_items.FirstOrDefault().cccType = Request.Form["cccType"];

                    await db.SaveChangesAsync();
                }
                else
                {
                    if (srp_items.Count() == 0)
                    {
                        var salesRequestAdditionalInfo = new salesRequestAdditionalInfo
                        {
                            Form_ID = srpviewmodel.FormID,
                            partNumberOrdescription = Request.Form["partNumberOrdescription"],
                            quantity = Request.Form["quantity"],
                            deliveryDate = Request.Form["deliveryDate"],
                            unitPrice = Request.Form["unitPrice"],
                            totalPrice = Request.Form["totalPrice"],
                            cccType = Request.Form["cccType"],
                            achType = Request.Form["achType"]
                        };

                        db.salesRequestAdditionalInfos.Add(salesRequestAdditionalInfo);
                        await db.SaveChangesAsync();
                    }
                }
            }
            else
            {
                var srp_items = db.salesRequestAdditionalInfos.Where(a => a.Form_ID == srpviewmodel.FormID);
                if (!string.IsNullOrEmpty(action) && action == "edit" && srp_items.Count() > 0)
                {
                    db.salesRequestAdditionalInfos.RemoveRange(srp_items);           
                    await db.SaveChangesAsync();
                }
            }

        }
        #endregion

        #region Activate Approvers
        private void activateApprovers(IQueryable<salesRequestApprovers> regionalMan)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            //Get Regional Managers
            ViewBag.approver = 0;
            ViewBag.isbuyer = 0;
            ViewBag.isaccountspayables = 0;
            //check approver
            ViewBag.approver = 0;
            foreach (var item in regionalMan)
            {
                if (item.UserID == userId)
                {
                    ViewBag.approver = 1;
                    ViewBag.approverId = item.ID;
                }
            }
            //check buyer
            foreach (var item in regionalMan.Where(a => a.Department == "-3"))
            {
                if (item.UserID == userId)
                {
                    ViewBag.isbuyer = 1;
                }
            }
            //check accounts payables
            foreach (var item in regionalMan.Where(a => a.Department == "-2"))
            {
                if (item.UserID == userId)
                {
                    ViewBag.isaccountspayables = 1;
                }
            }
        }
        #endregion

        #region Delete SRP
        // GET: MDFView/Delete/5
        public async Task<ActionResult> DeleteFile(long? id)
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

            salesRequestFile srp_files = await db.SalesRequestFiles.FindAsync(id);
            if (srp_files == null)
            {
                return HttpNotFound();
            }
            db.SalesRequestFiles.Remove(srp_files);
            await db.SaveChangesAsync();
            //Delete the file from the system 
            string sourcePath = string.Format("~/attachments/srp/files/{0}", @srp_files.FileName);
            if (System.IO.File.Exists(Server.MapPath(sourcePath)))
            {
                var path = Server.MapPath(sourcePath);
                System.IO.File.Delete(path);
            }
            //save action into log
            await logAction(srp_files.FormID, "Deleted File", "A file with Id=" + id.ToString() + " has been deleted", userId.ToString());

            return RedirectToAction("Edit", new { id = srp_files.FormID, n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The file has been deleted" });
        }

        // GET: SRPView/Delete/5
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(id);
            if (sRPViewModel == null)
            {
                return HttpNotFound();
            }

            var srp_files = db.SalesRequestFiles.Where(a=>a.FormID==id);
            if (srp_files != null)
            {
                db.SalesRequestFiles.RemoveRange(srp_files);
                foreach (var item in srp_files)
                {
                    //Delete the file from the system 
                    string sourcePath = string.Format("~/attachments/srp/files/{0}", @item.FileName);
                    if (System.IO.File.Exists(Server.MapPath(sourcePath)))
                    {
                        var path = Server.MapPath(sourcePath);
                        System.IO.File.Delete(path);
                    }
                }
                await db.SaveChangesAsync();
            }

            db.SRPViewModels.Remove(sRPViewModel);
            await db.SaveChangesAsync();
            //save action into log
            await logAction(sRPViewModel.FormID, "Deleted SRP", "The SRP with Id=" + sRPViewModel.FormID + " has been deleted and it files", userId.ToString());

            return RedirectToAction("Manage", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The Sales Request has been deleted" });
        }

        // POST: SRPView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            SRPViewModel sRPViewModel = await db.SRPViewModels.FindAsync(id);
            db.SRPViewModels.Remove(sRPViewModel);
            await db.SaveChangesAsync();
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

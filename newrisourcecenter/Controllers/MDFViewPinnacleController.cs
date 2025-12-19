using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Controllers;
using System.IO;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;

namespace newrisourcecenter.Models
{
    public class MDFViewPinnacleController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController comm = new CommonController();

        #region Index or User default
        // GET: MDFView
        public async Task<ActionResult> Index()
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModels = new MDFPinnacleViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a=>a.n2ID == 6 || a.n2ID == 6).FirstOrDefaultAsync();// Get page text
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(companyId);//Get company data

            //if (partnerCompanyViewModel.comp_MDF_amount > 0)
            //{
            //    ViewBag.MDF = "Yes";
            //}
            //else
            //{
            //    ViewBag.MDF = "No";
            //}

            //Get The MDF information
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model = await dbEntity.mdf_pinnacle_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == null).ToListAsync();
            List<MdfParts> prom = comm.MDFsActivities(mdf_pinnacle_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
            if (mdf_pinnacle_main_Model.Count() > 0)
            {
                MDFPinnacleViewModels.mdf_comp = mdf_pinnacle_main_Model.FirstOrDefault().mdf_comp;
            }
            MDFPinnacleViewModels.topdesc = topdesc.n2_descLong;
            MDFPinnacleViewModels.mdf_parts = prom;
            MDFPinnacleViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
            MDFPinnacleViewModels.company = partnerCompanyViewModel.comp_name;

            return View(MDFPinnacleViewModels);


            //return View(MDFPinnacleViewModels);
        }
        #endregion

        #region Submit forms
        // GET: MDFView
        public ActionResult Ap()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            long? companyId = Convert.ToInt32(Session["companyId"]);
            string usr_comp = db.partnerCompanyViewModels.Where(a => a.comp_ID == companyId).FirstOrDefault().comp_name;
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a=>a.comp_ID == companyId);
                           
            MDFPinnacleViewModel MDFPinnacleViewModel = new MDFPinnacleViewModel();
            MDFPinnacleViewModel.partner_loc = usr_loc;
            MDFPinnacleViewModel.mdf_comp = companyId;
            MDFPinnacleViewModel.company = usr_comp;

            return View(MDFPinnacleViewModel);
        }

        // POST: MDFView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Ap([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_validatedAmt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type,mdf_requestType")] MDFPinnacleViewModel MDFPinnacleViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            if (ModelState.IsValid)
            {
                int userId = Convert.ToInt32(Session["userId"]);
                int languageId = Convert.ToInt32(Session["userLanguageId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrEmpty(Request.Form["userEdit"]))
                {
                    MDFPinnacleViewModel.mdf_requestDate = DateTime.Now;
                }

                if (MDFPinnacleViewModel.mdf_type == 5 && Request.Form["prod_type"] == "1")
                {
                    MDFPinnacleViewModel.mdf_desc = "WM Wallmount Display Enclosure (9967.851)";
                    MDFPinnacleViewModel.mdf_title = "Marketing Enclosures";
                    MDFPinnacleViewModel.mdf_totalCost = 1000.00;
                    MDFPinnacleViewModel.mdf_mdfCost = 1000.00;
                }
                else if (MDFPinnacleViewModel.mdf_type == 5 && Request.Form["prod_type"] == "2")
                {
                    MDFPinnacleViewModel.mdf_desc = "Mini TS8 Display Enclosure (9967.898)";
                    MDFPinnacleViewModel.mdf_title = "Marketing Enclosures";
                    MDFPinnacleViewModel.mdf_totalCost = 1000.00;
                    MDFPinnacleViewModel.mdf_mdfCost = 1000.00;
                }
                else if (MDFPinnacleViewModel.mdf_type == 5 && Request.Form["prod_type"] == "3")
                {
                    MDFPinnacleViewModel.mdf_desc = "Hygienic Design Enclosure (9973.113)";
                    MDFPinnacleViewModel.mdf_title = "Marketing Enclosures";
                    MDFPinnacleViewModel.mdf_totalCost = 1000.00;
                    MDFPinnacleViewModel.mdf_mdfCost = 1000.00;
                }
                else if (MDFPinnacleViewModel.mdf_type == 5 && Request.Form["prod_type"] == "4")
                {
                    MDFPinnacleViewModel.mdf_desc = "RiLine Compact Sample Kit (2996.001)";
                    MDFPinnacleViewModel.mdf_title = "Marketing Enclosures";
                    MDFPinnacleViewModel.mdf_totalCost = 1000.00;
                    MDFPinnacleViewModel.mdf_mdfCost = 1000.00;
                }

                db.MDFPinnacleViewModels.Add(MDFPinnacleViewModel);
                await db.SaveChangesAsync();
                MDFPinnaclefiles mdf_files = new MDFPinnaclefiles();
                await SaveImages(fileupload, MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel.mdf_file_type, mdf_files);

                //Get user email
                //declare email variables
                string host = "";
                if (Request.Url.Port != 443)
                {
                    host = "http://" + Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = "https://" + Request.Url.Host;
                }
                string header = comm.emailheader(host);
                string footer = comm.emailfooter(host);
                ViewBag.host = host;
                var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                string body_addon = "The MDF has been submitted and it is pending <br />";
                string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                //send email out when the MDF is approved
                sendMDFEmail(body_addon + email_body, header, footer, email, title);
                
                return RedirectToAction("Index", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(MDFPinnacleViewModel);
        }

        // POST: MDFView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApEdit([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_validatedAmt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type")] MDFPinnacleViewModel MDFPinnacleViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            if (ModelState.IsValid)
            {
                int userId = Convert.ToInt32(Session["userId"]);
                int languageId = Convert.ToInt32(Session["userLanguageId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                //declare email variables
                string host = "";
                if (Request.Url.Port != 443)
                {
                    host = "http://" + Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = "https://" + Request.Url.Host;
                }
                string header = comm.emailheader(host);
                string footer = comm.emailfooter(host);
                ViewBag.host = host;

                if(fileupload != null && fileupload.Count() > 0)
                {
                    MDFPinnaclefiles mdf_files = new MDFPinnaclefiles();
                    await SaveImages(fileupload, Convert.ToInt32(MDFPinnacleViewModel.mdf_ID), Convert.ToInt32(MDFPinnacleViewModel.mdf_file_type), mdf_files);
                }

                if (!string.IsNullOrEmpty(Request.Form["userEdit"]))
                {
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string body_addon = "The MDF has been edited and the status is pending <br />";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                        //send email out when the MDF is approved
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manage", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userApproved"]) && MDFPinnacleViewModel.mdf_status == 3)
                {
                    MDFPinnacleViewModel.mdf_reviewDate = DateTime.Now;
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"]=="1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - "+ Request.Form["subtype"] + " - Update 1";
                        string body_addon = "The MDF has been completed <br />";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                        //send email out when the MDF is approved
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userCompleted"]) && MDFPinnacleViewModel.mdf_status == 4)
                {
                    MDFPinnacleViewModel.mdf_validationDate = DateTime.Now;
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string body_addon = "The MDF has been completed <br />";
                        string email = usr_data.usr_email + ",jung.b@rittal.us,accountsreceivable@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail( body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userCreditIssued"]) && MDFPinnacleViewModel.mdf_status == 5)
                {
                    MDFPinnacleViewModel.mdf_creditIssueDate = DateTime.Now;
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string body_addon = "Credit has been issued <br />";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }
                else if (MDFPinnacleViewModel.mdf_status == 6)
                {
                    MDFPinnacleViewModel.mdf_status = 6;
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string body_addon = "The MDF has been cancelled <br />";
                        string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }
                else if (MDFPinnacleViewModel.mdf_status == 2)
                {
                    MDFPinnacleViewModel.mdf_status = 2;
                    db.Entry(MDFPinnacleViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == MDFPinnacleViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + MDFPinnacleViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(MDFPinnacleViewModel.mdf_ID, MDFPinnacleViewModel, usr_data);
                        string body_addon = "The MDF has been denied <br />";
                        string email = usr_data.usr_email + ",jung.b@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
                }

                return RedirectToAction("manageAdmin", new { mdf_ID = MDFPinnacleViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "MDF has been updated successfully" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(MDFPinnacleViewModel);
        }
        #endregion


        #region User Manage
        // GET: MDFView
        public async Task<ActionResult> Manage( int year = 0)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int? archive_year = null;
            if (year != 0)
            {
                archive_year = year;
            }

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModels = new MDFPinnacleViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();// Get page text
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(companyId);//Get company data
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            string type_mdf = "";
            //if (partnerCompanyViewModel.comp_MDF_amount > 0)
            //{
                //Get The MDF information
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model = await dbEntity.mdf_pinnacle_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == archive_year).ToListAsync();
            List<MdfParts> prom = comm.MDFsActivities(mdf_pinnacle_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
            if (mdf_pinnacle_main_Model.Count() > 0)
            {
                MDFPinnacleViewModels.mdf_comp = mdf_pinnacle_main_Model.FirstOrDefault().mdf_comp;
            }
            MDFPinnacleViewModels.topdesc = topdesc.n2_descLong;
            MDFPinnacleViewModels.mdf_parts = prom;
            MDFPinnacleViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
            MDFPinnacleViewModels.company = partnerCompanyViewModel.comp_name;

            List<MDFPinnacleViewModel> list_MDFPinnacleViewModels = new List<MDFPinnacleViewModel>();

            foreach (var mdf in mdf_pinnacle_main_Model)
            {
                var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                var type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                if (type.Count() > 0)
                {
                    type_mdf = type.FirstOrDefault().mdf_subType_name;
                }

                list_MDFPinnacleViewModels.Add(new MDFPinnacleViewModel {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status
                });
            }

            MDFPinnacleViewModels.mdf_main = list_MDFPinnacleViewModels;

            return View(MDFPinnacleViewModels);
            //}

            //return View(MDFPinnacleViewModels);
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
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
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            if (MDFPinnacleViewModel == null)
            {
                return HttpNotFound();
            }
            string type_mdf = "";
            //Get files per MDF
            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);
            MDFPinnacleViewModel.list_mdf_files = files;
            //Get the MDF types
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            var type = sub_type.Where(a => a.mdf_type_ID == MDFPinnacleViewModel.mdf_type);
            if (type.Count() > 0)
            {
                type_mdf = type.FirstOrDefault().mdf_subType_name;
            }

            MDFPinnacleViewModel.request_type = type_mdf;
            MDFPinnacleViewModel.requester = fullname["fullName"];

            return View(MDFPinnacleViewModel);
        }
        #endregion

        #region User Manage Archive
        // GET: MDFView
        public async Task<ActionResult> ManageArchive(int year = 0)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int? archive_year = null;
            if (year != 0)
            {
                archive_year = year;
            }

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModels = new MDFPinnacleViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();// Get page text
            partnerCompany_Archive partnerCompanyViewModel = await dbEntity.partnerCompany_Archive.Where(a=>a.comp_ID==companyId && archive_year==year).FirstOrDefaultAsync();//Get company data
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            string type_mdf = "";
            MDFPinnacleViewModels.mdf_main = new List<MDFPinnacleViewModel>();
            if (partnerCompanyViewModel != null && partnerCompanyViewModel.comp_MDF_amount > 0)
            {
                //Get The MDF information
                IList<mdf_pinnacle_main> mdf_pinnacle_main_Model = await dbEntity.mdf_pinnacle_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == archive_year).ToListAsync();
                List<MdfParts> prom = comm.MDFsActivitiesArchive(mdf_pinnacle_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
                if (mdf_pinnacle_main_Model.Count() > 0)
                {
                    MDFPinnacleViewModels.mdf_comp = mdf_pinnacle_main_Model.FirstOrDefault().mdf_comp;
                }
                MDFPinnacleViewModels.topdesc = topdesc.n2_descLong;
                MDFPinnacleViewModels.mdf_parts = prom;
                MDFPinnacleViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
                MDFPinnacleViewModels.company = partnerCompanyViewModel.comp_name;

                List<MDFPinnacleViewModel> list_MDFPinnacleViewModels = new List<MDFPinnacleViewModel>();

                foreach (var mdf in mdf_pinnacle_main_Model)
                {
                    var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                    var type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                    if (type.Count() > 0)
                    {
                        type_mdf = type.FirstOrDefault().mdf_subType_name;
                    }

                    list_MDFPinnacleViewModels.Add(new MDFPinnacleViewModel
                    {
                        mdf_ID = mdf.mdf_ID,
                        request_type = type_mdf,
                        requester = fullname["fullName"],
                        mdf_totalCost = mdf.mdf_totalCost,
                        mdf_mdfCost = mdf.mdf_mdfCost,
                        mdf_approvedAmt = mdf.mdf_approvedAmt,
                        mdf_validatedAmt = mdf.mdf_validatedAmt,
                        mdf_requestDate = mdf.mdf_requestDate,
                        mdf_title = mdf.mdf_title,
                        mdf_status = mdf.mdf_status
                    });
                }

                MDFPinnacleViewModels.mdf_main = list_MDFPinnacleViewModels;

                return View(MDFPinnacleViewModels);
            }

            return View(MDFPinnacleViewModels);
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> DetailsArchive(long? id)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
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
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            if (MDFPinnacleViewModel == null)
            {
                return HttpNotFound();
            }
            string type_mdf = "";
            //Get files per MDF
            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);
            MDFPinnacleViewModel.list_mdf_files = files;
            //Get the MDF types
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            var type = sub_type.Where(a => a.mdf_type_ID == MDFPinnacleViewModel.mdf_type);
            if (type.Count() > 0)
            {
                type_mdf = type.FirstOrDefault().mdf_subType_name;
            }

            MDFPinnacleViewModel.request_type = type_mdf;
            MDFPinnacleViewModel.requester = fullname["fullName"];

            return View(MDFPinnacleViewModel);
        }
        #endregion

        #region MDF Admin
        [HttpGet]
        public async Task<ActionResult> ManualEntry()
        {
            MDFPinnacleViewModel MDFPinnacleViewModel = new MDFPinnacleViewModel();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            return View(MDFPinnacleViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManualEntry([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_validatedAmt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type,usr_email")] MDFPinnacleViewModel MDFPinnacleViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(MDFPinnacleViewModel.usr_email))
                {
                    return RedirectToAction("ManualEntry", new {  n1_name = Request.Form["n1_name"], n2_name = "Admin", msg = Request.Form["msg"], success = "The email can not be empty" });
                }
                //find user data
                var usr_data = await db.UserViewModels.Where(a=>a.usr_email==MDFPinnacleViewModel.usr_email).FirstOrDefaultAsync();
                MDFPinnacleViewModel.mdf_user = usr_data.usr_ID;
                MDFPinnacleViewModel.mdf_comp = usr_data.comp_ID;
                MDFPinnacleViewModel.mdf_status = 1;

                db.MDFPinnacleViewModels.Add(MDFPinnacleViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("DetailsAdmin", new { id = MDFPinnacleViewModel.mdf_ID, n1_name = Request.Form["n1_name"], n2_name = "Admin", msg = "Details for "+ MDFPinnacleViewModel.mdf_ID, success = "MDF has been added successfully" });
            }

            return View(MDFPinnacleViewModel);
        }

        // GET: MDFView
        public async Task<ActionResult> ManageAdmin(int year = 0,int comp_id=0, int status=0, int type=0, string request_type = "")
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int? archive_year = null;
            if (year != 0)
            {
                archive_year = year;
            }

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModels = new MDFPinnacleViewModel();
            // Get page text
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();
            MDFPinnacleViewModels.topdesc = topdesc.n2_descLong;
            //Get company data
            var partnerCompany = await db.partnerCompanyViewModels.ToListAsync();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;
            ViewBag.newTypes = sub_type.Where(x => (!string.IsNullOrEmpty(x.mdf_type_desc) && x.mdf_type_desc.ToLower() == "new")).ToList();
            ViewBag.oldTypes = sub_type.Where(x => (string.IsNullOrEmpty(x.mdf_type_desc) || x.mdf_type_desc.ToLower() != "new")).ToList();
            string type_mdf = "";
            string company = "";
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model;
            IQueryable<mdf_pinnacle_main> mdf_pinnacle_main = dbEntity.mdf_pinnacle_main;
            List<MdfParts> company_metrics;
            //Get The MDF information
            if (comp_id != 0)
            {
                //Get MDF by company
                if (status!=0 && type==0)
                {
                    //Filter only by status
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_status==status).ToListAsync();
                    partnerCompanyViewModel company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                    company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type==type).ToListAsync();
                    partnerCompanyViewModel company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                    company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type && a.mdf_status == status).ToListAsync();
                    partnerCompanyViewModel company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                    company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else
                {
                    //filter by only company
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id).ToListAsync();
                    partnerCompanyViewModel company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                    company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }

            }
            else
            {
                //Get all the MDFs
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_status == status).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_type == type ).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_type == type && a.mdf_status == status).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else
                {
                    //filter by only company
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }

            }

            //List the companies that are in the MDFs
            List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
            foreach (var mdf in mdf_pinnacle_main.Where(a=>a.archive_year== archive_year))
            {
                if (partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                {
                    list_comp.Add(new partnerCompanyViewModel { comp_ID = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID, comp_name = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                }
            }

            //Add to the list of MDFs for the table
            List<MDFPinnacleViewModel> list_MDFPinnacleViewModels = new List<MDFPinnacleViewModel>();
            foreach (var mdf in mdf_pinnacle_main_Model)
            {
                if (request_type != "" && mdf.mdf_requestType != request_type)
                    continue;
                if (partnerCompany.Where(a=>a.comp_ID==mdf.mdf_comp).Count() > 0)
                {
                    company = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name;
                }
                var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                var get_type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                if (get_type.Count() > 0)
                {
                    type_mdf = get_type.FirstOrDefault().mdf_subType_name;
                }

                list_MDFPinnacleViewModels.Add(new MDFPinnacleViewModel
                {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_date = mdf.mdf_date,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status,
                    company = company,
                    mdf_requestType = mdf.mdf_requestType
                });
            }

            MDFPinnacleViewModels.mdf_main = list_MDFPinnacleViewModels;
            MDFPinnacleViewModels.list_comp = list_comp;
            MDFPinnacleViewModels.company_metrics = company_metrics;

            return View(MDFPinnacleViewModels);     
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> DetailsAdmin(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            if (MDFPinnacleViewModel == null)
            {
                return HttpNotFound();
            }
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(MDFPinnacleViewModel.mdf_comp);//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == MDFPinnacleViewModel.mdf_comp);

            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);//Get images

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            MDFPinnacleViewModel.company = partnerCompanyViewModel.comp_name;
            MDFPinnacleViewModel.requester = fullname["fullName"];
            MDFPinnacleViewModel.partner_loc = usr_loc;
            MDFPinnacleViewModel.list_mdf_files = files;

            return View(MDFPinnacleViewModel);
        }
        #endregion

        #region MDF Admin Archive
        // GET: MDFView
        public async Task<ActionResult> ManageAdminArchive(string year, int comp_id = 0, int status = 0, int type = 0, string request_type = "")
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int? archive_year = Convert.ToInt32(year);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModels = new MDFPinnacleViewModel();
            // Get page text
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();
            MDFPinnacleViewModels.topdesc = topdesc.n2_descLong;
            //Get company data
            var partnerCompany = await dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year).ToListAsync();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;
            ViewBag.newTypes = sub_type.Where(x => (!string.IsNullOrEmpty(x.mdf_type_desc) && x.mdf_type_desc.ToLower() == "new")).ToList();
            ViewBag.oldTypes = sub_type.Where(x => (string.IsNullOrEmpty(x.mdf_type_desc) || x.mdf_type_desc.ToLower() != "new")).ToList();
            string type_mdf = "";
            string company = "";
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model;
            IQueryable<mdf_pinnacle_main> mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a=>a.archive_year==archive_year);
            List<MdfParts> company_metrics;
            //Get The MDF information
            if (comp_id != 0)
            {
                //Get MDF by company
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_status == status).ToListAsync();
                    partnerCompany_Archive company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                    company_metrics = comm.MDFsActivitiesArchive(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type).ToListAsync();
                    partnerCompany_Archive company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                    company_metrics = comm.MDFsActivitiesArchive(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type && a.mdf_status == status).ToListAsync();
                    partnerCompany_Archive company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                    company_metrics = comm.MDFsActivitiesArchive(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }
                else
                {
                    //filter by only company
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id).ToListAsync();
                    partnerCompany_Archive company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                    company_metrics = comm.MDFsActivitiesArchive(mdf_pinnacle_main_Model, company_data);//Calculate the percentages and mdf utilized
                }

            }
            else
            {
                //Get all the MDFs
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_status == status).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_type == type).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_type == type && a.mdf_status == status).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }
                else
                {
                    //filter by only company
                    mdf_pinnacle_main_Model = await mdf_pinnacle_main.Where(a => a.archive_year == archive_year).ToListAsync();
                    company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
                }

            }

            //List the companies that are in the MDFs
            List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
            foreach (var mdf in mdf_pinnacle_main.Where(a => a.archive_year == archive_year))
            {
                if (partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                {
                    long comp = Convert.ToInt32(partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID);
                    list_comp.Add(new partnerCompanyViewModel { comp_ID = comp, comp_name = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                }
            }

            //Add to the list of MDFs for the table
            List<MDFPinnacleViewModel> list_MDFPinnacleViewModels = new List<MDFPinnacleViewModel>();
            foreach (var mdf in mdf_pinnacle_main_Model)
            {
                if (request_type != "" && mdf.mdf_requestType != request_type)
                    continue;
                if (partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                {
                    company = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name;
                }
                var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                var get_type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                if (get_type.Count() > 0)
                {
                    type_mdf = get_type.FirstOrDefault().mdf_subType_name;
                }

                list_MDFPinnacleViewModels.Add(new MDFPinnacleViewModel
                {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_date = mdf.mdf_date,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status,
                    company = company,
                    mdf_requestType = mdf.mdf_requestType
                });
            }

            MDFPinnacleViewModels.mdf_main = list_MDFPinnacleViewModels;
            MDFPinnacleViewModels.list_comp = list_comp;
            MDFPinnacleViewModels.company_metrics = company_metrics;

            return View(MDFPinnacleViewModels);
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> DetailsAdminArchive(long? id,string year)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            if (MDFPinnacleViewModel == null)
            {
                return HttpNotFound();
            }
            partnerCompany_Archive partnerCompanyViewModel = dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year && a.comp_ID==MDFPinnacleViewModel.mdf_comp).FirstOrDefault();//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == MDFPinnacleViewModel.mdf_comp);

            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);//Get images

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            MDFPinnacleViewModel.company = partnerCompanyViewModel.comp_name;
            MDFPinnacleViewModel.requester = fullname["fullName"];
            MDFPinnacleViewModel.partner_loc = usr_loc;
            MDFPinnacleViewModel.list_mdf_files = files;

            return View(MDFPinnacleViewModel);
        }
        #endregion

        #region MDF files
        async Task SaveImages(IEnumerable<HttpPostedFileBase> fileupload, long mdf_id, int mdf_file_type, MDFPinnaclefiles mdf_files)
        {
            //Process the attached images
            foreach (HttpPostedFileBase file in fileupload)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file.FileName);
                    var NewFileName = mdf_id +"_"+ fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/mdf_pinnacle/images"), NewFileName);

                    file.SaveAs(path);
                    //if file_id is not set or the entry is empty
                    mdf_files = new MDFPinnaclefiles
                    {
                        mdf_ID = mdf_id,
                        mdf_file_type = Convert.ToByte(mdf_file_type),
                        mdf_file_name = NewFileName
                    };
                    db.mdfPinnacleFiles.Add(mdf_files);
                    await db.SaveChangesAsync();
                }
            }
        }

        async Task<IEnumerable<MDFPinnaclefiles>> getImages(long mdf_ID)
        {
            IEnumerable<MDFPinnaclefiles> files = await db.mdfPinnacleFiles.Where(a => a.mdf_ID == mdf_ID).ToListAsync();

            return files;
        }
        #endregion

        #region MDF email
        public void sendMDFEmail(string email_body, string email_header, string email_footer,string to_email,string title)
        {
            //Send email to the Return Requester
            MailMessage message_requester = new MailMessage("webmaster@rittal.us", to_email, title, email_header + email_body + email_footer);
            //send mail
            message_requester.IsBodyHtml = true;
            //Send the message.
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["Host"]);
            // Add credentials if the SMTP server requires them.
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            try
            {
                client.EnableSsl = false;
                client.Credentials = new NetworkCredential("", "");
                client.Send(message_requester);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateMessageWithAttachment(): {0}", ex.ToString());
            }
        }

        //Email body Test
        public async Task<ActionResult> emailbody(int mdf_id)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(mdf_id);
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model = await dbEntity.mdf_pinnacle_main.Where(a => a.mdf_comp == mdf_id && a.archive_year == null).ToListAsync();

            if (mdf_pinnacle_main_Model == null)
            {
                return HttpNotFound();
            }
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(MDFPinnacleViewModel.mdf_comp);//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == MDFPinnacleViewModel.mdf_comp);
            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);//Get images
            List<MdfParts> company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            MDFPinnacleViewModel.company = partnerCompanyViewModel.comp_name;
            MDFPinnacleViewModel.requester = fullname["fullName"];
            MDFPinnacleViewModel.partner_loc = usr_loc;
            MDFPinnacleViewModel.list_mdf_files = files;
            MDFPinnacleViewModel.company_metrics = company_metrics;

            return View(MDFPinnacleViewModel);
        }

        private async Task<string> emailbody(long? mdf_id, MDFPinnacleViewModel MDFPinnacleViewModel,UserViewModel usr_data)
        {
            //MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(mdf_ID);
            List<mdf_pinnacle_main> mdf_pinnacle_main_Model = await dbEntity.mdf_pinnacle_main.Where(a => a.mdf_ID == mdf_id && a.archive_year == null).ToListAsync();
            //Get company data
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(MDFPinnacleViewModel.mdf_comp);
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == MDFPinnacleViewModel.mdf_comp);
            //Get images
            IEnumerable<MDFPinnaclefiles> files = await getImages(MDFPinnacleViewModel.mdf_ID);
            //Calculate the percentages and mdf utilized
            List<MdfParts> company_metrics = comm.MDFsActivities(mdf_pinnacle_main_Model, partnerCompanyViewModel);

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(MDFPinnacleViewModel.mdf_user));
            MDFPinnacleViewModel.company = partnerCompanyViewModel.comp_name;
            MDFPinnacleViewModel.requester = fullname["fullName"];
            MDFPinnacleViewModel.partner_loc = usr_loc;
            MDFPinnacleViewModel.list_mdf_files = files;
            MDFPinnacleViewModel.company_metrics = company_metrics;

            String messageView = comm.RenderViewToString(ControllerContext, "~/Views/MDFViewPinnacle/emailbody.cshtml", MDFPinnacleViewModel, true);
            return messageView;
        }
        #endregion

        #region MDF Balance Report
        public ActionResult ExpExcl()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> BalanceReportForm(string year=null)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            string comp_type = Request.Form["compType"];
            string dateType = Request.Form["dateType"];
            string sDate = Request.Form["sDate"];
            string eDate = Request.Form["eDate"];

            List<MdfParts> company_metrics;
            List<mdf_pinnacle_main> mdf_pinnacle_main;

            if (string.IsNullOrEmpty(year))
            {
                IQueryable<partnerCompany> partnerCompLoop = dbEntity.partnerCompanies;//Get company data
                IQueryable<partnerCompanyViewModel> partnerComp = db.partnerCompanyViewModels;//Get company data
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate).ToList();
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_date >= ssDate && a.mdf_date <= eeDate).ToList();
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate).ToList();
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate).ToList();
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate).ToList();
                }
                else
                {
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null).ToList();
                }

                IEnumerable<partnerType> partner_type = await dbEntity.partnerTypes.ToListAsync();

                //List the companies that are in the MDFs
                //List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
                //foreach (var mdf in mdf_pinnacle_main)
                //{
                //    if (partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                //    {
                //        if (!string.IsNullOrEmpty(comp_type) && comp_type.Contains(partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_type.ToString()))
                //        {
                //            list_comp.Add(new partnerCompanyViewModel { comp_ID = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID, comp_name = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                //        }
                //        else if (string.IsNullOrEmpty(comp_type))
                //        {
                //            list_comp.Add(new partnerCompanyViewModel { comp_ID = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID, comp_name = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                //        }
                //    }
                //}

                var mdf_to_excel = new DataTable("ExpMDFs");
                mdf_to_excel.Columns.Add("Company Name", typeof(string));
                mdf_to_excel.Columns.Add("Company ID", typeof(int));
                mdf_to_excel.Columns.Add("Region", typeof(string));
                mdf_to_excel.Columns.Add("Company Type", typeof(string));
                mdf_to_excel.Columns.Add("Number of Requests", typeof(string));
                mdf_to_excel.Columns.Add("Total MDF", typeof(string));
                mdf_to_excel.Columns.Add("Promotional Activities Limit", typeof(string));
                mdf_to_excel.Columns.Add("RAS Investment Limit", typeof(string));
                mdf_to_excel.Columns.Add("Tradeshows & Events Limit", typeof(string));
                mdf_to_excel.Columns.Add("Rittal Training for Distributor Employees Limit", typeof(string));
                mdf_to_excel.Columns.Add("Sales Performance Incentive Limit", typeof(string));
                mdf_to_excel.Columns.Add("Display Products Limit", typeof(string));
                mdf_to_excel.Columns.Add("Amount Pending", typeof(string));
                mdf_to_excel.Columns.Add("Amount Approved", typeof(string));
                mdf_to_excel.Columns.Add("Amount Completed", typeof(string));
                mdf_to_excel.Columns.Add("Amount Utilized", typeof(string));
                mdf_to_excel.Columns.Add("Amount Available", typeof(string));
                mdf_to_excel.Columns.Add("Amount Credited", typeof(string));

                foreach (var item in partnerCompLoop.OrderBy(a => a.comp_name))
                {
                    partnerCompanyViewModel company_data = await partnerComp.Where(a => a.comp_ID == item.comp_ID).FirstOrDefaultAsync();
                    var Company_mdf_pinnacle_main = mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_comp == item.comp_ID).ToList();
                    company_metrics = comm.MDFsActivities(Company_mdf_pinnacle_main, company_data);//Calculate the percentages and mdf utilized
                    //Get company type
                    string comp_type_name = string.Empty;
                    foreach (var type in partner_type)
                    {
                        if (item.comp_type == type.pt_ID)
                        {
                            comp_type_name = type.pt_type;
                            break;
                        }
                    }
                    //Count the mdfs
                    int count_mdfs = 0;
                    count_mdfs = mdf_pinnacle_main.Where(a => a.mdf_comp == item.comp_ID).Count();
                    mdf_to_excel.Rows.Add(item.comp_name, item.comp_ID, item.comp_region, comp_type_name, count_mdfs, item.comp_MDF_amount, item.comp_MDF_aLimit, item.comp_MDF_rLimit, item.comp_MDF_trLimit, item.comp_MDF_rtLimit, item.comp_MDF_sLimit, item.comp_MDF_dLimit, company_metrics.FirstOrDefault().totalPending, company_metrics.FirstOrDefault().totalApproved, company_metrics.FirstOrDefault().totalComplete, company_metrics.FirstOrDefault().totalMDFUtilized, company_metrics.FirstOrDefault().totalMDFAva, company_metrics.FirstOrDefault().totalCredit);
                }


                //foreach (var item in list_comp.GroupBy(a => new { a.comp_ID, a.comp_name }).OrderBy(b => b.Key.comp_name))
                //{
                //    partnerCompanyViewModel company_data = await partnerComp.Where(a => a.comp_ID == item.Key.comp_ID).FirstOrDefaultAsync();
                //    var Company_mdf_pinnacle_main = mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_comp == item.Key.comp_ID).ToList();
                //    company_metrics = comm.MDFsActivities(Company_mdf_pinnacle_main, company_data);//Calculate the percentages and mdf utilized
                //                                                                          //Get company type
                //    string comp_type_name = string.Empty;
                //    foreach (var type in partner_type)
                //    {
                //        if (company_data.comp_type == type.pt_ID)
                //        {
                //            comp_type_name = type.pt_type;
                //            break;
                //        }
                //    }
                //    //Count the mdfs
                //    int count_mdfs = 0;
                //    count_mdfs = mdf_pinnacle_main.Where(a => a.mdf_comp == item.Key.comp_ID).Count();
                //    mdf_to_excel.Rows.Add(item.Key.comp_ID, company_data.comp_name, company_data.comp_region, comp_type_name, count_mdfs, company_data.comp_MDF_amount, company_data.comp_MDF_aLimit, company_data.comp_MDF_oLimit, company_data.comp_MDF_tLimit, company_data.comp_MDF_mLimit, company_data.comp_MDF_dLimit, company_metrics.FirstOrDefault().totalPending, company_metrics.FirstOrDefault().totalApproved, company_metrics.FirstOrDefault().totalComplete, company_metrics.FirstOrDefault().totalMDFUtilized, company_metrics.FirstOrDefault().totalMDFAva, company_metrics.FirstOrDefault().totalCredit);
                //}

                var grid = new GridView();
                grid.DataSource = mdf_to_excel;
                grid.DataBind();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement; filename=MDF_balance_report_current.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            else
            {
                int archive_year = Convert.ToInt32(year);
                //IQueryable<partnerCompany> partnerCompLoop = dbEntity.partnerCompanies;//Get company data
                IQueryable<partnerCompany_Archive> partnerComp = dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year);//Get company data
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);

                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate).ToList();
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_date >= ssDate && a.mdf_date <= eeDate).ToList();
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate).ToList();
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate).ToList();
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate).ToList();
                }
                else
                {
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year).ToList();
                }

                IEnumerable<partnerType> partner_type = await dbEntity.partnerTypes.ToListAsync();

                //List the companies that are in the MDFs
                //List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
                //foreach (var mdf in mdf_pinnacle_main)
                //{
                //    if (partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                //    {
                //        long comp = Convert.ToInt32(partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID);
                //        if (!string.IsNullOrEmpty(comp_type) && comp_type.Contains(partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_type.ToString()))
                //        {
                //            list_comp.Add(new partnerCompanyViewModel { comp_ID=comp, comp_name = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                //        }
                //        else if (string.IsNullOrEmpty(comp_type))
                //        {
                //            list_comp.Add(new partnerCompanyViewModel { comp_ID = comp, comp_name = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                //        }
                //    }
                //}

                var mdf_to_excel = new DataTable("ExpMDFs");
                mdf_to_excel.Columns.Add("Company Name", typeof(string));
                mdf_to_excel.Columns.Add("Company ID", typeof(int));
                mdf_to_excel.Columns.Add("Region", typeof(string));
                mdf_to_excel.Columns.Add("Company Type", typeof(string));
                mdf_to_excel.Columns.Add("Number of Requests", typeof(string));
                mdf_to_excel.Columns.Add("Total MDF", typeof(string));
                mdf_to_excel.Columns.Add("Promotional Activities Limit", typeof(string));
                mdf_to_excel.Columns.Add("Other Activities Limit", typeof(string));
                mdf_to_excel.Columns.Add("Training Limit", typeof(string));
                mdf_to_excel.Columns.Add("Merchandise Limit", typeof(string));
                mdf_to_excel.Columns.Add("Display Products Limit", typeof(string));
                mdf_to_excel.Columns.Add("RAS Investment Limit", typeof(string));
                mdf_to_excel.Columns.Add("Tradeshows & Events Limit", typeof(string));
                mdf_to_excel.Columns.Add("Rittal Training for Distributor Employees Limit", typeof(string));
                mdf_to_excel.Columns.Add("Sales Performance Incentive Limit", typeof(string));
                mdf_to_excel.Columns.Add("Amount Pending", typeof(string));
                mdf_to_excel.Columns.Add("Amount Approved", typeof(string));
                mdf_to_excel.Columns.Add("Amount Completed", typeof(string));
                mdf_to_excel.Columns.Add("Amount Utilized", typeof(string));
                mdf_to_excel.Columns.Add("Amount Available", typeof(string));
                mdf_to_excel.Columns.Add("Amount Credited", typeof(string));

                foreach (var item in partnerComp.OrderBy(a=>a.comp_name))
                {
                    partnerCompany_Archive company_data = await partnerComp.Where(a => a.comp_ID == item.comp_ID).FirstOrDefaultAsync();
                    var Company_mdf_pinnacle_main = mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_comp == item.comp_ID).ToList();
                    company_metrics = comm.MDFsActivitiesArchive(Company_mdf_pinnacle_main, company_data);//Calculate the percentages and mdf utilized
                                                                                                 //Get company type
                    string comp_type_name = string.Empty;
                    foreach (var type in partner_type)
                    {
                        if (company_data.comp_type == type.pt_ID)
                        {
                            comp_type_name = type.pt_type;
                            break;
                        }
                    }
                    //Count the mdfs
                    int count_mdfs = 0;
                    count_mdfs = mdf_pinnacle_main.Where(a => a.mdf_comp == item.comp_ID).Count();
                    mdf_to_excel.Rows.Add(company_data.comp_name, item.comp_ID, company_data.comp_region, comp_type_name, count_mdfs, company_data.comp_MDF_amount, company_data.comp_MDF_aLimit, company_data.comp_MDF_oLimit, company_data.comp_MDF_tLimit, company_data.comp_MDF_mLimit, company_data.comp_MDF_dLimit, company_data.comp_MDF_rLimit, company_data.comp_MDF_trLimit,company_data.comp_MDF_rtLimit,company_data.comp_MDF_sLimit, company_metrics.FirstOrDefault().totalPending, company_metrics.FirstOrDefault().totalApproved, company_metrics.FirstOrDefault().totalComplete, company_metrics.FirstOrDefault().totalMDFUtilized, company_metrics.FirstOrDefault().totalMDFAva, company_metrics.FirstOrDefault().totalCredit);
                }

                var grid = new GridView();
                grid.DataSource = mdf_to_excel;
                grid.DataBind();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement; filename=MDF_balance_report_"+archive_year+".xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            return View();
        }
        #endregion

        #region MDF Request Report
        [HttpPost]
        public async Task<ActionResult> RequestReportForm(string year=null)
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            string comp_type = Request.Form["compType"];
            string dateType = Request.Form["dateType"];
            string sDate = Request.Form["sDate"];
            string eDate = Request.Form["eDate"];
            string reqType = Request.Form["reqType"];
            string reqStatus = Request.Form["reqStatus"];

            IQueryable<mdf_pinnacle_main> mdf_pinnacle_main;
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();

            if (string.IsNullOrEmpty(year))
            {
                IQueryable<partnerCompanyViewModel> partnerComp = db.partnerCompanyViewModels;//Get company data
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate);
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_date >= ssDate && a.mdf_date <= eeDate);
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate);
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate);
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate);
                }
                else
                {
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == null);
                }

                var mdf_to_excel = new DataTable("ExpMDFs");
                mdf_to_excel.Columns.Add("Company ID", typeof(int));
                mdf_to_excel.Columns.Add("Region", typeof(string));
                mdf_to_excel.Columns.Add("Company Name", typeof(string));
                mdf_to_excel.Columns.Add("User Name", typeof(string));
                mdf_to_excel.Columns.Add("User Email", typeof(string));
                mdf_to_excel.Columns.Add("Req No.", typeof(string));
                mdf_to_excel.Columns.Add("Status", typeof(string));
                mdf_to_excel.Columns.Add("Type", typeof(string));
                mdf_to_excel.Columns.Add("Title", typeof(string));
                mdf_to_excel.Columns.Add("SAP", typeof(string));
                mdf_to_excel.Columns.Add("Total Cost", typeof(string));
                mdf_to_excel.Columns.Add("Total MDF", typeof(string));
                mdf_to_excel.Columns.Add("Request Date", typeof(string));
                mdf_to_excel.Columns.Add("Expected Date", typeof(string));
                mdf_to_excel.Columns.Add("Amount Approved", typeof(string));
                mdf_to_excel.Columns.Add("Reviewed Date", typeof(string));
                mdf_to_excel.Columns.Add("Validated Amount", typeof(string));
                mdf_to_excel.Columns.Add("Validation Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issue Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Memo Number", typeof(string));

                //List the companies that are in the MDFs
                List<MDFPinnacleViewModel> list_mdfs = new List<MDFPinnacleViewModel>();
                foreach (var mdf in mdf_pinnacle_main)
                {
                    if (partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                    {
                        //Get company type
                        string mdf_type_name = string.Empty;
                        foreach (var type in sub_type)
                        {
                            if (mdf.mdf_type == type.mdf_type_ID)
                            {
                                mdf_type_name = type.mdf_subType_name;
                                break;
                            }
                        }

                        var usr_data = await dbEntity.usr_user.Where(a => a.usr_ID == mdf.mdf_user).FirstOrDefaultAsync();
                        var company_data = await partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefaultAsync();
                        string cp_type = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_type.ToString();

                        if (usr_data!=null) {
                            if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                            {
                                //Filter only by company Type
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()))
                            {
                                //Filter only by request Type
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter only by status Type
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                            {
                                if (reqType.Contains(mdf.mdf_type.ToString()))
                                {
                                    //Filter by company type and request Type
                                    list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter by company type and status Type
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter by request type and status Type
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else
                        {
                            list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = "", usr_lName = "", usr_email = "", mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                        }
                    }
                }

                foreach (var mdf in list_mdfs.OrderBy(a => a.company))
                {
                    //Build MDf status
                    string status = string.Empty;
                    if (mdf.mdf_status == 1)
                    {
                        status = "Pending";
                    }
                    else if (mdf.mdf_status == 2)
                    {
                        status = "Denied";
                    }
                    else if (mdf.mdf_status == 3)
                    {
                        status = "Approved";
                    }
                    else if (mdf.mdf_status == 4)
                    {
                        status = "Completed";
                    }
                    else if (mdf.mdf_status == 5)
                    {
                        status = "Credit Issued";
                    }
                    else if (mdf.mdf_status == 6)
                    {
                        status = "Canceled";
                    }
                    else
                    {
                        status = "In Progress";
                    }
                    mdf_to_excel.Rows.Add(mdf.mdf_comp, mdf.region, mdf.company, mdf.usr_fName + " " + mdf.usr_lName, mdf.usr_email, mdf.mdf_ID, status, mdf.request_type, mdf.mdf_title, mdf.mdf_SAP, mdf.mdf_totalCost, mdf.mdf_mdfCost, mdf.mdf_requestDate, mdf.mdf_date, mdf.mdf_approvedAmt, mdf.mdf_reviewDate, mdf.mdf_validatedAmt, mdf.mdf_validationDate, mdf.mdf_validatedAmt, mdf.mdf_creditIssueDate, mdf.mdf_creditMemoNum);
                }

                var grid = new GridView();
                grid.DataSource = mdf_to_excel;
                grid.DataBind();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement; filename=data.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            else
            {
                IQueryable<partnerCompany_Archive> partnerComp = dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year);//Get company data
                int archive_year = Convert.ToInt32(year);
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate);
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_date >= ssDate && a.mdf_date <= eeDate);
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate);
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate);
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate);
                }
                else
                {
                    mdf_pinnacle_main = dbEntity.mdf_pinnacle_main.Where(a => a.archive_year == archive_year);
                }


                var mdf_to_excel = new DataTable("ExpMDFs");
                mdf_to_excel.Columns.Add("Company ID", typeof(int));
                mdf_to_excel.Columns.Add("Region", typeof(string));
                mdf_to_excel.Columns.Add("Company Name", typeof(string));
                mdf_to_excel.Columns.Add("User Name", typeof(string));
                mdf_to_excel.Columns.Add("User Email", typeof(string));
                mdf_to_excel.Columns.Add("Req No.", typeof(string));
                mdf_to_excel.Columns.Add("Status", typeof(string));
                mdf_to_excel.Columns.Add("Type", typeof(string));
                mdf_to_excel.Columns.Add("Title", typeof(string));
                mdf_to_excel.Columns.Add("SAP", typeof(string));
                mdf_to_excel.Columns.Add("Total Cost", typeof(string));
                mdf_to_excel.Columns.Add("Total MDF", typeof(string));
                mdf_to_excel.Columns.Add("Request Date", typeof(string));
                mdf_to_excel.Columns.Add("Expected Date", typeof(string));
                mdf_to_excel.Columns.Add("Amount Approved", typeof(string));
                mdf_to_excel.Columns.Add("Reviewed Date", typeof(string));
                mdf_to_excel.Columns.Add("Validated Amount", typeof(string));
                mdf_to_excel.Columns.Add("Validation Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issue Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Memo Number", typeof(string));

                //List the companies that are in the MDFs
                List<MDFPinnacleViewModel> list_mdfs = new List<MDFPinnacleViewModel>();
                foreach (var mdf in mdf_pinnacle_main)
                {
                    if (partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                    {
                        //Get company type
                        string mdf_type_name = string.Empty;
                        foreach (var type in sub_type)
                        {
                            if (mdf.mdf_type == type.mdf_type_ID)
                            {
                                mdf_type_name = type.mdf_subType_name;
                                break;
                            }
                        }

                        var usr_data = await dbEntity.usr_user.Where(a => a.usr_ID == mdf.mdf_user).FirstOrDefaultAsync();
                        var company_data = await partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefaultAsync();
                        string cp_type = partnerComp.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_type.ToString();

                        if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                        {
                            //Filter only by company Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()))
                        {
                            //Filter only by request Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter only by status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name,mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                        {
                            if (reqType.Contains(mdf.mdf_type.ToString()))
                            {
                                //Filter by company type and request Type
                                if (usr_data == null)
                                {
                                    list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                                else
                                {
                                    list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter by company type and status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter by request type and status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            if (usr_data==null) {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFPinnacleViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }

                        }
                    }
                }

                foreach (var mdf in list_mdfs.OrderBy(a => a.company))
                {
                    //Build MDf status
                    string status = string.Empty;
                    if (mdf.mdf_status == 1)
                    {
                        status = "Pending";
                    }
                    else if (mdf.mdf_status == 2)
                    {
                        status = "Denied";
                    }
                    else if (mdf.mdf_status == 3)
                    {
                        status = "Approved";
                    }
                    else if (mdf.mdf_status == 4)
                    {
                        status = "Completed";
                    }
                    else if (mdf.mdf_status == 5)
                    {
                        status = "Credit Issued";
                    }
                    else if (mdf.mdf_status == 6)
                    {
                        status = "Canceled";
                    }
                    else
                    {
                        status = "In Progress";
                    }
                    mdf_to_excel.Rows.Add(mdf.mdf_comp, mdf.region, mdf.company, mdf.usr_fName + " " + mdf.usr_lName, mdf.usr_email, mdf.mdf_ID, status, mdf.request_type, mdf.mdf_title, mdf.mdf_SAP, mdf.mdf_totalCost, mdf.mdf_mdfCost, mdf.mdf_requestDate, mdf.mdf_date, mdf.mdf_approvedAmt, mdf.mdf_reviewDate, mdf.mdf_validatedAmt, mdf.mdf_validationDate, mdf.mdf_validatedAmt, mdf.mdf_creditIssueDate, mdf.mdf_creditMemoNum);
                }

                var grid = new GridView();
                grid.DataSource = mdf_to_excel;
                grid.DataBind();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement; filename=data.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            return View();
        }
        #endregion

        #region Delete MDF
        // GET: MDFView/Delete/5
        public async Task<ActionResult> DeleteFiles(long? id, string redirect = "")
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
            MDFPinnaclefiles MDFPinnaclefilesModel = await db.mdfPinnacleFiles.FindAsync(id);
            if (MDFPinnaclefilesModel == null)
            {
                return HttpNotFound();
            }
            db.mdfPinnacleFiles.Remove(MDFPinnaclefilesModel);
            await db.SaveChangesAsync();
            //Delete the file from the system 
            string sourcePath = string.Format("~/attachments/mdf_pinnacle/images/{0}", @MDFPinnaclefilesModel.mdf_file_name);
            if (System.IO.File.Exists(Server.MapPath(sourcePath)))
            {
                var path = Server.MapPath(sourcePath);
                System.IO.File.Delete(path);
            }

            return RedirectToAction((string.IsNullOrEmpty(redirect) ? "Details" : redirect), new { id = MDFPinnaclefilesModel.mdf_ID, n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The file has been deleted" });
        }

        // GET: MDFView/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            if (MDFPinnacleViewModel == null)
            {
                return HttpNotFound();
            }
            var mdfFile = db.mdfPinnacleFiles.Where(a => a.mdf_ID == id);
            if (mdfFile != null)
            {
                db.mdfPinnacleFiles.RemoveRange(mdfFile);
            }

            foreach (var item in mdfFile)
            {
                //Delete the file from the system 
                string sourcePath = string.Format("~/attachments/mdf_pinnacle/images/{0}", @item.mdf_file_name);
                if (System.IO.File.Exists(Server.MapPath(sourcePath)))
                {
                    var path = Server.MapPath(sourcePath);
                    System.IO.File.Delete(path);
                }
            }

            db.MDFPinnacleViewModels.Remove(MDFPinnacleViewModel);
            await db.SaveChangesAsync();

            return RedirectToAction("ManageAdmin", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The MDF has been deleted" });
        }

        // POST: MDFView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            MDFPinnacleViewModel MDFPinnacleViewModel = await db.MDFPinnacleViewModels.FindAsync(id);
            db.MDFPinnacleViewModels.Remove(MDFPinnacleViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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

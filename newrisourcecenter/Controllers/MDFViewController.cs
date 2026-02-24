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
using NPOI.SS.Formula.Functions;
using static iTextSharp.text.pdf.AcroFields;

namespace newrisourcecenter.Models
{
    public class MDFViewController : Controller
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
            MDFViewModel mdfViewModels = new MDFViewModel();
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
            List<mdf_main> mdf_main_Model = await dbEntity.mdf_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == null && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();
            List<MdfParts> prom = comm.MDFsActivities(mdf_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
            if (mdf_main_Model.Count() > 0)
            {
                mdfViewModels.mdf_comp = mdf_main_Model.FirstOrDefault().mdf_comp;
            }
            mdfViewModels.topdesc = topdesc.n2_descLong;
            mdfViewModels.mdf_parts = prom;
            mdfViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
            mdfViewModels.company = partnerCompanyViewModel.comp_name;

            return View(mdfViewModels);


            //return View(mdfViewModels);
        }
        #endregion

        #region List MDF Users
        public async Task<ActionResult> MDFUsers(int compid = 0)
        {
            int companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (compid==0)
            {
                compid = companyId;
            }
            //select the company list from the partnerCompanies database
            var companies = from list in db.partnerCompanyViewModels where list.comp_active != 0 orderby list.comp_name ascending select list;
            //Add the data to the comp_listing object
            List<CompData> comp_listing = new List<CompData>();
            foreach (var item in companies)//iterate the add function
            {
                comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
            }
            ViewBag.partnerComp = comp_listing;

            var mDF_users = await db.UserViewModels.Join(
               db.partnerCompanyViewModels,
               comp=>comp.comp_ID,
               usrs=>usrs.comp_ID,
               (usrs,comp )=> new{usrs,comp}
               ).Where(a=>a.usrs.comp_ID==compid).ToListAsync();
            List<MDFUsers> mdfUsers = new List<MDFUsers>();
            foreach (var item in mDF_users)
            {
                string adminstatus = "";
                string userstatus = "";
                if (!string.IsNullOrEmpty(item.usrs.usr_pages) && item.usrs.usr_pages.Contains("30125"))
                {
                     adminstatus = "MDF Admin";
                }
                if (!string.IsNullOrEmpty(item.usrs.usr_pages) && item.usrs.usr_pages.Contains("6"))
                {
                    userstatus = "MDF User";
                }
                mdfUsers.Add(new MDFUsers { usr_ID = item.usrs.usr_ID, comp_ID = item.comp.comp_ID, company = item.comp.comp_name, fullName = item.usrs.usr_fName + " " + item.usrs.usr_lName, userstatus = userstatus, adminstatus= adminstatus });
            }
            return View(mdfUsers);
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
                           
            MDFViewModel mdfViewModel = new MDFViewModel();
            mdfViewModel.partner_loc = usr_loc;
            mdfViewModel.mdf_comp = companyId;
            mdfViewModel.company = usr_comp;

            return View(mdfViewModel);
        }

        // POST: MDFView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Ap([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_approvedAmt_mkt,mdf_validatedAmt,mdf_validatedAmt_mkt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type,mdf_requestType")] MDFViewModel mDFViewModel, IEnumerable<HttpPostedFileBase> fileupload)
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
                    mDFViewModel.mdf_requestDate = DateTime.Now;
                }

                if (mDFViewModel.mdf_type == 5 && Request.Form["prod_type"] == "1")
                {
                    mDFViewModel.mdf_desc = "WM Wallmount Display Enclosure (9967.851)";
                    mDFViewModel.mdf_title = "Marketing Enclosures";
                    mDFViewModel.mdf_totalCost = 1000.00;
                    mDFViewModel.mdf_mdfCost = 1000.00;
                }
                else if (mDFViewModel.mdf_type == 5 && Request.Form["prod_type"] == "2")
                {
                    mDFViewModel.mdf_desc = "Mini TS8 Display Enclosure (9967.898)";
                    mDFViewModel.mdf_title = "Marketing Enclosures";
                    mDFViewModel.mdf_totalCost = 1000.00;
                    mDFViewModel.mdf_mdfCost = 1000.00;
                }
                else if (mDFViewModel.mdf_type == 5 && Request.Form["prod_type"] == "3")
                {
                    mDFViewModel.mdf_desc = "Hygienic Design Enclosure (9973.113)";
                    mDFViewModel.mdf_title = "Marketing Enclosures";
                    mDFViewModel.mdf_totalCost = 1000.00;
                    mDFViewModel.mdf_mdfCost = 1000.00;
                }
                else if (mDFViewModel.mdf_type == 5 && Request.Form["prod_type"] == "4")
                {
                    mDFViewModel.mdf_desc = "RiLine Compact Sample Kit (2996.001)";
                    mDFViewModel.mdf_title = "Marketing Enclosures";
                    mDFViewModel.mdf_totalCost = 1000.00;
                    mDFViewModel.mdf_mdfCost = 1000.00;
                }
                string rcmEmail = await this.GetApprovers(mDFViewModel.mdf_comp.Value);
                mDFViewModel.rcm = rcmEmail;
                db.MDFViewModels.Add(mDFViewModel);
                await db.SaveChangesAsync();
                MDFfiles mdf_files = new MDFfiles();
                await SaveImages(fileupload, mDFViewModel.mdf_ID, mDFViewModel.mdf_file_type, mdf_files);

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
                var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                string body_addon = "An MDF request has been submitted and the status is Pending. The next step will be the review by the Regional Channel Manager. <br />";
                string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                
                string email = usr_data.usr_email + "," + (!string.IsNullOrEmpty(rcmEmail) ? rcmEmail : "barth.j@rittal.us") + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                //send email out when the MDF is approved
                sendMDFEmail(body_addon + email_body, header, footer, email, title);
                
                return RedirectToAction("Index", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(mDFViewModel);
        }

        public async Task<string> GetApprovers(long comp_ID)
        {
            string userZip = Session["zip"] != null ? Session["zip"].ToString() : "";
            List<string> companies = ConfigurationManager.AppSettings["NationalManagerCompanies"].Split(',').ToList();
            List<string> companyLocations = ConfigurationManager.AppSettings["NationalManagerCompanyLocations"].Split(',').ToList();
            partnerLocation userLocation = null;
            if (!string.IsNullOrEmpty(userZip))
            {
                userLocation = await dbEntity.partnerLocations.Where(x => x.comp_ID == comp_ID && x.loc_zip == userZip).FirstOrDefaultAsync();
            }
            List<string> approvers = new List<string>();
            if (companies.Contains(comp_ID.ToString()) || (userLocation != null && companyLocations.Contains(comp_ID + ":" + userLocation.loc_ID)))
            {
                approvers.Add(ConfigurationManager.AppSettings["NationalChannelManagerEmail"]);
            }
            else if (!string.IsNullOrEmpty(userZip))
            {
                approvers = dbEntity.RCMContacts.Where(a => a.zipcode == userZip && !string.IsNullOrEmpty(a.email)).Select(x => x.email).ToList();
            }
            return approvers.Count > 0 ? string.Join(",", approvers) : ConfigurationManager.AppSettings["NationalChannelManagerEmail"];
        }  

        // POST: MDFView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApEdit([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_approvedAmt_mkt,mdf_validatedAmt,mdf_validatedAmt_mkt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type,cost_center,old_status,rcm")] MDFViewModel mDFViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            bool bStatusValidate = true;
            if (mDFViewModel.mdf_status.HasValue && mDFViewModel.old_status != mDFViewModel.mdf_status)
            {
                if ((mDFViewModel.mdf_status == 2 || mDFViewModel.mdf_status == 8) && (mDFViewModel.old_status > 1 || string.IsNullOrEmpty(mDFViewModel.rcm) || mDFViewModel.rcm.ToLower() != Session["userEmail"].ToString().ToLower()))
                {
                    bStatusValidate = false;
                }
                else if((mDFViewModel.mdf_status == 7 || mDFViewModel.mdf_status == 3) && mDFViewModel.old_status != 8)
                {
                    bStatusValidate = false;
                }
                else if(mDFViewModel.mdf_status == 4 && mDFViewModel.old_status != 3)
                {
                    bStatusValidate = false;
                }
                else if(mDFViewModel.mdf_status == 5 && mDFViewModel.old_status != 4)
                {
                    bStatusValidate = false;
                }
            }
            if (mDFViewModel.mdf_status == 8 && mDFViewModel.mdf_approvedAmt <= 0 && mDFViewModel.mdf_approvedAmt_mkt <= 0)
            {
                bStatusValidate = false;
            }
            else if (mDFViewModel.mdf_status == 4 && mDFViewModel.mdf_validatedAmt <= 0 && mDFViewModel.mdf_validatedAmt_mkt <= 0)
            {
                bStatusValidate = false;
            }
            if (ModelState.IsValid && bStatusValidate)
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
                    MDFfiles mdf_files = new MDFfiles();
                    await SaveImages(fileupload, Convert.ToInt32(mDFViewModel.mdf_ID), Convert.ToInt32(mDFViewModel.mdf_file_type), mdf_files);
                }
                string rcmEmail = !string.IsNullOrEmpty(mDFViewModel.rcm) ? mDFViewModel.rcm : await this.GetApprovers(mDFViewModel.mdf_comp.Value);
                mDFViewModel.rcm = rcmEmail;
                if(mDFViewModel.cost_center != null && mDFViewModel.cost_center.Contains("MKT"))
                {
                    mDFViewModel.mdf_approvedAmt = mDFViewModel.mdf_approvedAmt_mkt;
                    mDFViewModel.mdf_validatedAmt = mDFViewModel.mdf_validatedAmt_mkt;
                }
                db.Entry(mDFViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!string.IsNullOrEmpty(Request.Form["userEdit"]))
                {
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string body_addon = "An MDF request has been edited and the status is Pending. The next step will be the review by the Regional Channel Manager. <br />";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                        //send email out when the MDF is approved
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manage", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userApproved"]) && mDFViewModel.mdf_status == 3)
                {
                    mDFViewModel.mdf_reviewDate = DateTime.Now;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"]=="1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - "+ Request.Form["subtype"] + " - Update 1";
                        string body_addon = "The MDF has been marked Approved by the Marketing department. The next step will be the review by the Regional Channel Manager. Once the activity has taken place and all supporting documents have been attached, the MDF will be marked as Completed. <br />";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                        //send email out when the MDF is approved
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userCompleted"]) && mDFViewModel.mdf_status == 4)
                {
                    mDFViewModel.mdf_validationDate = DateTime.Now;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string body_addon = "The MDF status has been marked as Completed by the Regional Channel Manager. The next step will be the issuing of a Credit Memo by our Accounting department. <br />";
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,accountsreceivable@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail( body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (!string.IsNullOrEmpty(Request.Form["userCreditIssued"]) && mDFViewModel.mdf_status == 5)
                {
                    mDFViewModel.mdf_creditIssueDate = DateTime.Now;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string body_addon = "The MDF status has been marked as Credit Issued by the Accounting department. The Credit Memo # is referenced below. This MDF is now considered closed. <br />";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,presswala.z@rittal.us,accountsreceivable@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (mDFViewModel.mdf_status == 6)
                {
                    mDFViewModel.mdf_status = 6;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string body_addon = "The MDF status has been marked as Canceled. If you have questions or concerns, please contact your company's Regional Channel Manager. <br />";
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (mDFViewModel.mdf_status == 2)
                {
                    mDFViewModel.mdf_status = 2;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string body_addon = "The MDF status has been marked as RCM Denied. If you have questions or concerns please contact your company's Regional Channel Manager. <br />";
                        string email = usr_data.usr_email + (!string.IsNullOrEmpty(rcmEmail) ? "," + rcmEmail : "") + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (mDFViewModel.mdf_status == 7)
                {
                    mDFViewModel.mdf_status = 7;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string body_addon = "The MDF has been denied by the marketing team. RCM can resubmit the request after making the required changes. <br />";
                        string email = (!string.IsNullOrEmpty(rcmEmail) ? rcmEmail + "," : "") + "mdfAdmin@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }
                else if (mDFViewModel.mdf_status == 8)
                {
                    mDFViewModel.mdf_status = 8;
                    db.Entry(mDFViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Get user email
                    if (Request.Form["emails"] == "1")
                    {
                        var usr_data = db.UserViewModels.Where(a => a.usr_ID == mDFViewModel.mdf_user).FirstOrDefault();
                        string title = "Brand Central Request No. " + mDFViewModel.mdf_ID + " - " + Request.Form["company"] + " - " + Request.Form["subtype"] + " - Update 1";
                        string email_body = await emailbody(mDFViewModel.mdf_ID, mDFViewModel, usr_data);
                        string body_addon = "The MDF status has been marked as Approved by the Regional Channel Manager. The next step will be the review by the Marketing department. <br />";
                        string email = usr_data.usr_email + "," + ConfigurationManager.AppSettings["MarketingTeamEmails"] + ",mdfAdmin@rittal.us,presswala.z@rittal.us";
                        sendMDFEmail(body_addon + email_body, header, footer, email, title);
                    }

                    return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
                }

                return RedirectToAction("manageAdmin", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your MDF has been added" });
            }
            else if (!ModelState.IsValid)
            {
                var locController = new CommonController();

                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }
            else if(!bStatusValidate)
            {
                return RedirectToAction("DetailsAdmin", new { id = mDFViewModel.mdf_ID, n1_name = Request.Form["n1_name"], n2_name = "Admin", error = "This status change isn't allowed based on the current MDF status." });
            }

            return View(mDFViewModel);
        }
        #endregion

        #region Go to Brand Central Store
        // GET: MDFView
        public ActionResult Merch()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            long? companyId = Convert.ToInt32(Session["companyId"]);
            var usr_data = db.UserViewModels.Where(a=>a.usr_ID==userId).FirstOrDefault();
            string usr_comp = db.partnerCompanyViewModels.Where(a => a.comp_ID == companyId).FirstOrDefault().comp_name;
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == companyId);

            MDFViewModel mdfViewModel = new MDFViewModel();
            mdfViewModel.partner_loc = usr_loc;
            mdfViewModel.mdf_comp = companyId;
            mdfViewModel.company = usr_comp;
            mdfViewModel.usr_email = usr_data.usr_email;
            mdfViewModel.usr_phone = usr_data.usr_phone;
            mdfViewModel.usr_fName= usr_data.usr_fName;
            mdfViewModel.usr_lName = usr_data.usr_lName;
            mdfViewModel.usr_city = usr_data.usr_city;
            mdfViewModel.usr_add1 = usr_data.usr_add1;
            mdfViewModel.usr_zip = usr_data.usr_zip;
            //Get the users country data
            if (usr_data.usr_state != null)
            {
                var stateData = dbEntity.data_state.Where(a => a.stateid == usr_data.usr_state);
                if (stateData.Count() > 0)
                {
                    mdfViewModel.usr_state = stateData.FirstOrDefault().state_long;
                }
            }

            return View(mdfViewModel);
        }

        // POST: MDFView/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Merch([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_approvedAmt_mkt,mdf_validatedAmt,mdf_validatedAmt_mkt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year")] MDFViewModel mDFViewModel)
        {

            if (!string.IsNullOrEmpty(Request.Form["agree"]))
            {
                return RedirectToAction("Merch", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], agree = "yes" });
            }
            else
            {
                return RedirectToAction("Merch", new { mdf_ID = mDFViewModel.mdf_ID, mA = Request.Form["mA"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"],success="Check the Aggreement box"});
            }
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
            MDFViewModel mdfViewModels = new MDFViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();// Get page text
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(companyId);//Get company data
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            string type_mdf = "";
            //if (partnerCompanyViewModel.comp_MDF_amount > 0)
            //{
                //Get The MDF information
            List<mdf_main> mdf_main_Model = await dbEntity.mdf_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == archive_year && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();
            List<MdfParts> prom = comm.MDFsActivities(mdf_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
            if (mdf_main_Model.Count() > 0)
            {
                mdfViewModels.mdf_comp = mdf_main_Model.FirstOrDefault().mdf_comp;
            }
            mdfViewModels.topdesc = topdesc.n2_descLong;
            mdfViewModels.mdf_parts = prom;
            mdfViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
            mdfViewModels.company = partnerCompanyViewModel.comp_name;

            List<MDFViewModel> list_mdfViewModels = new List<MDFViewModel>();

            foreach (var mdf in mdf_main_Model)
            {
                var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                var type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                if (type.Count() > 0)
                {
                    type_mdf = type.FirstOrDefault().mdf_subType_name;
                }

                list_mdfViewModels.Add(new MDFViewModel {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status
                });
            }

            mdfViewModels.mdf_main = list_mdfViewModels;

            return View(mdfViewModels);
            //}

            //return View(mdfViewModels);
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
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            if (mDFViewModel == null)
            {
                return HttpNotFound();
            }
            string type_mdf = "";
            //Get files per MDF
            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);
            mDFViewModel.list_mdf_files = files;
            //Get the MDF types
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            var type = sub_type.Where(a => a.mdf_type_ID == mDFViewModel.mdf_type);
            if (type.Count() > 0)
            {
                type_mdf = type.FirstOrDefault().mdf_subType_name;
            }

            mDFViewModel.request_type = type_mdf;
            mDFViewModel.requester = fullname["fullName"];

            return View(mDFViewModel);
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
            MDFViewModel mdfViewModels = new MDFViewModel();
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();// Get page text
            partnerCompany_Archive partnerCompanyViewModel = await dbEntity.partnerCompany_Archive.Where(a=>a.comp_ID==companyId && archive_year==year).FirstOrDefaultAsync();//Get company data
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            string type_mdf = "";
            if (partnerCompanyViewModel.comp_MDF_amount > 0)
            {
                //Get The MDF information
                IList<mdf_main> mdf_main_Model = await dbEntity.mdf_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == archive_year && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();
                List<MdfParts> prom = comm.MDFsActivitiesArchive(mdf_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
                if (mdf_main_Model.Count() > 0)
                {
                    mdfViewModels.mdf_comp = mdf_main_Model.FirstOrDefault().mdf_comp;
                }
                mdfViewModels.topdesc = topdesc.n2_descLong;
                mdfViewModels.mdf_parts = prom;
                mdfViewModels.comp_MDF_amount = partnerCompanyViewModel.comp_MDF_amount;
                mdfViewModels.company = partnerCompanyViewModel.comp_name;

                List<MDFViewModel> list_mdfViewModels = new List<MDFViewModel>();

                foreach (var mdf in mdf_main_Model)
                {
                    var fullname = await comm.GetfullName(Convert.ToInt32(mdf.mdf_user));
                    var type = sub_type.Where(a => a.mdf_type_ID == mdf.mdf_type);
                    if (type.Count() > 0)
                    {
                        type_mdf = type.FirstOrDefault().mdf_subType_name;
                    }

                    list_mdfViewModels.Add(new MDFViewModel
                    {
                        mdf_ID = mdf.mdf_ID,
                        request_type = type_mdf,
                        requester = fullname["fullName"],
                        mdf_totalCost = mdf.mdf_totalCost,
                        mdf_mdfCost = mdf.mdf_mdfCost,
                        mdf_approvedAmt = mdf.mdf_approvedAmt,
                        mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt,
                        mdf_validatedAmt = mdf.mdf_validatedAmt,
                        mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt,
                        mdf_requestDate = mdf.mdf_requestDate,
                        mdf_title = mdf.mdf_title,
                        mdf_status = mdf.mdf_status
                    });
                }

                mdfViewModels.mdf_main = list_mdfViewModels;

                return View(mdfViewModels);
            }

            return View(mdfViewModels);
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
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            if (mDFViewModel == null)
            {
                return HttpNotFound();
            }
            string type_mdf = "";
            //Get files per MDF
            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);
            mDFViewModel.list_mdf_files = files;
            //Get the MDF types
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            var type = sub_type.Where(a => a.mdf_type_ID == mDFViewModel.mdf_type);
            if (type.Count() > 0)
            {
                type_mdf = type.FirstOrDefault().mdf_subType_name;
            }

            mDFViewModel.request_type = type_mdf;
            mDFViewModel.requester = fullname["fullName"];

            return View(mDFViewModel);
        }
        #endregion

        #region MDF Admin
        [HttpGet]
        public async Task<ActionResult> ManualEntry()
        {
            MDFViewModel mDFViewModel = new MDFViewModel();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            return View(mDFViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManualEntry([Bind(Include = "mdf_ID,mdf_user,mdf_SAP,mdf_comp,mdf_title,mdf_desc,mdf_loc,mdf_totalCost,mdf_mdfCost,mdf_date,mdf_type,mdf_status,mdf_comments,mdf_comments2,mdf_comments3,mdf_requestDate,mdf_reviewDate,mdf_validationDate,mdf_creditIssueDate,mdf_approvedAmt,mdf_approvedAmt_mkt,mdf_validatedAmt,mdf_validatedAmt_mkt,mdf_creditMemoNum,mdf_accountingInstruct,archive_year,fileupload,mdf_file_type,usr_email")] MDFViewModel mDFViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(mDFViewModel.usr_email))
                {
                    return RedirectToAction("ManualEntry", new {  n1_name = Request.Form["n1_name"], n2_name = "Admin", msg = Request.Form["msg"], success = "The email can not be empty" });
                }
                //find user data
                var usr_data = await db.UserViewModels.Where(a=>a.usr_email==mDFViewModel.usr_email).FirstOrDefaultAsync();
                if (usr_data == null)
                {
                    return RedirectToAction("ManualEntry", new { n1_name = Request.Form["n1_name"], n2_name = "Admin", msg = Request.Form["msg"], success = "Email does not exists in system" });
                }
                mDFViewModel.mdf_user = usr_data.usr_ID;
                mDFViewModel.mdf_comp = usr_data.comp_ID;
                mDFViewModel.mdf_status = 1;

                db.MDFViewModels.Add(mDFViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("DetailsAdmin", new { id = mDFViewModel.mdf_ID, n1_name = Request.Form["n1_name"], n2_name = "Admin", msg = "Details for "+ mDFViewModel.mdf_ID, success = "Your MDF has been added" });
            }

            return View(mDFViewModel);
        }

        // GET: MDFView
        public async Task<ActionResult> ManageAdmin(int year = 0,int comp_id=0, int status=0, int type=0, string request_type = "",string rcm = "", string cost_center = "")
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
            MDFViewModel mdfViewModels = new MDFViewModel();
            // Get page text
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();
            mdfViewModels.topdesc = topdesc.n2_descLong;
            //Get company data
            var partnerCompany = await db.partnerCompanyViewModels.ToListAsync();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;
            ViewBag.newTypes = sub_type.Where(x => (!string.IsNullOrEmpty(x.mdf_type_desc) && x.mdf_type_desc.ToLower() == "new")).ToList();
            ViewBag.oldTypes = sub_type.Where(x => (string.IsNullOrEmpty(x.mdf_type_desc) || x.mdf_type_desc.ToLower() != "new")).ToList();
            string type_mdf = "";
            string company = "";
            IQueryable<mdf_main> mdf_main_Model = null;
            IQueryable<mdf_main> mdf_main = dbEntity.mdf_main;
            List<MdfParts> company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
            List<MdfParts> company_metrics_mkt = new List<MdfParts>();//Calculate the percentages and mdf utilized
            //Get The MDF information
            if (comp_id != 0)
            {
                partnerCompanyViewModel company_data;
                //Get MDF by company
                if (status!=0 && type==0)
                {
                    //Filter only by status
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_status==status);
                    company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type==type);
                    company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type && a.mdf_status == status);
                    company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                }
                else
                {
                    //filter by only company
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id);
                    company_data = partnerCompany.Find(a => a.comp_ID == comp_id);
                }
                company_metrics = comm.MDFsActivities(await mdf_main_Model.Where(a => (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync(), company_data);//Calculate the percentages and mdf utilized
                company_metrics_mkt = comm.MKTsActivities(await mdf_main_Model.Where(a => (!string.IsNullOrEmpty(a.cost_center) && (a.cost_center.Contains("MKT") || a.cost_center == "Split"))).ToListAsync(), company_data);//Calculate the percentages and mkt utilized
            }
            else
            {
                //Get all the MDFs
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_status == status);
                    
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_type == type );
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_type == type && a.mdf_status == status);
                }
                else
                {
                    //filter by only company
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year);
                }
            }

            //List the companies that are in the MDFs
            List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
            foreach (var mdf in mdf_main.Where(a=>a.archive_year== archive_year))
            {
                if (partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                {
                    list_comp.Add(new partnerCompanyViewModel { comp_ID = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID, comp_name = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                }
            }

            //Add to the list of MDFs for the table
            List<MDFViewModel> list_mdfViewModels = new List<MDFViewModel>();
            if(!string.IsNullOrEmpty(rcm))
            {
                mdf_main_Model = mdf_main_Model.Where(x => x.rcm == rcm);
            }
            List<mdf_main> mdfs = await mdf_main_Model.ToListAsync();
            foreach (var mdf in mdfs)
            {
                if (request_type != "" && mdf.mdf_requestType != request_type)
                    continue;
                if (cost_center != "")
                {
                    if (cost_center == "MDF CC 7200" && !string.IsNullOrEmpty(mdf.cost_center) && mdf.cost_center != cost_center)
                        continue;
                    else if (mdf.cost_center != cost_center)
                        continue;
                }
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

                list_mdfViewModels.Add(new MDFViewModel
                {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_date = mdf.mdf_date,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status,
                    company = company,
                    mdf_requestType = mdf.mdf_requestType,
                    rcm = mdf.rcm,
                    cost_center = mdf.cost_center
                });
            }

            mdfViewModels.mdf_main = list_mdfViewModels;
            mdfViewModels.list_comp = list_comp;
            mdfViewModels.company_metrics = company_metrics;
            mdfViewModels.company_metrics_mkt = company_metrics_mkt;
            mdfViewModels.rcmNames = this.getRCM();
            ViewBag.IsArchive = false;
            return View(mdfViewModels);
        }

        public Dictionary<string,string> getRCM()
        {
            Dictionary<string, string> dict = dbEntity.RCMContacts.Where(a => !string.IsNullOrEmpty(a.email)).OrderBy(x => x.first_name).GroupBy(x => x.email).ToDictionary(x => x.Key, y => (y.FirstOrDefault().first_name + " " + y.FirstOrDefault().last_name).Trim());
            dict.Add(ConfigurationManager.AppSettings["NationalChannelManagerEmail"], ConfigurationManager.AppSettings["NationalChannelManagerName"]);
            return dict;
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> DetailsAdmin(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            if (mDFViewModel == null)
            {
                return HttpNotFound();
            }
            if (Request.QueryString["error"] != null)
            {
                ViewBag.mdfSaveError = Request.QueryString["error"];
            }
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(mDFViewModel.mdf_comp);//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == mDFViewModel.mdf_comp);

            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);//Get images

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            mDFViewModel.company = partnerCompanyViewModel != null ? partnerCompanyViewModel.comp_name : "";
            mDFViewModel.requester = fullname["fullName"];
            mDFViewModel.partner_loc = usr_loc;
            mDFViewModel.list_mdf_files = files;
            double? mdfRemaining = 0;
            double? mdfRemainingWithPending = 0;
            double? mktRemaining = 0;
            double? mktRemainingWithPending = 0;
            bool bMDF = (string.IsNullOrEmpty(mDFViewModel.cost_center) || mDFViewModel.cost_center == "MDF CC 7200");
            if (partnerCompanyViewModel.comp_MDF_amount > 0)
            {
                //Get The MDF information
                var mdf_main = dbEntity.mdf_main.Where(a => a.mdf_comp == partnerCompanyViewModel.comp_ID && a.archive_year == null);
                List<mdf_main> listmdfs = await mdf_main.Where(a => (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split") && a.mdf_ID <= id).ToListAsync();
                List<MdfParts> prom = comm.MDFsActivities(listmdfs, partnerCompanyViewModel);//Calculate the percentages and mdf utilized
                if (prom.Count > 0)
                {
                    mdfRemaining = prom.FirstOrDefault().totalMDFAva;
                    if (mDFViewModel.mdf_status > 1)
                        mdfRemaining += (mDFViewModel.mdf_validatedAmt.HasValue && mDFViewModel.mdf_validatedAmt > 0 ? mDFViewModel.mdf_validatedAmt : (mDFViewModel.mdf_approvedAmt.HasValue && mDFViewModel.mdf_approvedAmt > 0 ? mDFViewModel.mdf_approvedAmt : mDFViewModel.mdf_mdfCost));
                    mdfRemainingWithPending = prom.FirstOrDefault().totalMDFAvaWithPending;
                }
                List<mdf_main> listmkts = await mdf_main.Where(a => a.cost_center.Contains("MKT") || a.cost_center == "Split").ToListAsync();
                List<MdfParts> promMkt = comm.MKTsActivities(listmkts, partnerCompanyViewModel);
                if(promMkt.Count > 0)
                {
                    mktRemaining = promMkt.FirstOrDefault().totalMDFAva;
                    if (mDFViewModel.mdf_status > 1)
                        mktRemaining += (mDFViewModel.mdf_validatedAmt_mkt.HasValue && mDFViewModel.mdf_validatedAmt_mkt > 0 ? mDFViewModel.mdf_validatedAmt_mkt : (mDFViewModel.mdf_approvedAmt_mkt.HasValue && mDFViewModel.mdf_approvedAmt_mkt > 0 ? mDFViewModel.mdf_approvedAmt_mkt : mDFViewModel.mdf_mdfCost));
                    mktRemainingWithPending = promMkt.FirstOrDefault().totalMDFAvaWithPending;
                }
            }
            mDFViewModel.mdf_remainingAmt = mdfRemaining.HasValue ? mdfRemaining.Value : 0;
            mDFViewModel.mdf_remainingAmtWithPending = mdfRemainingWithPending.HasValue ? mdfRemainingWithPending.Value : 0;
            mDFViewModel.mkt_remainingAmt = mktRemaining.HasValue ? mktRemaining.Value : 0;
            mDFViewModel.mkt_remainingAmtWithPending = mktRemainingWithPending.HasValue ? mktRemainingWithPending.Value : 0;
            mDFViewModel.rcmNames = this.getRCM();
            return View(mDFViewModel);
        }
        #endregion

        #region MDF Admin Archive
        // GET: MDFView
        public async Task<ActionResult> ManageAdminArchive(string year, int comp_id = 0, int status = 0, int type = 0, string request_type = "", string rcm = "", string cost_center = "")
        {
            long? companyId = Convert.ToInt32(Session["companyId"]);
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int? archive_year = Convert.ToInt32(year);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            MDFViewModel mdfViewModels = new MDFViewModel();
            // Get page text
            var topdesc = await db.Nav2ViewModel.Where(a => a.n2ID == 6).FirstOrDefaultAsync();
            mdfViewModels.topdesc = topdesc.n2_descLong;
            //Get company data
            var partnerCompany = await dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year).ToListAsync();
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;
            ViewBag.newTypes = sub_type.Where(x => (!string.IsNullOrEmpty(x.mdf_type_desc) && x.mdf_type_desc.ToLower() == "new")).ToList();
            ViewBag.oldTypes = sub_type.Where(x => (string.IsNullOrEmpty(x.mdf_type_desc) || x.mdf_type_desc.ToLower() != "new")).ToList();
            string type_mdf = "";
            string company = "";
            IQueryable<mdf_main> mdf_main_Model = null;
            IQueryable<mdf_main> mdf_main = dbEntity.mdf_main.Where(a=>a.archive_year==archive_year);
            List<MdfParts> company_metrics = new List<MdfParts>();//Calculate the percentages and mdf utilized
            List<MdfParts> company_metrics_mkt = new List<MdfParts>();//Calculate the percentages and mdf utilized
            //Get The MDF information
            if (comp_id != 0)
            {
                partnerCompany_Archive company_data;
                //Get MDF by company
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_status == status);
                    company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                    
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type);
                    company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id && a.mdf_type == type && a.mdf_status == status);
                    company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                }
                else
                {
                    //filter by only company
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == comp_id);
                    company_data = partnerCompany.Where(a => a.comp_ID == comp_id && a.archive_year == year).FirstOrDefault();
                }
                company_metrics = comm.MDFsActivitiesArchive(await mdf_main_Model.Where(a => (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync(), company_data);//Calculate the percentages and mdf utilized
                company_metrics_mkt = comm.MKTsActivitiesArchive(await mdf_main_Model.Where(a => (!string.IsNullOrEmpty(a.cost_center) && (a.cost_center.Contains("MKT") || a.cost_center == "Split"))).ToListAsync(), company_data);//Calculate the percentages and mkt utilized
            }
            else
            {
                //Get all the MDFs
                if (status != 0 && type == 0)
                {
                    //Filter only by status
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_status == status);
                }
                else if (status == 0 && type != 0)
                {
                    //Filter only by type
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_type == type);
                }
                else if (status != 0 && type != 0)
                {
                    //Filter only by both
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_type == type && a.mdf_status == status);
                }
                else
                {
                    //filter by only company
                    mdf_main_Model = mdf_main.Where(a => a.archive_year == archive_year);
                }
            }

            //List the companies that are in the MDFs
            List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
            foreach (var mdf in mdf_main.Where(a => a.archive_year == archive_year))
            {
                if (partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).Count() > 0)
                {
                    long comp = Convert.ToInt32(partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_ID);
                    list_comp.Add(new partnerCompanyViewModel { comp_ID = comp, comp_name = partnerCompany.Where(a => a.comp_ID == mdf.mdf_comp).FirstOrDefault().comp_name });
                }
            }

            //Add to the list of MDFs for the table
            List<MDFViewModel> list_mdfViewModels = new List<MDFViewModel>();
            if (!string.IsNullOrEmpty(rcm))
            {
                mdf_main_Model = mdf_main_Model.Where(x => x.rcm == rcm);
            }
            List<mdf_main> mdfs = await mdf_main_Model.ToListAsync();
            foreach (var mdf in mdfs)
            {
                if (request_type != "" && mdf.mdf_requestType != request_type)
                    continue;
                if (cost_center != "")
                {
                    if (cost_center == "MDF CC 7200" && !string.IsNullOrEmpty(mdf.cost_center) && mdf.cost_center != cost_center)
                        continue;
                    else if (mdf.cost_center != cost_center)
                        continue;
                }
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

                list_mdfViewModels.Add(new MDFViewModel
                {
                    mdf_ID = mdf.mdf_ID,
                    request_type = type_mdf,
                    requester = fullname["fullName"],
                    mdf_totalCost = mdf.mdf_totalCost,
                    mdf_mdfCost = mdf.mdf_mdfCost,
                    mdf_approvedAmt = mdf.mdf_approvedAmt,
                    mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt,
                    mdf_validatedAmt = mdf.mdf_validatedAmt,
                    mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt,
                    mdf_requestDate = mdf.mdf_requestDate,
                    mdf_date = mdf.mdf_date,
                    mdf_title = mdf.mdf_title,
                    mdf_status = mdf.mdf_status,
                    company = company,
                    mdf_requestType = mdf.mdf_requestType,
                    rcm = mdf.rcm
                });
            }

            mdfViewModels.mdf_main = list_mdfViewModels;
            mdfViewModels.list_comp = list_comp;
            mdfViewModels.company_metrics = company_metrics;
            mdfViewModels.company_metrics_mkt = company_metrics_mkt;
            mdfViewModels.rcmNames = this.getRCM();
            ViewBag.IsArchive = true;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_MDFTableResults", mdfViewModels);
            }
            return View(mdfViewModels);
        }

        // GET: MDFView/Details/5
        public async Task<ActionResult> DetailsAdminArchive(long? id,string year)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            if (mDFViewModel == null)
            {
                return HttpNotFound();
            }
            partnerCompany_Archive partnerCompanyViewModel = dbEntity.partnerCompany_Archive.Where(a=>a.archive_year==year && a.comp_ID==mDFViewModel.mdf_comp).FirstOrDefault();//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == mDFViewModel.mdf_comp);

            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);//Get images

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            mDFViewModel.company = partnerCompanyViewModel.comp_name;
            mDFViewModel.requester = fullname["fullName"];
            mDFViewModel.partner_loc = usr_loc;
            mDFViewModel.list_mdf_files = files;

            return View(mDFViewModel);
        }
        #endregion

        #region MDF files
        async Task SaveImages(IEnumerable<HttpPostedFileBase> fileupload, long mdf_id, int mdf_file_type, MDFfiles mdf_files)
        {
            //Process the attached images
            foreach (HttpPostedFileBase file in fileupload)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file.FileName);
                    var NewFileName = mdf_id +"_"+ fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/mdf/images"), NewFileName);

                    file.SaveAs(path);
                    //if file_id is not set or the entry is empty
                    mdf_files = new MDFfiles
                    {
                        mdf_ID = mdf_id,
                        mdf_file_type = Convert.ToByte(mdf_file_type),
                        mdf_file_name = NewFileName
                    };
                    db.mdfFiles.Add(mdf_files);
                    await db.SaveChangesAsync();
                }
            }
        }

        async Task<IEnumerable<MDFfiles>> getImages(long mdf_ID)
        {
            IEnumerable<MDFfiles> files = await db.mdfFiles.Where(a => a.mdf_ID == mdf_ID).ToListAsync();

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
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(mdf_id);
            List<mdf_main> mdf_main_Model = await dbEntity.mdf_main.Where(a => a.mdf_comp == mdf_id && a.archive_year == null && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();

            if (mdf_main_Model == null)
            {
                return HttpNotFound();
            }
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(mDFViewModel.mdf_comp);//Get company data
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == mDFViewModel.mdf_comp);
            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);//Get images
            List<MdfParts> company_metrics = comm.MDFsActivities(mdf_main_Model, partnerCompanyViewModel);//Calculate the percentages and mdf utilized

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            mDFViewModel.company = partnerCompanyViewModel.comp_name;
            mDFViewModel.requester = fullname["fullName"];
            mDFViewModel.partner_loc = usr_loc;
            mDFViewModel.list_mdf_files = files;
            mDFViewModel.company_metrics = company_metrics;

            return View(mDFViewModel);
        }

        private async Task<string> emailbody(long? mdf_id, MDFViewModel mDFViewModel,UserViewModel usr_data)
        {
            //MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(mdf_ID);
            List<mdf_main> mdf_main_Model = await dbEntity.mdf_main.Where(a => a.mdf_ID == mdf_id && a.archive_year == null && (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToListAsync();
            //Get company data
            partnerCompanyViewModel partnerCompanyViewModel = await db.partnerCompanyViewModels.FindAsync(mDFViewModel.mdf_comp);
            IQueryable<partnerLocationViewModel> usr_loc = db.partnerLocationViewModels.Where(a => a.comp_ID == mDFViewModel.mdf_comp);
            //Get images
            IEnumerable<MDFfiles> files = await getImages(mDFViewModel.mdf_ID);
            //Calculate the percentages and mdf utilized
            List<MdfParts> company_metrics = comm.MDFsActivities(mdf_main_Model, partnerCompanyViewModel);

            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();
            ViewBag.type = sub_type;

            var fullname = await comm.GetfullName(Convert.ToInt32(mDFViewModel.mdf_user));
            mDFViewModel.company = partnerCompanyViewModel.comp_name;
            mDFViewModel.requester = fullname["fullName"];
            mDFViewModel.partner_loc = usr_loc;
            mDFViewModel.list_mdf_files = files;
            mDFViewModel.company_metrics = company_metrics;

            String messageView = comm.RenderViewToString(ControllerContext, "~/Views/MDFView/emailbody.cshtml", mDFViewModel, true);
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
            string costCenter = Request.Form["cost_center"];
            bool bMDF = (string.IsNullOrEmpty(costCenter) || costCenter == "MDF CC 7200");
            List<MdfParts> company_metrics = new List<MdfParts>();
            List<MdfParts> company_metrics_mkt = new List<MdfParts>();
            List<mdf_main> mdf_main;
            string mdfAppend = (costCenter == "Split" ? " (MDF)" : "");
            string mktAppend = (costCenter == "Split" ? " (MKT)" : "");

            if (string.IsNullOrEmpty(year))
            {
                IQueryable<partnerCompany> partnerCompLoop = dbEntity.partnerCompanies;//Get company data
                IQueryable<partnerCompanyViewModel> partnerComp = db.partnerCompanyViewModels;//Get company data
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate).ToList();
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_date >= ssDate && a.mdf_date <= eeDate).ToList();
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate).ToList();
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate).ToList();
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate).ToList();
                }
                else
                {
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null).ToList();
                }
                if(bMDF)
                {
                    mdf_main = mdf_main.Where(x => string.IsNullOrEmpty(x.cost_center) || x.cost_center == costCenter || x.cost_center == "Split").ToList();
                }
                else
                {
                    mdf_main = mdf_main.Where(x => x.cost_center == costCenter || x.cost_center == "Split").ToList();
                }
                
                IEnumerable<partnerType> partner_type = await dbEntity.partnerTypes.ToListAsync();

                //List the companies that are in the MDFs
                //List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
                //foreach (var mdf in mdf_main)
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
                if (bMDF || costCenter == "Split")
                {
                    mdf_to_excel.Columns.Add("Total MDF", typeof(string));
                    mdf_to_excel.Columns.Add("Promotional Activities Limit", typeof(string));
                    mdf_to_excel.Columns.Add("RAS Investment Limit", typeof(string));
                    mdf_to_excel.Columns.Add("Tradeshows & Events Limit", typeof(string));
                    mdf_to_excel.Columns.Add("Rittal Training for Distributor Employees Limit", typeof(string));
                    mdf_to_excel.Columns.Add("Sales Performance Incentive Limit", typeof(string));
                    mdf_to_excel.Columns.Add("Display Products Limit", typeof(string));
                    mdf_to_excel.Columns.Add("Amount Pending" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Approved" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Completed" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Utilized" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Available" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Credited" + mdfAppend, typeof(string));
                }
                if(costCenter == "MKT CC 7615" || costCenter == "Split")
                {
                    mdf_to_excel.Columns.Add("Total MKT", typeof(string));
                    mdf_to_excel.Columns.Add("Amount Pending" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Approved" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Completed" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Utilized" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Available" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Credited" + mktAppend, typeof(string));
                }
               
                foreach (var item in partnerCompLoop.OrderBy(a => a.comp_name))
                {
                    partnerCompanyViewModel company_data = await partnerComp.Where(a => a.comp_ID == item.comp_ID).FirstOrDefaultAsync();
                    var Company_mdf_main = mdf_main.Where(a => a.archive_year == null && a.mdf_comp == item.comp_ID);
                    if(bMDF || costCenter == "Split")
                    {
                        company_metrics = comm.MDFsActivities(Company_mdf_main.Where(a => (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToList(), company_data);//Calculate the percentages and mdf utilized
                    }
                    if(costCenter == "MKT CC 7615" || costCenter == "Split")
                    {
                        company_metrics_mkt = comm.MKTsActivities(Company_mdf_main.Where(a => a.cost_center.Contains("MKT") || a.cost_center.Contains("Split")).ToList(), company_data);

                    }
                    
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
                    count_mdfs = mdf_main.Where(a => a.mdf_comp == item.comp_ID).Count();
                    List<object> rowValues = new List<object>() { item.comp_name, item.comp_ID, item.comp_region, comp_type_name, count_mdfs };
                    if(bMDF || costCenter == "Split")
                    {
                        rowValues.AddRange(new List<object> { item.comp_MDF_amount, item.comp_MDF_aLimit, item.comp_MDF_rLimit, item.comp_MDF_trLimit, item.comp_MDF_rtLimit, item.comp_MDF_sLimit, item.comp_MDF_dLimit, company_metrics.FirstOrDefault().totalPending, company_metrics.FirstOrDefault().totalApproved, company_metrics.FirstOrDefault().totalComplete, company_metrics.FirstOrDefault().totalMDFUtilized, company_metrics.FirstOrDefault().totalMDFAva, company_metrics.FirstOrDefault().totalCredit });
                    }
                    if(costCenter == "MKT CC 7615" || costCenter == "Split")
                    {
                        rowValues.AddRange(new List<object> { item.comp_MKT_Limit, company_metrics_mkt.FirstOrDefault().totalPending, company_metrics_mkt.FirstOrDefault().totalApproved, company_metrics_mkt.FirstOrDefault().totalComplete, company_metrics_mkt.FirstOrDefault().totalMDFUtilized, company_metrics_mkt.FirstOrDefault().totalMDFAva, company_metrics_mkt.FirstOrDefault().totalCredit });
                    }
                    mdf_to_excel.Rows.Add(rowValues.ToArray());
                }


                //foreach (var item in list_comp.GroupBy(a => new { a.comp_ID, a.comp_name }).OrderBy(b => b.Key.comp_name))
                //{
                //    partnerCompanyViewModel company_data = await partnerComp.Where(a => a.comp_ID == item.Key.comp_ID).FirstOrDefaultAsync();
                //    var Company_mdf_main = mdf_main.Where(a => a.archive_year == null && a.mdf_comp == item.Key.comp_ID).ToList();
                //    company_metrics = comm.MDFsActivities(Company_mdf_main, company_data);//Calculate the percentages and mdf utilized
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
                //    count_mdfs = mdf_main.Where(a => a.mdf_comp == item.Key.comp_ID).Count();
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

                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate).ToList();
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_date >= ssDate && a.mdf_date <= eeDate).ToList();
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate).ToList();
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate).ToList();
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate).ToList();
                }
                else
                {
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year).ToList();
                }
                if (bMDF)
                {
                    mdf_main = mdf_main.Where(x => string.IsNullOrEmpty(x.cost_center) || x.cost_center == costCenter || x.cost_center == "Split").ToList();
                }
                else
                {
                    mdf_main = mdf_main.Where(x => x.cost_center == costCenter || x.cost_center == "Split").ToList();
                }

                IEnumerable<partnerType> partner_type = await dbEntity.partnerTypes.ToListAsync();

                //List the companies that are in the MDFs
                //List<partnerCompanyViewModel> list_comp = new List<partnerCompanyViewModel>();
                //foreach (var mdf in mdf_main)
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
                if (bMDF || costCenter == "Split")
                {
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
                    mdf_to_excel.Columns.Add("Amount Pending" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Approved" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Completed" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Utilized" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Available" + mdfAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Credited" + mdfAppend, typeof(string));
                }
                if (costCenter == "MKT CC 7615" || costCenter == "Split")
                {
                    mdf_to_excel.Columns.Add("Total MKT", typeof(string));
                    mdf_to_excel.Columns.Add("Amount Pending" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Approved" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Completed" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Utilized" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Available" + mktAppend, typeof(string));
                    mdf_to_excel.Columns.Add("Amount Credited" + mktAppend, typeof(string));
                }
                

                foreach (var item in partnerComp.OrderBy(a=>a.comp_name))
                {
                    partnerCompany_Archive company_data = await partnerComp.Where(a => a.comp_ID == item.comp_ID).FirstOrDefaultAsync();
                    var Company_mdf_main = mdf_main.Where(a => a.archive_year == archive_year && a.mdf_comp == item.comp_ID);
                    if (bMDF || costCenter == "Split")
                    {
                        company_metrics = comm.MDFsActivitiesArchive(Company_mdf_main.Where(a => (string.IsNullOrEmpty(a.cost_center) || a.cost_center.Contains("MDF") || a.cost_center == "Split")).ToList(), company_data);//Calculate the percentages and mdf utilized
                    }
                    if (costCenter == "MKT CC 7615" || costCenter == "Split")
                    {
                        company_metrics_mkt = comm.MKTsActivitiesArchive(Company_mdf_main.Where(a => a.cost_center.Contains("MKT") || a.cost_center == "Split").ToList(), company_data);
                    }
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
                    count_mdfs = mdf_main.Where(a => a.mdf_comp == item.comp_ID).Count();
                    List<object> rowValues = new List<object>() { company_data.comp_name, item.comp_ID, company_data.comp_region, comp_type_name, count_mdfs };
                    if (bMDF || costCenter == "Split")
                    {
                        rowValues.AddRange(new List<object> { company_data.comp_MDF_amount, company_data.comp_MDF_aLimit, company_data.comp_MDF_oLimit, company_data.comp_MDF_tLimit, company_data.comp_MDF_mLimit, company_data.comp_MDF_dLimit, company_data.comp_MDF_rLimit, company_data.comp_MDF_trLimit, company_data.comp_MDF_rtLimit, company_data.comp_MDF_sLimit, company_metrics.FirstOrDefault().totalPending, company_metrics.FirstOrDefault().totalApproved, company_metrics.FirstOrDefault().totalComplete, company_metrics.FirstOrDefault().totalMDFUtilized, company_metrics.FirstOrDefault().totalMDFAva, company_metrics.FirstOrDefault().totalCredit });
                    }
                    if (costCenter == "MKT CC 7615" || costCenter == "Split")
                    {
                        rowValues.AddRange(new List<object> { company_data.comp_MKT_Limit, company_metrics_mkt.FirstOrDefault().totalPending, company_metrics_mkt.FirstOrDefault().totalApproved, company_metrics_mkt.FirstOrDefault().totalComplete, company_metrics_mkt.FirstOrDefault().totalMDFUtilized, company_metrics_mkt.FirstOrDefault().totalMDFAva, company_metrics_mkt.FirstOrDefault().totalCredit });
                    }
                    mdf_to_excel.Rows.Add(rowValues.ToArray());
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
            string costCenter = Request.Form["cost_center"];
            bool bMDF = (string.IsNullOrEmpty(costCenter) || costCenter == "MDF CC 7200");

            IQueryable<mdf_main> mdf_main;
            IEnumerable<mdf_subType> sub_type = await dbEntity.mdf_subType.ToListAsync();

            if (string.IsNullOrEmpty(year))
            {
                IQueryable<partnerCompanyViewModel> partnerComp = db.partnerCompanyViewModels;//Get company data
                if (dateType == "1" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Request Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate);
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_date >= ssDate && a.mdf_date <= eeDate);
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate);
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate);
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate);
                }
                else
                {
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == null);
                }
                if (bMDF)
                {
                    mdf_main = mdf_main.Where(x => string.IsNullOrEmpty(x.cost_center) || x.cost_center == costCenter || x.cost_center == "Split");
                }
                else
                {
                    mdf_main = mdf_main.Where(x => x.cost_center == costCenter || x.cost_center == "Split");
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
                mdf_to_excel.Columns.Add("Amount Approved (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Reviewed Date", typeof(string));
                mdf_to_excel.Columns.Add("Completed Amount", typeof(string));
                mdf_to_excel.Columns.Add("Completed Amount (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Validation Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issue Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Memo Number", typeof(string));

                //List the companies that are in the MDFs
                List<MDFViewModel> list_mdfs = new List<MDFViewModel>();
                foreach (var mdf in mdf_main)
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
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()))
                            {
                                //Filter only by request Type
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter only by status Type
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                            {
                                if (reqType.Contains(mdf.mdf_type.ToString()))
                                {
                                    //Filter by company type and request Type
                                    list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter by company type and status Type
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                //Filter by request type and status Type
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else
                        {
                            list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = "", usr_lName = "", usr_email = "", mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
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
                    mdf_to_excel.Rows.Add(mdf.mdf_comp, mdf.region, mdf.company, mdf.usr_fName + " " + mdf.usr_lName, mdf.usr_email, mdf.mdf_ID, status, mdf.request_type, mdf.mdf_title, mdf.mdf_SAP, mdf.mdf_totalCost, mdf.mdf_mdfCost, mdf.mdf_requestDate, mdf.mdf_date, mdf.mdf_approvedAmt, mdf.mdf_approvedAmt_mkt, mdf.mdf_reviewDate, mdf.mdf_validatedAmt, mdf.mdf_validatedAmt_mkt, mdf.mdf_validationDate, mdf.mdf_validatedAmt, mdf.mdf_validatedAmt_mkt, mdf.mdf_creditIssueDate, mdf.mdf_creditMemoNum);
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
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_requestDate >= ssDate && a.mdf_requestDate <= eeDate);
                }
                else if (dateType == "2" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Expected Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_date >= ssDate && a.mdf_date <= eeDate);
                }
                else if (dateType == "3" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Review Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_reviewDate >= ssDate && a.mdf_reviewDate <= eeDate);
                }
                else if (dateType == "4" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Validation Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_validationDate >= ssDate && a.mdf_validationDate <= eeDate);
                }
                else if (dateType == "5" && !string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
                {
                    //Credit Issue Date
                    DateTime ssDate = Convert.ToDateTime(sDate);
                    DateTime eeDate = Convert.ToDateTime(eDate);
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year && a.mdf_creditIssueDate >= ssDate && a.mdf_creditIssueDate <= eeDate);
                }
                else
                {
                    mdf_main = dbEntity.mdf_main.Where(a => a.archive_year == archive_year);
                }
                if (bMDF)
                {
                    mdf_main = mdf_main.Where(x => string.IsNullOrEmpty(x.cost_center) || x.cost_center == costCenter || x.cost_center == "Split");
                }
                else
                {
                    mdf_main = mdf_main.Where(x => x.cost_center == costCenter || x.cost_center == "Split");
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
                mdf_to_excel.Columns.Add("Amount Approved (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Reviewed Date", typeof(string));
                mdf_to_excel.Columns.Add("Validated Amount", typeof(string));
                mdf_to_excel.Columns.Add("Validated Amount (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Validation Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issued (MKT)", typeof(string));
                mdf_to_excel.Columns.Add("Credit Issue Date", typeof(string));
                mdf_to_excel.Columns.Add("Credit Memo Number", typeof(string));

                //List the companies that are in the MDFs
                List<MDFViewModel> list_mdfs = new List<MDFViewModel>();
                foreach (var mdf in mdf_main)
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
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()))
                        {
                            //Filter only by request Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter only by status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name,mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type))
                        {
                            if (reqType.Contains(mdf.mdf_type.ToString()))
                            {
                                //Filter by company type and request Type
                                if (usr_data == null)
                                {
                                    list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                                else
                                {
                                    list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter by company type and status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            //Filter by request type and status Type
                            if (usr_data == null)
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                        }
                        else if (!string.IsNullOrEmpty(comp_type) && !string.IsNullOrEmpty(reqType) && !string.IsNullOrEmpty(reqStatus) && comp_type.Contains(cp_type) && reqType.Contains(mdf.mdf_type.ToString()) && reqStatus.Contains(mdf.mdf_status.ToString()))
                        {
                            if (usr_data==null) {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
                            }
                            else
                            {
                                list_mdfs.Add(new MDFViewModel { mdf_comp = mdf.mdf_comp, region = company_data.comp_region, company = company_data.comp_name, usr_fName = usr_data.usr_fName, usr_lName = usr_data.usr_lName, usr_email = usr_data.usr_email, mdf_ID = mdf.mdf_ID, mdf_status = mdf.mdf_status, request_type = mdf_type_name, mdf_title = mdf.mdf_title, mdf_SAP = mdf.mdf_SAP, mdf_totalCost = mdf.mdf_totalCost, mdf_mdfCost = mdf.mdf_mdfCost, mdf_requestDate = mdf.mdf_requestDate, mdf_date = mdf.mdf_date, mdf_approvedAmt = mdf.mdf_approvedAmt, mdf_approvedAmt_mkt = mdf.mdf_approvedAmt_mkt, mdf_reviewDate = mdf.mdf_reviewDate, mdf_validatedAmt = mdf.mdf_validatedAmt, mdf_validatedAmt_mkt = mdf.mdf_validatedAmt_mkt, mdf_validationDate = mdf.mdf_validationDate, mdf_creditIssueDate = mdf.mdf_creditIssueDate, mdf_creditMemoNum = mdf.mdf_creditMemoNum });
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
                    mdf_to_excel.Rows.Add(mdf.mdf_comp, mdf.region, mdf.company, mdf.usr_fName + " " + mdf.usr_lName, mdf.usr_email, mdf.mdf_ID, status, mdf.request_type, mdf.mdf_title, mdf.mdf_SAP, mdf.mdf_totalCost, mdf.mdf_mdfCost, mdf.mdf_requestDate, mdf.mdf_date, mdf.mdf_approvedAmt, mdf.mdf_approvedAmt_mkt, mdf.mdf_reviewDate, mdf.mdf_validatedAmt, mdf.mdf_validatedAmt_mkt, mdf.mdf_validationDate, mdf.mdf_validatedAmt, mdf.mdf_validatedAmt_mkt, mdf.mdf_creditIssueDate, mdf.mdf_creditMemoNum);
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
            MDFfiles mDFFilesModel = await db.mdfFiles.FindAsync(id);
            if (mDFFilesModel == null)
            {
                return HttpNotFound();
            }
            db.mdfFiles.Remove(mDFFilesModel);
            await db.SaveChangesAsync();
            //Delete the file from the system 
            string sourcePath = string.Format("~/attachments/mdf/images/{0}", @mDFFilesModel.mdf_file_name);
            if (System.IO.File.Exists(Server.MapPath(sourcePath)))
            {
                var path = Server.MapPath(sourcePath);
                System.IO.File.Delete(path);
            }

            return RedirectToAction((string.IsNullOrEmpty(redirect) ? "Details" : redirect), new { id = mDFFilesModel.mdf_ID, n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The file has been deleted" });
        }

        // GET: MDFView/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            if (mDFViewModel == null)
            {
                return HttpNotFound();
            }
            var mdfFile = db.mdfFiles.Where(a => a.mdf_ID == id);
            if (mdfFile != null)
            {
                db.mdfFiles.RemoveRange(mdfFile);
            }

            foreach (var item in mdfFile)
            {
                //Delete the file from the system 
                string sourcePath = string.Format("~/attachments/mdf/images/{0}", @item.mdf_file_name);
                if (System.IO.File.Exists(Server.MapPath(sourcePath)))
                {
                    var path = Server.MapPath(sourcePath);
                    System.IO.File.Delete(path);
                }
            }

            db.MDFViewModels.Remove(mDFViewModel);
            await db.SaveChangesAsync();

            return RedirectToAction("ManageAdmin", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The MDF has been deleted" });
        }

        // POST: MDFView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            MDFViewModel mDFViewModel = await db.MDFViewModels.FindAsync(id);
            db.MDFViewModels.Remove(mDFViewModel);
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

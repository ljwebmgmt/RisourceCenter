using newrisourcecenter.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    public class BidRegistrationController : Controller
    {
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
        CommonController commCtl = new CommonController();
        private string html = @"Hello,<br><br>You have received a request to register a deal from the RiSource Center that requires your review. Please log into the RiSource Center prior to clicking the link and you will be taken directly to the <b>Register a Deal</b> Portal to approve or deny the request. Please respond within 48 hours of receipt of this email.<br><br><br><a href=" + "{0}" + @">Click here after logging into RiSource Center </a>";
        private string approveHtml = @"Hello {0},<br><br>Your <b>Register a Deal</b> request (ID # {1}) has been approved.<br/><br/>Click <a href='" + "{2}" + "'>here</a> to view the request.";
        private string rejectHtml = @"Hello {0},<br><br>Your <b>Register a Deal</b> request (ID # {1}) has been rejected by {2}.<br/><br/>Click <a href='" + "{3}" + "'>here</a> to view the request.";
        private string createHtml = @"Hello {0},<br><br>Your <b>Register a Deal</b> request (ID # {1}) has been received and sent to Rittal IT Team for approval.<br/>You will receive a response within 5-7 Business Days.<br/><br/>Click <a href='" + "{2}" + "'>here</a> to view the request.";
        string adminEmail = "czlonka.n@rittal.us";

        // GET: BidRegistration
        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            return Redirect("https://rittal-interlynx.com/srv/sso-dealautologin?sso_auth_str=" + CommonController.CreateMD5("43dt3cse@34a&l3~" + Convert.ToString(Session["userEmail"]).ToLower()) + "&path=dealregistration_tab%2Frebate_deals_type_active");
            //Dictionary<long, string> companies = db.partnerCompanies.Where(x => x.bid_registration == 1).ToDictionary(x => x.comp_ID, y => y.comp_name);
            //List<BidRegistration> registrationsList = db.BidRegistrations.Where(x => x.created_by == userId).ToList();
            //registrationsList.ForEach(x =>
            //{
            //    var usr = db.usr_user.Where(u => u.usr_ID == x.created_by).FirstOrDefault();
            //    x.username = usr.usr_fName + " " + usr.usr_lName;
            //    x.companyName = companies.ContainsKey(x.company_id) ? companies[x.company_id] : "";
            //});
            //return View(registrationsList);
        }

        // GET: BidRegistration/Create

        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var companies = from list in db.partnerCompanies where list.bid_registration == 1 orderby list.comp_name ascending select list;
            List<CompData> comp_listing = new List<CompData>();
            foreach (var item in companies)//iterate the add function
            {
                comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
            }
            ViewBag.partnerComp = comp_listing;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Create(BidRegistration bidRegistration, IEnumerable<HttpPostedFileBase> fileupload)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                bidRegistration.created_by = Convert.ToInt32(Session["userId"]);
                bidRegistration.created_on = DateTime.Now;
                db.BidRegistrations.Add(bidRegistration);
                if (fileupload.Count() > 0)
                    bidRegistration.files = SaveFiles(fileupload);
                db.SaveChanges();
                var company = db.partnerCompanies.Where(x => x.comp_ID == bidRegistration.company_id && !string.IsNullOrEmpty(x.it_territory_manager)).FirstOrDefault();
                if(company != null)
                {
                    commCtl.email("webmaster@rittal.us", company.it_territory_manager, "Register a Deal Request - IT Channel/Territory Manager Approval", string.Format(this.html, Url.Action("ViewRequest", "BidRegistration", new { id = bidRegistration.ID, type = "it_territory_manager" }, this.Request.Url.Scheme)), "yes", true);
                }
                commCtl.email("webmaster@rittal.us", Convert.ToString(Session["userEmail"]) + "," + "krantz.s@rittal.us", "New Register a Deal Request", string.Format(this.createHtml, (Session["firstName"] + " " + Session["lastName"]).Trim(),bidRegistration.ID, Url.Action("ViewRequest", "BidRegistration", new { id = bidRegistration.ID }, this.Request.Url.Scheme)), "yes", true);
                return RedirectToAction("Index");
            }

            return View(bidRegistration);
        }

        public string SaveFiles(IEnumerable<HttpPostedFileBase> fileupload)
        {
            List<string> uploadedFiles = new List<string>();
            foreach (HttpPostedFileBase file in fileupload)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var guid = Guid.NewGuid().ToString();
                    var NewFileName = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/bid_registrations"), NewFileName);
                    file.SaveAs(path);
                    uploadedFiles.Add(fileName + ":" + NewFileName);
                }
            }
            return String.Join(",", uploadedFiles);
        }

        // GET: BidRegistration/Edit/5
        public ActionResult Edit(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            BidRegistration bidRegistration = db.BidRegistrations.Find(id);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null || bidRegistration == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string[] pages = Session["userPages"] != null ? Convert.ToString(Session["userPages"]).Split(',') :  new string[] { };
            bool bAdmin = pages.Contains(ConfigurationManager.AppSettings["BidRegistrationAdminNav"]);
            if (bidRegistration.created_by != userId && !bAdmin)
            {
                return RedirectToAction("Index");
            }
            var companies = from list in db.partnerCompanies where list.bid_registration == 1 orderby list.comp_name ascending select list;
            List<CompData> comp_listing = new List<CompData>();
            foreach (var item in companies)//iterate the add function
            {
                comp_listing.Add(new CompData { comp_name = item.comp_name, comp_ID = item.comp_ID });
            }
            ViewBag.partnerComp = comp_listing;
            ViewBag.isAdmin = bAdmin ? "1" : "0";
            return View("Create",bidRegistration);
        }

        // POST: BidRegistration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Edit(BidRegistration bidRegistration, IEnumerable<HttpPostedFileBase> fileupload)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                bidRegistration.updated_by = Convert.ToInt32(Session["userId"]);
                bidRegistration.updated_on = DateTime.Now;
                if (fileupload.Count() > 0)
                    bidRegistration.files += (bidRegistration.files != "" ? "," : "") + SaveFiles(fileupload);
                db.Entry(bidRegistration).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bidRegistration);
        }

        [HttpGet]
        public async Task<ActionResult> DeleteFile(int? id, string file)
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
            var bidRegistration = await db.BidRegistrations.FindAsync(id);
            if (bidRegistration == null)
            {
                return HttpNotFound();
            }
            string[] parts = file.Split(':');
            if(parts.Length == 0)
            {
                return HttpNotFound();
            }
            string path = Path.Combine(Server.MapPath("~/attachments/bid_registrations"), parts[1]);
            FileInfo physicalFile = new FileInfo(path);
            if (physicalFile.Exists)//check file exsit or not  
            {
                physicalFile.Delete();
            }
            string[] files = bidRegistration.files.Split(',');
            files = files.Where(x => x != file).ToArray();
            bidRegistration.files = files.Count() > 0 ? String.Join(",", files) : "";
            db.Entry(bidRegistration).State = System.Data.Entity.EntityState.Modified;
            await db.SaveChangesAsync();
            return Redirect(Url.Action("Edit", new { id = id, n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], filesuccess = "The file has been removed" }) + "#images");
        }

        // GET: BidRegistration/ViewRequest/5
        public ActionResult ViewRequest(int? id)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            string type = !string.IsNullOrEmpty(Request.QueryString["type"]) ? Request.QueryString["type"] : "";
            BidRegistration bidRegistration = db.BidRegistrations.Find(id);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null || bidRegistration == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var company = db.partnerCompanies.Where(x => x.comp_ID == bidRegistration.company_id).FirstOrDefault();
            if (!string.IsNullOrEmpty(type))
            {
                Dictionary<string, string> emails = new Dictionary<string, string>() { { "it_territory_manager", company.it_territory_manager }, { "general_manager", company.general_manager } };
                if (company == null || !emails.ContainsKey(type) || Convert.ToString(Session["userEmail"]).ToLower() != emails[type].ToLower())
                {
                    return RedirectToAction("Index"); 
                }
            }
            bidRegistration.companyName = company.comp_name;
            ViewBag.approverType = type;
            return View("View", bidRegistration);
        }

        // POST: BidRegistration/ViewRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult ViewRequest(BidRegistration obj)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            BidRegistration bidObj = db.BidRegistrations.Where(x => x.ID == obj.ID).FirstOrDefault();
            if(bidObj != null)
            {
                bidObj.updated_by = Convert.ToInt32(Session["userId"]);
                bidObj.updated_on = DateTime.Now;
                var company = db.partnerCompanies.Where(x => x.comp_ID == bidObj.company_id).FirstOrDefault();
                var requestor = db.usr_user.Where(u => u.usr_ID == bidObj.created_by).FirstOrDefault();
                string email = "";
                string type = "";
                if (obj.approver_type == "it_territory_manager")
                {
                    bidObj.channel_manager_approval = obj.channel_manager_approval;
                    if (obj.status == "approve")
                        bidObj.territory_manager_approved = 1;
                    email = company.general_manager;
                    type = "General Manager";
                }
                else
                {
                    bidObj.gm_approval = obj.gm_approval;
                    if (obj.status == "approve")
                        bidObj.gm_approved = 1;
                }

                db.Entry(bidObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                if (obj.status == "approve")
                {
                    if (!string.IsNullOrEmpty(email))
                        commCtl.email("webmaster@rittal.us", email, "Register a Deal Request - " + type + " Approval", string.Format(this.html, Url.Action("ViewRequest", "BidRegistration", new { id = bidObj.ID, type = "general_manager" }, this.Request.Url.Scheme)), "yes", true);
                    else
                        commCtl.email("webmaster@rittal.us", requestor.usr_email + "," + adminEmail, "Register a Deal Request - Approved", string.Format(this.approveHtml,requestor.usr_fName + " " + requestor.usr_lName, bidObj.ID, Url.Action("ViewRequest", "BidRegistration", new { id = bidObj.ID }, this.Request.Url.Scheme)), "yes", true);
                }
                else
                    commCtl.email("webmaster@rittal.us", requestor.usr_email + "," + adminEmail, "Register a Deal Request - Rejected", string.Format(this.rejectHtml, requestor.usr_fName + " " + requestor.usr_lName, bidObj.ID, Session["firstName"] + " " + Session["lastName"] + " (" + Session["userEmail"] + ")", Url.Action("ViewRequest", "BidRegistration", new { id = bidObj.ID }, this.Request.Url.Scheme)), "yes", true);
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET: BidRegistration/Admin
        public ActionResult Admin()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            Dictionary<long, string> companies = db.partnerCompanies.Where(x => x.bid_registration == 1).ToDictionary(x => x.comp_ID, y => y.comp_name);
            List<BidRegistration> registrationsList = db.BidRegistrations.ToList();
            registrationsList.ForEach(x =>
            {
                var usr = db.usr_user.Where(u => u.usr_ID == x.created_by).FirstOrDefault();
                x.username = usr != null ? usr.usr_fName + " " + usr.usr_lName : "";
                x.companyName = companies.ContainsKey(x.company_id) ? companies[x.company_id] : "";
            });
            return View(registrationsList);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bidRegistration = db.BidRegistrations.Find(id);
            if (bidRegistration == null)
            {
                return HttpNotFound();
            }
            string[] pages = Session["userPages"] != null ? Convert.ToString(Session["userPages"]).Split(',') : new string[] { };
            bool bAdmin = pages.Contains(ConfigurationManager.AppSettings["BidRegistrationAdminNav"]);
            if (!bAdmin)
            {
                return RedirectToAction("Index");
            }
            db.BidRegistrations.Remove(bidRegistration);
            db.SaveChanges();
            return RedirectToAction("Admin");
        }
    }
}
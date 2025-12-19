using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace newrisourcecenter.Controllers
{
    //[Authorize]
    public class SupportToolsController : Controller
    {
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
        private RisourceCenterContext dbEnity = new RisourceCenterContext();
        private CommonController comm = new CommonController();

        #region Index
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            SupportToolsModels supporttools = new SupportToolsModels();

            var n1_descLong = db.nav1.Where(a => a.n1ID == 3).FirstOrDefault().n1_descLong;

            string industry = Convert.ToString(Session["userIndustry"]);
            string usrType = Convert.ToString(Session["companyType"]);
            string products = Convert.ToString(Session["userProducts"]);
            string siteRole = "";
            int id = 3;
            if (User.IsInRole("Super Admin"))
            {
                siteRole = "1";
            }
            
            CommonController commonController = new CommonController();
            List<nav2> nav2 = commonController.SubmenFilter(industry, usrType, products, siteRole, id);

            supporttools.n1_descLong = n1_descLong;
            supporttools.list_n2_data = nav2;

            return View(supporttools);
        }
        #endregion

        #region Inframed Pages
        public ActionResult PricingAndAvailability()
        {
            return View();
        }

        public ActionResult TSEightPricingTool()
        {
            return View();
        }

        [Authorize]
        public ActionResult SalesWins()
        {
            return View();
        }
        #endregion

        public ActionResult CollateralFulfillment()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = db.usr_user.Where(a => a.usr_ID == userId).FirstOrDefault();
            ViewBag.userEmail = user.usr_email;
            return View();
        }

        #region track emails
        [HttpPost]
        [AllowAnonymous]
        public async Task TrackEmail(string usraction=null, string type=null, string url=null, string sent=null, int usr=0)
        {
            DateTime date_sent = Convert.ToDateTime(sent);
            email_tracker trach_emails = new email_tracker {
                msg_action = usraction,
                email_type = type,
                date_opened = DateTime.Today,
                url_tracked = url,
                date_sent = date_sent,
                usr_ID = usr
            };

            db.email_tracker.Add(trach_emails);
            await db.SaveChangesAsync();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult WeeklyNewsletter(string sent = null, int usr = 0)
        {
            string usraction = "open";
            string type = "wN";
            string url = null;
            DateTime date_sent = Convert.ToDateTime(sent);
            email_tracker trach_emails = new email_tracker
            {
                msg_action = usraction,
                email_type = type,
                date_opened = DateTime.Today,
                url_tracked = url,
                date_sent = date_sent,
                usr_ID = usr
            };

            db.email_tracker.Add(trach_emails);
            db.SaveChanges();

            var dir = Server.MapPath("/Images");
            var path = Path.Combine(dir + "/blank.gif");
            return base.File(path, "image/jpeg");
        }
        #endregion

        #region Site Tracker Report
        public  async Task<ActionResult> SiteTrackerReport(int limit=0, string email_action=null)
        {
            List<emailtrackerViewModel> emailtracker= new List<emailtrackerViewModel>();
            List<GroupCount> group_data = new List<GroupCount>();

            if (email_action=="click")
            {
                var emailtracker_data = dbEnity.emailtrackerViewModels.Where(a=>a.msg_action==email_action).OrderByDescending(a => a.ID);
                foreach (var item in emailtracker_data.GroupBy(a => new { a.usr_ID,a.url_tracked,a.date_sent,a.date_opened}))
                {
                    var count = item.Count();
                    group_data.Add(new GroupCount
                    {
                        count = item.Count(),
                        usr_ID = item.Key.usr_ID,
                        url = item.Key.url_tracked,
                        sent = item.Key.date_sent,
                        opened = item.Key.date_opened
                    });
                }

                foreach (var item in group_data)
                {
                    string full_name = "";
                    //get rep name
                    var fullname = await comm.GetfullName(Convert.ToInt32(item.usr_ID));
                    if (fullname.Count() > 0)
                    {
                        full_name = fullname["fullName"];
                    }
                    emailtracker.Add(new emailtrackerViewModel
                    {
                        count = item.count,
                        full_name = full_name,
                        msg_action = email_action,
                        url_tracked = item.url,
                        date_sent =item.sent,
                        date_opened = item.opened
                    });
                }
            }

            else if (email_action == "date")
            {
                DateTime date = Convert.ToDateTime(Request.QueryString["date"]);
                var emailtracker_data = dbEnity.emailtrackerViewModels.Where(a => a.date_sent == date).OrderByDescending(a => a.ID);
                foreach (var item in emailtracker_data.GroupBy(a => new { a.usr_ID, a.date_sent,a.url_tracked }))
                {
                    var count = item.Count();
                    group_data.Add(new GroupCount
                    {
                        count = item.Count(),
                        usr_ID = item.Key.usr_ID,
                        sent = item.Key.date_sent,
                        url = item.Key.url_tracked
                    });
                }

                foreach (var item in group_data)
                {
                    string full_name = "";
                    //get rep name
                    var fullname = await comm.GetfullName(Convert.ToInt32(item.usr_ID));
                    if (fullname.Count() > 0)
                    {
                        full_name = fullname["fullName"];
                    }
                    emailtracker.Add(new emailtrackerViewModel
                    {
                        count = item.count,
                        full_name = full_name,
                        msg_action = email_action,
                        date_sent = item.sent,
                        url_tracked = item.url
                    });
                }
            }
            else if (email_action == "open")
            {
                var emailtracker_data = dbEnity.emailtrackerViewModels.Where(a => a.msg_action == email_action).OrderByDescending(a => a.ID);
                foreach (var item in emailtracker_data.GroupBy(a => new { a.usr_ID, a.date_sent}))
                {
                    var count = item.Count();
                    group_data.Add(new GroupCount
                    {
                        count = item.Count(),
                        usr_ID = item.Key.usr_ID,
                        sent = item.Key.date_sent
                    });
                }

                foreach (var item in group_data)
                {
                    string full_name = "";
                    //get rep name
                    var fullname = await comm.GetfullName(Convert.ToInt32(item.usr_ID));
                    if (fullname.Count() > 0)
                    {
                        full_name = fullname["fullName"];
                    }
                    emailtracker.Add(new emailtrackerViewModel
                    {
                        count = item.count,
                        full_name = full_name,
                        msg_action = email_action,
                        date_sent = item.sent                        
                    });
                }
            }
            else if (email_action == "all" )
            {
                var emailtracker_data = dbEnity.emailtrackerViewModels.OrderByDescending(a => a.ID);
                foreach (var item in emailtracker_data)
                {
                    string full_name = "";
                    ////get rep name
                    var fullname = await comm.GetfullName(Convert.ToInt32(item.usr_ID));
                    if (fullname.Count() > 0)
                    {
                        full_name = fullname["fullName"];
                    }
                    emailtracker.Add(new emailtrackerViewModel
                    {
                        full_name = full_name,
                        msg_action = item.msg_action,
                        date_sent = item.date_sent,
                        date_opened = item.date_opened,
                        url_tracked = item.url_tracked,
                        email_type = item.email_type
                    });
                }
            }
            else
            {
                var emailtracker_data = dbEnity.emailtrackerViewModels.OrderByDescending(a=>a.ID).Take(100);
                foreach (var item in emailtracker_data)
                {
                    string full_name = "";
                    ////get rep name
                    var fullname = await comm.GetfullName(Convert.ToInt32(item.usr_ID));
                    if (fullname.Count() > 0)
                    {
                        full_name = fullname["fullName"];
                    }
                    emailtracker.Add(new emailtrackerViewModel
                    {
                        full_name = full_name,
                        msg_action = item.msg_action,
                        date_sent = item.date_sent,
                        date_opened = item.date_opened,
                        url_tracked = item.url_tracked,
                        email_type = item.email_type
                    });
                }
            }

            return View(emailtracker);
        }
        #endregion

        #region Crossover Tool
        public ActionResult CrossoverTool()
        {

            return View();
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
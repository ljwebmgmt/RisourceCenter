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
using System.IO;
using System.Configuration;
using static iTextSharp.text.pdf.AcroFields;

namespace newrisourcecenter.Controllers
{
    public partial class SalesCommController
    {
        #region email function
        private void emailfunction(SalesCommViewModel salesCommViewModel, string action)
        {
            var status = "1,2,3";
            string Subject = "";
            string message = "";
            string it_email = "";
            string ie_email = "";
            string To = "";
            string From = "webmaster@rittal.us";
            string status_message = "";
            string host = "https://www.risourcecenter.com";

            if (Request.Url.Port != 443)
            {
                host = "http://"+Request.Url.Host + ":" + Request.Url.Port;
            }

            string header = emailheader(host);
            string footer = emailfooter(host);

            //get the emails to send
            if (salesCommViewModel.sc_industry != null)
            {
                string industry = salesCommViewModel.sc_industry;
                bool islist = industry.Any(m => m.ToString().Contains(","));
                List<string> approverEmails = new List<string>();
                string additionalApprovers = ConfigurationManager.AppSettings["additionalCommApprover"];
                if (!string.IsNullOrEmpty(additionalApprovers))
                {
                    approverEmails.AddRange(additionalApprovers.Split(','));
                }
                if (islist)
                {
                    //Get IT approver email
                    var get_it_approver_email = dbEntity.nav2.Join(
                              dbEntity.usr_user,
                              nav2 => nav2.n2_IT_approver,
                              usr => usr.usr_ID,
                              (nav2, usr) => new { nav2, usr }
                          ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == salesCommViewModel.n2ID);

                    foreach (var item in get_it_approver_email)
                    {
                        it_email = item.usr.usr_email;
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
                        ie_email = item.usr.usr_email;
                        approverEmails.Add(item.usr.usr_email);
                    }
                    

                    To = string.Join(",",approverEmails);
                }
                else
                {
                    if (industry == "1")
                    {
                        //Get IT approver email
                        var get_it_approver_email = dbEntity.nav2.Join(
                                  dbEntity.usr_user,
                                  nav2 => nav2.n2_IT_approver,
                                  usr => usr.usr_ID,
                                  (nav2, usr) => new { nav2, usr }
                              ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == salesCommViewModel.n2ID);

                        foreach (var item in get_it_approver_email)
                        {
                            approverEmails.Add(item.usr.usr_email);
                        }
                    }
                    else if (industry == "2")
                    {
                        //Get IE approver email
                        var get_ie_approver_email = dbEntity.nav2.Join(
                                  dbEntity.usr_user,
                                  nav2 => nav2.n2_IE_approver,
                                  usr => usr.usr_ID,
                                  (nav2, usr) => new { nav2, usr }
                              ).Where(a => a.nav2.n2_active == 1).Where(a => a.nav2.n2ID == salesCommViewModel.n2ID);
                        foreach (var item in get_ie_approver_email)
                        {
                            approverEmails.Add(item.usr.usr_email);
                        }
                    }
                    To = string.Join(",", approverEmails);
                }
            }
            string usrEamil = User.Identity.Name;
            var getUserData = db.UserViewModels.Where(a => a.usr_email == usrEamil).FirstOrDefault();

            //get the email subjects
            if (salesCommViewModel.sc_status == 1)
            {
                if (action == "created")
                {
                    status_message = "New";
                    Subject = "New RiSource Center Communication Awaiting Your Approval";
                    message = string.Format("The following communication has been added by <i>"+ getUserData.usr_fName+" "+ getUserData.usr_lName + "</i> and is awaiting your approval. View the details below and either <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=live\" target=\"_blank\">Approve</a> and publish or <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=inactive\" target=\"_blank\">Deny</a>, which makes the communication Inactive. The author will receive an email based on either of your actions. If you believe the communication needs to be modified in order to be approved, please contact the author at "+ getUserData.usr_email + " or login to the RiSource Center and make the change directly.");
                }
                else
                {
                    status_message = "Staging";
                    Subject = "Updated RiSource Center Communication Awaiting Your Approval";
                    message = string.Format("The following communication has been updated by <i>" + getUserData.usr_fName + " " + getUserData.usr_lName + "</i> and is awaiting your approval. View the details below and either <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=live\" target=\"_blank\">Approve</a> and publish or <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=inactive\" target=\"_blank\">Deny</a>, which makes the communication Inactive. The author will receive an email based on either of your actions. If you believe the communication needs to be further modified in order to be approved, please contact the author at " + getUserData.usr_email + " or login to the RiSource Center and make the change directly.");
                }
            }
            else if (salesCommViewModel.sc_status == 2)
            {
                status_message = "LIVE";
                Subject = "New RiSource Center Communication Awaiting Your Approval";
                message = string.Format("The following communication has been added by <i>" + getUserData.usr_fName + " " + getUserData.usr_lName + "</i> and is awaiting your approval. View the details below and either <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=live\" target=\"_blank\">Approve</a> and publish or <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=inactive\" target=\"_blank\">Deny</a>, which makes the communication Inactive. The author will receive an email based on either of your actions. If you believe the communication needs to be modified in order to be approved, please contact the author at " + getUserData.usr_email + " or login to the RiSource Center and make the change directly.");
            }
            else if (salesCommViewModel.sc_status == 3)
            {
                status_message = "Inactive";
                Subject = "RiSource Center Communication Set To Inactive";
                message = string.Format("The following communication has been set to Inactive by <i>" + getUserData.usr_fName + " " + getUserData.usr_lName + "</i>.If you agree with this action, please <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=inactive\" target=\"_blank\">Approve</a> now.If you disagree, choose <a href=\"" + host + "/SalesComm/confirmation?scID=" + salesCommViewModel.scID + "&status=live\" target=\"_blank\">Deny</a> and the communication will remain in Staging or Active status. The author will receive an email based on either of your actions.");
            }

            //email body
            if (status.Contains(salesCommViewModel.sc_status.ToString()))
            {
                try
                {
                    var n3Data = dbEntity.nav3.Where(a => a.n3ID == salesCommViewModel.n3ID).FirstOrDefault();//Get level 3 data
                    string Level3 = n3Data.n3_nameLong;

                    string emailEnding =
                    header +
                     emailbody(host, Subject, message, status_message, Level3, salesCommViewModel.sc_headline, salesCommViewModel.sc_startDate, salesCommViewModel.sc_endDate, salesCommViewModel.sc_teaser, salesCommViewModel.sc_body) +
                    footer;

                    if (!To.Contains(User.Identity.GetUserName()))
                    {
                        System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailEnding);
                        Email.IsBodyHtml = true;
                        System.Net.Mail.SmtpClient SMPTobj;
                        SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                        SMPTobj.EnableSsl = false;
                        SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                        SMPTobj.Send(Email);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} Error", ex);
                }
            }
        }
        #endregion

        #region email parts
        public string emailheader(string host)
        {
            string head = "";
            try
            {
                head =
                    "<!doctype html>" +
                    "<html>" +
                    "<head>" +
                    "<meta charset=\"utf-8\">" +
                    "<style>" +
                    "body{font-family:Arial, Helvetica, sans-serif;width:600px;}a, a:hover, a:visited{color:#ED0677;}" +
                    ".powerbar{background:#393939;}" +
                    "</style>" +
                    "<title>Rittal North America LLC - RiSource Center</title>" +
                    "</head>" +
                    "<body style=\"width:600px;\">" +
                    "<div style=\"width:600px;\">" +
                    "<table align=\"center\" style=\"background:#393939;width:100%;padding:4px;font-family:Verdana, Arial, Helvetica, sans-serif\">" +
                            "<tr>" +
                                "<td width=\"75px\" style=\"width:75px;\">" +
                                "<img src=\"" + host + "/images/rittal_logo_header.gif\" alt=\"Rittal Logo\" style=\"padding:4px;\" /></td>" +
                                "<td align=\"left\"><span id=\"title\" style=\"color:#FFF;font-weight:bold;font-size:26px;line-height:18px;margin-top:30px;\">&nbsp;<br />RiSource<br />Center <span style=\"font-size:11px;\">2.0</span></span></td>" +
                            "</tr>" +
                    "</table>" +
                    "<div style=\"height:20px;width:100%;background:#292929;\">&nbsp;</div>" +
                    "<strong><p style=\"color:#e2007a;text-align:left;font-family:Verdana, Arial, Helvetica, sans-serif;padding-left:8px;\">" +
                    string.Format("{0:MM/dd/yyyy}", DateTime.Now) +
                    "</p></strong>";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return head;
        }

        public string emailfooter(string host)
        {
            string foot = "";
            try
            {
                foot =
                    "<br />&nbsp;" +
                    "<div style=\"height:20px;width:100%;background:#292929;\">&nbsp;</div>" +
                    "<div class=\"powerBar\" style=\"height:23px\" align=\"center\">" +
                    "<div style=\"background-image:url(" + host + "/images/pbFiller.gif);background-repeat:repeat-x;background-size:100% 23px;\">" +
                    "<img src=\"" + host + "/images/powerBar5.gif\" width=\"100%\"></div>" +
                    "</div>" +
                        "<div align=\"center\" style=\"width:100%;background:#393939;height:100px;font-family:Verdana, Arial, Helvetica, sans-serif;\">&nbsp;" +
                            "<p style=\"color:#FFF;font-size:14px;\">Rittal North America LLC, 1 Rittal Place, Urbana, OH 43078</p>" +
                            "<p style=\"font-size:12px;\"><a href=\"https://www.risourcecenter.com/\" target=\"_blank\">RiSourceCenter.com</a> | <a href=\"https://www.rittal.us/\" target=\"_blank\">Rittal.us</a></p><br />" +
                        "</div>" +
                       "</div>" +
                    "</body>" +
                    "</html>";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return foot;
        }

        private string emailbody(string host, string Subject, string message, string status_message, string Level3, string sc_headline, DateTime? sc_startDate, DateTime? sc_endDate, string sc_teaser, string sc_body)
        {
            string body = "";
            try
            {
                body =
                        "<div style=\"font - family:Verdana, Arial, Helvetica, sans - serif;\">" +
                        "<h2 align=\"center\">" + Subject + "</h2>" +
                        "<p>" + message + "</p>" +
                        "<hr noshade />" +
                        "<table cellpadding=\"5px\" width=\"100%\" border=\"1px\" style=\"border: 1px solid ##000;border-collapse:collapse;border-spacing:0;\">" +
                            "<tr><th colspan=\"2\" style=\"background:##CCC;\">Sales Communication Details</th></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Status</th><td width=\"70%\" align=\"left\">" + status_message + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Level 3</th><td width=\"70%\" align=\"left\">" + Level3 + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Headline</th><td width=\"70%\" align=\"left\">" + sc_headline + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Start Date</th><td width=\"70%\" align=\"left\">" + sc_startDate + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">End Date</th><td width=\"70%\" align=\"left\">" + sc_endDate + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Teaser</th><td width=\"70%\" align=\"left\">" + sc_teaser + "</td></tr>" +
                            "<tr><th width=\"30%\" align=\"right\">Body</th><td width=\"70%\" align=\"left\">" + sc_body + "</td></tr>" +
                        "</table>" +
                        "<hr noshade />" +
                    "</div>";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return body;
        }
        #endregion

        #region Manage communications status
        [AllowAnonymous]
        public async Task<ActionResult> confirmation(string status = null, int scID = 0)
        {
            string From = "webmaster@rittal.us";
            string status_message = "";
            string Subject = "";
            string message = "";
            string user_email = "";
            string user_fName = "";
            string user_lName = "";
            string host = "https://www.risourcecenter.com";

            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }
            string header = emailheader(host);
            string footer = emailfooter(host);
            int status_value = 0;

            //Get IE user email
            var get_ie_approver_email = dbEntity.salesComms.Join(
                      dbEntity.usr_user,
                      sales => sales.sc_owner,
                      usr => usr.usr_ID,
                      (sales, usr) => new { sales, usr }
                  ).Where(a => a.sales.scID == scID);

            foreach (var item in get_ie_approver_email)
            {
                user_email = item.usr.usr_email;
                user_fName = item.usr.usr_fName;
                user_lName = item.usr.usr_lName;
                ViewBag.user_fName = item.usr.usr_fName;
                ViewBag.user_lName = item.usr.usr_lName;
                ViewBag.sales_title = item.sales.sc_headline;
                ViewBag.sc_headline = item.sales.sc_headline;
                ViewBag.sc_startDate = item.sales.sc_startDate;
                ViewBag.sc_endDate = item.sales.sc_endDate;
                ViewBag.sc_teaser = item.sales.sc_teaser;
                ViewBag.sc_body = item.sales.sc_body;
                ViewBag.n3ID = item.sales.n3ID;
            }

            long n3ID = ViewBag.n3ID;
            var n3Data = dbEntity.nav3.Where(a => a.n3ID == n3ID).FirstOrDefault();//Get level 3 data
            string Level3 = n3Data.n3_nameLong;

            if (status == "live")
            {
                status_message = "Live";
                status_value = 2;
                Subject = "Your RiSource Center Communication has been approved";
                message = string.Format("Dear " + user_fName + " " + user_lName + ",<br /><br />The following RiSource Center Communication has been approved and it is <b>" + status_message + "</b>. View the details below or click <a href=\"" + host + "/SalesComm/Edit/" + scID + "\" target=\"_blank\">HERE</a> to manage your Communication.<br /><br />");
            }
            else
            {
                status_message = "Inactive";
                status_value = 3;
                Subject = "Your RiSource Center Communication has been denied";
                message = string.Format("Dear " + user_fName + " " + user_lName + ",<br /><br />The following RiSource Center Communication has been denied and it is <b>" + status_message + "</b>. View the details below or click <a href=\"" + host + "/SalesComm/Edit/" + scID + "\" target=\"_blank\">HERE</a> to manage your communication and resubmit.<br /><br />");
            }

            try
            {
                string emailEnding =
                         header +
                          emailbody(host, Subject, message, status_message, Level3, ViewBag.sc_headline, ViewBag.sc_startDate, ViewBag.sc_endDate, ViewBag.sc_teaser, ViewBag.sc_body) +
                         footer;

                System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, user_email, Subject, emailEnding);
                Email.IsBodyHtml = true;
                System.Net.Mail.SmtpClient SMPTobj;
                SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                SMPTobj.EnableSsl = false;
                SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                SMPTobj.Send(Email);

                ViewBag.message = "The user will be notified of this action";

                //try to save changes to the status
                var model = dbEntity.salesComms.Find(scID);
                model.sc_status = status_value;
                await dbEntity.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return View();
        }
        #endregion

        #region Weekly Email
        [AllowAnonymous]
        public ActionResult WeeklyEmailTemplate(object mail)
        {
            return View();
        }

        protected string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> WeeklyEmail()
        {
            DateTime date = DateTime.Today.AddDays(-7);
            string From = "RiSource@rittal.us";
            string To = "";
            string host = "https://www.risourcecenter.com";

            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }

            string header = emailheader(host);
            string footer = emailfooter(host);
            string Subject = "RiSource Center Weekly Announcements";
            string emailbody = "";

            ViewBag.WeeklyEmail = "This is the weekly email";
            //Change this usr_user to usr_urser_test to test the email system
            var usrs = dbEntity.usr_user.Join(
                         dbEntity.partnerCompanies,
                         usr => usr.comp_ID,
                         comp => comp.comp_ID,
                         (usr, comp) => new { usr, comp }
                     )
                     .Join(
                    dbEntity.AspNetUsers,
                    usr => usr.usr.usr_email,
                    asp => asp.Email,
                    (userr,asp)=> new { userr,asp}
                ).Where(a=>a.asp.EmailConfirmed == true && a.userr.comp.comp_active == 1 && a.userr.usr.wN==1);
                //.Where(a => a.asp.EmailConfirmed == true && a.userr.comp.comp_active == 1 && a.userr.usr.usr_email.Contains("antwi")); use for testing
            foreach (var usr_comp in usrs)
            {
                List<WeeklyEmail> mail = new List<WeeklyEmail>();

                To = usr_comp.userr.usr.usr_email;

                var getSales = dbEntity.salesComms.Join(
                            dbEntity.nav2,
                            salesCom => salesCom.n2ID,
                            nav2 => nav2.n2ID,
                            (salesCom, nav2) => new { salesCom, nav2 })
                            .Where(a => a.salesCom.sc_status == 2)
                            .Where(a => a.salesCom.sc_usrTypes.Contains(usr_comp.userr.comp.comp_type.ToString()))
                            .Where(a => a.salesCom.sc_startDate >= date);

                if (usr_comp.userr.comp.comp_industry == 1)
                {
                    getSales = getSales.Where(a => a.salesCom.sc_industry.Contains("1"));
                }
                else if (usr_comp.userr.comp.comp_industry == 2)
                {
                    getSales = getSales.Where(a => a.salesCom.sc_industry.Contains("2"));
                }
                else
                {
                    getSales = getSales.Where(a => a.salesCom.sc_industry.Contains("1") || a.salesCom.sc_industry.Contains("2"));
                }

                string products = usr_comp.userr.comp.comp_products;
                if (!string.IsNullOrEmpty(products))
                {
                    bool islist = products.Any(m => m.ToString().Contains(","));
                    if (!islist)
                    {
                        //if product is not a list
                        foreach (var sales in getSales.Where(a => a.salesCom.sc_products.Contains(products)).OrderByDescending(a => a.salesCom.scID))
                        {
                            mail.Add(new WeeklyEmail { usr_email = usr_comp.userr.usr.usr_email, sc_headline = sales.salesCom.sc_headline, comp_industry = usr_comp.userr.comp.comp_industry, scID = sales.salesCom.scID, sc_industry = sales.salesCom.sc_industry, sc_products = sales.salesCom.sc_products,n3Id=sales.salesCom.n3ID,send_date=DateTime.Today });
                        }
                    }
                    else
                    {
                        var prod = products.ToArray();
                        //product is a list
                        foreach (var sales in getSales.OrderByDescending(a => a.salesCom.scID))
                        {
                            string sales_products = sales.salesCom.sc_products;
                            if (!string.IsNullOrEmpty(sales_products))
                            {
                                foreach (var item in prod)
                                {
                                    if (sales.salesCom.sc_products.Contains(@item.ToString()))
                                    {
                                        mail.Add(new WeeklyEmail { usr_email=usr_comp.userr.usr.usr_email, sc_headline=sales.salesCom.sc_headline, comp_industry = usr_comp.userr.comp.comp_industry, scID=sales.salesCom.scID, sc_industry=sales.salesCom.sc_industry,sc_products=sales.salesCom.sc_products,n3Id=sales.salesCom.n3ID,send_date=DateTime.Today });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                string list = "";
                list = RenderViewToString("WeeklyEmailTemplate", mail);
                emailbody =
                            "<div style=\"font-family:Verdana, Arial, Helvetica, sans-serif;\">" +
                            "<h2 align=\"center\">RiSource Center Weekly Announcements</h2>" +
                            "<p style=\"font-size:14px;padding-left:8px;padding-right:8px;\">" +
                            "Rittal's RiSource Center weekly email update contains just a preview of the latest information posted there within the last 7 days. We encourage you to visit frequently to get the most out of your selling relationship with Rittal. Remember, the RiSource Center is updated almost daily, so there's no need to wait for your preview email in order to access all of the useful information and valuable tools that it has to offer.<br /><br />" +
                            "Take a look at the information summary below, then click the button to log on for more details or click on a headline that interests you and be transported immediately. Please note that you must be logged into the RiSource Center in order for the hyperlinks below to redirect you to the full sales communication." +
                            "</p>" +
                            "<hr noshade />" +
                            "<ul style=\"font - size:14px\">"
                            + list+
                           "</ul>" +
                            "<hr noshade />" +
                            "</div>"+
                             "<img src=\""+@host+ "/SupportTools/weeklynewsletter/?sent=" + DateTime.Today+"&usr="+usr_comp.userr.usr.usr_ID + "\">";              
                try
                {
                    string emailEnding = header + emailbody + footer;

                    if (mail.Count() != 0)
                    {
                        System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailEnding);
                        Email.IsBodyHtml = true;
                        System.Net.Mail.SmtpClient SMPTobj;
                        SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                        SMPTobj.EnableSsl = false;
                        SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                        SMPTobj.Send(Email);
                    }

                    //Log the action by the user
                    await locController.siteActionLog(0, "SalesComm", DateTime.Now, User.Identity.Name + " Weekly Email was sent ", "WeeklyEmail", 0);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} Error", ex);
                }
            }

            return View();
        }
        #endregion

        #region Email communications stack in stagging
        [AllowAnonymous]
        public ActionResult EmailStagging()
        {
            string From = "RiSource@rittal.us";
            string To = "";
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }

            string header = emailheader(host);
            string footer = emailfooter(host);
            string Subject = "RiSource Center- Communication stack in staging";
            string emailbody = "";
            //set the dates
            DateTime date = DateTime.Today;
            TimeSpan datediff = TimeSpan.Zero;
            DateTime subdate = Convert.ToDateTime("2016-11-19 00:00:00.000");

            //email approvers to approve communications if they forget to approve it
            var staggingComm = db.SalesCommViewModels.Join(
                db.UserViewModels,
                sales => sales.sc_owner,
                usr => usr.usr_ID,
                (sales, usr) => new { sales, usr })
                .Where(a => a.sales.sc_status == 1 && a.sales.submission_date > subdate);

            int count = staggingComm.Count();

            foreach (var item in staggingComm)
            {
                //find the date difference
                datediff = date - item.sales.submission_date;
                if (datediff.Days > 14)
                {
                    emailbody =header + "Dear "+ item.usr.usr_fName +" "+ item.usr.usr_lName + ", <br />"+
                        "You were trying to create a communication called <b> " + item.sales.sc_headline + "</b> with ID <b>"+ item.sales.scID + 
                        " </b> but you did not finish it and you left it in staging. <br /><br /> We will be sending you this notice until you move it from staging to active. Or deleted it."+
                        "<br /><br /> Thank you. <br /><br />" + footer;

                    //send the email out
                    To = item.usr.usr_email;
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailbody);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj;
                    SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);
                }
            }

            return View();
        }
        #endregion

        #region Email communications approvers if not approved
        [AllowAnonymous]
        public async Task<ActionResult> EmailApprovers()
        {
            //Run it daily
            string From = "RiSource@rittal.us";
            string To = "";
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }

            string header = emailheader(host);
            string footer = emailfooter(host);
            string Subject = "RiSource Center- Communication stack in staging";
            string emailbody = "";
            //set the dates
            DateTime date = DateTime.Today;
            TimeSpan datediff = TimeSpan.Zero;
            DateTime subdate = Convert.ToDateTime("2016-11-19 00:00:00.000");
            var get_user = dbEntity.usr_user;

            //collect sale communications with status as staging
            var emailCommApprovers = db.SalesCommViewModels.Join(
                db.Nav2ViewModel,
                sales => sales.n2ID,
                nav2 => nav2.n2ID,
                (sales, nav2) => new { sales, nav2 })
                .Where(a => a.sales.sc_status == 3 && a.sales.submission_date > subdate);
            //.Where(a => a.sales.sc_status == 1 && a.sales.submission_date > subdate);
            int count = emailCommApprovers.Count();

            foreach (var item in emailCommApprovers)
            {
                var get_ie_user = await get_user.Where(a => a.usr_ID == item.nav2.n2_IE_approver).FirstOrDefaultAsync();
                var get_it_user = await get_user.Where(a => a.usr_ID == item.nav2.n2_IT_approver).FirstOrDefaultAsync();
                //find the date difference
                datediff = date - item.sales.submission_date;

                //Email IE Approver
                if (datediff.Days > 3 && !string.IsNullOrEmpty(get_ie_user.usr_email))
                {
                    emailbody = header + "Dear " + get_ie_user.usr_fName + " " + get_ie_user.usr_lName + ", <br />" +
                        "An email was sent to you to approve the IE communication called <b> " + item.sales.sc_headline + "</b> with ID <b>" + item.sales.scID +
                        " </b> but may have forgotton to approve it. <br /><br /> We will be sending you this notice until you approve the communication or deleted it." +
                        "<br /><br /> Thank you. <br /><br />" + footer;

                    //send the email out
                    To = get_ie_user.usr_email;
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailbody);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj;
                    SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);
                }

                //Email IT Approver
                if (datediff.Days > 3 && !string.IsNullOrEmpty(get_it_user.usr_email))
                {
                    emailbody = header + "Dear " + get_it_user.usr_fName + " " + get_it_user.usr_lName + ", <br />" +
                        "An email was sent to you to approve the IT communication called <b> " + item.sales.sc_headline + "</b> with ID <b>" + item.sales.scID +
                        " </b> but may have forgotton to approve it. <br /><br /> We will be sending you this notice until you approve the communication or deleted it." +
                        "<br /><br /> Thank you. <br /><br />" + footer;

                    //send the email out
                    To = get_it_user.usr_email;
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailbody);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj;
                    SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);
                }
            }

            return View();
        }
        #endregion

    }
}
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
using System.Configuration;

namespace newrisourcecenter.Controllers
{
    public class LitRequestedController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: LitRequested
        public async Task<ActionResult> Index()
        {
            string statuss = "";
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<LitRequestedModel> litRequestedModel = new List<LitRequestedModel>();

            var list = await db.LitRequestedViewModels.Join(
                        db.UserViewModels,
                        rlitUser => rlitUser.usr_ID,
                        usr => usr.usr_ID,
                        (rlitUser, usr) => new { rlitUser, usr }
                    ).ToListAsync();

            if (User.IsInRole("Super Admin"))
            {
                foreach (var item in list.OrderByDescending(a => a.rlitUser.rlit_ID))
                {
                    if (item.rlitUser.status == 1)
                    {
                        statuss = "shipped";
                    }
                    else
                    {
                        statuss = "not Shipped";
                    }

                    litRequestedModel.Add(new LitRequestedModel { rlit_ID = item.rlitUser.rlit_ID, lit_name = item.rlitUser.rlit_info, userName = item.usr.usr_fName + " " + item.usr.usr_lName, date_created = item.rlitUser.date_created, status = statuss });
                }

                return View(litRequestedModel);
            }
            else
            {
                var usr = db.UserViewModels.Where(a => a.usr_email == User.Identity.Name).FirstOrDefault();//get the user ID and move it to the usr_user table
                var id = usr.usr_ID;
                //return View(await db.LitRequestedViewModels.Where(a=>a.usr_ID==id).ToListAsync());

                foreach (var item in list.Where(a => a.rlitUser.usr_ID == id).OrderByDescending(a => a.rlitUser.rlit_ID))
                {
                    if (item.rlitUser.status == 1)
                    {
                        statuss = "shipped";
                    }
                    else
                    {
                        statuss = "not Shipped";
                    }
                    litRequestedModel.Add(new LitRequestedModel { rlit_ID = item.rlitUser.rlit_ID, lit_name = item.rlitUser.rlit_info, userName = item.usr.usr_fName + " " + item.usr.usr_lName, date_created = item.rlitUser.date_created, status = statuss });
                }
                return View(litRequestedModel);
            }
        }

        // GET: LitRequested/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LitRequestedViewModel litRequestedViewModel = await db.LitRequestedViewModels.FindAsync(id);
            if (litRequestedViewModel == null)
            {
                return HttpNotFound();
            }

            return View(litRequestedViewModel);
        }

        // GET: LitRequested/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (User.IsInRole("Super Admin"))
            {
                var countlits = db.LitRequestedViewModels.ToList();
                ViewBag.countlits = countlits.Count();
            }
            else
            {
                var countlits = db.LitRequestedViewModels.Where(a => a.usr_ID == userId).ToList();
                ViewBag.countlits = countlits.Count();
            }

            LitRequestedViewModel LitRequestedViewModel = new LitRequestedViewModel();
            var litrq = db.LiteratureViewModels.ToList();
            List<LiteratureViewModel> lit_name = new List<LiteratureViewModel>();
            foreach (var item in litrq)//iterate the add function
            {
                //Add attachment list to the Edit page
                List<Nav1List> list_attachments = new List<Nav1List>();
                var arrayOfAttachments = item.risource;
                if (arrayOfAttachments != null)
                {
                    int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);

                    foreach (int risou in nums)
                    {
                        var risour = db.RiSourcesViewModels.Where(a => a.ris_ID == risou);
                        list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                    }
                }
                lit_name.Add(new LiteratureViewModel { lit_ID =item.lit_ID,lit_name=item.lit_name, list_attachments=list_attachments });//default value for select dropdown
            }
            LitRequestedViewModel.lit_name = lit_name;

            return View(LitRequestedViewModel);
        }

        // POST: LitRequested/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "rlit_ID,lit_ID,lit_quantity,usr_ID,date_created,status,rlit_info")] LitRequestedViewModel litRequestedViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                string[] rlit_data = { };

                if (Request.Form["rlit_info"] != null)
                {
                    rlit_data = Request.Form["rlit_info"].Split(',');
                }
                else
                {
                    return View(litRequestedViewModel);
                }
                foreach (var data in rlit_data)
                {
                    string[] rlit = data.Split('|');

                    int num = rlit.Count();
                    if (num != 2)
                    {
                        return RedirectToAction("Index", new { msg="You need to enter quantity when requesting a literature"});
                    }
                }

                string host = "";
                string list = "";
                string statuss = "";

                litRequestedViewModel.rlit_info = Request.Form["rlit_info"];
                db.LitRequestedViewModels.Add(litRequestedViewModel);
                await db.SaveChangesAsync();

                if (Request.Url.Port != 80)
                {
                    host = Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = Request.Url.Host + "/mexico";
                }
                //update status of the shipment
                if (litRequestedViewModel.status == 1)
                {
                    statuss = "shipped";
                }
                else
                {
                    statuss = "not Shipped";
                }
                var salcom = new SalesCommController();
                string footer = salcom.emailfooter(host);
                string header = salcom.emailheader(host);

                //Try to send an email
                var usr = db.UserViewModels.Where(a => a.usr_email == User.Identity.Name);
                string firstName = usr.FirstOrDefault().usr_fName;
                string lastName = usr.FirstOrDefault().usr_lName;
                string useremail = usr.FirstOrDefault().usr_email;

                list = RenderViewToString("EmailTemplate", litRequestedViewModel);

                string From = "webmaster@rittal.us";
                string To = "presswala.z@rittal.us";
                string Subject = "A new literature request";
                string emailEnding = header + "<div style=\"border:solid 1px black;\">" +
                                     "<h3>A new literature has been requested</h3>" +
                                     "<div><b>Name: </b>" + firstName + " " + lastName + " </div>" +
                                     "<div><b>Delivery Status</b>: " + statuss + " </div>" +
                                        list +
                                    "</div>" +
                                     footer;
                string ToUser = useremail;
                string SubjectUser = "RiSource Center Literature Request";
                string emailEndingUser = header + "<div style=\"border:solid 1px black;\">" +
                     "<h3>Your literature request has been submitted</h3>" +
                     "<div><b>Name: </b>" + firstName + " " + lastName + " </div>" +
                     "<div><b>Delivery Status</b>: " + statuss + " </div>" +
                        list +
                    "</div>" +
                     footer;

                db.Entry(litRequestedViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!User.IsInRole("Super Admin"))
                {
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailEnding);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);

                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);


                    System.Net.Mail.MailMessage EmailUser = new System.Net.Mail.MailMessage(From, ToUser, SubjectUser, emailEndingUser);
                    EmailUser.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobjUser = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);

                    SMPTobjUser.EnableSsl = false;
                    SMPTobjUser.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobjUser.Send(EmailUser);
                }          

                return RedirectToAction("Index", new { n1_name="My Requested Literature"});
            }
            return View(litRequestedViewModel);
        }

        public ActionResult EmailTemplate(object litRequestedViewModel)
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

        // GET: LitRequested/Edit/5
        public async Task<ActionResult> Edit(int id=0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id==0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            LitRequestedViewModel litRequestedViewModel = await db.LitRequestedViewModels.FindAsync(id);
            if (litRequestedViewModel == null)
            {
                return HttpNotFound();
            }

            List<AddedLits> lit_list = new List<AddedLits>();
            string[] rlit_data = { };
            if (litRequestedViewModel.rlit_info != null)
            {
                rlit_data = litRequestedViewModel.rlit_info.Split(',');
            }
            foreach (var data in rlit_data)
            {
                string[] rlit = data.Split('|');
                lit_list.Add(new AddedLits { lit_list = rlit[0] });
            }

            var litrq = db.LiteratureViewModels.ToList();
            List<LiteratureViewModel> lits = new List<LiteratureViewModel>();
            foreach (var item in litrq)//iterate the add function
            {
                lits.Add(new LiteratureViewModel { lit_ID = item.lit_ID, lit_name = item.lit_name, risource = item.risource });//default value for select dropdown
            }

            litRequestedViewModel.lit_name = lits;
            litRequestedViewModel.added_lits = lit_list;

            return View(litRequestedViewModel);
        }

        // POST: LitRequested/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "rlit_ID,lit_ID,lit_quantity,usr_ID,date_created,status,rlit_info")] LitRequestedViewModel litRequestedViewModel)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                string[] rlit_data = { };

                if (Request.Form["rlit_info"] != null)
                {
                    rlit_data = Request.Form["rlit_info"].Split(',');
                }
                else
                {
                    return View(litRequestedViewModel);
                }
                foreach (var data in rlit_data)
                {
                    string[] rlit = data.Split('|');

                    int num = rlit.Count();
                    if (num != 2)
                    {
                        return RedirectToAction("Index", new { msg = "You need to enter quantity when requesting a literature" });
                    }
                }

                string list = "";
                string host = "";
                string statuss = "";
                if (Request.Url.Port != 80)
                {
                    host = Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = Request.Url.Host + "/mexico";
                }
                //update status of the shipment
                if (litRequestedViewModel.status == 1)
                {
                    statuss = "shipped";
                }
                else
                {
                    statuss = "not Shipped";
                }
                var salcom = new SalesCommController();
                string footer = salcom.emailfooter(host);
                string header = salcom.emailheader(host);

                //Try to send an email
                var usr = db.UserViewModels.Where(a=>a.usr_email == User.Identity.Name);
                string firstName = usr.FirstOrDefault().usr_fName;
                string lastName = usr.FirstOrDefault().usr_lName;

                litRequestedViewModel.rlit_info = Request.Form["rlit_info"];

                list = RenderViewToString("EmailTemplate", litRequestedViewModel);

                string From = "webmaster@rittal.us";
                string To = "presswala.z@rittal.us";
                string Subject = "Literature has been updated by";
                string emailEnding = header + "<div style=\"border:solid 1px black;\">" +
                                     "<h3>The requested literature has been updated</h3>" +
                                     "<div><b>Name: </b>" + firstName  +" "+ lastName + " </div>"+
                                     "<div><b>Delivery Status</b>: " +statuss+" </div>" +
                                        list +
                                    "</div>" +
                                     footer;

                db.Entry(litRequestedViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (!User.IsInRole("Super Admin"))
                {
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, emailEnding);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj = new System.Net.Mail.SmtpClient("10.38.0.114");

                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);
                }
                else
                {
                    int usr_ID = int.Parse(Request.Form["usr_ID"]);
                    var getUsrById = db.UserViewModels.Where(a => a.usr_ID==usr_ID);
                    string ToUser = getUsrById.FirstOrDefault().usr_email;
                    string SubjectUser = "Literature has been Shipped";
                    string emailEndingUser = header + "<div style=\"border:solid 1px black;\">" +
                                         "<h3>Your requested literature has been shipped</h3>" +
                                         "<div><b>Name: </b>" + firstName + " " + lastName + " </div>" +
                                         "<div><b>Delivery Status</b>: " + statuss + " </div>" +
                                            list +
                                        "</div>" +
                                         footer;

                    if (litRequestedViewModel.status == 1)
                    {
                        System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, ToUser, SubjectUser, emailEndingUser);
                        Email.IsBodyHtml = true;
                        System.Net.Mail.SmtpClient SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);

                        SMPTobj.EnableSsl = false;
                        SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                        SMPTobj.Send(Email);
                    }
                }

                return RedirectToAction("Index", new { n1_name=Request.QueryString["n1_name"]});
            }
            return View(litRequestedViewModel);
        }

        // GET: LitRequested/Delete/5
        public async Task<ActionResult> Delete(int id=0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LitRequestedViewModel litRequestedViewModel = await db.LitRequestedViewModels.FindAsync(id);
            if (litRequestedViewModel == null)
            {
                return HttpNotFound();
            }
            db.LitRequestedViewModels.Remove(litRequestedViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
            //return View(litRequestedViewModel);
        }

        // POST: LitRequested/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id=0)
        {
            LitRequestedViewModel litRequestedViewModel = await db.LitRequestedViewModels.FindAsync(id);
            db.LitRequestedViewModels.Remove(litRequestedViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

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

using newrisourcecenter.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    public class PMQuoteController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        // GET: PMQuote
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.fileUrl = "";
            if(Session["PMQuote_file_url"] != null)
            {
                ViewBag.fileUrl = Session["PMQuote_file_url"];
                Session.Remove("PMQuote_file_url");
            }
            List<PMQuoteViewModel> quotes = await db.PMQuoteViewModels.Where(a => a.submitted_by == userId).OrderByDescending(a => a.ID).ToListAsync();
            return View(quotes);
        }

        // GET: PMQuote/Admin
        public async Task<ActionResult> Admin(int next = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.fileUrl = "";
            if (Session["PMQuote_file_url"] != null)
            {
                ViewBag.fileUrl = Session["PMQuote_file_url"];
                Session.Remove("PMQuote_file_url");
            }
            IQueryable<PMQuoteViewModel> quoteData;
            if (next > 0)
            {
                quoteData = db.PMQuoteViewModels.Where(a => a.ID < next).OrderByDescending(a => a.ID);
            }
            else
            {
                quoteData = db.PMQuoteViewModels.OrderByDescending(a => a.ID);
            }
            List<PMQuoteViewModel> quotes = await quoteData.Take(100).ToListAsync();
            int lastQuoteID = quotes.Count() > 0 ? quotes.LastOrDefault().ID : 0;
            if (lastQuoteID > 0)
            {
                int quotesCount = db.PMQuoteViewModels.Count(x => x.ID < lastQuoteID);
                if (quotesCount > 0)
                {
                    ViewBag.lastQuoteID = lastQuoteID;
                }
            }
            else
            {
                ViewBag.lastQuoteID = 0;
            }
            return View(quotes);
        }

        // GET: PMQuote/Create
        public ActionResult Create()
        {
            PMQuoteViewModel quoteViewModel = new PMQuoteViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == userId).FirstOrDefault();
            if (Request.QueryString["form_id"] != null)
            {
                quoteViewModel = db.PMQuoteViewModels.Where(a => a.ID == form_id).FirstOrDefault();
            }
            return View(quoteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Create(PMQuoteViewModel quote)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://www.rittal.com/us_en/apps/pm_quote/");
                    quote.quote_number = await db.PMQuoteViewModels.MaxAsync(x => x.ID) + 1;
                    quote.generated_by = Session["firstName"] + " " + Session["lastName"];
                    HttpResponseMessage response = await client.PostAsJsonAsync("index.php", quote);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        dynamic responseObj = JsonConvert.DeserializeObject<dynamic>(content);
                        if(responseObj != null && (bool)responseObj.success)
                        {
                            quote.submitted_by = Convert.ToInt32(Session["userId"]);
                            quote.submitted_on = DateTime.Now;
                            quote.file_url = responseObj.file;
                            quote.enviromental_conditions = quote.conditions != null && quote.conditions.Length > 0 ? string.Join(",", quote.conditions) : "";
                            db.PMQuoteViewModels.Add(quote);
                            await db.SaveChangesAsync();
                            Session["PMQuote_file_url"] = quote.file_url;
                            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The Quote has been submitted" });
                        }                        
                    }
                }
            }
            return View(quote);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PMQuoteViewModel viewModel = await db.PMQuoteViewModels.FindAsync(id);

            if (viewModel == null)
            {
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], error = "Unable to delete Quote." });
            }

            db.PMQuoteViewModels.Remove(viewModel);

            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The Quote has been deleted" });
        }
    }
}
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
using Newtonsoft.Json;

namespace newrisourcecenter.Controllers
{
    public class ITSalesController : Controller
    {
        private RittalUSAPIEntities db = new RittalUSAPIEntities();
        private RisourceCenterMexicoEntities rittalDB = new RisourceCenterMexicoEntities();

        #region Get All IT Sales
        public async Task<String> getITSales()
        {
            try
            {
                string restunedString = "";

                List<mrk_sales> new_tble = new List<mrk_sales>();
                var tbl = db.tbl_MRK_Zipcode.Select(m => new { m.V1Name, m.V1Email }).Distinct();
                foreach (var item in tbl.ToList())
                {
                    if (!item.V1Email.Contains('_'))
                    {
                        if (!item.V1Name.Contains('+') && !item.V1Name.Contains("Unassigned Outside Sales"))
                        {
                            new_tble.Add(new mrk_sales { name = item.V1Name, email = item.V1Email });
                        }
                    }
                }

                restunedString = JsonConvert.SerializeObject(new { tbl = new_tble, status = "OK" });

                return restunedString;

            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "Upload failed";
            }
        }
        #endregion

        #region Get All IT Sales
        public async Task<String> GetITSalesQuerystring(string querystring = null)
        {
            try
            {
                string restunedString = "";

                List<mrk_sales> new_tble = new List<mrk_sales>();
                var tbl = db.tbl_MRK_Zipcode.Where(a => a.PostalCode == querystring).Select(m => new { m.V1Name, m.V1Email }).Distinct();
                foreach (var item in tbl.ToList())
                {
                    if (!item.V1Email.Contains('_'))
                    {
                        if (!item.V1Name.Contains('+') && !item.V1Name.Contains("Unassigned Outside Sales"))
                        {
                            new_tble.Add(new mrk_sales { name = item.V1Name, email = item.V1Email });
                        }
                        else
                        {
                            new_tble.Add(new mrk_sales { name = "CustomerService", email = "CustomerService@Rittal.US" });
                        }
                    }
                }

                restunedString = JsonConvert.SerializeObject(new { tbl = new_tble, status = "OK" });

                return restunedString;

            }
            catch (Exception ex)
            {

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "Upload failed";
            }
        }

        [HttpPost]
        public string Getitsalestring(string querystring = null,string token=null)
        {
            try
            {
                if (token==null || token=="")
                {
                    return "";
                }
                string restunedString = "";

                List<mrk_sales> new_tble = new List<mrk_sales>();
                List<string> emails = new List<string>();  
                string[] listV1 = { "US09166", "US09176", "US09174", "US09164", "US09170", "US09172", "US09168" };
                List<string> groups = new List<string>() { "1", "3" };
                var tbls = db.tbl_MRK_Zipcode.Where(a => a.PostalCode == querystring && groups.Contains(a.CGrp)).OrderBy(x => x.CGrp);
                var items = tbls.ToList();
                
                foreach (var item in items)
                {
                    if (!item.V1Email.Contains('_') && !item.V1Name.Contains("+++") && !item.V1Name.Contains("Unassigned Outside Sales") && !item.V1Name.Contains("Team"))
                    {
                        if (listV1.Contains(item.V1))
                        {
                            if(!emails.Contains(item.V6Email))
                            {
                                new_tble.Add(new mrk_sales { name = item.V6Name, email = item.V6Email, phone = item.V6Phone, zip = item.PostalCode, group = item.CGrp });
                                emails.Add(item.V6Email);
                            }                            
                            }
                            else
                            {
                            if (!emails.Contains(item.V1Email))
                            {
                                new_tble.Add(new mrk_sales { name = item.V1Name, email = item.V1Email, phone = item.V1Phone, zip = item.PostalCode, group = item.CGrp });
                                emails.Add(item.V1Email);
                            }
                        }
                    }
                }

                if(new_tble.Count() == 0)
                {
                    new_tble.Add(new mrk_sales { name = "Customer Service", email = "customerservice@rittal.us", phone = "800-477-4000" });
                }
                else
                {
                    foreach(string group in groups)
                    {
                        if(new_tble.Where(x => x.group == group).Count() == 0)
                            new_tble.Add(new mrk_sales { name = "Customer Service", email = "customerservice@rittal.us", phone = "800-477-4000", group = group });
                    }
                }

                restunedString = JsonConvert.SerializeObject(new { tbl = new_tble.Select(m=> new { m.name,m.phone,m.email,m.group}).Distinct(), status = "OK" });

                return restunedString;

            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "Upload failed";
            }
        }
        #endregion

        public async Task<String> getRCMContacts(string querystring)
        {
            try
            {
                List<mrk_sales> new_tble = new List<mrk_sales>();
                var items = rittalDB.RCMContacts.Where(a => a.zipcode == querystring).ToList();
                foreach (var item in items)
                {
                    new_tble.Add(new mrk_sales { name = (item.first_name + " " + item.last_name).Trim(), email = item.email });
                }
                return JsonConvert.SerializeObject(new { contacts = new_tble, status = "OK" });
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(new { error = "An error occurred while processing your request, please try again", status = "OK" });
            }
        }

        public async Task<String> getTMContacts(string querystring)
        {
            try
            {
                List<string> emails = new List<string>();
                string[] listV1 = { "US09166", "US09176", "US09174", "US09164", "US09170", "US09172", "US09168" };
                List<mrk_sales> new_tble = new List<mrk_sales>();
                var items = db.tbl_MRK_Zipcode.Where(a => a.PostalCode == querystring.Trim() && a.CGrp == "3").ToList();
                foreach (var item in items)
                {
                    if (!item.V1Email.Contains('_') && !item.V1Name.Contains("+++") && !item.V1Name.Contains("Unassigned Outside Sales") && !item.V1Name.Contains("Team"))
                    {
                        if (listV1.Contains(item.V1))
                        {
                            if (!emails.Contains(item.V6Email))
                            {
                                new_tble.Add(new mrk_sales { name = item.V6Name, email = item.V6Email, phone = item.V6Phone });
                                emails.Add(item.V6Email);
                            }
                        }
                        else
                        {
                            if (!emails.Contains(item.V1Email))
                            {
                                new_tble.Add(new mrk_sales { name = item.V1Name, email = item.V1Email, phone = item.V1Phone });
                                emails.Add(item.V1Email);
                            }
                        }
                    }
                }
                return JsonConvert.SerializeObject(new { contacts = new_tble, status = "OK" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = "An error occurred while processing your request, please try again", status = "OK" });
            }
        }
        // GET: ITSales
        /**public async Task<ActionResult> Index()
        {
            return View(await db.tbl_MRK_Zipcode.Take(10).ToListAsync());
        }

        // GET: ITSales/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_MRK_Zipcode tbl_MRK_Zipcode = await db.tbl_MRK_Zipcode.FindAsync(id);
            if (tbl_MRK_Zipcode == null)
            {
                return HttpNotFound();
            }
            return View(tbl_MRK_Zipcode);
        }

        // GET: ITSales/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ITSales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "id,PostalCode,CGrp,V1,V1Name,V1Email,V1Phone")] tbl_MRK_Zipcode tbl_MRK_Zipcode)
        {
            if (ModelState.IsValid)
            {
                db.tbl_MRK_Zipcode.Add(tbl_MRK_Zipcode);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tbl_MRK_Zipcode);
        }

        // GET: ITSales/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_MRK_Zipcode tbl_MRK_Zipcode = await db.tbl_MRK_Zipcode.FindAsync(id);
            if (tbl_MRK_Zipcode == null)
            {
                return HttpNotFound();
            }
            return View(tbl_MRK_Zipcode);
        }

        // POST: ITSales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,PostalCode,CGrp,V1,V1Name,V1Email,V1Phone")] tbl_MRK_Zipcode tbl_MRK_Zipcode)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_MRK_Zipcode).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tbl_MRK_Zipcode);
        }

        // GET: ITSales/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_MRK_Zipcode tbl_MRK_Zipcode = await db.tbl_MRK_Zipcode.FindAsync(id);
            if (tbl_MRK_Zipcode == null)
            {
                return HttpNotFound();
            }
            return View(tbl_MRK_Zipcode);
        }

        // POST: ITSales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            tbl_MRK_Zipcode tbl_MRK_Zipcode = await db.tbl_MRK_Zipcode.FindAsync(id);
            db.tbl_MRK_Zipcode.Remove(tbl_MRK_Zipcode);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    **/
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

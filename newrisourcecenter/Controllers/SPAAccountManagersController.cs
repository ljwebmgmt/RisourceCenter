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
using System.Text;
using System.IO;

namespace newrisourcecenter.Controllers
{
    public class SPAAccountManagersController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: SPAAccountManagers
        public async Task<ActionResult> Index()
        {
            return View(await db.SPAAccountManagers.ToListAsync());
        }

        // GET: SPAAccountManagers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAAccountManager sPAAccountManager = await db.SPAAccountManagers.FindAsync(id);
            if (sPAAccountManager == null)
            {
                return HttpNotFound();
            }
            return View(sPAAccountManager);
        }

        // GET: SPAAccountManagers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SPAAccountManagers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,contact_name,contact_type,title,zip,email,territory_code")] SPAAccountManager sPAAccountManager)
        {
            if (ModelState.IsValid)
            {
                db.SPAAccountManagers.Add(sPAAccountManager);
                await db.SaveChangesAsync();

                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "Your item has been added" });
            }

            return View(sPAAccountManager);
        }

        // GET: SPAAccountManagers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAAccountManager sPAAccountManager = await db.SPAAccountManagers.FindAsync(id);
            if (sPAAccountManager == null)
            {
                return HttpNotFound();
            }
            return View(sPAAccountManager);
        }

        // POST: SPAAccountManagers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,contact_name,contact_type,title,zip,email,territory_code")] SPAAccountManager sPAAccountManager)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sPAAccountManager).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "Your item has been updated" });
            }
            return View(sPAAccountManager);
        }

        [HttpPost]
        public ActionResult UploadSKUdata(HttpPostedFileBase attachment)
        {
            //attach a file to the risources
            if (attachment != null && attachment.ContentLength > 0)
            {
                var fileName = Path.GetFileName(attachment.FileName);
                var path = Path.Combine(Server.MapPath("~/attachments/spa/files"), fileName);
                string ext = Path.GetExtension(fileName);
                attachment.SaveAs(path);
                if (ext == ".csv")
                {
                    var reader = new StreamReader(path);
                    if (System.IO.File.Exists(path))
                    {
                        //Parse Open Ended file
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var columns = line.Split(',');
                            if (columns[0] != "Contact Name")
                            {
                                string contact_name = columns[0];
                                string contact_type = columns[1];
                                string title = columns[2];
                                string zip = columns[3];
                                string email = columns[4];
                                string territory_code = columns[5];
                                if (contact_name != "")
                                {
                                    var dup_sales_rep = db.SPAAccountManagers.Where(a => a.zip==zip && a.email==email);
                                    if (dup_sales_rep.Count() != 0)
                                    {
                                        SPAAccountManager sales_rep_new = dup_sales_rep.FirstOrDefault();
                                        //if id exists update
                                        sales_rep_new.contact_name = contact_name;
                                        sales_rep_new.contact_type = contact_type;
                                        sales_rep_new.title = title;
                                        sales_rep_new.zip = zip;
                                        sales_rep_new.email = email;
                                        sales_rep_new.territory_code = territory_code;

                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        //if id does not exist insert
                                        SPAAccountManager sales_rep_new = new SPAAccountManager();
                                        sales_rep_new.contact_name = contact_name;
                                        sales_rep_new.contact_type = contact_type;
                                        sales_rep_new.title = title;
                                        sales_rep_new.zip = zip;
                                        sales_rep_new.email = email;
                                        sales_rep_new.territory_code = territory_code;

                                        db.SPAAccountManagers.Add(sales_rep_new);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], success = "You can only upload CSV file" });
                }
            }

            return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], success = "The file has been uploaded" });
        }

        [HttpGet]
        public async Task<ActionResult> ExportSPASalesReps()
        {
            List<SPAAccountManager> spa_salesreps = await db.SPAAccountManagers.ToListAsync();
            StringBuilder dataSource = new StringBuilder();
            dataSource.AppendLine("Contact Name, Contact Type, Title, Zip, Email, Territory Code");
            foreach (var spa_salesrep in spa_salesreps)
            {
                dataSource.AppendLine("" + spa_salesrep.contact_name + "," + spa_salesrep.contact_type + "," + spa_salesrep.title + "," + spa_salesrep.zip + "," + spa_salesrep.email + "," + spa_salesrep.territory_code + "");
            }

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachement;filename=spa_sales_rep.csv");
            Response.ContentType = "text/csv";
            Response.Output.Write(dataSource);
            Response.Flush();
            Response.End();

            return View();
        }

        // GET: Skus/Delete/5
        public async Task<ActionResult> Deletefiles()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<SPAAccountManager> sPAAccountManager = await db.SPAAccountManagers.ToListAsync();
            if (sPAAccountManager == null)
            {
                return HttpNotFound();
            }
            db.SPAAccountManagers.RemoveRange(sPAAccountManager);
            db.SaveChanges();

            //Log the action by the user
            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "All the items have been deleted" });
        }

        // GET: SPAAccountManagers/Delete/5
        public async Task<ActionResult> Delete(int? id)
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
            SPAAccountManager sPAAccountManager = await db.SPAAccountManagers.FindAsync(id);
            if (sPAAccountManager == null)
            {
                return HttpNotFound();
            }
            db.SPAAccountManagers.Remove(sPAAccountManager);
            await db.SaveChangesAsync();

            //Log the action by the user
            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "Your item has been deleted" });
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

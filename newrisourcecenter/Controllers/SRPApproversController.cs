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

namespace newrisourcecenter.Controllers
{
    public class SRPApproversController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private CommonController comm = new CommonController();

        // GET: SRPApprovers
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var data = await db.salesRequestApprovers.ToListAsync();
            List<salesRequestApprovers> req_data = new List<salesRequestApprovers>();
            foreach (var req in data)
            {
                string department = "";
                if (req.Department=="-2")
                {
                    department = "Accounts Payables";
                }
                else if (req.Department == "-3")
                {
                    department = "Buyer/Procurement";
                }
                else
                {
                    int department_position = Convert.ToInt32(req.Department);
                    department = FuncCommonSRP().department[department_position].Text;
                }
                int status_position = Convert.ToInt32(req.Status);

                var approver = await comm.GetfullName(Convert.ToInt32(req.UserID));
                req_data.Add(new salesRequestApprovers {
                    ID=req.ID,
                    UserID = req.UserID,
                    fullName = approver["fullName"],
                    RegionName = req.RegionName,
                    Status = req.Status,
                    StatusName = FuncCommonSRP().requestType[status_position].Text,
                    Department = department,
                });
            }

            return View(req_data);
        }

        // GET: SRPApprovers/Details/5
        public async Task<ActionResult> Details(int? id)
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
            salesRequestApprovers salesRequestApprovers = await db.salesRequestApprovers.FindAsync(id);
            if (salesRequestApprovers == null)
            {
                return HttpNotFound();
            }
            return View(salesRequestApprovers);
        }

        // GET: SRPApprovers/Create
        public ActionResult Create()
        {
            salesRequestApprovers model = new salesRequestApprovers();
            IQueryable<UserViewModel>  usr_data = db.UserViewModels;
            List<SelectListItem> list_usrs = new List<SelectListItem>();
            list_usrs.Add(new SelectListItem { Text="Select A User", Value = "1", Selected=true });
            foreach (var usrs in usr_data)
            {
                list_usrs.Add(new SelectListItem { Text = usrs.usr_fName + " " + usrs.usr_lName, Value = usrs.usr_ID.ToString() });
            }
            //List departments
            //List<SelectListItem> department = new List<SelectListItem>();
            model.list_departments = FuncCommonSRP().department.OrderBy(a => a.Text);
            model.list_regions = FuncCommonSRP().regionalManager.OrderBy(a=>a.Text);
            model.list_status = FuncCommonSRP().requestType.OrderBy(a => a.Text);
            model.list_users = list_usrs.OrderBy(a=>a.Text);

            return View(model);
        }

        // POST: SRPApprovers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,UserID,RegionName,Status,Department")] salesRequestApprovers salesRequestApprovers)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.salesRequestApprovers.Add(salesRequestApprovers);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = "Sales Request Approvers" });
            }

            return View(salesRequestApprovers);
        }

        // GET: SRPApprovers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            salesRequestApprovers salesRequestApprovers = await db.salesRequestApprovers.FindAsync(id);
            if (salesRequestApprovers == null)
            {
                return HttpNotFound();
            }
            IQueryable<UserViewModel> usr_data = db.UserViewModels;
            List<SelectListItem> list_usrs = new List<SelectListItem>();
            list_usrs.Add(new SelectListItem { Text = "Select A User", Value = "1", Selected = true });
            foreach (var usrs in usr_data)
            {
                list_usrs.Add(new SelectListItem { Text = usrs.usr_fName + " " + usrs.usr_lName, Value = usrs.usr_ID.ToString() });
            }
            salesRequestApprovers.list_users = list_usrs.OrderBy(a=>a.Text);
            salesRequestApprovers.list_departments = FuncCommonSRP().department.OrderBy(a => a.Text);
            salesRequestApprovers.list_regions = FuncCommonSRP().regionalManager.OrderBy(a => a.Text);
            salesRequestApprovers.list_status = FuncCommonSRP().requestType.OrderBy(a => a.Text);

            return View(salesRequestApprovers);
        }

        // POST: SRPApprovers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,UserID,RegionName,Status,Department")] salesRequestApprovers salesRequestApprovers)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                db.Entry(salesRequestApprovers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { n1_name = "Sales Request Approvers" });
            }
            return View(salesRequestApprovers);
        }

        private SRPCommon FuncCommonSRP()
        {
            //List regions
            List<SelectListItem> regions = new List<SelectListItem>();
            regions.Add(new SelectListItem { Text = "NorthEast Sales", Value = "NorthEast Sales" });
            regions.Add(new SelectListItem { Text = "SouthEast Sales", Value = "SouthEast Sales" });
            regions.Add(new SelectListItem { Text = "South Sales", Value = "South Sales" });
            regions.Add(new SelectListItem { Text = "West Sales", Value = "West Sales" });
            regions.Add(new SelectListItem { Text = "North Sales", Value = "North Sales" });
            regions.Add(new SelectListItem { Text = "Central Sales", Value = "Central Sales" });
            regions.Add(new SelectListItem { Text = "Accounts Payables", Value = "Accounts Payables" });
            regions.Add(new SelectListItem { Text = "Buyer/Procurement", Value = "Buyer/Procurement" });
            regions.Add(new SelectListItem { Text = "Channel", Value = "Channel" });
            regions.Add(new SelectListItem { Text = "IT Sales", Value = "IT Sales" });

            //List departments
            List<SelectListItem> departments = new List<SelectListItem>();
            departments.Add(new SelectListItem { Text = "IE Sales", Value = "0" });
            departments.Add(new SelectListItem { Text = "IT Sales", Value = "1" });
            departments.Add(new SelectListItem { Text = "Channel", Value = "2" });
            departments.Add(new SelectListItem { Text = "Accounts Payables", Value = "-2" });
            departments.Add(new SelectListItem { Text = "Buyer/Procurement", Value = "-3" });

            //List status
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem { Text = "Deactivate", Value = "0" });
            status.Add(new SelectListItem { Text = "Active", Value = "1", Selected=true });

            SRPCommon srp_common = new SRPCommon
            {
                regionalManager = regions,
                department = departments,
                requestType = status
            };

            return srp_common;
        }

        // GET: SRPApprovers/Delete/5
        public async Task<ActionResult> Delete(int? id)
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
            salesRequestApprovers salesRequestApprovers = await db.salesRequestApprovers.FindAsync(id);
            if (salesRequestApprovers == null)
            {
                return HttpNotFound();
            }
            return View(salesRequestApprovers);
        }

        // POST: SRPApprovers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            salesRequestApprovers salesRequestApprovers = await db.salesRequestApprovers.FindAsync(id);
            db.salesRequestApprovers.Remove(salesRequestApprovers);
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

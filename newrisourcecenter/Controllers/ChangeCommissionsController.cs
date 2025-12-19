using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using newrisourcecenter.Models;
using newrisourcecenter.ViewModels;

namespace newrisourcecenter.Controllers
{
    [Authorize(Roles = "Rittal User")]
    public class ChangeCommissionsController : Controller
    {
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
        private string html = @"Hello {1},
<br>
<br>
 You have received a CCR from the RiSource Center that requires your review. Please log into the RiSource Center prior to clicking the link and you will be taken directly to the CCR Portal to approve or deny. Please respond within 72 hours of receipt of this email. Send any questions you have regarding this CCR to RittalBI@rittal.us.
<br>
<br>
<br>
<a href=" + "{0}" + @">Click here after logging into RiSource Center </a>
<br>
<br>
<br>
With kind regards,
<br>
Rittal North America LLC • 425 N. Martingale Rd. Suite 400 • Schaumburg, IL 
60173
Tel: (800) 477-4000 • Fax: (800) 477-4003 
Email: customerservice@rittal.us • www.rittal.us • www.friedhelm-loh-group.com
<br>
Rittal – The System. 
Faster – better – everywhere.
";


        private string AdminHtml = @"
Hello {1},
<br>
<br>
Request #{2} has been {3}.
<br>
<br>
<a href=" + "{0}" + @">Please login to RiSource Center to review and Approve.</a>
<br>
<br>
<br>
With kind regards, 
Rittal North America LLC • 425 N. Martingale Rd. Suite 400 • Schaumburg, IL 60173 Tel: (800) 477-4000 • Fax: (800) 477-4003 Email: customerservice@rittal.us • www.rittal.us • www.friedhelm-loh-group.com 
Rittal – The System. Faster – better – everywhere.
";

        string[] roleTypes = new string[] { "Admin", "Manager", "User" };

        // GET: ChangeCommissions

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            string level2Manager = ConfigurationManager.AppSettings["Level2Manager"];
            List<int> level1UserIds = new List<int>(), level2UserIds = new List<int>();
            bool IsLevel2Users;

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            foreach (var item in level2Manager.Split(','))
            {
                level2UserIds.Add(Convert.ToInt32(item));
            }
            IsLevel2Users = level2UserIds.Contains(userId);
            ViewBag.IsLevel2Users = IsLevel2Users;

            List<ChangeCommissionViewModel> changecommissionList = db.ChangeCommissions.Select(x => new ChangeCommissionViewModel
            {
                ID = x.ID,
                number = x.number,
                credit_sale = x.credit_sale,
                to_representive = x.to_representive,
                from_representive = x.from_representive,
                sales_doc_number = x.sales_doc_number,
                po_number = x.po_number,
                invoice_date = x.invoice_date,
                assigned_rep = x.assigned_rep,
                request_manager = x.request_manager,
                assigned_manager = x.assigned_manager,
                request_representive = x.request_representive,
                amount = x.amount,
                initialApproval = x.initialApproval,
                FinalApproval = x.FinalApproval,
                created_by = x.created_by
            }).Where(x => x.created_by == userId |  (int)x.assigned_rep == userId | (int)x.request_manager == userId | (int)x.assigned_manager == userId | level2UserIds.Contains(userId)).ToList();


            int statusId = Convert.ToInt32(getPreApproval(-1).Single(y => y.Text == "Denied").Value);

            changecommissionList.ForEach(x =>
            {
                x.roles = new ChangeCommissionRoles();
                level1UserIds = new List<int>() { (int)x.assigned_rep, (int)x.request_manager, (int)x.assigned_manager };
                x.roles.IsViewable = GetRoleIsViewable(x, level1UserIds, level2UserIds, userId);
                var usr = db.usr_user.Where(u => u.usr_ID == x.created_by).FirstOrDefault();
                x.username = usr.usr_fName + " " + usr.usr_lName;

                //      bool IsLevel2Eligible;
                //      List<int> userIds = db.ChangeCommissionHistories.Where(y => y.ChangeCommissionId == x.ID
                //&& level1UserIds.Contains(y.created_by)).Select(y => y.created_by).Distinct().ToList();

                //      IsLevel2Eligible = userIds.Count == level1UserIds.Count;

                //      //if FinalApproval is pending and log in user is assigned rep or requestmanager or assigned manager 
                //      if (x.FinalApproval == "Pending" && (
                //      (x.assigned_rep == userId || x.request_manager == userId || x.assigned_manager == userId)
                //      || (x.created_by == userId && userIds.Count == 0) || IsLevel2Users))
                //      {
                //          x.roles = new ChangeCommissionRoles();
                //          x.roles.IsEditable = GetRoleIsEditable(x, level2UserIds, userId);
                //      }
                //      else
                //      {
                //          x.roles = new ChangeCommissionRoles();
                //          x.roles.IsEditable = false;
                //      }
            });

            return View(changecommissionList);
        }

        // GET: ChangeCommissions/Details/5

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeCommission changeCommission = db.ChangeCommissions.Find(id);
            if (changeCommission == null)
            {
                return HttpNotFound();
            }
            return View(changeCommission);
        }

        // GET: ChangeCommissions/Create

        public ActionResult Create()
        {

            long userId = Convert.ToInt64(Session["userId"]);
            //ChangeCommission changeCommission = new ChangeCommission();
            //CommissionAdmin commissionAdmin = db.CommissionAdmins.FirstOrDefault();
            //List<SelectListItem> FromRep = new List<SelectListItem>(), ToRep = new List<SelectListItem>(),Reps=new List<SelectListItem>();


            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }


            ViewBag.qty = new List<string>() { "" };
            ViewBag.part_number = new List<string>() { "" };
            GetDataForUser(userId);

            return View();
        }

        public void GetDataForUser(long userId)
        {
            ChangeCommission changeCommission = new ChangeCommission();
            CommissionAdmin commissionAdmin = db.CommissionAdmins.FirstOrDefault();
            List<SelectListItem> Req_Mang = new List<SelectListItem>(), Assigned_Manag = new List<SelectListItem>(), Reps = new List<SelectListItem>();
            if (commissionAdmin != null)
            {
                int[] Req_ManagerIds = commissionAdmin.Req_Manager.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                int[] Assigned_ManagerIds = commissionAdmin.Assigned_Manager.Split(',').Select(x => Convert.ToInt32(x)).ToArray();

                Req_Mang = db.usr_user.Where(x => Req_ManagerIds.Contains(x.usr_ID)).Select(x =>
                   new SelectListItem
                   {
                       Text = x.usr_fName + " " + x.usr_lName + "[" + x.usr_email + "]",
                       Value = x.usr_ID.ToString()

                   }).ToList();

                Assigned_Manag = db.usr_user.Where(x => Assigned_ManagerIds.Contains(x.usr_ID)).Select(x =>
                   new SelectListItem
                   {
                       Text = x.usr_fName + " " + x.usr_lName + "[" + x.usr_email + "]",
                       Value = x.usr_ID.ToString()

                   }).ToList();
            }

            Reps = db.usr_user.Where(x => x.usr_email.Contains("@rittal.us")).Select(x => new SelectListItem
            {
                Text = x.usr_fName + " " + x.usr_lName + "[" + x.usr_email + "]",
                Value = x.usr_ID.ToString()

            }).ToList();

            ViewBag.Req_Mang = Req_Mang;
            ViewBag.Ass_Manag = Assigned_Manag;
            ViewBag.Reps = Reps;

            usr_user usr_User = db.usr_user.Single(x => x.usr_ID == userId);
            ViewBag.creditOptions = new SelectList(FillcreditOptions(), "Value", "Text"); ;

            ViewBag.from_representive = usr_User.usr_fName + " " + usr_User.usr_lName;


        }

        // POST: ChangeCommissions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Create(ChangeCommission changeCommission, List<string> qty, List<string> part_number)
        {
            //string level1Manager = ConfigurationManager.AppSettings["Level1Manager"];
            long userId = Convert.ToInt64(Session["userId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                Random random = new Random();
                changeCommission.number = "CH-" + random.Next().ToString();
                changeCommission.FinalApproval = "Pending";
                changeCommission.created_by = Convert.ToInt32(Session["userId"]);
                changeCommission.created_on = DateTime.Now;
                changeCommission.initialApproval = 0;
                db.ChangeCommissions.Add(changeCommission);
                db.SaveChanges();

                for (int i = 0; i < qty.Count; i++)
                {
                    ChangeCommissionPart changeCommissionPart = new ChangeCommissionPart();
                    changeCommissionPart.ChangeCommissionId = changeCommission.ID;
                    changeCommissionPart.part_number = Convert.ToInt32(part_number[i]);
                    changeCommissionPart.quantity = Convert.ToInt32(qty[i]);

                    db.ChangeCommissionParts.Add(changeCommissionPart);
                    db.SaveChanges();
                }

                //string html = "<p> Please follow <a href=\"" + Url.Action("Edit", "ChangeCommissions", new { id = changeCommission.ID }) + "\">This link</a> to approve or reject Change Commission </p>";

                List<usr_user> usr_Users = db.usr_user.Where(x =>
                changeCommission.assigned_rep == x.usr_ID || changeCommission.request_manager == x.usr_ID || changeCommission.assigned_manager == x.usr_ID
                ).ToList();

                foreach (var item in usr_Users)
                {
                    string html = string.Format(this.html, Url.Action("Edit", "ChangeCommissions", new { id = changeCommission.ID }, this.Request.Url.Scheme), item.usr_fName);
                    //SendEmail(item.usr_email, string.Format("Commission Form Request #{0}", changeCommission.number), html);
                    SendEmail("brown.b@rittal.us", string.Format("Commission Form Request #{0}", changeCommission.number), html);
                }

                return RedirectToAction("Index");
            }

            ViewBag.qty = qty;
            ViewBag.part_number = part_number;
            GetDataForUser(userId);

            return View(changeCommission);
        }

        [NonAction]
        public List<SelectListItem> FillcreditOptions()
        {
            List<SelectListItem> creditOptions = new List<SelectListItem>();
            for (int i = 5; i < 100; i += 5)
            {
                creditOptions.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            return creditOptions;
        }


        public string GetRoleType(ChangeCommission changeCommission, List<int> level1UserIds, List<int> level2UserIds, int userId)
        {
            if (level1UserIds.Contains(userId))
            {
                return roleTypes[1];
            }
            else if (level2UserIds.Contains(userId))
            {
                return roleTypes[0];
            }
            else
            {
                return roleTypes[2];
            }
        }

        public bool GetRoleIsAdmin(int userId, List<int> level2UserIds, List<int> level1UserIds, ChangeCommission changeCommission)
        {
            if (level2UserIds.Contains(userId) && level1UserIds.Contains(userId))
            {
                if (db.ChangeCommissionHistories.Any(x => x.ChangeCommissionId == changeCommission.ID && x.created_by == userId))
                {
                    //In Admin Role
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return level2UserIds.Contains(userId);
            }

        }

        public bool GetRoleIsEditable(ChangeCommission changeCommission, List<int> level1UserIds, List<int> level2UserIds, int userId)
        {
            if (changeCommission.created_by == userId || level2UserIds.Contains(userId))
            {
                List<int> userIds = db.ChangeCommissionHistories.Where(x => x.ChangeCommissionId == changeCommission.ID && level1UserIds.Contains(x.created_by))
                                .Select(x => x.created_by).Distinct().ToList();

                if (userIds.Count > 0 && changeCommission.created_by == userId)
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public bool GetRoleIsViewable(ChangeCommission changeCommission, List<int> level1UserIds, List<int> level2UserIds, int userId )
        {
            return (changeCommission.created_by == userId | level1UserIds.Contains(userId) | level2UserIds.Contains(userId));
        }

        public bool GetRoleIsApprovable(ChangeCommission changeCommission, List<int> level1UserIds, List<int> level2UserIds, int userId)
        {
            if (level1UserIds.Contains(userId) || level2UserIds.Contains(userId))
            {
                if (level2UserIds.Contains(userId))
                {
                    //Get user ids who responed
                    List<int> userIds = db.ChangeCommissionHistories.Where(x => x.ChangeCommissionId == changeCommission.ID && level1UserIds.Contains(x.created_by))
                                .Select(x => x.created_by).Distinct().ToList();

                    //Get Denied status id
                    int statusId = Convert.ToInt16(getPreApproval(-1).Single(y => y.Text == "Denied").Value);

                    bool isDenied = db.ChangeCommissionHistories.Any(x => x.ChangeCommissionId == changeCommission.ID && x.status == statusId);
                    bool isAllLevel1User = userIds.Count == level1UserIds.Count;

                    return isDenied | isAllLevel1User;
                }
                return true;
            }
            return false;
        }

        public bool GetRoleIsEmail(ChangeCommission changeCommission, List<int> level1UserIds)
        {
            //Get user ids who responed
            List<int> userIds = db.ChangeCommissionHistories.Where(x => x.ChangeCommissionId == changeCommission.ID && level1UserIds.Contains(x.created_by))
                        .Select(x => x.created_by).Distinct().ToList();

            //Get Denied status id
            int statusId = Convert.ToInt16(getPreApproval(-1).Single(y => y.Text == "Denied").Value);

            bool isDenied = db.ChangeCommissionHistories.Any(x => x.ChangeCommissionId == changeCommission.ID && x.status == statusId);
            bool isAllLevel1User = userIds.Count == level1UserIds.Count;

            if (!GetRoleIsLocked(changeCommission))
            {
                if (isDenied && isAllLevel1User)
                {
                    return false;
                }
                else if (isAllLevel1User)
                {
                    return true;
                }
                else if(isDenied)
                {
                    return true;
                }
            }

            return false;

        }

        public bool GetIsCreator(ChangeCommission changeCommission, int userid)
        {
            return changeCommission.created_by == userid;
        }

        public bool GetRoleIsLocked(ChangeCommission changeCommission)
        {
            return changeCommission.FinalApproval != "Pending";
        }

        // GET: ChangeCommissions/Edit/5
        public ActionResult Edit(int? id)
        {

            long userId = Convert.ToInt64(Session["userId"]);
            string level2Manager = ConfigurationManager.AppSettings["Level2Manager"];
            ChangeCommissionRoles roles = new ChangeCommissionRoles();
            ChangeCommission changeCommission = db.ChangeCommissions.Find(id);

            List<int> level1UserIds = new List<int>(), level2UserIds = new List<int>();

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (changeCommission == null)
            {
                return HttpNotFound();
            }


            //Fill Level 1 UserIds
            level1UserIds.Add((int)changeCommission.assigned_rep);
            level1UserIds.Add((int)changeCommission.request_manager);
            level1UserIds.Add((int)changeCommission.assigned_manager);

            //Fill Level 2 UserIds
            foreach (var item in level2Manager.Split(','))
            {
                level2UserIds.Add(Convert.ToInt32(item));
            }

            roles.IsAdmin = GetRoleIsAdmin((int)userId, level2UserIds, level1UserIds, changeCommission);
            roles.IsApprovable = GetRoleIsApprovable(changeCommission, level1UserIds, level2UserIds, (int)userId);
            roles.IsEditable = GetRoleIsEditable(changeCommission,level1UserIds, level2UserIds, (int)userId);
            roles.IsLocked = GetRoleIsLocked(changeCommission);
            roles.IsCreator = GetIsCreator(changeCommission, (int)userId);
            roles.IsViewable = GetRoleIsViewable(changeCommission,level1UserIds,level2UserIds, (int)userId);
            roles.RoleType = GetRoleType(changeCommission, level1UserIds, level2UserIds, (int)userId);

            //if CommissionForm is locked or user is not the permission of edit or approve
            if (!roles.IsViewable)
            {
                return RedirectToAction("Index");
            }

            //if (!roles.IsCreator)
            {

                var changeCommHistory = db.ChangeCommissionHistories.Where(x => x.ChangeCommissionId == changeCommission.ID).ToList();
                List<ChangeCommissionHistoryViewModel> chngComHistoryViewModel = new List<ChangeCommissionHistoryViewModel>();
                var listApprovalStatus = getPreApproval();
                foreach (var item in changeCommHistory)
                {
                    var user = db.usr_user.Single(x => x.usr_ID == item.created_by);
                    chngComHistoryViewModel.Add(new ChangeCommissionHistoryViewModel
                    {
                        notes = item.notes,
                        statusName = listApprovalStatus.Single(x => x.Value == item.status.ToString()).Text,
                        usr_User = user,
                        created_on = item.created_on
                    });
                }

                ViewBag.ChangeCommHistory = chngComHistoryViewModel;
            }

            List<ChangeCommissionPart> commissionParts = db.ChangeCommissionParts.Where(x => x.ChangeCommissionId == id).ToList();
            ViewBag.qty = commissionParts.Select(x => x.quantity.ToString()).ToList();
            ViewBag.part_number = commissionParts.Select(x => x.part_number.ToString()).ToList();
            ViewBag.creditOptions = new SelectList(FillcreditOptions(), "Value", "Text", changeCommission.credit_sale);
            ViewBag.ApprovalStatus = getPreApproval();

            GetDataForUser(userId);
            ViewBag.roles = roles;



            return View(changeCommission);
        }


        // POST: ChangeCommissions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("View")]

        public ActionResult Edit(ChangeCommission changeCommission, List<string> qty, List<string> part_number, string HistoryApproval, string historyNotes)
        {

            long userId = Convert.ToInt64(Session["userId"]);
            string level2Manager = ConfigurationManager.AppSettings["Level2Manager"];
            List<int> level1UserIds = new List<int>(), level2UserIds = new List<int>();
            ChangeCommissionRoles roles = new ChangeCommissionRoles();

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //Populate level 1 user ids
            level1UserIds.Add((int)changeCommission.assigned_rep);
            level1UserIds.Add((int)changeCommission.request_manager);
            level1UserIds.Add((int)changeCommission.assigned_manager);

            //Populate level 2 user ids
            foreach (var item in level2Manager.Split(','))
            {
                level2UserIds.Add(Convert.ToInt32(item));
            }

            if (ModelState.IsValid)
            {
                ChangeCommission changeComm = db.ChangeCommissions.AsNoTracking().Single(x => x.ID == changeCommission.ID);
                changeCommission.number = changeComm.number;

                roles.IsAdmin = GetRoleIsAdmin((int)userId, level2UserIds, level1UserIds, changeCommission);
                roles.IsApprovable = GetRoleIsApprovable(changeCommission, level1UserIds, level2UserIds, (int)userId);
                roles.IsCreator = GetIsCreator(changeCommission, (int)userId);

                //Check if level 2 user is in role of level 1 user (assigned managers)
                if (roles.IsApprovable && roles.IsAdmin && !string.IsNullOrEmpty(HistoryApproval))
                {
                    var listApprovalStatus = getPreApproval();
                    changeCommission.FinalApproval = listApprovalStatus.Single(x => x.Value.ToLower() == HistoryApproval.ToLower()).Text;
                }
                else
                {
                    changeCommission.FinalApproval = changeComm.FinalApproval;
                }

                //Set initial approval count if user has not already approved or reject 
                if (!string.IsNullOrEmpty(HistoryApproval) && roles.IsApprovable)
                {
                    changeCommission.initialApproval = ++changeComm.initialApproval;
                }
                else
                {
                    changeCommission.initialApproval = changeComm.initialApproval;
                }


                changeCommission.created_by = changeComm.created_by;
                changeCommission.created_on = changeComm.created_on;
                if (roles.IsCreator)
                {
                    changeCommission.updated_by = (int)userId;
                    changeCommission.updated_on = DateTime.Now;
                }
                db.Entry(changeCommission).State = EntityState.Modified;
                db.SaveChanges();

                //Delete all prevoius commissions
                List<ChangeCommissionPart> changeCommissionPartList = db.ChangeCommissionParts.Where(x => x.ChangeCommissionId == changeCommission.ID).ToList();
                db.ChangeCommissionParts.RemoveRange(changeCommissionPartList);

                for (int i = 0; i < qty.Count; i++)
                {
                    ChangeCommissionPart changeCommissionPart = new ChangeCommissionPart();
                    changeCommissionPart.ChangeCommissionId = changeCommission.ID;
                    changeCommissionPart.part_number = Convert.ToInt32(part_number[i]);
                    changeCommissionPart.quantity = Convert.ToInt32(qty[i]);

                    db.ChangeCommissionParts.Add(changeCommissionPart);
                    db.SaveChanges();
                }

                if (!string.IsNullOrEmpty(HistoryApproval) && roles.IsApprovable)
                {
                    var commHist = new ChangeCommissionHistory
                    {
                        ChangeCommissionId = changeCommission.ID,
                        notes = historyNotes,
                        status = Convert.ToInt32(HistoryApproval),
                        created_by = (int)userId,
                        created_on = DateTime.Now
                    };
                    db.ChangeCommissionHistories.Add(commHist);
                    db.SaveChanges();

                    if (GetRoleIsEmail(changeCommission, level1UserIds))
                    {
                        List<usr_user> usr_Users = db.usr_user.Where(x => level2UserIds.Contains(x.usr_ID)).ToList();

                        foreach (var item in usr_Users)
                        {
                            string html = string.Format(this.AdminHtml, Url.Action("Edit", "ChangeCommissions", new { id = changeCommission.ID }, Request.Url.Scheme),
                                item.usr_fName, changeCommission.number, getPreApproval().Single(x => x.Value.ToLower() == HistoryApproval.ToLower()).Text);
                            SendEmail(item.usr_email, string.Format("Request #{0} final decision required", changeCommission.number), html);
                        }
                    }

                }

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.creditOptions = new SelectList(FillcreditOptions(), "Value", "Text", changeCommission.credit_sale);
                ViewBag.qty = qty;
                ViewBag.part_number = part_number;
                ViewBag.ApprovalStatus = getPreApproval();
                GetDataForUser(userId);
                ViewBag.roles = roles;
            }



            return View(changeCommission);
        }

        public List<SelectListItem> getPreApproval(int id = -1)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string[] status = new string[] { "Approved", "Denied" };

            for (int i = 0; i < status.Length; i++)
            {
                if ((i + 1) == id)
                {
                    items.Add(new SelectListItem { Text = status[i], Value = (i + 1).ToString(), Selected = true });
                }
                else
                {
                    items.Add(new SelectListItem { Text = status[i], Value = (i + 1).ToString() });
                }
            }

            return items;
        }


        [HttpGet]
        public ActionResult CommissionAdminPanel()
        {
            List<usr_user> usr_User = db.usr_user.Where(x => x.usr_email.Contains("@rittal.us")).ToList();

            CommissionAdmin commissionAdmin = db.CommissionAdmins.FirstOrDefault();
            commissionAdmin = commissionAdmin ?? new CommissionAdmin();
            ViewBag.users = usr_User.Select(x => new SelectListItem
            {
                Text = x.usr_fName + " " + x.usr_lName + "[" + x.usr_email + "]",
                Value = x.usr_ID.ToString()
            }).ToList();
            return View(commissionAdmin);
        }

        [HttpPost]
        public ActionResult CommissionAdminPanel(CommissionAdmin commissionAdmin)
        {
            List<usr_user> usr_User = db.usr_user.Where(x => x.usr_email.Contains("@rittal.us")).ToList();

            //if (ModelState.IsValid)
            {

                db.CommissionAdmins.RemoveRange(db.CommissionAdmins);

                db.CommissionAdmins.Add(commissionAdmin);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.users = usr_User.Select(x => new SelectListItem
            //{
            //    Text = x.usr_fName + " " + x.usr_lName + "[" + x.usr_email + "]",
            //    Value = x.usr_ID.ToString()
            //}).ToList();
            //return View();

        }

        // GET: ChangeCommissions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChangeCommission changeCommission = db.ChangeCommissions.Find(id);
            if (changeCommission == null)
            {
                return HttpNotFound();
            }
            return View(changeCommission);
        }

        // POST: ChangeCommissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ChangeCommission changeCommission = db.ChangeCommissions.Find(id);
            db.ChangeCommissions.Remove(changeCommission);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public bool SendEmail(string to_email, string title, string content)
        {
            CommonController comm = new CommonController();
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

            //Send email to the Return Requester
            MailMessage message_requester = new MailMessage("webmaster@rittal.us", to_email, title, header + content + footer);
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

            return true;
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

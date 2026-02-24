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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using CsvHelper;
using System.IO;
using newrisourcecenter.ViewModels;
using Microsoft.Office.Interop.Excel;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class RegisteredUserController : Controller
    {
        private ApplicationUserManager _userManager;
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController commCtl = new CommonController();

        public RegisteredUserController()
        {
        }

        public RegisteredUserController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #region Index
        // GET: UserViewModels
        public ActionResult Index(int compid = 0, int locid = 0, string msg = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Keep the Company Dropdown logic for the filter menu
            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                var compdata = db.partnerCompanyViewModels;
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata.OrderBy(a => a.comp_name))
                {
                    complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name });
                }
                ViewBag.compData = complist;
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetUsersPaged(int start, int limit, int compid = 0, int locid = 0, string querystring = "")
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long companyId = Convert.ToInt64(Session["companyId"]);
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            // 1. Base Query
            IQueryable<UserViewModel> query = db.UserViewModels.Where(a => !a.deleted && string.IsNullOrEmpty(a.system_ID));

            // 2. Apply your existing filtering logic
            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                if (compid > 0)
                {
                    query = query.Where(a => a.comp_ID == compid);
                    if (locid != 0)
                    {
                        query = query.Where(a => a.comp_loc_ID == locid);
                    }
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                query = query.Where(a => a.comp_ID == companyId);
            }
            else
            {
                query = query.Where(a => a.usr_ID == userId);
            }

            // 3. Apply Search String (querystring)
            if (!string.IsNullOrEmpty(querystring))
            {
                query = query.Where(a => querystring.Contains(a.usr_fName) ||
                                         querystring.Contains(a.usr_lName) ||
                                         a.usr_email.Contains(querystring));
            }

            // 4. Get Total Count for Pagination
            int totalCount = await query.CountAsync();

            // 5. Paginate and Fetch
            var rawData = await query.OrderBy(a => a.usr_fName)
                                     .Skip(start)
                                     .Take(limit)
                                     .ToListAsync();

            // 6. Map to ViewModel & Role Logic
            var resultList = rawData.Select(item => new {
                item.usr_ID,
                item.usr_fName,
                item.usr_lName,
                item.usr_email,
                item.comp_ID,
                user_role = ""
            }).ToList();

            return Json(new { rows = resultList, total = totalCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocations(int compid)
        {
            var locations = db.partnerLocationViewModels
                .Where(a => a.comp_ID == compid)
                .Select(l => new { id = l.loc_ID, name = l.loc_name }) // Adjust names to match your model
                .ToList();

            return Json(locations, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Export
        // GET: UserViewModels
        public async Task<ActionResult> Export(int compid = 0, int locid = 0, int next = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            long companyId = Convert.ToInt64(Session["companyId"]);
            string system_ID = Convert.ToString(Session["system_ID"]);
            ViewBag.LastID = "";

            List<UserViewModel> usr_list = new List<UserViewModel>();
            IQueryable<UserViewModel> user_data;

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                var compdata = db.partnerCompanyViewModels.Where(x => x.comp_active == 1);
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata.OrderBy(a => a.comp_name))
                {
                    complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name });
                }
                ViewBag.compData = complist;

                if (compid == 0)
                {
                    if (next == 0)
                    {
                        user_data = db.UserViewModels.Where(a => a.comp_ID == companyId);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => a.comp_ID == companyId && a.usr_ID >= next);
                    }
                }
                else
                {
                    ViewBag.locationsDatas = db.partnerLocationViewModels.Where(a => a.comp_ID == compid);
                    if (compid != 0 && locid == 0)
                    {
                        user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.usr_ID >= next);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_ID >= next);
                    }
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                if (next == 0)
                {
                    user_data = db.UserViewModels.Where(a => a.comp_ID == companyId);
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => a.comp_ID == companyId && a.usr_ID >= next);
                }
            }
            else
            {
                user_data = db.UserViewModels.Where(a => a.usr_ID == userId);
            }

            int userCount = await user_data.CountAsync();
            ViewBag.showNext = userCount > 20;
            foreach (var item in await user_data.Take(20).ToListAsync())
            {
                string admin = "";
                if (item.system_ID != null)
                {
                    var users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                    if (users_roles.Count() != 0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") && User.IsInRole("Rittal User") || users_roles.Contains("Local Admin") && users_roles.Contains("Channel User") || users_roles.Contains("Global Admin")))
                    {
                        admin = "Admin";
                    }
                }

                usr_list.Add(new UserViewModel
                {
                    usr_ID = item.usr_ID,
                    usr_fName = item.usr_fName,
                    usr_lName = item.usr_lName,
                    comp_ID = item.comp_ID,
                    usr_email = item.usr_email,
                    user_role = admin
                });
            }

            //Get last ID
            if (usr_list.Count() != 0)
            {
                ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
            }

            return View(usr_list.OrderBy(a => a.usr_fName));
        }

        [HttpGet]
        public async Task<FileStreamResult> ExportCSV(int compid = 0, int locid = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long companyId = Convert.ToInt64(Session["companyId"]);
            List<UserExport> usr_list = new List<UserExport>();
            IQueryable<UserViewModel> user_data;

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin") || (User.IsInRole("Local Admin") && User.IsInRole("Channel User")))
            {
                user_data = db.UserViewModels.Where(a => a.comp_ID == (compid == 0 ? companyId : compid));
            }
            else
            {
                user_data = db.UserViewModels.Where(a => a.usr_ID == userId);
            }
            Dictionary<long, string> companyNames = await db.partnerCompanyViewModels.ToDictionaryAsync(x => x.comp_ID, x => x.comp_name);
            Dictionary<long, string> locationNames = await db.partnerLocationViewModels.ToDictionaryAsync(x => x.loc_ID, x => x.loc_name);
            foreach (var item in await user_data.ToListAsync())
            {
                usr_list.Add(new UserExport
                {
                    userID = item.usr_ID,
                    firstName = item.usr_fName,
                    lastName = item.usr_lName,
                    email = item.usr_email,
                    companyName = (item.comp_ID.HasValue && companyNames.ContainsKey(item.comp_ID.Value) ? companyNames[item.comp_ID.Value] : ""),
                    companyLocation = (item.comp_loc_ID.HasValue && locationNames.ContainsKey(item.comp_loc_ID.Value) ? locationNames[item.comp_loc_ID.Value] : "")
                });
            }
            var result = WriteCsvToMemory(usr_list);
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "users_export.csv" };
        }

        public byte[] WriteCsvToMemory(dynamic records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }
        #endregion

        #region Change User Status
        [HttpPost]
        public async Task<JsonResult> UserStatus(string status = null, int id = 0)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }
                UserViewModel userViewModel = await db.UserViewModels.Where(a => a.usr_ID == id).FirstOrDefaultAsync();
                if (userViewModel == null)
                {
                    throw new Exception("User not found");
                }
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));//get the users and roles
                AccountController accountController = new AccountController();
                var userdata = await UserManager.Users.Where(a => a.Email == userViewModel.usr_email).FirstOrDefaultAsync();
                userdata.EmailConfirmed = (status == "approve");
                UserStore<ApplicationUser> dbApp = new UserStore<ApplicationUser>(db);
                await dbApp.UpdateAsync(userdata);
                if (status == "approve")
                {
                    await accountController.ConfirmUser(userdata.Id);
                }
                else
                {
                    await accountController.DenyUser(userdata.Id);
                }
                return Json("OK");

            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(e.Message);
            }
        }
        #endregion

        // GET: RegisteredUser/Delete/5
        public async Task<JsonResult> Delete(int? id)
        {
            try
            {
                if(id == null)
                {
                    throw new Exception("An error occurred while processing your request.");
                }
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    throw new Exception("Please Login. Login has timed out");
                }
                UserViewModel userViewModel = await db.UserViewModels.Where(a => a.usr_ID == id).FirstOrDefaultAsync();
                if (userViewModel == null)
                {
                    throw new Exception("User not found");
                }
                string userEmail = userViewModel.usr_email;
                db.UserViewModels.Remove(userViewModel);
                await db.SaveChangesAsync();
                var checkuser_temp = await dbEntity.usr_user_temp.Where(a => a.usr_email == userEmail).FirstOrDefaultAsync();
                if (checkuser_temp != null)
                {
                    dbEntity.usr_user_temp.Remove(checkuser_temp);
                }
                var userdata = await UserManager.Users.Where(a => a.Email == userEmail).FirstOrDefaultAsync();
                if (userdata != null)
                {
                    await UserManager.DeleteAsync(userdata);
                }
                //Log the action by the user
                await commCtl.siteActionLog(0, "RegisteredUser", DateTime.Now, " The user id =" + userViewModel.usr_email + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));
                return Json("OK");
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(e.Message);
            }
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

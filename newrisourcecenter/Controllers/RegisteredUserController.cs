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
        public async Task<ActionResult> Index(int compid = 0, int locid = 0, string msg = null, int prev1 = 0, int next = 0, string querystring = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            long siteRole = Convert.ToInt64(Session["siteRole"]);
            long adminRole = Convert.ToInt64(Session["userRole"]);
            long companyId = Convert.ToInt64(Session["companyId"]);
            string system_ID = Convert.ToString(Session["system_ID"]);
            ViewBag.LastID = "";

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var roles = roleManager.Roles;
            roles.Where(a => a.Id == system_ID);
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));//get the users and roles
            List<UserViewModel> usr_list = new List<UserViewModel>();
            IQueryable<UserViewModel> user_data;

            if (User.IsInRole("Super Admin") || User.IsInRole("Global Admin"))
            {
                var compdata = db.partnerCompanyViewModels;
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata.OrderBy(a => a.comp_name))
                {
                    complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name });
                }
                ViewBag.compData = complist;

                if (compid == 0 && msg == null)
                {
                    if (next == 0)
                    {
                        user_data = db.UserViewModels.Where(a => string.IsNullOrEmpty(a.system_ID));
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => a.usr_ID > next && string.IsNullOrEmpty(a.system_ID));
                    }

                    foreach (var item in await user_data.Take(20).ToListAsync())
                    {
                        usr_list.Add(new UserViewModel
                        {
                            usr_ID = item.usr_ID,
                            usr_fName = item.usr_fName,
                            usr_lName = item.usr_lName,
                            comp_ID = item.comp_ID,
                            usr_email = item.usr_email,
                            user_role = ""
                        });
                    }

                    //Get last ID
                    int totalCount = await user_data.CountAsync();
                    if (totalCount > 20 && usr_list.Count() != 0)
                    {
                        ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                    }
                    else
                    {
                        ViewBag.LastID = 0;
                    }

                    return View(usr_list.OrderByDescending(a => a.usr_ID));
                }
                else
                {
                    ViewBag.locationsDatas = db.partnerLocationViewModels.Where(a => a.comp_ID == compid);
                    if (querystring != "")
                    {
                        if (compid != 0 && locid == 0)
                        {
                            user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.usr_fName == querystring || a.comp_ID == compid && a.usr_lName == querystring || a.comp_ID == compid && a.usr_email.Contains(querystring) && a.usr_ID >= next && string.IsNullOrEmpty(a.system_ID));
                        }
                        else
                        {
                            user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_fName == querystring || a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_lName == querystring || a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_email.Contains(querystring) && a.usr_ID >= next && string.IsNullOrEmpty(a.system_ID));
                        }
                    }
                    else
                    {
                        if (compid != 0 && locid == 0)
                        {
                            user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.usr_ID >= next && string.IsNullOrEmpty(a.system_ID));
                        }
                        else
                        {
                            user_data = db.UserViewModels.Where(a => a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_ID >= next && string.IsNullOrEmpty(a.system_ID));
                        }
                    }

                    foreach (var item in await user_data.Take(20).ToListAsync())
                    {
                        usr_list.Add(new UserViewModel
                        {
                            usr_ID = item.usr_ID,
                            usr_fName = item.usr_fName,
                            usr_lName = item.usr_lName,
                            comp_ID = item.comp_ID,
                            usr_email = item.usr_email,
                            user_role = ""
                        });
                    }

                    //Get last ID
                    int totalCount = await user_data.CountAsync();
                    if (totalCount > 20 && usr_list.Count() != 0)
                    {
                        ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                    }
                    else
                    {
                        ViewBag.LastID = 0;
                    }

                    return View(usr_list.OrderByDescending(a => a.usr_ID));
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                if (next == 0)
                {
                    user_data = db.UserViewModels.Where(a => a.comp_ID == companyId && string.IsNullOrEmpty(a.system_ID));
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => a.comp_ID == companyId && a.usr_ID > next && string.IsNullOrEmpty(a.system_ID));
                }

                foreach (var item in await user_data.ToListAsync())
                {

                    usr_list.Add(new UserViewModel
                    {
                        usr_ID = item.usr_ID,
                        usr_fName = item.usr_fName,
                        usr_lName = item.usr_lName,
                        comp_ID = item.comp_ID,
                        usr_email = item.usr_email,
                        user_role = ""
                    });
                }

                //Get last ID
                int totalCount = await user_data.CountAsync();
                if (totalCount > 20 && usr_list.Count() != 0)
                {
                    ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                }
                else
                {
                    ViewBag.LastID = 0;
                }

                return View(usr_list.OrderByDescending(a => a.usr_ID));
            }
            else
            {
                user_data = db.UserViewModels.Where(a => a.usr_ID == userId);
                foreach (var item in await user_data.ToListAsync())
                {
                    usr_list.Add(new UserViewModel
                    {
                        usr_ID = item.usr_ID,
                        usr_fName = item.usr_fName,
                        usr_lName = item.usr_lName,
                        comp_ID = item.comp_ID,
                        usr_email = item.usr_email,
                        user_role = ""
                    });
                }

                //Get last ID
                ViewBag.LastID = 0;

                return View(usr_list);
            }
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

        #region Search User
        public async Task<ActionResult> searchUsers(string form_value = null, int selected_comp_id = 0, int selected_loc_id = 0)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                long companyId = Convert.ToInt64(Session["companyId"]);

                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));//get the users and roles
                List<UserViewModel> usr_list = new List<UserViewModel>();

                //Get the combined data for user and form
                List<UserViewModel> user_data;
                var form_data = form_value.Split(' ');
                if (form_data.Count() == 1)
                {
                    user_data = await db.UserViewModels.Where(a => (a.usr_fName == form_value || a.usr_lName == form_value || a.usr_email.Contains(form_value)) && string.IsNullOrEmpty(a.system_ID)).ToListAsync();
                }
                else
                {
                    var firstname = form_data[0].ToString();
                    var lastname = form_data[1].ToString();
                    user_data = await db.UserViewModels.Where(a => a.usr_fName.Contains(firstname) && a.usr_lName.Contains(lastname) && string.IsNullOrEmpty(a.system_ID)).ToListAsync();
                }

                foreach (var item in user_data)
                {
                    if (selected_comp_id > 0 && item.comp_ID != selected_comp_id)
                    {
                        continue;
                    }
                    if (selected_loc_id > 0 && item.comp_loc_ID != selected_loc_id)
                    {
                        continue;
                    }
                    usr_list.Add(new UserViewModel
                    {
                        usr_ID = item.usr_ID,
                        usr_fName = item.usr_fName,
                        usr_lName = item.usr_lName,
                        comp_ID = item.comp_ID,
                        usr_email = item.usr_email,
                        user_role = ""
                    });
                }

                return View(usr_list.OrderByDescending(a => a.usr_ID));
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
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
            UserViewModel userViewModel = await db.UserViewModels.FindAsync(id);
            if (userViewModel == null)
            {
                return HttpNotFound();
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

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"] });
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

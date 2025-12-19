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

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class UserViewModelsController : Controller
    {
        private ApplicationUserManager _userManager;
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController commCtl = new CommonController();

        public UserViewModelsController()
        {
        }

        public UserViewModelsController(ApplicationUserManager userManager)
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
        public async Task<ActionResult> Index(int compid = 0,int locid=0, string msg = null, int prev1 = 0, int next = 0,string querystring = null)
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

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
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
                        user_data = db.UserViewModels.Where(a =>  !a.deleted && a.comp_ID == companyId);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId && a.usr_ID >= next);
                    }

                    foreach (var item in await user_data.Take(20).ToListAsync())
                    {
                        ViewBag.admin = "";
                        if (item.system_ID != null)
                        {
                            IList<string> users_roles = new List<string>();
                            try
                            {
                                users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                            }
                            catch (Exception) { continue; }//Get users by role
                            if (users_roles.Count() != 0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") && User.IsInRole("Rittal User") || users_roles.Contains("Local Admin") && users_roles.Contains("Channel User") || users_roles.Contains("Global Admin")))
                            {
                                ViewBag.admin = "Admin";
                            }
                        }

                        usr_list.Add(new UserViewModel
                        {
                            usr_ID = item.usr_ID,
                            usr_fName = item.usr_fName,
                            usr_lName = item.usr_lName,
                            comp_ID = item.comp_ID,
                            usr_email = item.usr_email,
                            user_role = ViewBag.admin
                        });
                    }

                    //Get last ID
                    if (usr_list.Count() != 0)
                    {
                        ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                    }

                    return View(usr_list.OrderBy(a => a.usr_fName));
                }
                else
                {
                    ViewBag.locationsDatas = db.partnerLocationViewModels.Where(a=>a.comp_ID==compid);
                    if (querystring != "")
                    {
                        if (compid != 0 && locid == 0)
                        {
                            user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.usr_fName == querystring || a.comp_ID == compid && a.usr_lName == querystring || a.comp_ID == compid && a.usr_email.Contains(querystring) && a.usr_ID >= next);
                        }
                        else
                        {
                            user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_fName == querystring || a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_lName == querystring || a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_email.Contains(querystring) && a.usr_ID >= next);
                        }
                    }
                    else
                    {
                        if (compid != 0 && locid == 0)
                        {
                            user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.usr_ID >= next);
                        }
                        else
                        {
                            user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_ID >= next);
                        }
                    }

                    foreach (var item in await user_data.Take(20).ToListAsync())
                    {
                        ViewBag.admin = "";
                        if (item.system_ID != null)
                        {
                            IList<string> users_roles = new List<string>();
                            try
                            {
                                users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                            }
                            catch (Exception) { continue; }//Get users by role
                            if (users_roles.Count()!=0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") && users_roles.Contains("Rittal User") || users_roles.Contains("Local Admin") && users_roles.Contains("Channel User") || User.IsInRole("Global Admin")))
                            {
                                ViewBag.admin = "Admin";
                            }
                        }

                        usr_list.Add(new UserViewModel
                        {
                            usr_ID = item.usr_ID,
                            usr_fName = item.usr_fName,
                            usr_lName = item.usr_lName,
                            comp_ID = item.comp_ID,
                            usr_email = item.usr_email,
                            user_role = ViewBag.admin
                        });
                    }

                    //Get last ID
                    if (usr_list.Count() != 0)
                    {
                        ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                    }

                    return View(usr_list.OrderBy(a => a.usr_fName));
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                if (next == 0)
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId).Take(20);
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId && a.usr_ID >= next).Take(20);
                }

                foreach (var item in await user_data.ToListAsync())
                {
                    ViewBag.admin = "";
                    if (item.system_ID != null)
                    {
                        IList<string> users_roles = new List<string>();
                        try
                        {
                            users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                        }
                        catch (Exception) { continue; }//Get users by role
                        if (users_roles.Count() != 0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") && users_roles.Contains("Rittal User") || users_roles.Contains("Local Admin") && users_roles.Contains("Channel User") || User.IsInRole("Global Admin")))
                        {
                            ViewBag.admin = "Admin";
                        }
                    }

                    usr_list.Add(new UserViewModel
                    {
                        usr_ID = item.usr_ID,
                        usr_fName = item.usr_fName,
                        usr_lName = item.usr_lName,
                        comp_ID = item.comp_ID,
                        usr_email = item.usr_email,
                        user_role = ViewBag.admin
                    });
                }

                //Get last ID
                if (usr_list.Count() != 0)
                {
                    ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                }

                return View(usr_list.OrderBy(a => a.usr_fName));
            }
            else
            {
                user_data = db.UserViewModels.Where(a => a.usr_ID == userId);
                foreach (var item in await user_data.ToListAsync())
                {
                    ViewBag.admin = "";
                    if (item.system_ID != null)
                    {
                        IList<string> users_roles = new List<string>();
                        try
                        {
                            users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                        }
                        catch (Exception) { continue; }//Get users by role
                        if (users_roles.Count() != 0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") && users_roles.Contains("Rittal User") || users_roles.Contains("Local Admin") && users_roles.Contains("Channel User") || User.IsInRole("Global Admin")))
                        {
                            ViewBag.admin = "Admin";
                        }
                    }

                    usr_list.Add(new UserViewModel
                    {
                        usr_ID = item.usr_ID,
                        usr_fName = item.usr_fName,
                        usr_lName = item.usr_lName,
                        comp_ID = item.comp_ID,
                        usr_email = item.usr_email,
                        user_role = ViewBag.admin
                    });
                }

                //Get last ID
                if (usr_list.Count() != 0)
                {
                    ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
                }

                return View(usr_list.OrderBy(a => a.usr_fName));
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
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId && a.usr_ID >= next);
                    }
                }
                else
                {
                    ViewBag.locationsDatas = db.partnerLocationViewModels.Where(a => a.comp_ID == compid);
                    if (compid != 0 && locid == 0)
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.usr_ID >= next);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_ID >= next);
                    }
                }
            }
            else if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
            {
                if (next == 0)
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId);
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId && a.usr_ID >= next);
                }
            }
            else
            {
                user_data = db.UserViewModels.Where(a => !a.deleted && a.usr_ID == userId);
            }

            int userCount = await user_data.CountAsync();
            ViewBag.showNext = userCount > 20;
            foreach (var item in await user_data.Take(20).ToListAsync())
            {
                string admin = "";
                if (item.system_ID != null)
                {
                    IList<string> users_roles = new List<string>();
                    try
                    {
                        users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                    }
                    catch (Exception) { continue; }//Get users by role
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
                user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == (compid == 0 ? companyId : compid));
            }
            else
            {
                user_data = db.UserViewModels.Where(a => !a.deleted && a.usr_ID == userId);
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
                    companyLocation = (item.comp_loc_ID.HasValue && locationNames.ContainsKey(item.comp_loc_ID.Value) ? locationNames[item.comp_loc_ID.Value] : ""),
                    status = (item.inactive ? "In-Active" : "Active")
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

        #region Last Login Report
        // GET: UserViewModels
        public async Task<ActionResult> LastLoginReport(int compid = 0, int locid = 0, int next = 0)
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
            var compdata = db.partnerCompanyViewModels.Where(x => x.comp_active == 1);
            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
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
                        user_data = db.UserViewModels.Where(a => !a.deleted);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.usr_ID >= next);
                    }
                }
                else
                {
                    ViewBag.locationsDatas = db.partnerLocationViewModels.Where(a => a.comp_ID == compid);
                    if (compid != 0 && locid == 0)
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.usr_ID >= next);
                    }
                    else
                    {
                        user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid && a.comp_loc_ID == locid && a.usr_ID >= next);
                    }
                }
            }
            else if (User.IsInRole("Channel User"))
            {
                if (next == 0)
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId);
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId && a.usr_ID >= next);
                }
            }
            else
            {
                user_data = db.UserViewModels.Where(a => !a.deleted && a.usr_ID == userId);
            }

            int userCount = await user_data.CountAsync();
            ViewBag.showNext = userCount > 100;
            foreach (var item in await user_data.Take(100).ToListAsync())
            {
                string admin = "";
                if (item.system_ID != null)
                {
                    IList<string> users_roles = new List<string>();
                    try
                    {
                        users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                    }
                    catch (Exception) { continue; }//Get users by role
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
                    user_role = admin,
                    usr_lastLogin = item.usr_lastLogin
                });
            }

            //Get last ID
            if (usr_list.Count() != 0)
            {
                ViewBag.LastID = usr_list.LastOrDefault().usr_ID;
            }
            LastLoginReportViewModel viewModel = new LastLoginReportViewModel()
            {
                users = usr_list.OrderBy(a => a.usr_fName).ToList(),
                companyNames = compdata.ToDictionary(x => x.comp_ID, x => x.comp_name)
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<FileStreamResult> ExportLastLoginCSV(int compid = 0, int locid = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long companyId = Convert.ToInt64(Session["companyId"]);
            List<UserExport> usr_list = new List<UserExport>();
            IQueryable<UserViewModel> user_data;

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                if(compid == 0)
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted);
                }
                else
                {
                    user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == compid);
                }
            }
            else if(User.IsInRole("Channel User"))
            {
                user_data = db.UserViewModels.Where(a => !a.deleted && a.comp_ID == companyId);
            }
            else
            {
                user_data = db.UserViewModels.Where(a => !a.deleted && a.usr_ID == userId);
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
                    companyLocation = (item.comp_loc_ID.HasValue && locationNames.ContainsKey(item.comp_loc_ID.Value) ? locationNames[item.comp_loc_ID.Value] : ""),
                    lastLogin = (item.usr_lastLogin.HasValue ? item.usr_lastLogin.Value.ToString() : ""),
                    status = (item.inactive ? "In-Active" : "Active")
                });
            }
            var result = WriteCsvToMemory(usr_list);
            var memoryStream = new MemoryStream(result);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "last_login_export.csv" };
        }
        #endregion
        #region Search User
        public async Task<ActionResult> searchUsers(string form_value = null,int selected_comp_id=0,int selected_loc_id=0)
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
                if (form_data.Count()==1)
                {
                    user_data = await db.UserViewModels.Where(a => !a.deleted && (a.usr_fName == form_value || a.usr_lName == form_value || a.usr_email.Contains(form_value))).ToListAsync();
                }
                else
                {
                    var firstname = form_data[0].ToString();
                    var lastname = form_data[1].ToString();
                    user_data = await db.UserViewModels.Where(a => !a.deleted && a.usr_fName.Contains(firstname) && a.usr_lName.Contains(lastname)).ToListAsync();
                }

                foreach (var item in user_data)
                {
                    ViewBag.admin = "";
                    if (item.system_ID != null)
                    {
                        IList<string> users_roles = new List<string>();
                        try
                        {
                            users_roles = UserManager.GetRoles(item.system_ID);//Get users by role
                        }
                        catch(Exception) { continue; }
                        if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") && User.IsInRole("Rittal User") || User.IsInRole("Global Admin"))
                        {
                            if (users_roles.Count()!=0 && (users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") || User.IsInRole("Global Admin")))//filter by user role and put a star on the end
                            {
                                //filter by selected company
                                if (selected_comp_id!=0 && selected_loc_id==0)
                                {
                                    if (selected_comp_id == item.comp_ID)
                                    {
                                        usr_list.Add(new UserViewModel
                                        {
                                            usr_ID = item.usr_ID,
                                            usr_fName = item.usr_fName,
                                            usr_lName = item.usr_lName,
                                            comp_ID = item.comp_ID,
                                            usr_email = item.usr_email,
                                            user_role = "Admin"
                                        });
                                    }
                                }
                                //filter by selected company and location
                                if (selected_comp_id != 0 && selected_loc_id != 0)
                                {
                                    if (selected_comp_id==item.comp_ID && selected_loc_id==item.comp_loc_ID)
                                    {
                                        usr_list.Add(new UserViewModel
                                        {
                                            usr_ID = item.usr_ID,
                                            usr_fName = item.usr_fName,
                                            usr_lName = item.usr_lName,
                                            comp_ID = item.comp_ID,
                                            usr_email = item.usr_email,
                                            user_role = "Admin"
                                        });
                                    }
                                }
                                else if (selected_comp_id == 0 && selected_loc_id == 0)
                                {
                                    usr_list.Add(new UserViewModel
                                    {
                                        usr_ID = item.usr_ID,
                                        usr_fName = item.usr_fName,
                                        usr_lName = item.usr_lName,
                                        comp_ID = item.comp_ID,
                                        usr_email = item.usr_email,
                                        user_role = "Admin"
                                    });
                                }
                            }
                            else
                            {
                                //filter by selected company
                                if (selected_comp_id != 0 && selected_loc_id == 0)
                                {
                                    if (selected_comp_id == item.comp_ID)
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
                                }
                                //filter by selected company and location
                                if (selected_comp_id != 0 && selected_loc_id != 0)
                                {
                                    if (selected_comp_id == item.comp_ID && selected_loc_id == item.comp_loc_ID)
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
                                }
                                else if(selected_comp_id == 0 && selected_loc_id == 0)
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
                            }
                        }
                        else
                        {
                            if (item.comp_ID == companyId)
                            {
                                if (users_roles.Count() != 0 && users_roles.Contains("Super Admin") || users_roles.Contains("Local Admin") || User.IsInRole("Global Admin"))
                                {
                                    usr_list.Add(new UserViewModel
                                    {
                                        usr_ID = item.usr_ID,
                                        usr_fName = item.usr_fName,
                                        usr_lName = item.usr_lName,
                                        comp_ID = item.comp_ID,
                                        usr_email = item.usr_email,
                                        user_role = "Admin"
                                    });
                                }
                                else
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
                            }
                        }
                    }
                }

                return View(usr_list.OrderBy(a=>a.usr_fName));
            }
            catch (Exception ex)
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
                UserViewModel userViewModel = await db.UserViewModels.FindAsync(id);// Get the users data
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));//get the users and roles

                if (userViewModel.system_ID == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json("Update failed");
                }

                if (status == "deactivate")
                {
                    //Update the users status
                    var userdata = await UserManager.Users.Where(a => a.Id == userViewModel.system_ID).FirstOrDefaultAsync();
                    userdata.EmailConfirmed = false;
                    UserStore<ApplicationUser> dbApp = new UserStore<ApplicationUser>(db);
                    await dbApp.UpdateAsync(userdata);

                    //Set the global variables for the email function
                    string From = "webmaster@rittal.us";
                    string To = userdata.Email;
                    var Subject = "RiSourceCenter Account Deactivated";
                    var Body = string.Format("Dear " + To + ",<br /> Your RiSourceCenter Account has been deactivated. Contact customer service in case of any questions.");

                    commCtl.email(From, To, Subject, Body); // call the email function
                }
                else
                {
                    //Update the users status
                    var userdata = await UserManager.Users.Where(a => a.Id == userViewModel.system_ID).FirstOrDefaultAsync();
                    userdata.EmailConfirmed = true;
                    UserStore<ApplicationUser> dbApp = new UserStore<ApplicationUser>(db);
                    await dbApp.UpdateAsync(userdata);

                    //Set the global variables for the email function
                    string From = "webmaster@rittal.us";
                    string To = userdata.Email;
                    var Subject = "RiSourceCenter Account Activated";
                    var callbackUrl = Url.Action("Login", "Account", new { }, protocol: Request.Url.Scheme); // create the login Url
                    var Body = string.Format("Dear " + To + ",<br /> Your RiSourceCenter Account has been activated. You may login at any time by clicking  <a href=\"" + callbackUrl + "\">here</a> to login to your account.");

                    commCtl.email(From, To, Subject, Body); // call the email function
                }

                return Json("OK");

            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
        }
        #endregion

        #region Create User
        // GET: UserViewModels/Create
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var themes = db.UserThemes;
            List<SelectListItem> them = new List<SelectListItem>();
            them.Add(new SelectListItem { Text = "Select A Them", Value = "Select", Selected = true });
            foreach (var theme in themes)
            {
                them.Add(new SelectListItem { Text = theme.theme_name, Value = theme.theme_id.ToString() });
            }
            var userViewModel = new UserViewModel
            {
                list_theme_name = them
            };

            return View(userViewModel);
        }

        // POST: UserViewModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "usr_ID,usr_fName,usr_lName,usr_email,usr_title,usr_add1,usr_add2,usr_city,usr_state,usr_zip,usr_phone,usr_fax,usr_web,admin_theme,comp_ID,comp_loc_ID,system_ID,role,usr_pages,usr_sales")] UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                db.UserViewModels.Add(userViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(userViewModel);
        }
        #endregion

        #region Edit User Data
        // GET: UserViewModels/Edit/5
        public async Task<ActionResult> Edit(int? id, int compid = 0)
        {
            //set company id
            //if (compid != Convert.ToInt32(Session["companyId"]))
            //{
            //    compid = Convert.ToInt32(Session["companyId"]);
            //}

            long userId = Convert.ToInt64(Session["userId"]);
            var locController = new CommonController();
            int languageId = Convert.ToInt32(Session["userLanguageId"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserViewModel userViewModel = await db.UserViewModels.FindAsync(id);// Get the users data
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));//get the users and roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));// get the roles
            if (userViewModel.system_ID == null || userViewModel.deleted)
            {
                return RedirectToAction("Index", "Home");
            }
            var users_roles = UserManager.GetRoles(userViewModel.system_ID);//Get users by role

            //Get the users status
            var userdata = UserManager.Users.Where(a => a.Id == userViewModel.system_ID).FirstOrDefault();
            ViewBag.status = userdata.EmailConfirmed;

            List<Role> users_role = new List<Role>();
            foreach (var item in users_roles)
            {
                users_role.Add(new Role
                {
                    name = locController.GetLable(item, "Nav1ViewModel", "Index", languageId)
                });
            }
            ViewBag.users_role = users_role;

            bool match = users_roles.Contains("Rittal User");//Check to see if user is a Rittal User
            ViewBag.match_role = match;

            //push the roles into a list
            var roles = roleManager.Roles;
            List<SelectListItem> list_roles = new List<SelectListItem>();
            foreach (var item in roles)
            {
                list_roles.Add(new SelectListItem { Text = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId)[0], Value = locController.GetLable(item.Name, "Nav1ViewModel", "Index", languageId)[1] });
            }

            //set the users theme
            var themes = db.UserThemes;
            List<SelectListItem> Userthemes = new List<SelectListItem>();
            Userthemes.Add(new SelectListItem { Text = locController.GetLable("Select a theme", "UserViewModels", "Edit", languageId)[0], Value = "Select", Selected = true });
            foreach (var theme in themes)
            {
                Userthemes.Add(new SelectListItem { Text = locController.GetLable(theme.theme_name, "UserViewModels", "Edit", languageId)[0], Value = theme.theme_id.ToString() });
            }

            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                //Add admin nav to the list
                var nav2 = db.Nav2ViewModel;

                List<Nav1List> admin_nav = new List<Nav1List>();
                var admin_nav_list = nav2.Where(a => a.usr_group.Contains("1") || a.usr_group.Contains("2"));
                foreach (var item in admin_nav_list)
                {
                    int idn2 = Convert.ToInt32(item.n2ID);
                    admin_nav.Add(new Nav1List { n2id = item.n2ID, n2name = locController.localization("nav2", "n2_nameLong", item.n2_nameLong, idn2, languageId) });
                }
                userViewModel.admin_navs = admin_nav;
                List<Nav1List> special_nav = new List<Nav1List>();
                if (User.IsInRole("Local Admin") && User.IsInRole("Channel User"))
                {
                    //Add other nav to the list
                    var special_nav_list = nav2.Where(a => a.usr_group.Contains("5"));
                    foreach (var item in special_nav_list)
                    {
                        int idn2 = Convert.ToInt32(item.n2ID);
                        special_nav.Add(new Nav1List { n2id = item.n2ID, n2name = locController.localization("nav2", "n2_nameLong", item.n2_nameLong, idn2, languageId) });
                    }
                   
                }
                else
                {
                    //Add other nav to the list
                    var special_nav_list = nav2.Where(a => a.usr_group.Contains("0"));
                    foreach (var item in special_nav_list)
                    {
                        int idn2 = Convert.ToInt32(item.n2ID);
                        special_nav.Add(new Nav1List { n2id = item.n2ID, n2name = locController.localization("nav2", "n2_nameLong", item.n2_nameLong, idn2, languageId) });
                    }
                }
                special_nav.Add(new Nav1List { n2id = -1, n2name = "SPA contracts and rebates" });
                userViewModel.special_navs = special_nav;
            }
            List<long> companyIds = new List<long>();
            if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
            {
                //List companies
                var compdata = db.partnerCompanyViewModels;
                List<Nav1List> complist = new List<Nav1List>();
                foreach (var item in compdata.OrderBy(a => a.comp_name))
                {
                    complist.Add(new Nav1List { id = item.comp_ID, name = item.comp_name });
                    if (item.comp_type == 3 || item.comp_name.ToLower() == "rittal united states")
                    {
                        companyIds.Add(item.comp_ID);
                    }
                }
                userViewModel.list_comp_id = complist;
                //list locations
                var locdata = db.partnerLocationViewModels.Where(a => a.comp_ID == compid);
                List<Nav1List> loc_list = new List<Nav1List>();
                foreach (var item in locdata)
                {
                    if (string.IsNullOrEmpty(item.loc_name))
                    {
                        loc_list.Add(new Nav1List { id = item.loc_ID, name = item.loc_city });
                    }
                    else
                    {
                        loc_list.Add(new Nav1List { id = item.loc_ID, name = item.loc_name + "-" + item.loc_city });
                    }
                }
                userViewModel.list_loc_id = loc_list;
            }

            //set the users state
            var states = dbEntity.data_state.Where(a => a.state_country == "US" || a.state_country == "CA").OrderBy(a => a.state_abbr);
            List<SelectListItem> UserStates = new List<SelectListItem>();
            UserStates.Add(new SelectListItem { Text = locController.GetLable("Select your State", "UserViewModels", "Edit", languageId)[0], Value = "Select", Selected = true });
            foreach (var state in states)
            {
                UserStates.Add(new SelectListItem { Text = state.state_long, Value = state.stateid.ToString() });
            }

            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> UserCountries = new List<SelectListItem>();
            UserCountries.Add(new SelectListItem { Text = locController.GetLable("Select your Country", "Nav1ViewModel", "Index", languageId)[0], Value = "Select", Selected = true });
            foreach (var country in countries.OrderBy(a => a.country_id))
            {
                if (country.country_id != 242)
                {
                    UserCountries.Add(new SelectListItem { Text = locController.GetLable(country.country_long, "Nav1ViewModel", "Index", languageId)[0], Value = country.country_id.ToString() });
                }
            }


            //Apply the page lables to the edit page
            List<Userlabels> userlabels = new List<Userlabels>();
            userlabels.Add(new Userlabels
            {
                usr_fName_label = locController.GetLable("First Name", "UserViewModels", "Edit", languageId),
                usr_lName_label = locController.GetLable("Last Name", "UserViewModels", "Edit", languageId),
                usr_email_label = locController.GetLable("Email", "UserViewModels", "Edit", languageId),
                usr_title_label = locController.GetLable("Title", "UserViewModels", "Edit", languageId),
                usr_add1_label = locController.GetLable("Address 1", "UserViewModels", "Edit", languageId),
                usr_add2_label = locController.GetLable("Address 2", "UserViewModels", "Edit", languageId),
                usr_city_label = locController.GetLable("City", "UserViewModels", "Edit", languageId),
                usr_state_label = locController.GetLable("State", "UserViewModels", "Edit", languageId),
                usr_country_label = locController.GetLable("Country", "UserViewModels", "Edit", languageId),
                usr_language_label = locController.GetLable("Language", "UserViewModels", "Edit", languageId),
                usr_zip_label = locController.GetLable("Zip", "UserViewModels", "Edit", languageId),
                usr_phone_label = locController.GetLable("Phone", "UserViewModels", "Edit", languageId),
                usr_fax_label = locController.GetLable("Fax", "UserViewModels", "Edit", languageId),
                usr_web_label = locController.GetLable("Website", "UserViewModels", "Edit", languageId),
                admin_theme_label = locController.GetLable("Theme", "UserViewModels", "Edit", languageId),
                comp_loc_ID_label = locController.GetLable("User Location", "UserViewModels", "Edit", languageId),
                comp_ID_label = locController.GetLable("User Company", "UserViewModels", "Edit", languageId),
                role_label = locController.GetLable("User Role", "UserViewModels", "Edit", languageId),
                special_navs_label = locController.GetLable("Special Pages", "UserViewModels", "Edit", languageId),
                admin_navs_label = locController.GetLable("Assign Admin Pages", "UserViewModels", "Edit", languageId),
                select_role_label = locController.GetLable("Select a role", "UserViewModels", "Edit", languageId),
                invoke_label = locController.GetLable("Revoke", "UserViewModels", "Edit", languageId),
                password_label = locController.GetLable("Change my Password", "UserViewModels", "Edit", languageId),
                usr_sap_label = locController.GetLable("SAP Account", "UserViewModels", "Edit", languageId),
                usr_jigsaw_login_label = locController.GetLable("Jigsaw Login", "UserViewModels", "Edit", languageId),
                usr_jigsaw_password_label = locController.GetLable("Jigsaw Password", "UserViewModels", "Edit", languageId),
                usr_MDF_login_label = locController.GetLable("MDF Login", "UserViewModels", "Edit", languageId),
                usr_MDF_password_label = locController.GetLable("MDF Password", "UserViewModels", "Edit", languageId),
                usr_spa_label = locController.GetLable("Regional Director SPA/POS", "UserViewModels", "Edit", languageId),
                usr_pos_label = locController.GetLable("Account Manager SPA/POS", "UserViewModels", "Edit", languageId),
                usr_wN_label = locController.GetLable("Weekly Communications", "UserViewModels", "Edit", languageId)
            });
            List<SelectListItem> approver_listing = new List<SelectListItem>() { new SelectListItem { Text = "Select your Approver", Value = "", Selected = true } };
            var approvers = from list in dbEntity.RegionApprovers orderby list.name ascending select list;
            foreach (var item in approvers)//iterate the add function
            {
                approver_listing.Add(new SelectListItem { Text = item.name, Value = item.code });
                if(userViewModel.region_approver == item.code)
                {
                    userViewModel.region_approver_name = item.name;
                }
            }
            approver_listing.Add(new SelectListItem { Text = "None of the above", Value = "" });
            userViewModel.Approver_listings = approver_listing;
            userViewModel.userlabels = userlabels;
            userViewModel.list_country_ids = UserCountries;
            userViewModel.list_state_ids = UserStates;
            userViewModel.list_theme_name = Userthemes;
            userViewModel.list_roles = list_roles;
            userViewModel.approver_companyIds = companyIds;
            if (userViewModel == null)
            {
                return HttpNotFound();
            }

            return View(userViewModel);
        }

        // POST: UserViewModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "usr_ID,usr_fName,usr_lName,usr_title,usr_email,usr_add1,usr_add2,usr_city,usr_state,usr_country,usr_language,usr_zip,usr_phone,usr_fax,usr_web,admin_theme,comp_ID,comp_loc_ID,system_ID,role,usr_pages,usr_jigsaw_login,usr_jigsaw_password,usr_MDF_login,usr_MDF_password,usr_role,usr_siteRole,usr_password,usr_MDF,usr_project_reg,usr_SPA,usr_POS,wN,usr_sales,access_request,region_approver,old_approver")] UserViewModel userViewModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var locController = new CommonController();
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            //var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new RisourceCenterContext()));

            if (ModelState.IsValid)
            {
                if (userViewModel.system_ID == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                //set the users roles
                if (userViewModel.role != null)
                {
                    if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
                    {
                        try
                        {
                            UserManager.AddToRole(userViewModel.system_ID, userViewModel.role);
                            db.SaveChanges();
                        }
                        catch (HttpException e)
                        {
                            var msg = e.Message;
                        }
                    }
                }

                userViewModel.usr_pages = Request.Form["usr_pages"];
                userViewModel.interlynx_user = false;
                if (!string.IsNullOrEmpty(userViewModel.usr_pages) && userViewModel.usr_pages.Contains("-1"))
                    userViewModel.interlynx_user = true;

                if (User.IsInRole("Super Admin") || User.IsInRole("Local Admin") || User.IsInRole("Global Admin"))
                {
                    try
                    {
                        //set the company ID
                        int compid = Convert.ToInt32(Request.Form["comp_ID"]);
                        var compdata = db.partnerCompanyViewModels.Where(a => a.comp_ID == compid).FirstOrDefault();
                        userViewModel.comp_ID = compid;
                        userViewModel.comp_loc_ID = Convert.ToInt32(Request.Form["comp_loc_ID"]);
                        //use the company id to set the default location
                        //if (string.IsNullOrEmpty(Request.Form["comp_loc_ID"]) || compid != Convert.ToInt32(Session["companyId"]))
                        //{
                        //    long locid = db.partnerLocationViewModels.Where(a => a.comp_ID == compid).Take(1).FirstOrDefault().loc_ID;
                        //    userViewModel.comp_loc_ID = locid;
                        //}
                    }
                    catch (HttpException e)
                    {
                        var msg = e.Message;
                    }
                }

                db.Entry(userViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (!string.IsNullOrEmpty(userViewModel.region_approver) && userViewModel.region_approver != userViewModel.old_approver)
                {
                    usr_user user = dbEntity.usr_user.Where(x => x.usr_ID == userViewModel.usr_ID).FirstOrDefault();
                    if(user != null)
                    {
                        await new AccountController().CreateStoreFrontUser(user, "");
                    }
                }

                if (Request.Form["usr_email"] == User.Identity.GetUserName())
                {
                    //Change the users country id in the session since they have 
                    Session["userLanguageId"] = Request.Form["usr_language"];
                    Session["userCountryId"] = Request.Form["usr_country"];
                }

                //Log the action by the user
                await commCtl.siteActionLog(0, "UserViewModels", DateTime.Now, userId + " edited user " + userViewModel.usr_email, "Edit", Convert.ToInt32(userId));

                if (Request.QueryString["n2_name"] == "home")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if ((Request.QueryString["tool"] == "MDF"))
                    {
                        return RedirectToAction("MDFusers", "MDFView", new { compid = Request.QueryString["compid"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"] });
                    }
                    else
                    {
                        return RedirectToAction("Edit", "UserViewModels", new { id = Request.Form["usr_ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], compid = Request.QueryString["compid"], tool = Request.Form["tool"] });
                    }

                }
            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return View(userViewModel);
        }
        #endregion

        #region Delete Role
        // GET: UserViewModels/Delete/5
        public async Task<ActionResult> DeleteRole(int? id = 0, string systemID = null, string role = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new RisourceCenterContext()));
            UserManager.RemoveFromRole(systemID, role);

            //Log the action by the user
            await commCtl.siteActionLog(0, "UserViewModels", DateTime.Now, " This role " + role + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Edit", "UserViewModels", new { id = id, n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], compid = Request.QueryString["compid"] });
        }

        // GET: UserViewModels/Delete/5
        [HttpDelete]
        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }

                if (id == null)
                {
                    return Json("An error occurred while processing your request");
                }
                UserViewModel userViewModel = await db.UserViewModels.FindAsync(id);
                if (userViewModel == null)
                {
                    return Json("An error occurred while processing your request");
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
                await commCtl.siteActionLog(0, "UserViewModels", DateTime.Now, " The user id =" + userEmail + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));
                return Json("Ok");
            }
            catch(Exception ex)
            {
                return Json(ex.Message);
            }
        }

        // POST: UserViewModels/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    UserViewModel userViewModel = await db.UserViewModels.FindAsync(id);
        //    db.UserViewModels.Remove(userViewModel);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
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

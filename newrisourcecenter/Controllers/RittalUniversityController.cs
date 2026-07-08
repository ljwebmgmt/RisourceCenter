using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using newrisourcecenter.Internals;
using newrisourcecenter.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class RittalUniversityController : Controller
    {
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: RittalUniversity
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string companyType = Convert.ToString(Session["companyType"]);
            string userProducts = Convert.ToString(Session["userProducts"]);
            //get nav 1 description
            var page_text = dbEntity.nav1.Where(a=>a.n1ID== 2).FirstOrDefault();
            ViewBag.page_desc = page_text.n1_descLong;
            //Get all courses
            var myClassDataSet = dbEntity.nav2.Join(
                                          dbEntity.nav3,
                                          nav2data => nav2data.n2ID,
                                          nav3data => nav3data.n2ID,
                                          (nav2data,nav3data) => new {nav2data,nav3data }
                                      ).Where(a => a.nav2data.n1ID == 2 && a.nav2data.n2_active == 1 && a.nav2data.n2_usrTypes.Contains(companyType) && a.nav2data.n2ID != 64 && a.nav2data.n2ID != 65 && a.nav2data.n2ID != 12 && a.nav2data.n2ID != 13 );
            //List of courses
            List<Nav2ViewModel> courses_list = new List<Nav2ViewModel>();
            //List classes
            List<Nav3ViewModel> list_classes = new List<Nav3ViewModel>();

            if (userIndustry != "3")
            {
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav2data.n2ID, a.nav2data.n2_nameLong, a.nav2data.n2_industry, a.nav2data.n2_usrTypes,a.nav2data.n2_products }).Where(a => a.Key.n2_usrTypes.Contains(companyType)))
                {
                    //filter by product type && a.Key.n2_industry.Contains(userIndustry)
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n2_products.Contains(products))
                            {
                                courses_list.Add(new Nav2ViewModel { n2ID = item.Key.n2ID, n2_nameLong = item.Key.n2_nameLong });
                                break;
                            }
                        }
                    }
                }

                //List of classes
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav3data.n3ID, a.nav3data.n3_nameLong, a.nav2data.n2ID, a.nav3data.file_name, a.nav3data.n3_active, a.nav3data.n3_industry, a.nav3data.n3_usrTypes, a.nav3data.n3_products, a.nav3data.n3order }).Where(a => a.Key.n3_active == 1 && a.Key.n3_usrTypes.Contains(companyType) && a.Key.n3_industry.Contains(userIndustry)).OrderBy(a => a.Key.n3order))
                {
                    //filter by product type
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n3_products.Contains(products))
                            {
                                list_classes.Add(new Nav3ViewModel { n2ID = item.Key.n2ID, n3ID = item.Key.n3ID, n3_nameLong = item.Key.n3_nameLong, file_name = item.Key.file_name });
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav2data.n2ID, a.nav2data.n2_nameLong, a.nav2data.n2_industry, a.nav2data.n2_usrTypes, a.nav2data.n2_products }).Where(a => a.Key.n2_usrTypes.Contains(companyType)))
                {
                    //filter by product type
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n2_products.Contains(products))
                            {
                                courses_list.Add(new Nav2ViewModel { n2ID = item.Key.n2ID, n2_nameLong = item.Key.n2_nameLong });
                                break;
                            }
                        }
                    }
                }

                //List of classes
                foreach (var item in myClassDataSet.GroupBy(a => new { a.nav3data.n3ID, a.nav3data.n3_nameLong, a.nav2data.n2ID, a.nav3data.file_name, a.nav3data.n3_active, a.nav3data.n3_industry, a.nav3data.n3_usrTypes, a.nav3data.n3_products, a.nav3data.n3order }).Where(a => a.Key.n3_active == 1 && a.Key.n3_usrTypes.Contains(companyType)).OrderBy(a => a.Key.n3order))
                {
                    //filter by product type
                    if (userProducts != null)
                    {
                        foreach (var products in userProducts.ToArray())
                        {
                            if (item.Key.n3_products.Contains(products))
                            {
                                list_classes.Add(new Nav3ViewModel { n2ID = item.Key.n2ID, n3ID = item.Key.n3ID, n3_nameLong = item.Key.n3_nameLong, file_name = item.Key.file_name });
                                break;
                            }
                        }
                    }
                }
            }

            //Get Announcements from Sales Communications
            var salesComm = db.SalesCommViewModels;
            List<SalesCommViewModel> list_announcements = new List<SalesCommViewModel>();
            foreach (var courses in salesComm)
            {
                list_announcements.Add( new SalesCommViewModel {
                    scID = courses.scID,
                    n2ID = courses.n2ID,
                    n3ID = courses.n3ID,
                    sc_headline = courses.sc_headline,
                    sc_body = courses.sc_body,
                    sc_startDate=courses.sc_startDate,
                    sc_teaser = courses.sc_teaser
                });
            }

            //Get user classess taken
            var myClassData = db.rittalUniversityViewModels.Where(a => a.tr_usr == userId);
            //get unique data from my data
            List<Uniclasses> uniqueMyClassData = new List<Uniclasses>();
            foreach (var data in myClassData.GroupBy(a=>new { a.tr_module,a.tr_usr }))
            {
                uniqueMyClassData.Add( new Uniclasses { id=data.Key.tr_usr,name=data.Key.tr_module});
            }
            //Add user classes to object
            List<RittalUniversityViewModels> rittalViewModel = new List<RittalUniversityViewModels>();
            if ( uniqueMyClassData.Count() != 0 ) {
                foreach (var classdata in uniqueMyClassData)
                {
                    var mytestClass = myClassData.Where(a => a.tr_usr == classdata.id && a.tr_module == classdata.name).OrderBy(a => a.tr_date).ToList().LastOrDefault();
                    ViewBag.test = mytestClass;
                    //Add the unique class with the latest data
                    rittalViewModel.Add(new RittalUniversityViewModels
                    {
                        trid = mytestClass.trid,
                        tr_usr = mytestClass.tr_usr,
                        tr_date = mytestClass.tr_date,
                        tr_module = mytestClass.tr_module,
                        tr_NumQuestions = mytestClass.tr_NumQuestions,
                        tr_PassGrade = mytestClass.tr_PassGrade,
                        tr_score = mytestClass.tr_score,
                        list_courses = courses_list,
                        list_classes = list_classes,
                        list_announcements= list_announcements
                    });
                }
            }
            else
            {
                //Add the unique class with the latest data
                rittalViewModel.Add(new RittalUniversityViewModels
                {
                    trid = 0,
                    tr_usr = 0,
                    tr_date = DateTime.Now,
                    tr_module = "You have not take a test",
                    tr_NumQuestions = "10",
                    tr_PassGrade = "80",
                    tr_score = "0",
                    list_courses = courses_list,
                    list_classes = list_classes,
                    list_announcements = list_announcements
                });
            }

            return View(rittalViewModel);
        }

        // GET: RittalUniversity
        public ActionResult Courses()
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string companyType = Convert.ToString(Session["companyType"]);
            string userProducts = Convert.ToString(Session["userProducts"]);

            var page_text = dbEntity.nav1.Where(a => a.n1ID == 2).FirstOrDefault();
            ViewBag.page_desc = page_text.n1_descLong;

            IQueryable<nav2> nav2 = dbEntity.nav2.Where(a => a.n1ID == 2 && a.n2_active == 1 && a.n2_usrTypes.Contains(companyType) && a.n2ID!= 64 && a.n2ID!=65);
            //filter by product type
            if (userProducts != null)
            {
                foreach (var products in userProducts.ToArray())
                {
                    nav2.Where(a => a.n2_products.Contains(products));
                }
            }
            else
            {
                nav2.Where(a => a.n2_products == "999");
            }
            if (userIndustry == "3")
            {
                return View(nav2.OrderBy(a => a.n2order));
            }
            else
            {
                return View(nav2.OrderBy(a => a.n2order).Where(a => a.n2_usrTypes.Contains(userIndustry)));
            }
        }

        // GET: RittalUniversity/Allclasses
        public ActionResult Allclasses(int childId = 0)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string companyType = Convert.ToString(Session["companyType"]);
            string userProducts = Convert.ToString(Session["userProducts"]);

            //Add n1ID to the list of n1IDs for the drop down
            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1);
            List<Nav1List> list_n1ID = new List<Nav1List>();
            foreach (var n11dsitems in n1ids)
            {
                var n2ids = dbEntity.nav2.Where(a => a.n1ID == n11dsitems.n1ID && a.n2ID != 12 && a.n2ID != 13 && a.n2ID != 64 && a.n2ID != 65 && a.n2_active == 1 && a.n1ID==2);

                foreach (var n12dsitems in n2ids)
                {
                    list_n1ID.Add(new Nav1List { id = n11dsitems.n1ID, name = n11dsitems.n1_nameLong, n2id = n12dsitems.n2ID, n2name = n12dsitems.n2_nameLong });
                }
            }
            ViewBag.list_n1ID = list_n1ID;

            IQueryable<Nav3ViewModel> nav3;
            if (childId==65)
            {
               nav3 = db.Nav3ViewModel.Where(a => a.n2ID == 14 );

            }
            else
            {
               nav3 = db.Nav3ViewModel.Where(a => a.n2ID == childId);

            }

            return View(nav3.OrderByDescending(a => a.n3ID));

        }

        // GET: SalesComm
        public async Task<ActionResult> AllSalesComms(int parentID = 2, int childId = 0, string n2_name = null, int n3id = 0, string n3_name = null)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.n2_name = n2_name;
            ViewBag.n3_name = n3_name;

            //Add n1ID to the list of n1IDs for the drop down
            var n1ids = dbEntity.nav1.Where(a => a.n1_active == 1 && a.n1ID==2);
            List<Nav1List> list_n1ID = new List<Nav1List>();
            foreach (var n11dsitems in n1ids)
            {
                var n2ids = dbEntity.nav2.Where(a=>a.n1ID==2 && a.n2_active==1 && a.usr_group==null);

                foreach (var n12dsitems in n2ids)
                {
                    list_n1ID.Add(new Nav1List { id = n11dsitems.n1ID, name = n11dsitems.n1_nameLong, n2id = n12dsitems.n2ID, n2name = n12dsitems.n2_nameLong });
                }
            }
            ViewBag.list_n1ID = list_n1ID;

            var n2idsData = dbEntity.nav2.Where(a => a.n1ID == 2 && a.n2_active == 1 && a.usr_group == null);
            List<SalesCommViewModel> salesComData = new List<SalesCommViewModel>();
            foreach (var n12dsitems in n2idsData)
            {
                var salesCom = await dbEntity.salesComms.Where(a => a.n2ID == n12dsitems.n2ID ).ToListAsync();
                if( salesCom.Count()!=0 )
                {
                    foreach (var item in salesCom)
                    {
                        salesComData.Add(new SalesCommViewModel
                        {
                            scID = item.scID,
                            sc_headline = item.sc_headline,
                            sc_startDate = item.sc_startDate,
                            sc_endDate = item.sc_endDate,
                            n2ID = item.n2ID,
                            n3ID = item.n3ID
                        });
                    }
                }
            }

            //return View(salesComData.OrderByDescending(a => a.scID).ToList());

            if (childId == 66)
            {
                return View(salesComData.OrderByDescending(a => a.scID).ToList());
            }
            else
            {
                return View(salesComData.OrderByDescending(a => a.scID).Where(a => a.n2ID == childId).ToList());
            }
                      
        }

        // GET: RittalUniversity/clases
        public ActionResult Classes(int childId = 0)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string userIndustry = Convert.ToString(Session["userIndustry"]);
            string companyType = Convert.ToString(Session["companyType"]);
            string userProducts = Convert.ToString(Session["userProducts"]);

            IQueryable<nav3> nav3 = dbEntity.nav3.Where(a => a.n2ID == childId && a.n3_active == 1 && a.n3_usrTypes.Contains(companyType));
            var page_text = dbEntity.nav2.Where(a => a.n2ID == childId).FirstOrDefault();
            ViewBag.page_desc = page_text.n2_descLong;

            //filter by product type
            if (userProducts != null)
            {
                foreach (var products in userProducts.ToArray())
                {
                    nav3.Where(a => a.n3_products.Contains(products));
                }
            }
            else
            {
                nav3.Where(a => a.n3_products == "999");
            }

            if (userIndustry == "3")
            {
                return View(nav3.OrderBy(a => a.n3order));
            }
            else
            {
                return View(nav3.OrderBy(a => a.n3order).Where(a => a.n3_usrTypes.Contains(userIndustry)));
            }
        }

        // GET: RittalUniversity/clases
        [HttpGet]
        public ActionResult Results(bool export = false)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            DataTable dt = new DataTable();
            List<RittalUniversityViewModels> rittalU = new List<RittalUniversityViewModels>();
            var myClassData = dbEntity.stats_training.Join(
                                                            dbEntity.usr_user,
                                                            training => training.tr_usr,
                                                            userdata => userdata.usr_ID,
                                                            (training, userdata) => new {training, userdata }
                                                           ).Join(
                                                              dbEntity.partnerCompanies,
                                                              usrs =>usrs.userdata.comp_ID,
                                                              comp=> comp.comp_ID,
                                                              (usrs, comp) =>new {usrs, comp }
                                                          );

            //if export is true
            if (export) {
                //set export columns
                dt.Columns.Add("TestId", typeof(Int32));
                dt.Columns.Add("Class Name", typeof(string));
                dt.Columns.Add("User Name", typeof(string));
                dt.Columns.Add("Date Created", typeof(string));
                dt.Columns.Add("Passing Grade", typeof(string));
                dt.Columns.Add("Score", typeof(string));
                dt.Columns.Add("Email", typeof(string));
            }

            foreach (var item in myClassData.OrderBy(a => a.usrs.training.tr_date).Take(50))
            {
                string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                rittalU.Add(new RittalUniversityViewModels
                {
                    trid = item.usrs.training.trid,
                    UserName = name,
                    tr_module = item.usrs.training.tr_module,
                    tr_date = item.usrs.training.tr_date,
                    tr_NumQuestions = item.usrs.training.tr_module,
                    tr_score = item.usrs.training.tr_score,
                    tr_PassGrade = item.usrs.training.tr_PassGrade,
                    usr_email = item.usrs.userdata.usr_email,
                });
                //if export is true
                if (export) {
                    dt.Rows.Add(
                        "" + item.usrs.training.trid + "",
                        "" + item.usrs.training.tr_module + "",
                        "" + name + "",
                        "" + item.usrs.training.tr_date + "",
                        "" + item.usrs.training.tr_PassGrade + "",
                        "" + item.usrs.training.tr_score + "",
                        "" + item.usrs.userdata.usr_email + ""
                    );
                }
            }
            //if export is true
            if (export)
            {
                WriteExcelWithNPOI(dt, "xlsx");
            }

            //Users
            List<Nav1List> users_list = new List<Nav1List>();
            foreach (var item in myClassData.GroupBy(a=>new { a.usrs.userdata.usr_ID,a.usrs.userdata.usr_fName,a.usrs.userdata.usr_lName}).OrderBy(a => a.Key.usr_lName))
            {
                users_list.Add(new Nav1List { id = item.Key.usr_ID, name = item.Key.usr_lName+" "+ item.Key.usr_fName });
            }
            ViewBag.users = users_list.OrderBy(a => a.name);

            //companies
            List<Nav1List> company_list = new List<Nav1List>();
            foreach (var item in myClassData.GroupBy(a => new { a.comp.comp_ID, a.comp.comp_name}).OrderBy(a => a.Key.comp_name))
            {
                company_list.Add(new Nav1List { id = item.Key.comp_ID, name = item.Key.comp_name });
            }
            ViewBag.companies = company_list;

            //classes
            List<Nav1List> classes_list = new List<Nav1List>();
            foreach (var item in myClassData.GroupBy(a => new { a.usrs.training.tr_module }).OrderBy(a => a.Key.tr_module))
            {
                classes_list.Add(new Nav1List { name=item.Key.tr_module});
            }
            ViewBag.classess = classes_list;
             
            return View(rittalU);
        }

        public ActionResult Filterresults(string className=null, int userid=0, int companyid=0, int count=1000000, int date=0, bool export = false,int location=0,string search=null)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            DataTable dt = new DataTable();
            DateTime dateRanage = DateTime.Today.AddDays(- date);//get date range
            List<RittalUniversityViewModels> rittalU = new List<RittalUniversityViewModels>();
            var myClassData = dbEntity.stats_training.Join(
                                                            dbEntity.usr_user,
                                                            training => training.tr_usr,
                                                            userdata => userdata.usr_ID,
                                                            (training, userdata) => new { training, userdata }
                                                           ).Join(
                                                              dbEntity.partnerCompanies,
                                                              usrs => usrs.userdata.comp_ID,
                                                              comp => comp.comp_ID,
                                                              (usrs, comp) => new { usrs, comp }
                                                           ).OrderBy(a => a.usrs.training.tr_date);
            //if export is true
            if (export)
            {
                //set export columns
                dt.Columns.Add("TestId", typeof(Int32));
                dt.Columns.Add("Class Name", typeof(string));
                dt.Columns.Add("User Name", typeof(string));
                dt.Columns.Add("Date Created", typeof(string));
                dt.Columns.Add("Passing Grade", typeof(string));
                dt.Columns.Add("Score", typeof(string));
                dt.Columns.Add("Email", typeof(string));
            }

            if (className != null)
            {
                foreach (var item in myClassData.Where(a => a.usrs.training.tr_module == className).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                }
            }

            if (location != 0)
            {
                foreach (var item in myClassData.Where(a => a.usrs.userdata.usr_country==location).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                }
            }

            if (userid != 0)
            {
                foreach (var item in myClassData.Where(a => a.usrs.training.tr_usr == userid).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                }
            }

            if (companyid != 0)
            {
                foreach (var item in myClassData.Where(a => a.comp.comp_ID == companyid).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                }
            }

            if (date!=0)
            {
                foreach (var item in myClassData.Where(a => a.usrs.training.tr_date >= dateRanage && a.usrs.training.tr_date <= DateTime.Today || a.usrs.training.tr_date == null).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                } 
            }

            if (search != null)
            {
                
                foreach (var item in myClassData.Where(a => a.usrs.training.tr_module == search || (a.usrs.userdata.usr_fName+ " " +a.usrs.userdata.usr_lName).Contains(search) || a.usrs.userdata.usr_email.Contains(search)).Take(count))
                {
                    string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                    rittalU.Add(new RittalUniversityViewModels
                    {
                        trid = item.usrs.training.trid,
                        UserName = name,
                        tr_module = item.usrs.training.tr_module,
                        tr_date = item.usrs.training.tr_date,
                        tr_NumQuestions = item.usrs.training.tr_module,
                        tr_score = item.usrs.training.tr_score,
                        tr_PassGrade = item.usrs.training.tr_PassGrade,
                        usr_email = item.usrs.userdata.usr_email,
                    });
                    //if export is true
                    if (export)
                    {
                        dt.Rows.Add(
                            "" + item.usrs.training.trid + "",
                            "" + item.usrs.training.tr_module + "",
                            "" + name + "",
                            "" + item.usrs.training.tr_date + "",
                            "" + item.usrs.training.tr_PassGrade + "",
                            "" + item.usrs.training.tr_score + "",
                            "" + item.usrs.userdata.usr_email + ""
                        );
                    }
                }
            }

            if ( companyid == 0 && userid == 0 && className == null && date==0 && location==0 && search==null)
            {
                if (count == 1000000)
                {
                    foreach (var item in myClassData)
                    {
                        string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                        rittalU.Add(new RittalUniversityViewModels
                        {
                            trid = item.usrs.training.trid,
                            UserName = name,
                            tr_module = item.usrs.training.tr_module,
                            tr_date = item.usrs.training.tr_date,
                            tr_NumQuestions = item.usrs.training.tr_module,
                            tr_score = item.usrs.training.tr_score,
                            tr_PassGrade = item.usrs.training.tr_PassGrade,
                            usr_email = item.usrs.userdata.usr_email,
                        });
                        //if export is true
                        if (export)
                        {
                            dt.Rows.Add(
                                "" + item.usrs.training.trid + "",
                                "" + item.usrs.training.tr_module + "",
                                "" + name + "",
                                "" + item.usrs.training.tr_date + "",
                                "" + item.usrs.training.tr_PassGrade + "",
                                "" + item.usrs.training.tr_score + "",
                                "" + item.usrs.userdata.usr_email + ""
                            );
                        }
                    }
                }
                else
                {
                    foreach (var item in myClassData.Take(count))
                    {
                        string name = item.usrs.userdata.usr_fName + " " + item.usrs.userdata.usr_lName;
                        rittalU.Add(new RittalUniversityViewModels
                        {
                            trid = item.usrs.training.trid,
                            UserName = name,
                            tr_module = item.usrs.training.tr_module,
                            tr_date = item.usrs.training.tr_date,
                            tr_NumQuestions = item.usrs.training.tr_module,
                            tr_score = item.usrs.training.tr_score,
                            tr_PassGrade = item.usrs.training.tr_PassGrade,
                            usr_email = item.usrs.userdata.usr_email,
                        });
                        //if export is true
                        if (export)
                        {
                            dt.Rows.Add(
                                "" + item.usrs.training.trid + "",
                                "" + item.usrs.training.tr_module + "",
                                "" + name + "",
                                "" + item.usrs.training.tr_date + "",
                                "" + item.usrs.training.tr_PassGrade + "",
                                "" + item.usrs.training.tr_score + "",
                                "" + item.usrs.userdata.usr_email + ""
                            );
                        }
                    }
                }
            }

            //if export is true
            if (export)
            {
                WriteExcelWithNPOI(dt, "xlsx");
            }

            return View(rittalU);
        }

        // GET: RittalUniversity/Details/5
        public ActionResult Test(string dir_name)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.dir_name = dir_name;
            
            return View();
        }

        //POST: /RittalUniversity/score
        public ActionResult score()
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //ViewBag.dir_name = dir_name;

            return View();
        }

        //POST: /RittalUniversity/score
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult printCert()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            RittalUniversityViewModels model = new RittalUniversityViewModels();
            //ViewBag.dir_name = dir_name;
            model.tr_module = Request.Form["tr_module"];
            model.tr_date = Convert.ToDateTime(Request.Form["tr_date"]);
            model.tr_score = Request.Form["tr_score"];

            return View(model);
        }

        // GET: RittalUniversity/Create
        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var nav2 = dbEntity.nav2.Where(a=>a.n1ID==2 && a.n2_active==1 && a.n2ID!=64 && a.n2ID!=65);
            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;

            List<SelectListItem> list_n2IDs = new List<SelectListItem>();
            list_n2IDs.Add(new SelectListItem { Text = "Select Level 2 Nav Item", Value = "select", Selected = true });
            foreach (var items in nav2)
            {

                    list_n2IDs.Add(new SelectListItem { Text = items.n2_nameLong, Value = items.n2ID.ToString() });
                
            }

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            var Nav3ViewModel = new Nav3ViewModel();
            {
                Nav3ViewModel.list_n2ID = list_n2IDs;
                Nav3ViewModel.list_Type = list_types;
                Nav3ViewModel.list_industry = list_industry;
                Nav3ViewModel.list_products = list_products;
            };

            return View(Nav3ViewModel);
        }

        // POST: RittalUniversity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "n3ID,n2ID,n3order,n3_nameLong,n3_descLong,n3_active,n3_products,n3_usrTypes,n3_editBy,n3_editDate,n3_redirect,n3_industry,file_name,attachment")] Nav3ViewModel nav3ViewModel, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                //attach a file to the risources
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    var guid = Request.Form["n3_nameLong"];
                    var file = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/classes/tmp"), file);
                    string extractPath = Server.MapPath("~/classes/");
                    nav3ViewModel.file_name = fileName.Split('.')[0];

                    attachment.SaveAs(path);
                    using (ZipArchive archive = ZipFile.OpenRead(path))
                    {
                        archive.ExtractToDirectory(extractPath, true);
                    }
                    //ZipFile.ExtractToDirectory(path, extractPath);
                    //using (var unzip = new Unzip(path))
                    //{
                    //    unzip.ExtractToDirectory(extractPath);
                    //}
                }

                nav3ViewModel.n3_products = Request.Form["n3_products"];
                nav3ViewModel.n3_usrTypes = Request.Form["n3_usrTypes"];
                nav3ViewModel.n3_industry = Request.Form["n3_industry"];

                db.Nav3ViewModel.Add(nav3ViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("AllClasses", new { childId = Request.Form["childId"], parentid = Request.Form["parentid"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"] });
            }

            return View(nav3ViewModel);
        }

        // GET: RittalUniversity/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            return RedirectToAction("Index", "RittalUniversity");
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);

            long? n2ID = nav3ViewModel.n2ID;
            var n2ids = dbEntity.nav2.Where(a => a.n2_active == 1);

            //Get the name of the sub menu
            var n2data = n2ids.Where(a => a.n2ID == n2ID);
            ViewBag.n2_name = n2data.FirstOrDefault().n2_nameLong;

            //Get n1ID of Item
            int? n1ID = n2data.FirstOrDefault().n1ID;

            //Get the name of the top menu
            var n1data = dbEntity.nav1.Where(a => a.n1_active == 1).Where(a => a.n1ID == n2data.FirstOrDefault().n1ID);
            ViewBag.n1_name = n1data.FirstOrDefault().n1_nameLong;

            var partnerType = dbEntity.partnerTypes;
            var partnerProducts = dbEntity.partnerProducts;
            var partnerIndustry = dbEntity.partnerIndustries;

            //Add n1ID to the list of n1IDs for the drop down
            List<SelectListItem> list_n2ID = new List<SelectListItem>();
            list_n2ID.Add(new SelectListItem { Text = "Select Top Nav", Value = "select", Selected = true });

            foreach (var items in n2ids.Where(a => a.n1ID == n1ID))
            {
                if (items.n2ID != 64)
                {
                    list_n2ID.Add(new SelectListItem { Text = items.n2_nameLong, Value = items.n2ID.ToString() });
                }
            }

            //Add partner type to the list of types for the drop down
            List<partnerType> list_types = new List<partnerType>();
            foreach (var items in partnerType)
            {
                list_types.Add(new partnerType { pt_ID = items.pt_ID, pt_type = items.pt_type });
            }

            //Add partner producst to the list of types for the drop down
            List<partnerProduct> list_products = new List<partnerProduct>();
            foreach (var items in partnerProducts)
            {
                list_products.Add(new partnerProduct { pp_ID = items.pp_ID, pp_product = items.pp_product });
            }

            //Add partner industry to the list of types for the drop down
            List<partnerIndustry> list_industry = new List<partnerIndustry>();
            foreach (var items in partnerIndustry)
            {
                list_industry.Add(new partnerIndustry { pi_ID = items.pi_ID, pi_industry = items.pi_industry });
            }

            nav3ViewModel.list_industry = list_industry;
            nav3ViewModel.list_products = list_products;
            nav3ViewModel.list_Type = list_types;
            nav3ViewModel.list_n2ID = list_n2ID;

            if (nav3ViewModel == null)
            {
                return HttpNotFound();
            }

            return View(nav3ViewModel);
        }

        // POST: RittalUniversity/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "n3ID,n2ID,n3order,n3_nameLong,n3_descLong,n3_active,n3_products,n3_usrTypes,n3_editBy,n3_editDate,n3_redirect,n3_industry,file_name,attachment")] Nav3ViewModel nav3ViewModel, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                //attach a file to the risources
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    var guid = Request.Form["n3_nameLong"];
                    var file = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/classes/tmp"), file);
                    string extractPath = Server.MapPath("~/classes/");
                    nav3ViewModel.file_name = fileName.Split('.')[0];

                    attachment.SaveAs(path);
                    using (var unzip = new Unzip(path))
                    {
                        unzip.ExtractToDirectory(extractPath);
                    }
                }

                nav3ViewModel.n3_products = Request.Form["n3_products"];
                nav3ViewModel.n3_usrTypes = Request.Form["n3_usrTypes"];
                nav3ViewModel.n3_industry = Request.Form["n3_industry"];

                db.Entry(nav3ViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("AllClasses", new { childId = Request.Form["childId"],parentid= Request.Form["parentid"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"] });
            }

            return View(nav3ViewModel);
        }

        // GET: Nav3ViewModel/Delete/5
        public async Task<ActionResult> Delete(long? id)
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
            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);
            if (nav3ViewModel == null)
            {
                return HttpNotFound();
            }
            db.Nav3ViewModel.Remove(nav3ViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("AllClasses", new { childId = Request.QueryString["childId"], parentid = Request.QueryString["parentid"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"],msg= Request.QueryString["msg"] });
        }

        //Use this method to export .XSL & .XLSX file formats
        public void WriteExcelWithNPOI(DataTable dt, String extension)
        {

            IWorkbook workbook;

            if (extension == "xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (extension == "xls")
            {
                workbook = new HSSFWorkbook();
            }
            else
            {
                throw new Exception("This format is not supported");
            }

            ISheet sheet1 = workbook.CreateSheet("Sheet 1");

            //make a header row
            IRow row1 = sheet1.CreateRow(0);

            for (int j = 0; j < dt.Columns.Count; j++)
            {

                ICell cell = row1.CreateCell(j);
                String columnName = dt.Columns[j].ToString();
                cell.SetCellValue(columnName);
            }

            //loops through data
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(dt.Rows[i][columnName].ToString());
                }
            }

            using (var exportData = new MemoryStream())
            {
                Response.Clear();
                workbook.Write(exportData);
                if (extension == "xlsx") //xlsx file format
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "RittalUnResults.xlsx"));
                    Response.BinaryWrite(exportData.ToArray());
                }
                else if (extension == "xls")  //xls file format
                {
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "RittalUnResults.xls"));
                    Response.BinaryWrite(exportData.GetBuffer());
                }
                Response.End();
            }
        }

        // POST: Nav3ViewModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            Nav3ViewModel nav3ViewModel = await db.Nav3ViewModel.FindAsync(id);
            db.Nav3ViewModel.Remove(nav3ViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Training()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            RittalUniversityViewModels model = new RittalUniversityViewModels();
            model.trainingTracks = await dbEntity.trainingTracks.ToListAsync();
            model.trainingTrackNames = model.trainingTracks.Select(x => x.name).ToList();
            List<trainingClass> trainingClasses = await dbEntity.trainingClasses.ToListAsync();
            Dictionary<string,List<trainingClass>> classes = new Dictionary<string,List<trainingClass>>();
            foreach(var trainingClass in trainingClasses)
            {
                if(!classes.ContainsKey(trainingClass.category))
                {
                    classes[trainingClass.category] = new List<trainingClass>();
                }
                classes[trainingClass.category].Add(trainingClass);
            }
            model.trainingClasses = classes;
            model.usrClasses = await dbEntity.usrClasses.Where(x => x.usr_ID == userId).ToDictionaryAsync(x => x.class_ID, x => x);
            ViewBag.userId = userId;
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> MarkClassComplete(int id)
        {
            var response = new { Message = "", Success = true };
            try
            {
                usrClass obj = await dbEntity.usrClasses.Where(x => x.id == id).FirstOrDefaultAsync();
                bool insert = false;
                if(obj == null)
                {
                    obj = new usrClass();
                    insert = true;
                }
                obj.usr_ID = Convert.ToInt32(Session["userId"]);
                obj.class_ID = id;
                obj.completed = 1;
                obj.completed_at = DateTime.Now;
                if(insert)
                {
                    dbEntity.usrClasses.Add(obj);
                }
                else
                {
                    dbEntity.Entry(obj).State = EntityState.Modified;
                }
                await dbEntity.SaveChangesAsync();
                response = new { Message = "Class marked as completed", Success = true };
            }
            catch(Exception ex)
            {
                response = new { Message = ex.Message, Success = false };
            }
            return Json(response);
        }

        [HttpPost]
        public JsonResult CertifyComplete(string track)
        {
            var response = new { Message = "", Success = true };
            try
            {
                string html = Session["firstName"].ToString() + " " + Session["lastName"].ToString() + " has marked a training track completed, details are given below: "; 
                html += "<br/><br/><strong>First Name: </strong>" + Session["firstName"].ToString();
                html += "<br/><strong>Last Name: </strong>" + Session["lastName"].ToString();
                html += "<br/><strong>Email Address: </strong>" + Session["userEmail"].ToString();
                html += "<br/><strong>Company: </strong>" + Session["comp_name"].ToString();
                html += "<br/><strong>Location: </strong>" + Session["location_name"].ToString();
                html += "<br/><strong>Training Track: </strong><span style='text-transform: capitalize;'>" + track + "</span>";
                CommonController commCtl = new CommonController();
                commCtl.email("webmaster@rittal.us", "channel@rittal.us", "Rittal University's Training Track Completion Notice", html);
                response = new { Message = "Training Track marked completed successfully", Success = true };
            }
            catch (Exception ex)
            {
                response = new { Message = ex.Message, Success = false };
            }
            return Json(response);
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

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
using System.Text;
using System.Net.Mail;
using System.Web.WebPages.Html;
using SelectListItem = System.Web.Mvc.SelectListItem;
using System.Configuration;
using System.Text.RegularExpressions;
using NPOI.HPSF;

namespace newrisourcecenter.Controllers
{
    public partial class RFQNewToolController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        private readonly List<string> waitStatus = new List<string> { "Waiting on Vendor", "Pending Approval", "Waiting on Requestor", "Wait for Engineering", "Wait for Purchasing" };
        private readonly List<string> submitStatus = new List<string> { "Submit Quote", "Submit Quote Request", "Submitted Quote Request","Cloned/Submitted" };
        private readonly List<SelectListItem> machineTypes = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Select Option", Value = "" },
            new SelectListItem { Text = "MT 1101 S with Pedestal", Value = "MT 1101 S with Pedestal" },
            new SelectListItem { Text = "MT 1101 S with Pendant Arm", Value = "MT 1101 S with Pedestal" },
            new SelectListItem { Text = "MT 2101 S with Pedestal", Value = "MT 2101 S with Pedestal" },
            new SelectListItem { Text = "MT 2101 S with Pendant Arm", Value = "MT 2101 S with Pendant Arm" },
            new SelectListItem { Text = "MT 2201 S with Pedestal", Value = "MT 2201 S with Pedestal" },
            new SelectListItem { Text = "MT 2201 S with Pendant Arm", Value = "MT 2201 S with Pendant Arm" },
            new SelectListItem { Text = "LC3035", Value = "LC3035" },
            new SelectListItem { Text = "WT C5", Value = "WT C5" },
            new SelectListItem { Text = "WT C10", Value = "WT C10" },
            new SelectListItem { Text = "Secarex", Value = "Secarex" },
            new SelectListItem { Text = "Special Spare Part", Value = "Special Spare Part" },
            new SelectListItem { Text = "CT - Cutting Terminal", Value = "CT - Cutting Terminal" },
            new SelectListItem { Text = "PT S4 – Punching Terminal", Value = "PT S4 – Punching Terminal" },
            new SelectListItem { Text = "EHRT FlexPunch", Value = "EHRT FlexPunch" },
            new SelectListItem { Text = "EHRT FlexPunch Compact", Value = "EHRT FlexPunch Compact" },
            new SelectListItem { Text = "EHRT HC 60", Value = "EHRT HC 60" },
            new SelectListItem { Text = "EHRT HC 80", Value = "EHRT HC 80" },
            new SelectListItem { Text = "EHRT EB 20", Value = "EHRT EB 20" },
            new SelectListItem { Text = "EHRT EB 20 Professional", Value = "EHRT EB 20 Professional" },
            new SelectListItem { Text = "EHRT EB 40", Value = "EHRT EB 40" },
            new SelectListItem { Text = "EHRT EB 40 Professional", Value = "EHRT EB 40 Professional" },
            new SelectListItem { Text = "EHRT Gantry", Value = "EHRT Gantry" }
        };
        private readonly List<SelectListItem> deliveryTypes = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Select Option", Value = "" },
            new SelectListItem { Text = "Dock", Value = "Dock" },
            new SelectListItem { Text = "Flatbed", Value = "Flatbed" }
        };
        private readonly List<SelectListItem> voltageTypes = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Select Option", Value = "" },
            new SelectListItem { Text = "208V", Value = "208V" },
            new SelectListItem { Text = "480V", Value = "480V" },
        };

        CommonController locController = new CommonController();
        RFQ_New_File RFQ_New_Files = new RFQ_New_File();
        #region Index
        // GET: RFQ
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolSubmissionRecipients"].Split(',');
            if (!newTesters.Contains(Convert.ToString(Session["userEmail"])))
            {
                return RedirectToAction("Index", "RFQTool");
            }

            List<RFQNewViewModel> myRFQs = await db.RFQNewViewModels.Where(a => a.user_id == userId).OrderByDescending(a => a.ID).ToListAsync();
            List<RFQNewViewModel> list_rfqs = new List<RFQNewViewModel>();

            foreach (var rfq in myRFQs)
            {
                var RFQ_New_Action_Log = dbEntity.RFQ_New_Action_Log.Where(a => a.Form_ID == rfq.ID);
                //Get logs
                List<RFQ_New_Action_LogViewModel> get_RFQ_logs = new List<RFQ_New_Action_LogViewModel>();
                if (RFQ_New_Action_Log.Count() > 0)
                {
                    //Check if it is ever been cloned
                    var cloneddata = RFQ_New_Action_Log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                    bool isCloned = false;
                    if (cloneddata.Count() > 0)
                    {
                        if (cloneddata.FirstOrDefault().Action == "Cloned/Submitted")
                        {
                            ViewBag.ClonedData = "Cloned Without Changes";
                        }
                        else
                        {
                            ViewBag.ClonedData = "Cloned With Changes";
                        }
                        isCloned = true;
                    }

                    foreach (var item in RFQ_New_Action_Log)
                    {
                        //Get user's full name
                        string fullname = string.Empty;
                        if (!string.IsNullOrEmpty(item.Usr_ID))
                        {
                            int usrId = Convert.ToInt32(item.Usr_ID);
                            var getfullName = await locController.GetfullName(usrId);
                            fullname = getfullName["fullName"];
                        }

                        //Get Admin Name 
                        string Adminfullname = string.Empty;
                        if (!string.IsNullOrEmpty(item.Admin_ID))
                        {
                            int adminId = Convert.ToInt32(item.Admin_ID);
                            var getAdminfullName = await locController.GetfullName(adminId);
                            Adminfullname = getAdminfullName["firstName"];
                        }

                        get_RFQ_logs.Add(new RFQ_New_Action_LogViewModel
                        {
                            Form_ID = Convert.ToInt32(item.Form_ID),
                            Action = item.Action,
                            Action_Time = item.Action_Time,
                            Notes = item.Notes,
                            Usr_ID = item.Usr_ID,
                            Admin_ID = item.Admin_ID,
                            fullName = fullname,
                            AdminfullName = Adminfullname,
                            IsCloned = isCloned
                        });
                    }
                }

                list_rfqs.Add(new RFQNewViewModel
                {
                    ID = rfq.ID,
                    Quote_Num = rfq.Quote_Num,
                    qte_num = rfq.qte_num,
                    sold_to_party = rfq.sold_to_party,
                    submission_date = rfq.submission_date,
                    completion_date=rfq.completion_date,
                    send = rfq.send,
                    save = rfq.save,
                    list_RFQ_logs = get_RFQ_logs,
                    qte_ref = rfq.qte_ref,
                    end_user_name = rfq.end_user_name,
                    end_user_location = rfq.end_user_location,
                    updated_quote = rfq.updated_quote
                });
            }

            return View(list_rfqs);
        }
        #endregion

        #region RFQ Admin
        // GET: RFQ
        public async Task<ActionResult> RfqAdmin(string filter=null, string filter_region = null,string filter_variant = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolSubmissionRecipients"].Split(',');
            if (!newTesters.Contains(Convert.ToString(Session["userEmail"])))
            {
                return RedirectToAction("RfqAdmin", "RFQTool");
            }

            List<RFQNewViewModel> myRFQs;
            if (filter== "Completed this month")
            {
                myRFQs = await db.RFQNewViewModels.Where(a => a.send == "Completed" && a.completion_date.Value.Month.Equals(DateTime.Today.Month) && a.completion_date.Value.Year.Equals(DateTime.Today.Year)).OrderByDescending(a => a.ID).ToListAsync();
            }
            else
            {
               myRFQs = await db.RFQNewViewModels.Where(a => a.save != "Save Progress" && a.send != "Completed" && a.send != "Cloned with Changes/Not Submitted").OrderByDescending(a => a.ID).ToListAsync();
            }
            List<string> compRegions = new List<string>();
            if (!string.IsNullOrEmpty(filter_region))
            {
                compRegions = await db.partnerCompanyViewModels.Where(x => x.comp_region == filter_region).Select(x => x.comp_name).ToListAsync();
            }
            List<RFQNewViewModel> list_rfqs = new List<RFQNewViewModel>();
            short variant = 0;
            if(!string.IsNullOrEmpty(filter_variant))
            {
                variant = Convert.ToInt16(filter_variant);
            }
            foreach (var rfq in myRFQs)
            {
                string salesPerson = "";
                string compName = "";
                if (rfq.sold_to_party != "Distributor Submission")
                {
                    salesPerson = rfq.sales_engineer;
                    compName = rfq.sold_to_party;
                }
                else
                {
                    salesPerson = rfq.distro_name;
                    compName = rfq.distro_company;
                }

                if(!string.IsNullOrEmpty(filter_region) && compRegions.Count() > 0 && !compRegions.Contains(compName))
                {
                    continue;
                }
                if (variant > 0 && rfq.variant != variant)
                {
                    continue;
                }
                var RFQ_New_Action_Log = dbEntity.RFQ_New_Action_Log.Where(a => a.Form_ID == rfq.ID);
                //Get logs
                List<RFQ_New_Action_LogViewModel> get_RFQ_logs = new List<RFQ_New_Action_LogViewModel>();
                if (RFQ_New_Action_Log.Count() > 0)
                {
                    //Check if it is ever been cloned
                    var cloneddata = RFQ_New_Action_Log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                    bool isCloned = false;
                    if (cloneddata.Count() > 0)
                    {
                        if (cloneddata.FirstOrDefault().Action == "Cloned/Submitted")
                        {
                            ViewBag.ClonedData = "Cloned Without Changes";
                        }
                        else
                        {
                            ViewBag.ClonedData = "Cloned With Changes";
                        }
                        isCloned = true;
                    }

                    foreach (var item in RFQ_New_Action_Log)
                    {
                        //Get user's full name
                        string fullname = string.Empty;
                        if (!string.IsNullOrEmpty(item.Usr_ID))
                        {
                            int usrId = Convert.ToInt32(item.Usr_ID);
                            var getfullName = await locController.GetfullName(usrId);
                            fullname = getfullName["fullName"];
                        }

                        //Get Admin Name 
                        string Adminfullname = string.Empty;
                        if (!string.IsNullOrEmpty(item.Admin_ID))
                        {
                            int adminId = Convert.ToInt32(item.Admin_ID);
                            var getAdminfullName = await locController.GetfullName(adminId);
                            Adminfullname = getAdminfullName["firstName"];
                        }

                        get_RFQ_logs.Add(new RFQ_New_Action_LogViewModel
                        {
                            Form_ID = Convert.ToInt32(item.Form_ID),
                            Action = item.Action,
                            Action_Time = item.Action_Time,
                            Notes = item.Notes,
                            Usr_ID = item.Usr_ID,
                            Admin_ID = item.Admin_ID,
                            fullName = fullname,
                            AdminfullName = Adminfullname,
                            IsCloned = isCloned
                        });
                    }
                }

                

                //use the logs to determine the filters
                if (filter == "Assigned")
                {
                    if (RFQ_New_Action_Log.Count() > 0 && !string.IsNullOrEmpty(RFQ_New_Action_Log.OrderByDescending(a=>a.ID).FirstOrDefault().Admin_ID))
                    {
                        list_rfqs.Add(new RFQNewViewModel
                        {
                            ID = rfq.ID,
                            qte_num = rfq.Quote_Num,
                            sold_to_party = rfq.sold_to_party,
                            submission_date = rfq.submission_date,
                            completion_date = rfq.completion_date,
                            send = rfq.send,
                            save = rfq.save,
                            list_RFQ_logs = get_RFQ_logs,
                            requestor = salesPerson,
                            comp_name = compName,
                            qte_ref = rfq.qte_ref,
                            end_user_name = rfq.end_user_name,
                            end_user_location = rfq.end_user_location,
                            updated_quote = rfq.updated_quote,
                            variant = rfq.variant
                        });
                    }
                }
                else if (filter == "Not Assigned")
                {
                    if (RFQ_New_Action_Log.Count() > 0 && string.IsNullOrEmpty(RFQ_New_Action_Log.OrderByDescending(a => a.ID).FirstOrDefault().Admin_ID))
                    {
                        list_rfqs.Add(new RFQNewViewModel
                        {
                            ID = rfq.ID,
                            qte_num = rfq.Quote_Num,
                            sold_to_party = rfq.sold_to_party,
                            submission_date = rfq.submission_date,
                            completion_date = rfq.completion_date,
                            send = rfq.send,
                            save = rfq.save,
                            list_RFQ_logs = get_RFQ_logs,
                            requestor = salesPerson,
                            comp_name = compName,
                            qte_ref = rfq.qte_ref,
                            end_user_name = rfq.end_user_name,
                            end_user_location = rfq.end_user_location,
                            updated_quote = rfq.updated_quote,
                            variant = rfq.variant
                        });
                    }
                }
                else
                {
                    list_rfqs.Add(new RFQNewViewModel
                    {
                        ID = rfq.ID,
                        qte_num = rfq.Quote_Num,
                        sold_to_party = rfq.sold_to_party,
                        submission_date = rfq.submission_date,
                        completion_date = rfq.completion_date,
                        sales_engineer =  rfq.sales_engineer,
                        send = rfq.send,
                        save = rfq.save,
                        list_RFQ_logs = get_RFQ_logs,
                        requestor = salesPerson,
                        comp_name = compName,
                        qte_ref = rfq.qte_ref,
                        end_user_name = rfq.end_user_name,
                        end_user_location = rfq.end_user_location,
                        updated_quote = rfq.updated_quote,
                        variant = rfq.variant
                    });
                }
            }

            return View(list_rfqs);
        }
        #endregion

        #region Create
        // GET: RFQ/Create
        public async Task<ActionResult> Create()
        {
            RFQNewViewModel RFQNewViewModel = new RFQNewViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolSubmissionRecipients"].Split(',');
            if (!newTesters.Contains(Convert.ToString(Session["userEmail"])))
            {
                return RedirectToAction("Create", "RFQTool");
            }

            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == userId).FirstOrDefault();

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            if (Request.QueryString["form_id"] != null)
            {

                RFQNewViewModel = db.RFQNewViewModels.Where(a => a.ID == form_id).FirstOrDefault();
                ViewBag.product_category = RFQNewViewModel.product_category;
                ViewBag.total_qty = RFQNewViewModel.total_qty;
                ViewBag.original_id = RFQNewViewModel.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == form_id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    /*RFQNewViewModelExt RFQNewViewModelExt = await db.RFQNewViewModelExts.FindAsync(item.id);
                    if (RFQNewViewModelExt == null)
                    {
                        return HttpNotFound();
                    }*/
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {   
                        rfqid = form_id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                        extModel = new RFQNewViewModelExt(),
                        //RFQNewViewModelExt,
                    });
                }
                if(Request.QueryString["first_time"] == "no" && get_RFQ_Ext.Count() == 0)
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = form_id.ToString(),
                        rfqidExt = 0,
                        total_quantity = "0",
                        product_categories = "",
                        extModel = new RFQNewViewModelExt(),
                    });
                }
                RFQNewViewModel.RFQExt = get_RFQ_Ext;
            }

            List<SelectListItem> list_product_cats = new List<SelectListItem>();
            list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "select", Selected = true });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Indoor)", Value = "industrial_freestanding_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Outdoor)", Value = "industrial_freestanding_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Indoor)", Value = "industrial_wallounted_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Outdoor)", Value = "industrial_wallounted_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "IT Freestanding", Value = "it_freestanding" });
            list_product_cats.Add(new SelectListItem { Text = "Climate Control", Value = "climate_control" });
            list_product_cats.Add(new SelectListItem { Text = "Power Distribution", Value = "power_distribution" });
            list_product_cats.Add(new SelectListItem { Text = "Automation Systems", Value = "automation_systems" });
            list_product_cats.Add(new SelectListItem { Text = "Spare Parts", Value = "spare_parts" });

            //Assign values to variables
            RFQNewViewModel.sales_engineer = userdata.usr.usr_fName + " " + userdata.usr.usr_lName;
            RFQNewViewModel.distro_name = userdata.usr.usr_fName + " " + userdata.usr.usr_lName;
            RFQNewViewModel.distro_company = userdata.comp.comp_name;
            RFQNewViewModel.cell_phone = userdata.usr.usr_phone;
            RFQNewViewModel.email = userdata.usr.usr_email;
            RFQNewViewModel.submission_date = DateTime.Now;
            RFQNewViewModel.list_prod_cat = list_product_cats;
            List<long> excludeAccounts = new List<long>();
            string exAccounts = ConfigurationManager.AppSettings["RFQExcludeAccounts"];
            foreach(var item in exAccounts.Split(','))
            {
                excludeAccounts.Add(Convert.ToInt64(item));
            }
            List<long?> accounts = await db.partnerStockCheckViewModels.Where(a => a.usr_user == userId && !excludeAccounts.Contains(a.ps_account.Value)).Select(x => x.ps_account).Distinct().ToListAsync();
            List<SelectListItem> listSAP = new List<SelectListItem>();
            listSAP.Add(new SelectListItem { Text = "Select SAP Account Number", Value = "", Selected = true });
            foreach (long? account in accounts)
            {
                if(!account.HasValue) continue;
                if(excludeAccounts.Contains(account.Value)) {
                    continue;
                }
                var locdata = db.partnerLocationViewModels.Join(
                    db.partnerCompanyViewModels,
                    comp => comp.comp_ID,
                    loc => loc.comp_ID,
                    (loc, comp) => new { loc, comp }
                    ).Where(a => a.loc.loc_SAP_account == account);

                if (locdata.Count() > 0)
                {
                    listSAP.Add(new SelectListItem { Text = account + " - " + locdata.FirstOrDefault().comp.comp_name, Value = account + "" });
                }
            }
            listSAP.Add(new SelectListItem { Text = "Other", Value = "-1" });
            RFQNewViewModel.listSAP = listSAP;
            RFQNewViewModel.listCompanies = await db.partnerCompanyViewModels.ToDictionaryAsync(x => x.comp_ID, x => x.comp_name);
            RFQNewViewModel.listLocations = await db.partnerLocationViewModels.GroupBy(x => x.comp_ID).ToDictionaryAsync(x => x.Key.Value, x => x.ToDictionary(y => y.loc_ID, y => new Dictionary<string, string>() { { "name", y.loc_name }, { "SAP", y.loc_SAP_account + "" }, { "loc_add1", y.loc_add1 } }));
            List<product> products = await dbEntity.products.ToListAsync();
            RFQNewViewModel.products = products.Select(x => x.part_number).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productMaterials = products.Where(x => !string.IsNullOrEmpty(x.material)).Select(x => x.material).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productFamilies = products.Where(x => !string.IsNullOrEmpty(x.family)).Select(x => x.family).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productRatings = products.Where(x => !string.IsNullOrEmpty(x.rating)).Select(x => x.rating).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productEnclosureTypes = products.Where(x => !string.IsNullOrEmpty(x.enclosure_type)).Select(x => x.enclosure_type).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productTypes = products.Where(x => !string.IsNullOrEmpty(x.product_type)).Select(x => x.product_type).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productAccessories = await dbEntity.product_accessories.GroupBy(x => x.product).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.part_number).ToList());
            Dictionary<string,List<string>> accessoryFilters = new Dictionary<string,List<string>>();
            accessoryFilters["install_locations"] = new List<string>() { "Front door", "Rear wall/door", "Left sidewall", "Right Sidewall", "Mounting panel", "Roof", "Interior", "Exterior", "Ship with" };
            RFQNewViewModel.accessoryFilters = accessoryFilters;
            RFQNewViewModel.enclosureAccessories = await dbEntity.accessories.ToDictionaryAsync(x => x.part_number, x => x.description_1);
            Dictionary<string, Dictionary<string, string>> climateParts = new Dictionary<string, Dictionary<string, string>>();
            climateParts["Air Conditioner"] = await dbEntity.ac_products.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            climateParts["Fan/Filter"] = await dbEntity.fan_products.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            Dictionary<string, string> filterParts = await dbEntity.fan_filters.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            foreach (var filter in filterParts)
            {
                if (!climateParts["Fan/Filter"].ContainsKey(filter.Key))
                {
                    climateParts["Fan/Filter"].Add(filter.Key, filter.Value);
                }
            }
            climateParts["Heat Exchanger"] = await dbEntity.heat_exchangers.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            RFQNewViewModel.climateParts = climateParts;
            return View(RFQNewViewModel);
        }

        // GET: RFQ/Create
        public ActionResult CreateRAS()
        {
            RFQRASViewModel RFQNewViewModel = new RFQRASViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            RFQNewViewModel.listMachineTypes = machineTypes;
            RFQNewViewModel.listDeliveryTypes = deliveryTypes;
            RFQNewViewModel.listVoltage = voltageTypes;
            return View(RFQNewViewModel);
        }

        // POST: RFQ/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,sales_engineer,regional_director,account_manager,cell_phone,email,submission_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,deal_registration,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,first_time,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,plinths_type_ie,end_user,mods_it,mods_ie,mods_other,mods_WM_AE_JB,special_dimension_it,special_dimension_ie,special_dimension_other,special_dimension_WM_AE_JB,suited_enclosures_it,suited_enclosures_ie,enclosure_1_it,enclosure_2_it,enclosure_3_it,enclosure_4_it,enclosure_5_it,enclosure_1_ie,enclosure_2_ie,enclosure_3_ie,enclosure_4_ie,enclosure_5_ie,other_sap_account_num,part_type_it,part_type_ie,part_type_other,part_type_WM_AE_JB,end_user_name,end_user_location,accessories,surfaces,mods,enclosures,default_baying_accessories,baying_accessories,product_industry,notes,variant,climate_spare")] RFQNewViewModel RFQNewViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            var locController = new CommonController();

            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);

                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                RFQ_Data_Extend rFQDataExtend = new RFQ_Data_Extend
                {
                    form_id = RFQNewViewModel.form_id,
                    total_qty = RFQNewViewModel.total_qty,
                    release_qty = RFQNewViewModel.release_qty,
                    target_price = RFQNewViewModel.target_price,
                    product_category = RFQNewViewModel.product_category,
                    xpress_mod_data = RFQNewViewModel.xpress_mod_data,
                    xpress_mod_non_data = RFQNewViewModel.xpress_mod_non_data,
                    enclosure_type_it = RFQNewViewModel.enclosure_type_it,
                    part_num_it = RFQNewViewModel.part_num_it,
                    size_hxwxd_it = RFQNewViewModel.size_hxwxd_it,
                    color_it = RFQNewViewModel.color_it,
                    sidewall_style_it = RFQNewViewModel.sidewall_style_it,
                    sidewall_location_it = RFQNewViewModel.sidewall_location_it,
                    castors_it = RFQNewViewModel.castors_it,
                    Leveling_feet_it = RFQNewViewModel.Leveling_feet_it,
                    front_it = RFQNewViewModel.front_it,
                    rear_it = RFQNewViewModel.rear_it,
                    cable_it = RFQNewViewModel.cable_it,
                    handles_it = RFQNewViewModel.handles_it,
                    inserts_it = RFQNewViewModel.inserts_it,
                    partition_wall_it = RFQNewViewModel.partition_wall_it,
                    baffles_it = RFQNewViewModel.baffles_it,
                    bsaying_brackets_it = RFQNewViewModel.bsaying_brackets_it,
                    additional_info_datacenter = RFQNewViewModel.additional_info_datacenter,
                    intell_data = RFQNewViewModel.intell_data,
                    voltage_data = RFQNewViewModel.voltage_data,
                    amp_data = RFQNewViewModel.amp_data,
                    outlet_it = RFQNewViewModel.outlet_it,
                    quantity_type_data = RFQNewViewModel.quantity_type_data,
                    input_cord_it = RFQNewViewModel.input_cord_it,
                    expansion_it = RFQNewViewModel.expansion_it,
                    part_num_ie = RFQNewViewModel.part_num_ie,
                    size_hxwxd_ie = RFQNewViewModel.size_hxwxd_ie,
                    material_ie = RFQNewViewModel.material_ie,
                    mpl_ie = RFQNewViewModel.mpl_ie,
                    sidewall_ie = RFQNewViewModel.sidewall_ie,
                    front_ie = RFQNewViewModel.front_ie,
                    rear_ie = RFQNewViewModel.rear_ie,
                    plinths_ie = RFQNewViewModel.plinths_ie,
                    cable_ie = RFQNewViewModel.cable_ie,
                    handles_ie = RFQNewViewModel.handles_ie,
                    inserts_ie = RFQNewViewModel.inserts_ie,
                    Rails = RFQNewViewModel.Rails,
                    Suited = RFQNewViewModel.suited_bay_ie,
                    suited_bay_ie = RFQNewViewModel.suited_bay_ie,
                    door_ie = RFQNewViewModel.door_ie,
                    roof_ie = RFQNewViewModel.roof_ie,
                    rear_wall_ie = RFQNewViewModel.rear_wall_ie,
                    sidewall_mod_ie = RFQNewViewModel.sidewall_mod_ie,
                    mpl_mod_ie = RFQNewViewModel.mpl_mod_ie,
                    special_paint_ie = RFQNewViewModel.special_paint_ie,
                    color_mod_ie = RFQNewViewModel.color_mod_ie,
                    ul_nema_other_ie = RFQNewViewModel.ul_nema_other_ie,
                    rating_ie = RFQNewViewModel.rating_ie,
                    part_num_WM_AE_JB = RFQNewViewModel.part_num_WM_AE_JB,
                    size_hxwxd_WM_AE_JB = RFQNewViewModel.size_hxwxd_WM_AE_JB,
                    material_WM_AE_JB = RFQNewViewModel.material_WM_AE_JB,
                    mpl_WM_AE_JB = RFQNewViewModel.mpl_WM_AE_JB,
                    latching_wm = RFQNewViewModel.latching_wm,
                    body_modified_wm = RFQNewViewModel.body_modified_wm,
                    door_modified_wm = RFQNewViewModel.door_modified_wm,
                    mpl_modified_wm = RFQNewViewModel.mpl_modified_wm,
                    special_paint_wm = RFQNewViewModel.special_paint_wm,
                    color_WM_AE_JB = RFQNewViewModel.color_WM_AE_JB,
                    ul_nema_WM_AE_JB = RFQNewViewModel.ul_nema_WM_AE_JB,
                    rating_WM_AE_JB = RFQNewViewModel.rating_WM_AE_JB,
                    part_num_other_1 = RFQNewViewModel.part_num_other_1,
                    size_hxwxd_other_1 = RFQNewViewModel.size_hxwxd_other_1,
                    producttype_other_1 = RFQNewViewModel.producttype_other_1,
                    material_other_1 = RFQNewViewModel.material_other_1,
                    body_modified_other_1 = RFQNewViewModel.body_modified_other_1,
                    door_modified_other_1 = RFQNewViewModel.door_modified_other_1,
                    mpl_modified_other_1 = RFQNewViewModel.mpl_modified_other_1,
                    specialpaint_other_1 = RFQNewViewModel.specialpaint_other_1,
                    ul_nema_other_1 = RFQNewViewModel.ul_nema_other_1,
                    rating_other_1 = RFQNewViewModel.rating_other_1,
                    additional_info_footer = RFQNewViewModel.additional_info_footer,
                    send = RFQNewViewModel.send,
                    Image_Name = RFQNewViewModel.Image_Name,
                    specialpaint_ie_1 = RFQNewViewModel.specialpaint_ie_1,
                    specialpaint_wm_1 = RFQNewViewModel.specialpaint_wm_1,
                    color_mod_other = RFQNewViewModel.color_mod_other,
                    specialpaint_other = RFQNewViewModel.specialpaint_other,
                    plinths_type_ie = RFQNewViewModel.plinths_type_ie
                };

                //Move submission date to Monday if form was submitted on weekend and move to a working day if submitted on a holiday
                if (!string.IsNullOrEmpty(Request.Form["date"]) && Request.Form["date"] != ",")
                {
                    RFQNewViewModel.submission_date = Convert.ToDateTime(Request.Form["date"]);
                }
                else
                {
                    RFQNewViewModel.submission_date = DateTime.Now;
                }
                DayOfWeek DayOfweek = RFQNewViewModel.submission_date.Value.DayOfWeek;

                if (DayOfweek.ToString() == "Sunday" && RFQNewViewModel.submission_date != Convert.ToDateTime("12/25/2016") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/27/2016") || RFQNewViewModel.submission_date == Convert.ToDateTime("05/29/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("07/04/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("09/04/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/26/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("01/02/2017"))
                {
                    RFQNewViewModel.submission_date = RFQNewViewModel.submission_date.Value.AddDays(1);
                }
                else if (DayOfweek.ToString() == "Saturday" && RFQNewViewModel.submission_date != Convert.ToDateTime("12/24/2016") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/26/2016") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/25/2017"))
                {
                    RFQNewViewModel.submission_date = RFQNewViewModel.submission_date.Value.AddDays(2);
                }
                else if (RFQNewViewModel.submission_date == Convert.ToDateTime("04/14/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("11/24/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/25/2016"))
                {
                    RFQNewViewModel.submission_date = RFQNewViewModel.submission_date.Value.AddDays(3);
                }
                else if (RFQNewViewModel.submission_date == Convert.ToDateTime("11/23/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/29/2017") || RFQNewViewModel.submission_date == Convert.ToDateTime("12/24/2016"))
                {
                    RFQNewViewModel.submission_date = RFQNewViewModel.submission_date.Value.AddDays(4);
                }
                else
                {
                    RFQNewViewModel.submission_date = DateTime.Now;
                }
                if (RFQNewViewModel.sap_account_num == "-1")
                    RFQNewViewModel.sap_account_num = RFQNewViewModel.other_sap_account_num;
                if (RFQNewViewModel.mods != null && RFQNewViewModel.mods.Length > 0)
                {
                    RFQNewViewModel.mods_it = string.Join("|", RFQNewViewModel.mods);
                }
                if (RFQNewViewModel.accessories != null && RFQNewViewModel.accessories.Length > 0)
                {
                    RFQNewViewModel.accessories_it = string.Join("|", RFQNewViewModel.accessories);
                }
                if (RFQNewViewModel.surfaces != null && RFQNewViewModel.surfaces.Length > 0)
                {
                    RFQNewViewModel.surface_to_modify = string.Join("|", RFQNewViewModel.surfaces.Distinct().ToArray());
                }
                RFQNewViewModel.user_id = user_id;

                if (RFQNewViewModel.save == null && RFQNewViewModel.send == null)//save multiple parts
                {
                    //Process the data
                    if (string.IsNullOrEmpty(Request.Form["first_time"]))//save first time
                    {
                        RFQNewViewModel.save = "Save Progress";
                        db.RFQNewViewModels.Add(RFQNewViewModel);
                        await db.SaveChangesAsync();
                        //save the images
                        await SaveImages(fileupload, RFQNewViewModel.ID, 0, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(RFQNewViewModel.ID, 0, user_id);
                        return RedirectToAction("Index", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                    else
                    {
                        dbEntity.RFQ_Data_Extend.Add(rFQDataExtend);
                        await dbEntity.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);

                        return RedirectToAction("Create", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                }
                else
                {
                    if (RFQNewViewModel.save != null)
                    {
                        RFQNewViewModel.save = "Save Progress";
                        db.RFQNewViewModels.Add(RFQNewViewModel);
                        await db.SaveChangesAsync();
                        //Add the log
                        await logAction(RFQNewViewModel.ID.ToString(), RFQNewViewModel.save, RFQNewViewModel.admin_notes, userId.ToString());
                        //Don't send email when saved
                    }
                    else if (RFQNewViewModel.send != null)
                    {
                        RFQNewViewModel.send = "Submitted Quote Request";
                        db.RFQNewViewModels.Add(RFQNewViewModel);
                        await db.SaveChangesAsync();
                        //Add the log
                        await logAction(RFQNewViewModel.ID.ToString(), RFQNewViewModel.send, RFQNewViewModel.admin_notes, userId.ToString());
                        //send email when submit is clicked after additional enclosures
                        string host = Request.Url.Port != 443 ? "http://" + Request.Url.Host + ":" + Request.Url.Port : "https://" + Request.Url.Host;
                        MailMessage message_requester = new MailMessage("NoReply@rittal.us", ConfigurationManager.AppSettings["RFQToolSubmissionRecipients"], "RiSourceCenter - A New RFQ has been submitted - RFQ #" + RFQNewViewModel.ID.ToString() + "", locController.emailheader(host) + "A new Request for Quote has been submitted.<br /><br />Request ID: " + RFQNewViewModel.ID.ToString() + "<br /><br /><br /><br />" + locController.emailfooter(host));
                        locController.sendEmailSingle(message_requester, new List<string>());
                    }
                    else
                    {
                        db.RFQNewViewModels.Add(RFQNewViewModel);
                        await db.SaveChangesAsync();
                        //Add the log
                        await logAction(RFQNewViewModel.ID.ToString(), RFQNewViewModel.send, RFQNewViewModel.admin_notes, userId.ToString());
                        //send email upon first time submit.
                    }
                    await SaveImages(fileupload, RFQNewViewModel.ID, 0, user_id);
                    //Save Installed and Shipped with parts
                    await saveInstalledShipped(RFQNewViewModel.ID, 0, user_id);
                    return RedirectToAction("Index", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }
            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);

                List<SelectListItem> list_product_cats = new List<SelectListItem>();
                list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "select", Selected = true });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Indoor)", Value = "industrial_freestanding_indoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Outdoor)", Value = "industrial_freestanding_outdoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Indoor)", Value = "industrial_wallounted_indoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Outdoor)", Value = "industrial_wallounted_outdoor" });
                list_product_cats.Add(new SelectListItem { Text = "IT Freestanding", Value = "it_freestanding" });
                list_product_cats.Add(new SelectListItem { Text = "Climate Control", Value = "climate_control" });
                list_product_cats.Add(new SelectListItem { Text = "Power Distribution", Value = "power_distribution" });
                list_product_cats.Add(new SelectListItem { Text = "Automation Systems", Value = "automation_systems" });
                list_product_cats.Add(new SelectListItem { Text = "Spare Parts", Value = "spare_parts" });
                //add to model
                RFQNewViewModel.list_prod_cat = list_product_cats;
            }

            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            if (!string.IsNullOrEmpty(Request.Form["form_id"]))
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);

                var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                RFQNewViewModel.RFQExt = get_RFQ_Ext;
            }

            return View(RFQNewViewModel);
        }

        // POST: RFQ/CreateRAS
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRAS(RFQRASViewModel RFQNewViewModel)
        {
            var locController = new CommonController();
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);

                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                db.RFQRASViewModels.Add(RFQNewViewModel);
                await db.SaveChangesAsync();
                Dictionary<string, string> emailFields = new Dictionary<string, string>() { { "account_manager", "Rittal Account Manager" }, { "regional_manager", "Rittal Regional Manager" }, { "machine_type", "Machine Type" }, { "competitor", "Competitor" }, { "opportunity_id", "Opportunity ID" }, { "erp_id", "ERP ID" }, { "company", "Company Name" }, { "contact_name", "Contact Name" }, { "phone_number", "Phone Number" }, { "email", "Email" }, { "street_address", "Street Address" }, { "city", "City" }, { "state", "State" }, { "zipcode", "Zip Code" }, { "delivery_type", "Delivery Type" }, { "transformer_voltage", "Transformer Voltage" }, { "equipment_options", "Equipment Options" } };
                string emailBody = "<table style='width:100%;border-collapse:collapse;' border='1' cellpadding='0' cellspacing='0'>";
                foreach(var item in emailFields)
                {
                    emailBody += "<tr><th style='width:25%;padding:10px;'>" + item.Value + "</th>";
                    emailBody += "<td style='width:75%;padding:10px;'>" + (RFQNewViewModel[item.Key] != null ? RFQNewViewModel[item.Key] : "") + "</td></tr>";
                }
                emailBody += "</table><br/><br/>";
                locController.email("noreply@rittal.us", "herzog.m@rittal.us,rittal@rittal.us,robbins.j@rittal.us,shaw.c@rittal.us", "New RFQ for RAS", "A new RFQ for RAS has been submitted.<br /><br />" + emailBody, "yes", true);
                locController.email("noreply@rittal.us", Convert.ToString(Session["userEmail"]), "New RFQ for RAS", "Your RFQ for RAS request has been submitted to Mike Herzog. If you have any questions or concerns, please reach out to Mike at Herzog.m@rittal.us.<br /><br />" + emailBody, "yes", true);
                return RedirectToAction("Index", new { success = "Your Request For Quote has been added" });
            }
            else
            {
                ViewBag.error = "Complete all fields marked *";
                RFQNewViewModel.listMachineTypes = machineTypes;
                RFQNewViewModel.listDeliveryTypes = deliveryTypes;
                RFQNewViewModel.listVoltage = voltageTypes;
                return View(RFQNewViewModel);
            }
        }
        #endregion

        #region Edit
        // GET: RFQTool/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolSubmissionRecipients"].Split(',');
            if (!newTesters.Contains(Convert.ToString(Session["userEmail"])))
            {
                return RedirectToAction("Index", "RFQTool");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
            if (RFQNewViewModel == null)
            {
                return HttpNotFound();
            }
            //get the requestors id from the RFQ forms
            int requestor_id = Convert.ToInt32(RFQNewViewModel.user_id);
            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == requestor_id).FirstOrDefault();

            List<SelectListItem> list_product_cats = new List<SelectListItem>();
            list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "select", Selected = true });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Indoor)", Value = "industrial_freestanding_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Outdoor)", Value = "industrial_freestanding_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Indoor)", Value = "industrial_wallounted_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Outdoor)", Value = "industrial_wallounted_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "IT Freestanding", Value = "it_freestanding" });
            list_product_cats.Add(new SelectListItem { Text = "Climate Control", Value = "climate_control" });
            list_product_cats.Add(new SelectListItem { Text = "Power Distribution", Value = "power_distribution" });
            list_product_cats.Add(new SelectListItem { Text = "Automation Systems", Value = "automation_systems" });
            list_product_cats.Add(new SelectListItem { Text = "Spare Parts", Value = "spare_parts" });
            RFQNewViewModel.list_prod_cat = list_product_cats;

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            //Collect the data from the database
            var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == id).FirstOrDefault();
            var RFQ_Ext_data = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
            var RFQ_New_Files_data = dbEntity.RFQ_New_Files.Where(a => a.form_id == id && a.ext_form_id == 0);
            var RFQ_New_Action_Log = dbEntity.RFQ_New_Action_Log.Where(a => a.Form_ID == id);
            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == id && a.ext_form_id == 0);
            ViewBag.waitStatus = waitStatus;

            //Get installed parts
            if (rfq_installed_parts.Count() > 0)
            {
                RFQNewViewModel.list_installed_parts = rfq_installed_parts;
            }

            //Get installed parts
            if (rfq_shipped_parts.Count() > 0)
            {
                RFQNewViewModel.list_shipped_parts = rfq_shipped_parts;
            }

            //Get extensions for the top portlet
            if (RFQ_Ext_data.Count() > 0)
            {
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                foreach (var item in RFQ_Ext_data.OrderBy(a => a.id))
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                RFQNewViewModel.RFQExt = get_RFQ_Ext;
            }
            else
            {
                ViewBag.original_id = id;
            }

            //Get images
            List<RFQ_New_File> get_RFQ_New_Files = new List<RFQ_New_File>();
            if (RFQ_New_Files_data.Count() > 0)
            {
                foreach (var item in RFQ_New_Files_data)
                {
                    get_RFQ_New_Files.Add(new RFQ_New_File
                    {
                        file_id = item.file_id,
                        form_id = item.form_id,
                        file_name = item.file_name,
                        file_type = item.file_type
                    });
                }
                RFQNewViewModel.list_RFQ_files = get_RFQ_New_Files;
            }
            //Get logs
            List<RFQ_New_Action_LogViewModel> get_RFQ_logs = new List<RFQ_New_Action_LogViewModel>();
            if (RFQ_New_Action_Log.Count() > 0)
            {
                foreach (var item in RFQ_New_Action_Log)
                {
                    if (string.IsNullOrEmpty(item.Usr_ID))
                    {
                        continue;
                    }
                    //Get user's full name
                    int usrId = Convert.ToInt32(item.Usr_ID);
                    var getfullName = await locController.GetfullName(usrId);
                    //Get Admin Name 
                    int adminId = Convert.ToInt32(item.Admin_ID);
                    var getAdminfullName = await locController.GetfullName(adminId);

                    get_RFQ_logs.Add(new RFQ_New_Action_LogViewModel
                    {
                        Form_ID = Convert.ToInt32(item.Form_ID),
                        Action = item.Action,
                        Action_Time = item.Action_Time,
                        Notes = item.Notes,
                        Usr_ID = item.Usr_ID,
                        Admin_ID = item.Admin_ID,
                        fullName = getfullName["fullName"],
                        AdminfullName = getAdminfullName["firstName"]
                    });
                }
                RFQNewViewModel.list_RFQ_logs = get_RFQ_logs;
                //Check if it is ever been cloned
                var cloneddata = RFQ_New_Action_Log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                bool isCloned = false;
                if (cloneddata.Count() > 0)
                {
                    if (cloneddata.FirstOrDefault().Action == "Cloned/Submitted")
                    {
                        ViewBag.ClonedData = "Cloned Without Changes";
                    }
                    else
                    {
                        ViewBag.ClonedData = "Cloned With Changes";
                    }
                    isCloned = true;
                }
                RFQNewViewModel.IsCloned = isCloned;
            }
            if (userdata!=null)
            {
                ViewBag.comp_type = userdata.comp.comp_type;
            }
            else
            {
                return RedirectToAction("RfqAdmin", new { form_id = RFQNewViewModel.form_id, first_time = Request.QueryString["first_time"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The user account has been deleted", admin = "yes" });
            }
            RFQNewViewModel.listCompanies = await db.partnerCompanyViewModels.ToDictionaryAsync(x => x.comp_ID, x => x.comp_name);
            RFQNewViewModel.listLocations = await db.partnerLocationViewModels.GroupBy(x => x.comp_ID).ToDictionaryAsync(x => x.Key.Value, x => x.ToDictionary(y => y.loc_ID, y => new Dictionary<string, string>() { { "name", y.loc_name }, { "SAP", y.loc_SAP_account + "" }, { "loc_add1", y.loc_add1 } }));
            List<product> products = await dbEntity.products.ToListAsync();
            RFQNewViewModel.products = products.Select(x => x.part_number).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productMaterials = products.Where(x => !string.IsNullOrEmpty(x.material)).Select(x => x.material).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productFamilies = products.Where(x => !string.IsNullOrEmpty(x.family)).Select(x => x.family).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productRatings = products.Where(x => !string.IsNullOrEmpty(x.rating)).Select(x => x.rating).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productEnclosureTypes = products.Where(x => !string.IsNullOrEmpty(x.enclosure_type)).Select(x => x.enclosure_type).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productTypes = products.Where(x => !string.IsNullOrEmpty(x.product_type)).Select(x => x.product_type).Distinct().OrderBy(x => x).ToList();
            RFQNewViewModel.productAccessories = await dbEntity.product_accessories.GroupBy(x => x.product).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.part_number).ToList());
            Dictionary<string, List<string>> accessoryFilters = new Dictionary<string, List<string>>();
            accessoryFilters["install_locations"] = new List<string>() { "Front door", "Rear wall/door", "Left sidewall", "Right Sidewall", "Mounting panel", "Roof", "Interior", "Exterior", "Ship with" };
            RFQNewViewModel.accessoryFilters = accessoryFilters;
            RFQNewViewModel.enclosureAccessories = await dbEntity.accessories.ToDictionaryAsync(x => x.part_number, x => x.description_1);
            Dictionary<string, Dictionary<string, string>> climateParts = new Dictionary<string, Dictionary<string, string>>();
            climateParts["Air Conditioner"] = await dbEntity.ac_products.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            climateParts["Fan/Filter"] = await dbEntity.fan_products.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            Dictionary<string, string> filterParts = await dbEntity.fan_filters.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            foreach (var filter in filterParts)
            {
                if (!climateParts["Fan/Filter"].ContainsKey(filter.Key))
                {
                    climateParts["Fan/Filter"].Add(filter.Key, filter.Value);
                }
            }
            climateParts["Heat Exchanger"] = await dbEntity.heat_exchangers.GroupBy(x => x.part_number).ToDictionaryAsync(x => x.Key, x => x.Select(y => y.description).FirstOrDefault());
            RFQNewViewModel.climateParts = climateParts;
            return View(RFQNewViewModel);
        }

        // GET: RFQTool/View/5
        public async Task<ActionResult> ViewRFQ(int? id)
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
            RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
            if (RFQNewViewModel == null)
            {
                return HttpNotFound();
            }
            //get the requestors id from the RFQ forms
            int requestor_id = Convert.ToInt32(RFQNewViewModel.user_id);
            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == requestor_id).FirstOrDefault();

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            //Collect the data from the database
            var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == id).FirstOrDefault();
            var RFQ_Ext_data = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
            var RFQ_New_Files_data = dbEntity.RFQ_New_Files.Where(a => a.form_id == id && a.ext_form_id == 0);
            var RFQ_New_Action_Log = dbEntity.RFQ_New_Action_Log.Where(a => a.Form_ID == id);
            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == id && a.ext_form_id == 0);
            ViewBag.waitStatus = waitStatus;

            //Get installed parts
            if (rfq_installed_parts.Count() > 0)
            {
                RFQNewViewModel.list_installed_parts = rfq_installed_parts;
            }

            //Get installed parts
            if (rfq_shipped_parts.Count() > 0)
            {
                RFQNewViewModel.list_shipped_parts = rfq_shipped_parts;
            }

            //Get extensions for the top portlet
            if (RFQ_Ext_data.Count() > 0)
            {
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                foreach (var item in RFQ_Ext_data.OrderBy(a => a.id))
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                RFQNewViewModel.RFQExt = get_RFQ_Ext;
            }
            else
            {
                ViewBag.original_id = id;
            }

            //Get images
            List<RFQ_New_File> get_RFQ_New_Files = new List<RFQ_New_File>();
            if (RFQ_New_Files_data.Count() > 0)
            {
                foreach (var item in RFQ_New_Files_data)
                {
                    get_RFQ_New_Files.Add(new RFQ_New_File
                    {
                        file_id = item.file_id,
                        form_id = item.form_id,
                        file_name = item.file_name,
                        file_type = item.file_type
                    });
                }
                RFQNewViewModel.list_RFQ_files = get_RFQ_New_Files;
            }
            //Get logs
            List<RFQ_New_Action_LogViewModel> get_RFQ_logs = new List<RFQ_New_Action_LogViewModel>();
            if (RFQ_New_Action_Log.Count() > 0)
            {
                foreach (var item in RFQ_New_Action_Log)
                {
                    if (string.IsNullOrEmpty(item.Usr_ID))
                    {
                        continue;
                    }
                    //Get user's full name
                    int usrId = Convert.ToInt32(item.Usr_ID);
                    var getfullName = await locController.GetfullName(usrId);
                    //Get Admin Name 
                    int adminId = Convert.ToInt32(item.Admin_ID);
                    var getAdminfullName = await locController.GetfullName(adminId);

                    get_RFQ_logs.Add(new RFQ_New_Action_LogViewModel
                    {
                        Form_ID = Convert.ToInt32(item.Form_ID),
                        Action = item.Action,
                        Action_Time = item.Action_Time,
                        Notes = item.Notes,
                        Usr_ID = item.Usr_ID,
                        Admin_ID = item.Admin_ID,
                        fullName = getfullName["fullName"],
                        AdminfullName = getAdminfullName["firstName"]
                    });
                }
                RFQNewViewModel.list_RFQ_logs = get_RFQ_logs;
                //Check if it is ever been cloned
                var cloneddata = RFQ_New_Action_Log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                bool isCloned = false;
                if (cloneddata.Count() > 0)
                {
                    if (cloneddata.FirstOrDefault().Action == "Cloned/Submitted")
                    {
                        ViewBag.ClonedData = "Cloned Without Changes";
                    }
                    else
                    {
                        ViewBag.ClonedData = "Cloned With Changes";
                    }
                    isCloned = true;
                }
                RFQNewViewModel.IsCloned = isCloned;
            }
            if (userdata != null)
            {
                ViewBag.comp_type = userdata.comp.comp_type;
            }
            else
            {
                return RedirectToAction("RfqAdmin", new { form_id = RFQNewViewModel.form_id, first_time = Request.QueryString["first_time"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The user account has been deleted", admin = "yes" });
            }
            return View(RFQNewViewModel);
        }

        // POST: RFQ/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,sales_engineer,regional_director,account_manager,cell_phone,email,submission_date,completion_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,deal_registration,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,admin_notes,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,Quote_Num,admin_status,plinths_type_ie,end_user,mods_it,mods_ie,mods_other,mods_WM_AE_JB,special_dimension_it,special_dimension_ie,special_dimension_other,special_dimension_WM_AE_JB,suited_enclosures_it,suited_enclosures_ie,enclosure_1_it,enclosure_2_it,enclosure_3_it,enclosure_4_it,enclosure_5_it,enclosure_1_ie,enclosure_2_ie,enclosure_3_ie,enclosure_4_ie,enclosure_5_ie,other_sap_account_num,part_type_it,part_type_ie,part_type_other,part_type_WM_AE_JB,end_user_name,end_user_location,accessories,surfaces,mods,enclosures,default_baying_accessories,baying_accessories,product_industry")] RFQNewViewModel RFQNewViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                //declare email variables
                string host = "";
                if (Request.Url.Port != 443)
                {
                    host = "http://" + Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = "https://" + Request.Url.Host;
                }
                if (string.IsNullOrEmpty(Request.Form["saveadmin"]) && string.IsNullOrEmpty(Request.Form["complete"]) && string.IsNullOrEmpty(Request.Form["return"]))
                {
                    var formId = Convert.ToInt32(RFQNewViewModel.form_id);
                    RFQNewViewModel rfqTemp = await db.RFQNewViewModels.AsNoTracking().Where(x => x.ID == formId).FirstAsync();

                    if (rfqTemp == null || rfqTemp.completion_date != null)
                    {
                        return RedirectToAction("Index", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }

                }

                string header = locController.emailheader(host);
                string footer = locController.emailfooter(host);
                MailMessage message_requester;
                List<string> files = new List<string>();

                RFQ_Data_Extend rFQDataExtend = new RFQ_Data_Extend
                {
                    form_id = RFQNewViewModel.form_id,
                    total_qty = RFQNewViewModel.total_qty,
                    release_qty = RFQNewViewModel.release_qty,
                    target_price = RFQNewViewModel.target_price,
                    product_category = RFQNewViewModel.product_category,
                    xpress_mod_data = RFQNewViewModel.xpress_mod_data,
                    xpress_mod_non_data = RFQNewViewModel.xpress_mod_non_data,
                    enclosure_type_it = RFQNewViewModel.enclosure_type_it,
                    part_num_it = RFQNewViewModel.part_num_it,
                    size_hxwxd_it = RFQNewViewModel.size_hxwxd_it,
                    color_it = RFQNewViewModel.color_it,
                    sidewall_style_it = RFQNewViewModel.sidewall_style_it,
                    sidewall_location_it = RFQNewViewModel.sidewall_location_it,
                    castors_it = RFQNewViewModel.castors_it,
                    Leveling_feet_it = RFQNewViewModel.Leveling_feet_it,
                    front_it = RFQNewViewModel.front_it,
                    rear_it = RFQNewViewModel.rear_it,
                    cable_it = RFQNewViewModel.cable_it,
                    handles_it = RFQNewViewModel.handles_it,
                    inserts_it = RFQNewViewModel.inserts_it,
                    partition_wall_it = RFQNewViewModel.partition_wall_it,
                    baffles_it = RFQNewViewModel.baffles_it,
                    bsaying_brackets_it = RFQNewViewModel.bsaying_brackets_it,
                    additional_info_datacenter = RFQNewViewModel.additional_info_datacenter,
                    intell_data = RFQNewViewModel.intell_data,
                    voltage_data = RFQNewViewModel.voltage_data,
                    amp_data = RFQNewViewModel.amp_data,
                    outlet_it = RFQNewViewModel.outlet_it,
                    quantity_type_data = RFQNewViewModel.quantity_type_data,
                    input_cord_it = RFQNewViewModel.input_cord_it,
                    expansion_it = RFQNewViewModel.expansion_it,
                    part_num_ie = RFQNewViewModel.part_num_ie,
                    size_hxwxd_ie = RFQNewViewModel.size_hxwxd_ie,
                    material_ie = RFQNewViewModel.material_ie,
                    mpl_ie = RFQNewViewModel.mpl_ie,
                    sidewall_ie = RFQNewViewModel.sidewall_ie,
                    front_ie = RFQNewViewModel.front_ie,
                    rear_ie = RFQNewViewModel.rear_ie,
                    plinths_ie = RFQNewViewModel.plinths_ie,
                    cable_ie = RFQNewViewModel.cable_ie,
                    handles_ie = RFQNewViewModel.handles_ie,
                    inserts_ie = RFQNewViewModel.inserts_ie,
                    Rails = RFQNewViewModel.Rails,
                    Suited = RFQNewViewModel.suited_bay_ie,
                    suited_bay_ie = RFQNewViewModel.suited_bay_ie,
                    door_ie = RFQNewViewModel.door_ie,
                    roof_ie = RFQNewViewModel.roof_ie,
                    rear_wall_ie = RFQNewViewModel.rear_wall_ie,
                    sidewall_mod_ie = RFQNewViewModel.sidewall_mod_ie,
                    mpl_mod_ie = RFQNewViewModel.mpl_mod_ie,
                    special_paint_ie = RFQNewViewModel.special_paint_ie,
                    color_mod_ie = RFQNewViewModel.color_mod_ie,
                    ul_nema_other_ie = RFQNewViewModel.ul_nema_other_ie,
                    rating_ie = RFQNewViewModel.rating_ie,
                    part_num_WM_AE_JB = RFQNewViewModel.part_num_WM_AE_JB,
                    size_hxwxd_WM_AE_JB = RFQNewViewModel.size_hxwxd_WM_AE_JB,
                    material_WM_AE_JB = RFQNewViewModel.material_WM_AE_JB,
                    mpl_WM_AE_JB = RFQNewViewModel.mpl_WM_AE_JB,
                    latching_wm = RFQNewViewModel.latching_wm,
                    body_modified_wm = RFQNewViewModel.body_modified_wm,
                    door_modified_wm = RFQNewViewModel.door_modified_wm,
                    mpl_modified_wm = RFQNewViewModel.mpl_modified_wm,
                    special_paint_wm = RFQNewViewModel.special_paint_wm,
                    color_WM_AE_JB = RFQNewViewModel.color_WM_AE_JB,
                    ul_nema_WM_AE_JB = RFQNewViewModel.ul_nema_WM_AE_JB,
                    rating_WM_AE_JB = RFQNewViewModel.rating_WM_AE_JB,
                    part_num_other_1 = RFQNewViewModel.part_num_other_1,
                    size_hxwxd_other_1 = RFQNewViewModel.size_hxwxd_other_1,
                    producttype_other_1 = RFQNewViewModel.producttype_other_1,
                    material_other_1 = RFQNewViewModel.material_other_1,
                    body_modified_other_1 = RFQNewViewModel.body_modified_other_1,
                    door_modified_other_1 = RFQNewViewModel.door_modified_other_1,
                    mpl_modified_other_1 = RFQNewViewModel.mpl_modified_other_1,
                    specialpaint_other_1 = RFQNewViewModel.specialpaint_other_1,
                    ul_nema_other_1 = RFQNewViewModel.ul_nema_other_1,
                    rating_other_1 = RFQNewViewModel.rating_other_1,
                    additional_info_footer = RFQNewViewModel.additional_info_footer,
                    send = RFQNewViewModel.send,
                    Image_Name = RFQNewViewModel.Image_Name,
                    specialpaint_ie_1 = RFQNewViewModel.specialpaint_ie_1,
                    specialpaint_wm_1 = RFQNewViewModel.specialpaint_wm_1,
                    color_mod_other = RFQNewViewModel.color_mod_other,
                    specialpaint_other = RFQNewViewModel.specialpaint_other,
                    plinths_type_ie = RFQNewViewModel.plinths_type_ie
                };
                if (RFQNewViewModel.sap_account_num == "-1")
                    RFQNewViewModel.sap_account_num = RFQNewViewModel.other_sap_account_num;
                if(RFQNewViewModel.mods != null && RFQNewViewModel.mods.Length > 0)
                {
                    RFQNewViewModel.mods_it = string.Join("|", RFQNewViewModel.mods);
                }
                if(RFQNewViewModel.accessories != null && RFQNewViewModel.accessories.Length > 0)
                {
                    RFQNewViewModel.accessories_it = string.Join("|", RFQNewViewModel.accessories);
                }
                if(RFQNewViewModel.surfaces != null && RFQNewViewModel.surfaces.Length > 0)
                {
                    RFQNewViewModel.surface_to_modify = string.Join("|", RFQNewViewModel.surfaces.Distinct().ToArray());
                }
                

                //Handle all actions
                if (!string.IsNullOrEmpty(Request.Form["saveadmin"]) || !string.IsNullOrEmpty(Request.Form["complete"]) || !string.IsNullOrEmpty(Request.Form["return"]))
                {
                    //Get user information
                    int requestor_id = Convert.ToInt32(RFQNewViewModel.user_id);
                    var getuserdata = await db.UserViewModels.Where(a => a.usr_ID == requestor_id).FirstOrDefaultAsync();
                    var getAdmindata = await db.UserViewModels.Where(a => a.usr_ID == userId).FirstOrDefaultAsync();
                    string salutaion = "Please do not reply to this email. <br /><br />" + getAdmindata.usr_fName + " " + getAdmindata.usr_lName +
                                        ",<br />" + getAdmindata.usr_title + "<br />" +
                                        "Rittal Corporation -1 Rittal Place Urbana, OH 43078<br />" +
                                        "Phone: " + getAdmindata.usr_phone + "  · Fax: " + getAdmindata.usr_fax + " · Email: " + getAdmindata.usr_email + "<br />";
                    //Handle RFQ admin actions
                    string actionStr = "";
                    bool bAssign = true;
                    if (!string.IsNullOrEmpty(Request.Form["saveadmin"]))
                    {
                        string action = "";
                        if (!string.IsNullOrEmpty(Request.Form["wait_status"])){
                            action = Request.Form["wait_status"];
                        }
                        if(RFQNewViewModel.send != "Completed")
                        {
                            RFQNewViewModel.send = "Admin Saved with Notes";
                        }
                        actionStr = action;
                        bAssign = false;
                    }
                    else if (!string.IsNullOrEmpty(Request.Form["complete"]))
                    {
                        RFQNewViewModel.send = "Completed";
                        RFQNewViewModel.completion_date = DateTime.Now;
                        
                        //Send the return email
                        string body_requester = "Your request for quote has been completed.<br /><br />" +
                                                "Request ID: " + RFQNewViewModel.form_id + "<br /><br />" +
                                                "Thank you for offering Rittal the opportunity to provide a solution for " + RFQNewViewModel.sold_to_party + " " +
                                                "Your quotation has been completed on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) + " and assigned Quote Number " + RFQNewViewModel.Quote_Num + "<br />" +
                                                "The quotation will be emailed to you shortly.Please contact me if you have any questions and thank you for the opportunity to provide a proposal." +
                                                "If you are ready to proceed with an order, purchase orders can be sent directly to <a href=\"mailto: orders@rittal.us? Subject = RFQ purchase orders request\" >orders@rittal.us</a>.Thank you for your business!" +
                                                "<br /><br />-Rittal Quotation Team <br /><br />" + salutaion;
                        //Send email to the Return Requester
                        message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been completed #" + RFQNewViewModel.form_id + "", header + body_requester + footer);
                        locController.sendEmailSingle(message_requester, files);
                        actionStr = "Completed";
                    }
                    else
                    {
                        var getAssigned = db.RFQ_New_Action_LogViewModels.Where(a=>a.Form_ID==RFQNewViewModel.ID && a.Action=="Assigned");
                        if (getAssigned.Count() > 0)
                        {
                            actionStr = "Assigned-Returned";
                            RFQNewViewModel.send = "Returned";
                            //RFQNewViewModel.admin_status = null;
                        }
                        else
                        {
                            actionStr = "Returned";
                            RFQNewViewModel.send = "Returned";
                            RFQNewViewModel.admin_status = null;
                        }

                        //Send the return email
                        string body_requester = "Your request for quote has been returned.<br /><br />" +
                                                "Quote Team Notes: <b><u>" + RFQNewViewModel.admin_notes + "</u></b><br /><br />" +
                                                 "Request ID: " + RFQNewViewModel.form_id + "<br /><br />" + 
                                                "Thank you for offering Rittal the opportunity to provide a solution for " + RFQNewViewModel.sold_to_party + ". Your quotation was RETURNED on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) +
                                                 ".<br /><br /> More information has been provided below.<br /><br />" + salutaion;
                                                 

                        //Send email to the Return Requester
                        message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been returned #" + RFQNewViewModel.form_id + "", header + body_requester + footer);
                        locController.sendEmailSingle(message_requester, files);
                    }

                    db.Entry(RFQNewViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    await logAction(RFQNewViewModel.form_id, actionStr, RFQNewViewModel.admin_notes, userId.ToString(), user_id, bAssign);
                    return RedirectToAction("RfqAdmin", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = (!string.IsNullOrEmpty(Request.Form["saveadmin"]) ? "Notes have been added to the RFQ" : "The status on the RFQ has been updated"), admin = "yes" });
                }
                else
                {
                    //Handle none RFQ admin actions
                    if (RFQNewViewModel.save == null && RFQNewViewModel.send == null)//save multiple parts
                    {
                        if (rFQDataExtend.suited_bay_ie != "" && rFQDataExtend.suited_bay_ie != null)
                        {
                            int bay = Convert.ToInt32(rFQDataExtend.suited_bay_ie);
                            bay += 1;
                            rFQDataExtend.suited_bay_ie = bay.ToString();
                        }
                        else
                        {
                            rFQDataExtend.suited_bay_ie = "1";
                        }
                        dbEntity.RFQ_Data_Extend.Add(rFQDataExtend);
                        await dbEntity.SaveChangesAsync();
                        await SaveImages(fileupload, RFQNewViewModel.ID, rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id, "edit");

                        return RedirectToAction("EditExt", new { id= rFQDataExtend.id, form_id = RFQNewViewModel.form_id, first_time = "no", n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                    else
                    {
                        if (RFQNewViewModel.save != null)
                        {
                            db.Entry(RFQNewViewModel).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            await SaveImages(fileupload, RFQNewViewModel.ID, 0, user_id);
                            //Save Installed and Shipped with parts
                            await saveInstalledShipped(RFQNewViewModel.ID, 0, user_id, "edit");
                            //Add the log
                            await logAction(RFQNewViewModel.ID.ToString(), RFQNewViewModel.save, RFQNewViewModel.admin_notes, userId.ToString());
                            //Don't send email upon first time save
                        }
                        else
                        {
                            RFQNewViewModel.submission_date = DateTime.Now;
                            db.Entry(RFQNewViewModel).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            await SaveImages(fileupload, RFQNewViewModel.ID, 0, user_id);
                            //Save Installed and Shipped with parts
                            await saveInstalledShipped(RFQNewViewModel.ID, 0, user_id, "edit");
                            //Add the log
                            await logAction(RFQNewViewModel.ID.ToString(), RFQNewViewModel.send, RFQNewViewModel.admin_notes, userId.ToString());
                            //send email upon first time submit.
                        }

                        return RedirectToAction("Index", new { form_id = RFQNewViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                }

            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);

                List<SelectListItem> list_product_cats = new List<SelectListItem>();
                list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "select", Selected = true });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Indoor)", Value = "industrial_freestanding_indoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Outdoor)", Value = "industrial_freestanding_outdoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Indoor)", Value = "industrial_wallounted_indoor" });
                list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Outdoor)", Value = "industrial_wallounted_outdoor" });
                list_product_cats.Add(new SelectListItem { Text = "IT Freestanding", Value = "it_freestanding" });
                list_product_cats.Add(new SelectListItem { Text = "Climate Control", Value = "climate_control" });
                list_product_cats.Add(new SelectListItem { Text = "Power Distribution", Value = "power_distribution" });
                list_product_cats.Add(new SelectListItem { Text = "Automation Systems", Value = "automation_systems" });
                list_product_cats.Add(new SelectListItem { Text = "Spare Parts", Value = "spare_parts" });
                RFQNewViewModel.list_prod_cat = list_product_cats;
            }
            
            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            if (Request.Form["form_id"] != null)
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);

                var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                RFQNewViewModel.RFQExt = get_RFQ_Ext;
            }

            return View(RFQNewViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ViewRFQ([Bind(Include = "form_id,admin_notes,Quote_Num")] RFQNewViewModel inputModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            int formId = Convert.ToInt32(inputModel.form_id);
            RFQNewViewModel rfqModel = await db.RFQNewViewModels.Where(x => x.ID == formId).FirstOrDefaultAsync();
            //declare email variables
            string host = "";
            if (Request.Url.Port != 443)
            {
                host = "http://" + Request.Url.Host + ":" + Request.Url.Port;
            }
            else
            {
                host = "https://" + Request.Url.Host;
            }
            if (string.IsNullOrEmpty(Request.Form["saveadmin"]) && string.IsNullOrEmpty(Request.Form["complete"]) && string.IsNullOrEmpty(Request.Form["return"]))
            {
                if (rfqModel == null || rfqModel.completion_date != null)
                {
                    return RedirectToAction("Index", new { form_id = inputModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }

            }

            string header = locController.emailheader(host);
            string footer = locController.emailfooter(host);
            MailMessage message_requester;
            List<string> files = new List<string>();

            //Handle all actions
            if (!string.IsNullOrEmpty(Request.Form["saveadmin"]) || !string.IsNullOrEmpty(Request.Form["complete"]) || !string.IsNullOrEmpty(Request.Form["return"]))
            {
                //Get user information
                int requestor_id = Convert.ToInt32(rfqModel.user_id);
                var getuserdata = await db.UserViewModels.Where(a => a.usr_ID == requestor_id).FirstOrDefaultAsync();
                var getAdmindata = await db.UserViewModels.Where(a => a.usr_ID == userId).FirstOrDefaultAsync();
                string salutaion = "Please do not reply to this email. <br /><br />" + getAdmindata.usr_fName + " " + getAdmindata.usr_lName +
                                    ",<br />" + getAdmindata.usr_title + "<br />" +
                                    "Rittal Corporation -1 Rittal Place Urbana, OH 43078<br />" +
                                    "Phone: " + getAdmindata.usr_phone + "  · Fax: " + getAdmindata.usr_fax + " · Email: " + getAdmindata.usr_email + "<br />";
                //Handle RFQ admin actions
                string actionStr = "";
                bool bAssign = true;
                if (!string.IsNullOrEmpty(Request.Form["saveadmin"]))
                {
                    string action = "";
                    if (!string.IsNullOrEmpty(Request.Form["wait_status"]))
                    {
                        action = Request.Form["wait_status"];
                    }
                    if (rfqModel.send != "Completed")
                    {
                        rfqModel.send = "Admin Saved with Notes";
                    }
                    actionStr = action;
                    bAssign = false;
                }
                else if (!string.IsNullOrEmpty(Request.Form["complete"]))
                {
                    rfqModel.send = "Completed";
                    rfqModel.completion_date = DateTime.Now;

                    //Send the return email
                    string body_requester = "Your request for quote has been completed.<br /><br />" +
                                            "Request ID: " + rfqModel.form_id + "<br /><br />" +
                                            "Thank you for offering Rittal the opportunity to provide a solution for " + rfqModel.sold_to_party + " " +
                                            "Your quotation has been completed on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) + " and assigned Quote Number " + rfqModel.Quote_Num + "<br />" +
                                            "The quotation will be emailed to you shortly.Please contact me if you have any questions and thank you for the opportunity to provide a proposal." +
                                            "If you are ready to proceed with an order, purchase orders can be sent directly to <a href=\"mailto: orders@rittal.us? Subject = RFQ purchase orders request\" >orders@rittal.us</a>.Thank you for your business!" +
                                            "<br /><br />-Rittal Quotation Team <br /><br />" + salutaion;
                    //Send email to the Return Requester
                    message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been completed #" + rfqModel.form_id + "", header + body_requester + footer);
                    locController.sendEmailSingle(message_requester, files);
                    actionStr = "Completed";
                }
                else
                {
                    var getAssigned = db.RFQ_New_Action_LogViewModels.Where(a => a.Form_ID == rfqModel.ID && a.Action == "Assigned");
                    if (getAssigned.Count() > 0)
                    {
                        actionStr = "Assigned-Returned";
                        rfqModel.send = "Returned";
                        //RFQNewViewModel.admin_status = null;
                    }
                    else
                    {
                        actionStr = "Returned";
                        rfqModel.send = "Returned";
                        rfqModel.admin_status = null;
                    }

                    //Send the return email
                    string body_requester = "Your request for quote has been returned.<br /><br />" +
                                            "Quote Team Notes: <b><u>" + inputModel.admin_notes + "</u></b><br /><br />" +
                                                "Request ID: " + inputModel.form_id + "<br /><br />" +
                                            "Thank you for offering Rittal the opportunity to provide a solution for " + rfqModel.sold_to_party + ". Your quotation was RETURNED on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) +
                                                ".<br /><br /> More information has been provided below.<br /><br />" + salutaion;


                    //Send email to the Return Requester
                    message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been returned #" + inputModel.form_id + "", header + body_requester + footer);
                    locController.sendEmailSingle(message_requester, files);
                }
                if(!string.IsNullOrEmpty(inputModel.Quote_Num) && rfqModel.Quote_Num != inputModel.Quote_Num)
                {
                    rfqModel.Quote_Num = inputModel.Quote_Num;
                }
                db.Entry(rfqModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                await logAction(inputModel.form_id, actionStr, inputModel.admin_notes, userId.ToString(), user_id, bAssign);
                return RedirectToAction("RfqAdmin", new { form_id = inputModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = (!string.IsNullOrEmpty(Request.Form["saveadmin"]) ? "Notes have been added to the RFQ" : "The status on the RFQ has been updated"), admin = "yes" });
            }
            else
            {
                return RedirectToAction("ViewRFQ", new { id = rfqModel.ID });
            }
        }

        #endregion

        #region Edit Extention
        // GET: RFQTool/EditExt/5
        public async Task<ActionResult> EditExt(int? id)
        {
            ViewBag.extId = id;
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            RFQNewViewModelExt RFQNewViewModelExt = await db.RFQNewViewModelExts.FindAsync(id);
            if (RFQNewViewModelExt == null)
            {
                return HttpNotFound();
            }
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == userId).FirstOrDefault();

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            if (Request.QueryString["form_id"] != null)
            {
                RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(form_id);
                if (RFQNewViewModel == null)
                {
                    return HttpNotFound();
                }
                RFQNewViewModelExt.mainModel = RFQNewViewModel;
                var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == form_id).FirstOrDefault();
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == form_id.ToString());
                foreach (var item in RFQ_Ext.OrderBy(a => a.id))
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = form_id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                RFQNewViewModelExt.RFQExt = get_RFQ_Ext;
            }

            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.ext_form_id == id);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.ext_form_id == id);

            //Get installed parts
            if (rfq_installed_parts.Count() > 0)
            {
                RFQNewViewModelExt.list_installed_parts = rfq_installed_parts;
            }

            //Get installed parts
            if (rfq_shipped_parts.Count() > 0)
            {
                RFQNewViewModelExt.list_shipped_parts = rfq_shipped_parts;
            }

            //Get images
            var RFQ_New_Files_data = dbEntity.RFQ_New_Files.Where(a => a.ext_form_id == id);
            List<RFQ_New_File> get_RFQ_New_Files = new List<RFQ_New_File>();
            if (RFQ_New_Files_data.Count() > 0)
            {
                foreach (var item in RFQ_New_Files_data)
                {
                    get_RFQ_New_Files.Add(new RFQ_New_File
                    {
                        file_id = item.file_id,
                        ext_form_id = item.ext_form_id,
                        file_name = item.file_name,
                        file_type = item.file_type
                    });
                }
                RFQNewViewModelExt.list_RFQ_files = get_RFQ_New_Files;
            }

            //Get logs
            int formid = Convert.ToInt32(RFQNewViewModelExt.form_id);
            var RFQ_New_Action_Log = dbEntity.RFQ_New_Action_Log.Where(a => a.Form_ID ==formid);
            List<RFQ_New_Action_LogViewModel> get_RFQ_logs = new List<RFQ_New_Action_LogViewModel>();
            if (RFQ_New_Action_Log.Count() > 0)
            {
                foreach (var item in RFQ_New_Action_Log)
                {
                    //Get user's full name
                    int usrId = Convert.ToInt32(item.Usr_ID);
                    var getfullName = await locController.GetfullName(usrId);
                    //Get Admin Name 
                    int adminId = Convert.ToInt32(item.Admin_ID);
                    var getAdminfullName = await locController.GetfullName(adminId);

                    get_RFQ_logs.Add(new RFQ_New_Action_LogViewModel
                    {
                        Form_ID = Convert.ToInt32(item.Form_ID),
                        Action = item.Action,
                        Action_Time = item.Action_Time,
                        Notes = item.Notes,
                        Usr_ID = item.Usr_ID,
                        Admin_ID = item.Admin_ID,
                        fullName = getfullName["fullName"],
                        AdminfullName = getAdminfullName["firstName"]
                    });
                }
                //Check if it is ever been cloned
                var cloneddata = RFQ_New_Action_Log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                bool isCloned = false;
                if (cloneddata.Count() > 0)
                {
                    if (cloneddata.FirstOrDefault().Action == "Cloned/Submitted")
                    {
                        ViewBag.ClonedData = "Cloned Without Changes";
                    }
                    else
                    {
                        ViewBag.ClonedData = "Cloned With Changes";
                    }
                    isCloned = true;
                }
                RFQNewViewModelExt.IsCloned = isCloned;
            }

            List<SelectListItem> list_product_cats = new List<SelectListItem>();
            list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "select", Selected = true });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Indoor)", Value = "industrial_freestanding_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Freestading (Outdoor)", Value = "industrial_freestanding_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Indoor)", Value = "industrial_wallounted_indoor" });
            list_product_cats.Add(new SelectListItem { Text = "Industrial Wallmounted (Outdoor)", Value = "industrial_wallounted_outdoor" });
            list_product_cats.Add(new SelectListItem { Text = "IT Freestanding", Value = "it_freestanding" });
            list_product_cats.Add(new SelectListItem { Text = "Climate Control", Value = "climate_control" });
            list_product_cats.Add(new SelectListItem { Text = "Power Distribution", Value = "power_distribution" });
            list_product_cats.Add(new SelectListItem { Text = "Automation Systems", Value = "automation_systems" });
            list_product_cats.Add(new SelectListItem { Text = "Spare Parts", Value = "spare_parts" });
            RFQNewViewModelExt.list_prod_cat = list_product_cats;

            return View(RFQNewViewModelExt);
        }

        // POST: RFQ/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditExt([Bind(Include = "ID,sales_engineer,regional_director,cell_phone,email,submission_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,admin_notes,,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,plinths_type_ie,end_user")] RFQNewViewModelExt rFQDataExtend, IEnumerable<HttpPostedFileBase> fileupload)
        {
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);

                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                var form_id = Convert.ToInt32(rFQDataExtend.form_id);
                RFQNewViewModel rfqTemp = await db.RFQNewViewModels.AsNoTracking().Where(x => x.ID == form_id).FirstAsync();
                
                if (rfqTemp == null || rfqTemp.completion_date != null)
                {
                    return RedirectToAction("Index", new { form_id = rFQDataExtend.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }

                if (rFQDataExtend.save == null && rFQDataExtend.send == null)//save multiple parts
                {
                    if(rFQDataExtend.suited_bay_ie != ""  && rFQDataExtend.suited_bay_ie != null)
                    {
                        int bay = Convert.ToInt32(rFQDataExtend.suited_bay_ie);
                        bay += 1;
                        rFQDataExtend.suited_bay_ie = bay.ToString();
                    } else
                    {
                        rFQDataExtend.suited_bay_ie = "1";
                    }
                    
                    db.RFQNewViewModelExts.Add(rFQDataExtend);
                    await db.SaveChangesAsync();
                    await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                    //Save Installed and Shipped with parts
                    await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id, "edit");

                    return RedirectToAction("EditExt", new { id = rFQDataExtend.id, form_id = rFQDataExtend.form_id, first_time = "no", n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }
                else
                {
                    if (rFQDataExtend.save != null)
                    {
                        db.Entry(rFQDataExtend).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        int formId = Convert.ToInt32(rFQDataExtend.form_id);
                        //Select the RFQ and Update the status
                        var myRFQs = await db.RFQNewViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
                        myRFQs.save = "Save Progress";
                        myRFQs.send = "";
                        db.Entry(myRFQs).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id, "edit");
                        //Add the log
                        await logAction(rFQDataExtend.form_id, rFQDataExtend.save, "Saved from edit", userId.ToString());
                        //Don't send email upon first time save
                    }
                    else
                    {
                        db.Entry(rFQDataExtend).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        int formId = Convert.ToInt32(rFQDataExtend.form_id);
                        //Select the RFQ and Update the status
                        var myRFQs = await db.RFQNewViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
                        myRFQs.send = "Submitted Quote Request";
                        myRFQs.save = "";
                        myRFQs.submission_date = DateTime.Today;
                        db.Entry(myRFQs).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id, "edit");
                        //Add the log
                        await logAction(rFQDataExtend.form_id, rFQDataExtend.send, "Submitted from edit", userId.ToString());

                        //send email upon first time submit.
                    }

                    return RedirectToAction("Index", new { form_id = rFQDataExtend.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }
            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);

                List<ProductCategories> list_product_cats = new List<ProductCategories>();
                list_product_cats.Add(new ProductCategories { cat_name = "Industrial Freestading (Indoor)", value = "industrial_freestanding_indoor" });
                list_product_cats.Add(new ProductCategories { cat_name = "Industrial Freestading (Outdoor)", value = "industrial_freestanding_outdoor" });
                list_product_cats.Add(new ProductCategories { cat_name = "Industrial Wallmounted (Indoor)", value = "industrial_wallounted_indoor" });
                list_product_cats.Add(new ProductCategories { cat_name = "Industrial Wallmounted (Outdoor)", value = "industrial_wallounted_outdoor" });
                list_product_cats.Add(new ProductCategories { cat_name = "IT Freestanding", value = "it_freestanding" });
                list_product_cats.Add(new ProductCategories { cat_name = "Climate Control", value = "climate_control" });
                list_product_cats.Add(new ProductCategories { cat_name = "Power Distribution", value = "power_distribution" });
                list_product_cats.Add(new ProductCategories { cat_name = "Automation Systems", value = "automation_systems" });
                list_product_cats.Add(new ProductCategories { cat_name = "Spare Parts", value = "spare_parts" });
                ViewBag.list_prod_cat = list_product_cats;
            }

            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQNewViewModelExtPart> get_RFQ_Ext = new List<RFQNewViewModelExtPart>();
            if (Request.Form["form_id"] != null)
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);
                var RFQNewViewModel_data = db.RFQNewViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = RFQNewViewModel_data.product_category;
                ViewBag.total_qty = RFQNewViewModel_data.total_qty;
                ViewBag.original_id = RFQNewViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQNewViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                rFQDataExtend.RFQExt = get_RFQ_Ext;
            }
            return View(rFQDataExtend);
        }
        #endregion

        #region Clone RFQ
        public async Task<RedirectToRouteResult> CloneRFQ(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            Dictionary<string, string> status = await CloneRFQFunction(id);
            if (status["status"] == "OK")
            {
                return RedirectToAction("Index", new { form_id = status["id"], first_time = Request.Form["first_time"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.Form["msg"], success = "The Request has been cloned" });
            }
            else
            {
                return RedirectToAction("Index", new { form_id = status["id"], first_time = Request.Form["first_time"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.Form["msg"], success = "Clone failed" });
            }
        }

        public async Task<Dictionary<string, string>> CloneRFQFunction(int id)
        {
            Dictionary<string, string> status = new Dictionary<string, string>();
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    status.Add("status", "NotLoggedIn");
                    status.Add("id", "");
                    return status;
                }
                //Save RFQ
                RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
                RFQNewViewModel.send = "Cloned/Submitted";
                RFQNewViewModel.qte_num = RFQNewViewModel.Quote_Num;
                RFQNewViewModel.Quote_Num = null;
                RFQNewViewModel.updated_quote = "";
                RFQNewViewModel.submission_date = DateTime.Now;
                RFQNewViewModel.completion_date = null;
                RFQNewViewModel.admin_status = null;
                db.RFQNewViewModels.Add(RFQNewViewModel);
                await db.SaveChangesAsync();

                //Add the log
                await logAction(RFQNewViewModel.ID.ToString(), "Cloned/Submitted", "The RFQ id = " + RFQNewViewModel.form_id + " was cloned", userId.ToString());
                status.Add("status", "OK");
                status.Add("id", RFQNewViewModel.ID.ToString());
                return status;
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                status.Add("status", "Error");
                status.Add("id", "");
                return status;
            }
        }

        public async Task<RedirectResult> CloneRFQandSubmit(int id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return Redirect(Url.Action("Login", "Account"));
            }
            Dictionary<string, string> status = await CloneRFQFunctionCategories(id);
            if (status["status"] == "OK")
            {
                return Redirect(Url.Action("CloneRFQShippedandInstaled", new { formids = status["id"], first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], success = "The Request has been cloned" }));
            }
            else
            {
                return Redirect(Url.Action("Index", new { id = status["id"], first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], success = "The Clone failed" }));
            }
        }

        public async Task<Dictionary<string, string>> CloneRFQFunctionCategories(int id)
        {
            Dictionary<string, string> status = new Dictionary<string, string>();
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    status.Add("status", "NotLoggedIn");
                    status.Add("id", "");
                    return status;
                }
                //Save RFQ
                RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
                RFQNewViewModel.send = "Cloned with Changes/Not Submitted";
                RFQNewViewModel.qte_num = RFQNewViewModel.Quote_Num;
                RFQNewViewModel.Quote_Num = null;
                RFQNewViewModel.updated_quote = "";
                RFQNewViewModel.submission_date = DateTime.Now;
                RFQNewViewModel.completion_date = null;
                RFQNewViewModel.admin_status = null;
                db.RFQNewViewModels.Add(RFQNewViewModel);
                await db.SaveChangesAsync();
                string rfqid = id.ToString();
                //Save RFQ Extended
                List<CloneParts> statusExtIds = new List<CloneParts>();
                IQueryable<RFQNewViewModelExt> RFQNewViewModelExts = db.RFQNewViewModelExts.Where(a => a.form_id == rfqid);
                foreach (var Exts_item in RFQNewViewModelExts)
                {
                    Exts_item.form_id = RFQNewViewModel.ID.ToString();
                    db.RFQNewViewModelExts.Add(Exts_item);
                }
                await db.SaveChangesAsync();
                //Add the log
                await logAction(RFQNewViewModel.ID.ToString(), "Cloned with Changes/Not Submitted", "The RFQ id = " + RFQNewViewModel.form_id + " was cloned", userId.ToString());
                status.Add("status", "OK");
                status.Add("id", RFQNewViewModel.ID.ToString() + "," + id);
                return status;
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                status.Add("status", "Error");
                status.Add("id", "");
                return status;
            }
        }

        public async Task<RedirectResult> CloneRFQShippedandInstaled(string formids)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return Redirect(Url.Action("Login", "Account"));
            }
            string[] ids = formids.Split(',');
            int newid = Convert.ToInt32(ids[0]);
            int oldid = Convert.ToInt32(ids[1]);

            Dictionary<string, string> status = await CloneRFQFunctionInstalledParts(newid, oldid);
            if (status["status"] == "OK")
            {
                return Redirect(Url.Action("Edit", new { id = newid, first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], success = "The Request has been cloned" }));
            }
            else
            {
                return Redirect(Url.Action("Index", new { id = newid, first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], success = "The Clone failed" }));
            }
        }

        public async Task<Dictionary<string, string>> CloneRFQFunctionInstalledParts(int newid, int oldid)
        {
            Dictionary<string, string> status = new Dictionary<string, string>();
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    status.Add("status", "NotLoggedIn");
                    status.Add("id", "");
                    return status;
                }
                string old_id = oldid.ToString();
                string new_id = newid.ToString();

                var RFQNewViewModelExtsOld = await db.RFQNewViewModelExts.Where(a => a.form_id == old_id).ToListAsync();
                var RFQNewViewModelExtsNew = await db.RFQNewViewModelExts.Where(a => a.form_id == new_id).ToListAsync();
                List<CloneParts> CloneParts = new List<CloneParts>();
                foreach (var itemExt in RFQNewViewModelExtsNew.OrderByDescending(a => a.id))
                {
                    CloneParts.Add(new CloneParts { new_id = itemExt.id });
                }
                int x = 0;
                bool addedInstalled = false;
                //Save RFQ Installed parts
                IQueryable<RFQ_Parts_InstalledViewModel> rFQInstalled = db.RFQ_Parts_InstalledViewModels.Where(a => a.form_id == oldid);
                foreach (var itemExt in RFQNewViewModelExtsOld.OrderByDescending(a => a.id))
                {
                    foreach (var item in rFQInstalled)
                    {
                        if (itemExt.id == item.ext_form_id)
                        {
                            item.form_id = newid;
                            item.ext_form_id = CloneParts[x].new_id;
                            db.RFQ_Parts_InstalledViewModels.Add(item);
                        }
                        if (item.ext_form_id == 0 && !addedInstalled)
                        {
                            item.form_id = newid;
                            db.RFQ_Parts_InstalledViewModels.Add(item);
                            addedInstalled = true;
                        }
                    }
                    x++;
                }
                await db.SaveChangesAsync();
                //Save RFQ Shipped parts
                int y = 0;
                bool addedShipped = false;
                //Save RFQ Installed parts
                IQueryable<RFQ_Parts_ShippedViewModel> rFQShipped = db.RFQ_Parts_ShippedViewModels.Where(a => a.form_id == oldid);
                foreach (var itemExt in RFQNewViewModelExtsOld.OrderByDescending(a => a.id))
                {
                    foreach (var item in rFQShipped)
                    {
                        if (itemExt.id == item.ext_form_id)
                        {
                            item.form_id = newid;
                            item.ext_form_id = CloneParts[y].new_id;
                            db.RFQ_Parts_ShippedViewModels.Add(item);
                        }
                        if (item.ext_form_id == 0 && !addedShipped)
                        {
                            item.form_id = newid;
                            db.RFQ_Parts_ShippedViewModels.Add(item);
                            addedShipped = true;
                        }
                    }
                    y++;
                }
                await db.SaveChangesAsync();
                //Save RFQ Files
                int z = 0;
                //Save RFQ Installed parts
                IQueryable<RFQ_New_File> rFQFiles = db.RFQNewFiles.Where(a => a.form_id == oldid);
                foreach (var itemExt in RFQNewViewModelExtsOld.OrderByDescending(a => a.id))
                {
                    foreach (var item in rFQFiles)
                    {
                        if (itemExt.id == item.ext_form_id)
                        {
                            item.form_id = newid;
                            item.ext_form_id = CloneParts[z].new_id;
                            db.RFQNewFiles.Add(item);
                        }
                    }
                    z++;
                }
                //Handle zero files
                foreach (var item in rFQFiles)
                {
                    if (item.ext_form_id == 0)
                    {
                        item.form_id = newid;
                        item.ext_form_id = 0;
                        db.RFQNewFiles.Add(item);
                    }
                }
                await db.SaveChangesAsync();

                status.Add("status", "OK");
                status.Add("id", "");
                return status;
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                status.Add("status", "Error");
                status.Add("id", "");
                return status;
            }
        }
        #endregion

        #region Admin
        public StringBuilder GetRFQAdmins(string querystring = "")
        {
            StringBuilder string_admins = new StringBuilder();
            string_admins.Append("<ul id=\"admin_list\" >");
            try
            {
                var user_data = db.UserViewModels.Where(a => a.usr_fName.Contains(querystring));
                foreach (var part in user_data)
                {
                    string_admins.Append("<li><a href=\"#\" id=\"" + part.usr_ID + "\" onclick=\"processRFQs.assign(this.id); return false;\">" + part.usr_fName + " " + part.usr_lName + "</a></li>");
                }
                string_admins.Append("</ul>");

                return string_admins;
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return string_admins.Append("action failed");
            }
        }

        public async Task<JsonResult> ManageAssignRFQ(int rfq_id, int usr_id,string assign=null)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                int user_id = Convert.ToInt32(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("action failed");
                }
                string host = "";
                if (Request.Url.Port != 443)
                {
                    host = "http://" + Request.Url.Host + ":" + Request.Url.Port;
                }
                else
                {
                    host = "https://" + Request.Url.Host;
                }

                string header = locController.emailheader(host);
                string footer = locController.emailfooter(host);
                MailMessage message_requester;
                List<string> files = new List<string>();

                var getAssignerfullName = await locController.GetfullName(user_id);
                //Add the log
                //var getAdminfullName = await locController.getfullName(usr_id);
                //Get RFQ
                var RFQNewViewModel = await db.RFQNewViewModels.Where(a => a.ID == rfq_id).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(RFQNewViewModel.admin_status) || assign=="yes")
                {
                    //Get user information
                    int requester_id = Convert.ToInt32(RFQNewViewModel.user_id);
                    var getuserdata = await db.UserViewModels.Where(a => a.usr_ID == requester_id).FirstOrDefaultAsync();
                    var getAdmindata = await db.UserViewModels.Where(a => a.usr_ID == usr_id).FirstOrDefaultAsync();
                    string salutaion = getAdmindata.usr_fName + " " + getAdmindata.usr_lName +
                                        ",<br />Deal Desk<br />" +
                                        "Phone: (937) 399 - 0500 - Toll Free: 1 - 800 - 477 - 4220 - Fax: 1 - 800 - 477 - 4003<br />" +
                                        "Phone: " + getAdmindata.usr_phone + "  · Fax: " + getAdmindata.usr_fax + " · Email: " + getAdmindata.usr_email + "<br />";
                    //Add the log
                    await logAction(rfq_id.ToString(), "Assigned", "The RFQ id=" + rfq_id.ToString() + " was assigned by " + getAssignerfullName["fullName"] + " to " + getAdmindata.usr_fName + " " + getAdmindata.usr_lName, userId.ToString(), usr_id);

                    //Send the return email
                    string body_requester = "Your request for quote has been assigned.<br /><br />" +
                                             "Request ID: " + rfq_id + "<br /><br />" +
                                             "Thank you for offering Rittal the opportunity to provide a solution for  " + RFQNewViewModel.sold_to_party +
                                             ". My name is " + getAdmindata.usr_fName + " " + getAdmindata.usr_lName + " and I will be handling your request. I am based in our Ohio facility and can be reached via the information below should you need to communicate any additional information or changes. As soon as I complete your request you will receive an immediate email to let you know your quotation is ready. The majority of our requests are completed within one business day.<br /><br />" + salutaion;

                    //Send email to the Return Requester
                    message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been assigned #" + RFQNewViewModel.ID + "", header + body_requester + footer);
                    locController.sendEmailSingle(message_requester, files);

                    RFQNewViewModel.admin_status = "Assigned";
                    await db.SaveChangesAsync();

                    return Json(getAdmindata.usr_fName + " " + getAdmindata.usr_lName);
                }
                else
                {
                    return Json("taken");
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("action failed");
            }
        }
        #endregion

        #region Save File
        async Task SaveImages(IEnumerable<HttpPostedFileBase> fileupload, int id, int extid, int user_id)
        {
            //Process the attached images
            foreach (HttpPostedFileBase file in fileupload)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // and optionally write the file to disk
                    var fileName = Path.GetFileName(file.FileName);
                    var guid = Guid.NewGuid().ToString();
                    var NewFileName = guid + fileName;
                    var path = Path.Combine(Server.MapPath("~/attachments/rfq_tool/images"), NewFileName);

                    file.SaveAs(path);
                    //if file_id is not set or the entry is empty
                    RFQ_New_Files = new RFQ_New_File
                    {
                        form_id = id,
                        ext_form_id = extid,
                        user_id = user_id,
                        file_name = NewFileName,
                        file_type = Path.GetExtension(file.FileName)
                    };
                    db.RFQNewFiles.Add(RFQ_New_Files);
                    await db.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region save Installed or Shipped
        async Task saveInstalledShipped(int form_id, int ext_form_id, int user_id, string action = null)
        {
            if (!string.IsNullOrEmpty(Request.Form["qty_installed"]))
            {
                string[] qty_installed = Request.Form["qty_installed"].Split(',');
                var rfq_installed = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == form_id && a.ext_form_id == ext_form_id);
                if (!string.IsNullOrEmpty(action) && action == "edit" && rfq_installed.Count() > 0)
                {
                    //insert installed parts
                    rfq_installed.FirstOrDefault().qty_installed = Request.Form["qty_installed"];
                    rfq_installed.FirstOrDefault().part_number_installed = Request.Form["part_number_installed"];
                    rfq_installed.FirstOrDefault().description_installed = Request.Form["description_installed"];
                    await dbEntity.SaveChangesAsync();
                }
                else
                {
                    if (qty_installed.Count() >= 0)
                    {
                        var rFQInstalled = new RFQ_Parts_Installed
                        {
                            form_id = form_id,
                            ext_form_id = ext_form_id,
                            qty_installed = Request.Form["qty_installed"],
                            part_number_installed = Request.Form["part_number_installed"],
                            description_installed = Request.Form["description_installed"],
                            user_id = user_id
                        };
                        dbEntity.RFQ_Parts_Installed.Add(rFQInstalled);
                        await dbEntity.SaveChangesAsync();
                    }
                }
            }
            else
            {
                //insert installed parts
                var rfq_installed = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == form_id && a.ext_form_id == ext_form_id);
                if (!string.IsNullOrEmpty(action) && action == "edit" && rfq_installed.Count() > 0)
                {
                    rfq_installed.FirstOrDefault().qty_installed = "";
                    rfq_installed.FirstOrDefault().part_number_installed = "";
                    rfq_installed.FirstOrDefault().description_installed = "";
                    await dbEntity.SaveChangesAsync();
                }
                else
                {
                    var rFQInstalled = new RFQ_Parts_Installed
                    {
                        form_id = form_id,
                        ext_form_id = ext_form_id,
                        qty_installed = "",
                        part_number_installed = "",
                        description_installed = "",
                        user_id = user_id
                    };
                    dbEntity.RFQ_Parts_Installed.Add(rFQInstalled);
                    await dbEntity.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(Request.Form["qty_shipped"]))
            {
                string[] qty_shipped = Request.Form["qty_shipped"].Split(',');
                //insert shipped parts
                var rfq_shipped = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == form_id && a.ext_form_id == ext_form_id);
                if (!string.IsNullOrEmpty(action) && action == "edit" && rfq_shipped.Count() > 0)
                {
                    rfq_shipped.FirstOrDefault().qty_shipped = Request.Form["qty_shipped"];
                    rfq_shipped.FirstOrDefault().part_number_shipped = Request.Form["part_number_shipped"];
                    rfq_shipped.FirstOrDefault().description_shipped = Request.Form["description_shipped"];
                    await dbEntity.SaveChangesAsync();
                }
                else
                {
                    //insert shipped parts
                    if (qty_shipped.Count() >= 0)
                    {
                        var rFQShipped = new RFQ_Parts_Shipped
                        {
                            form_id = form_id,
                            ext_form_id = ext_form_id,
                            qty_shipped = Request.Form["qty_shipped"],
                            part_number_shipped = Request.Form["part_number_shipped"],
                            description_shipped = Request.Form["description_shipped"],
                            user_id = user_id
                        };
                        dbEntity.RFQ_Parts_Shipped.Add(rFQShipped);
                        await dbEntity.SaveChangesAsync();
                    }
                }
            }
            else
            {
                //insert shipped parts
                var rfq_shipped = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == form_id && a.ext_form_id == ext_form_id);
                if (!string.IsNullOrEmpty(action) && action == "edit" && rfq_shipped.Count() > 0)
                {
                    rfq_shipped.FirstOrDefault().qty_shipped = "";
                    rfq_shipped.FirstOrDefault().part_number_shipped = "";
                    rfq_shipped.FirstOrDefault().description_shipped = "";
                    await dbEntity.SaveChangesAsync();
                }
                else
                {
                    var rFQShipped = new RFQ_Parts_Shipped
                    {
                        form_id = form_id,
                        ext_form_id = ext_form_id,
                        qty_shipped = "",
                        part_number_shipped = "",
                        description_shipped = "",
                        user_id = user_id
                    };
                    dbEntity.RFQ_Parts_Shipped.Add(rFQShipped);
                    await dbEntity.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region log Action
        async Task logAction(string form_id, string action, string notes, string user_id, int usr_id = 0, bool bAssign = true)
        {
            RFQ_New_Action_Log rFQActionLog = new RFQ_New_Action_Log();
            int ids = Convert.ToInt32(form_id);
            var RFQ_New_Action_Log = db.RFQ_New_Action_LogViewModels.Where(a => a.Form_ID == ids);
            var lastObj = RFQ_New_Action_Log.OrderByDescending(a => a.ID).FirstOrDefault();
            if (bAssign)
            {
                if (RFQ_New_Action_Log.Count() > 0 && usr_id == 0)
                {
                    rFQActionLog = new RFQ_New_Action_Log
                    {
                        Form_ID = Convert.ToInt32(form_id),
                        Action = action,
                        Action_Time = DateTime.Now,
                        Notes = notes,
                        Usr_ID = user_id,
                        Admin_ID = lastObj.Admin_ID
                    };
                }
                else if (RFQ_New_Action_Log.Count() > 0 && usr_id != 0)
                {
                    rFQActionLog = new RFQ_New_Action_Log
                    {
                        Form_ID = Convert.ToInt32(form_id),
                        Action = action,
                        Action_Time = DateTime.Now,
                        Notes = notes,
                        Usr_ID = user_id,
                        Admin_ID = usr_id.ToString()
                    };
                }
            }
            if(!bAssign || RFQ_New_Action_Log.Count() == 0)
            {
                rFQActionLog = new RFQ_New_Action_Log
                {
                    Form_ID = Convert.ToInt32(form_id),
                    Action = (action != "" ? action : (lastObj != null ? lastObj.Action : "")), 
                    Action_Time = DateTime.Now,
                    Notes = notes,
                    Usr_ID = user_id
                };
                if (!bAssign && RFQ_New_Action_Log.Count() > 0 && !string.IsNullOrEmpty(lastObj.Admin_ID))
                    rFQActionLog.Admin_ID = lastObj.Admin_ID;
            }
            dbEntity.RFQ_New_Action_Log.Add(rFQActionLog);
            await dbEntity.SaveChangesAsync();
        }
        #endregion

        #region Delete
        public async Task<ActionResult> DeleteImage(int? id, int? form_id = 0, int? ext_form_id = 0)
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
            //Get the file data
            var rFQFileData = await db.RFQNewFiles.FindAsync(id);
            if (rFQFileData == null)
            {
                return HttpNotFound();
            }
            db.RFQNewFiles.Remove(rFQFileData);
            await db.SaveChangesAsync();

            if (Request.QueryString["ext_form"] == "no")
            {
                return Redirect(Url.Action("Edit", new { id = form_id, first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], imagesuccess = "The image has been removed" }) + "#images");
            }
            else
            {
                return Redirect(Url.Action("EditExt", new { id = ext_form_id, form_id = form_id, n1_name = Request.QueryString["n1_name"], first_time = "no", n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], ext_form = Request.QueryString["ext_form"], imagesuccess = "The image has been removed" }) + "#images");
            }
        }

        public async Task<ActionResult> Deleteproducts(int? id)
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
            //Get the installed parts data
            var rFQInstalled = db.RFQ_Parts_InstalledViewModels.Where(a => a.ext_form_id == id);
            if (rFQInstalled != null)
            {
                db.RFQ_Parts_InstalledViewModels.RemoveRange(rFQInstalled);
            }
            //Get the shipped parts data
            var rFQShipped = db.RFQ_Parts_ShippedViewModels.Where(a => a.ext_form_id == id);
            if (rFQShipped != null)
            {
                db.RFQ_Parts_ShippedViewModels.RemoveRange(rFQShipped);
            }
            //Get the file data
            var rFQFileData = db.RFQNewFiles.Where(a => a.ext_form_id == id);
            if (rFQFileData != null)
            {
                db.RFQNewFiles.RemoveRange(rFQFileData);
            }

            RFQNewViewModelExt rFQDataExtend = await db.RFQNewViewModelExts.FindAsync(id);
            if (rFQDataExtend == null)
            {
                return HttpNotFound();
            }
            db.RFQNewViewModelExts.Remove(rFQDataExtend);
            await db.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = rFQDataExtend.form_id, first_time = "no", n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], msg = Request.QueryString["msg"], success = "The part has been deleted" });
        }


        // GET: RFQ/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);

            if (RFQNewViewModel == null || RFQNewViewModel.completion_date != null)
            {
                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], error = "Unable to delete RFQ." });
            }

            //Get the installed parts data
            var rFQInstalled = db.RFQ_Parts_InstalledViewModels.Where(a => a.form_id == id);
            if (rFQInstalled != null)
            {
                db.RFQ_Parts_InstalledViewModels.RemoveRange(rFQInstalled);
            }
            //Get the shipped parts data
            var rFQShipped = db.RFQ_Parts_ShippedViewModels.Where(a => a.form_id == id);
            if (rFQShipped != null)
            {
                db.RFQ_Parts_ShippedViewModels.RemoveRange(rFQShipped);
            }

            /*RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
            if (RFQNewViewModel == null)
            {
                return HttpNotFound();
            }*/
            var rFQDataExtend = db.RFQNewViewModelExts.Where(a => a.form_id == id.ToString());
            if (rFQDataExtend != null)
            {
                db.RFQNewViewModelExts.RemoveRange(rFQDataExtend);
            }
            var rFQFileData = db.RFQNewFiles.Where(a => a.form_id == id);
            if (rFQFileData != null)
            {
                db.RFQNewFiles.RemoveRange(rFQFileData);
            }

            db.RFQNewViewModels.Remove(RFQNewViewModel);

            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The RFQ has been deleted" });
        }

        // POST: RFQ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RFQNewViewModel RFQNewViewModel = await db.RFQNewViewModels.FindAsync(id);
            db.RFQNewViewModels.Remove(RFQNewViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Get Products
        [HttpPost]
        public async Task<JsonResult> GetProducts([Bind(Include = "height,width,depth,enclosure_type,material,family,rating,product_type,industry,return_filters,dimension_type,height_IT,width_IT,depth_IT,part_number")] RFQSearchProduct inputModel)
        {
            var prodsObj = dbEntity.products.Where(x => !string.IsNullOrEmpty(x.part_number));
            bool returnFilters = false;
            if(inputModel != null)
            {
                if (!string.IsNullOrEmpty(inputModel.part_number))
                {
                    prodsObj = prodsObj.Where(x => x.part_number == inputModel.part_number);
                }
                else
                {
                    if (inputModel.return_filters)
                        returnFilters = true;
                    int variance = ConfigurationManager.AppSettings["RFQPartDimensionVariance"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["RFQPartDimensionVariance"]) : 50;
                    if (!string.IsNullOrEmpty(inputModel.product_type))
                    {
                        if (inputModel.product_type.ToLower().Contains("freestanding"))
                        {
                            variance = 100;
                        }
                        else if (inputModel.product_type.ToLower().Contains("wallmounted"))
                        {
                            variance = 50;
                        }
                        prodsObj = prodsObj.Where(x => x.product_type == inputModel.product_type);
                    }
                    if (!string.IsNullOrEmpty(inputModel.industry))
                    {
                        prodsObj = prodsObj.Where(x => x.industry == inputModel.industry);
                    }
                    if (!string.IsNullOrEmpty(inputModel.enclosure_type))
                    {
                        prodsObj = prodsObj.Where(x => x.enclosure_type == inputModel.enclosure_type);
                    }
                    if (!string.IsNullOrEmpty(inputModel.material))
                    {
                        prodsObj = prodsObj.Where(x => x.material.Contains(inputModel.material));
                    }
                    if (!string.IsNullOrEmpty(inputModel.family))
                    {
                        prodsObj = prodsObj.Where(x => x.family == inputModel.family);
                    }
                    if (!string.IsNullOrEmpty(inputModel.rating))
                    {
                        prodsObj = prodsObj.Where(x => x.rating == inputModel.rating);
                    }
                    if (inputModel.width > 0)
                    {
                        if (inputModel.dimension_type == "in")
                            inputModel.width = Convert.ToInt32(inputModel.width * 25.4);
                        prodsObj = prodsObj.Where(x => x.width >= inputModel.width - variance && x.width <= inputModel.width + variance);
                    }
                    else if (inputModel.width_IT > 0)
                    {
                        prodsObj = prodsObj.Where(x => x.width == inputModel.width_IT);
                    }
                    if (inputModel.height > 0)
                    {
                        if (inputModel.dimension_type == "in")
                            inputModel.height = Convert.ToInt32(inputModel.height * 25.4);
                        prodsObj = prodsObj.Where(x => x.height >= inputModel.height - variance && x.height <= inputModel.height + variance);
                    }
                    else if (!string.IsNullOrEmpty(inputModel.height_IT))
                    {
                        int heightIT = Convert.ToInt32(inputModel.height_IT);
                        prodsObj = prodsObj.Where(x => x.height == heightIT);
                    }
                    if (inputModel.depth > 0)
                    {
                        if (inputModel.dimension_type == "in")
                            inputModel.depth = Convert.ToInt32(inputModel.depth * 25.4);
                        prodsObj = prodsObj.Where(x => x.depth >= inputModel.depth - variance && x.depth <= inputModel.depth + variance);
                    }
                    else if (inputModel.depth_IT > 0)
                    {
                        prodsObj = prodsObj.Where(x => x.depth == inputModel.depth_IT);
                    }
                }
            }
            List<product> products = await prodsObj.OrderBy(x => x.enclosure_type).ToListAsync();
            List<string> materials = new List<string>();
            List<string> family = new List<string>();
            List<string> rating = new List<string>(); 
            List<string> enclosureType = new List<string>();
            if (returnFilters)
            {
                materials = products.Where(x => !string.IsNullOrEmpty(x.material)).Select(x => x.material).Distinct().OrderBy(x => x).ToList();
                family = products.Where(x => !string.IsNullOrEmpty(x.family)).Select(x => x.family).Distinct().OrderBy(x => x).ToList();
                rating = products.Where(x => !string.IsNullOrEmpty(x.rating)).Select(x => x.rating).Distinct().OrderBy(x => x).ToList();
                enclosureType = products.Where(x => !string.IsNullOrEmpty(x.enclosure_type)).Select(x => x.enclosure_type).Distinct().OrderBy(x => x).ToList();
            }
            return Json(new { products = products, material = materials, family = family, rating = rating, enclosure_type = enclosureType });
        }

        [HttpPost]
        public async Task<JsonResult> GetAccessories([Bind(Include = "product_type,product_family,group_1,group_2,description_1,description_2,description_3,color,material")] RFQSearchAccessory inputModel)
        {
            var partsObj = dbEntity.accessories.Where(x => !string.IsNullOrEmpty(x.part_number));
            if (inputModel != null)
            {
                if (!string.IsNullOrEmpty(inputModel.product_type))
                {
                    partsObj = partsObj.Where(x => x.product_type == inputModel.product_type);
                }
                if (!string.IsNullOrEmpty(inputModel.product_family))
                {
                    partsObj = partsObj.Where(x => x.family.Contains(inputModel.product_family));
                }
                if (!string.IsNullOrEmpty(inputModel.group_1))
                {
                    partsObj = partsObj.Where(x => x.group_1 == inputModel.group_1);
                }
                if (!string.IsNullOrEmpty(inputModel.group_2))
                {
                    partsObj = partsObj.Where(x => x.group_2 == inputModel.group_2);
                }
                if (!string.IsNullOrEmpty(inputModel.description_1))
                {
                    partsObj = partsObj.Where(x => x.description_1 == inputModel.description_1);
                }
                if (!string.IsNullOrEmpty(inputModel.description_2))
                {
                    partsObj = partsObj.Where(x => x.description_2 == inputModel.description_2);
                }
                if (!string.IsNullOrEmpty(inputModel.description_3))
                {
                    partsObj = partsObj.Where(x => x.description_3 == inputModel.description_3);
                }
                if (!string.IsNullOrEmpty(inputModel.color))
                {
                    partsObj = partsObj.Where(x => x.color == inputModel.color);
                }
                if (!string.IsNullOrEmpty(inputModel.material))
                {
                    partsObj = partsObj.Where(x => x.material == inputModel.material);
                }
            }
            List<accessory> accessories = await partsObj.OrderBy(x => x.part_number).ToListAsync();
            return Json(new { accessories });
        }

        [HttpPost]
        public async Task<JsonResult> GetAccessoryGroups([Bind(Include = "product_type,product_family,group_1")] RFQSearchAccessory inputModel)
        {
            var partsObj = dbEntity.accessories.Where(x => x.product_type == inputModel.product_type && x.family.Contains(inputModel.product_family));
            List<string> groups = new List<string>();
            if (!string.IsNullOrEmpty(inputModel.group_1))
            {
                groups = await partsObj.Where(x => !string.IsNullOrEmpty(x.group_2) && x.group_1 == inputModel.group_1).Select(x => x.group_2).Distinct().OrderBy(x => x).ToListAsync();
            }
            else
            {
                groups = await partsObj.Where(x => !string.IsNullOrEmpty(x.group_1)).Select(x => x.group_1).Distinct().OrderBy(x => x).ToListAsync();
            }
            return Json(new { groups });
        }

        [HttpPost]
        public async Task<JsonResult> GetFans([Bind(Include = "type,voltage,cfm,color,part_number")] RFQSearchClimate inputModel)
        {
            var prodsObj = dbEntity.fan_products.Where(x => !string.IsNullOrEmpty(x.part_number));
            if (inputModel != null)
            {
                if(!string.IsNullOrEmpty(inputModel.part_number))
                {
                    prodsObj = prodsObj.Where(x => x.part_number == inputModel.part_number);
                }
                if (!string.IsNullOrEmpty(inputModel.type))
                {
                    prodsObj = prodsObj.Where(x => x.description.ToLower() == inputModel.type.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.voltage))
                {
                    prodsObj = prodsObj.Where(x => x.voltage == inputModel.voltage);
                }
                if (!string.IsNullOrEmpty(inputModel.color))
                {
                    prodsObj = prodsObj.Where(x => x.color == inputModel.color);
                }
            }
            List<fan_products> productsList = await prodsObj.OrderBy(x => x.part_number).ToListAsync();
            List<fan_products> products = new List<fan_products>();
            if (!string.IsNullOrEmpty(inputModel.cfm))
            {
                string[] parts = inputModel.cfm.Split('-');
                foreach (fan_products product in productsList)
                {
                    if(int.Parse(product.cfm.Replace("cfm", "").Trim()) >= Convert.ToInt32(parts[0].Trim()) && int.Parse(product.cfm.Replace("cfm", "").Trim()) <= Convert.ToInt32(parts[1].Trim()))
                    {
                        products.Add(product);
                    }
                }
            }
            else
            {
                products = productsList;
            }
            return Json(new { products = products });
        }

        [HttpPost]
        public async Task<JsonResult> GetFanFilters([Bind(Include = "type,color,part_number")] RFQSearchClimate inputModel)
        {
            var prodsObj = dbEntity.fan_filters.Where(x => !string.IsNullOrEmpty(x.part_number));
            if (inputModel != null)
            {
                if (!string.IsNullOrEmpty(inputModel.part_number))
                {
                    prodsObj = prodsObj.Where(x => x.part_number == inputModel.part_number);
                }
                if (!string.IsNullOrEmpty(inputModel.type))
                {
                    prodsObj = prodsObj.Where(x => x.description.ToLower().Contains(inputModel.type.ToLower()));
                }
                if (!string.IsNullOrEmpty(inputModel.color))
                {
                    prodsObj = prodsObj.Where(x => x.color == inputModel.color);
                }
            }
            List<fan_filters> products = await prodsObj.OrderBy(x => x.part_number).ToListAsync();
            return Json(new { products = products });
        }

        [HttpPost]
        public async Task<JsonResult> GetHeatExchangers([Bind(Include = "type,version,voltage,btu,part_number")] RFQSearchClimate inputModel)
        {
            var prodsObj = dbEntity.heat_exchangers.Where(x => !string.IsNullOrEmpty(x.part_number));
            if (inputModel != null)
            {
                if (!string.IsNullOrEmpty(inputModel.part_number))
                {
                    prodsObj = prodsObj.Where(x => x.part_number == inputModel.part_number);
                }
                if (!string.IsNullOrEmpty(inputModel.type))
                {
                    prodsObj = prodsObj.Where(x => x.type.ToLower() == inputModel.type.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.version))
                {
                    prodsObj = prodsObj.Where(x => x.version.ToLower() == inputModel.version.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.voltage))
                {
                    prodsObj = prodsObj.Where(x => x.operatIng_voltage.ToLower() == inputModel.voltage.ToLower());
                }
            }
            List<heat_exchangers> productsList = await prodsObj.OrderBy(x => x.part_number).ToListAsync();
            List<heat_exchangers> products = new List<heat_exchangers>();
            if (!string.IsNullOrEmpty(inputModel.btu))
            {
                string[] parts = inputModel.btu.Split('-');
                foreach (heat_exchangers product in productsList)
                {
                    if (int.Parse(product.btu.Trim()) >= Convert.ToInt32(parts[0].Trim()) && int.Parse(product.btu.Trim()) <= Convert.ToInt32(parts[1].Trim()))
                    {
                        products.Add(product);
                    }
                }
            }
            else
            {
                products = productsList;
            }
            return Json(new { products = products });
        }

        [HttpPost]
        public async Task<JsonResult> GetACs([Bind(Include = "type,material,ul_nema,btu,kw,voltage,phase,part_number")] RFQSearchClimate inputModel)
        {
            var prodsObj = dbEntity.ac_products.Where(x => !string.IsNullOrEmpty(x.part_number));
            if (inputModel != null)
            {
                if (!string.IsNullOrEmpty(inputModel.part_number))
                {
                    prodsObj = prodsObj.Where(x => x.part_number == inputModel.part_number);
                }
                if (!string.IsNullOrEmpty(inputModel.type))
                {
                    prodsObj = prodsObj.Where(x => x.type.ToLower() == inputModel.type.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.voltage))
                {
                    prodsObj = prodsObj.Where(x => x.voltage.ToLower() == inputModel.voltage.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.material))
                {
                    prodsObj = prodsObj.Where(x => x.material.ToLower() == inputModel.material.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.ul_nema))
                {
                    prodsObj = prodsObj.Where(x => x.ul_nema.ToLower() == inputModel.ul_nema.ToLower());
                }
                if (!string.IsNullOrEmpty(inputModel.phase))
                {
                    prodsObj = prodsObj.Where(x => x.phase.ToLower() == inputModel.phase.ToLower());
                }
            }
            List<ac_products> productsList = await prodsObj.OrderBy(x => x.part_number).ToListAsync();
            List<ac_products> products = new List<ac_products>();
            if (!string.IsNullOrEmpty(inputModel.btu) || !string.IsNullOrEmpty(inputModel.kw))
            {
                string[] parts = !string.IsNullOrEmpty(inputModel.btu) ? inputModel.btu.Split('-') : null;
                string[] kwParts = !string.IsNullOrEmpty(inputModel.kw) ? inputModel.kw.Split('-') : null;
                foreach (ac_products product in productsList)
                {
                    if ((string.IsNullOrEmpty(inputModel.btu) || (int.Parse(product.btu.Trim()) >= Convert.ToInt32(parts[0].Trim()) && int.Parse(product.btu.Trim()) <= Convert.ToInt32(parts[1].Trim()))) && (string.IsNullOrEmpty(inputModel.kw) || (Convert.ToDecimal(product.kw.Trim()) >= Convert.ToDecimal(parts[0].Trim()) && Convert.ToDecimal(product.kw.Trim()) <= Convert.ToDecimal(parts[1].Trim()))))
                    {
                        products.Add(product);
                    }
                }
            }
            else
            {
                products = productsList;
            }
            return Json(new { products = products });
        }

        #endregion

        [HttpPost]
        public async Task<JsonResult> SaveEnclosures([Bind(Include = "rfqID, enclosures, variant")] RFQEditEnclosure inputModel)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            RFQNewViewModel rfq = await db.RFQNewViewModels.Where(x => x.ID == inputModel.rfqID).FirstAsync();
            if(rfq != null)
            {
                rfq.enclosures = inputModel.enclosures;
                if(inputModel.variant.HasValue && inputModel.variant.Value > 0)
                {
                    rfq.variant = inputModel.variant.Value;
                }
                db.Entry(rfq).State = EntityState.Modified;
                await db.SaveChangesAsync();
                await logAction(inputModel.rfqID.ToString(), "Submitted Quote Request", "Re-Submitted from edit", userId.ToString());
            }
            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbEntity.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

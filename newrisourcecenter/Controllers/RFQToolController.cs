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

namespace newrisourcecenter.Controllers
{
    public partial class RFQToolController : Controller
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
            new SelectListItem { Text = "Secarex (Discontinued, Select CT M or CT H)", Value = "Secarex" },
            new SelectListItem { Text = "Special Spare Part", Value = "Special Spare Part" },
            new SelectListItem { Text = "CT M - Cutting Terminal Manual", Value = "CT M - Cutting Terminal Manual" },
            new SelectListItem { Text = "CT H - Cutting Terminal Hydraulic", Value = "CT M - Cutting Terminal Hydraulic" },
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
        RFQ_File rFQ_Files = new RFQ_File();
        #region Index
        // GET: RFQ
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RFQViewModel> myRFQs = await db.RFQViewModels.Where(a => a.user_id == userId).OrderByDescending(a => a.ID).ToListAsync();
            List<RFQViewModel> list_rfqs = new List<RFQViewModel>();

            foreach (var rfq in myRFQs)
            {
                var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == rfq.ID);
                //Get logs
                List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                if (rfq_action_log.Count() > 0)
                {
                    //Check if it is ever been cloned
                    var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
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

                    foreach (var item in rfq_action_log)
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

                        get_RFQ_logs.Add(new RFQ_Action_LogViewModel
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

                list_rfqs.Add(new RFQViewModel
                {
                    ID = rfq.ID,
                    qte_num = rfq.Quote_Num,
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
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolTesters"].Split(',');
            if (newTesters.Contains(Convert.ToString(Session["userId"])))
            {
                ViewBag.readonlyView = "true";
            }
            else
            {
                ViewBag.readonlyView = "false";
            }
            return View(list_rfqs);
        }
        #endregion

        #region RFQ Admin
        // GET: RFQ
        public async Task<ActionResult> RfqAdmin(string filter=null, string filter_region = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RFQViewModel> myRFQs;
            if (filter== "Completed this month")
            {
                myRFQs = await db.RFQViewModels.Where(a => a.send == "Completed" && a.completion_date.Value.Month.Equals(DateTime.Today.Month) && a.completion_date.Value.Year.Equals(DateTime.Today.Year)).OrderByDescending(a => a.ID).ToListAsync();
            }
            else
            {
               myRFQs = await db.RFQViewModels.Where(a => a.save != "Save Progress" && a.send != "Completed" && a.send != "Cloned with Changes/Not Submitted").OrderByDescending(a => a.ID).ToListAsync();
            }
            List<string> compRegions = new List<string>();
            if (!string.IsNullOrEmpty(filter_region))
            {
                compRegions = await db.partnerCompanyViewModels.Where(x => x.comp_region == filter_region).Select(x => x.comp_name).ToListAsync();
            }
            List<RFQViewModel> list_rfqs = new List<RFQViewModel>();
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
                var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == rfq.ID);
                //Get logs
                List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                if (rfq_action_log.Count() > 0)
                {
                    //Check if it is ever been cloned
                    var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
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

                    foreach (var item in rfq_action_log)
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

                        get_RFQ_logs.Add(new RFQ_Action_LogViewModel
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
                    if (rfq_action_log.Count() > 0 && !string.IsNullOrEmpty(rfq_action_log.OrderByDescending(a=>a.ID).FirstOrDefault().Admin_ID))
                    {
                        list_rfqs.Add(new RFQViewModel
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
                            part_type_other = rfq.part_type_other,
                            mods_it = rfq.mods_it,
                            product_category = rfq.product_category
                        });
                    }
                }
                else if (filter == "Not Assigned")
                {
                    if (rfq_action_log.Count() > 0 && string.IsNullOrEmpty(rfq_action_log.OrderByDescending(a => a.ID).FirstOrDefault().Admin_ID))
                    {
                        list_rfqs.Add(new RFQViewModel
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
                            part_type_other = rfq.part_type_other,
                            mods_it = rfq.mods_it,
                            product_category = rfq.product_category
                        });
                    }
                }
                else
                {
                    list_rfqs.Add(new RFQViewModel
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
                        part_type_other = rfq.part_type_other,
                        mods_it = rfq.mods_it,
                        product_category = rfq.product_category
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
            RFQViewModel rFQViewModel = new RFQViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolTesters"].Split(',');
            if (newTesters.Contains(Convert.ToString(Session["userId"])))
            {
                return RedirectToAction("Index", "RFQTool");
            }
            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == userId).FirstOrDefault();

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            if (Request.QueryString["form_id"] != null)
            {

                rFQViewModel = db.RFQViewModels.Where(a => a.ID == form_id).FirstOrDefault();
                ViewBag.product_category = rFQViewModel.product_category;
                ViewBag.total_qty = rFQViewModel.total_qty;
                ViewBag.original_id = rFQViewModel.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == form_id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    /*RFQViewModelExt rFQViewModelExt = await db.RFQViewModelExts.FindAsync(item.id);
                    if (rFQViewModelExt == null)
                    {
                        return HttpNotFound();
                    }*/
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {   
                        rfqid = form_id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                        extModel = new RFQViewModelExt(),
                        //rFQViewModelExt,
                    });
                }
                if(Request.QueryString["first_time"] == "no" && get_RFQ_Ext.Count() == 0)
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {
                        rfqid = form_id.ToString(),
                        rfqidExt = 0,
                        total_quantity = "0",
                        product_categories = "",
                        extModel = new RFQViewModelExt(),
                    });
                }
                rFQViewModel.RFQExt = get_RFQ_Ext;
            }

            List<SelectListItem> list_product_cats = new List<SelectListItem>();
            list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "", Selected = true });
            list_product_cats.Add(new SelectListItem { Text = "TS8 INDUSTRIAL", Value = "ts8-ie" });
            list_product_cats.Add(new SelectListItem { Text = "VX INDUSTRIAL", Value = "vx-ie" });
            list_product_cats.Add(new SelectListItem { Text = "TS8 DATA CENTER", Value = "ts8-it" });
            list_product_cats.Add(new SelectListItem { Text = "WM/AE/AX/JB/KX", Value = "WM_AE_JB" });
            list_product_cats.Add(new SelectListItem { Text = "SPARE PARTS", Value = "spare" });
            list_product_cats.Add(new SelectListItem { Text = "OTHER", Value = "Other" });

            //Assign values to variables
            rFQViewModel.sales_engineer = userdata.usr.usr_fName + " " + userdata.usr.usr_lName;
            rFQViewModel.cell_phone = userdata.usr.usr_phone;
            rFQViewModel.email = userdata.usr.usr_email;
            rFQViewModel.submission_date = DateTime.Now;
            rFQViewModel.list_prod_cat = list_product_cats;
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
            rFQViewModel.listSAP = listSAP;
            return View(rFQViewModel);
        }

        // GET: RFQ/Create
        public ActionResult CreateRAS()
        {
            RFQRASViewModel rFQViewModel = new RFQRASViewModel();

            long userId = Convert.ToInt64(Session["userId"]);
            int form_id = Convert.ToInt32(Request.QueryString["form_id"]);

            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            rFQViewModel.listMachineTypes = machineTypes;
            rFQViewModel.listDeliveryTypes = deliveryTypes;
            rFQViewModel.listVoltage = voltageTypes;
            return View(rFQViewModel);
        }

        // POST: RFQ/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,rfq_type,sales_engineer,regional_director,account_manager,cell_phone,email,submission_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,deal_registration,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,first_time,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,plinths_type_ie,end_user,mods_it,mods_ie,mods_other,mods_WM_AE_JB,special_dimension_it,special_dimension_ie,special_dimension_other,special_dimension_WM_AE_JB,suited_enclosures_it,suited_enclosures_ie,enclosure_1_it,enclosure_2_it,enclosure_3_it,enclosure_4_it,enclosure_5_it,enclosure_1_ie,enclosure_2_ie,enclosure_3_ie,enclosure_4_ie,enclosure_5_ie,other_sap_account_num,part_type_it,part_type_ie,part_type_other,part_type_WM_AE_JB,end_user_name,end_user_location,check_project")] RFQViewModel rFQViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolTesters"].Split(',');
            if (newTesters.Contains(Convert.ToString(Session["userId"])))
            {
                return RedirectToAction("Index", "RFQTool");
            }
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
                    form_id = rFQViewModel.form_id,
                    total_qty = rFQViewModel.total_qty,
                    release_qty = rFQViewModel.release_qty,
                    target_price = rFQViewModel.target_price,
                    product_category = rFQViewModel.product_category,
                    xpress_mod_data = rFQViewModel.xpress_mod_data,
                    xpress_mod_non_data = rFQViewModel.xpress_mod_non_data,
                    enclosure_type_it = rFQViewModel.enclosure_type_it,
                    part_num_it = rFQViewModel.part_num_it,
                    size_hxwxd_it = rFQViewModel.size_hxwxd_it,
                    color_it = rFQViewModel.color_it,
                    sidewall_style_it = rFQViewModel.sidewall_style_it,
                    sidewall_location_it = rFQViewModel.sidewall_location_it,
                    castors_it = rFQViewModel.castors_it,
                    Leveling_feet_it = rFQViewModel.Leveling_feet_it,
                    front_it = rFQViewModel.front_it,
                    rear_it = rFQViewModel.rear_it,
                    cable_it = rFQViewModel.cable_it,
                    handles_it = rFQViewModel.handles_it,
                    inserts_it = rFQViewModel.inserts_it,
                    partition_wall_it = rFQViewModel.partition_wall_it,
                    baffles_it = rFQViewModel.baffles_it,
                    bsaying_brackets_it = rFQViewModel.bsaying_brackets_it,
                    additional_info_datacenter = rFQViewModel.additional_info_datacenter,
                    intell_data = rFQViewModel.intell_data,
                    voltage_data = rFQViewModel.voltage_data,
                    amp_data = rFQViewModel.amp_data,
                    outlet_it = rFQViewModel.outlet_it,
                    quantity_type_data = rFQViewModel.quantity_type_data,
                    input_cord_it = rFQViewModel.input_cord_it,
                    expansion_it = rFQViewModel.expansion_it,
                    part_num_ie = rFQViewModel.part_num_ie,
                    size_hxwxd_ie = rFQViewModel.size_hxwxd_ie,
                    material_ie = rFQViewModel.material_ie,
                    mpl_ie = rFQViewModel.mpl_ie,
                    sidewall_ie = rFQViewModel.sidewall_ie,
                    front_ie = rFQViewModel.front_ie,
                    rear_ie = rFQViewModel.rear_ie,
                    plinths_ie = rFQViewModel.plinths_ie,
                    cable_ie = rFQViewModel.cable_ie,
                    handles_ie = rFQViewModel.handles_ie,
                    inserts_ie = rFQViewModel.inserts_ie,
                    Rails = rFQViewModel.Rails,
                    Suited = rFQViewModel.suited_bay_ie,
                    suited_bay_ie = rFQViewModel.suited_bay_ie,
                    door_ie = rFQViewModel.door_ie,
                    roof_ie = rFQViewModel.roof_ie,
                    rear_wall_ie = rFQViewModel.rear_wall_ie,
                    sidewall_mod_ie = rFQViewModel.sidewall_mod_ie,
                    mpl_mod_ie = rFQViewModel.mpl_mod_ie,
                    special_paint_ie = rFQViewModel.special_paint_ie,
                    color_mod_ie = rFQViewModel.color_mod_ie,
                    ul_nema_other_ie = rFQViewModel.ul_nema_other_ie,
                    rating_ie = rFQViewModel.rating_ie,
                    part_num_WM_AE_JB = rFQViewModel.part_num_WM_AE_JB,
                    size_hxwxd_WM_AE_JB = rFQViewModel.size_hxwxd_WM_AE_JB,
                    material_WM_AE_JB = rFQViewModel.material_WM_AE_JB,
                    mpl_WM_AE_JB = rFQViewModel.mpl_WM_AE_JB,
                    latching_wm = rFQViewModel.latching_wm,
                    body_modified_wm = rFQViewModel.body_modified_wm,
                    door_modified_wm = rFQViewModel.door_modified_wm,
                    mpl_modified_wm = rFQViewModel.mpl_modified_wm,
                    special_paint_wm = rFQViewModel.special_paint_wm,
                    color_WM_AE_JB = rFQViewModel.color_WM_AE_JB,
                    ul_nema_WM_AE_JB = rFQViewModel.ul_nema_WM_AE_JB,
                    rating_WM_AE_JB = rFQViewModel.rating_WM_AE_JB,
                    part_num_other_1 = rFQViewModel.part_num_other_1,
                    size_hxwxd_other_1 = rFQViewModel.size_hxwxd_other_1,
                    producttype_other_1 = rFQViewModel.producttype_other_1,
                    material_other_1 = rFQViewModel.material_other_1,
                    body_modified_other_1 = rFQViewModel.body_modified_other_1,
                    door_modified_other_1 = rFQViewModel.door_modified_other_1,
                    mpl_modified_other_1 = rFQViewModel.mpl_modified_other_1,
                    specialpaint_other_1 = rFQViewModel.specialpaint_other_1,
                    ul_nema_other_1 = rFQViewModel.ul_nema_other_1,
                    rating_other_1 = rFQViewModel.rating_other_1,
                    additional_info_footer = rFQViewModel.additional_info_footer,
                    send = rFQViewModel.send,
                    Image_Name = rFQViewModel.Image_Name,
                    specialpaint_ie_1 = rFQViewModel.specialpaint_ie_1,
                    specialpaint_wm_1 = rFQViewModel.specialpaint_wm_1,
                    color_mod_other = rFQViewModel.color_mod_other,
                    specialpaint_other = rFQViewModel.specialpaint_other,
                    plinths_type_ie = rFQViewModel.plinths_type_ie
                };

                //Move submission date to Monday if form was submitted on weekend and move to a working day if submitted on a holiday
                if (!string.IsNullOrEmpty(Request.Form["date"]))
                {
                    rFQViewModel.submission_date = Convert.ToDateTime(Request.Form["date"]);
                }
                else
                {
                    rFQViewModel.submission_date = DateTime.Now;
                }
                DayOfWeek DayOfweek = rFQViewModel.submission_date.Value.DayOfWeek;

                if (DayOfweek.ToString() == "Sunday" && rFQViewModel.submission_date != Convert.ToDateTime("12/25/2016") || rFQViewModel.submission_date == Convert.ToDateTime("12/27/2016") || rFQViewModel.submission_date == Convert.ToDateTime("05/29/2017") || rFQViewModel.submission_date == Convert.ToDateTime("07/04/2017") || rFQViewModel.submission_date == Convert.ToDateTime("09/04/2017") || rFQViewModel.submission_date == Convert.ToDateTime("12/26/2017") || rFQViewModel.submission_date == Convert.ToDateTime("01/02/2017"))
                {
                    rFQViewModel.submission_date = rFQViewModel.submission_date.Value.AddDays(1);
                }
                else if (DayOfweek.ToString() == "Saturday" && rFQViewModel.submission_date != Convert.ToDateTime("12/24/2016") || rFQViewModel.submission_date == Convert.ToDateTime("12/26/2016") || rFQViewModel.submission_date == Convert.ToDateTime("12/25/2017"))
                {
                    rFQViewModel.submission_date = rFQViewModel.submission_date.Value.AddDays(2);
                }
                else if (rFQViewModel.submission_date == Convert.ToDateTime("04/14/2017") || rFQViewModel.submission_date == Convert.ToDateTime("11/24/2017") || rFQViewModel.submission_date == Convert.ToDateTime("12/25/2016"))
                {
                    rFQViewModel.submission_date = rFQViewModel.submission_date.Value.AddDays(3);
                }
                else if (rFQViewModel.submission_date == Convert.ToDateTime("11/23/2017") || rFQViewModel.submission_date == Convert.ToDateTime("12/29/2017") || rFQViewModel.submission_date == Convert.ToDateTime("12/24/2016"))
                {
                    rFQViewModel.submission_date = rFQViewModel.submission_date.Value.AddDays(4);
                }
                else
                {
                    rFQViewModel.submission_date = DateTime.Now;
                }
                if (rFQViewModel.sap_account_num == "-1")
                    rFQViewModel.sap_account_num = rFQViewModel.other_sap_account_num;

                if (rFQViewModel.save == null && rFQViewModel.send == null)//save multiple parts
                {
                    //Process the data
                    if (string.IsNullOrEmpty(Request.Form["first_time"]))//save first time
                    {
                        rFQViewModel.save = "Save Progress";
                        db.RFQViewModels.Add(rFQViewModel);
                        await db.SaveChangesAsync();
                        //save the images
                        await SaveImages(fileupload, rFQViewModel.ID, 0, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(rFQViewModel.ID, 0, user_id);

                        return RedirectToAction("Edit", new { id = rFQViewModel.ID, first_time = "no", n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                    else
                    {
                        dbEntity.RFQ_Data_Extend.Add(rFQDataExtend);
                        await dbEntity.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);

                        return RedirectToAction("Create", new { form_id = rFQViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request.Form["first_time"]) && rFQViewModel.save != null)
                    {
                        dbEntity.RFQ_Data_Extend.Add(rFQDataExtend);
                        await dbEntity.SaveChangesAsync();
                        int formId = Convert.ToInt32(rFQDataExtend.form_id);
                        //Select the RFQ and Update the status
                        var myRFQs = await db.RFQViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
                        myRFQs.save = "Save Progress";
                        db.Entry(myRFQs).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Add the log
                        await logAction(rFQDataExtend.form_id, rFQViewModel.save, rFQViewModel.admin_notes, userId.ToString());
                        //Don't send email when saved
                    }
                    else if (!string.IsNullOrEmpty(Request.Form["first_time"]) && rFQViewModel.send != null)
                    {
                        dbEntity.RFQ_Data_Extend.Add(rFQDataExtend);
                        await dbEntity.SaveChangesAsync();
                        int formId = Convert.ToInt32(rFQDataExtend.form_id);
                        //Select the RFQ and Update the status
                        var myRFQs = await db.RFQViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
                        myRFQs.send = "Submitted Quote Request";
                        myRFQs.save = null;
                        db.Entry(myRFQs).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id);
                        //Add the log
                        await logAction(rFQDataExtend.form_id, rFQViewModel.send, rFQViewModel.admin_notes, userId.ToString());
                        //send email when submit is clicked after additional enclosures
                    }
                    else if (rFQViewModel.save != null)
                    {
                        db.RFQViewModels.Add(rFQViewModel);
                        await db.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, rFQViewModel.ID, 0, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(rFQViewModel.ID, 0, user_id);
                        //Add the log
                        await logAction(rFQViewModel.ID.ToString(), rFQViewModel.save, rFQViewModel.admin_notes, userId.ToString());
                        //Don't send email upon first time save
                    }
                    else
                    {
                        db.RFQViewModels.Add(rFQViewModel);
                        await db.SaveChangesAsync();
                        //Add images
                        await SaveImages(fileupload, rFQViewModel.ID, 0, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(rFQViewModel.ID, 0, user_id);
                        //Add the log
                        await logAction(rFQViewModel.ID.ToString(), rFQViewModel.send, rFQViewModel.admin_notes, userId.ToString());
                        //send email upon first time submit.
                    }

                    return RedirectToAction("Index", new { form_id = rFQViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                }
            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);

                List<SelectListItem> list_product_cats = new List<SelectListItem>();
                list_product_cats.Add(new SelectListItem { Text = "Select a Product Category", Value = "", Selected = true });
                list_product_cats.Add(new SelectListItem { Text = "TS8 INDUSTRIAL", Value = "ts8-ie" });
                list_product_cats.Add(new SelectListItem { Text = "VX INDUSTRIAL", Value = "vx-ie" });
                list_product_cats.Add(new SelectListItem { Text = "TS8 DATA CENTER", Value = "ts8-it" });
                list_product_cats.Add(new SelectListItem { Text = "WM/AE/AX/JB/KX", Value = "WM_AE_JB" });
                list_product_cats.Add(new SelectListItem { Text = "SPARE PARTS", Value = "spare" });
                list_product_cats.Add(new SelectListItem { Text = "OTHER", Value = "Other" });
                //add to model
                rFQViewModel.list_prod_cat = list_product_cats;
                long userId = Convert.ToInt64(Session["userId"]);
                List<long?> accounts = await db.partnerStockCheckViewModels.Where(a => a.usr_user == userId).Select(x => x.ps_account).Distinct().ToListAsync();
                List<SelectListItem> listSAP = new List<SelectListItem>();
                listSAP.Add(new SelectListItem { Text = "Select SAP Account Number", Value = "", Selected = true });
                foreach (long? account in accounts)
                {
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
                rFQViewModel.listSAP = listSAP;

            }

            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            if (!string.IsNullOrEmpty(Request.Form["form_id"]))
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);

                var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = rFQViewModel_data.product_category;
                ViewBag.total_qty = rFQViewModel_data.total_qty;
                ViewBag.original_id = rFQViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                rFQViewModel.RFQExt = get_RFQ_Ext;
            }

            return View(rFQViewModel);
        }

        // POST: RFQ/CreateRAS
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRAS(RFQRASViewModel rFQViewModel)
        {
            var locController = new CommonController();
            if (ModelState.IsValid)
            {
                long userId = Convert.ToInt64(Session["userId"]);

                if (!Request.IsAuthenticated || userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                db.RFQRASViewModels.Add(rFQViewModel);
                await db.SaveChangesAsync();
                Dictionary<string, string> emailFields = new Dictionary<string, string>() { { "account_manager", "Rittal Account Manager" }, { "regional_manager", "Rittal Regional Manager" }, { "machine_type", "Machine Type" }, { "competitor", "Competitor" }, { "opportunity_id", "Opportunity ID" }, { "erp_id", "ERP ID" }, { "company", "Company Name" }, { "contact_name", "Contact Name" }, { "phone_number", "Phone Number" }, { "email", "Email" }, { "street_address", "Street Address" }, { "city", "City" }, { "state", "State" }, { "zipcode", "Zip Code" }, { "delivery_type", "Delivery Type" }, { "transformer_voltage", "Transformer Voltage" }, { "equipment_options", "Equipment Options" } };
                string emailBody = "<table style='width:100%;border-collapse:collapse;' border='1' cellpadding='0' cellspacing='0'>";
                foreach(var item in emailFields)
                {
                    emailBody += "<tr><th style='width:25%;padding:10px;'>" + item.Value + "</th>";
                    emailBody += "<td style='width:75%;padding:10px;'>" + (rFQViewModel[item.Key] != null ? rFQViewModel[item.Key] : "") + "</td></tr>";
                }
                emailBody += "</table><br/><br/>";
                locController.email("noreply@rittal.us", "herzog.m@rittal.us,rittal@rittal.us,robbins.j@rittal.us,kuhn.ch@rittal.us,parker.s@rittal.us", "New RFQ for RAS", "A new RFQ for RAS has been submitted.<br /><br />" + emailBody, "yes", true);
                locController.email("noreply@rittal.us", Convert.ToString(Session["userEmail"]), "New RFQ for RAS", "Your RFQ for RAS request has been submitted to Mike Herzog. If you have any questions or concerns, please reach out to Mike at Herzog.m@rittal.us.<br /><br />" + emailBody, "yes", true);
                return RedirectToAction("Index", new { success = "Your Request For Quote has been added" });
            }
            else
            {
                ViewBag.error = "Complete all fields marked *";
                rFQViewModel.listMachineTypes = machineTypes;
                rFQViewModel.listDeliveryTypes = deliveryTypes;
                rFQViewModel.listVoltage = voltageTypes;
                return View(rFQViewModel);
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);
            if (rFQViewModel == null)
            {
                return HttpNotFound();
            }
            //get the requestors id from the RFQ forms
            int requestor_id = Convert.ToInt32(rFQViewModel.user_id);
            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == requestor_id).FirstOrDefault();

            List<ProductCategories> list_product_cats = new List<ProductCategories>();
            list_product_cats.Add(new ProductCategories { cat_name = "TS8 INDUSTRIAL", value = "ts8-ie" });
            list_product_cats.Add(new ProductCategories { cat_name = "VX INDUSTRIAL", value = "vx-ie" });
            list_product_cats.Add(new ProductCategories { cat_name = "TS8 DATA CENTER", value = "ts8-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            ViewBag.list_prod_cat = list_product_cats;

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            //Collect the data from the database
            var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
            var RFQ_Ext_data = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
            var RFQ_files_data = dbEntity.RFQ_Files.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == id);
            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == id && a.ext_form_id == 0);
            ViewBag.waitStatus = waitStatus;

            //Get installed parts
            if (rfq_installed_parts.Count() > 0)
            {
                rFQViewModel.list_installed_parts = rfq_installed_parts;
            }

            //Get installed parts
            if (rfq_shipped_parts.Count() > 0)
            {
                rFQViewModel.list_shipped_parts = rfq_shipped_parts;
            }

            //Get extensions for the top portlet
            if (RFQ_Ext_data.Count() > 0)
            {
                ViewBag.product_category = rFQViewModel_data.product_category;
                ViewBag.total_qty = rFQViewModel_data.total_qty;
                ViewBag.original_id = rFQViewModel_data.ID;

                foreach (var item in RFQ_Ext_data.OrderBy(a => a.id))
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                rFQViewModel.RFQExt = get_RFQ_Ext;
            }
            else
            {
                ViewBag.original_id = id;
            }

            //Get images
            List<RFQ_File> get_RFQ_files = new List<RFQ_File>();
            if (RFQ_files_data.Count() > 0)
            {
                foreach (var item in RFQ_files_data)
                {
                    get_RFQ_files.Add(new RFQ_File
                    {
                        file_id = item.file_id,
                        form_id = item.form_id,
                        file_name = item.file_name,
                        file_type = item.file_type
                    });
                }
                rFQViewModel.list_RFQ_files = get_RFQ_files;
            }
            //Get logs
            List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
            if (rfq_action_log.Count() > 0)
            {
                foreach (var item in rfq_action_log)
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

                    get_RFQ_logs.Add(new RFQ_Action_LogViewModel
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
                rFQViewModel.list_RFQ_logs = get_RFQ_logs;
                //Check if it is ever been cloned
                var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
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
                rFQViewModel.IsCloned = isCloned;
            }
            if (userdata!=null)
            {
                ViewBag.comp_type = userdata.comp.comp_type;
            }
            List<long?> accounts = await db.partnerStockCheckViewModels.Where(a => a.usr_user == userId).Select(x => x.ps_account).Distinct().ToListAsync();
            List<SelectListItem> listSAP = new List<SelectListItem>();
            listSAP.Add(new SelectListItem { Text = "Select SAP Account Number", Value = "", Selected = true });
            bool bOther = true;
            foreach (long? account in accounts)
            {
                var locdata = db.partnerLocationViewModels.Join(
                    db.partnerCompanyViewModels,
                    comp => comp.comp_ID,
                    loc => loc.comp_ID,
                    (loc, comp) => new { loc, comp }
                    ).Where(a => a.loc.loc_SAP_account == account);

                if (locdata.Count() > 0)
                {
                    string accountNum = account + "";
                    if (rFQViewModel.sap_account_num == accountNum)
                        bOther = false;
                    listSAP.Add(new SelectListItem { Text = account + " - " + locdata.FirstOrDefault().comp.comp_name, Value = accountNum });
                }
            }
            listSAP.Add(new SelectListItem { Text = "Other", Value = "-1" });
            if (bOther)
            {
                string accountNumber = rFQViewModel.sap_account_num;
                rFQViewModel.other_sap_account_num = accountNumber;
                rFQViewModel.sap_account_num = "-1";
            }
            rFQViewModel.listSAP = listSAP;
            return View(rFQViewModel);
        }

        // POST: RFQ/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,sales_engineer,regional_director,account_manager,cell_phone,email,submission_date,completion_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,deal_registration,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,admin_notes,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,Quote_Num,admin_status,plinths_type_ie,end_user,mods_it,mods_ie,mods_other,mods_WM_AE_JB,special_dimension_it,special_dimension_ie,special_dimension_other,special_dimension_WM_AE_JB,suited_enclosures_it,suited_enclosures_ie,enclosure_1_it,enclosure_2_it,enclosure_3_it,enclosure_4_it,enclosure_5_it,enclosure_1_ie,enclosure_2_ie,enclosure_3_ie,enclosure_4_ie,enclosure_5_ie,other_sap_account_num,part_type_it,part_type_ie,part_type_other,part_type_WM_AE_JB,end_user_name,end_user_location,check_project")] RFQViewModel rFQViewModel, IEnumerable<HttpPostedFileBase> fileupload)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            int user_id = Convert.ToInt32(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            string[] newTesters = ConfigurationManager.AppSettings["RFQToolTesters"].Split(',');
            if (newTesters.Contains(Convert.ToString(Session["userId"])))
            {
                return RedirectToAction("Index", "RFQTool");
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
                    var formId = Convert.ToInt32(rFQViewModel.form_id);
                    RFQViewModel rfqTemp = await db.RFQViewModels.AsNoTracking().Where(x => x.ID == formId).FirstAsync();

                    if (rfqTemp == null || rfqTemp.completion_date != null)
                    {
                        return RedirectToAction("Index", new { form_id = rFQViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }

                }

                string header = locController.emailheader(host);
                string footer = locController.emailfooter(host);
                MailMessage message_requester;
                List<string> files = new List<string>();

                RFQ_Data_Extend rFQDataExtend = new RFQ_Data_Extend
                {
                    form_id = rFQViewModel.form_id,
                    total_qty = rFQViewModel.total_qty,
                    release_qty = rFQViewModel.release_qty,
                    target_price = rFQViewModel.target_price,
                    product_category = rFQViewModel.product_category,
                    xpress_mod_data = rFQViewModel.xpress_mod_data,
                    xpress_mod_non_data = rFQViewModel.xpress_mod_non_data,
                    enclosure_type_it = rFQViewModel.enclosure_type_it,
                    part_num_it = rFQViewModel.part_num_it,
                    size_hxwxd_it = rFQViewModel.size_hxwxd_it,
                    color_it = rFQViewModel.color_it,
                    sidewall_style_it = rFQViewModel.sidewall_style_it,
                    sidewall_location_it = rFQViewModel.sidewall_location_it,
                    castors_it = rFQViewModel.castors_it,
                    Leveling_feet_it = rFQViewModel.Leveling_feet_it,
                    front_it = rFQViewModel.front_it,
                    rear_it = rFQViewModel.rear_it,
                    cable_it = rFQViewModel.cable_it,
                    handles_it = rFQViewModel.handles_it,
                    inserts_it = rFQViewModel.inserts_it,
                    partition_wall_it = rFQViewModel.partition_wall_it,
                    baffles_it = rFQViewModel.baffles_it,
                    bsaying_brackets_it = rFQViewModel.bsaying_brackets_it,
                    additional_info_datacenter = rFQViewModel.additional_info_datacenter,
                    intell_data = rFQViewModel.intell_data,
                    voltage_data = rFQViewModel.voltage_data,
                    amp_data = rFQViewModel.amp_data,
                    outlet_it = rFQViewModel.outlet_it,
                    quantity_type_data = rFQViewModel.quantity_type_data,
                    input_cord_it = rFQViewModel.input_cord_it,
                    expansion_it = rFQViewModel.expansion_it,
                    part_num_ie = rFQViewModel.part_num_ie,
                    size_hxwxd_ie = rFQViewModel.size_hxwxd_ie,
                    material_ie = rFQViewModel.material_ie,
                    mpl_ie = rFQViewModel.mpl_ie,
                    sidewall_ie = rFQViewModel.sidewall_ie,
                    front_ie = rFQViewModel.front_ie,
                    rear_ie = rFQViewModel.rear_ie,
                    plinths_ie = rFQViewModel.plinths_ie,
                    cable_ie = rFQViewModel.cable_ie,
                    handles_ie = rFQViewModel.handles_ie,
                    inserts_ie = rFQViewModel.inserts_ie,
                    Rails = rFQViewModel.Rails,
                    Suited = rFQViewModel.suited_bay_ie,
                    suited_bay_ie = rFQViewModel.suited_bay_ie,
                    door_ie = rFQViewModel.door_ie,
                    roof_ie = rFQViewModel.roof_ie,
                    rear_wall_ie = rFQViewModel.rear_wall_ie,
                    sidewall_mod_ie = rFQViewModel.sidewall_mod_ie,
                    mpl_mod_ie = rFQViewModel.mpl_mod_ie,
                    special_paint_ie = rFQViewModel.special_paint_ie,
                    color_mod_ie = rFQViewModel.color_mod_ie,
                    ul_nema_other_ie = rFQViewModel.ul_nema_other_ie,
                    rating_ie = rFQViewModel.rating_ie,
                    part_num_WM_AE_JB = rFQViewModel.part_num_WM_AE_JB,
                    size_hxwxd_WM_AE_JB = rFQViewModel.size_hxwxd_WM_AE_JB,
                    material_WM_AE_JB = rFQViewModel.material_WM_AE_JB,
                    mpl_WM_AE_JB = rFQViewModel.mpl_WM_AE_JB,
                    latching_wm = rFQViewModel.latching_wm,
                    body_modified_wm = rFQViewModel.body_modified_wm,
                    door_modified_wm = rFQViewModel.door_modified_wm,
                    mpl_modified_wm = rFQViewModel.mpl_modified_wm,
                    special_paint_wm = rFQViewModel.special_paint_wm,
                    color_WM_AE_JB = rFQViewModel.color_WM_AE_JB,
                    ul_nema_WM_AE_JB = rFQViewModel.ul_nema_WM_AE_JB,
                    rating_WM_AE_JB = rFQViewModel.rating_WM_AE_JB,
                    part_num_other_1 = rFQViewModel.part_num_other_1,
                    size_hxwxd_other_1 = rFQViewModel.size_hxwxd_other_1,
                    producttype_other_1 = rFQViewModel.producttype_other_1,
                    material_other_1 = rFQViewModel.material_other_1,
                    body_modified_other_1 = rFQViewModel.body_modified_other_1,
                    door_modified_other_1 = rFQViewModel.door_modified_other_1,
                    mpl_modified_other_1 = rFQViewModel.mpl_modified_other_1,
                    specialpaint_other_1 = rFQViewModel.specialpaint_other_1,
                    ul_nema_other_1 = rFQViewModel.ul_nema_other_1,
                    rating_other_1 = rFQViewModel.rating_other_1,
                    additional_info_footer = rFQViewModel.additional_info_footer,
                    send = rFQViewModel.send,
                    Image_Name = rFQViewModel.Image_Name,
                    specialpaint_ie_1 = rFQViewModel.specialpaint_ie_1,
                    specialpaint_wm_1 = rFQViewModel.specialpaint_wm_1,
                    color_mod_other = rFQViewModel.color_mod_other,
                    specialpaint_other = rFQViewModel.specialpaint_other,
                    plinths_type_ie = rFQViewModel.plinths_type_ie
                };
                if (rFQViewModel.sap_account_num == "-1")
                    rFQViewModel.sap_account_num = rFQViewModel.other_sap_account_num;

                //Handle all actions
                if (!string.IsNullOrEmpty(Request.Form["saveadmin"]) || !string.IsNullOrEmpty(Request.Form["complete"]) || !string.IsNullOrEmpty(Request.Form["return"]))
                {
                    //Get user information
                    int requestor_id = Convert.ToInt32(rFQViewModel.user_id);
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
                        if(rFQViewModel.send != "Completed")
                        {
                            rFQViewModel.send = "Admin Saved with Notes";
                        }
                        actionStr = action;
                        bAssign = false;
                    }
                    else if (!string.IsNullOrEmpty(Request.Form["complete"]))
                    {
                        rFQViewModel.send = "Completed";
                        rFQViewModel.completion_date = DateTime.Now;
                        
                        //Send the return email
                        string body_requester = "Your request for quote has been completed.<br /><br />" +
                                                "Request ID: " + rFQViewModel.form_id + "<br /><br />" +
                                                "Thank you for offering Rittal the opportunity to provide a solution for " + rFQViewModel.sold_to_party + " " +
                                                "Your quotation has been completed on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) + " and assigned Quote Number " + rFQViewModel.Quote_Num + "<br />" +
                                                "The quotation will be emailed to you shortly.Please contact me if you have any questions and thank you for the opportunity to provide a proposal." +
                                                "If you are ready to proceed with an order, purchase orders can be sent directly to <a href=\"mailto: orders@rittal.us? Subject = RFQ purchase orders request\" >orders@rittal.us</a>.Thank you for your business!" +
                                                "<br /><br />-Rittal Quotation Team <br /><br />" + salutaion;
                        //Send email to the Return Requester
                        if(getuserdata != null)
                        {
                            message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been completed #" + rFQViewModel.form_id + "", header + body_requester + footer);
                            locController.sendEmailSingle(message_requester, files);
                        }
                        actionStr = "Completed";
                    }
                    else
                    {
                        var getAssigned = db.RFQ_Action_LogViewModels.Where(a=>a.Form_ID==rFQViewModel.ID && a.Action=="Assigned");
                        if (getAssigned.Count() > 0)
                        {
                            actionStr = "Assigned-Returned";
                            rFQViewModel.send = "Returned";
                            //rFQViewModel.admin_status = null;
                        }
                        else
                        {
                            actionStr = "Returned";
                            rFQViewModel.send = "Returned";
                            rFQViewModel.admin_status = null;
                        }

                        //Send email to the Return Requester
                        if(getuserdata != null)
                        {
                            //Send the return email
                            string body_requester = "Your request for quote has been returned.<br /><br />" +
                                                "Quote Team Notes: <b><u>" + rFQViewModel.admin_notes + "</u></b><br /><br />" +
                                                 "Request ID: " + rFQViewModel.form_id + "<br /><br />" +
                                                "Thank you for offering Rittal the opportunity to provide a solution for " + rFQViewModel.sold_to_party + ". Your quotation was RETURNED on " + string.Format("{0:MM/dd/yyyy}", DateTime.Now) +
                                                 ".<br /><br /> More information has been provided below.<br /><br />" + salutaion;
                            message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been returned #" + rFQViewModel.form_id + "", header + body_requester + footer);
                            locController.sendEmailSingle(message_requester, files);
                        }
                    }

                    db.Entry(rFQViewModel).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    await logAction(rFQViewModel.form_id, actionStr, rFQViewModel.admin_notes, userId.ToString(), user_id, bAssign);
                    return RedirectToAction("RfqAdmin", new { form_id = rFQViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = (!string.IsNullOrEmpty(Request.Form["saveadmin"]) ? "Notes have been added to the RFQ" : "The status on the RFQ has been updated"), admin = "yes" });
                }
                else
                {
                    //Handle none RFQ admin actions
                    if (rFQViewModel.save == null && rFQViewModel.send == null)//save multiple parts
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
                        await SaveImages(fileupload, rFQViewModel.ID, rFQDataExtend.id, user_id);
                        //Save Installed and Shipped with parts
                        await saveInstalledShipped(Convert.ToInt32(rFQDataExtend.form_id), rFQDataExtend.id, user_id, "edit");

                        return RedirectToAction("EditExt", new { id= rFQDataExtend.id, form_id = rFQViewModel.form_id, first_time = "no", n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                    else
                    {
                        if (rFQViewModel.save != null)
                        {
                            db.Entry(rFQViewModel).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            await SaveImages(fileupload, rFQViewModel.ID, 0, user_id);
                            //Save Installed and Shipped with parts
                            await saveInstalledShipped(rFQViewModel.ID, 0, user_id, "edit");
                            //Add the log
                            await logAction(rFQViewModel.ID.ToString(), rFQViewModel.save, rFQViewModel.admin_notes, userId.ToString());
                            //Don't send email upon first time save
                        }
                        else
                        {
                            rFQViewModel.submission_date = DateTime.Now;
                            db.Entry(rFQViewModel).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            await SaveImages(fileupload, rFQViewModel.ID, 0, user_id);
                            //Save Installed and Shipped with parts
                            await saveInstalledShipped(rFQViewModel.ID, 0, user_id, "edit");
                            //Add the log
                            await logAction(rFQViewModel.ID.ToString(), rFQViewModel.send, rFQViewModel.admin_notes, userId.ToString());
                            //send email upon first time submit.
                        }

                        return RedirectToAction("Index", new { form_id = rFQViewModel.form_id, first_time = Request.Form["first_time"], n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], msg = Request.Form["msg"], success = "Your Request For Quote has been added" });
                    }
                }

            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);

                List<ProductCategories> list_product_cats = new List<ProductCategories>();
                list_product_cats.Add(new ProductCategories { cat_name = "TS8 INDUSTRIAL", value = "ts8-ie" });
                list_product_cats.Add(new ProductCategories { cat_name = "VX INDUSTRIAL", value = "vx-ie" });
                list_product_cats.Add(new ProductCategories { cat_name = "TS8 DATA CENTER", value = "ts8-it" });
                list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
                list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
                list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
                ViewBag.list_prod_cat = list_product_cats;
            }
            
            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            if (Request.Form["form_id"] != null)
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);

                var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = rFQViewModel_data.product_category;
                ViewBag.total_qty = rFQViewModel_data.total_qty;
                ViewBag.original_id = rFQViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {
                        rfqid = id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                rFQViewModel.RFQExt = get_RFQ_Ext;
                List<long?> accounts = await db.partnerStockCheckViewModels.Where(a => a.usr_user == userId).Select(x => x.ps_account).Distinct().ToListAsync();
                List<SelectListItem> listSAP = new List<SelectListItem>();
                listSAP.Add(new SelectListItem { Text = "Select SAP Account Number", Value = "", Selected = true });
                bool bOther = true;
                foreach (long? account in accounts)
                {
                    var locdata = db.partnerLocationViewModels.Join(
                        db.partnerCompanyViewModels,
                        comp => comp.comp_ID,
                        loc => loc.comp_ID,
                        (loc, comp) => new { loc, comp }
                        ).Where(a => a.loc.loc_SAP_account == account);

                    if (locdata.Count() > 0)
                    {
                        string accountNum = account + "";
                        if (rFQViewModel.sap_account_num == accountNum)
                            bOther = false;
                        listSAP.Add(new SelectListItem { Text = account + " - " + locdata.FirstOrDefault().comp.comp_name, Value = accountNum });
                    }
                }
                listSAP.Add(new SelectListItem { Text = "Other", Value = "-1" });
                if (bOther)
                {
                    string accountNumber = rFQViewModel.sap_account_num;
                    rFQViewModel.other_sap_account_num = accountNumber;
                    rFQViewModel.sap_account_num = "-1";
                }
                rFQViewModel.listSAP = listSAP;
            }

            return View(rFQViewModel);
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

            RFQViewModelExt rFQViewModelExt = await db.RFQViewModelExts.FindAsync(id);
            if (rFQViewModelExt == null)
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

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            if (Request.QueryString["form_id"] != null)
            {
                RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(form_id);
                if (rFQViewModel == null)
                {
                    return HttpNotFound();
                }
                rFQViewModelExt.mainModel = rFQViewModel;
                var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == form_id).FirstOrDefault();
                ViewBag.product_category = rFQViewModel_data.product_category;
                ViewBag.total_qty = rFQViewModel_data.total_qty;
                ViewBag.original_id = rFQViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == form_id.ToString());
                foreach (var item in RFQ_Ext.OrderBy(a => a.id))
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
                    {
                        rfqid = form_id.ToString(),
                        rfqidExt = item.id,
                        total_quantity = item.total_qty,
                        product_categories = item.product_category,
                    });
                }
                rFQViewModelExt.RFQExt = get_RFQ_Ext;
            }

            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.ext_form_id == id);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.ext_form_id == id);

            //Get installed parts
            if (rfq_installed_parts.Count() > 0)
            {
                rFQViewModelExt.list_installed_parts = rfq_installed_parts;
            }

            //Get installed parts
            if (rfq_shipped_parts.Count() > 0)
            {
                rFQViewModelExt.list_shipped_parts = rfq_shipped_parts;
            }

            //Get images
            var RFQ_files_data = dbEntity.RFQ_Files.Where(a => a.ext_form_id == id);
            List<RFQ_File> get_RFQ_files = new List<RFQ_File>();
            if (RFQ_files_data.Count() > 0)
            {
                foreach (var item in RFQ_files_data)
                {
                    get_RFQ_files.Add(new RFQ_File
                    {
                        file_id = item.file_id,
                        ext_form_id = item.ext_form_id,
                        file_name = item.file_name,
                        file_type = item.file_type
                    });
                }
                rFQViewModelExt.list_RFQ_files = get_RFQ_files;
            }

            //Get logs
            int formid = Convert.ToInt32(rFQViewModelExt.form_id);
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID ==formid);
            List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
            if (rfq_action_log.Count() > 0)
            {
                foreach (var item in rfq_action_log)
                {
                    //Get user's full name
                    int usrId = Convert.ToInt32(item.Usr_ID);
                    var getfullName = await locController.GetfullName(usrId);
                    //Get Admin Name 
                    int adminId = Convert.ToInt32(item.Admin_ID);
                    var getAdminfullName = await locController.GetfullName(adminId);

                    get_RFQ_logs.Add(new RFQ_Action_LogViewModel
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
                var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
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
                rFQViewModelExt.IsCloned = isCloned;
            }

            List<ProductCategories> list_product_cats = new List<ProductCategories>();
            list_product_cats.Add(new ProductCategories { cat_name = "TS8 INDUSTRIAL", value = "ts8-ie" });
            list_product_cats.Add(new ProductCategories { cat_name = "VX INDUSTRIAL", value = "vx-ie" });
            list_product_cats.Add(new ProductCategories { cat_name = "TS8 DATA CENTER", value = "ts8-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            ViewBag.list_prod_cat = list_product_cats;

            return View(rFQViewModelExt);
        }

        // POST: RFQ/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditExt([Bind(Include = "ID,sales_engineer,regional_director,cell_phone,email,submission_date,updated_quote,sold_to_party,qte_num,sap_account_num,location,end_contact,opportunity_num,qte_ref,qte_description,draw_num,total_qty,release_qty,competition,target_price,scale_volume,spa_contract_num,spa_mult,drawing_approval,product_category,xpress_mod_data,xpress_mod_non_data,enclosure_type_it,part_num_it,size_hxwxd_it,color_it,sidewall_style_it,sidewall_location_it,castors_it,Leveling_feet_it,front_it,rear_it,cable_it,handles_it,inserts_it,partition_wall_it,baffles_it,bsaying_brackets_it,additional_info_datacenter,intell_data,voltage_data,amp_data,outlet_it,quantity_type_data,input_cord_it,expansion_it,part_num_ie,size_hxwxd_ie,material_ie,mpl_ie,sidewall_ie,front_ie,rear_ie,plinths_ie,cable_ie,handles_ie,inserts_ie,Rails,Suited,suited_bay_ie,door_ie,roof_ie,rear_wall_ie,sidewall_mod_ie,mpl_mod_ie,special_paint_ie,color_mod_ie,ul_nema_other_ie,rating_ie,part_num_WM_AE_JB,size_hxwxd_WM_AE_JB,material_WM_AE_JB,mpl_WM_AE_JB,latching_wm,body_modified_wm,door_modified_wm,mpl_modified_wm,special_paint_wm,color_WM_AE_JB,ul_nema_WM_AE_JB,rating_WM_AE_JB,part_num_other_1,size_hxwxd_other_1,producttype_other_1,material_other_1,body_modified_other_1,door_modified_other_1,mpl_modified_other_1,specialpaint_other_1,ul_nema_other_1,rating_other_1,additional_info_footer,send,fileupload,user_id,save,distro_name,distro_company,form_id,admin_notes,,color_mod_other,specialpaint_other,specialpaint_wm_1,specialpaint_wm_1,specialpaint_ie_1,qty_installed,part_number_installed,description_installed,qty_shipped,part_number_shipped,description_shipped,plinths_type_ie,end_user")] RFQViewModelExt rFQDataExtend, IEnumerable<HttpPostedFileBase> fileupload)
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
                RFQViewModel rfqTemp = await db.RFQViewModels.AsNoTracking().Where(x => x.ID == form_id).FirstAsync();
                
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
                    
                    db.RFQViewModelExts.Add(rFQDataExtend);
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
                        var myRFQs = await db.RFQViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
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
                        var myRFQs = await db.RFQViewModels.Where(a => a.ID == formId).FirstOrDefaultAsync();
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
                list_product_cats.Add(new ProductCategories { cat_name = "TS8 INDUSTRIAL", value = "ts8-ie" });
                list_product_cats.Add(new ProductCategories { cat_name = "VX INDUSTRIAL", value = "vx-ie" });
                list_product_cats.Add(new ProductCategories { cat_name = "TS8 DATA CENTER", value = "ts8-it" });
                list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
                list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
                list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
                ViewBag.list_prod_cat = list_product_cats;
            }

            ViewBag.n1_name = Request.Form["n1_name"];
            ViewBag.n2_name = Request.Form["n2_name"];
            ViewBag.form_id = Request.Form["form_id"];
            ViewBag.msg = Request.Form["msg"];
            ViewBag.first_time = Request.Form["first_time"];
            ViewBag.error = "Complete all fields marked *";

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            if (Request.Form["form_id"] != null)
            {
                int id = Convert.ToInt32(Request.Form["form_id"]);
                var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
                ViewBag.product_category = rFQViewModel_data.product_category;
                ViewBag.total_qty = rFQViewModel_data.total_qty;
                ViewBag.original_id = rFQViewModel_data.ID;

                var RFQ_Ext = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
                foreach (var item in RFQ_Ext)
                {
                    get_RFQ_Ext.Add(new RFQViewModelExtPart
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
                RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);
                rFQViewModel.send = "Cloned/Submitted";
                rFQViewModel.qte_num = rFQViewModel.Quote_Num;
                rFQViewModel.Quote_Num = null;
                rFQViewModel.updated_quote = "";
                rFQViewModel.submission_date = DateTime.Now;
                rFQViewModel.completion_date = null;
                rFQViewModel.admin_status = null;
                db.RFQViewModels.Add(rFQViewModel);
                await db.SaveChangesAsync();

                //Add the log
                await logAction(rFQViewModel.ID.ToString(), "Cloned/Submitted", "The RFQ id = " + rFQViewModel.form_id + " was cloned", userId.ToString());
                status.Add("status", "OK");
                status.Add("id", rFQViewModel.ID.ToString());
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
                RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);
                rFQViewModel.send = "Cloned with Changes/Not Submitted";
                rFQViewModel.qte_num = rFQViewModel.Quote_Num;
                rFQViewModel.Quote_Num = null;
                rFQViewModel.updated_quote = "";
                rFQViewModel.submission_date = DateTime.Now;
                rFQViewModel.completion_date = null;
                rFQViewModel.admin_status = null;
                db.RFQViewModels.Add(rFQViewModel);
                await db.SaveChangesAsync();
                string rfqid = id.ToString();
                //Save RFQ Extended
                List<CloneParts> statusExtIds = new List<CloneParts>();
                IQueryable<RFQViewModelExt> rfqViewModelExts = db.RFQViewModelExts.Where(a => a.form_id == rfqid);
                foreach (var Exts_item in rfqViewModelExts)
                {
                    Exts_item.form_id = rFQViewModel.ID.ToString();
                    db.RFQViewModelExts.Add(Exts_item);
                }
                await db.SaveChangesAsync();
                //Add the log
                await logAction(rFQViewModel.ID.ToString(), "Cloned with Changes/Not Submitted", "The RFQ id = " + rFQViewModel.form_id + " was cloned", userId.ToString());
                status.Add("status", "OK");
                status.Add("id", rFQViewModel.ID.ToString() + "," + id);
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

                var RFQViewModelExtsOld = await db.RFQViewModelExts.Where(a => a.form_id == old_id).ToListAsync();
                var RFQViewModelExtsNew = await db.RFQViewModelExts.Where(a => a.form_id == new_id).ToListAsync();
                List<CloneParts> CloneParts = new List<CloneParts>();
                foreach (var itemExt in RFQViewModelExtsNew.OrderByDescending(a => a.id))
                {
                    CloneParts.Add(new CloneParts { new_id = itemExt.id });
                }
                int x = 0;
                bool addedInstalled = false;
                //Save RFQ Installed parts
                IQueryable<RFQ_Parts_InstalledViewModel> rFQInstalled = db.RFQ_Parts_InstalledViewModels.Where(a => a.form_id == oldid);
                foreach (var itemExt in RFQViewModelExtsOld.OrderByDescending(a => a.id))
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
                foreach (var itemExt in RFQViewModelExtsOld.OrderByDescending(a => a.id))
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
                IQueryable<RFQ_File> rFQFiles = db.RFQFiles.Where(a => a.form_id == oldid);
                foreach (var itemExt in RFQViewModelExtsOld.OrderByDescending(a => a.id))
                {
                    foreach (var item in rFQFiles)
                    {
                        if (itemExt.id == item.ext_form_id)
                        {
                            item.form_id = newid;
                            item.ext_form_id = CloneParts[z].new_id;
                            db.RFQFiles.Add(item);
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
                        db.RFQFiles.Add(item);
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
                var rFQViewModel = await db.RFQViewModels.Where(a => a.ID == rfq_id).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(rFQViewModel.admin_status) || assign=="yes")
                {
                    //Get user information
                    int requester_id = Convert.ToInt32(rFQViewModel.user_id);
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
                                             "Thank you for offering Rittal the opportunity to provide a solution for  " + rFQViewModel.sold_to_party +
                                             ". My name is " + getAdmindata.usr_fName + " " + getAdmindata.usr_lName + " and I will be handling your request. I am based in our Ohio facility and can be reached via the information below should you need to communicate any additional information or changes. As soon as I complete your request you will receive an immediate email to let you know your quotation is ready. The majority of our requests are completed within one business day.<br /><br />" + salutaion;

                    //Send email to the Return Requester
                    message_requester = new MailMessage("NoReply@rittal.us", getuserdata.usr_email, "RiSourceCenter -your RFQ has been assigned #" + rFQViewModel.ID + "", header + body_requester + footer);
                    locController.sendEmailSingle(message_requester, files);

                    rFQViewModel.admin_status = "Assigned";
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
                    rFQ_Files = new RFQ_File
                    {
                        form_id = id,
                        ext_form_id = extid,
                        user_id = user_id,
                        file_name = NewFileName,
                        file_type = Path.GetExtension(file.FileName)
                    };
                    db.RFQFiles.Add(rFQ_Files);
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
            RFQ_Action_Log rFQActionLog = new RFQ_Action_Log();
            int ids = Convert.ToInt32(form_id);
            var rfq_action_log = db.RFQ_Action_LogViewModels.Where(a => a.Form_ID == ids);
            var lastObj = rfq_action_log.OrderByDescending(a => a.ID).FirstOrDefault();
            if (bAssign)
            {
                if (rfq_action_log.Count() > 0 && usr_id == 0)
                {
                    rFQActionLog = new RFQ_Action_Log
                    {
                        Form_ID = Convert.ToInt32(form_id),
                        Action = action,
                        Action_Time = DateTime.Now,
                        Notes = notes,
                        Usr_ID = user_id,
                        Admin_ID = lastObj.Admin_ID
                    };
                }
                else if (rfq_action_log.Count() > 0 && usr_id != 0)
                {
                    rFQActionLog = new RFQ_Action_Log
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
            if(!bAssign || rfq_action_log.Count() == 0)
            {
                rFQActionLog = new RFQ_Action_Log
                {
                    Form_ID = Convert.ToInt32(form_id),
                    Action = (action != "" ? action : (lastObj != null ? lastObj.Action : "")), 
                    Action_Time = DateTime.Now,
                    Notes = notes,
                    Usr_ID = user_id
                };
                if (!bAssign && rfq_action_log.Count() > 0 && !string.IsNullOrEmpty(lastObj.Admin_ID))
                    rFQActionLog.Admin_ID = lastObj.Admin_ID;
            }
            dbEntity.RFQ_Action_Log.Add(rFQActionLog);
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
            var rFQFileData = await db.RFQFiles.FindAsync(id);
            if (rFQFileData == null)
            {
                return HttpNotFound();
            }
            db.RFQFiles.Remove(rFQFileData);
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
            var rFQFileData = db.RFQFiles.Where(a => a.ext_form_id == id);
            if (rFQFileData != null)
            {
                db.RFQFiles.RemoveRange(rFQFileData);
            }

            RFQViewModelExt rFQDataExtend = await db.RFQViewModelExts.FindAsync(id);
            if (rFQDataExtend == null)
            {
                return HttpNotFound();
            }
            db.RFQViewModelExts.Remove(rFQDataExtend);
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
            
            RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);

            if (rFQViewModel == null || rFQViewModel.completion_date != null)
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

            /*RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);
            if (rFQViewModel == null)
            {
                return HttpNotFound();
            }*/
            var rFQDataExtend = db.RFQViewModelExts.Where(a => a.form_id == id.ToString());
            if (rFQDataExtend != null)
            {
                db.RFQViewModelExts.RemoveRange(rFQDataExtend);
            }
            var rFQFileData = db.RFQFiles.Where(a => a.form_id == id);
            if (rFQFileData != null)
            {
                db.RFQFiles.RemoveRange(rFQFileData);
            }

            db.RFQViewModels.Remove(rFQViewModel);

            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "The RFQ has been deleted" });
        }

        // POST: RFQ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RFQViewModel rFQViewModel = await db.RFQViewModels.FindAsync(id);
            db.RFQViewModels.Remove(rFQViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

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

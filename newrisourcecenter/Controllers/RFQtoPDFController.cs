using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using newrisourcecenter.Models;
using System.Globalization;

namespace newrisourcecenter.Controllers
{
    public partial class RFQToolController : Controller
    {
        #region View PDF
        public async Task<ActionResult> ViewPDF(int id = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
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
            list_product_cats.Add(new ProductCategories { cat_name = "VX DATA CENTER", value = "vx-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            ViewBag.list_prod_cat = list_product_cats;
            var obj = list_product_cats.Where(x => x.value == rFQViewModel.product_category).FirstOrDefault();
            ViewBag.productCategoryName = obj != null ? obj.cat_name : "";

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            //Collect the data from the database
            var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
            var RFQ_Ext_data = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
            var RFQ_files_data = dbEntity.RFQ_Files.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == id);
            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == id && a.ext_form_id == 0);

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

                foreach (var item in RFQ_Ext_data.OrderByDescending(a => a.id))
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
            ViewBag.comp_type = userdata.comp.comp_type;

            return View(rFQViewModel);
        }
        #endregion

        #region Get PDF
        public async Task<ActionResult> GetPDF(int id = 0, int ext_formid = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
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
            list_product_cats.Add(new ProductCategories { cat_name = "VX DATA CENTER", value = "vx-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            ViewBag.list_prod_cat = list_product_cats;
            var obj = list_product_cats.Where(x => x.value == rFQViewModel.product_category).FirstOrDefault();
            ViewBag.productCategoryName = obj != null ? obj.cat_name : "";

            List<RFQViewModelExtPart> get_RFQ_Ext = new List<RFQViewModelExtPart>();
            //Collect the data from the database
            var rFQViewModel_data = db.RFQViewModels.Where(a => a.ID == id).FirstOrDefault();
            var RFQ_Ext_data = dbEntity.RFQ_Data_Extend.Where(a => a.form_id == id.ToString());
            var RFQ_files_data = dbEntity.RFQ_Files.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == id);
            var rfq_installed_parts = dbEntity.RFQ_Parts_Installed.Where(a => a.form_id == id && a.ext_form_id == 0);
            var rfq_shipped_parts = dbEntity.RFQ_Parts_Shipped.Where(a => a.form_id == id && a.ext_form_id == 0);

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
            ViewBag.comp_type = userdata.comp.comp_type;

            //call the function that returns the view to string for the email body
            String messageView = RenderViewToString(ControllerContext, "~/Views/RFQTool/ViewPDF.cshtml", rFQViewModel, true);

            string Body = messageView;

            ViewBag.Message = MvcHtmlString.Create(Body);
            MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(Body);
            // step 1: creation of a document-object
            Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
            doc.AddTitle("Return Request Tool");
            doc.AddCreator("RiSourceCenter");
            doc.AddAuthor("Return Request Tool");
            doc.AddHeader("Rittal USA", "The RiSourceCenter");
            // step 2:
            // we create a writer that listens to the document
            // and directs a XML-stream to a file
            PdfWriter writer = PdfWriter.GetInstance(doc, msOutput);
            // step 3: we open document and start the worker on the document
            doc.Open();
            //step 4: user the XMLWorkerHelper to read the file and add it to the doc
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, reader);
            //step 5: close all processes
            doc.Close();
            writer.CloseStream = false;
            reader.Close();
            writer.Close();
            //step 7:parse the output into a byte array to be read by the
            byte[] content = msOutput.ToArray();
            //return the content
            // Write out PDF from memory stream.
            string path = Server.MapPath("~/attachments/rfq_tool/PDFs/pdf_" + rFQViewModel.ID + ".pdf");
            System.IO.File.Delete(path);
            System.IO.File.WriteAllBytes(path, content);//create the PDF file

            return Redirect("~/attachments/rfq_tool/PDFs/pdf_" + rFQViewModel.ID + ".pdf");
        }
        #endregion

        #region View PDF Extention
        public async Task<ActionResult> ViewPDFExt(int id = 0)
        {
            ViewBag.extId = id;
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
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
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == formid);
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
            list_product_cats.Add(new ProductCategories { cat_name = "VX DATA CENTER", value = "vx-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            ViewBag.list_prod_cat = list_product_cats;

            return View(rFQViewModelExt);
        }
        #endregion

        #region Print PDF Extention
        public async Task<ActionResult> GetPDFExt(int id = 0)
        {
            ViewBag.extId = id;
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
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
            var rfq_action_log = dbEntity.RFQ_Action_Log.Where(a => a.Form_ID == formid);
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
            list_product_cats.Add(new ProductCategories { cat_name = "VX DATA CENTER", value = "vx-it" });
            list_product_cats.Add(new ProductCategories { cat_name = "WM/AE/AX/JB/KX", value = "WM_AE_JB" });
            list_product_cats.Add(new ProductCategories { cat_name = "OTHER", value = "Other" });
            list_product_cats.Add(new ProductCategories { cat_name = "SPARE PARTS", value = "spare" });
            ViewBag.list_prod_cat = list_product_cats;

            //call the function that returns the view to string for the email body
            String messageView = RenderViewToString(ControllerContext, "~/Views/RFQTool/ViewPDFExt.cshtml", rFQViewModelExt, true);

            string Body = messageView;

            ViewBag.Message = MvcHtmlString.Create(Body);

            ViewBag.Message = MvcHtmlString.Create(Body);
            MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(Body);
            // step 1: creation of a document-object
            Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
            doc.AddTitle("Return Request Tool");
            doc.AddCreator("RiSourceCenter");
            doc.AddAuthor("Return Request Tool");
            doc.AddHeader("Rittal USA", "The RiSourceCenter");
            // step 2:
            // we create a writer that listens to the document
            // and directs a XML-stream to a file
            PdfWriter writer = PdfWriter.GetInstance(doc, msOutput);
            // step 3: we open document and start the worker on the document
            doc.Open();
            //step 4: user the XMLWorkerHelper to read the file and add it to the doc
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, reader);
            //step 5: close all processes
            doc.Close();
            writer.CloseStream = false;
            reader.Close();
            writer.Close();
            //step 7:parse the output into a byte array to be read by the
            byte[] content = msOutput.ToArray();
            //return the content
            // Write out PDF from memory stream.
            string path = Server.MapPath("~/attachments/rfq_tool/PDFs/pdf_form_Ext_" + rFQViewModelExt.id + "_form_" + rFQViewModelExt.form_id + ".pdf");
            System.IO.File.Delete(path);
            System.IO.File.WriteAllBytes(path, content);//create the PDF file

            return Redirect("~/attachments/rfq_tool/PDFs/pdf_form_Ext_" + rFQViewModelExt.id + "_form_" + rFQViewModelExt.form_id + ".pdf");
        }
        #endregion

        #region render view to string function
        static string RenderViewToString(ControllerContext context, string viewPath, object model = null, bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view, context.Controller.ViewData, context.Controller.TempData, sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }
        #endregion

        #region RFQ Report
        public async Task<ActionResult> RFQReport(string users = null, int form_ID = 0, string user_type = null, string company = null, string status_type = null, string sDate = null, string eDate = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<RFQReportModel> myRFQs = new List<RFQReportModel>();
           
            //Collect the user's data
            var RFQ_User_data = db.RFQViewModels.Join(
                    db.UserViewModels,
                    rfq => rfq.user_id,
                    usr => usr.usr_ID,
                    (rfq, usr) => new { rfq, usr }
                ).Where(a=>a.rfq.save != "Save Progress");
            //Get the logs data
            var rfq_action_logs = dbEntity.RFQ_Action_Log;

            //Add the data to the comp_listing object
            List<SelectListItem> comp_listing = new List<SelectListItem>();
            comp_listing.Add(new SelectListItem { Text = "Select your company", Value = "select", Selected = true });//default value for select dropdown
            foreach (var item in RFQ_User_data.Where(a => a.rfq.admin_status == "Assigned").GroupBy(group => new { group.rfq.sold_to_party }))//iterate the add function
            {
                comp_listing.Add(new SelectListItem { Text = item.Key.sold_to_party, Value = item.Key.sold_to_party });
            }
            //Add the data to the comp_listing object
            List<SelectListItem> users_listing = new List<SelectListItem>();
            users_listing.Add(new SelectListItem { Text = "Select a user", Value = "select", Selected = true });//default value for select dropdown
            //iterate the add function
            foreach (var item in RFQ_User_data.Where(a => a.rfq.admin_status == "Assigned").GroupBy(group => new { userid=group.rfq.user_id,fname= group.usr.usr_fName, lname = group.usr.usr_lName, }))
            {
                users_listing.Add(new SelectListItem { Text = item.Key.fname + " " + item.Key.lname, Value = item.Key.userid.ToString() });
            }
            //Activate start date
            DateTime eeDate = DateTime.Today;
            if (!string.IsNullOrEmpty(eDate))
            {
               eeDate = Convert.ToDateTime(eDate);
            }
            //Activate end date
            DateTime ssDate = DateTime.Today;
            if (!string.IsNullOrEmpty(sDate))
            {
                ssDate = Convert.ToDateTime(sDate);
            }

            if (users != "select" && company == "select" && sDate == "" && eDate == "")
            {
                int usrid = Convert.ToInt32(users);
                //filter by users
                RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid);
            }
            else if (users == "select" && company != "select" && sDate == "" && eDate == "")
            {
                //filter by company
                RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company);
            }
            else if (users == "select" && company == "select" && sDate != "" && eDate == "")
            {
                //filter by start date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.completion_date >= ssDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.submission_date >= ssDate);
                }
            }
            else if (users == "select" && company == "select" && sDate == "" && eDate != "")
            {
                //filter by end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.submission_date <= eeDate);
                }
            }
            else if (users != "select" && company != "select" && sDate == "" && eDate == "")
            {
                int usrid = Convert.ToInt32(users);
                //filter by company and user
                RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.user_id == usrid);
            }
            else if (users == "select" && company != "select" && sDate != "" && eDate == "")
            {
                //filter by company and start date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.completion_date >= ssDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.submission_date >= ssDate);
                }
            }
            else if (users == "select" && company != "select" && sDate == "" && eDate != "")
            {
                //filter by company and end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.submission_date <= eeDate);
                }
            }
            else if (users != "select" && company == "select" && sDate != "" && eDate == "")
            {
                int usrid = Convert.ToInt32(users);
                //filter by user and start date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.completion_date >= ssDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.submission_date >= ssDate);
                }
            }
            else if (users != "select" && company == "select" && sDate == "" && eDate != "")
            {
                int usrid = Convert.ToInt32(users);
                //filter by user and end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.submission_date <= eeDate);
                }
            }
            else if (users != "select" && company == "select" && sDate != "" && eDate != "")
            {
                int usrid = Convert.ToInt32(users);
                //filter by start date and end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.completion_date >= ssDate && a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.user_id == usrid && a.rfq.submission_date >= ssDate && a.rfq.submission_date <= eeDate);
                }
            }
            else if (users == "select" && company != "select" && sDate != "" && eDate != "")
            {
                //filter by start date and end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.completion_date >= ssDate && a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.sold_to_party == company && a.rfq.submission_date >= ssDate && a.rfq.submission_date <= eeDate);
                }
            }
            else if (users == "select" && company == "select" && sDate != "" && eDate != "")
            {
                //filter by start date and end date
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.completion_date >= ssDate && a.rfq.completion_date <= eeDate);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.submission_date >= ssDate && a.rfq.submission_date <= eeDate);
                }
            }
            else if (form_ID == 0 && users != "select" && company != "select" && sDate != "" && eDate != "")
            {
                int usrid = Convert.ToInt32(users);
                if (Request.QueryString["date_type"] == "complete_date")
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.completion_date >= ssDate && a.rfq.completion_date <= eeDate && a.rfq.sold_to_party == company && a.rfq.user_id == usrid);
                }
                else
                {
                    RFQ_User_data = RFQ_User_data.Where(a => a.rfq.submission_date >= ssDate && a.rfq.submission_date <= eeDate && a.rfq.sold_to_party == company && a.rfq.user_id == usrid);
                }
            }

            if (form_ID != 0)
            {
                //Filter by form ID
                foreach (var item in RFQ_User_data)
                {
                    IQueryable<RFQ_Action_Log> rfq_action_log=null;

                    string salesPerson = "";
                    string compName = "";
                    if (item.rfq.sold_to_party != "Distributor Submission")
                    {
                        salesPerson = item.rfq.sales_engineer;
                        compName = item.rfq.sold_to_party;
                    }
                    else
                    {
                        salesPerson = item.rfq.distro_name;
                        compName = item.rfq.distro_company;
                    }

                    if (item.rfq.ID == form_ID)
                    {
                        rfq_action_log = rfq_action_logs.Where(a => a.Form_ID == item.rfq.ID).OrderByDescending(a => a.ID);
                        //Get logs
                        List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                        bool isCloned = false;
                        bool isReturned = false;
                        string status = string.Empty;
                        string admin_notes = string.Empty;
                        string getAdminFirstName = string.Empty;

                        if (rfq_action_log.Count() > 0)
                        {
                            //Check if it is ever been cloned
                            var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                            if (cloneddata.Count() > 0)
                            {
                                isCloned = true;
                            }

                            //Check if it is ever been Returned
                            var returneddata = rfq_action_log.Where(a => a.Action == "Returned" || a.Action == "Assigned-Returned");
                            if (returneddata.Count() > 0)
                            {
                                isReturned = true;
                                admin_notes = returneddata.FirstOrDefault().Notes;//get admin notes
                            } else
                            {
                                admin_notes = rfq_action_log.FirstOrDefault().Notes;//get admin notes
                            }
                            status = rfq_action_log.FirstOrDefault().Action;//get status
                                                                                //get admin info
                            string Admin_ID = rfq_action_log.FirstOrDefault().Admin_ID;
                            if (!string.IsNullOrEmpty(Admin_ID))
                            {
                                //Get Admin Name 
                                int adminId = Convert.ToInt32(Admin_ID);
                                var getAdminfullName = await locController.GetfullName(adminId);
                                getAdminFirstName = getAdminfullName["firstName"];
                            }
                        }
                        bool bAdd = false;
                        if (status_type != "none")
                        {
                            //filter by status type
                            if (status_type == status)
                            {
                                bAdd = true;
                            }
                            else if (status_type == "Not Assigned" && item.rfq.admin_status != "Assigned")
                            {
                                bAdd = true;
                            }
                        }
                        else
                        {
                            bAdd = true;
                        }

                        if (bAdd)
                        {
                            short checkProject = 0;
                            if (item.rfq.check_project.HasValue)
                                checkProject = item.rfq.check_project.Value;
                            myRFQs.Add(new RFQReportModel
                            {
                                form_id = item.rfq.ID,
                                user_id = item.rfq.user_id,
                                quote_num = item.rfq.Quote_Num,
                                requestor = salesPerson,
                                comp_name = compName,
                                qte_ref = item.rfq.qte_ref,
                                submission_date = item.rfq.submission_date,
                                completion_date = item.rfq.completion_date,
                                adminfullName = getAdminFirstName,
                                admin_status = status,
                                admin_notes = admin_notes,
                                send = item.rfq.send,
                                IsCloned = isCloned,
                                IsReturned = isReturned,
                                logs = rfq_action_log.ToList(),
                                end_user = item.rfq.end_user_name + "<br/>" + item.rfq.end_user_location,
                                check_project = checkProject,
                            });
                        }
                    }                  
                }
            }
            else if (form_ID == 0 && user_type == "rittal")
            {
                foreach (var item in RFQ_User_data)
                {
                    IQueryable<RFQ_Action_Log> rfq_action_log=null;

                    if (item.rfq.sold_to_party != "Distributor Submission")
                    {
                        rfq_action_log = rfq_action_logs.Where(a => a.Form_ID == item.rfq.ID).OrderByDescending(a => a.ID);
                        //Get logs
                        List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                        bool isCloned = false;
                        bool isReturned = false;
                        string status = string.Empty;
                        string admin_notes = string.Empty;
                        string getAdminFirstName = string.Empty;

                        if (rfq_action_log.Count() > 0)
                        {
                            //Check if it is ever been cloned
                            var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                            if (cloneddata.Count() > 0)
                            {
                                isCloned = true;
                            }

                            //Check if it is ever been Returned
                            var returneddata = rfq_action_log.Where(a => a.Action == "Returned" || a.Action == "Assigned-Returned");
                            if (returneddata.Count() > 0)
                            {
                                isReturned = true;
                                admin_notes = returneddata.FirstOrDefault().Notes;//get admin notes
                            } else
                            {
                                admin_notes = rfq_action_log.FirstOrDefault().Notes;//get admin notes
                            }
                            status = rfq_action_log.FirstOrDefault().Action;//get status

                                                                                //get admin info
                            string Admin_ID = rfq_action_log.FirstOrDefault().Admin_ID;
                            if (!string.IsNullOrEmpty(Admin_ID))
                            {
                                //Get Admin Name 
                                int adminId = Convert.ToInt32(Admin_ID);
                                var getAdminfullName = await locController.GetfullName(adminId);
                                getAdminFirstName = getAdminfullName["firstName"];
                            }
                        }
                        bool bAdd = false;
                        if (status_type != "none")
                        {
                            //filter by status type
                            if (status_type == status)
                            {
                                bAdd = true;
                            }
                            else if (status_type == "Not Assigned" && item.rfq.admin_status != "Assigned")
                            {
                                bAdd = true;
                            }
                        }
                        else
                        {
                            bAdd = true;
                        }

                        if (bAdd)
                        {
                            short checkProject = 0;
                            if (item.rfq.check_project.HasValue)
                                checkProject = item.rfq.check_project.Value;
                            myRFQs.Add(new RFQReportModel
                            {
                                form_id = item.rfq.ID,
                                user_id = item.rfq.user_id,
                                quote_num = item.rfq.Quote_Num,
                                requestor = item.rfq.sales_engineer,
                                comp_name = item.rfq.sold_to_party,
                                qte_ref = item.rfq.qte_ref,
                                submission_date = item.rfq.submission_date,
                                completion_date = item.rfq.completion_date,
                                adminfullName = getAdminFirstName,
                                admin_status = status,
                                admin_notes = admin_notes,
                                send = item.rfq.send,
                                IsCloned = isCloned,
                                IsReturned = isReturned,
                                logs = rfq_action_log.ToList(),
                                end_user = item.rfq.end_user_name + "<br/>" + item.rfq.end_user_location,
                                check_project = checkProject
                            });
                        }
                    }
                }
            }
            else if (form_ID == 0 && user_type == "distro")
            {
                foreach (var item in RFQ_User_data)
                {
                    IQueryable<RFQ_Action_Log> rfq_action_log = null;

                    if (item.rfq.sold_to_party == "Distributor Submission")
                    {
                        rfq_action_log = rfq_action_logs.Where(a => a.Form_ID == item.rfq.ID).OrderByDescending(a => a.ID);
                        //Get logs
                        List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                        bool isCloned = false;
                        bool isReturned = false;
                        string status = string.Empty;
                        string admin_notes = string.Empty;
                        string getAdminFirstName = string.Empty;

                        if (rfq_action_log.Count() > 0)
                        {
                            //Check if it is ever been cloned
                            var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                            if (cloneddata.Count() > 0)
                            {
                                isCloned = true;
                            }
                            //Check if it is ever been Returned
                            var returneddata = rfq_action_log.Where(a => a.Action == "Returned" || a.Action == "Assigned-Returned");
                            if (returneddata.Count() > 0)
                            {
                                isReturned = true;
                                admin_notes = returneddata.FirstOrDefault().Notes;//get admin notes
                            } else
                            {
                                admin_notes = rfq_action_log.FirstOrDefault().Notes;//get admin notes
                            }
                            status = rfq_action_log.FirstOrDefault().Action;//get status
                            
                                                                                //get admin info
                            string Admin_ID = rfq_action_log.FirstOrDefault().Admin_ID;
                            if (!string.IsNullOrEmpty(Admin_ID))
                            {
                                //Get Admin Name 
                                int adminId = Convert.ToInt32(Admin_ID);
                                var getAdminfullName = await locController.GetfullName(adminId);
                                getAdminFirstName = getAdminfullName["firstName"];
                            }
                        }

                        bool bAdd = false;
                        if (status_type != "none")
                        {
                            //filter by status type
                            if (status_type == status)
                            {
                                bAdd = true;
                            }
                            else if (status_type == "Not Assigned" && item.rfq.admin_status != "Assigned")
                            {
                                bAdd = true;
                            }
                        }
                        else
                        {
                            bAdd = true;
                        }
                        if (bAdd)
                        {
                            short checkProject = 0;
                            if (item.rfq.check_project.HasValue)
                                checkProject = item.rfq.check_project.Value;
                            myRFQs.Add(new RFQReportModel
                            {
                                form_id = item.rfq.ID,
                                user_id = item.rfq.user_id,
                                quote_num = item.rfq.Quote_Num,
                                requestor = item.rfq.distro_name,
                                comp_name = item.rfq.distro_company,
                                qte_ref = item.rfq.qte_ref,
                                submission_date = item.rfq.submission_date,
                                completion_date = item.rfq.completion_date,
                                adminfullName = getAdminFirstName,
                                admin_status = status,
                                admin_notes = admin_notes,
                                send = item.rfq.send,
                                IsCloned = isCloned,
                                IsReturned = isReturned,
                                logs = rfq_action_log.ToList(),
                                end_user = item.rfq.end_user_name + "<br/>" + item.rfq.end_user_location,
                                check_project = checkProject
                            });
                        }
                    }
                }
            }
            else
            {
                foreach (var item in RFQ_User_data)
                {
                    IQueryable<RFQ_Action_Log> rfq_action_log = null;

                    string salesPerson = "";
                    string compName = "";
                    if (item.rfq.sold_to_party != "Distributor Submission")
                    {
                        salesPerson = item.rfq.sales_engineer;
                        compName = item.rfq.sold_to_party;
                    }
                    else
                    {
                        salesPerson = item.rfq.distro_name;
                        compName = item.rfq.distro_company;
                    }

                    rfq_action_log = rfq_action_logs.Where(a => a.Form_ID == item.rfq.ID).OrderByDescending(a => a.ID);
                    //Get logs
                    List<RFQ_Action_LogViewModel> get_RFQ_logs = new List<RFQ_Action_LogViewModel>();
                    bool isCloned = false;
                    bool isReturned = false;
                    string status = string.Empty;
                    string admin_notes = string.Empty;
                    string getAdminFirstName = string.Empty;

                    if (rfq_action_log.Count() > 0)
                    {
                        //Check if it is ever been cloned
                        var cloneddata = rfq_action_log.Where(a => a.Action == "Cloned/Submitted" || a.Action == "Cloned with Changes/Not Submitted");
                        if (cloneddata.Count() > 0)
                        {
                            isCloned = true;
                        }
                        //Check if it is ever been Returned
                        var returneddata = rfq_action_log.Where(a => a.Action == "Returned" || a.Action == "Assigned-Returned");
                        if (returneddata.Count() > 0)
                        {
                            isReturned = true;
                            admin_notes = returneddata.FirstOrDefault().Notes;//get admin notes
                        } else
                        {
                            admin_notes = rfq_action_log.FirstOrDefault().Notes;//get admin notes
                        }
                        status = rfq_action_log.FirstOrDefault().Action;//get status
                        
                        //get admin info
                        string Admin_ID = rfq_action_log.FirstOrDefault().Admin_ID;
                        if (!string.IsNullOrEmpty(Admin_ID))
                        {
                            //Get Admin Name 
                            int adminId = Convert.ToInt32(Admin_ID);
                            var getAdminfullName = await locController.GetfullName(adminId);
                            getAdminFirstName = getAdminfullName["firstName"];
                        }
                    }

                    var bAdd = false;
                    if (status_type != "none")
                    {
                        //filter by status type
                        if (status_type == status)
                        {
                            bAdd = true;
                        }
                        else if (status_type == "Not Assigned" && item.rfq.admin_status != "Assigned")
                        {
                            bAdd = true;
                        }
                    }
                    else
                    {
                        bAdd = true;
                    }
                    if (bAdd)
                    {
                        short checkProject = 0;
                        if (item.rfq.check_project.HasValue)
                            checkProject = item.rfq.check_project.Value;
                        myRFQs.Add(new RFQReportModel
                        {
                            form_id = item.rfq.ID,
                            user_id = item.rfq.user_id,
                            quote_num = item.rfq.Quote_Num,
                            requestor = item.rfq.distro_name,
                            comp_name = item.rfq.distro_company,
                            qte_ref = item.rfq.qte_ref,
                            submission_date = item.rfq.submission_date,
                            completion_date = item.rfq.completion_date,
                            adminfullName = getAdminFirstName,
                            admin_status = status,
                            admin_notes = admin_notes,
                            send = item.rfq.send,
                            IsCloned = isCloned,
                            IsReturned = isReturned,
                            logs = rfq_action_log.ToList(),
                            end_user = item.rfq.end_user_name + "<br/>" + item.rfq.end_user_location,
                            check_project = checkProject
                        });
                    }
                }
            }

            List<RFQReportModel> list_rfqs = new List<RFQReportModel>();
            TimeSpan? diff_hours;
            double totalrounded = 0;
            double totalroundedWithoutWait = 0;
            int totalCompletedForms = 0;//Total forms completed
            int totalForms = myRFQs.Count();//Total number of forms
            int totalReturnedforms = 0;
            double totalCompletedhours = 0;
            double totalCompletedhoursWithoutWait = 0;
            int countUnder24 = 0;
            double hoursUnder24 = 0;
            double averageUnder24HoursCalc = 0;
            double averageUnder24HoursCalcWithoutWait = 0;
            int percUnder24HoursCalc = 0;
            int percUnder24HoursCalcWithoutWait = 0;
            int percentReturned = 0;
            int averageHoursCalc = 0;
            int averageHoursCalcWithoutWait = 0;
            int totalUnder24 = 0;
            int totalUnder48 = 0;
            double hoursUnder24WithoutWait = 0;


            List<Yearly_holidays> Yearly_holidays = new List<Yearly_holidays> {
                new Yearly_holidays { memorialDay ="5/30/2016", independenceDay ="7/04/2016", labourDay ="9/05/2016", thanksgivingEve = "11/23/2016", thanksgivingDay = "11/24/2016", christmasEve = "12/24/2016", christmasDay = "12/25/2016", BoxingDay = "12/26/2016", TwentyNith = "12/29/2016", NewYearsDay = "01/01/2017" },
                new Yearly_holidays { memorialDay ="5/29/2017", independenceDay ="7/04/2017", labourDay ="9/04/2017", thanksgivingEve = "11/23/2017", thanksgivingDay = "11/24/2017", christmasEve = "12/24/2017", christmasDay = "12/25/2017", BoxingDay = "12/26/2017", TwentyNith = "12/29/2017", NewYearsDay = "01/01/2018" },
                new Yearly_holidays { memorialDay ="5/28/2018", independenceDay ="7/04/2018", labourDay ="9/03/2018", thanksgivingEve = "11/23/2018", thanksgivingDay = "11/24/2018", christmasEve = "12/24/2018", christmasDay = "12/25/2018", BoxingDay = "12/26/2018", TwentyNith = "12/29/2018", NewYearsDay = "01/01/2019" },
                new Yearly_holidays { memorialDay ="5/27/2019", independenceDay ="7/04/2019", labourDay ="9/03/2019", thanksgivingEve = "11/23/2019", thanksgivingDay = "11/24/2019", christmasEve = "12/24/2019", christmasDay = "12/25/2019", BoxingDay = "12/26/2019", TwentyNith = "12/29/2019", NewYearsDay = "01/01/2020" },
                new Yearly_holidays { memorialDay ="5/25/2020", independenceDay ="7/04/2020", labourDay ="9/03/2020", thanksgivingEve = "11/23/2020", thanksgivingDay = "11/24/2020", christmasEve = "12/24/2020", christmasDay = "12/25/2020", BoxingDay = "12/26/2020", TwentyNith = "12/29/2020", NewYearsDay = "01/01/2021" },
                new Yearly_holidays { memorialDay ="5/31/2021", independenceDay ="7/04/2021", labourDay ="9/03/2021", thanksgivingEve = "11/23/2021", thanksgivingDay = "11/24/2021", christmasEve = "12/24/2021", christmasDay = "12/25/2021", BoxingDay = "12/26/2021", TwentyNith = "12/29/2021", NewYearsDay = "01/01/2022" },
                new Yearly_holidays { memorialDay ="5/30/2022", independenceDay ="7/04/2022", labourDay ="9/03/2022", thanksgivingEve = "11/23/2022", thanksgivingDay = "11/24/2022", christmasEve = "12/24/2022", christmasDay = "12/25/2022", BoxingDay = "12/26/2022", TwentyNith = "12/29/2022", NewYearsDay = "01/01/2023" },
                new Yearly_holidays { memorialDay ="5/30/2023", independenceDay ="7/04/2023", labourDay ="9/03/2023", thanksgivingEve = "11/23/2023", thanksgivingDay = "11/24/2023", christmasEve = "12/24/2023", christmasDay = "12/25/2023", BoxingDay = "12/26/2023", TwentyNith = "12/29/2023", NewYearsDay = "01/01/2024" },
                new Yearly_holidays { memorialDay ="5/30/2024", independenceDay ="7/04/2024", labourDay ="9/03/2024", thanksgivingEve = "11/23/2024", thanksgivingDay = "11/24/2024", christmasEve = "12/24/2024", christmasDay = "12/25/2024", BoxingDay = "12/26/2024", TwentyNith = "12/29/2024", NewYearsDay = "01/01/2025" },
                new Yearly_holidays { memorialDay ="5/30/2025", independenceDay ="7/04/2025", labourDay ="9/03/2025", thanksgivingEve = "11/23/2025", thanksgivingDay = "11/24/2025", christmasEve = "12/24/2025", christmasDay = "12/25/2025", BoxingDay = "12/26/2025", TwentyNith = "12/29/2025", NewYearsDay = "01/01/2026" }
            };
            Dictionary<string, double> waitStatusTimes = waitStatus.ToDictionary(x => x, x => Convert.ToDouble(0));

            foreach (var rfq in myRFQs.OrderByDescending(a=>a.form_id))
            {
                //Get returned counts
                if (rfq.IsReturned)
                {
                    totalReturnedforms++;
                }

                //Total Completed
                if (rfq.completion_date != null && rfq.IsReturned && rfq.admin_status == "Returned")
                {
                    rfq.admin_status = "Completed and returned";
                }

                string total_time = "";
                double total_hours = 0;
                double hours_to_days = 0;
                double wait_time = 0;
                double total_without_wait = 0;
                Dictionary<DateTime, DateTime> waitSpans = new Dictionary<DateTime, DateTime>();
                Dictionary<DateTime, string> waitSpanStatus = new Dictionary<DateTime, string>();

                //double num_weekends = 0;
                //Total Completed
                DateTime? submissionDate = rfq.submission_date;
                if (rfq.admin_status == "Completed" || rfq.completion_date != null)
                {
                    // Calculate duration of time not spent on the rfq
                    DateTime? wait_start = null;
                    DateTime? wait_end = null;
                    string waitStartStatus = null;
                    if (rfq.logs.Count() > 0)
                    {
                        foreach(var log in rfq.logs.OrderBy(a => a.ID))
                        {
                            if(waitStatus.Where(x => x == log.Action).Count() > 0 && !wait_start.HasValue)
                            {
                                wait_start = log.Action_Time;
                                waitStartStatus = log.Action;
                            }
                            else if(wait_start.HasValue && waitStatus.Where(x => x == log.Action).Count() > 0 && waitStartStatus != log.Action)
                            {
                                wait_end = log.Action_Time;
                                waitSpans[wait_start.Value] = wait_end.Value;
                                waitSpanStatus[wait_start.Value] = waitStartStatus;
                                TimeSpan? diff_wait = wait_end - wait_start;
                                if (diff_wait != null)
                                {
                                    double diff = Convert.ToDouble(diff_wait.Value.TotalHours.ToString());
                                    wait_time += diff;
                                    if (waitStartStatus != null && waitStatusTimes.ContainsKey(waitStartStatus))
                                    {
                                        waitStatusTimes[waitStartStatus] += diff;
                                    }

                                }
                                wait_start = log.Action_Time;
                                waitStartStatus = log.Action;
                            }
                            else if(wait_start.HasValue && waitStatus.Where(x => x == log.Action).Count() == 0)
                            {
                                wait_end = log.Action_Time;
                                waitSpans[wait_start.Value] = wait_end.Value;
                                waitSpanStatus[wait_start.Value] = waitStartStatus;
                                TimeSpan? diff_wait = wait_end - wait_start;
                                if(diff_wait != null)
                                {
                                    double diff = Convert.ToDouble(diff_wait.Value.TotalHours.ToString());
                                    wait_time += diff;
                                    if(waitStartStatus != null && waitStatusTimes.ContainsKey(waitStartStatus))
                                    {
                                        waitStatusTimes[waitStartStatus] += diff;
                                    }
                                    
                                }
                                wait_start = null;
                                waitStartStatus = null;
                            }

                            if (submitStatus.Any(x => x == log.Action) && log.Action_Time.HasValue && log.Action_Time < submissionDate)
                                submissionDate = log.Action_Time;
                        }
                    }


                    //Calculate total minutes spent on the rfq
                    diff_hours = rfq.completion_date - submissionDate;
                    if (diff_hours != null)
                    {
                        hours_to_days = diff_hours.Value.TotalDays;//Convert hours to days

                        DayOfWeek DayOfweek = submissionDate.Value.DayOfWeek;
                        string startdate = String.Format("{0:d}", submissionDate);
                        string enddate = string.Format("{0:d}", rfq.completion_date);
                        total_hours = Convert.ToDouble(diff_hours.Value.TotalHours.ToString());
                        total_without_wait = total_hours;
                        //get year from data
                        string yearOfDate = submissionDate.Value.Year.ToString();

                        var getDates = Yearly_holidays.Where(a=>a.thanksgivingDay.Contains(yearOfDate)).FirstOrDefault();
                        if(wait_time != 0)
                        {
                            total_hours = total_hours - wait_time;
                        }
                        for (double i = 0; i < hours_to_days; i++)
                        {
                            DateTime Newdata = submissionDate.Value.AddDays(i);
                            bool bWait = false;
                            DateTime? waitKey = null;
                            foreach (KeyValuePair<DateTime, DateTime> waitEntry in waitSpans)
                            {
                                if (Newdata.Date >= waitEntry.Key.Date && Newdata <= waitEntry.Value)
                                {
                                    bWait = true;
                                    waitKey = waitEntry.Key;
                                    break;
                                }
                            }
                            string Newstartdate = String.Format("{0:d}", Newdata);
                            DayOfWeek NewDayOfweek = Newdata.DayOfWeek;
                            //Subtract Saturday and Sundays
                            if (NewDayOfweek.ToString() == "Friday" && Newstartdate != enddate)
                            {
                                if(!bWait && total_hours > 48)
                                    total_hours -= 48;
                                if(total_without_wait > 48)
                                    total_without_wait -= 48;
                                if (waitKey.HasValue && waitSpanStatus.ContainsKey(waitKey.Value))
                                    waitStatusTimes[waitSpanStatus[waitKey.Value]] -= 48;
                                
                            }

                            //Day is not Saturday or Sunday
                            if ((NewDayOfweek.ToString() != "Saturday" || NewDayOfweek.ToString() != "Sunday") && getDates != null)
                            {
                                //Subtract thanksgiving
                                if ((Newstartdate == getDates.thanksgivingEve || Newstartdate == getDates.thanksgivingDay))
                                {
                                    if(!bWait && total_hours > 24)
                                        total_hours -= 24;
                                    if(total_without_wait > 24)
                                        total_without_wait -= 24;
                                    if (waitKey.HasValue && waitSpanStatus.ContainsKey(waitKey.Value))
                                        waitStatusTimes[waitSpanStatus[waitKey.Value]] -= 24;
                                }

                                //Subtract christmass
                                if ((Newstartdate == getDates.christmasDay || Newstartdate == getDates.BoxingDay || Newstartdate == getDates.TwentyNith))
                                {
                                    if(!bWait && total_hours > 24)
                                        total_hours -= 24;
                                    if(total_without_wait > 24)
                                        total_without_wait -= 24;
                                    if (waitKey.HasValue && waitSpanStatus.ContainsKey(waitKey.Value))
                                        waitStatusTimes[waitSpanStatus[waitKey.Value]] -= 24;
                                }

                                //Subtract New Years Day
                                if (Newstartdate == getDates.NewYearsDay)
                                {
                                    if(!bWait && total_hours > 24)
                                        total_hours -= 24;
                                    if(total_without_wait > 24)
                                        total_without_wait -= 24;
                                    if (waitKey.HasValue && waitSpanStatus.ContainsKey(waitKey.Value))
                                        waitStatusTimes[waitSpanStatus[waitKey.Value]] -= 24;
                                }

                                //Subtract labour day, independence day, memorial day
                                if ((Newstartdate == getDates.labourDay || Newstartdate == getDates.memorialDay || Newstartdate == getDates.independenceDay))
                                {
                                    if(!bWait && total_hours > 24)
                                        total_hours -= 24;
                                    if(total_without_wait > 24)
                                        total_without_wait -= 24;
                                    if (waitKey.HasValue && waitSpanStatus.ContainsKey(waitKey.Value))
                                        waitStatusTimes[waitSpanStatus[waitKey.Value]] -= 24;
                                }

                            }
                        }

                        total_time = total_hours.ToString();

                        totalrounded = (double)Math.Round(Convert.ToDouble(total_time),2);
                        totalroundedWithoutWait = (double)Math.Round(Convert.ToDouble(total_without_wait), 2);

                        totalCompletedhours = totalCompletedhours + totalrounded;
                        totalCompletedhoursWithoutWait = totalCompletedhoursWithoutWait + (double)Math.Round(Convert.ToDouble(total_without_wait), 2);

                        //Collect data about hours under 24
                        if (Convert.ToDouble(total_time)<24)
                        {
                            hoursUnder24 = hoursUnder24 + totalrounded;
                            countUnder24++;
                        }
                        if(total_without_wait < 24)
                        {
                            totalUnder24++;
                            hoursUnder24WithoutWait = hoursUnder24WithoutWait + totalroundedWithoutWait;
                        }
                        else if(total_without_wait < 48)
                        {
                            totalUnder48++;
                        }
                            
                    }
                    totalCompletedForms++;
                }

                if (Request.QueryString["date_type"] == "submission_date" && sDate != "" && eDate != "" && (submissionDate < ssDate))
                    continue;
                list_rfqs.Add(new RFQReportModel
                {
                    form_id = rfq.form_id,
                    user_id = rfq.user_id,
                    comp_type = rfq.comp_type,
                    quote_num = rfq.quote_num,
                    requestor = rfq.requestor,
                    comp_name = rfq.comp_name,
                    qte_ref = rfq.qte_ref,
                    action = rfq.admin_status,
                    adminfullName = rfq.adminfullName,
                    IsCloned = rfq.IsCloned,
                    IsReturned = rfq.IsReturned,
                    submission_date = submissionDate,
                    completion_date = rfq.completion_date,
                    total_time = totalrounded,
                    admin_notes = rfq.admin_notes,
                    end_user = rfq.end_user,
                    check_project = rfq.check_project,
                    completion_time = Math.Round(total_without_wait, 2)
            });
            }

            if (totalForms!=0)
            {
                percentReturned = (int)Math.Round((double)(100 * totalReturnedforms)/totalForms);
            }

            if (totalCompletedForms!=0 )
            {
                averageHoursCalc = (int)Math.Round((double)(totalCompletedhours)/totalCompletedForms);
                averageHoursCalcWithoutWait = (int)Math.Round((double)(totalCompletedhoursWithoutWait) / totalCompletedForms);

            }

            if (countUnder24>0)
            {
                averageUnder24HoursCalc = Math.Round((double)(hoursUnder24)/countUnder24);
                percUnder24HoursCalc = (int)Math.Round((double)(100 * countUnder24)/totalForms);
            }

            if(totalUnder24 > 0)
            {
                averageUnder24HoursCalcWithoutWait = Math.Round((double)(hoursUnder24WithoutWait) / totalUnder24);
                percUnder24HoursCalcWithoutWait = (int)Math.Round((double)(100 * totalUnder24) / totalForms);
            }

            var RFQViewModelReport = new RFQViewModelReport
            {
                list_reports_model = list_rfqs,
                list_comps = comp_listing.OrderBy(a => a.Text),
                list_users = users_listing.OrderBy(a => a.Text),
                totalReturnedforms = percentReturned,
                averageHoursCalc = averageHoursCalc,
                averageHoursCalcWithoutWait = averageHoursCalcWithoutWait,
                averageUnder24HoursCalc = averageUnder24HoursCalc,
                percUnder24HoursCalc = percUnder24HoursCalc,
                averageUnder24HoursCalcWithoutWait = averageUnder24HoursCalcWithoutWait,
                percUnder24HoursCalcWithoutWait = percUnder24HoursCalcWithoutWait,
                hoursUnder24= hoursUnder24,
                hoursUnder24WithoutWait = hoursUnder24WithoutWait,
                totalForms = totalForms,
                countUnder24 = countUnder24,
                totalUnder24 = totalUnder24,
                totalUnder48 = totalUnder48,
                waitStatusTimes = waitStatusTimes
            };

            if (!string.IsNullOrEmpty(Request.QueryString["export"]))
            {
               exportRFQ(RFQViewModelReport);
            }
            return View(RFQViewModelReport);
        }
        #endregion

        #region Function to export Excel
        public void exportRFQ(RFQViewModelReport RFQViewModelReport)
        {
            var mdf_to_excel = new DataTable("ExpMDFs");
            mdf_to_excel.Columns.Add("Form ID", typeof(int));
            mdf_to_excel.Columns.Add("Qte Num", typeof(string));
            mdf_to_excel.Columns.Add("Requester", typeof(string));
            mdf_to_excel.Columns.Add("Company Name", typeof(string));
            mdf_to_excel.Columns.Add("Project Name", typeof(string));
            mdf_to_excel.Columns.Add("End User", typeof(string));
            mdf_to_excel.Columns.Add("Submission/Start Date", typeof(string));
            mdf_to_excel.Columns.Add("Completion Date", typeof(string));
            mdf_to_excel.Columns.Add("Total Time", typeof(string));
            mdf_to_excel.Columns.Add("Start - End Time", typeof(string));
            mdf_to_excel.Columns.Add("Current Status", typeof(string));
            mdf_to_excel.Columns.Add("Admin Notes", typeof(string));
            mdf_to_excel.Columns.Add("Return Check", typeof(string));
            mdf_to_excel.Columns.Add("Cloned Check", typeof(string));

            foreach (var item in RFQViewModelReport.list_reports_model)
            {
                string status = string.Empty;
                string cloned = string.Empty;
                string returned = string.Empty;

                if (!string.IsNullOrEmpty(item.adminfullName))
                {
                    status = item.action +"-"+ item.adminfullName;
                }
                else
                {
                    status = item.action;
                }

                if (item.IsCloned)
                {
                    cloned = "Cloned";
                }

                if (item.IsReturned)
                {
                    returned = "Returned";
                }

                mdf_to_excel.Rows.Add(
                    item.form_id,
                    item.quote_num,
                    item.requestor,
                    item.comp_name,
                    item.qte_ref,
                    item.end_user,
                    string.Format("{0:d}", item.submission_date),
                    string.Format("{0:d}", item.completion_date),
                    item.total_time,
                    item.completion_time,
                    status,
                    item.admin_notes,
                    returned,
                    cloned
                );
            }

            var grid = new GridView();
            grid.DataSource = mdf_to_excel;
            grid.DataBind();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachement; filename=data.xls");
            Response.ContentType = "application/excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
           // return View();
        }
        #endregion
    }
}
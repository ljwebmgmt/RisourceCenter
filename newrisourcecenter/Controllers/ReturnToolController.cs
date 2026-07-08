using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public class ReturnToolController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();
        CommonController locController = new CommonController();

        #region Index
        // GET: Labels
        public ActionResult Index(int prev = 0, int next = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            IQueryable<ReturnTools> returnRequest = db.returnTools.Where(a => a.user_id == userId).OrderByDescending(a => a.form_id);

            //if (next == 0)
            //{
            //    return View(returnRequest.Take(10));
            //}
            //else
            //{
            //    return View(returnRequest.Where(a => a.form_id >= next && a.form_id <= prev));
            //}
            return View(returnRequest);

        }
        #endregion

        #region Create
        // GET: Labels
        [HttpGet]
        public ActionResult Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //Create the list for the dropdown on the return type
            List<SelectListItem> list_return_types = new List<SelectListItem>();
            list_return_types.Add(new SelectListItem { Text = "Select a Return Type", Value = "select", Selected = true });//default value for select dropdown
            list_return_types.Add(new SelectListItem { Text = "Planned Stock Rotations", Value = "4" });
            list_return_types.Add(new SelectListItem { Text = "Product Return", Value = "1" });
            list_return_types.Add(new SelectListItem { Text = "Missing or Defective Product", Value = "2" });
            list_return_types.Add(new SelectListItem { Text = "Transportation Damage", Value = "3" });

            //Collect the user's data
            var userdata = db.UserViewModels.Join(
                    db.partnerCompanyViewModels,
                    usr => usr.comp_ID,
                    comp => comp.comp_ID,
                    (usr, comp) => new { usr, comp }
                ).Where(a => a.usr.usr_ID == userId).FirstOrDefault();

            ReturnToolViewModel returnToolViewModel = new ReturnToolViewModel
            {
                name = userdata.usr.usr_fName + " " + userdata.usr.usr_lName,
                phone = userdata.usr.usr_phone,
                email = userdata.usr.usr_email,
                company = userdata.comp.comp_name,
                list_return_types = list_return_types,
                request_date = null
            };

            return View(returnToolViewModel);
        }

        // Post: Labels
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "user_Id,po_num,sap_num,location,request_date,return_type,quantity,return_reason,reasoncheckbox,part_num,quote_num,attachments_lading,attachments_ul,attachments_file,count,offset_po,email,warranty")]  ReturnToolViewModel returnToolViewModel, IEnumerable<HttpPostedFileBase> attachments_file, IEnumerable<HttpPostedFileBase> attachments_ul, IEnumerable<HttpPostedFileBase> attachments_lading)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                //Get General information
                var returnTools = new ReturnTools
                {
                    user_id = returnToolViewModel.user_id,
                    po_num = returnToolViewModel.po_num,
                    request_date = returnToolViewModel.request_date,
                    submission_date = DateTime.Now,
                    completion_date = null,
                    status = "Saved",
                    sap_num = returnToolViewModel.sap_num,
                    location = returnToolViewModel.location,
                    return_type = returnToolViewModel.return_type,
                    warranty = returnToolViewModel.warranty
                };
                db.returnTools.Add(returnTools);
                await db.SaveChangesAsync();
                //Get the identity returned
                int? form_id = returnTools.form_id;

                //upload part numbers
                int count = returnToolViewModel.count;
                for (int i = 1; i <= count; i++)
                {
                    if (!string.IsNullOrEmpty(Request.Form["part_num_" + i + ""]))
                    {
                        var returnExtension = new ReturnToolExtentions
                        {
                            form_id = form_id,
                            return_type = returnToolViewModel.return_type,
                            return_reason = Request.Unvalidated.Form["return_reason_" + i + ""],
                            reasoncheckbox = Request.Unvalidated.Form["reasoncheckbox_" + i + ""],
                            part_num = Request.Form["part_num_" + i + ""],
                            partpo_num = Request.Form["partpo_num_" + i + ""],
                            quantity = Convert.ToInt32(Request.Form["quantity_" + i + ""])
                        };
                        db.returnToolExtentions.Add(returnExtension);
                        await db.SaveChangesAsync();
                    }
                }

                var returnToolFiles = new ReturnToolFiles();//instantiate file object
                foreach (HttpPostedFileBase file in attachments_file)
                {
                    if (file != null && file.ContentLength > 0)
                    {

                        var fileName = Guid.NewGuid().ToString() + Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/attachments/return_tools/files"), fileName);
                        //returnToolViewModel.type_link = file;
                        file.SaveAs(path);

                        returnToolFiles = new ReturnToolFiles
                        {
                            form_id = form_id,
                            identifier = "attachments_file",
                            file_name = fileName,
                            offset_po = returnToolViewModel.offset_po
                        };
                        db.returnToolFile.Add(returnToolFiles);
                        await db.SaveChangesAsync();
                    }
                }

                int x = 0;
                if (returnToolViewModel.return_type == "2")
                {
                    foreach (HttpPostedFileBase file in attachments_ul)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // and optionally write the file to disk
                            var fileName = Path.GetFileName(file.FileName);
                            string NewFileName = "file_" + x + "_" + fileName;
                            var path = Path.Combine(Server.MapPath("~/attachments/return_tools/images"), NewFileName);
                            file.SaveAs(path);
                            //if file_id is not set or the entry is empty
                            returnToolFiles = new ReturnToolFiles
                            {
                                form_id = form_id,
                                return_type = returnToolViewModel.return_type,
                                identifier = "attachments_ul",
                                file_name = NewFileName
                            };
                            db.returnToolFile.Add(returnToolFiles);
                            await db.SaveChangesAsync();
                        }
                        x++;
                    }
                }
                else if (returnToolViewModel.return_type == "3")
                {
                    foreach (HttpPostedFileBase file in attachments_lading)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // and optionally write the file to disk
                            var fileName = Path.GetFileName(file.FileName);
                            string NewFileName = "file_" + x + "_" + fileName;
                            var path = Path.Combine(Server.MapPath("~/attachments/return_tools/images"), NewFileName);
                            file.SaveAs(path);
                            //if file_id is not set or the entry is empty
                            returnToolFiles = new ReturnToolFiles
                            {
                                form_id = form_id,
                                return_type = returnToolViewModel.return_type,
                                identifier = "attachments_lading",
                                file_name = NewFileName
                            };
                            db.returnToolFile.Add(returnToolFiles);
                            await db.SaveChangesAsync();
                        }
                        x++;
                    }
                }

                //send email
                await sendEmail(form_id, returnToolViewModel.email, "create");

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(returnTools.form_id), "ReturnTool", DateTime.Now, " Returns was created by user " + userId, "Create", Convert.ToInt32(userId));

            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], msg = "Your Return has been submitted" });
        }
        #endregion

        #region Edit
        // GET: Labels
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            //Create the list for the dropdown on the return type
            List<SelectListItem> list_return_types = new List<SelectListItem>();
            list_return_types.Add(new SelectListItem { Text = "Select a Return Type", Value = "select", Selected = true });//default value for select dropdown
            list_return_types.Add(new SelectListItem { Text = "Planned Stock Rotations", Value = "4" });
            list_return_types.Add(new SelectListItem { Text = "Product Return", Value = "1" });
            list_return_types.Add(new SelectListItem { Text = "Missing or Defective Product", Value = "2" });
            list_return_types.Add(new SelectListItem { Text = "Transportation Damage", Value = "3" });

            //Get the return tool data
            var returnRequest = db.returnTools.Where(a => a.form_id == id).FirstOrDefault();
            //Get the extension data
            var returnRequestExt = db.returnToolExtentions.Where(a => a.form_id == id);
            //Collect parts data
            List<ReturnToolExtentions> returnToolExtentions = new List<ReturnToolExtentions>();
            foreach (var part in returnRequestExt)
            {
                returnToolExtentions.Add(new ReturnToolExtentions { ext_id = part.ext_id, part_num = part.part_num, quantity = part.quantity, return_reason = part.return_reason, reasoncheckbox = part.reasoncheckbox, partpo_num = part.partpo_num });
            }
            //Collect the user data
            var userdata = db.UserViewModels.Join(
                                                    db.partnerCompanyViewModels,
                                                    usr => usr.comp_ID,
                                                    comp => comp.comp_ID,
                                                    (usr, comp) => new { usr, comp }
                                                  ).Where(a => a.usr.usr_ID == returnRequest.user_id).FirstOrDefault();
            //Get the file data
            var spreadsheets = db.returnToolFile.Where(a => a.form_id == id && a.identifier == "attachments_file");
            if (spreadsheets.Count() != 0)
            {
                //ViewBag.spreadsheet_name = spreadsheet.FirstOrDefault().file_name;
                //ViewBag.spreadsheet_id = spreadsheet.FirstOrDefault().file_id;
                ViewBag.spreadsheets = spreadsheets;
                ViewBag.offset_po = spreadsheets.FirstOrDefault().offset_po;
            }
            //Get the UL images
            var getUlImages = db.returnToolFile.Where(a => a.form_id == id && a.identifier == "attachments_ul");
            if (getUlImages.Count() != 0)
            {
                ViewBag.getUlImages = getUlImages;
            }
            //Get the Bill of lading images
            var getLadingImages = db.returnToolFile.Where(a => a.form_id == id && a.identifier == "attachments_Lading");
            if (getLadingImages.Count() != 0)
            {
                ViewBag.getLadingImages = getLadingImages;
            }
            //Get the log data
            var returnlog = db.returnToolActionLog.Where(a => a.form_id == id).OrderByDescending(a => a.log_id).Take(1);
            if (returnlog.Count() != 0)
            {
                ViewBag.notes = returnlog.FirstOrDefault().notes;
                ViewBag.authNumber = returnlog.FirstOrDefault().authNumber;
            }
            //Add all the data to the View Model
            ReturnToolViewModel returnToolViewModel = new ReturnToolViewModel();
            if (returnToolExtentions.Count() != 0)
            {
                returnToolViewModel = new ReturnToolViewModel
                {
                    form_id = returnRequest.form_id,
                    status = returnRequest.status,
                    user_id = userdata.usr.usr_ID,
                    name = userdata.usr.usr_fName + " " + userdata.usr.usr_lName,
                    phone = userdata.usr.usr_phone,
                    email = userdata.usr.usr_email,
                    company = userdata.comp.comp_name,
                    request_date = returnRequest.request_date,
                    po_num = returnRequest.po_num,
                    sap_num = returnRequest.sap_num,
                    location = returnRequest.location,
                    return_type = returnRequest.return_type,
                    list_return_types = list_return_types,
                    warranty = returnRequest.warranty,
                    returnToolExtentions = returnToolExtentions
                };
            }
            else
            {
                returnToolViewModel = new ReturnToolViewModel
                {
                    form_id = returnRequest.form_id,
                    status = returnRequest.status,
                    user_id = userdata.usr.usr_ID,
                    name = userdata.usr.usr_fName + " " + userdata.usr.usr_lName,
                    phone = userdata.usr.usr_phone,
                    email = userdata.usr.usr_email,
                    company = userdata.comp.comp_name,
                    request_date = returnRequest.request_date,
                    po_num = returnRequest.po_num,
                    sap_num = returnRequest.sap_num,
                    location = returnRequest.location,
                    return_type = returnRequest.return_type,
                    warranty = returnRequest.warranty,
                    list_return_types = list_return_types,
                };
            }

            return View(returnToolViewModel);
        }

        // Post: Labels
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "form_id,ext_id,file_id,user_id,po_num,sap_num,location,request_date,return_type,quantity,return_reason,reasoncheckbox,part_num,quote_num,attachments_laiden,attachments_ul,attachments_file,count,oldCount,offset_po,email,warranty")]  ReturnToolViewModel returnToolViewModel, IEnumerable<HttpPostedFileBase> attachments_file)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                //Get General information
                var returnTools = new ReturnTools
                {
                    form_id = returnToolViewModel.form_id,
                    user_id = returnToolViewModel.user_id,
                    po_num = returnToolViewModel.po_num,
                    request_date = returnToolViewModel.request_date,
                    submission_date = DateTime.Now,
                    completion_date = null,
                    status = "Saved",
                    sap_num = returnToolViewModel.sap_num,
                    location = returnToolViewModel.location,
                    return_type = returnToolViewModel.return_type,
                    warranty = returnToolViewModel.warranty,
                };
                db.Entry(returnTools).State = EntityState.Modified;
                await db.SaveChangesAsync();

                int count = returnToolViewModel.count;
                for (int i = 0; i <= count; i++)
                {
                    if (!string.IsNullOrEmpty(Request.Form["part_num_" + i + ""]))
                    {
                        if (i < Convert.ToInt32(Request.Form["oldCount"]))
                        {
                            var returnExtension = new ReturnToolExtentions
                            {
                                ext_id = Convert.ToInt32(Request.Form["ext_id_" + i + ""]),
                                form_id = returnToolViewModel.form_id,
                                return_type = returnToolViewModel.return_type,
                                return_reason = Request.Unvalidated.Form["return_reason_" + i + ""],
                                reasoncheckbox = Request.Unvalidated.Form["reasoncheckbox_" + i + ""],
                                part_num = Request.Form["part_num_" + i + ""],
                                partpo_num = Request.Form["partpo_num_" + i + ""],
                                quantity = Convert.ToInt32(Request.Form["quantity_" + i + ""])
                            };
                            db.Entry(returnExtension).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            var returnExtension = new ReturnToolExtentions
                            {
                                form_id = returnToolViewModel.form_id,
                                return_type = returnToolViewModel.return_type,
                                return_reason = Request.Unvalidated.Form["return_reason_" + i + ""],
                                reasoncheckbox = Request.Unvalidated.Form["reasoncheckbox_" + i + ""],
                                part_num = Request.Form["part_num_" + i + ""],
                                partpo_num = Request.Form["partpo_num_" + i + ""],
                                quantity = Convert.ToInt32(Request.Form["quantity_" + i + ""])
                            };
                            db.returnToolExtentions.Add(returnExtension);
                            await db.SaveChangesAsync();
                        }
                    }
                }

                var returnToolFiles = new ReturnToolFiles();
                //Save files data for spreadsheet
                foreach(HttpPostedFileBase file in attachments_file)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/attachments/return_tools/files"), fileName);
                        //returnToolViewModel.type_link = file;
                        file.SaveAs(path);
                            //if file_id is not set or the entry is empty
                            returnToolFiles = new ReturnToolFiles
                            {
                                form_id = returnToolViewModel.form_id,
                                identifier = "attachments_file",
                                file_name = fileName,
                                offset_po = returnToolViewModel.offset_po
                            };
                            db.returnToolFile.Add(returnToolFiles);
                            await db.SaveChangesAsync();
                    }
                }
                if (returnToolViewModel.offset_po != null)
                {
                    var getfiles = db.returnToolFile.Where(a => a.identifier == "attachments_file" && a.form_id == returnToolViewModel.form_id);
                    await getfiles.ForEachAsync(x => x.offset_po = returnToolViewModel.offset_po);
                    await db.SaveChangesAsync();
                }


                //send email
                await sendEmail(returnToolViewModel.form_id, returnToolViewModel.email, "edit");

                //Log the action by the user
                await locController.siteActionLog(Convert.ToInt32(returnToolViewModel.form_id), "ReturnTool", DateTime.Now, " Returns was edited by user " + userId, "Edit", Convert.ToInt32(userId));

            }
            else if (!ModelState.IsValid)
            {
                //check model state errors
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                locController.emailErrors(message);//send errors by email
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
            }

            return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], msg = "Your Return has been submitted" });
        }
        #endregion

        #region Upload Images
        //Upload images to the server get
        [HttpGet]
        public ActionResult UploadImages()
        {
            return View();
        }

        //Upload images to the server post
        [HttpPost]
        public async Task<JsonResult> UploadImages(string typedata, int form_id, string returnType)
        {
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        //Instantiate the returntoolfiles object
                        var returnToolFiles = new ReturnToolFiles();

                        // and optionally write the file to disk
                        var fileName = Path.GetFileName(file);
                        var path = Path.Combine(Server.MapPath("~/attachments/return_tools/images"), fileName);
                        fileContent.SaveAs(path);

                        if (typedata == "UL")
                        {
                            //if file_id is not set or the entry is empty
                            returnToolFiles = new ReturnToolFiles
                            {
                                form_id = form_id,
                                return_type = returnType,
                                identifier = "attachments_ul",
                                file_name = file
                            };
                            db.returnToolFile.Add(returnToolFiles);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            //if file_id is not set or the entry is empty
                            returnToolFiles = new ReturnToolFiles
                            {
                                form_id = form_id,
                                return_type = returnType,
                                identifier = "attachments_lading",
                                file_name = file
                            };
                            db.returnToolFile.Add(returnToolFiles);
                            await db.SaveChangesAsync();
                        }

                    }
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return Json("File uploaded successfully");
        }
        #endregion

        #region Send Email
        public async Task<string> sendEmail(int? form_id = 0, string useremail = null, string statusAction = null)
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
            MailMessage message_return;
            MailMessage message_requester;
            List<string> files = new List<string>();

            //Return HTML
            List<string> paragraphhtml = paragraph(form_id);
            //Create PDF files
            byte[] content = comm.CreatePDF(form_id, paragraphhtml[1], paragraphhtml[0]);
            // Write out PDF from memory stream.
            string path = Server.MapPath("~/attachments/return_tools/PDFs/pdf_" + form_id + ".pdf");
            System.IO.File.WriteAllBytes(path, content);
            //Collect files to be sent by email
            IQueryable<ReturnToolFiles> files_data = db.returnToolFile.Where(a => a.form_id == form_id);
            foreach (var item in files_data)
            {
                if (item.identifier == "attachments_file")
                {
                    files.Add(Server.MapPath("~/attachments/return_tools/files/" + item.file_name));
                }
                else
                {
                    files.Add(Server.MapPath("~/attachments/return_tools/images/" + item.file_name));
                }
            }
            files.Add(Server.MapPath("~/attachments/return_tools/PDFs/pdf_" + form_id + ".pdf"));
            //Get the log data
            var returnlog = db.returnToolActionLog.Where(a => a.form_id == form_id).OrderByDescending(a => a.log_id).Take(1);
            if (returnlog.Count() != 0)
            {
                ViewBag.notes = returnlog.FirstOrDefault().notes;
                if (!string.IsNullOrEmpty(returnlog.FirstOrDefault().authNumber))
                {
                    ViewBag.authNumber = returnlog.FirstOrDefault().authNumber;
                }
                else
                {
                    ViewBag.authNumber = " not yet issued";
                }
            }

            /**if (statusAction=="Completed")
            {
                //Update the database to save the completion time
                var formData = db.returnTools.Find(form_id);
                //Get General information
                formData.completion_date = DateTime.Now;
                db.Entry(formData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string body_requester = "Your return request has been completed with the <br /><br /><br /><br /><b>Authorization Number:</b>" + ViewBag.authNumber + "<br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                //Send email to the Return Requester
                message_requester = new MailMessage("webmaster@rittal.us", User.Identity.Name, "RiSourceCenter -Return has been completed", header + body_requester + footer);
                sendEmailSingle(message_requester, files);
            }
            else
            if (statusAction == "Return Received")
            {
                string body_requester = "Your return request has been recieved and we will notify you of any changes in the status of your request.<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                //Send email to the Return Requester
                message_requester = new MailMessage("webmaster@rittal.us", User.Identity.Name, "RiSourceCenter -Return has been received", header + body_requester + footer);
                sendEmailSingle(message_requester, files);
            }
            else**/
            if (statusAction == "RA Issued")
            {

                //Update status amd save
                var formData = db.returnTools.Find(form_id);

                string body_requester = "Your return request has been issued an <br /><br /><br /><br /><b>Authorization Number:</b>" + ViewBag.authNumber + "<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                //Send email to the Return Requester
                message_requester = new MailMessage("donotreply@rittal.us", useremail, "RiSourceCenter -Authorization number has been Issued", header + body_requester + footer);

                //Get General information
                formData.completion_date = DateTime.Now;
                db.Entry(formData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                sendEmailSingle(message_requester, files);
            }
            else if (statusAction == "Cancel")
            {
                //Update the database to save the completion time
                var formData = db.returnTools.Find(form_id);
                //Get General information
                formData.completion_date = DateTime.Now;
                db.Entry(formData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string body_requester = "Your return request has been canceled with the <br /><br /><br /><b>Authorization Number:</b>" + ViewBag.authNumber + "<br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                //Send email to the Return Requester
                message_requester = new MailMessage("donotreply@rittal.us", useremail, "RiSourceCenter -your return has been canceled #" + form_id + "", header + body_requester + footer);
                sendEmailSingle(message_requester, files);
            }
            else if (statusAction == "Under Review")
            {
                string body_requester = "Your return request is under review with the <br /><br /><br /><b>Authorization Number:</b>" + ViewBag.authNumber + "<br /><br /><br /><br /><b>Note:</b>" + ViewBag.notes + ".<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />"
                    +"Please use this email returns@returns.us to reply<br /><br />";
                //Send email to the Return Requester
                message_requester = new MailMessage("donotreply@rittal.us", useremail, "RiSourceCenter -your return is under review #" + form_id + "", header + body_requester + footer);
                sendEmailSingle(message_requester, files);
            }
            else if (statusAction == "edit")
            {
                string body_return = "A user has updated their return request.<br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                string body_requester = "You updated a return request and it is under review.<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />"
                    + "Please use this email returns@returns.us to reply<br /><br />";
                //Send email to the Return Team
                message_return = new MailMessage("donotreply@rittal.us", "returns@rittal.us", "RiSourceCenter -Updated return request #"+ form_id, header + body_return + footer);
                //Send email to the Return Requester
                message_requester = new MailMessage("donotreply@rittal.us", useremail, "RiSourceCenter -You updated your return #" + form_id + "", header + body_requester + footer);
                sendEmailMultiple(message_requester, message_return, files);
            }
            else
            {
                string body_return = "A new return request has been submited.<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                string body_requester = "Your request has been submited and it is under review.<br /><br /><br /><br /> Please see attached for the information or use the link to edit your request <a href=\"" + host + "/ReturnTool/Edit/" + form_id + "?n1_name=Return%20Request&status=live\" target=\"_blank\">click here</a><br /><br />";
                //Send email to the Return Team
                message_return = new MailMessage("donotreply@rittal.us", "returns@rittal.us", "RiSourceCenter -New return request #" + form_id, header + body_return + footer);
                //Send email to the Return Requester
                message_requester = new MailMessage("donotreply@rittal.us", useremail, "RiSourceCenter -Your new return request #" + form_id + "", header + body_requester + footer);
                sendEmailMultiple(message_requester, message_return, files);
            }

            return "";
        }

        public void sendEmailSingle(MailMessage message_requester, List<string> files)
        {
            //send mail
            message_requester.IsBodyHtml = true;
            foreach (string file in files)
            {
                // Create  the file attachment for this e-mail message.
                Attachment data = new Attachment(file);
                // Add time stamp information for the file.
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(file);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
                // Add the file attachment to this e-mail message.
                message_requester.Attachments.Add(data);
            }
            //Send the message.
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["Host"]);
            // Add credentials if the SMTP server requires them.
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            try
            {
                client.EnableSsl = false;
                client.Credentials = new System.Net.NetworkCredential("", "");
                client.Send(message_requester);
                
            }
            catch (Exception ex)
            {
                locController.FileLog("Exception caught in sendEmailSingle(): " + ex.Message + " " + ex.TargetSite);
            }
            finally
            {
                message_requester.Attachments.Dispose();
            }
        }

        public void sendEmailMultiple(MailMessage message_requester, MailMessage message_return, List<string> files)
        {
            //send mail
            message_return.IsBodyHtml = true;
            message_requester.IsBodyHtml = true;
            foreach (string file in files)
            {
                // Create  the file attachment for this e-mail message.
                Attachment data = new Attachment(file);
                // Add time stamp information for the file.
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(file);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
                // Add the file attachment to this e-mail message.
                message_return.Attachments.Add(data);
                message_requester.Attachments.Add(data);
            }
            //Send the message.
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["Host"]);
            // Add credentials if the SMTP server requires them.
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            try
            {
                client.EnableSsl = false;
                client.Credentials = new System.Net.NetworkCredential("", "");
                client.Send(message_return);
                client.Send(message_requester);
            }
            catch (Exception ex)
            {
                locController.FileLog("Exception caught in sendEmailMultiple(): " + ex.Message + " " + ex.TargetSite);
            }
            finally
            {
                message_return.Attachments.Dispose();
                message_requester.Attachments.Dispose();
            }
        }
        #endregion

        #region Generate Attachment
        public List<string> paragraph(int? form_id)
        {
            string return_type = "";
            List<string> html_data = new List<string>();
            //Get the return tool data
            var returnRequest = db.returnTools.Where(a => a.form_id == form_id).FirstOrDefault();
            //Get the extension data
            var returnRequestExt = db.returnToolExtentions.Where(a => a.form_id == form_id);
            //Collect parts data
            //List<ReturnToolExtentions> returnToolExtentions = new List<ReturnToolExtentions>();
            StringBuilder string_parts = new StringBuilder();
            string_parts.Append("<tr><th>Part Number</th><td style=\"width:10px;\" ></td><th>Purchase Order</th><td style=\"width:10px;\" ></td><th>Quantity</th><td style=\"width:10px;\" ></td><th>Reason</th></tr>");
            foreach (var part in returnRequestExt)
            {
                if (!string.IsNullOrEmpty(part.reasoncheckbox) && part.reasoncheckbox.Contains("1"))
                {
                    ViewBag.OO = "Over Ordered";
                }
                else
                {
                    ViewBag.OO = "";
                }
                if (!string.IsNullOrEmpty(part.reasoncheckbox) && part.reasoncheckbox.Contains("2"))
                {
                    ViewBag.WP = "Wrong Product Ordered";
                }
                else
                {
                    ViewBag.WP = "";
                }
                if (!string.IsNullOrEmpty(part.reasoncheckbox) && part.reasoncheckbox.Contains("3"))
                {
                    ViewBag.JC = "Job Cancelled";
                }
                else
                {
                    ViewBag.JC = "";
                }

                string_parts.Append("<tr><td>" + part.part_num + "</td><td style=\"width:10px;\" ></td><td>" + part.partpo_num + "</td><td style=\"width:10px;\" ></td><td>" + part.quantity + "</td><td style=\"width:10px;\" ></td><td style=\"width:40%;\" >" + HttpUtility.HtmlEncode(part.return_reason) + "<br />" + ViewBag.OO + "<br />" + ViewBag.WP + "<br />" + ViewBag.JC + "</td></tr>");
            }
            //Collect the user data
            var userdata = db.UserViewModels.Join(
                                                    db.partnerCompanyViewModels,
                                                    usr => usr.comp_ID,
                                                    comp => comp.comp_ID,
                                                    (usr, comp) => new { usr, comp }
                                                  ).Where(a => a.usr.usr_ID == returnRequest.user_id).FirstOrDefault();
            //Get return type
            if (returnRequest.return_type == "2")
            {
                return_type = "Missing or Defective Product";
            }
            else if (returnRequest.return_type == "3")
            {
                return_type = "Transportation Damage";
            }
            else if (returnRequest.return_type == "4")
            {
                return_type = "Planned Stock Rotations";
            }
            else
            {
                return_type = "Product Return";
            }

            //Collect the files
            StringBuilder files_list = new StringBuilder();
            IQueryable<ReturnToolFiles> files_data = db.returnToolFile.Where(a => a.form_id == form_id);
            foreach (var item in files_data)
            {
                if (item.identifier == "attachments_file")
                {
                    string urlink = Session["host"].ToString() + "/attachments/return_tools/files/" + item.file_name + "";
                    files_list.Append(string.Format("<a href=\"{2}://{0}\" >{1}</a>", urlink, urlink, HttpContext.Request.Url.Scheme));
                }
                else
                {
                    string urlink = Session["host"].ToString() + "/attachments/return_tools/images/" + item.file_name + "";
                    files_list.Append(string.Format("<a href=\"{2}://{0}\" >{1}</a>", urlink, urlink, HttpContext.Request.Url.Scheme));
                }
            }

            //Get the log data
            var returnlog = db.returnToolActionLog.Where(a => a.form_id == form_id).OrderByDescending(a => a.log_id).Take(1);
            if (returnlog.Count() != 0)
            {
                ViewBag.notes = returnlog.FirstOrDefault().notes;
                ViewBag.authNumber = returnlog.FirstOrDefault().authNumber;
            }

            html_data.Add(userdata.usr.usr_email);
            string html = "<div style=\"float:right;\"><img src=" + Server.MapPath("~/images/rittal_logo_header.gif") + " /></div>" +
                         "<table>" +
                         "<tr><td>Form ID #</td><td>" + form_id + "<br /><br /></td></tr>" +
                         "<tr><td>Authorization Number #</td><td>" + ViewBag.authNumber + "<br /><br /></td></tr>" +
                         "<tr><td><strong style=\"font-size:20px;\" >Personal Information</strong></td></tr>" +
                         "<tr><th>Name</th><td>" + userdata.usr.usr_fName + " " + userdata.usr.usr_lName + "</td></tr>" +
                         "<tr><th>Company</th><td>" + userdata.comp.comp_name + "</td></tr>" +
                         "<tr><th>Email</th><td>" + userdata.usr.usr_email + "</td></tr>" +
                         "<tr><th>Phone</th><td>" + userdata.usr.usr_phone + "</td></tr>" +
                         "<tr><td><br /><br /></td></tr>" +
                         "<tr><td><strong style=\"font-size:20px;\"  >General Information</strong></td></tr>" +
                         "<tr><th>Purchase Order</th><td>" + returnRequest.po_num + "</td></tr>" +
                         "<tr><th>SAP Account #</th><td>" + returnRequest.sap_num + "</td></tr>" +
                         "<tr><th>Location</th><td>" + returnRequest.location + "</td></tr>" +
                         "<tr><th>Request Date</th><td>" + string.Format("{0:MM/dd/yyyy}", returnRequest.request_date) + "</td></tr>" +
                         "<tr><th>Return Type</th><td>" + return_type + "</td></tr>" +
                         "<tr><td><br /><br /></td></tr>" +
                         "<tr><td><strong style=\"font-size:20px;\" >Product Information</strong></td></tr>" +
                         "</table>" +
                         "<table style=\"border:solid 1px #000;width:100%;font-size:10px;\" cellpadding=\"5\" cellspacing=\"10\" >" +
                           string_parts +
                         "</table><h4>Attached Files</h4>" + files_list;
            html_data.Add(html);

            return html_data;
        }
        #endregion

        #region Admin Panel
        // GET: Labels
        [HttpGet]
        public ActionResult AdminPanel(int next = 0, int prev = 0)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //Get the combined data for user and form
            var returnRequest = db.returnTools.Join(
                                                    db.UserViewModels,
                                                    rt => rt.user_id,
                                                    usr => usr.usr_ID,
                                                    (rt, usr) => new { rt, usr }
                                                ).Join(
                                                    db.partnerCompanyViewModels,
                                                    comp => comp.usr.comp_ID,
                                                    usRT => usRT.comp_ID,
                                                    (usrRT, comp) => new { usrRT, comp }
                                                ).OrderByDescending(a => a.usrRT.rt.form_id);
            //Get the log data
            var returnlog = db.returnToolActionLog.ToList();

            //Add the request to the request return tool view model object
            List<ReturnToolViewModel> returnTools = new List<ReturnToolViewModel>();
            foreach (var item in returnRequest)
            {
                int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                IEnumerable<ReturnToolActionLogs> authonumber;
                //Add the authNumber
                if (returnlog.Count() > 0)
                {
                    authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id).OrderByDescending(a => a.log_id);
                    if (authonumber.Count() > 0)
                    {
                        returnTools.Add(new ReturnToolViewModel { form_id = form_id, submission_date = item.usrRT.rt.submission_date, completion_date = item.usrRT.rt.completion_date, status = item.usrRT.rt.status, name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName, company = item.comp.comp_name, authonumber = authonumber.FirstOrDefault().authNumber });
                    }
                    else
                    {
                        returnTools.Add(new ReturnToolViewModel { form_id = form_id, submission_date = item.usrRT.rt.submission_date, completion_date = item.usrRT.rt.completion_date, status = item.usrRT.rt.status, name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName, company = item.comp.comp_name });
                    }
                }
                else
                {
                    returnTools.Add(new ReturnToolViewModel { form_id = form_id, submission_date = item.usrRT.rt.submission_date, completion_date = item.usrRT.rt.completion_date, status = item.usrRT.rt.status, name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName, company = item.comp.comp_name });
                }


            }
            //Add company types to the list
            StringBuilder user_strings = new StringBuilder();
            List<Nav1List> list_comp = new List<Nav1List>();
            foreach (var item in returnRequest.GroupBy(a => new { a.usrRT.usr.usr_ID, a.usrRT.usr.usr_lName, a.usrRT.usr.usr_fName, a.usrRT.usr.comp_ID }).OrderBy(a => a.Key.usr_fName))
            {
                user_strings.Append(string.Format("<option value={0}>{1}</option>", item.Key.usr_ID, item.Key.usr_fName + " " + item.Key.usr_lName));
                list_comp.Add(new Nav1List { id = item.Key.comp_ID });
            }
            ViewBag.list_users = user_strings;

            //Add company types to the list
            StringBuilder company_strings = new StringBuilder();
            foreach (var item in list_comp.GroupBy(m => new { m.id }))
            {
                var companyNames = db.partnerCompanyViewModels.Where(m => m.comp_ID == item.Key.id);
                company_strings.Append(string.Format("\"<option value={0}>{1}</option>\"+", item.Key.id, companyNames.FirstOrDefault().comp_name));
            }
            ViewBag.list_companys = company_strings;

            //if (next==0)
            //{
            //    return View(returnTools.Take(10));
            //}
            //else 
            //{
            //    return View(returnTools.Where(a=>a.form_id>=next && a.form_id<=prev));
            //}
            return View(returnTools);

        }
        #endregion

        #region Admin Panel Filter
        // GET: Labels
        [HttpGet]
        public ActionResult AdminPanelFilter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminPanelFilter(string filterType = null)
        {
            try
            {
                long userId = Convert.ToInt64(Session["userId"]);
                if (!Request.IsAuthenticated || userId == 0)
                {
                    return Json("Please Login. Login has timed out");
                }

                //Get the combined data for user and form
                var returnRequest = db.returnTools.Join(
                                                        db.UserViewModels,
                                                        rt => rt.user_id,
                                                        usr => usr.usr_ID,
                                                        (rt, usr) => new { rt, usr }
                                                    ).Join(
                                                        db.partnerCompanyViewModels,
                                                        comp => comp.usr.comp_ID,
                                                        usRT => usRT.comp_ID,
                                                        (usrRT, comp) => new { usrRT, comp }
                                                    ).OrderByDescending(a => a.usrRT.rt.form_id);

                List<ReturnToolViewModel> returnTools = new List<ReturnToolViewModel>();
                //Get the log data
                var returnlog = db.returnToolActionLog.ToList();

                if (filterType == "formdata")
                {
                    int? formID = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.form_id == formID))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "rma")
                {
                    string returnType = Request.QueryString["form_value"];
                    var returnToolActionLog = db.returnToolActionLog.Where(a => a.authNumber == returnType);
                    if (returnToolActionLog.Count() > 0)
                    {
                        var form_id_returnToolActionLog = returnToolActionLog.FirstOrDefault().form_id;
                        foreach (var item in returnRequest.Where(a => a.usrRT.rt.form_id == form_id_returnToolActionLog))
                        {
                            int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                            IEnumerable<ReturnToolActionLogs> authonumber;
                            //Add the authNumber
                            ReturnToolViewModel returnObj = new ReturnToolViewModel
                            {
                                form_id = form_id,
                                submission_date = item.usrRT.rt.submission_date,
                                completion_date = item.usrRT.rt.completion_date,
                                status = item.usrRT.rt.status,
                                name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                                company = item.comp.comp_name
                            };
                            if (returnlog.Count() > 0)
                            {
                                authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                                if (authonumber.Count() > 0)
                                {
                                    returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                                }
                            }
                            returnTools.Add(returnObj);
                        }
                    }
                }
                else if (filterType == "companyName")
                {
                    int? compID = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in returnRequest.Where(a => a.usrRT.usr.comp_ID == compID))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "returnType")
                {
                    string returnType = Request.QueryString["form_value"];
                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.return_type == returnType))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "status")
                {
                    string status = Request.QueryString["form_value"];
                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.status == status))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "requester")
                {
                    int? requester = Convert.ToInt32(Request.QueryString["form_value"]);
                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.user_id == requester))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "request_date")
                {
                    DateTime start_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value"]));
                    DateTime end_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value1"]));

                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.request_date >= start_date && a.usrRT.rt.request_date <= end_date))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else if (filterType == "date_completed")
                {
                    DateTime start_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value"]));
                    DateTime end_date = Convert.ToDateTime(string.Format("{0:MM/dd/yyyy}", Request.QueryString["form_value1"]));

                    foreach (var item in returnRequest.Where(a => a.usrRT.rt.completion_date >= start_date && a.usrRT.rt.completion_date <= end_date))
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }
                else
                {
                    foreach (var item in returnRequest)
                    {
                        int form_id = Convert.ToInt32(item.usrRT.rt.form_id);
                        IEnumerable<ReturnToolActionLogs> authonumber;
                        //Add the authNumber
                        ReturnToolViewModel returnObj = new ReturnToolViewModel
                        {
                            form_id = form_id,
                            submission_date = item.usrRT.rt.submission_date,
                            completion_date = item.usrRT.rt.completion_date,
                            status = item.usrRT.rt.status,
                            name = item.usrRT.usr.usr_fName + " " + item.usrRT.usr.usr_lName,
                            company = item.comp.comp_name
                        };
                        if (returnlog.Count() > 0)
                        {
                            authonumber = returnlog.Where(a => a.form_id == item.usrRT.rt.form_id);
                            if (authonumber.Count() > 0)
                            {
                                returnObj.authonumber = authonumber.FirstOrDefault().authNumber;
                            }
                        }
                        returnTools.Add(returnObj);
                    }
                }

                //return View(returnTools.Take(10));
                return View(returnTools);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
        }
        #endregion

        #region Save Admin Action
        [HttpPost]
        public async Task<JsonResult> AdminPanelSave(int form_id = 0, string status = null, string authNumber = null, string n_descLong = null, string email = null)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return Json("Please Login. Login has timed out");
            }
            try
            {
                //Update status amd save
                var formData = db.returnTools.Find(form_id);
                //Get General information
                formData.status = status;
                db.Entry(formData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //if file_id is not set or the entry is empty
                var returnToolActionLog = new ReturnToolActionLogs()
                {
                    form_id = form_id,
                    action = status,
                    action_time = DateTime.Now,
                    notes = n_descLong,
                    user_id = userId,
                    authNumber = authNumber
                };
                db.returnToolActionLog.Add(returnToolActionLog);
                await db.SaveChangesAsync();

                //send email
                await sendEmail(form_id, email, status);

                return Json("OK");
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
        }
        #endregion

        #region Delete Part
        // GET: Nav1ViewModel/Delete/5
        public async Task<ActionResult> Deletepart(int? id)
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
            ReturnToolExtentions returnToolExten = await db.returnToolExtentions.FindAsync(id);
            if (returnToolExten == null)
            {
                return HttpNotFound();
            }
            db.returnToolExtentions.Remove(returnToolExten);
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(id), "ReturnToolPart", DateTime.Now, " ReturnsTool Part from form id" + returnToolExten.form_id + " was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Edit", new { id = Request.QueryString["itemID"], n1_name = Request.QueryString["n1_name"] });
        }

        // GET: Nav1ViewModel/Delete/5
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
            ReturnTools returnRequest = await db.returnTools.FindAsync(id);//get data for form
            if (returnRequest == null)
            {
                return HttpNotFound();
            }
            var returnRequestExt = db.returnToolExtentions.Where(a => a.form_id == id);//get data from the parts or extension table
            if (returnRequestExt == null)
            {
                return HttpNotFound();
            }
            var returnRequesFiles = db.returnToolFile.Where(a => a.form_id == id);//get data from the files table
            if (returnRequesFiles == null)
            {
                return HttpNotFound();
            }
            db.returnToolFile.RemoveRange(returnRequesFiles);//remove all records for files with the form_id
            db.returnToolExtentions.RemoveRange(returnRequestExt);//remove all records for parts with form_id
            db.returnTools.Remove(returnRequest);//remove record for form
            await db.SaveChangesAsync();

            //Log the action by the user
            await locController.siteActionLog(Convert.ToInt32(id), "ReturnTool", DateTime.Now, " Returns was deleted by user " + userId, "Delete", Convert.ToInt32(userId));

            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"] });
        }

        // POST: Nav1ViewModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Nav1ViewModel nav1ViewModel = await db.Nav1ViewModel.FindAsync(id);
            db.Nav1ViewModel.Remove(nav1ViewModel);
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
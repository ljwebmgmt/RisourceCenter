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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Net.Mail;
using System.Net.Mime;
using System.Globalization;
using System.Configuration;
using System.Text;

namespace newrisourcecenter.Controllers
{
    public class CommonController : Controller
    {
        private RisourceCenterContext db;
        private RisourceCenterMexicoEntities dbEntity;

        public CommonController():this(new RisourceCenterContext(), new RisourceCenterMexicoEntities())
        {

        }

        public CommonController(RisourceCenterContext _db,RisourceCenterMexicoEntities _dbEntity)
        {
            db = _db;
            dbEntity = _dbEntity;
        }

        // GET: Common
        public ActionResult Index()
        {
            return View();
        }

        #region Get User data
        public async Task<Dictionary<string, string>> GetfullName(int usrId)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            try
            {
                var getfullName = db.UserViewModels.Where(a => a.usr_ID == usrId);
                if (getfullName.Count() != 0)
                {
                    var getname = await getfullName.FirstOrDefaultAsync();
                    names.Add("firstName", getname.usr_fName);
                    names.Add("lastName", getname.usr_lName);
                    names.Add("fullName", getname.usr_fName + " " + getname.usr_lName);
                    names.Add("email", getname.usr_email);
                    names.Add("pos", getname.usr_POS.ToString());
                    names.Add("spa", getname.usr_SAP.ToString());
                }
                else
                {
                    names.Add("firstName", string.Empty);
                    names.Add("lastName", string.Empty);
                    names.Add("fullName", string.Empty);
                    names.Add("email", string.Empty);
                    names.Add("pos", string.Empty);
                    names.Add("spa", string.Empty);
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return names;
        }
        #endregion

        #region Get Localization
        public string localization(string tableName, string columnName, string message, int parent_id, int languageId)
        {
            string data = "";
            var trans_column = dbEntity.Localizations.Where(a => a.column_name == columnName && a.table_name == tableName && a.language == languageId && a.parent_id == parent_id && a.message_translated != null);

            if (trans_column.Count() != 0)
            {
                data = trans_column.FirstOrDefault().message_translated;
            }
            else
            {
                data = message;
            }
            return data;
        }

        public string[] GetLable(string labelName, string controllerName, string pageName, long languageId, string key = "")
        {
            string[] data = { "", "" };
            var trans_labels = dbEntity.Labels.Where(a => a.controller_name == controllerName && a.page_name == pageName && a.language == languageId && a.label_name == labelName);

            if (trans_labels.Count() != 0)
            {
                data[0] = trans_labels.FirstOrDefault().translated_label;
                data[1] = labelName;
            }
            else
            {
                data[0] = labelName;
                data[1] = labelName;
            }

            return data;
        }
        #endregion

        #region PDF Processor
        public byte[] CreatePDF(int? form_id, string paragraphhtml, string email)
        {
            MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(paragraphhtml);
            // step 1: creation of a document-object
            Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
            doc.AddTitle("Return Request Tool");
            doc.AddSubject("The is the return request for form ID" + form_id);
            doc.AddKeywords("" + email + "");
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
            return content;
        }
        #endregion

        #region Email Error messages
        public void emailErrors(string errors)
        {
            try
            {
                System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage("webmaster@rittal.us", "presswala.z@rittal.us", "RiSource Center Error" + User.Identity.Name, errors);
                Email.IsBodyHtml = true;
                System.Net.Mail.SmtpClient SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
                SMPTobj.EnableSsl = false;
                SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                SMPTobj.Send(Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }
        }
        #endregion

        #region Email Handlers
        public string emailheader(string host)
        {
            string head = "";
            try
            {
                head =
                    "<!doctype html>" +
                    "<html>" +
                    "<head>" +
                    "<meta charset=\"utf-8\">" +
                    "<style>" +
                    "body{font-family:Arial, Helvetica, sans-serif;width:600px;}a, a:hover, a:visited{color:#ED0677;}" +
                    ".powerbar{background:#393939;}" +
                    "</style>" +
                    "<title>Rittal North America LLC - RiSource Center</title>" +
                    "</head>" +
                    "<body style=\"width:600px;\">" +
                    "<div style=\"width:600px;\">" +
                    "<table align=\"center\" style=\"background:#393939;width:100%;padding:4px;font-family:Verdana, Arial, Helvetica, sans-serif\">" +
                            "<tr>" +
                                "<td width=\"75px\" style=\"width:75px;\">" +
                                "<img src=\"" + host + "/images/rittal_logo_header.gif\" alt=\"Rittal Logo\" style=\"padding:4px;\" /></td>" +
                                "<td align=\"left\"><span id=\"title\" style=\"color:#FFF;font-weight:bold;font-size:26px;line-height:18px;margin-top:30px;\">&nbsp;<br />RiSource<br />Center <span style=\"font-size:11px;\">2.0</span></span></td>" +
                            "</tr>" +
                    "</table>" +
                    "<div style=\"height:20px;width:100%;background:#292929;\">&nbsp;</div>" +
                    "<strong><p style=\"color:#e2007a;text-align:left;font-family:Verdana, Arial, Helvetica, sans-serif;padding-left:8px;\">" +
                    string.Format("{0:MM/dd/yyyy}", DateTime.Now) +
                    "</p></strong>";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return head;
        }

        public string emailfooter(string host)
        {
            string foot = "";
            try
            {
                foot =
                    "<br />&nbsp;" +
                    "<div style=\"height:20px;width:100%;background:#292929;\">&nbsp;</div>" +
                    "<div class=\"powerBar\" style=\"height:23px\" align=\"center\">" +
                    "<div style=\"background-image:url(" + host + "/images/pbFiller.gif);background-repeat:repeat-x;background-size:100% 23px;\">" +
                    "<img src=\"" + host + "/images/powerBar5.gif\" width=\"100%\"></div>" +
                    "</div>" +
                        "<div align=\"center\" style=\"width:100%;background:#393939;height:100px;font-family:Verdana, Arial, Helvetica, sans-serif;\">&nbsp;" +
                            "<p style=\"color:#FFF;font-size:14px;\">Rittal North America LLC, 1 Rittal Place, Urbana, OH 43078</p>" +
                            "<p style=\"font-size:12px;\"><a href=\"http://www.risourcecenter.com/\" target=\"_blank\">RiSourceCenter.com</a> | <a href=\"http://www.rittal.us/\" target=\"_blank\">Rittal.us</a></p><br />" +
                        "</div>" +
                       "</div>" +
                    "</body>" +
                    "</html>";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Error", ex);
            }

            return foot;
        }
        #endregion

        #region Send Email function
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
                message_requester.Attachments.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateMessageWithAttachment(): {0}", ex.ToString());
            }
        }

        public void email(string From, string To, string Subject, string Body,string footer="yes", bool isBcc = false,string bccEmails = "")
        {
            try
            {
                string emailEnding = "";
                if (footer=="yes")
                {
                    emailEnding = "<br /><br />RiSource Center Webmaster<br/>Rittal North America LLC • 425 N. Martingale Rd. Suite 400 • Schaumburg, IL 60173<br />Email: webmaster@rittal.us • www.rittal.us • www.friedhelm-loh-group.com";
                    emailEnding += "<br /><br /><b><u>Rittal – The System.</u> <br />Faster – better – everywhere.</b>";
                    emailEnding += "<br /><br />This email contains confidential information. You are not authorized to copy the contents without the consent of the sender.***";
                }
                var FormAddress = new MailAddress(From, "Rittal RiSourceCenter");
                MailAddress ToAddress;
                List<string> aTo = new List<string>();
                if (To.Contains(",")) // Checks if there are more then one receipants
                {
                    aTo = To.Split(',').ToList(); // move to list
                    ToAddress = new MailAddress(aTo[0]);
                    aTo.RemoveAt(0); // remove first address
                } else
                {
                    ToAddress = new MailAddress(To);
                }
                MailMessage Email = new MailMessage(FormAddress, ToAddress);
                if(aTo.Count() > 0) // checking if receipants are there
                {
                    foreach (var address in aTo) // add to in email one by one
                    {
                        Email.To.Add(new MailAddress(address));
                    }
                }
                Email.Subject = Subject;
                Email.Body = Body + emailEnding;
                Email.IsBodyHtml = true;
                SmtpClient SMPTobj = new SmtpClient(ConfigurationManager.AppSettings["Host"]);
                if (isBcc)
                {
                    Email.Bcc.Add("presswala.z@rittal.us");
                    if (!string.IsNullOrEmpty(bccEmails))
                        Email.Bcc.Add(bccEmails);
                }
                SMPTobj.EnableSsl = false;
                SMPTobj.Credentials = new NetworkCredential("", "");
                SMPTobj.Send(Email);
            }
            catch (Exception ex)
            {
                FileLog("Email: "+ex.Message+" "+ex.TargetSite);
                FileLog("Inner Exception: " + ex.InnerException.Message);
                //Console.WriteLine("{0} Error", ex);
            }
        }
        #endregion

        #region MDFsArchive AND Processors    
        /*
        MDF statuses
        1 - Pending
        2 - Denied
        3 - Approved
        4 - Completed
        5 - Credit Issued
        6 - Cancelled 
        */

        public List<MdfParts> MDFsActivities(List<mdf_main> mdf_mainModel, partnerCompanyViewModel partnerCompanyViewModel)
        {
            //List Training
            List<PendingTraining> Pending_Training = new List<PendingTraining>();
            List<ApprovedTraining> Approved_Training = new List<ApprovedTraining>();
            List<CompletedTraining> Completed_Training = new List<CompletedTraining>();
            List<CreditTraining> Credit_Training = new List<CreditTraining>();
            //List Promotional Events
            List<PendingPromEvents> Pending_PromoEv = new List<PendingPromEvents>();
            List<ApprovedPromEvents> Approved_PromoEv = new List<ApprovedPromEvents>();
            List<CompletedPromEvents> Completed_PromoEv = new List<CompletedPromEvents>();
            List<CreditPromEvents> Credit_PromoEv = new List<CreditPromEvents>();
            //List Promotional Activities
            List<PendingPromAcitivies> Pending_PromoAc = new List<PendingPromAcitivies>();
            List<ApprovedPromAcitivies> Approved_PromoAc = new List<ApprovedPromAcitivies>();
            List<CompletedPromAcitivies> Completed_PromoAc = new List<CompletedPromAcitivies>();
            List<CreditPromAcitivies> Credit_PromoAc = new List<CreditPromAcitivies>();
            //List Merchandise
            List<PendingMerchandise> Pending_merch = new List<PendingMerchandise>();
            List<ApprovedMerchandise> Approved_merch = new List<ApprovedMerchandise>();
            List<CompletedMerchandise> Completed_merch = new List<CompletedMerchandise>();
            List<CreditMerchandise> Credit_merch = new List<CreditMerchandise>();
            //List Display Product
            List<PendingDisplayProduct> Pending_dp = new List<PendingDisplayProduct>();
            List<ApprovedDisplayProduct> Approved_dp = new List<ApprovedDisplayProduct>();
            List<CompletedDisplayProduct> Completed_dp = new List<CompletedDisplayProduct>();
            List<CreditDisplayProduct> Credit_dp = new List<CreditDisplayProduct>();
            //List Other Product
            List<PendingOtherProduct> Pending_op = new List<PendingOtherProduct>();
            List<ApprovedOtherProduct> Approved_op = new List<ApprovedOtherProduct>();
            List<CompletedOtherProduct> Completed_op = new List<CompletedOtherProduct>();
            List<CreditOtherProduct> Credit_op = new List<CreditOtherProduct>();
            //List RAS
            List<PendingActivity> Pending_ras = new List<PendingActivity>();
            List<ApprovedActivity> Approved_ras = new List<ApprovedActivity>();
            List<CompletedActivity> Completed_ras = new List<CompletedActivity>();
            List<CreditActivity> Credit_ras = new List<CreditActivity>();
            //List SPIF
            List<PendingActivity> Pending_spif = new List<PendingActivity>();
            List<ApprovedActivity> Approved_spif = new List<ApprovedActivity>();
            List<CompletedActivity> Completed_spif = new List<CompletedActivity>();
            List<CreditActivity> Credit_spif = new List<CreditActivity>();
            //List Tradeshow
            List<PendingActivity> Pending_tradeshow = new List<PendingActivity>();
            List<ApprovedActivity> Approved_tradeshow = new List<ApprovedActivity>();
            List<CompletedActivity> Completed_tradeshow = new List<CompletedActivity>();
            List<CreditActivity> Credit_tradeshow = new List<CreditActivity>();

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1 && item.mdf_type == 1)
                {
                    //Training column for pending
                    Pending_Training.Add(new PendingTraining { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 1)
                {
                    //Traing column for Approved 
                    Approved_Training.Add(new ApprovedTraining { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Completed_Training.Add(new CompletedTraining { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Credit_Training.Add(new CreditTraining { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 2)
                {
                    //Promotional Event column for pending
                    Pending_PromoEv.Add(new PendingPromEvents { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 2)
                {
                    //Promotional Event column for Approved 
                    Approved_PromoEv.Add(new ApprovedPromEvents { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Completed_PromoEv.Add(new CompletedPromEvents { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Credit_PromoEv.Add(new CreditPromEvents { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 3)
                {
                    //Promotional Activities column for pending
                    Pending_PromoAc.Add(new PendingPromAcitivies { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 3)
                {
                    //Promotional Activities column for Approved 
                    Approved_PromoAc.Add(new ApprovedPromAcitivies { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Completed_PromoAc.Add(new CompletedPromAcitivies { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Credit_PromoAc.Add(new CreditPromAcitivies { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 4)
                {
                    //Merchandise column for pending
                    Pending_merch.Add(new PendingMerchandise { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 4)
                {
                    //Merchandise column for Approved 
                    Approved_merch.Add(new ApprovedMerchandise { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Completed_merch.Add(new CompletedMerchandise { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Credit_merch.Add(new CreditMerchandise { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 5)
                {
                    //Display Product column for pending
                    Pending_dp.Add(new PendingDisplayProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 5)
                {
                    //Display Product column for Approved 
                    Approved_dp.Add(new ApprovedDisplayProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Completed_dp.Add(new CompletedDisplayProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Credit_dp.Add(new CreditDisplayProduct { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 6)
                {
                    //Other Products column for pending
                    Pending_op.Add(new PendingOtherProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 6)
                {
                    //Other Products column for Approved 
                    Approved_op.Add(new ApprovedOtherProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Completed_op.Add(new CompletedOtherProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Credit_op.Add(new CreditOtherProduct { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 7)
                {
                    //Training column for pending
                    Pending_ras.Add(new PendingActivity { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 7)
                {
                    //Traing column for Approved 
                    Approved_ras.Add(new ApprovedActivity { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 7)
                {
                    //Traing column for Completed
                    Completed_ras.Add(new CompletedActivity { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 7)
                {
                    //Traing column for Completed
                    Credit_ras.Add(new CreditActivity { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 8)
                {
                    //Training column for pending
                    Pending_tradeshow.Add(new PendingActivity { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 8)
                {
                    //Traing column for Approved 
                    Approved_tradeshow.Add(new ApprovedActivity { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 8)
                {
                    //Traing column for Completed
                    Completed_tradeshow.Add(new CompletedActivity { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 8)
                {
                    //Traing column for Completed
                    Credit_tradeshow.Add(new CreditActivity { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 10)
                {
                    //Training column for pending
                    Pending_spif.Add(new PendingActivity { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 10)
                {
                    //Traing column for Approved 
                    Approved_spif.Add(new ApprovedActivity { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 10)
                {
                    //Traing column for Completed
                    Completed_spif.Add(new CompletedActivity { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 10)
                {
                    //Traing column for Completed
                    Credit_spif.Add(new CreditActivity { value = item.mdf_validatedAmt });
                }
            }

            //Calculate Training Data
            double? spentTraining = Approved_Training.Sum(a => a.value) + Completed_Training.Sum(a => a.value) + Credit_Training.Sum(a => a.value);
            double trainingLimit = (partnerCompanyViewModel.comp_MDF_amount.HasValue ? partnerCompanyViewModel.comp_MDF_amount.Value : 0) * 0.5;
            double? TrainingAva = trainingLimit - spentTraining;
            int PctTrainingUsed = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_amount);
            int PctTraining = getPercentage(spentTraining, trainingLimit);
            double PctTrainingValid = getPercentage(Credit_Training.Sum(a => a.value), trainingLimit);

            //Calculate Promotional Events Data
            double? spentPromotionalEvent = Approved_PromoEv.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value);
            double? PromoeAva = partnerCompanyViewModel.comp_MDF_eLimit - spentPromotionalEvent;
            int PctPromotionalEventUsed = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalEvent = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_eLimit);
            double PctPromotEventValid = getPercentage(Credit_PromoEv.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_eLimit);

            //Calculate Promotional Activities Data
            double? spentPromotionalActivity = Approved_PromoAc.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value);
            double? PromoaAva = partnerCompanyViewModel.comp_MDF_aLimit - spentPromotionalActivity;
            int PctPromotionalActivityUsed = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalActivity = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_aLimit);
            double PctPromoActivityValid = getPercentage(Credit_PromoAc.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_aLimit);

            //Calculate Merchandise Data
            double? spentMerchandise = Approved_merch.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Credit_merch.Sum(a => a.value);
            double? MerchAva = partnerCompanyViewModel.comp_MDF_mLimit - spentMerchandise;
            int PctMerchandiseUsed = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_amount);
            int PctMerchandise = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_mLimit);
            double PctMerchandiseValid = getPercentage(Credit_merch.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_mLimit);

            //Calculate Display Products Data
            double? spentDisplayProducts = Approved_dp.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Credit_dp.Sum(a => a.value);
            double? DpAva = partnerCompanyViewModel.comp_MDF_dLimit - spentDisplayProducts;
            int PctDisplayProductsUsed = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_amount);
            int PctDisplayProducts = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_dLimit);
            double PctDisplayProdValid = getPercentage(Credit_dp.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_dLimit);

            //Calculate Other Products Data
            double? spentOtherActivity = Approved_op.Sum(a => a.value) + Completed_op.Sum(a => a.value) + Credit_op.Sum(a => a.value);
            double? OtherAva = partnerCompanyViewModel.comp_MDF_oLimit - spentOtherActivity;
            int PctOtherActivityUsed = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctOtherActivity = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_oLimit);
            double PctOtherActivityValid = getPercentage(Credit_op.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_oLimit);

            //Calculate RAS Data
            double? spentRAS = Approved_ras.Sum(a => a.value) + Completed_ras.Sum(a => a.value) + Credit_ras.Sum(a => a.value);
            double? rasAva = partnerCompanyViewModel.comp_MDF_amount - spentRAS;
            int PctRASUsed = getPercentage(spentRAS, partnerCompanyViewModel.comp_MDF_amount);
            int PctRAS = getPercentage(spentRAS, partnerCompanyViewModel.comp_MDF_amount);
            double PctRasValid = getPercentage(Credit_ras.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_amount);

            //Calculate Tradeshow Data
            double? spentTradeshow = Approved_tradeshow.Sum(a => a.value) + Completed_tradeshow.Sum(a => a.value) + Credit_tradeshow.Sum(a => a.value);
            double tradeLimit = (partnerCompanyViewModel.comp_MDF_amount.HasValue ? partnerCompanyViewModel.comp_MDF_amount.Value : 0) * 0.5;
            double? tradeAva = tradeLimit - spentTradeshow;
            int PctTradeUsed = getPercentage(spentTradeshow, partnerCompanyViewModel.comp_MDF_amount);
            int PctTrade = getPercentage(spentTradeshow, tradeLimit);
            double PctTradeValid = getPercentage(Credit_tradeshow.Sum(a => a.value), tradeLimit);

            //Calculate SPIF Data
            double? spentSPIF = Approved_spif.Sum(a => a.value) + Completed_spif.Sum(a => a.value) + Credit_spif.Sum(a => a.value);
            double spifLimit = (partnerCompanyViewModel.comp_MDF_amount.HasValue ? partnerCompanyViewModel.comp_MDF_amount.Value : 0) * 0.25;
            double? spifAva = spifLimit - spentSPIF;
            int PctSPIFUsed = getPercentage(spentSPIF, partnerCompanyViewModel.comp_MDF_amount);
            int PctSPIF = getPercentage(spentSPIF, spifLimit);
            double PctSPIFValid = getPercentage(Credit_spif.Sum(a => a.value), spifLimit);

            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = spentTraining + spentPromotionalEvent + spentPromotionalActivity + spentMerchandise + spentDisplayProducts + spentOtherActivity + spentRAS + spentTradeshow + spentSPIF;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MDF_amount);
            double? TotalAvailable = partnerCompanyViewModel.comp_MDF_amount - TotalUsed;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MDF_amount);

            //Calculating total Pending, Approved, Complete
            double? Pending = Pending_Training.Sum(a => a.value) + Pending_PromoEv.Sum(a => a.value) + Pending_PromoAc.Sum(a => a.value) + Pending_merch.Sum(a => a.value) + Pending_dp.Sum(a => a.value) + Pending_op.Sum(a => a.value) + Pending_ras.Sum(a => a.value) + Pending_tradeshow.Sum(a => a.value) + Pending_spif.Sum(a => a.value);
            double? Approved = Approved_Training.Sum(a => a.value) + Approved_PromoEv.Sum(a => a.value) + Approved_PromoAc.Sum(a => a.value) + Approved_merch.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_op.Sum(a => a.value) + Approved_ras.Sum(a => a.value) + Approved_tradeshow.Sum(a => a.value) + Approved_spif.Sum(a => a.value);
            double? Complete = Completed_Training.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Completed_op.Sum(a => a.value) + Completed_ras.Sum(a => a.value) + Completed_tradeshow.Sum(a => a.value) + Completed_spif.Sum(a => a.value);
            double? Credit = Credit_Training.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value) + Credit_merch.Sum(a => a.value) + Credit_dp.Sum(a => a.value) + Credit_op.Sum(a => a.value) + Credit_ras.Sum(a => a.value) + Credit_tradeshow.Sum(a => a.value) + Credit_spif.Sum(a => a.value);

            double? TotalAvailableWithPending = TotalAvailable - Pending;
            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                SpentTraining = spentTraining,
                SpentDP = spentDisplayProducts,
                SpentMerchandise = spentMerchandise,
                SpentOP = spentOtherActivity,
                SpentPromoAc = spentPromotionalActivity,
                SpentPromoEv = spentPromotionalEvent,
                SpentRAS = spentRAS,
                SpentTradeshow = spentTradeshow,
                SpentSPIF = spentSPIF,
                Pending_Training_total = Pending_Training.Sum(a => a.value),
                Approved_Training_total = Approved_Training.Sum(a => a.value),
                Completed_Training_total = Completed_Training.Sum(a => a.value),
                Pending_PromoEv_total = Pending_PromoEv.Sum(a => a.value),
                Approved_PromoEv_total = Approved_PromoEv.Sum(a => a.value),
                Completed_PromoEv_total = Completed_PromoEv.Sum(a => a.value),
                Pending_PromoAc_total = Pending_PromoAc.Sum(a => a.value),
                Approved_PromoAc_total = Approved_PromoAc.Sum(a => a.value),
                Completed_PromoAc_total = Completed_PromoAc.Sum(a => a.value),
                Pending_Merchandise_total = Pending_merch.Sum(a => a.value),
                Approved_Merchandise_total = Approved_merch.Sum(a => a.value),
                Completed_Merchandise_total = Completed_merch.Sum(a => a.value),
                Pending_DP_total = Pending_dp.Sum(a => a.value),
                Approved_DP_total = Approved_dp.Sum(a => a.value),
                Completed_DP_total = Completed_dp.Sum(a => a.value),
                Pending_OP_total = Pending_op.Sum(a => a.value),
                Approved_OP_total = Approved_op.Sum(a => a.value),
                Completed_OP_total = Completed_op.Sum(a => a.value),
                Credit_PromoAc_total = Credit_PromoAc.Sum(a=>a.value),               
                Credit_OP_total = Credit_op.Sum(a=>a.value),
                Credit_Training_total = Credit_Training.Sum(a=>a.value),
                Credit_PromoEv_total = Credit_PromoEv.Sum(a=>a.value),
                Credit_DP_total = Credit_dp.Sum(a=>a.value),
                Credit_Merchandise_total=Credit_merch.Sum(a=>a.value),
                Pending_RAS_total = Pending_ras.Sum(a=>a.value),
                Approved_RAS_total = Approved_ras.Sum(a=>a.value),
                Completed_RAS_total = Completed_ras.Sum(a=>a.value),
                Credit_RAS_total = Credit_ras.Sum(a=>a.value),
                Pending_Tradeshow_total = Pending_tradeshow.Sum(a => a.value),
                Approved_Tradeshow_total = Approved_tradeshow.Sum(a => a.value),
                Completed_Tradeshow_total = Completed_tradeshow.Sum(a => a.value),
                Credit_Tradeshow_total = Credit_tradeshow.Sum(a => a.value),
                Pending_SPIF_total = Pending_spif.Sum(a => a.value),
                Approved_SPIF_total = Approved_spif.Sum(a => a.value),
                Completed_SPIF_total = Completed_spif.Sum(a => a.value),
                Credit_SPIF_total = Credit_spif.Sum(a => a.value),
                totalTrainingAva = TrainingAva,
                totalPromoeAva = PromoeAva,
                totalPromoaAva = PromoaAva,
                totalMerchAva = MerchAva,
                totalDpAva = DpAva,
                totalOtherAva = OtherAva,
                totalRASAva = rasAva,
                totalTradeshowAva = tradeAva,
                totalSPIFAva = spifAva,
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized,
                PercentageTrainingUsed = PctTrainingUsed,
                PercentageTraining = PctTraining,
                PercentageTrainingValid = PctTrainingValid,
                PercentagePromotionalEventUsed = PctPromotionalEventUsed,
                PercentagePromotionalEvent = PctPromotionalEvent,
                PercentagePromotionalValid = PctPromotEventValid,
                PercentagePromotionalActivityUsed = PctPromotionalActivityUsed,
                PercentagePromotionalActivity = PctPromotionalActivity,
                PercentagePromotionalActivityValid = PctPromoActivityValid,
                PercentageMerchandiseUsed = PctMerchandiseUsed,
                PercentageMerchandise = PctMerchandise,
                PercentageMerchandiseValid = PctMerchandiseValid,
                PercentageDisplayProductsUsed = PctDisplayProductsUsed,
                PercentageDisplayProducts = PctDisplayProducts,
                PercentageDisplayProductsValid = PctDisplayProdValid,
                PercentageOtherProductsUsed = PctOtherActivityUsed,
                PercentageOtherProducts = PctOtherActivity,
                PercentageOtherProductsValid = PctOtherActivityValid,
                PercentageRASUsed = PctRASUsed,
                PercentageRAS = PctRAS,
                PercentageRASValid = PctRasValid,
                PercentageTradeshowUsed = PctTradeUsed,
                PercentageTradeshow = PctTrade,
                PercentageTradeshowValid = PctTradeValid,
                PercentageSPIFUsed = PctSPIFUsed,
                PercentageSPIF = PctSPIF,
                PercentageSPIFValid = PctSPIFValid
            });

            return mdfs_parts;
        }

        public List<MdfParts> MDFsActivitiesArchive(IList<mdf_main> mdf_mainModel, partnerCompany_Archive partnerCompanyViewModel)
        {
            //List Training
            List<PendingTraining> Pending_Training = new List<PendingTraining>();
            List<ApprovedTraining> Approved_Training = new List<ApprovedTraining>();
            List<CompletedTraining> Completed_Training = new List<CompletedTraining>();
            List<CreditTraining> Credit_Training = new List<CreditTraining>();
            //List Promotional Events
            List<PendingPromEvents> Pending_PromoEv = new List<PendingPromEvents>();
            List<ApprovedPromEvents> Approved_PromoEv = new List<ApprovedPromEvents>();
            List<CompletedPromEvents> Completed_PromoEv = new List<CompletedPromEvents>();
            List<CreditPromEvents> Credit_PromoEv = new List<CreditPromEvents>();
            //List Promotional Activities
            List<PendingPromAcitivies> Pending_PromoAc = new List<PendingPromAcitivies>();
            List<ApprovedPromAcitivies> Approved_PromoAc = new List<ApprovedPromAcitivies>();
            List<CompletedPromAcitivies> Completed_PromoAc = new List<CompletedPromAcitivies>();
            List<CreditPromAcitivies> Credit_PromoAc = new List<CreditPromAcitivies>();
            //List Merchandise
            List<PendingMerchandise> Pending_merch = new List<PendingMerchandise>();
            List<ApprovedMerchandise> Approved_merch = new List<ApprovedMerchandise>();
            List<CompletedMerchandise> Completed_merch = new List<CompletedMerchandise>();
            List<CreditMerchandise> Credit_merch = new List<CreditMerchandise>();
            //List Display Product
            List<PendingDisplayProduct> Pending_dp = new List<PendingDisplayProduct>();
            List<ApprovedDisplayProduct> Approved_dp = new List<ApprovedDisplayProduct>();
            List<CompletedDisplayProduct> Completed_dp = new List<CompletedDisplayProduct>();
            List<CreditDisplayProduct> Credit_dp = new List<CreditDisplayProduct>();
            //List Other Product
            List<PendingOtherProduct> Pending_op = new List<PendingOtherProduct>();
            List<ApprovedOtherProduct> Approved_op = new List<ApprovedOtherProduct>();
            List<CompletedOtherProduct> Completed_op = new List<CompletedOtherProduct>();
            List<CreditOtherProduct> Credit_op = new List<CreditOtherProduct>();

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1 && item.mdf_type == 1)
                {
                    //Training column for pending
                    Pending_Training.Add(new PendingTraining { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 1)
                {
                    //Traing column for Approved 
                    Approved_Training.Add(new ApprovedTraining { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Completed_Training.Add(new CompletedTraining { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Credit_Training.Add(new CreditTraining { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 2)
                {
                    //Promotional Event column for pending
                    Pending_PromoEv.Add(new PendingPromEvents { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 2)
                {
                    //Promotional Event column for Approved 
                    Approved_PromoEv.Add(new ApprovedPromEvents { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Completed_PromoEv.Add(new CompletedPromEvents { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Credit_PromoEv.Add(new CreditPromEvents { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 3)
                {
                    //Promotional Activities column for pending
                    Pending_PromoAc.Add(new PendingPromAcitivies { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 3)
                {
                    //Promotional Activities column for Approved 
                    Approved_PromoAc.Add(new ApprovedPromAcitivies { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Completed_PromoAc.Add(new CompletedPromAcitivies { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Credit_PromoAc.Add(new CreditPromAcitivies { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 4)
                {
                    //Merchandise column for pending
                    Pending_merch.Add(new PendingMerchandise { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 4)
                {
                    //Merchandise column for Approved 
                    Approved_merch.Add(new ApprovedMerchandise { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Completed_merch.Add(new CompletedMerchandise { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Credit_merch.Add(new CreditMerchandise { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 5)
                {
                    //Display Product column for pending
                    Pending_dp.Add(new PendingDisplayProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 5)
                {
                    //Display Product column for Approved 
                    Approved_dp.Add(new ApprovedDisplayProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Completed_dp.Add(new CompletedDisplayProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Credit_dp.Add(new CreditDisplayProduct { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 6)
                {
                    //Other Products column for pending
                    Pending_op.Add(new PendingOtherProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 6)
                {
                    //Other Products column for Approved 
                    Approved_op.Add(new ApprovedOtherProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Completed_op.Add(new CompletedOtherProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Credit_op.Add(new CreditOtherProduct { value = item.mdf_validatedAmt });
                }
            }

            //Calculate Training Data
            double? spentTraining = Approved_Training.Sum(a => a.value) + Completed_Training.Sum(a => a.value) + Credit_Training.Sum(a => a.value);
            double? TrainingAva = partnerCompanyViewModel.comp_MDF_tLimit - spentTraining;
            int PctTrainingUsed = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_amount);
            int PctTraining = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_tLimit);
            double PctTrainingValid = getPercentage(Credit_Training.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_tLimit);

            //Calculate Promotional Events Data
            double? spentPromotionalEvent = Approved_PromoEv.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value);
            double? PromoeAva = partnerCompanyViewModel.comp_MDF_eLimit - spentPromotionalEvent;
            int PctPromotionalEventUsed = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalEvent = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_eLimit);
            double PctPromotEventValid = getPercentage(Credit_PromoEv.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_eLimit);

            //Calculate Promotional Activities Data
            double? spentPromotionalActivity = Approved_PromoAc.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value);
            double? PromoaAva = partnerCompanyViewModel.comp_MDF_aLimit - spentPromotionalActivity;
            int PctPromotionalActivityUsed = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalActivity = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_aLimit);
            double PctPromoActivityValid = getPercentage(Credit_PromoAc.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_aLimit);

            //Calculate Merchandise Data
            double? spentMerchandise = Approved_merch.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Credit_merch.Sum(a => a.value);
            double? MerchAva = partnerCompanyViewModel.comp_MDF_mLimit - spentMerchandise;
            int PctMerchandiseUsed = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_amount);
            int PctMerchandise = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_mLimit);
            double PctMerchandiseValid = getPercentage(Credit_merch.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_mLimit);

            //Calculate Display Products Data
            double? spentDisplayProducts = Approved_dp.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Credit_dp.Sum(a => a.value);
            double? DpAva = partnerCompanyViewModel.comp_MDF_dLimit - spentDisplayProducts;
            int PctDisplayProductsUsed = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_amount);
            int PctDisplayProducts = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_dLimit);
            double PctDisplayProdValid = getPercentage(Credit_dp.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_dLimit);

            //Calculate Other Products Data
            double? spentOtherActivity = Approved_op.Sum(a => a.value) + Completed_op.Sum(a => a.value) + Credit_op.Sum(a => a.value);
            double? OtherAva = partnerCompanyViewModel.comp_MDF_oLimit - spentOtherActivity;
            int PctOtherActivityUsed = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctOtherActivity = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_oLimit);
            double PctOtherActivityValid = getPercentage(Credit_op.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_oLimit);

            double? Pending = Pending_Training.Sum(a => a.value) + Pending_PromoEv.Sum(a => a.value) + Pending_PromoAc.Sum(a => a.value) + Pending_merch.Sum(a => a.value) + Pending_dp.Sum(a => a.value) + Pending_op.Sum(a => a.value);
            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = spentTraining + spentPromotionalEvent + spentPromotionalActivity + spentMerchandise + spentDisplayProducts + spentOtherActivity;
            double? TotalUsedWithPending = TotalUsed + Pending;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MDF_amount);
            double? TotalAvailable = partnerCompanyViewModel.comp_MDF_amount - TotalUsed;
            double? TotalAvailableWithPending = partnerCompanyViewModel.comp_MDF_amount - TotalUsedWithPending;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MDF_amount);

            //Calculating total Pending, Approved, Complete
            
            double? Approved = Approved_Training.Sum(a => a.value) + Approved_PromoEv.Sum(a => a.value) + Approved_PromoAc.Sum(a => a.value) + Approved_merch.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_op.Sum(a => a.value);
            double? Complete = Completed_Training.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Completed_op.Sum(a => a.value);
            double? Credit = Credit_Training.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value) + Credit_merch.Sum(a => a.value) + Credit_dp.Sum(a => a.value) + Credit_op.Sum(a => a.value);

            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                SpentTraining = spentTraining,
                SpentDP = spentDisplayProducts,
                SpentMerchandise = spentMerchandise,
                SpentOP = spentOtherActivity,
                SpentPromoAc = spentPromotionalActivity,
                SpentPromoEv = spentPromotionalEvent,
                Pending_Training_total = Pending_Training.Sum(a => a.value),
                Approved_Training_total = Approved_Training.Sum(a => a.value),
                Completed_Training_total = Completed_Training.Sum(a => a.value),
                Pending_PromoEv_total = Pending_PromoEv.Sum(a => a.value),
                Approved_PromoEv_total = Approved_PromoEv.Sum(a => a.value),
                Completed_PromoEv_total = Completed_PromoEv.Sum(a => a.value),
                Pending_PromoAc_total = Pending_PromoAc.Sum(a => a.value),
                Approved_PromoAc_total = Approved_PromoAc.Sum(a => a.value),
                Completed_PromoAc_total = Completed_PromoAc.Sum(a => a.value),
                Pending_Merchandise_total = Pending_merch.Sum(a => a.value),
                Approved_Merchandise_total = Approved_merch.Sum(a => a.value),
                Completed_Merchandise_total = Completed_merch.Sum(a => a.value),
                Pending_DP_total = Pending_dp.Sum(a => a.value),
                Approved_DP_total = Approved_dp.Sum(a => a.value),
                Completed_DP_total = Completed_dp.Sum(a => a.value),
                Pending_OP_total = Pending_op.Sum(a => a.value),
                Approved_OP_total = Approved_op.Sum(a => a.value),
                Completed_OP_total = Completed_op.Sum(a => a.value),
                Credit_PromoAc_total = Credit_PromoAc.Sum(a => a.value),
                Credit_OP_total = Credit_op.Sum(a => a.value),
                Credit_Training_total = Credit_Training.Sum(a => a.value),
                Credit_PromoEv_total = Credit_PromoEv.Sum(a => a.value),
                Credit_DP_total = Credit_dp.Sum(a => a.value),
                Credit_Merchandise_total = Credit_merch.Sum(a => a.value),
                totalTrainingAva = TrainingAva,
                totalPromoeAva = PromoeAva,
                totalPromoaAva = PromoaAva,
                totalMerchAva = MerchAva,
                totalDpAva = DpAva,
                totalOtherAva = OtherAva,
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized,
                PercentageTrainingUsed = PctTrainingUsed,
                PercentageTraining = PctTraining,
                PercentageTrainingValid = PctTrainingValid,
                PercentagePromotionalEventUsed = PctPromotionalEventUsed,
                PercentagePromotionalEvent = PctPromotionalEvent,
                PercentagePromotionalValid = PctPromotEventValid,
                PercentagePromotionalActivityUsed = PctPromotionalActivityUsed,
                PercentagePromotionalActivity = PctPromotionalActivity,
                PercentagePromotionalActivityValid = PctPromoActivityValid,
                PercentageMerchandiseUsed = PctMerchandiseUsed,
                PercentageMerchandise = PctMerchandise,
                PercentageMerchandiseValid = PctMerchandiseValid,
                PercentageDisplayProductsUsed = PctDisplayProductsUsed,
                PercentageDisplayProducts = PctDisplayProducts,
                PercentageDisplayProductsValid = PctDisplayProdValid,
                PercentageOtherProductsUsed = PctOtherActivityUsed,
                PercentageOtherProducts = PctOtherActivity,
                PercentageOtherProductsValid = PctOtherActivityValid,
            });

            return mdfs_parts;
        }

        public List<MdfParts> MKTsActivities(List<mdf_main> mdf_mainModel, partnerCompanyViewModel partnerCompanyViewModel)
        {
            double Pending = 0;
            double Approved = 0;
            double Complete = 0;
            double Credit = 0;

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1)
                {
                    Pending += (item.mdf_mdfCost.HasValue ? item.mdf_mdfCost.Value : 0);
                }
                else if (item.mdf_status == 3)
                {
                    Approved += (item.mdf_approvedAmt_mkt.HasValue ? item.mdf_approvedAmt_mkt.Value : 0);
                }
                else if (item.mdf_status == 4)
                {
                    Complete += (item.mdf_validatedAmt_mkt.HasValue ? item.mdf_validatedAmt_mkt.Value : 0);
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum))
                {
                    Credit += (item.mdf_validatedAmt_mkt.HasValue ? item.mdf_validatedAmt_mkt.Value : 0);
                }
            }

            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = Approved + Complete + Credit;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MKT_Limit);
            double? TotalAvailable = partnerCompanyViewModel.comp_MKT_Limit - TotalUsed;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MKT_Limit);
            double? TotalAvailableWithPending = TotalAvailable - Pending;
            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized
            });

            return mdfs_parts;
        }

        public List<MdfParts> MKTsActivitiesArchive(IList<mdf_main> mdf_mainModel, partnerCompany_Archive partnerCompanyViewModel)
        {
            double Pending = 0;
            double Approved = 0;
            double Complete = 0;
            double Credit = 0;

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1)
                {
                    Pending += (item.mdf_mdfCost.HasValue ? item.mdf_mdfCost.Value : 0);
                }
                else if (item.mdf_status == 3)
                {
                    Approved += (item.mdf_approvedAmt_mkt.HasValue ? item.mdf_approvedAmt_mkt.Value : 0);
                }
                else if (item.mdf_status == 4)
                {
                    Complete += (item.mdf_validatedAmt_mkt.HasValue ? item.mdf_validatedAmt_mkt.Value : 0);
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum))
                {
                    Credit += (item.mdf_validatedAmt_mkt.HasValue ? item.mdf_validatedAmt_mkt.Value : 0);
                }
            }

            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = Pending + Approved + Complete + Credit;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MKT_Limit);
            double? TotalAvailable = partnerCompanyViewModel.comp_MKT_Limit - TotalUsed;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MKT_Limit);
            double? TotalAvailableWithPending = TotalAvailable - Pending;

            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized
            });

            return mdfs_parts;
        }

        public List<MdfParts> MDFsActivities(List<mdf_pinnacle_main> mdf_mainModel, partnerCompanyViewModel partnerCompanyViewModel)
        {
            //List Training
            List<PendingTraining> Pending_Training = new List<PendingTraining>();
            List<ApprovedTraining> Approved_Training = new List<ApprovedTraining>();
            List<CompletedTraining> Completed_Training = new List<CompletedTraining>();
            List<CreditTraining> Credit_Training = new List<CreditTraining>();
            //List Promotional Events
            List<PendingPromEvents> Pending_PromoEv = new List<PendingPromEvents>();
            List<ApprovedPromEvents> Approved_PromoEv = new List<ApprovedPromEvents>();
            List<CompletedPromEvents> Completed_PromoEv = new List<CompletedPromEvents>();
            List<CreditPromEvents> Credit_PromoEv = new List<CreditPromEvents>();
            //List Promotional Activities
            List<PendingPromAcitivies> Pending_PromoAc = new List<PendingPromAcitivies>();
            List<ApprovedPromAcitivies> Approved_PromoAc = new List<ApprovedPromAcitivies>();
            List<CompletedPromAcitivies> Completed_PromoAc = new List<CompletedPromAcitivies>();
            List<CreditPromAcitivies> Credit_PromoAc = new List<CreditPromAcitivies>();
            //List Merchandise
            List<PendingMerchandise> Pending_merch = new List<PendingMerchandise>();
            List<ApprovedMerchandise> Approved_merch = new List<ApprovedMerchandise>();
            List<CompletedMerchandise> Completed_merch = new List<CompletedMerchandise>();
            List<CreditMerchandise> Credit_merch = new List<CreditMerchandise>();
            //List Display Product
            List<PendingDisplayProduct> Pending_dp = new List<PendingDisplayProduct>();
            List<ApprovedDisplayProduct> Approved_dp = new List<ApprovedDisplayProduct>();
            List<CompletedDisplayProduct> Completed_dp = new List<CompletedDisplayProduct>();
            List<CreditDisplayProduct> Credit_dp = new List<CreditDisplayProduct>();
            //List Other Product
            List<PendingOtherProduct> Pending_op = new List<PendingOtherProduct>();
            List<ApprovedOtherProduct> Approved_op = new List<ApprovedOtherProduct>();
            List<CompletedOtherProduct> Completed_op = new List<CompletedOtherProduct>();
            List<CreditOtherProduct> Credit_op = new List<CreditOtherProduct>();

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1 && item.mdf_type == 1)
                {
                    //Training column for pending
                    Pending_Training.Add(new PendingTraining { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 1)
                {
                    //Traing column for Approved 
                    Approved_Training.Add(new ApprovedTraining { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Completed_Training.Add(new CompletedTraining { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Credit_Training.Add(new CreditTraining { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 2)
                {
                    //Promotional Event column for pending
                    Pending_PromoEv.Add(new PendingPromEvents { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 2)
                {
                    //Promotional Event column for Approved 
                    Approved_PromoEv.Add(new ApprovedPromEvents { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Completed_PromoEv.Add(new CompletedPromEvents { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Credit_PromoEv.Add(new CreditPromEvents { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 3)
                {
                    //Promotional Activities column for pending
                    Pending_PromoAc.Add(new PendingPromAcitivies { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 3)
                {
                    //Promotional Activities column for Approved 
                    Approved_PromoAc.Add(new ApprovedPromAcitivies { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Completed_PromoAc.Add(new CompletedPromAcitivies { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Credit_PromoAc.Add(new CreditPromAcitivies { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 4)
                {
                    //Merchandise column for pending
                    Pending_merch.Add(new PendingMerchandise { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 4)
                {
                    //Merchandise column for Approved 
                    Approved_merch.Add(new ApprovedMerchandise { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Completed_merch.Add(new CompletedMerchandise { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Credit_merch.Add(new CreditMerchandise { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 5)
                {
                    //Display Product column for pending
                    Pending_dp.Add(new PendingDisplayProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 5)
                {
                    //Display Product column for Approved 
                    Approved_dp.Add(new ApprovedDisplayProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Completed_dp.Add(new CompletedDisplayProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Credit_dp.Add(new CreditDisplayProduct { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 6)
                {
                    //Other Products column for pending
                    Pending_op.Add(new PendingOtherProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 6)
                {
                    //Other Products column for Approved 
                    Approved_op.Add(new ApprovedOtherProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Completed_op.Add(new CompletedOtherProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Credit_op.Add(new CreditOtherProduct { value = item.mdf_validatedAmt });
                }
            }

            //Calculate Training Data
            double? spentTraining = Approved_Training.Sum(a => a.value) + Completed_Training.Sum(a => a.value) + Credit_Training.Sum(a => a.value);
            double? TrainingAva = partnerCompanyViewModel.comp_MDF_tLimit - spentTraining;
            int PctTrainingUsed = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_amount);
            int PctTraining = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_tLimit);
            double PctTrainingValid = getPercentage(Credit_Training.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_tLimit);

            //Calculate Promotional Events Data
            double? spentPromotionalEvent = Approved_PromoEv.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value);
            double? PromoeAva = partnerCompanyViewModel.comp_MDF_eLimit - spentPromotionalEvent;
            int PctPromotionalEventUsed = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalEvent = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_eLimit);
            double PctPromotEventValid = getPercentage(Credit_PromoEv.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_eLimit);

            //Calculate Promotional Activities Data
            double? spentPromotionalActivity = Approved_PromoAc.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value);
            double? PromoaAva = partnerCompanyViewModel.comp_MDF_aLimit - spentPromotionalActivity;
            int PctPromotionalActivityUsed = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalActivity = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_aLimit);
            double PctPromoActivityValid = getPercentage(Credit_PromoAc.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_aLimit);

            //Calculate Merchandise Data
            double? spentMerchandise = Approved_merch.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Credit_merch.Sum(a => a.value);
            double? MerchAva = partnerCompanyViewModel.comp_MDF_mLimit - spentMerchandise;
            int PctMerchandiseUsed = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_amount);
            int PctMerchandise = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_mLimit);
            double PctMerchandiseValid = getPercentage(Credit_merch.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_mLimit);

            //Calculate Display Products Data
            double? spentDisplayProducts = Approved_dp.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Credit_dp.Sum(a => a.value);
            double? DpAva = partnerCompanyViewModel.comp_MDF_dLimit - spentDisplayProducts;
            int PctDisplayProductsUsed = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_amount);
            int PctDisplayProducts = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_dLimit);
            double PctDisplayProdValid = getPercentage(Credit_dp.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_dLimit);

            //Calculate Other Products Data
            double? spentOtherActivity = Approved_op.Sum(a => a.value) + Completed_op.Sum(a => a.value) + Credit_op.Sum(a => a.value);
            double? OtherAva = partnerCompanyViewModel.comp_MDF_oLimit - spentOtherActivity;
            int PctOtherActivityUsed = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctOtherActivity = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_oLimit);
            double PctOtherActivityValid = getPercentage(Credit_op.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_oLimit);

            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = spentTraining + spentPromotionalEvent + spentPromotionalActivity + spentMerchandise + spentDisplayProducts + spentOtherActivity;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MDF_amount);
            double? TotalAvailable = partnerCompanyViewModel.comp_MDF_amount - TotalUsed;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MDF_amount);

            //Calculating total Pending, Approved, Complete
            double? Pending = Pending_Training.Sum(a => a.value) + Pending_PromoEv.Sum(a => a.value) + Pending_PromoAc.Sum(a => a.value) + Pending_merch.Sum(a => a.value) + Pending_dp.Sum(a => a.value) + Pending_op.Sum(a => a.value);
            double? Approved = Approved_Training.Sum(a => a.value) + Approved_PromoEv.Sum(a => a.value) + Approved_PromoAc.Sum(a => a.value) + Approved_merch.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_op.Sum(a => a.value);
            double? Complete = Completed_Training.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Completed_op.Sum(a => a.value);
            double? Credit = Credit_Training.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value) + Credit_merch.Sum(a => a.value) + Credit_dp.Sum(a => a.value) + Credit_op.Sum(a => a.value);

            double? TotalAvailableWithPending = TotalAvailable - Pending;
            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                SpentTraining = spentTraining,
                SpentDP = spentDisplayProducts,
                SpentMerchandise = spentMerchandise,
                SpentOP = spentOtherActivity,
                SpentPromoAc = spentPromotionalActivity,
                SpentPromoEv = spentPromotionalEvent,
                Pending_Training_total = Pending_Training.Sum(a => a.value),
                Approved_Training_total = Approved_Training.Sum(a => a.value),
                Completed_Training_total = Completed_Training.Sum(a => a.value),
                Pending_PromoEv_total = Pending_PromoEv.Sum(a => a.value),
                Approved_PromoEv_total = Approved_PromoEv.Sum(a => a.value),
                Completed_PromoEv_total = Completed_PromoEv.Sum(a => a.value),
                Pending_PromoAc_total = Pending_PromoAc.Sum(a => a.value),
                Approved_PromoAc_total = Approved_PromoAc.Sum(a => a.value),
                Completed_PromoAc_total = Completed_PromoAc.Sum(a => a.value),
                Pending_Merchandise_total = Pending_merch.Sum(a => a.value),
                Approved_Merchandise_total = Approved_merch.Sum(a => a.value),
                Completed_Merchandise_total = Completed_merch.Sum(a => a.value),
                Pending_DP_total = Pending_dp.Sum(a => a.value),
                Approved_DP_total = Approved_dp.Sum(a => a.value),
                Completed_DP_total = Completed_dp.Sum(a => a.value),
                Pending_OP_total = Pending_op.Sum(a => a.value),
                Approved_OP_total = Approved_op.Sum(a => a.value),
                Completed_OP_total = Completed_op.Sum(a => a.value),
                Credit_PromoAc_total = Credit_PromoAc.Sum(a => a.value),
                Credit_OP_total = Credit_op.Sum(a => a.value),
                Credit_Training_total = Credit_Training.Sum(a => a.value),
                Credit_PromoEv_total = Credit_PromoEv.Sum(a => a.value),
                Credit_DP_total = Credit_dp.Sum(a => a.value),
                Credit_Merchandise_total = Credit_merch.Sum(a => a.value),
                totalTrainingAva = TrainingAva,
                totalPromoeAva = PromoeAva,
                totalPromoaAva = PromoaAva,
                totalMerchAva = MerchAva,
                totalDpAva = DpAva,
                totalOtherAva = OtherAva,
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized,
                PercentageTrainingUsed = PctTrainingUsed,
                PercentageTraining = PctTraining,
                PercentageTrainingValid = PctTrainingValid,
                PercentagePromotionalEventUsed = PctPromotionalEventUsed,
                PercentagePromotionalEvent = PctPromotionalEvent,
                PercentagePromotionalValid = PctPromotEventValid,
                PercentagePromotionalActivityUsed = PctPromotionalActivityUsed,
                PercentagePromotionalActivity = PctPromotionalActivity,
                PercentagePromotionalActivityValid = PctPromoActivityValid,
                PercentageMerchandiseUsed = PctMerchandiseUsed,
                PercentageMerchandise = PctMerchandise,
                PercentageMerchandiseValid = PctMerchandiseValid,
                PercentageDisplayProductsUsed = PctDisplayProductsUsed,
                PercentageDisplayProducts = PctDisplayProducts,
                PercentageDisplayProductsValid = PctDisplayProdValid,
                PercentageOtherProductsUsed = PctOtherActivityUsed,
                PercentageOtherProducts = PctOtherActivity,
                PercentageOtherProductsValid = PctOtherActivityValid,
            });

            return mdfs_parts;
        }

        public List<MdfParts> MDFsActivitiesArchive(IList<mdf_pinnacle_main> mdf_mainModel, partnerCompany_Archive partnerCompanyViewModel)
        {
            //List Training
            List<PendingTraining> Pending_Training = new List<PendingTraining>();
            List<ApprovedTraining> Approved_Training = new List<ApprovedTraining>();
            List<CompletedTraining> Completed_Training = new List<CompletedTraining>();
            List<CreditTraining> Credit_Training = new List<CreditTraining>();
            //List Promotional Events
            List<PendingPromEvents> Pending_PromoEv = new List<PendingPromEvents>();
            List<ApprovedPromEvents> Approved_PromoEv = new List<ApprovedPromEvents>();
            List<CompletedPromEvents> Completed_PromoEv = new List<CompletedPromEvents>();
            List<CreditPromEvents> Credit_PromoEv = new List<CreditPromEvents>();
            //List Promotional Activities
            List<PendingPromAcitivies> Pending_PromoAc = new List<PendingPromAcitivies>();
            List<ApprovedPromAcitivies> Approved_PromoAc = new List<ApprovedPromAcitivies>();
            List<CompletedPromAcitivies> Completed_PromoAc = new List<CompletedPromAcitivies>();
            List<CreditPromAcitivies> Credit_PromoAc = new List<CreditPromAcitivies>();
            //List Merchandise
            List<PendingMerchandise> Pending_merch = new List<PendingMerchandise>();
            List<ApprovedMerchandise> Approved_merch = new List<ApprovedMerchandise>();
            List<CompletedMerchandise> Completed_merch = new List<CompletedMerchandise>();
            List<CreditMerchandise> Credit_merch = new List<CreditMerchandise>();
            //List Display Product
            List<PendingDisplayProduct> Pending_dp = new List<PendingDisplayProduct>();
            List<ApprovedDisplayProduct> Approved_dp = new List<ApprovedDisplayProduct>();
            List<CompletedDisplayProduct> Completed_dp = new List<CompletedDisplayProduct>();
            List<CreditDisplayProduct> Credit_dp = new List<CreditDisplayProduct>();
            //List Other Product
            List<PendingOtherProduct> Pending_op = new List<PendingOtherProduct>();
            List<ApprovedOtherProduct> Approved_op = new List<ApprovedOtherProduct>();
            List<CompletedOtherProduct> Completed_op = new List<CompletedOtherProduct>();
            List<CreditOtherProduct> Credit_op = new List<CreditOtherProduct>();

            foreach (var item in mdf_mainModel)
            {
                if (item.mdf_status == 1 && item.mdf_type == 1)
                {
                    //Training column for pending
                    Pending_Training.Add(new PendingTraining { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 1)
                {
                    //Traing column for Approved 
                    Approved_Training.Add(new ApprovedTraining { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Completed_Training.Add(new CompletedTraining { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 1)
                {
                    //Traing column for Completed
                    Credit_Training.Add(new CreditTraining { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 2)
                {
                    //Promotional Event column for pending
                    Pending_PromoEv.Add(new PendingPromEvents { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 2)
                {
                    //Promotional Event column for Approved 
                    Approved_PromoEv.Add(new ApprovedPromEvents { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Completed_PromoEv.Add(new CompletedPromEvents { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 2)
                {
                    //Promotional Event column for completed
                    Credit_PromoEv.Add(new CreditPromEvents { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 3)
                {
                    //Promotional Activities column for pending
                    Pending_PromoAc.Add(new PendingPromAcitivies { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 3)
                {
                    //Promotional Activities column for Approved 
                    Approved_PromoAc.Add(new ApprovedPromAcitivies { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Completed_PromoAc.Add(new CompletedPromAcitivies { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 3)
                {
                    //Promotional Activities column for completed
                    Credit_PromoAc.Add(new CreditPromAcitivies { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 4)
                {
                    //Merchandise column for pending
                    Pending_merch.Add(new PendingMerchandise { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 4)
                {
                    //Merchandise column for Approved 
                    Approved_merch.Add(new ApprovedMerchandise { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Completed_merch.Add(new CompletedMerchandise { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 4)
                {
                    //Merchandise column for completed
                    Credit_merch.Add(new CreditMerchandise { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 5)
                {
                    //Display Product column for pending
                    Pending_dp.Add(new PendingDisplayProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 5)
                {
                    //Display Product column for Approved 
                    Approved_dp.Add(new ApprovedDisplayProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Completed_dp.Add(new CompletedDisplayProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 5)
                {
                    //Display Product column for completed
                    Credit_dp.Add(new CreditDisplayProduct { value = item.mdf_validatedAmt });
                }

                if (item.mdf_status == 1 && item.mdf_type == 6)
                {
                    //Other Products column for pending
                    Pending_op.Add(new PendingOtherProduct { value = item.mdf_mdfCost });
                }
                else if (item.mdf_status == 3 && item.mdf_type == 6)
                {
                    //Other Products column for Approved 
                    Approved_op.Add(new ApprovedOtherProduct { value = item.mdf_approvedAmt });
                }
                else if (item.mdf_status == 4 && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Completed_op.Add(new CompletedOtherProduct { value = item.mdf_validatedAmt });
                }
                else if (item.mdf_status == 5 && !string.IsNullOrEmpty(item.mdf_creditMemoNum) && item.mdf_type == 6)
                {
                    //Display Products column for completed
                    Credit_op.Add(new CreditOtherProduct { value = item.mdf_validatedAmt });
                }
            }

            //Calculate Training Data
            double? spentTraining = Approved_Training.Sum(a => a.value) + Completed_Training.Sum(a => a.value) + Credit_Training.Sum(a => a.value);
            double? TrainingAva = partnerCompanyViewModel.comp_MDF_tLimit - spentTraining;
            int PctTrainingUsed = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_amount);
            int PctTraining = getPercentage(spentTraining, partnerCompanyViewModel.comp_MDF_tLimit);
            double PctTrainingValid = getPercentage(Credit_Training.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_tLimit);

            //Calculate Promotional Events Data
            double? spentPromotionalEvent = Approved_PromoEv.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value);
            double? PromoeAva = partnerCompanyViewModel.comp_MDF_eLimit - spentPromotionalEvent;
            int PctPromotionalEventUsed = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalEvent = getPercentage(spentPromotionalEvent, partnerCompanyViewModel.comp_MDF_eLimit);
            double PctPromotEventValid = getPercentage(Credit_PromoEv.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_eLimit);

            //Calculate Promotional Activities Data
            double? spentPromotionalActivity = Approved_PromoAc.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value);
            double? PromoaAva = partnerCompanyViewModel.comp_MDF_aLimit - spentPromotionalActivity;
            int PctPromotionalActivityUsed = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctPromotionalActivity = getPercentage(spentPromotionalActivity, partnerCompanyViewModel.comp_MDF_aLimit);
            double PctPromoActivityValid = getPercentage(Credit_PromoAc.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_aLimit);

            //Calculate Merchandise Data
            double? spentMerchandise = Approved_merch.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Credit_merch.Sum(a => a.value);
            double? MerchAva = partnerCompanyViewModel.comp_MDF_mLimit - spentMerchandise;
            int PctMerchandiseUsed = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_amount);
            int PctMerchandise = getPercentage(spentMerchandise, partnerCompanyViewModel.comp_MDF_mLimit);
            double PctMerchandiseValid = getPercentage(Credit_merch.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_mLimit);

            //Calculate Display Products Data
            double? spentDisplayProducts = Approved_dp.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Credit_dp.Sum(a => a.value);
            double? DpAva = partnerCompanyViewModel.comp_MDF_dLimit - spentDisplayProducts;
            int PctDisplayProductsUsed = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_amount);
            int PctDisplayProducts = getPercentage(spentDisplayProducts, partnerCompanyViewModel.comp_MDF_dLimit);
            double PctDisplayProdValid = getPercentage(Credit_dp.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_dLimit);

            //Calculate Other Products Data
            double? spentOtherActivity = Approved_op.Sum(a => a.value) + Completed_op.Sum(a => a.value) + Credit_op.Sum(a => a.value);
            double? OtherAva = partnerCompanyViewModel.comp_MDF_oLimit - spentOtherActivity;
            int PctOtherActivityUsed = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_amount);
            int PctOtherActivity = getPercentage(spentOtherActivity, partnerCompanyViewModel.comp_MDF_oLimit);
            double PctOtherActivityValid = getPercentage(Credit_op.Sum(a => a.value), partnerCompanyViewModel.comp_MDF_oLimit);

            //Total Utilized MDFs And Remaining MDFs
            double? TotalUsed = spentTraining + spentPromotionalEvent + spentPromotionalActivity + spentMerchandise + spentDisplayProducts + spentOtherActivity;
            int PctUtilized = getPercentage(TotalUsed, partnerCompanyViewModel.comp_MDF_amount);
            double? TotalAvailable = partnerCompanyViewModel.comp_MDF_amount - TotalUsed;
            int PctAvailable = getPercentage(TotalAvailable, partnerCompanyViewModel.comp_MDF_amount);

            //Calculating total Pending, Approved, Complete
            double? Pending = Pending_Training.Sum(a => a.value) + Pending_PromoEv.Sum(a => a.value) + Pending_PromoAc.Sum(a => a.value) + Pending_merch.Sum(a => a.value) + Pending_dp.Sum(a => a.value) + Pending_op.Sum(a => a.value);
            double? Approved = Approved_Training.Sum(a => a.value) + Approved_PromoEv.Sum(a => a.value) + Approved_PromoAc.Sum(a => a.value) + Approved_merch.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_dp.Sum(a => a.value) + Approved_op.Sum(a => a.value);
            double? Complete = Completed_Training.Sum(a => a.value) + Completed_PromoEv.Sum(a => a.value) + Completed_PromoAc.Sum(a => a.value) + Completed_merch.Sum(a => a.value) + Completed_dp.Sum(a => a.value) + Completed_op.Sum(a => a.value);
            double? Credit = Credit_Training.Sum(a => a.value) + Credit_PromoEv.Sum(a => a.value) + Credit_PromoAc.Sum(a => a.value) + Credit_merch.Sum(a => a.value) + Credit_dp.Sum(a => a.value) + Credit_op.Sum(a => a.value);

            double? TotalAvailableWithPending = TotalAvailable - Pending;
            //Add the values to the Object
            List<MdfParts> mdfs_parts = new List<MdfParts>();
            mdfs_parts.Add(new MdfParts
            {
                SpentTraining = spentTraining,
                SpentDP = spentDisplayProducts,
                SpentMerchandise = spentMerchandise,
                SpentOP = spentOtherActivity,
                SpentPromoAc = spentPromotionalActivity,
                SpentPromoEv = spentPromotionalEvent,
                Pending_Training_total = Pending_Training.Sum(a => a.value),
                Approved_Training_total = Approved_Training.Sum(a => a.value),
                Completed_Training_total = Completed_Training.Sum(a => a.value),
                Pending_PromoEv_total = Pending_PromoEv.Sum(a => a.value),
                Approved_PromoEv_total = Approved_PromoEv.Sum(a => a.value),
                Completed_PromoEv_total = Completed_PromoEv.Sum(a => a.value),
                Pending_PromoAc_total = Pending_PromoAc.Sum(a => a.value),
                Approved_PromoAc_total = Approved_PromoAc.Sum(a => a.value),
                Completed_PromoAc_total = Completed_PromoAc.Sum(a => a.value),
                Pending_Merchandise_total = Pending_merch.Sum(a => a.value),
                Approved_Merchandise_total = Approved_merch.Sum(a => a.value),
                Completed_Merchandise_total = Completed_merch.Sum(a => a.value),
                Pending_DP_total = Pending_dp.Sum(a => a.value),
                Approved_DP_total = Approved_dp.Sum(a => a.value),
                Completed_DP_total = Completed_dp.Sum(a => a.value),
                Pending_OP_total = Pending_op.Sum(a => a.value),
                Approved_OP_total = Approved_op.Sum(a => a.value),
                Completed_OP_total = Completed_op.Sum(a => a.value),
                Credit_PromoAc_total = Credit_PromoAc.Sum(a => a.value),
                Credit_OP_total = Credit_op.Sum(a => a.value),
                Credit_Training_total = Credit_Training.Sum(a => a.value),
                Credit_PromoEv_total = Credit_PromoEv.Sum(a => a.value),
                Credit_DP_total = Credit_dp.Sum(a => a.value),
                Credit_Merchandise_total = Credit_merch.Sum(a => a.value),
                totalTrainingAva = TrainingAva,
                totalPromoeAva = PromoeAva,
                totalPromoaAva = PromoaAva,
                totalMerchAva = MerchAva,
                totalDpAva = DpAva,
                totalOtherAva = OtherAva,
                totalPending = Pending,
                totalApproved = Approved,
                totalComplete = Complete,
                totalCredit = Credit,
                totalMDFUtilized = TotalUsed,
                totalMDFAva = TotalAvailable,
                totalMDFAvaWithPending = TotalAvailableWithPending,
                PercentageTotalMDFAva = PctAvailable,
                PercentageUtilized = PctUtilized,
                PercentageTrainingUsed = PctTrainingUsed,
                PercentageTraining = PctTraining,
                PercentageTrainingValid = PctTrainingValid,
                PercentagePromotionalEventUsed = PctPromotionalEventUsed,
                PercentagePromotionalEvent = PctPromotionalEvent,
                PercentagePromotionalValid = PctPromotEventValid,
                PercentagePromotionalActivityUsed = PctPromotionalActivityUsed,
                PercentagePromotionalActivity = PctPromotionalActivity,
                PercentagePromotionalActivityValid = PctPromoActivityValid,
                PercentageMerchandiseUsed = PctMerchandiseUsed,
                PercentageMerchandise = PctMerchandise,
                PercentageMerchandiseValid = PctMerchandiseValid,
                PercentageDisplayProductsUsed = PctDisplayProductsUsed,
                PercentageDisplayProducts = PctDisplayProducts,
                PercentageDisplayProductsValid = PctDisplayProdValid,
                PercentageOtherProductsUsed = PctOtherActivityUsed,
                PercentageOtherProducts = PctOtherActivity,
                PercentageOtherProductsValid = PctOtherActivityValid,
            });

            return mdfs_parts;
        }

        public int getPercentage(double? difference = 0, double? total = 0)
        {
            int percentComplete = 0;
            if (total != null && total > 0)
            {
                percentComplete = (int)Math.Round((double)(difference / total) * 100);
            }
            return percentComplete;
        }
        #endregion

        #region File logger

        public void FileLog(string Message, string Type = "Errror")
        {
            StreamWriter log;
            string strLogText = Type +" ---\n" + Message;

            string timestamp = DateTime.Now.ToString("d-MMMM-yyyy", new CultureInfo("en-GB"));

            string error_folder = @"C:\public_html\RiSourceCenter\Logs";

            if (!System.IO.Directory.Exists(error_folder))
            {
                System.IO.Directory.CreateDirectory(error_folder);
            }
            if (System.IO.File.Exists(String.Format(@"{0}\Log_{1}.txt", error_folder, timestamp)))
            {
                log = System.IO.File.AppendText(String.Format(@"{0}\Log_{1}.txt", error_folder, timestamp));
            }
            else
            {
                log = new StreamWriter(String.Format(@"{0}\Log_{1}.txt", error_folder, timestamp));
            }
            // Write to the file:
            log.WriteLine(Environment.NewLine + DateTime.Now);
            log.WriteLine("------------------------------------------------------------------------------------------------");
            log.WriteLine(strLogText);
            log.WriteLine();
            log.Close();
        }

        #endregion

        #region Site Navigation Filters
        public List<nav2> SubmenFilter(string industry, string usrType, string products, string siteRole, int id)
        {
            List<nav2> nav2 = new List<nav2>();

            //Colect sub nav data
            List<nav2> nav2data = dbEntity.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == id && a.n2_usrTypes.Contains(usrType) && a.n2_industry != null && a.n2_products != null).OrderBy(a => a.n2order).ToList();
            //Filter sub nav data
            if (siteRole == "1")
            {
                //Super admin sees all of it
                foreach (var item in nav2data)
                {
                    nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                }
            }
            else
            {
                //None Super Admin filter by Industry
                foreach (var item in nav2data)
                {
                    //proccess industry filters for IT and Industrial
                    if (industry == "3")
                    {
                        //process products filters 
                        if (products != null)
                        {
                            char[] prod = products.ToArray();
                            string prods = new string(prod);
                            foreach (var num in prods)
                            {
                                if (item.n2_products.Contains(num))
                                {
                                    nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //process industry filter for IT or Industry and show links that are both
                        if (item.n2_industry == industry || item.n2_industry == "3")
                        {
                            //process products filter
                            if (products != null)
                            {
                                char[] prod = products.ToArray();
                                string prods = new string(prod);
                                foreach (var num in prods)
                                {
                                    if (item.n2_products.Contains(num.ToString()))
                                    {
                                        nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return nav2;
        }
        #endregion

        #region Strack Site Activities
        public async Task siteActionLog(int form_id, string feature_page, DateTime actionTime, string note, string action, int user_id)
        {
            SiteActionLogModel siteLog = new SiteActionLogModel
            {
                form_id = form_id,
                feature_page = feature_page,
                action_Time = actionTime,
                notes = note,
                action = action,
                usr_id = user_id
            };

            dbEntity.SiteActionLogModels.Add(siteLog);
            await dbEntity.SaveChangesAsync();
        }
        #endregion

        #region Render View To String
        public string RenderViewToString(ControllerContext context, string viewPath, object model = null, bool partial = false)
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

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
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
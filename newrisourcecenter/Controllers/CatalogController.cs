using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    [AllowAnonymous]
    public class CatalogController : Controller
    {
        // GET: Catalog
        public ActionResult Index(string pgn=null)
        {
            if (String.IsNullOrEmpty(pgn))
            {
                return View();
            }

            //path to source file
            string fileName = Path.Combine(Server.MapPath("~/attachments/digital_on_demand/PDFs/Catalog34/"), "livebook.pdf");

            //path to the output file
            string outputPdfPath = Server.MapPath("~/attachments/digital_on_demand/PDFs/") + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".pdf";

            //process the page that is passed in by the angularjs script
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            String[] pages2 = pgn.Split(delimiterChars);

            List<int> myCollection = new List<int>();
            myCollection.Add(1);

            for (int i = 0; i <= pages2.Length - 1; i++)
            {
                if (pages2[i].Contains("-") == true)
                {
                    string statend = pages2[i].ToString();
                    String[] startend = statend.Split('-');
                    string start = startend[0];
                    string end = startend[1];
                    int startpage = Convert.ToInt32(start);
                    int endpage = Convert.ToInt32(end);

                    //pagelength(startpage, endpage);
                    for (int p = startpage; p <= endpage; p++)
                    {
                        myCollection.Add(p);
                    }
                }
                else
                {
                    int a = Convert.ToInt32(pages2[i]);
                    if (a != 2)
                    {
                        if (a == 649)
                        {
                            myCollection.Add(648);
                        }
                        else
                        {
                            myCollection.Add(a);
                        }
                    }
                }
            }

            //int[] extractThesePages = { 1, 2 };
            ViewBag.p = myCollection.ToArray();
            int[] extractThesePages = myCollection.ToArray();
            //ViewBag.p = extractThesePages;
            //call the function that extracts the data
            ExtractPages(fileName, outputPdfPath, extractThesePages);

            return View();
        }

        [AllowAnonymous]
        public ActionResult Mailbody()
        {
            ViewBag.Message = "This is the emial ";

            return View();
        }

        [AllowAnonymous]
        public void ExtractPages(string sourcePdfPath, string outputPdfPath, int[] extractThesePages)
        {
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage = null;

            try
            {
                // Intialize a new PdfReader instance with the 
                // contents of the source Pdf file:
                reader = new PdfReader(sourcePdfPath);

                // For simplicity, I am assuming all the pages share the same size
                // and rotation as the first page:
                sourceDocument = new Document(PageSize.A4);

                // Initialize an instance of the PdfCopyClass with the source 
                // document and an output file stream:
                pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

                sourceDocument.Open();

                // Walk the array and add the page copies to the output file:
                foreach (int pageNumber in extractThesePages)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, pageNumber);
                    pdfCopyProvider.AddPage(importedPage);
                }

                //close the sourceDocument and the pdfReader
                sourceDocument.Close();
                reader.Close();

                //Print the content to file and then print the content from file to browser
                string mimeType = "application/pdf";

                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.ContentType = mimeType;
                Response.AddHeader("Content-Disposition", "inline");
                Response.WriteFile(outputPdfPath);
                Response.Flush();
                Response.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AllowAnonymous]
        public ActionResult email()
        {
            //get all the requested values and assign them to a string variable for the email method
            string From = Request["from_Email"];
            string To = Request["to_email"];
            string Name = Request["from_Name"];
            string Subject = "Rittal Digital on Demand - Custom Document from " + Request["project_Title"];
            string project = Request["project_Title"];
            string Company = Request["company_Name"];
            string Company_City = Request["company_City"];
            string Company_State = Request["company_State"];
            string Company_Zip = Request["company_Zip"];
            string Company_Phone = Request["company_Phone"];
            string pgn = Request["pgn"];
            string Comment = Request["Comments"];

            //instantiate the model ModelData in the ManageViewModels.cs
            var model = new ModelData();

            //set a value in the setter method in ModelData by passing in png to the view
            if (pgn != null)
            {
                model.pages = pgn;
                model.names = Name;
                model.company_names = Company;
                model.cities = Company_City;
                model.states = Company_State;
                model.zip = Company_Zip;
                model.phones = Company_Phone;
                model.comments = Comment;
                model.project_titles = project;
                model.emial_addresses = From;
            }

            //call the function that returns the view to string for the email body
            String messageView = RenderViewToString(ControllerContext, "~/Views/Catalog/Mailbody.cshtml", model, true);

            string Body = messageView;

            ViewBag.Message = MvcHtmlString.Create(Body);

            //try sending the email if the values are set
            if (!String.IsNullOrEmpty(From) || !String.IsNullOrEmpty(Body) || !String.IsNullOrEmpty(pgn) || Body != "")
            {
                try
                {
                    System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, Body);
                    Email.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);

                    SMPTobj.EnableSsl = false;
                    SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
                    SMPTobj.Send(Email);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex;
                }
            }
            else
            {

                ViewBag.notice = "Please go back and complete the forms";
            }

            return View();
        }

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

    }
}
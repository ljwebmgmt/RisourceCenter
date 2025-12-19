using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public partial class SRPViewController
    {
        #region Email Requestor
        public async Task EmailRequestor(IQueryable<salesRequestApprovers> regionalMan, SRPViewModel sRPViewModel, string Subject, string requesterEmailBody = null)
        {
            //get data
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            int department = Convert.ToInt32(sRPViewModel.department);
            int paymentMethod = Convert.ToInt32(sRPViewModel.paymentMethod);
            int requestType = Convert.ToInt32(sRPViewModel.requestType);
            //Text for ACH/Check
            var list_achType = FuncCommonSRP(regionalMan).achType;
            //Text for Cost Center
            var list_cccType = FuncCommonSRP(regionalMan).cccType;
            string ponumber = "";
            if (!string.IsNullOrEmpty(sRPViewModel.ponumber))
            {
                ponumber = "<br /><strong> PO Number </strong> : " + sRPViewModel.ponumber;
            }
            //set email properties
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }
            string showinConcur = "<br />";
            if (sRPViewModel.paymentMethod == "1")
            {
                showinConcur = "<strong> Supplier </strong> : " + sRPViewModel.supplier +
                "<br />" +
                "<strong> Supplier Number(SAP) </strong> : " + sRPViewModel.supplierNumber +
                "<br />" +
                "<strong> Ship To </strong> : " + sRPViewModel.shipTo +
                "<br />" +
                "<strong> Ship To Attn. </strong> : " + sRPViewModel.shiptoAttn +
                "<br />" +
                getInvoices(sRPViewModel.list_srp_invoice, host) +
                "<table cellpadding=\"5\" style=\"background: #5f5b5b;width:100%;border:solid 1px #000;border-collapse:collapse;\" border='1' >" +
                "<thead style=\"background: #efefef;\"><tr><th>Item</th><th>Part Number / Description</th><th>Quantity</th><th>Delivery Date</th><th>Unit Price</th><th>Total Price</th><th>Check of Accounts</th><th>Cost Center</th></tr><thead>" +
                getItemsList(sRPViewModel, list_achType, list_cccType) +
                " </table><br />";
            }
            string header = comm.emailheader(host);
            string footer = comm.emailfooter(host);
            var To = salesrep["email"];
            string From = "webmaster@risourcecenter.com";
            string Emailbody = "Dear " + salesrep["firstName"] + "," +
                    requesterEmailBody +
                    "<strong> Status </strong> : " + sRPViewModel.status +
                    "<br />" +
                    "<strong> REQ No.</strong> : " + sRPViewModel.FormID +
                    "<br />" +
                    "<strong> Created On </strong> : " + sRPViewModel.dateCreated +
                    "<br />" +
                    "<strong> Department </strong> : " + FuncCommonSRP(regionalMan).department[department].Text +
                    "<br />" +
                    "<strong> Request Type </strong> : " + FuncCommonSRP(regionalMan).requestType[requestType].Text +
                    "<br />" +
                    "<strong> Method of Payment </strong> : " + FuncCommonSRP(regionalMan).paymentMethod[paymentMethod].Text +
                    "<br />" +
                    "<strong> Title </strong> : " + sRPViewModel.title +
                    "<br />" +
                    "<strong> Description </strong> : " + sRPViewModel.description +
                    "<br />" +
                    "<strong> Estimated Cost </strong> : $" + sRPViewModel.estimatedCost +
                    "<br />" +
                    "<strong> Activity Date </strong> : " + string.Format("{0:MM/dd/yyyy}", sRPViewModel.activitydate) +
                    ponumber +
                    "<br />" +
                    getSupportingDocs(sRPViewModel.list_srp_files, host) +
                     showinConcur +
                    "Thank you."+
                    "<br /><br />" +
                    "Yours Sincerely,"+
					"<br />" +
                    "Rittal Sales Department"+
                    "<br /><br />";

                    emailfunction(To, From, Subject, header + Emailbody + footer);
        }
        #endregion

        #region Email Supervisor
        public async Task EmailSupervisor(IQueryable<salesRequestApprovers> regionalMan, SRPViewModel sRPViewModel, string Subject, string supervisorEmailBody = null)
        {
            //get data
            int region = Convert.ToInt32(sRPViewModel.region);
            var regionalApproval = regionalMan.Where(x =>x.Status==1 && x.ID == region).FirstOrDefault();
            var approver = await comm.GetfullName(Convert.ToInt32(regionalApproval.UserID));
            int department = Convert.ToInt32(sRPViewModel.department);
            int paymentMethod = Convert.ToInt32(sRPViewModel.paymentMethod);
            int requestType = Convert.ToInt32(sRPViewModel.requestType);
            //Text for ACH/Check
            var list_achType = FuncCommonSRP(regionalMan).achType;
            //Text for Cost Center
            var list_cccType = FuncCommonSRP(regionalMan).cccType;
            string ponumber = "";
            if (!string.IsNullOrEmpty(sRPViewModel.ponumber))
            {
                ponumber = "<br /><strong> PO Number </strong> : " + sRPViewModel.ponumber;
            }
            //set email properties
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }
            string showinConcur = "<br />";
            if (sRPViewModel.paymentMethod == "1")
            {
                showinConcur = "<strong> Supplier </strong> : " + sRPViewModel.supplier +
                "<br />" +
                "<strong> Supplier Number(SAP) </strong> : " + sRPViewModel.supplierNumber +
                "<br />" +
                "<strong> Ship To </strong> : " + sRPViewModel.shipTo +
                "<br />" +
                "<strong> Ship To Attn. </strong> : " + sRPViewModel.shiptoAttn +
                "<br />" +
                getInvoices(sRPViewModel.list_srp_invoice, host) +
                "<table cellpadding=\"5\" style=\"background: #5f5b5b;width:100%;border:solid 1px #000;border-collapse:collapse;\" border='1' >" +
                "<thead style=\"background: #efefef;\"><tr><th>Item</th><th>Part Number / Description</th><th>Quantity</th><th>Delivery Date</th><th>Unit Price</th><th>Total Price</th><th>Check of Accounts</th><th>Cost Center</th></tr><thead>" +
                getItemsList(sRPViewModel, list_achType, list_cccType) +
                " </table><br />";
            }
            string header = comm.emailheader(host);
            string footer = comm.emailfooter(host);
            string[] statusList = { "Approved", "Additional Information", "Denied" };
            StringBuilder string_status = new StringBuilder();

            if (Request.QueryString["status"]==null)
            {
                foreach (var item in statusList)
                {
                    string link = string.Format("<a href=\"{0}/SRPView/StatusSupervisor/?formid={1}&status={2}&department={3}&postComment=false\">{2}</a>", host, sRPViewModel.FormID, item, department);
                    string_status.Append(link + "<br /><br />");
                }
            }

            //string To = "antwi.s@rittal.us";
            string To = approver["email"];//uncomment this for production
            string From = "RiSource@rittal.us";
            string Emailbody = "Dear " + approver["firstName"] + "," +
                "<br />"+
                supervisorEmailBody +
                "<br /><br />" +
                "<strong> Status </strong> : " + sRPViewModel.status +
                "<br />" +
                "<strong > REQ No.</strong> : " + sRPViewModel.FormID +
                "<br />" +
                "<strong> Created On </strong> : "+ sRPViewModel.dateCreated +
                "<br />" +
                "<strong> Department </strong> : " + FuncCommonSRP(regionalMan).department[department].Text +
                "<br />" +
                "<strong> Request Type </strong> :" + FuncCommonSRP(regionalMan).requestType[requestType].Text +
                "<br />" +
                "<strong> Method of Payment</strong> : " + FuncCommonSRP(regionalMan).paymentMethod[paymentMethod].Text +
                "<br />" +
                "<strong> Title </strong> : " + sRPViewModel.title +
                "<br />" +
                "<strong> Description </strong> : " + sRPViewModel.description +
                "<br />" +
                "<strong> Estimated Cost </strong> : $" + sRPViewModel.estimatedCost +
                "<br />" +
                "<strong> Activity Date </strong> : " + string.Format("{0:MM/dd/yyyy}", sRPViewModel.activitydate) +
                ponumber +
                "<br />"+
                getSupportingDocs(sRPViewModel.list_srp_files, host) +
                showinConcur+
                "You may mark this request as "+
                "<br /><br />" +
                string_status +
                " by clicking one of the links above."+
                "<br /><br />" +
                "Thank you."+
                "<br /><br />" +
                "Yours Sincerely,<br />"+
                "Rittal.us Webmaster"+
                "<br /><br />";

                emailfunction(To, From, Subject, header + Emailbody + footer);
        }
        #endregion

        #region Email Procurement
        public async Task EmailProcurement(IQueryable<salesRequestApprovers> regionalMan, SRPViewModel sRPViewModel, string Subject, string procurementEmailBody = null)
        {
            //get data
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            int region = Convert.ToInt32(sRPViewModel.region);
            int department = Convert.ToInt32(sRPViewModel.department);
            int paymentMethod = Convert.ToInt32(sRPViewModel.paymentMethod);
            int requestType = Convert.ToInt32(sRPViewModel.requestType);
            //Text for ACH/Check
            var list_achType = FuncCommonSRP(regionalMan).achType;
            //Text for Cost Center
            var list_cccType = FuncCommonSRP(regionalMan).cccType;
            string ponumber = "";
            if (!string.IsNullOrEmpty(sRPViewModel.ponumber))
            {
                ponumber = "<br /><strong> PO Number </strong> : " + sRPViewModel.ponumber;
            }
            //set email properties
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }
            string showinConcur = "<br />";
            if (sRPViewModel.paymentMethod == "1")
            {
                showinConcur = "<strong> Supplier </strong> : " + sRPViewModel.supplier +
                "<br />" +
                "<strong> Supplier Number(SAP) </strong> : " + sRPViewModel.supplierNumber +
                "<br />" +
                "<strong> Ship To </strong> : " + sRPViewModel.shipTo +
                "<br />" +
                "<strong> Ship To Attn. </strong> : " + sRPViewModel.shiptoAttn +
                "<br />" +
                getInvoices(sRPViewModel.list_srp_invoice, host) +
                "<table cellpadding=\"5\" style=\"background: #5f5b5b;width:100%;border:solid 1px #000;border-collapse:collapse;\" border='1' >" +
                "<thead style=\"background: #efefef;\"><tr><th>Item</th><th>Part Number / Description</th><th>Quantity</th><th>Delivery Date</th><th>Unit Price</th><th>Total Price</th><th>Check of Accounts</th><th>Cost Center</th></tr><thead>" +
                getItemsList(sRPViewModel, list_achType, list_cccType) +
                " </table>";
            }
            string header = comm.emailheader(host);
            string footer = comm.emailfooter(host);
            string[] statusList = { "Approved", "Denied", "Additional Information" };
            StringBuilder string_parts = new StringBuilder();
            foreach (var item in statusList)
            {
                string link = string.Format("<a href=\"{0}/SRPView/StatusProcurment/?formid={1}&status={2}&department={3}&postComment=false\">{2}</a>", host, sRPViewModel.FormID, item, department);
                string_parts.Append(link + "<br /><br />");
            }

            string From = "webmaster@risourcecenter.com";
            //string To = approver["email"];//uncomment this for production to email Bryce Sayre "sayre.b@rittal.us"
            var To = "";
            string test = "";
            var getapprovers = regionalMan.Where(a =>a.Status==1 && a.Department == "-3");
            StringBuilder AddtoEmail = new StringBuilder();
            int countapprovers = getapprovers.Count();//Count approvers
            if (countapprovers > 0)
            {
                int x = 0;
                foreach (var item in getapprovers)
                {
                    var approver = await comm.GetfullName(Convert.ToInt32(item.UserID));
                    if (x < countapprovers - 1)
                    {
                        AddtoEmail.Append(approver["email"] + ",");
                    }
                    else
                    {
                        AddtoEmail.Append(approver["email"]);
                    }
                    x++;
                }

                test = AddtoEmail.ToString();
                To = "presswala.z@rittal.us";
            }
            else
            {
                int approverId = Convert.ToInt32(getapprovers.FirstOrDefault().UserID);
                var approver = await comm.GetfullName(approverId);
                To = "presswala.z@rittal.us";
                test = approver["email"];
            }

            string Emailbody = "Dear Rittal Buyer," +
                "<br />" +
                procurementEmailBody+
                "<br /><br />" +
                "<strong > REQ No.</strong> : " + sRPViewModel.FormID +
                "<br />" +
                "<strong> Created On </strong> : " + sRPViewModel.dateCreated +
                "<br />" +
                "<strong> Department </strong> : " + FuncCommonSRP(regionalMan).department[department].Text +
                "<br />" +
                "<strong> Request Type </strong> :" + FuncCommonSRP(regionalMan).requestType[requestType].Text +
                "<br />" +
                "<strong> Method of Payment</strong> : " + FuncCommonSRP(regionalMan).paymentMethod[paymentMethod].Text +
                "<br />" +
                "<strong> Title </strong> : " + sRPViewModel.title +
                "<br />" +
                "<strong> Description </strong> : " + sRPViewModel.description +
                "<br />" +
                "<strong> Estimated Cost </strong> : $" + sRPViewModel.estimatedCost +
                "<br />" +
                "<strong> Activity Date </strong> : " + string.Format("{0:MM/dd/yyyy}", sRPViewModel.activitydate) +
                ponumber +
                "<br />" +
                getSupportingDocs(sRPViewModel.list_srp_files, host) +
                showinConcur+
                "<br />" +
                "You may mark this request as " +
                "<br /><br />" +
                string_parts +
                " by clicking one of the links above." +
                "<br /><br />" +
                "Thank you." +
                "<br /><br />" +
                "Yours Sincerely,<br />" +
                "Rittal.us Webmaster" +
                "<br /><br />";

                emailfunction(To, From, Subject, header + Emailbody + footer);
        }
        #endregion      

        #region Email Accounting
        public async Task EmailAccounting(IQueryable<salesRequestApprovers> regionalMan, SRPViewModel sRPViewModel, string Subject, string accountingEmailBody = null)
        {
            //get data
            var salesrep = await comm.GetfullName(Convert.ToInt32(sRPViewModel.SalesRepID));
            int region = Convert.ToInt32(sRPViewModel.region);
            int department = Convert.ToInt32(sRPViewModel.department);
            int paymentMethod = Convert.ToInt32(sRPViewModel.paymentMethod);
            int requestType = Convert.ToInt32(sRPViewModel.requestType);
            //Text for ACH/Check
            var list_achType = FuncCommonSRP(regionalMan).achType;
            //Text for Cost Center
            var list_cccType = FuncCommonSRP(regionalMan).cccType;
            string ponumber = "";
            if (!string.IsNullOrEmpty(sRPViewModel.ponumber))
            {
                ponumber = "<br /><strong> PO Number </strong> : " + sRPViewModel.ponumber;
            }
            //set email properties
            string host = "https://www.risourcecenter.com";
            if (Request.Url.Port != 443)
            {
                host = Request.Url.Host + ":" + Request.Url.Port;
            }
            string showinConcur = "<br />";
            if (sRPViewModel.paymentMethod == "1")
            {
                showinConcur = "<strong> Supplier </strong> : " + sRPViewModel.supplier +
                "<br />" +
                "<strong> Supplier Number(SAP) </strong> : " + sRPViewModel.supplierNumber +
                "<br />" +
                "<strong> Ship To </strong> : " + sRPViewModel.shipTo +
                "<br />" +
                "<strong> Ship To Attn. </strong> : " + sRPViewModel.shiptoAttn +
                "<br />" +
                getInvoices(sRPViewModel.list_srp_invoice, host) +
                "<table cellpadding=\"5\" style=\"background: #5f5b5b;width:100%;border:solid 1px #000;border-collapse:collapse;\" border='1' >" +
                "<thead style=\"background: #efefef;\"><tr><th>Item</th><th>Part Number / Description</th><th>Quantity</th><th>Delivery Date</th><th>Unit Price</th><th>Total Price</th><th>Check of Accounts</th><th>Cost Center</th></tr><thead>" +
                getItemsList(sRPViewModel, list_achType, list_cccType) +
                " </table>";
            }
            string header = comm.emailheader(host);
            string footer = comm.emailfooter(host);
            string[] statusList = { "Completed", "Additional Information", "Denied" };
            StringBuilder string_parts = new StringBuilder();
            foreach (var item in statusList)
            {
                string link = string.Format("<a href=\"{0}/SRPView/StatusAccounting/?formid={1}&status={2}&department={3}&postComment=false\">{2}</a>", host, sRPViewModel.FormID, item, department);
                string_parts.Append(link + "<br /><br />");
            }
            string From = "webmaster@risourcecenter.com";
            //string To = "Kranz.a @rittal.us,Whittington.p @rittal.us";//uncomment this for production Vendors A-M: Angie Kranz, Kranz.a @rittal.us Vendors N - Z: Patty Whittington, Whittington.p @rittal.us
            var To = "";
            var test = "";
            var getapprovers = regionalMan.Where(a =>a.Status==1 && a.Department == "-2");
            StringBuilder AddtoEmail = new StringBuilder();
            int countapprovers = getapprovers.Count();//Count approvers
            if (countapprovers > 0)
            {
                int x = 0;
                foreach (var item in getapprovers)
                {
                    var approver = await comm.GetfullName(Convert.ToInt32(item.UserID));
                    if (x < countapprovers - 1)
                    {
                        AddtoEmail.Append(approver["email"] + ",");
                    }
                    else
                    {
                        AddtoEmail.Append(approver["email"]);
                    }
                    x++;
                }

                To = "presswala.z@rittal.us";
                test = AddtoEmail.ToString();
            }
            else
            {
                int approverId = Convert.ToInt32(getapprovers.FirstOrDefault().UserID);
                var approver = await comm.GetfullName(approverId);
                To = "presswala.z@rittal.us";
                test = approver["email"];
            }

            string Emailbody = "Dear Rittal Accounts Payable," +
                "<br />" +
                accountingEmailBody+
                "<br /><br />" +
                "<strong> Status </strong> : " + sRPViewModel.status +
                "<br />" +
                "<strong > REQ No.</strong> : " + sRPViewModel.FormID +
                "<br />" +
                "<strong> Created On </strong> : " + sRPViewModel.dateCreated +
                "<br />" +
                "<strong> Department </strong> : " + FuncCommonSRP(regionalMan).department[department].Text +
                "<br />" +
                "<strong> Request Type </strong> :" + FuncCommonSRP(regionalMan).requestType[requestType].Text +
                "<br />" +
                "<strong> Method of Payment</strong> : " + FuncCommonSRP(regionalMan).paymentMethod[paymentMethod].Text +
                "<br />" +
                "<strong> Title </strong> : " + sRPViewModel.title +
                "<br />" +
                "<strong> Description </strong> : " + sRPViewModel.description +
                "<br />" +
                "<strong> Estimated Cost </strong> : $" + sRPViewModel.estimatedCost +
                "<br />" +
                "<strong> Activity Date </strong> : " + string.Format("{0:MM/dd/yyyy}", sRPViewModel.activitydate) +
                ponumber +
                "<br />" +
                getSupportingDocs(sRPViewModel.list_srp_files, host) +
                showinConcur+
                "<br />" +
                "You may mark this request as " +
                "<br /><br />" +             
                string_parts +
                "by clicking one of the links above." +
                "<br /><br />" +
                "Thank you." +
                "<br /><br />" +
                "Yours Sincerely,<br />" +
                "Rittal.us Webmaster" +
                "<br /><br />";

                emailfunction(To, From, Subject, header + Emailbody + footer);
        }
        #endregion

        #region Get Item List
        private StringBuilder getItemsList(SRPViewModel sRPViewModel, List<SelectListItem> list_achType, List<SelectListItem> list_cccType)
        {
            int i = 0;
            StringBuilder items_list = new StringBuilder();
            var getItems = db.salesRequestAdditionalInfos.Where(a => a.Form_ID == sRPViewModel.FormID).FirstOrDefault();
            if (getItems!=null)
            {
                string[] partNumberOrdescription = getItems.partNumberOrdescription.Split(',');
                double total_price = 0;
                if (partNumberOrdescription.Count() > 0)
                {
                    foreach (var item in partNumberOrdescription)
                    {
                        items_list.Append("<tr>" +
                                        "<td>" +
                                            i +
                                        "</td>" +
                                        "<td>" +
                                          getItems.partNumberOrdescription.Split(',')[i] +
                                        "</td>" +
                                        "<td>" +
                                            getItems.quantity.Split(',')[i] +
                                        "</td>" +
                                        "<td>" +
                                            getItems.deliveryDate.Split(',')[i] +
                                        "</td>" +
                                        "<td>$" +
                                            getItems.unitPrice.Split(',')[i] +
                                        "</td>" +
                                        "<td>$" +
                                            getItems.totalPrice.Split(',')[i] +
                                        "</td>" +
                                        "<td>" +
                                           list_achType[Convert.ToInt32(getItems.achType.Split(',')[i])].Text +
                                        "</td>" +
                                        "<td>" +
                                           list_cccType[Convert.ToInt32(getItems.cccType.Split(',')[i])].Text +
                                        "</td>" +
                                    "</tr>");

                                    total_price = total_price + Convert.ToDouble(getItems.totalPrice.Split(',')[i]);
                        i++;
                    }

                    items_list.Append("<tr style=\"background-color: #efefef;\" >" +
                                        "<th colspan=\"5\" >" +
                                        "Total Cost" +
                                        "</th>" +
                                        "<th colspan=\"1\" align=\"left\" >$" +
                                        total_price +
                                        "</th>" +
                                        "</tr>");
                }
            }
            return items_list;
        }
        #endregion

        #region Get File Links
        private StringBuilder getSupportingDocs(IQueryable<salesRequestFile> getfileNames, string host = null)
        {
            StringBuilder items_list = new StringBuilder();
            if (getfileNames != null)
            {
                items_list.Append("<table><tr><td><b>Supporting Documents: </b></td>");

                foreach (var fileName in getfileNames)
                {
                    string link = string.Format("<a href=\"{0}/attachments/srp/files/{1}\">{1}</a>", host,fileName.FileName);
                    items_list.Append("<td>"+ link + "</td>");
                }
                items_list.Append("</tr></table>");
            }
            return items_list;
        }
        private StringBuilder getInvoices(IQueryable<salesRequestFile> getfileNames, string host = null)
        {
            StringBuilder items_list = new StringBuilder();
            if (getfileNames != null)
            {
                items_list.Append("<table><tr><td><b>Invoices: </b></td>");

                foreach (var fileName in getfileNames)
                {
                    string link = string.Format("<a href=\"{0}/attachments/srp/files/{1}\">{1}</a>", host, fileName.FileName);
                    items_list.Append("<td>" + link + "</td>");
                }
                items_list.Append("</tr></table>");
            }
            return items_list;
        }
        #endregion

        #region Email functions
        public void emailfunction(string To, string From, string Subject, string Emailbody)
        {
            //send the email out
            System.Net.Mail.MailMessage Email = new System.Net.Mail.MailMessage(From, To, Subject, Emailbody);
            Email.IsBodyHtml = true;
            System.Net.Mail.SmtpClient SMPTobj;
            SMPTobj = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["Host"]);
            SMPTobj.EnableSsl = false;
            SMPTobj.Credentials = new System.Net.NetworkCredential("", "");
            SMPTobj.Send(Email);
        }
        #endregion
    }
}
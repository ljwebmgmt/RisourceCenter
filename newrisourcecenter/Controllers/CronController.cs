using newrisourcecenter.Internals;
using newrisourcecenter.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace newrisourcecenter.Controllers
{
    public class CronController : Controller
    {
        private RisourceCenterContext db = null;
        private RisourceCenterMexicoEntities dbEntity = null;
        CommonController common = new CommonController();

        public CronController()
        {
            db = new RisourceCenterContext();
            dbEntity = new RisourceCenterMexicoEntities();
        }
        [AllowAnonymous]
        // GET: Cron/RFQ
        public async Task<ActionResult> RFQ()
        {
            List<string> tableNames = new List<string>() { "RFQ_Data", "RFQ_Files", "RFQ_Action_Log", "RFQ_RAS_Data" };
            string date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            string path = AppDomain.CurrentDomain.BaseDirectory + "exports";
            List<string> files = new List<string>();
            foreach (string tableName in tableNames)
            {
                string filename = tableName + "_" + date + ".xlsx";
                switch (tableName)
                {
                    case "RFQ_Data":
                        ExportHelper.ExportToExcel<RFQ_Data>(await dbEntity.RFQ_Data.OrderBy(a => a.ID).ToListAsync(), path, filename);
                        break;
                    case "RFQ_Files":
                        ExportHelper.ExportToExcel<RFQ_Files>(await dbEntity.RFQ_Files.OrderBy(a => a.file_id).ToListAsync(), path, filename);
                        break;
                    case "RFQ_Action_Log":
                        ExportHelper.ExportToExcel<RFQ_Action_Log>(await dbEntity.RFQ_Action_Log.OrderBy(a => a.ID).ToListAsync(), path, filename);
                        break;
                    case "RFQ_RAS_Data":
                        ExportHelper.ExportToExcel<RFQ_RAS_Data>(await dbEntity.RFQ_RAS_Data.OrderBy(a => a.ID).ToListAsync(), path, filename);
                        break;
                    default:
                        break;

                }
                files.Add(filename);
            }
            UploadRFQFilestoSFTP(files.ToArray(), path);
            return null;
        }

        public void UploadRFQFilestoSFTP(string[] files, string path)
        {
            try
            {
                string server = ConfigurationManager.AppSettings["BI-SFTP:server"];
                string port = ConfigurationManager.AppSettings["BI-SFTP:port"];
                string userName = ConfigurationManager.AppSettings["BI-SFTP:userName"];
                string password = ConfigurationManager.AppSettings["BI-SFTP:password"];
                string DestPath = ConfigurationManager.AppSettings["BI-SFTP:path"];
                foreach(string file in files)
                {
                    SFTP.UploadSFTPFile(server, userName, password, path, DestPath, file, int.Parse(port));
                }
            }
            catch (Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'UploadRFQFilestoSFTP'");
            }
        }
    }
}
using newrisourcecenter.Controllers;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace newrisourcecenter.Internals
{
    public class SFTP
    {
        public static void UploadSFTPFile(string host, string username,
        string password, string sourcefile, string destinationpath, string fileName, int port)
        {

            try
            {
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    if (client.IsConnected)
                    {
                        client.ChangeDirectory(destinationpath);
                        using (FileStream fs = new FileStream(sourcefile + "/" + fileName, FileMode.Open))
                        {
                            client.BufferSize = 4 * 1024;
                            client.UploadFile(fs, Path.GetFileName(sourcefile + "/" + fileName));
                        }
                    }
                }

                //string filename = Path.GetFileName(sourcefile);

                //FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"ftp://q-sols.com//public_html/rital/" + filename + ".csv");

                //request.Timeout = 10000;
                //request.Method = WebRequestMethods.Ftp.UploadFile;

                //request.Credentials = new NetworkCredential(username, password);

                //StreamReader sourceStream = new StreamReader(sourcefile + "/" + fileName);

                //byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());

                //request.ContentLength = fileContents.Length;

                //Stream requestStream = request.GetRequestStream();

                //requestStream.Write(fileContents, 0, fileContents.Length);
                //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                //response.Close();

                //requestStream.Close();

                //sourceStream.Close();
            }
            catch (Exception ex)
            {
                CommonController common = new CommonController();
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "On SFTP Upload");
            }


        }
    }
}
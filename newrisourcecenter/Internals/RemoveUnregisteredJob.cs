using CsvHelper;
using newrisourcecenter.Controllers;
using newrisourcecenter.Models;
using newrisourcecenter.ViewModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newrisourcecenter.Internals
{
    public class RemoveUnregisteredJob : IJob//, IDisposable
    {
        CommonController common = new CommonController();
        private ApplicationUserManager _userManager;
        AccountController accountController = new AccountController();
        string path = AppDomain.CurrentDomain.BaseDirectory + "/Content/csv";
        string fileName = "Users.csv";


        public Task Execute(IJobExecutionContext context)
        {
            Task.Factory.StartNew(CheckInactiveUsers);
            Task.Factory.StartNew(UploadUserInfotoSTFP);
            return Task.Factory.StartNew(SendEmail);
        }

        public void SendEmail()
        {
            try
            {
                List<usr_user_temp> usr_Users = new List<usr_user_temp>();
                StringBuilder emailbody = new StringBuilder();

                using (RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities())
                {
                    usr_Users = db.usr_user_temp.Where(x =>
                                !db.usr_user.Any(y => x.usr_email == y.usr_email)
                                && DbFunctions.DiffHours(x.usr_dateCreated, DateTime.Now) > 24).ToList();

                    emailbody.Append("Users Deleted with user Details as Follows : </br>");
                    foreach (var item in usr_Users)
                    {
                        emailbody.Append("First Name : " + item.usr_fName + ", Last Name : " + item.usr_lName + ", Email : " + item.usr_email + "</br>");
                    }

                    db.usr_user_temp.RemoveRange(usr_Users);
                    db.SaveChanges();
                }

                //Send email to the Return Requester
                string[] emailUsers = new string[] { "henderson.r@rittal.us", "presswala.z@rittal.us" };

                string body = emailbody.ToString();
                string From = "webmaster@rittal.us";

                if (usr_Users.Count > 0)
                {
                    foreach (var item in emailUsers)
                    {
                        common.email(From, item, "RiSourceCenter -Temporary User has been deleted ", body);
                    }
                }
            }
            catch (Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'SendEmail'");
            }
        }

        public void UploadUserInfotoSTFP()
        {
            try
            {
                string server = ConfigurationManager.AppSettings["SFTP:server"];
                string port = ConfigurationManager.AppSettings["SFTP:port"];
                string userName = ConfigurationManager.AppSettings["SFTP:userName"];
                string password = ConfigurationManager.AppSettings["SFTP:password"];
                string DestPath = ConfigurationManager.AppSettings["SFTP:path"];


                GetDataForCSV();

                SFTP.UploadSFTPFile(server, userName, password, path, DestPath, fileName, int.Parse(port));

                if (File.Exists(path + "/" + fileName))
                {
                    File.Delete(path + "/" + fileName);
                }
            }
            catch (Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'UploadUserInfotoSTFP'");
            }
        }

        public void CheckInactiveUsers()
        {
            try
            {
                using (RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities())
                {
                    int notificationDays = Convert.ToInt32(ConfigurationManager.AppSettings["UserInactiveNotificationCutoff"]);
                    int inactiveDays = Convert.ToInt32(ConfigurationManager.AppSettings["UserInactiveCutoff"]);
                    int deleteDays = Convert.ToInt32(ConfigurationManager.AppSettings["UserDeleteCutoff"]);

                    var cutoffNotification = DateTime.UtcNow.AddDays((notificationDays * -1));
                    var cutoffInactive = DateTime.UtcNow.AddDays((inactiveDays * -1));
                    var cutoffDelete = DateTime.UtcNow.AddDays((deleteDays * -1));

                    string From = "webmaster@rittal.us";
                    var users = db.usr_user.Where(x => !x.deleted && !x.inactive && x.usr_lastLogin.HasValue && ((x.usr_lastLogin.Value <= cutoffNotification && x.last_warning == null) || (x.usr_lastLogin.Value <= cutoffInactive && !x.inactove_notified.Value) || (x.usr_lastLogin.Value <= cutoffDelete))).Take(50).ToList();

                    foreach (var user in users)
                    {
                        if (user.usr_lastLogin.Value <= cutoffNotification && user.last_warning == null)   // only once
                        {
                            common.email(From, user.usr_email, "RiSourceCenter - Inactivity Warning", "Your account has been inactive for " + notificationDays + " days. It will be disabled in " + (inactiveDays - notificationDays) + " days if you do not log in.<br/<br/><a href='https://www.risourcecenter.com/Account/Login'>Log in</a> now to keep your account active.", "yes", true, "henderson.r@rittal.us");
                            user.last_warning = DateTime.UtcNow;
                        }
                        else if (user.usr_lastLogin.Value <= cutoffInactive && !user.inactove_notified.Value)   // only once
                        {
                            user.inactive = true;
                            user.inactove_notified = true;
                            common.email(From, user.usr_email, "RiSourceCenter - Account Disabled", "Your account has been disabled due to " + inactiveDays + " days of inactivity. Please contact support to reactivate", "yes", true, "henderson.r@rittal.us");
                        }
                        else if (user.usr_lastLogin.Value <= cutoffDelete)   // only once
                        {
                            user.deleted = true;
                            common.email(From, user.usr_email, "RiSourceCenter - Account Deleted", "Your account has been deleted due to " + deleteDays + " days of inactivity. Please contact support to reactivate", "yes", true, "henderson.r@rittal.us");
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'CheckInactiveUsers'");
            }
        }
        public void GetDataForCSV()
        {
            try
            {
                List<UserInfo> userInfo = new List<UserInfo>();
                using (RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities())
                {

                    userInfo = db.Database.SqlQuery<UserInfo>(@"
select 
u.usr_email UserName,u.usr_fName FirstName , u.usr_lName LastName, u.usr_email [Password],u.usr_email Email,
'Rittal North America LLC' Company,'VISOgraphic, Inc.' Facility,'No Preference' UnitSystem ,
'en-US' DefaultLanguage, case when u.usr_add1 is null or u.usr_add1 = '' then '123 street' else u.usr_add1 end AddressLineOne ,isnull(usr_add2,'') AddressLineTwo ,case when usr_city is null or usr_city = '' then 'city' else usr_city end City,
case when state.state_abbr is null or state.state_abbr = '' then 'IL' else state.state_abbr end State,case when u.usr_zip  is null or u.usr_zip = '' then '12345' else u.usr_zip end Zip 
,c.country_abbr Country,case when u.usr_phone is null or u.usr_phone = '' then '12345678' else u.usr_phone end PhoneNumber1,
'Active' AccountStatus ,u.usr_email SSOToken ,'FALSE' ChangePassword
 from usr_user u
join AspNetUsers aspU on CONVERT(nvarchar, u.system_ID)=CONVERT(nvarchar, aspU.Id)
left join data_state state on state.stateid=u.usr_state
left join countries c on c.country_id=u.usr_country
where aspU.EmailConfirmed = 1
").ToList();
                }

                MakeCSV(userInfo);
            }
            catch (Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'GetDataForCSV'");
            }
        }

        public void MakeCSV(List<UserInfo> userRecords)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (var writer = new StreamWriter(string.Format(path + "/" + fileName)))
                using (var csv = new CsvWriter(writer))
                {
                    csv.Configuration.RegisterClassMap<UserInfoMap>();
                    csv.WriteRecords(userRecords);
                }
            }
            catch (Exception ex)
            {
                common.FileLog("Exception : \n" + ex.Message + "\n" + ex.StackTrace, "Error in 'MakeCSV'");
            }
        }

    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using newrisourcecenter.Models;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Data.Entity;
using newrisourcecenter.Controllers;
using Newtonsoft.Json;
using System.Web;

namespace newrisourcecenter.Tests.Controllers
{
    [TestClass]
    public class SPAControllerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestClass]
        public class GetBlogs
        {
            [TestMethod]
            public void GetSalesReps_Test()
            {
                var admin = new Mock<HttpSessionStateBase>();
                string zipcode = "1002";
                var SPA_Territory_Codes = new List<SPATerritoryCode>
                {
                    new SPATerritoryCode { ID=1, zip = "501",territory_code="57" },
                    new SPATerritoryCode { ID=2, zip = "1003",territory_code="4" },
                    new SPATerritoryCode { ID=3, zip = "1002",territory_code="4" },
                }.AsQueryable();

                var mockSet = new Mock<DbSet<SPATerritoryCode>>();
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.Provider).Returns(SPA_Territory_Codes.Provider);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.Expression).Returns(SPA_Territory_Codes.Expression);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.ElementType).Returns(SPA_Territory_Codes.ElementType);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.GetEnumerator()).Returns(SPA_Territory_Codes.GetEnumerator());

                var SPA_Account_Manager = new List<SPAAccountManager>
                {
                    new SPAAccountManager {ID=1, zip = "501",territory_code="57",contact_name="Rick Kappel",email="Kappel.r@rittal.us" },
                    new SPAAccountManager { ID=2, zip = "1003",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=3, zip = "1005",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                    new SPAAccountManager { ID=4, zip = "1033",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=5, zip = "1007",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                    new SPAAccountManager { ID=6, zip = "1012",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=7, zip = "1026",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                }.AsQueryable();

                var mockSet1 = new Mock<DbSet<SPAAccountManager>>();
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.Provider).Returns(SPA_Account_Manager.Provider);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.Expression).Returns(SPA_Account_Manager.Expression);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.ElementType).Returns(SPA_Account_Manager.ElementType);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.GetEnumerator()).Returns(SPA_Account_Manager.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPATerritoryCodes).Returns(mockSet.Object);
                mockContext.Setup(m => m.SPAAccountManagers).Returns(mockSet1.Object);

                var service = new SPAController(mockContext.Object);
                string get_sales_reps = service.GetSalesReps(zipcode, "yes");
                dynamic results = JsonConvert.DeserializeObject(get_sales_reps);
                var territory_code = results.territory_code.Value;
                var spaaccountmanager = results.spaaccountmanager;

                Assert.IsNotNull(results);
                Assert.AreEqual("4", territory_code);
                Assert.IsTrue(2 == spaaccountmanager.Count);
            }

            [TestMethod]
            public void GetSalesRepsNoMatch_Test()
            {
                var admin = new Mock<HttpSessionStateBase>();
                string zipcode = "502";
                var SPA_Territory_Codes = new List<SPATerritoryCode>
                {
                    new SPATerritoryCode {ID=1, zip = "501",territory_code="57" },
                    new SPATerritoryCode { ID=2, zip = "1003",territory_code="4" },
                    new SPATerritoryCode { ID=3, zip = "1002",territory_code="4" },
                }.AsQueryable();

                var mockSet = new Mock<DbSet<SPATerritoryCode>>();
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.Provider).Returns(SPA_Territory_Codes.Provider);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.Expression).Returns(SPA_Territory_Codes.Expression);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.ElementType).Returns(SPA_Territory_Codes.ElementType);
                mockSet.As<IQueryable<SPATerritoryCode>>().Setup(m => m.GetEnumerator()).Returns(SPA_Territory_Codes.GetEnumerator());

                var SPA_Account_Manager = new List<SPAAccountManager>
                {
                    new SPAAccountManager {ID=1, zip = "501",territory_code="57",contact_name="Rick Kappel",email="Kappel.r@rittal.us" },
                    new SPAAccountManager { ID=2, zip = "1003",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=3, zip = "1005",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                    new SPAAccountManager { ID=4, zip = "1033",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=5, zip = "1007",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                    new SPAAccountManager { ID=6, zip = "1012",territory_code="4",contact_name="Art Bors",email="Bors.a@rittal.us" },
                    new SPAAccountManager { ID=7, zip = "1026",territory_code="4",contact_name="Clifford Clark",email="Clark.c@rittal.us" },
                }.AsQueryable();

                var mockSet1 = new Mock<DbSet<SPAAccountManager>>();
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.Provider).Returns(SPA_Account_Manager.Provider);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.Expression).Returns(SPA_Account_Manager.Expression);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.ElementType).Returns(SPA_Account_Manager.ElementType);
                mockSet1.As<IQueryable<SPAAccountManager>>().Setup(m => m.GetEnumerator()).Returns(SPA_Account_Manager.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPATerritoryCodes).Returns(mockSet.Object);
                mockContext.Setup(m => m.SPAAccountManagers).Returns(mockSet1.Object);

                var service = new SPAController(mockContext.Object);
                string get_sales_reps = service.GetSalesReps(zipcode, "yes");
                dynamic results = JsonConvert.DeserializeObject(get_sales_reps);
                var territory_code = results.territory_code.Value;
                var spaaccountmanager = results.spaaccountmanager;

                Assert.AreEqual("", territory_code);
                Assert.IsFalse(2 == spaaccountmanager.Count);
            }

            [TestMethod]
            public void Search_Participants_NoQueryString_Test()
            {
                int spaid = 2;
                string querystring = null;//Name,email
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=2,Customer_name="Amazon" },
                    new SPAViewModels{Spa_id=2,Comp_id=321,Customer_name="Dell" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());
                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.GetSalesRepsParticipants(querystring, spaid, "Yes");
                dynamic results = JsonConvert.DeserializeObject(search_participants);
                var querystrings = results.querystring.Value;
                var spaaccountmanager = results.spaaccountmanager;
                var count = spaaccountmanager.Count;
                Assert.AreEqual(null, querystrings);
                Assert.IsTrue(6 == count );
            }

            [TestMethod]
            public void Search_Participants_Email_Test()
            {
                int spaid = 2;
                string querystring = "weeks.s@rittal.us";//Name,email
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=2,Customer_name="Amazon" },
                    new SPAViewModels{Spa_id=2,Comp_id=321,Customer_name="Dell" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());
                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.GetSalesRepsParticipants(querystring, spaid, "Yes");
                dynamic results = JsonConvert.DeserializeObject(search_participants);
                var querystrings = results.querystring.Value;
                var spaaccountmanager = results.spaaccountmanager;

                Assert.AreEqual("weeks.s@rittal.us", querystrings);
                Assert.IsTrue(1== spaaccountmanager.Count);
            }

            [TestMethod]
            public void Search_Participants_Name_Not_Included_Test()
            {
                int spaid = 2;
                string querystring = "Michelle Savage";//Name,email
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=2,Customer_name="Amazon" },
                    new SPAViewModels{Spa_id=2,Comp_id=321,Customer_name="Dell" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());
                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.GetSalesRepsParticipants(querystring, spaid, "Yes");
                dynamic results = JsonConvert.DeserializeObject(search_participants);
                var querystrings = results.querystring.Value;
                var spaaccountmanager = results.spaaccountmanager;

                Assert.AreEqual("Michelle Savage", querystrings);
                Assert.IsTrue(0 == spaaccountmanager.Count);
            }

            [TestMethod]
            public void Search_Participants_In_Rittal_Test()
            {
                int spaid = 2;
                string querystring = "Shawn Weeks";//Name,email
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=2,Customer_name="Amazon" },
                    new SPAViewModels{Spa_id=2,Comp_id=321,Customer_name="Dell" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());
                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.GetSalesRepsParticipants(querystring, spaid, "Yes");
                dynamic results = JsonConvert.DeserializeObject(search_participants);
                var querystrings = results.querystring.Value;
                var spaaccountmanager = results.spaaccountmanager;

                Assert.AreEqual("Shawn Weeks", querystrings);
                Assert.IsTrue(0 == spaaccountmanager.Count);
            }

            [TestMethod]
            public void Notes_internal_TestAsync()
            {
                //Rittal user internal note writer
                int spaid = 2;
                string companyId = "362";
                string username = "koetzsch.j@rittal.de";//loggin as user
                //Get spa data
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=312,Usr_id=18127,Customer_name="Amazon",Sales_rep_user="Kappel.r@rittal.us" },
                    new SPAViewModels{Spa_id=2,Comp_id=2,Usr_id=141,Customer_name="Dell",Sales_rep_user="Clark.c@rittal.us" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get SPA sales reps data
                var spa_sales_rep = new List<SPASalesRepsViewModel>
                {
                    new SPASalesRepsViewModel{Rep_id=1,Usr_id="noffke.e@rittal.us",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=2,Usr_id="jarel@i-automation.com",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=3,Usr_id="mjohnson@i-automation.com",Form_id=2 },
                    new SPASalesRepsViewModel{Rep_id=4,Usr_id="csteward@smrgulfcoast.com",Form_id=2 }
                }.AsQueryable();

                var spa_sales_rep_mockSet = new Mock<DbSet<SPASalesRepsViewModel>>();
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Provider).Returns(spa_sales_rep.Provider);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Expression).Returns(spa_sales_rep.Expression);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.ElementType).Returns(spa_sales_rep.ElementType);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.GetEnumerator()).Returns(spa_sales_rep.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());

                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);
                mockContext.Setup(m => m.SPA_SalesRepsViewModels).Returns(spa_sales_rep_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants =  service.InnternalExternal(spaid,companyId,username);
                Assert.IsTrue("true" == search_participants);
            }

            [TestMethod]
            public void Notes_external_TestAsync()
            {
                //Man rep external note writer
                int spaid = 2;
                string companyId = "251";
                string username = "steveb@mechanicalindustrial.com";//loggin as user
                //Get spa data
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=312,Usr_id=18127,Customer_name="Amazon",Sales_rep_user="Kappel.r@rittal.us" },
                    new SPAViewModels{Spa_id=2,Comp_id=2,Usr_id=141,Customer_name="Dell",Sales_rep_user="Clark.c@rittal.us" },//Company 321 is SMR Gulfcoast type is Man rep
                    new SPAViewModels{Spa_id=3,Comp_id=312,Usr_id=18127,Customer_name="Dell",Sales_rep_user="antwi.s@rittal.us" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get SPA sales reps data
                var spa_sales_rep = new List<SPASalesRepsViewModel>
                {
                    new SPASalesRepsViewModel{Rep_id=1,Usr_id="noffke.e@rittal.us",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=2,Usr_id="jarel@i-automation.com",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=3,Usr_id="mjohnson@i-automation.com",Form_id=2 },
                    new SPASalesRepsViewModel{Rep_id=4,Usr_id="csteward@smrgulfcoast.com",Form_id=2 },
                    new SPASalesRepsViewModel{Rep_id=4,Usr_id="steveb@mechanicalindustrial.com",Form_id=2 }
               }.AsQueryable();

                var spa_sales_rep_mockSet = new Mock<DbSet<SPASalesRepsViewModel>>();
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Provider).Returns(spa_sales_rep.Provider);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Expression).Returns(spa_sales_rep.Expression);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.ElementType).Returns(spa_sales_rep.ElementType);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.GetEnumerator()).Returns(spa_sales_rep.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());

                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);
                mockContext.Setup(m => m.SPA_SalesRepsViewModels).Returns(spa_sales_rep_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.InnternalExternal(spaid, companyId, username);
                Assert.IsTrue("false" == search_participants);
            }

            [TestMethod]
            public void Notes_Excluded_Not_Particiapnts_TestAsync()
            {
                //Man rep external note writer
                int spaid = 2;
                string companyId = "251";
                string username = "steveb@mechanicalindustrial.com";//loggin as user
                //Get spa data
                var spa_data = new List<SPAViewModels>
                {
                    new SPAViewModels{Spa_id=1,Comp_id=312,Usr_id=18127,Customer_name="Amazon",Sales_rep_user="Kappel.r@rittal.us" },
                    new SPAViewModels{Spa_id=2,Comp_id=2,Usr_id=141,Customer_name="Dell",Sales_rep_user="Clark.c@rittal.us" },//Company 321 is SMR Gulfcoast type is Man rep
                    new SPAViewModels{Spa_id=3,Comp_id=312,Usr_id=18127,Customer_name="Dell",Sales_rep_user="antwi.s@rittal.us" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get SPA sales reps data
                var spa_sales_rep = new List<SPASalesRepsViewModel>
                {
                    new SPASalesRepsViewModel{Rep_id=1,Usr_id="noffke.e@rittal.us",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=2,Usr_id="jarel@i-automation.com",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=3,Usr_id="mjohnson@i-automation.com",Form_id=2 },
                    new SPASalesRepsViewModel{Rep_id=4,Usr_id="csteward@smrgulfcoast.com",Form_id=2 },
                    //new SPASalesRepsViewModel{Rep_id=4,Usr_id="steveb@mechanicalindustrial.com",Form_id=2 }
               }.AsQueryable();

                var spa_sales_rep_mockSet = new Mock<DbSet<SPASalesRepsViewModel>>();
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Provider).Returns(spa_sales_rep.Provider);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Expression).Returns(spa_sales_rep.Expression);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.ElementType).Returns(spa_sales_rep.ElementType);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.GetEnumerator()).Returns(spa_sales_rep.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());

                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);
                mockContext.Setup(m => m.SPA_SalesRepsViewModels).Returns(spa_sales_rep_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.InnternalExternal(spaid, companyId, username);
                Assert.IsTrue("" == search_participants);
            }

            [TestMethod]
            public void Notes_Excluded_NotRittal_TestAsync()
            {
                //Not a SPA sales rep
                int spaid = 2;
                string companyId = "321";
                string username = "csteward@smrgulfcoast.com";
                //Get spa data
                var spa_data = new List<SPAViewModels>
                {
                   new SPAViewModels{Spa_id=1,Comp_id=312,Usr_id=18127,Customer_name="Amazon",Sales_rep_user="Kappel.r@rittal.us" },
                   new SPAViewModels{Spa_id=2,Comp_id=2,Usr_id=141,Customer_name="Dell",Sales_rep_user="Clark.c@rittal.us" },//Company 321 is SMR Gulfcoast type is Man rep
                   new SPAViewModels{Spa_id=3,Comp_id=312,Usr_id=18127,Customer_name="Dell",Sales_rep_user="antwi.s@rittal.us" }//Company 321 is SMR Gulfcoast type is Man rep
                }.AsQueryable();

                var spa_mockSet = new Mock<DbSet<SPAViewModels>>();
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Provider).Returns(spa_data.Provider);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.Expression).Returns(spa_data.Expression);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.ElementType).Returns(spa_data.ElementType);
                spa_mockSet.As<IQueryable<SPAViewModels>>().Setup(m => m.GetEnumerator()).Returns(spa_data.GetEnumerator());

                //Get SPA sales reps data
                var spa_sales_rep = new List<SPASalesRepsViewModel>
                {
                    new SPASalesRepsViewModel{Rep_id=1,Usr_id="noffke.e@rittal.us",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=2,Usr_id="jarel@i-automation.com",Form_id=1 },
                    new SPASalesRepsViewModel{Rep_id=3,Usr_id="mjohnson@i-automation.com",Form_id=2 },
                }.AsQueryable();

                var spa_sales_rep_mockSet = new Mock<DbSet<SPASalesRepsViewModel>>();
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Provider).Returns(spa_sales_rep.Provider);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.Expression).Returns(spa_sales_rep.Expression);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.ElementType).Returns(spa_sales_rep.ElementType);
                spa_sales_rep_mockSet.As<IQueryable<SPASalesRepsViewModel>>().Setup(m => m.GetEnumerator()).Returns(spa_sales_rep.GetEnumerator());

                //Get company data
                var partner_Company = new List<partnerCompanyViewModel>
                {
                    new partnerCompanyViewModel {comp_ID=1, comp_name = "Accu-Tech Corporation",comp_industry=1, comp_type=2,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=2, comp_name = "iAutomation",comp_industry=2, comp_type=1,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=312, comp_name = "Rittal Corporation",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=362, comp_name = "Rittal GmbH",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=407, comp_name = "Rittal Corp. NE",comp_industry=3, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=412, comp_name = "Rittal Corp. IT East",comp_industry=1, comp_type=5,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=220, comp_name = "Prime Devices Corporation",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=239, comp_name = "Austin Brown, Inc",comp_industry=2, comp_type=3,comp_active=0 },
                    new partnerCompanyViewModel {comp_ID=251, comp_name = "Cal Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
                    new partnerCompanyViewModel {comp_ID=321, comp_name = "Cal2 Industrial Sales Co.",comp_industry=2, comp_type=3,comp_active=1 },
               }.AsQueryable();

                var comp_mockSet = new Mock<DbSet<partnerCompanyViewModel>>();
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Provider).Returns(partner_Company.Provider);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.Expression).Returns(partner_Company.Expression);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.ElementType).Returns(partner_Company.ElementType);
                comp_mockSet.As<IQueryable<partnerCompanyViewModel>>().Setup(m => m.GetEnumerator()).Returns(partner_Company.GetEnumerator());

                //Get User data
                var user = new List<UserViewModel>
                {
                    //Rittal Users
                    new UserViewModel {usr_ID=7, usr_fName="Shawn",usr_lName="Weeks", usr_email="weeks.s@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=8, usr_fName="George",usr_lName="Correira", usr_email="correira.g@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=9, usr_fName="Michelle",usr_lName="McFaddin", usr_email="mcfaddin.m@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=13, usr_fName="Ann-Marie",usr_lName="Lough", usr_email="lough.a@rittal.us", comp_ID=312, usr_status=1},
                    new UserViewModel {usr_ID=4549, usr_fName="Michelle",usr_lName="Savage", usr_email="michelle.savage@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=4550, usr_fName="Steve",usr_lName="Shannon", usr_email="steve.shannon@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=38373, usr_fName="Paul",usr_lName="Varone", usr_email="paul.varone@accu-tech.com", comp_ID=1, usr_status=1},
                    new UserViewModel {usr_ID=141, usr_fName="John",usr_lName="Arel", usr_email="jarel@i-automation.com", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=1326, usr_fName="Michele",usr_lName="Johnson", usr_email="koetzsch.j@rittal.de", comp_ID=2, usr_status=1},
                    new UserViewModel {usr_ID=5382, usr_fName="Judith",usr_lName="Koetzsch", usr_email="mjohnson@i-automation.com", comp_ID=362, usr_status=1},
                    new UserViewModel {usr_ID=38535, usr_fName="Christina",usr_lName="Schneider", usr_email="Schneider.Chr@rittal.de", comp_ID=362, usr_status=1},
                    //Manreps Users
                    new UserViewModel {usr_ID=3961, usr_fName="Bob",usr_lName="Wilson", usr_email="bwilson@primedevices.com", comp_ID=220, usr_status=1},
                    new UserViewModel {usr_ID=3972, usr_fName="Steve",usr_lName="Bruening", usr_email="steveb@mechanicalindustrial.com", comp_ID=251, usr_status=1},
                    new UserViewModel {usr_ID=982, usr_fName="Collette",usr_lName="Steward", usr_email="csteward@smrgulfcoast.com", comp_ID=321, usr_status=1},
                    new UserViewModel {usr_ID=1006, usr_fName="Chris",usr_lName="Ware", usr_email="cware@smrgulfcoast.com", comp_ID=321, usr_status=1},
               }.AsQueryable();

                var user_mockSet = new Mock<DbSet<UserViewModel>>();
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Provider).Returns(user.Provider);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.Expression).Returns(user.Expression);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.ElementType).Returns(user.ElementType);
                user_mockSet.As<IQueryable<UserViewModel>>().Setup(m => m.GetEnumerator()).Returns(user.GetEnumerator());

                var mockContext = new Mock<RisourceCenterContext>();
                mockContext.Setup(m => m.SPAViewModels).Returns(spa_mockSet.Object);
                mockContext.Setup(m => m.partnerCompanyViewModels).Returns(comp_mockSet.Object);
                mockContext.Setup(m => m.UserViewModels).Returns(user_mockSet.Object);
                mockContext.Setup(m => m.SPA_SalesRepsViewModels).Returns(spa_sales_rep_mockSet.Object);

                var service = new SPAController(mockContext.Object);
                string search_participants = service.InnternalExternal(spaid, companyId, username);
                Assert.IsTrue("" == search_participants);
            }
        }
    }
}

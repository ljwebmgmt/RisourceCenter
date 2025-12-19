using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using newrisourcecenter.Models;
using System.Collections.Generic;
using Moq;
using System.Linq;
using System.Data.Entity;
using newrisourcecenter.Controllers;
using Newtonsoft.Json;

namespace newrisourcecenter.Tests.Controllers
{
    [TestClass]
    public class CommunicationsTest
    {
        //[TestMethod]
        //public async System.Threading.Tasks.Task SearchCommunications_Rittal_TestAsync()
        //{
        //    string userProducts = "1,2,3";
        //    string audience = "5";//Rittal User
        //    string industry = "3";//Both IT and IE
        //    //Get company data
        //    var sales_communications = new List<SalesCommViewModel>
        //        {
        //            new SalesCommViewModel {
        //                scID = 1,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified Old",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_keywords ="PRL 32 IT Commodity Material Old",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2009-02-04 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //            new SalesCommViewModel {
        //                scID =325,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2015-12-31 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //           new SalesCommViewModel {
        //                scID =328,
        //                sc_headline ="Rittal Holiday Shipping Schedule IT",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p> test ",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p> test",
        //                sc_keywords ="PRL 32 IT Commodity Material test",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "1",
        //                attach_risource = "80",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =329,
        //                sc_headline ="Rittal Holiday Shipping Schedule IE",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "83",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =23812,
        //                sc_headline ="Q3 Product Training Webinars Schedule",
        //                sc_teaser ="Q3 Product Training Webinars Schedule",
        //                sc_body ="<p>The Q3 Product Training webinar schedule is posted and ready for your registrations. <a href=\"https://rittal-corp.webex.com/mw3000/mywebex/default.do?siteurl=rittal-corp&amp;service=7\"><span style=\"color: #3366ff;\">&nbsp;Click here</span> </a>to view the schedule and reserve your seat.</p><p> &nbsp;</p>",
        //                sc_keywords ="",
        //                sc_products ="1,2,3,4,5,6,8",
        //                sc_usrTypes ="1,3,5",
        //                sc_startDate=Convert.ToDateTime("2017-09-26 00:00:00.000"),
        //                sc_industry = "1,2",
        //                attach_risource = "",
        //                sc_status =1 },
        //        }.AsQueryable();

        //    var comm_mockSet = new Mock<DbSet<SalesCommViewModel>>();
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Provider).Returns(sales_communications.Provider);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Expression).Returns(sales_communications.Expression);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.ElementType).Returns(sales_communications.ElementType);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.GetEnumerator()).Returns(sales_communications.GetEnumerator());

        //    //Get User data
        //    var risources = new List<RiSourcesViewModel>
        //        {
        //            //Rittal Users
        //            new RiSourcesViewModel {ris_ID=1, ris_headline="Rittal Chimney - Thermal Management Solution",ris_teaser="Rittal Chimney - Thermal Management Solution",ris_link="a4d9c744-8309-4d2e-9522-e0f66788118dRittal_Rittal_Chimney_-_Data_Sheet_5_3088.pdf"},
        //            new RiSourcesViewModel {ris_ID=80, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="PRL32wITCommodity0421090409.xls"},
        //            new RiSourcesViewModel {ris_ID=83, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="2009HolidayShutdownSchedule1209.doc"},
        //       }.AsQueryable();

        //    var ris_mockSet = new Mock<DbSet<RiSourcesViewModel>>();
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Provider).Returns(risources.Provider);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Expression).Returns(risources.Expression);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.ElementType).Returns(risources.ElementType);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.GetEnumerator()).Returns(risources.GetEnumerator());

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    var mockContext1 = new Mock<RisourceCenterMexicoEntities>();

        //    mockContext.Setup(m => m.RiSourcesViewModels).Returns(ris_mockSet.Object);
        //    mockContext.Setup(m => m.SalesCommViewModels).Returns(comm_mockSet.Object);
        //    var service = new SalesCommController(mockContext.Object, mockContext1.Object);
        //    string get_communications = await service.SearchCommuications("Schedule", userProducts, industry, audience, "yes");
        //    dynamic results = JsonConvert.DeserializeObject(get_communications);
        //    var commData = results.commData;
        //    Assert.IsTrue(3 == commData.Count);
        //}

        //[TestMethod]
        //public async System.Threading.Tasks.Task SearchComm_NullProducts_Rittal_TestAsync()
        //{
        //    string userProducts = "8";
        //    string audience = "5";//Rittal User
        //    string industry = "3";//Both IT and IE
        //    //Get company data
        //    var sales_communications = new List<SalesCommViewModel>
        //        {
        //            new SalesCommViewModel {
        //                scID = 1,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified Old",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_keywords ="PRL 32 IT Commodity Material Old",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2009-02-04 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //            new SalesCommViewModel {
        //                scID =325,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2015-12-31 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //           new SalesCommViewModel {
        //                scID =328,
        //                sc_headline ="Rittal Holiday Shipping Schedule IT",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p> test ",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p> test",
        //                sc_keywords ="PRL 32 IT Commodity Material test",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "1",
        //                attach_risource = "80",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =329,
        //                sc_headline ="Rittal Holiday Shipping Schedule IE",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "83",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =23812,
        //                sc_headline ="Q3 Product Training Webinars Schedule",
        //                sc_teaser ="Q3 Product Training Webinars Schedule",
        //                sc_body ="<p>The Q3 Product Training webinar schedule is posted and ready for your registrations. <a href=\"https://rittal-corp.webex.com/mw3000/mywebex/default.do?siteurl=rittal-corp&amp;service=7\"><span style=\"color: #3366ff;\">&nbsp;Click here</span> </a>to view the schedule and reserve your seat.</p><p> &nbsp;</p>",
        //                sc_keywords ="",
        //                sc_products ="1,2,3,4,5,6,8",
        //                sc_usrTypes ="1,3,5",
        //                sc_startDate=Convert.ToDateTime("2017-09-26 00:00:00.000"),
        //                sc_industry = "1,2",
        //                attach_risource = "",
        //                sc_status =1 },
        //        }.AsQueryable();

        //    var comm_mockSet = new Mock<DbSet<SalesCommViewModel>>();
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Provider).Returns(sales_communications.Provider);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Expression).Returns(sales_communications.Expression);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.ElementType).Returns(sales_communications.ElementType);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.GetEnumerator()).Returns(sales_communications.GetEnumerator());

        //    //Get User data
        //    var risources = new List<RiSourcesViewModel>
        //        {
        //            //Rittal Users
        //            new RiSourcesViewModel {ris_ID=1, ris_headline="Rittal Chimney - Thermal Management Solution",ris_teaser="Rittal Chimney - Thermal Management Solution",ris_link="a4d9c744-8309-4d2e-9522-e0f66788118dRittal_Rittal_Chimney_-_Data_Sheet_5_3088.pdf"},
        //            new RiSourcesViewModel {ris_ID=80, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="PRL32wITCommodity0421090409.xls"},
        //            new RiSourcesViewModel {ris_ID=83, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="2009HolidayShutdownSchedule1209.doc"},
        //       }.AsQueryable();

        //    var ris_mockSet = new Mock<DbSet<RiSourcesViewModel>>();
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Provider).Returns(risources.Provider);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Expression).Returns(risources.Expression);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.ElementType).Returns(risources.ElementType);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.GetEnumerator()).Returns(risources.GetEnumerator());

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    var mockContext1 = new Mock<RisourceCenterMexicoEntities>();

        //    mockContext.Setup(m => m.RiSourcesViewModels).Returns(ris_mockSet.Object);
        //    mockContext.Setup(m => m.SalesCommViewModels).Returns(comm_mockSet.Object);
        //    var service = new SalesCommController(mockContext.Object, mockContext1.Object);
        //    string get_communications = await service.SearchCommuications("Schedule", userProducts, industry, audience, "yes");
        //    dynamic results = JsonConvert.DeserializeObject(get_communications);
        //    var commData = results.commData;
        //    Assert.IsTrue(1 == commData.Count);
        //}

        //[TestMethod]
        //public async System.Threading.Tasks.Task SearchCommunications_NonRittal_TestAsync()
        //{
        //    string userProducts = "1,2,3";
        //    string audience = "1";//Distributor
        //    string industry = "1";//IT
        //    //Get company data
        //    var sales_communications = new List<SalesCommViewModel>
        //        {
        //            new SalesCommViewModel {
        //                scID = 1,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified Old",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_keywords ="PRL 32 IT Commodity Material Old",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2009-02-04 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //            new SalesCommViewModel {
        //                scID =325,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2015-12-31 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //           new SalesCommViewModel {
        //                scID =328,
        //                sc_headline ="Rittal Holiday Shipping Schedule IT",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p> test ",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p> test",
        //                sc_keywords ="PRL 32 IT Commodity Material test",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "1",
        //                attach_risource = "80",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =329,
        //                sc_headline ="Rittal Holiday Shipping Schedule IE",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "83",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =23812,
        //                sc_headline ="Q3 Product Training Webinars Schedule",
        //                sc_teaser ="Q3 Product Training Webinars Schedule",
        //                sc_body ="<p>The Q3 Product Training webinar schedule is posted and ready for your registrations. <a href=\"https://rittal-corp.webex.com/mw3000/mywebex/default.do?siteurl=rittal-corp&amp;service=7\"><span style=\"color: #3366ff;\">&nbsp;Click here</span> </a>to view the schedule and reserve your seat.</p><p> &nbsp;</p>",
        //                sc_keywords ="",
        //                sc_products ="1,2,3,4,5,6,8",
        //                sc_usrTypes ="1,3,5",
        //                sc_startDate=Convert.ToDateTime("2017-09-26 00:00:00.000"),
        //                sc_industry = "1,2",
        //                attach_risource = "",
        //                sc_status =1 },
        //        }.AsQueryable();

        //    var comm_mockSet = new Mock<DbSet<SalesCommViewModel>>();
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Provider).Returns(sales_communications.Provider);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Expression).Returns(sales_communications.Expression);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.ElementType).Returns(sales_communications.ElementType);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.GetEnumerator()).Returns(sales_communications.GetEnumerator());

        //    //Get User data
        //    var risources = new List<RiSourcesViewModel>
        //        {
        //            //Rittal Users
        //            new RiSourcesViewModel {ris_ID=1, ris_headline="Rittal Chimney - Thermal Management Solution",ris_teaser="Rittal Chimney - Thermal Management Solution",ris_link="a4d9c744-8309-4d2e-9522-e0f66788118dRittal_Rittal_Chimney_-_Data_Sheet_5_3088.pdf"},
        //            new RiSourcesViewModel {ris_ID=80, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="PRL32wITCommodity0421090409.xls"},
        //            new RiSourcesViewModel {ris_ID=83, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="2009HolidayShutdownSchedule1209.doc"},
        //       }.AsQueryable();

        //    var ris_mockSet = new Mock<DbSet<RiSourcesViewModel>>();
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Provider).Returns(risources.Provider);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Expression).Returns(risources.Expression);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.ElementType).Returns(risources.ElementType);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.GetEnumerator()).Returns(risources.GetEnumerator());

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    var mockContext1 = new Mock<RisourceCenterMexicoEntities>();

        //    mockContext.Setup(m => m.RiSourcesViewModels).Returns(ris_mockSet.Object);
        //    mockContext.Setup(m => m.SalesCommViewModels).Returns(comm_mockSet.Object);
        //    var service = new SalesCommController(mockContext.Object, mockContext1.Object);
        //    string get_communications = await service.SearchCommuications("Schedule", userProducts, industry, audience, "yes");
        //    dynamic results = JsonConvert.DeserializeObject(get_communications);
        //    var commData = results.commData;
        //    Assert.IsTrue(2 == commData.Count);
        //}

        //[TestMethod]
        //public async System.Threading.Tasks.Task SearchCommun_Null_TestAsync()
        //{
        //    string userProducts = "1,2,3";
        //    string audience = "10";//Distributor
        //    string industry = "1";//IT
        //    //Get company data
        //    var sales_communications = new List<SalesCommViewModel>
        //        {
        //            new SalesCommViewModel {
        //                scID = 1,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified Old",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p> Old",
        //                sc_keywords ="PRL 32 IT Commodity Material Old",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2009-02-04 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //            new SalesCommViewModel {
        //                scID =325,
        //                sc_headline ="PRL 32 w IT & Commodity Material Identified",
        //                sc_teaser ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_body ="<p>PRL 32 w IT &amp; Commodity Material Identified </p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2015-12-31 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "80",
        //                sc_status =1 },
        //           new SalesCommViewModel {
        //                scID =328,
        //                sc_headline ="Rittal Holiday Shipping Schedule IT",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p> test ",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p> test",
        //                sc_keywords ="PRL 32 IT Commodity Material test",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "1",
        //                attach_risource = "80",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =329,
        //                sc_headline ="Rittal Holiday Shipping Schedule IE",
        //                sc_teaser ="<p> Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_body ="<p>Rittal 2009 Holiday Shipping Schedule</p><p>Click the link to download the Rittal 2009 Holiday Shipping Schedule</p>",
        //                sc_keywords ="PRL 32 IT Commodity Material",
        //                sc_products ="1,2,3",
        //                sc_usrTypes ="1,2,3,4,5",
        //                sc_startDate=Convert.ToDateTime("2016-12-10 00:00:00.000"),
        //                sc_industry = "2",
        //                attach_risource = "83",
        //                sc_status =3 },
        //           new SalesCommViewModel {
        //                scID =23812,
        //                sc_headline ="Q3 Product Training Webinars Schedule",
        //                sc_teaser ="Q3 Product Training Webinars Schedule",
        //                sc_body ="<p>The Q3 Product Training webinar schedule is posted and ready for your registrations. <a href=\"https://rittal-corp.webex.com/mw3000/mywebex/default.do?siteurl=rittal-corp&amp;service=7\"><span style=\"color: #3366ff;\">&nbsp;Click here</span> </a>to view the schedule and reserve your seat.</p><p> &nbsp;</p>",
        //                sc_keywords ="",
        //                sc_products ="1,2,3,4,5,6,8",
        //                sc_usrTypes ="1,3,5",
        //                sc_startDate=Convert.ToDateTime("2017-09-26 00:00:00.000"),
        //                sc_industry = "1,2",
        //                attach_risource = "",
        //                sc_status =1 },
        //        }.AsQueryable();

        //    var comm_mockSet = new Mock<DbSet<SalesCommViewModel>>();
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Provider).Returns(sales_communications.Provider);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.Expression).Returns(sales_communications.Expression);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.ElementType).Returns(sales_communications.ElementType);
        //    comm_mockSet.As<IQueryable<SalesCommViewModel>>().Setup(m => m.GetEnumerator()).Returns(sales_communications.GetEnumerator());

        //    //Get User data
        //    var risources = new List<RiSourcesViewModel>
        //        {
        //            //Rittal Users
        //            new RiSourcesViewModel {ris_ID=1, ris_headline="Rittal Chimney - Thermal Management Solution",ris_teaser="Rittal Chimney - Thermal Management Solution",ris_link="a4d9c744-8309-4d2e-9522-e0f66788118dRittal_Rittal_Chimney_-_Data_Sheet_5_3088.pdf"},
        //            new RiSourcesViewModel {ris_ID=80, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="PRL32wITCommodity0421090409.xls"},
        //            new RiSourcesViewModel {ris_ID=83, ris_headline="PRL 32 w/ IT & Commodity",ris_teaser="PRL 32 w/ IT & Commodity",ris_link="2009HolidayShutdownSchedule1209.doc"},
        //       }.AsQueryable();

        //    var ris_mockSet = new Mock<DbSet<RiSourcesViewModel>>();
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Provider).Returns(risources.Provider);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.Expression).Returns(risources.Expression);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.ElementType).Returns(risources.ElementType);
        //    ris_mockSet.As<IQueryable<RiSourcesViewModel>>().Setup(m => m.GetEnumerator()).Returns(risources.GetEnumerator());

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    var mockContext1 = new Mock<RisourceCenterMexicoEntities>();

        //    mockContext.Setup(m => m.RiSourcesViewModels).Returns(ris_mockSet.Object);
        //    mockContext.Setup(m => m.SalesCommViewModels).Returns(comm_mockSet.Object);
        //    var service = new SalesCommController(mockContext.Object, mockContext1.Object);
        //    string get_communications = await service.SearchCommuications("Schedule", userProducts, industry, audience, "yes");
        //    dynamic results = JsonConvert.DeserializeObject(get_communications);
        //    var commData = results.commData;
        //    Assert.IsTrue(0 == commData.Count);
        //}


    }
}

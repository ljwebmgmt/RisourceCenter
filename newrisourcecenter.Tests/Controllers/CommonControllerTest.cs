using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using newrisourcecenter.Controllers;
using newrisourcecenter.Models;
using System.Linq;
using Moq;
using System.Data.Entity;

namespace newrisourcecenter.Tests.Controllers
{
    [TestClass]
    public class CommonControllerTest
    {
        [TestMethod]
        public void TestMenuFilterSuperAdmin()
        {
            string industry = "3";//Industry both
            string usrType = "5";
            string products = "1,2,3";
            int id = 3;
            string siteRole = "1";//Super Admin
            var data = new List<nav2>
                {
                    new nav2 { n2ID=1, n1ID = 1,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,3"},
                    new nav2 { n2ID=2, n1ID = 3,n2_nameShort="Price and Availability",n2_nameLong="Price and Availability" ,n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=3, n1ID = 3,n2_nameShort="Submit an MDF Request",n2_nameLong="Submit an MDF Request",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=4, n1ID = 3,n2_nameShort="QuickQuote",n2_nameLong="QuickQuote",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=5, n1ID = 3,n2_nameShort="SPA Management",n2_nameLong="SPA Management",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=6, n1ID = 2,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5,6"},
                }.AsQueryable();

            var mockSet = new Mock<DbSet<nav2>>();
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<RisourceCenterMexicoEntities>();
            var mockContext1 = new Mock<RisourceCenterContext>();
            mockContext.Setup(m => m.nav2).Returns(mockSet.Object);
            var service = new CommonController(mockContext1.Object, mockContext.Object);
            var results = service.SubmenFilter(industry, usrType, products, siteRole, id) as List<nav2>;
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count == 4);
        }

        [TestMethod]
        public void TestMenuFilterIndustryIs3()
        {
            string industry = "3";//Industry both
            string usrType = "2";
            string products = "1,2,3";
            int id = 3;
            string siteRole = "2";//Super Admin
            var data = new List<nav2>
                {
                    new nav2 { n2ID=1, n1ID = 1,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,3"},
                    new nav2 { n2ID=2, n1ID = 3,n2_nameShort="Price and Availability",n2_nameLong="Price and Availability" ,n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=3, n1ID = 3,n2_nameShort="Submit an MDF Request",n2_nameLong="Submit an MDF Request",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=4, n1ID = 3,n2_nameShort="QuickQuote",n2_nameLong="QuickQuote",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=5, n1ID = 3,n2_nameShort="SPA Management",n2_nameLong="SPA Management",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=6, n1ID = 2,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5,6"},
                }.AsQueryable();

            var mockSet = new Mock<DbSet<nav2>>();
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<RisourceCenterMexicoEntities>();
            var mockContext1 = new Mock<RisourceCenterContext>();
            mockContext.Setup(m => m.nav2).Returns(mockSet.Object);
            var service = new CommonController(mockContext1.Object, mockContext.Object);
            var results = service.SubmenFilter(industry, usrType, products, siteRole, id) as List<nav2>;
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count == 4);
        }

        [TestMethod]
        public void TestMenuFilterIndustryIsNot3()
        {
            string industry = "2";//Industry both
            string usrType = "5";
            string products = "1,2,3";
            int id = 3;
            string siteRole = "2";//Super Admin
            var data = new List<nav2>
                {
                    new nav2 { n2ID=1, n1ID = 1,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,3"},
                    new nav2 { n2ID=2, n1ID = 3,n2_nameShort="Price and Availability",n2_nameLong="Price and Availability" ,n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=3, n1ID = 3,n2_nameShort="Submit an MDF Request",n2_nameLong="Submit an MDF Request",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="1",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=4, n1ID = 3,n2_nameShort="QuickQuote",n2_nameLong="QuickQuote",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="2",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=5, n1ID = 3,n2_nameShort="SPA Management",n2_nameLong="SPA Management",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="1",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=6, n1ID = 3,n2_nameShort="SPA Management",n2_nameLong="SPA Management",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5"},
                    new nav2 { n2ID=7, n1ID = 2,n2_nameShort="Enclosures",n2_nameLong="Enclosures",n2_active=1,n2_usrTypes="1,2,3,4,5",n2_industry="3",n2_products= "1,2,3,4,5,6"},
                }.AsQueryable();

            var mockSet = new Mock<DbSet<nav2>>();
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<nav2>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<RisourceCenterMexicoEntities>();
            var mockContext1 = new Mock<RisourceCenterContext>();
            mockContext.Setup(m => m.nav2).Returns(mockSet.Object);
            var service = new CommonController(mockContext1.Object, mockContext.Object);
            var results = service.SubmenFilter(industry, usrType, products, siteRole, id) as List<nav2>;
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count == 3);
        }
    }
}

using System;
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
    public class RiSources
    {
        //[TestMethod]
        //public void Add_ReSources_Files_ToCart_TestAsync()
        //{
        //    int list_ris_IDs = 3;

        //    var RiSourcesCarts_model = new List<RiSourcesCarts>
        //    {
        //        new RiSourcesCarts { ID=0, ris_ID=1,user_id=40 },
        //        new RiSourcesCarts{ID=1, ris_ID=2,user_id=18127}
        //    }.AsQueryable();

        //    var mockSet = new Mock<DbSet<RiSourcesCarts>>();
        //   // mockSet.Setup(m => m.Create()).Returns(new RiSourcesCarts());
        //    mockSet.As<IQueryable<RiSourcesCarts>>().Setup(m => m.GetEnumerator()).Returns(RiSourcesCarts_model.GetEnumerator());

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    mockContext.Setup(m => m.RiSourcesCarts).Returns(mockSet.Object);
        //    mockContext.Setup(m => m.Set<RiSourcesCarts>()).Returns(mockSet.Object);
        //    mockContext.Setup(m=>m.SaveChanges()).Returns(1);

        //    var service = new RiSourcesController(mockContext.Object);
        //    var get_sales_reps = service.AddReSourcesFilesToCart(list_ris_IDs, "yes");

        //    mockSet.Verify(m => m.Add(It.IsAny<RiSourcesCarts>()), Times.Once);
        //    mockContext.Verify(m => m.SaveChanges(),Times.Once);

        //    dynamic results = JsonConvert.DeserializeObject(get_sales_reps);
        //    //var first_id = results.risourcesModel[0].ID.ToString();
        //    //var spaaccountmanager = results.spaaccountmanager;

        //    Assert.IsNotNull(results);
        //    //Assert.AreEqual("0", first_id);
        //    //Assert.IsTrue(2 == results.risourcesModel.Count);
        //}

        //[TestMethod]
        //public void Delete_ReSources_Files_ToCart_TestAsync()
        //{
        //    int list_ris_IDs = 1;

        //    var RiSourcesCarts_model = new List<RiSourcesCarts>
        //    {
        //        new RiSourcesCarts { ID=0, ris_ID=1,user_id=40 },
        //        new RiSourcesCarts{ID=1, ris_ID=2,user_id=18127},
        //        new RiSourcesCarts{ID=2, ris_ID=2,user_id=18127},
        //        new RiSourcesCarts{ID=3, ris_ID=2,user_id=18127}
        //   };

        //    var mockSet = new Mock<DbSet<RiSourcesCarts>>();
        //    // mockSet.Setup(m => m.Create()).Returns(new RiSourcesCarts());
        //    mockSet.As<IQueryable<RiSourcesCarts>>().Setup(m => m.GetEnumerator()).Returns(RiSourcesCarts_model.GetEnumerator());
        //    mockSet.Setup(m => m.Remove(It.IsAny<RiSourcesCarts>())).Callback<RiSourcesCarts>((entity)=> RiSourcesCarts_model.Remove(new RiSourcesCarts { ID = 1, ris_ID = 2, user_id = 18127 }));

        //    var mockContext = new Mock<RisourceCenterContext>();
        //    mockContext.Setup(m => m.RiSourcesCarts).Returns(mockSet.Object);
        //    //mockContext.Setup(m => m.Set<RiSourcesCarts>()).Returns(mockSet.Object);
        //    //mockContext.Setup(m => m.SaveChanges()).Returns(1);

        //    var service = new RiSourcesController(mockContext.Object);
        //    var get_sales_reps = service.DeleteReSourcesFilesToCart(list_ris_IDs, "yes");

        //    mockSet.Verify(m => m.Remove(It.IsAny<RiSourcesCarts>()), Times.Exactly(1));
        //    mockContext.Verify(m => m.SaveChanges(), Times.Once());

        //    dynamic results = JsonConvert.DeserializeObject(get_sales_reps);
        //    var first_id = results.risourcesModel[0].ID.ToString();
        //    //var spaaccountmanager = results.spaaccountmanager;

        //    //Assert.IsNotNull(results);
        //    //Assert.AreEqual("0", first_id);
        //   // Assert.IsTrue(2 == results.risourcesModel.Count);
        //}


        [TestMethod]
        public void getReSourcesFiles_ToCart_TestAsync()
        {

        }

    }
}

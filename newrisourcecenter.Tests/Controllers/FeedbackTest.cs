using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using newrisourcecenter.Controllers;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace newrisourcecenter.Tests.Controllers
{
    [TestClass]
    public class FeedbackTest
    {
        [TestMethod]
        public void Feedback()
        {
            // Arrange
            FeedbackController controller = new FeedbackController();

            // Act
            ViewResult result = controller.Index() as ViewResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Please provide feedback below. Your comments will be completely anonymous", result.ViewBag.Message);
        }

        [TestMethod]
        public void FeedbackSubmit()
        {
            // Arrange
            HomeController controller = new HomeController();
            string email_body = "This is the email body";

            var service = new FeedbackController();
            var get_sales_reps = service.SubmitFeedback(email_body);
            dynamic results = JsonConvert.DeserializeObject(get_sales_reps);
            var message = results.message.ToString();

            Assert.IsNotNull(results);
            Assert.AreEqual("Thanks for your feedback!", message);
        }
    }
}

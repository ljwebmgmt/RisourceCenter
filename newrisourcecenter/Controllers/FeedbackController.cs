using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    public class FeedbackController : Controller
    {
        // GET: Feedback
        public ActionResult Index()
        {
            ViewBag.Message = "Please provide feedback below. Your comments will be completely anonymous";

            return View();
        }

        [HttpPost]
        public string SubmitFeedback(string email_body)
        {
            string message = "Thanks for your feedback!";
            string From = "webmaster@rittal.us";
            string To = "corbolotti.m@rittal.us,rittal@rittal.us";
            //string To = "antwi.s@rittal.us,rittal@rittal.us,rittaltest1@rittal.us";
            var locController = new CommonController();
            locController.email(From, To, "Anonymous Feedback", email_body,"no");

            var restunedString = JsonConvert.SerializeObject(new { message = message });

            return restunedString;
        }

        public string getUserName()
        {
            string user = User.Identity.Name;
            var restunedString = JsonConvert.SerializeObject(new { message = user });

            return restunedString;
        }
    }
}
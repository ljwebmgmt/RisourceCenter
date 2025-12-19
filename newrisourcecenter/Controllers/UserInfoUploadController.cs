using newrisourcecenter.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    public class UserInfoUploadController : Controller
    {
        // GET: UserInfoUpload
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            RemoveUnregisteredJob job = new RemoveUnregisteredJob();

            job.UploadUserInfotoSTFP();

            return RedirectToAction("Index", "Home");
        }

    }
}

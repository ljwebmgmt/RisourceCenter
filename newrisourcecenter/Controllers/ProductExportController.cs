using newrisourcecenter.Models;
using newrisourcecenter.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class ProductExportController : Controller
    {
        private RisourceCenterMexicoEntities db = null;
        public ProductExportController()
        {
            db = new RisourceCenterMexicoEntities();
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            ProductExportModel viewModel = new ProductExportModel();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.rittal.com/us_en/apps/services/product_apis.php");
                HttpResponseMessage response = await client.GetAsync("?type=category");
                if (response.IsSuccessStatusCode)
                {
                    var categories = response.Content.ReadAsStringAsync().Result;
                    viewModel = JsonConvert.DeserializeObject<ProductExportModel>(categories);
                }
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Index([Bind(Include = "category,subcategory")] ProductExportInputModel model)
        {
            ProductExportResponse responseModel = new ProductExportResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception(string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://www.rittal.com/us_en/apps/services/product_apis.php");
                    HttpResponseMessage response = await client.GetAsync("?category=" + model.category + (!string.IsNullOrEmpty(model.subcategory) ? "&subcategory=" + model.subcategory : "") + "&type=download");
                    if (response.IsSuccessStatusCode)
                    {
                        responseModel = JsonConvert.DeserializeObject<ProductExportResponse>(response.Content.ReadAsStringAsync().Result);
                        if (string.IsNullOrEmpty(responseModel.error) && string.IsNullOrEmpty(responseModel.fileUrl))
                        {
                            throw new Exception("An error occurred while processing your request.");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                responseModel.error = ex.Message;
            }
            return Json(responseModel);
        }
    }
}
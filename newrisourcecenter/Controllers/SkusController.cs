using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;

namespace newrisourcecenter.Controllers
{
    public class SkusController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: Skus
        public ActionResult Index()
        {
            return View(db.SPAMaterialMasterViewModels.OrderByDescending(a=>a.ID).ToList());
        }

        // GET: Skus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAMaterialMasterViewModel skusViewModel = db.SPAMaterialMasterViewModels.Find(id);
            if (skusViewModel == null)
            {
                return HttpNotFound();
            }
            return View(skusViewModel);
        }

        // GET: Skus/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Skus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,material,material_description,mpg,mpg_description,list_price,cost")] SPAMaterialMasterViewModel skusViewModel)
        {
            if (ModelState.IsValid)
            {
                db.SPAMaterialMasterViewModels.Add(skusViewModel);
                db.SaveChanges();

                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "Your item has been added" });
            }

            return View(skusViewModel);
        }

        // GET: Skus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAMaterialMasterViewModel skusViewModel = db.SPAMaterialMasterViewModels.Find(id);
            if (skusViewModel == null)
            {
                return HttpNotFound();
            }
            return View(skusViewModel);
        }

        // POST: Skus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,material,material_description,mpg,mpg_description,list_price,cost")] SPAMaterialMasterViewModel skusViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(skusViewModel).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", new { n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"], success = "Your item has been updated" });
            }
            return View(skusViewModel);
        }

        [HttpPost]
        public ActionResult UploadSKUdata(HttpPostedFileBase attachment)
        {
            //attach a file to the risources
            if (attachment != null && attachment.ContentLength > 0)
            {
                var fileName = Path.GetFileName(attachment.FileName);
                var path = Path.Combine(Server.MapPath("~/attachments/spa/files"), fileName);
                string ext = Path.GetExtension(fileName);
                attachment.SaveAs(path);
                if (ext == ".csv")
                {
                    var reader = new StreamReader(path);
                    if (System.IO.File.Exists(path))
                    {
                        //Parse Open Ended file
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var columns = line.Split(',');
                            if (columns[0] != "material")
                            {
                                string material = columns[0];
                                string material_description = columns[1];
                                string mpg = columns[2];
                                string mpg_description = columns[3];
                                string cost = columns[4];
                                string list_price = columns[5];
                                if (material != "")
                                {
                                    string skus = material;
                                    var dup_sku = db.SPAMaterialMasterViewModels.Where(a => a.material == skus);
                                    if (dup_sku.Count() != 0)
                                    {
                                        SPAMaterialMasterViewModel sku_new = dup_sku.FirstOrDefault();
                                        //if id exists update
                                        sku_new.material = material;
                                        sku_new.mpg = mpg;
                                        sku_new.mpg_description = mpg_description;
                                        sku_new.material_description = material_description;
                                        sku_new.list_price = list_price;
                                        sku_new.cost = cost;

                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        //if id does not exist insert
                                        SPAMaterialMasterViewModel sku_new = new SPAMaterialMasterViewModel();
                                        sku_new.material = material;
                                        sku_new.mpg = mpg;
                                        sku_new.mpg_description = mpg_description;
                                        sku_new.material_description = material_description;
                                        sku_new.list_price = list_price;
                                        sku_new.cost = cost;

                                        db.SPAMaterialMasterViewModels.Add(sku_new);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], success="You can only upload CSV file" });
                }
            }

            return RedirectToAction("Index", new { n1_name = Request.Form["n1_name"], n2_name = Request.Form["n2_name"], success = "The file has been uploaded" });
        }

        [HttpGet]
        public async Task<ActionResult> ExportSKUdata()
        {
            List<SPAMaterialMasterViewModel> spa_skus = await db.SPAMaterialMasterViewModels.ToListAsync();
            StringBuilder dataSource = new StringBuilder();
            dataSource.AppendLine("material, material_description, mpg, mpg_description, cost, list_price");
            foreach (var skus in spa_skus)
            {
                dataSource.AppendLine("" + skus.material + "," + skus.material_description + "," + skus.mpg + "," + skus.mpg_description + ","+skus.cost+"," + skus.list_price + "");
            }

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachement;filename=spa_skus_data.csv");
            Response.ContentType = "text/csv";
            Response.Output.Write(dataSource);
            Response.Flush();
            Response.End();

            return View();
        }

        // GET: Skus/Delete/5
        public ActionResult Deletefiles()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            List<SPAMaterialMasterViewModel> skusViewModel = db.SPAMaterialMasterViewModels.ToList();
            if (skusViewModel == null)
            {
                return HttpNotFound();
            }
            db.SPAMaterialMasterViewModels.RemoveRange(skusViewModel);
            db.SaveChanges();

            //Log the action by the user
            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"],success="All the items have been deleted" });
        }

        // GET: Nav1ViewModel/Delete/5
        public ActionResult Delete(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPAMaterialMasterViewModel skusViewModel = db.SPAMaterialMasterViewModels.Find(id);
            if (skusViewModel == null)
            {
                return HttpNotFound();
            }
            db.SPAMaterialMasterViewModels.Remove(skusViewModel);
            db.SaveChanges();

            //Log the action by the user
            return RedirectToAction("Index", new { n2Id = Request.Form["n2ID"], n1_name = Request.QueryString["n1_name"], n2_name = Request.QueryString["n2_name"],success="Your item has been deleted" });
        }

        // POST: Skus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SkusViewModel skusViewModel = db.SkusViewModels.Find(id);
            db.SkusViewModels.Remove(skusViewModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

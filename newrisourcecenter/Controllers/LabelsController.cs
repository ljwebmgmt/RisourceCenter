using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace newrisourcecenter.Models
{
    public class LabelsController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();

        // GET: Labels
        public async Task<ActionResult> Index()
        {
            return View(await db.LabelsModels.ToListAsync());
        }

        // GET: Labels/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LabelsModel labelsModel = await db.LabelsModels.FindAsync(id);
            if (labelsModel == null)
            {
                return HttpNotFound();
            }
            return View(labelsModel);
        }

        // GET: Labels/Create
        public ActionResult Create()
        {
            LabelsModel labelsModel = new LabelsModel();
            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> UserCountries = new List<SelectListItem>();
            UserCountries.Add(new SelectListItem { Text = "Select a Country", Value = "Select", Selected = true });
            foreach (var country in countries.OrderBy(a=>a.Language))
            {
                if (country.country_id!=38)
                {
                    UserCountries.Add(new SelectListItem { Text = country.Language, Value = country.country_id.ToString() });
                }
            }
            labelsModel.list_country_ids = UserCountries;

            return View(labelsModel);
        }

        // POST: Labels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "label_id,label_name,controller_name,page_name,translated_label,language,status,date")] LabelsModel labelsModel)
        {
            if (ModelState.IsValid)
            {
                db.LabelsModels.Add(labelsModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(labelsModel);
        }

        // GET: Labels/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LabelsModel labelsModel = await db.LabelsModels.FindAsync(id);
            if (labelsModel == null)
            {
                return HttpNotFound();
            }
            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> UserCountries = new List<SelectListItem>();
            UserCountries.Add(new SelectListItem { Text = "Select a Country", Value = "Select", Selected = true });
            foreach (var country in countries.OrderBy(a => a.Language))
            {
                if (country.country_id != 38)
                {
                    UserCountries.Add(new SelectListItem { Text = country.Language, Value = country.country_id.ToString() });
                }
            }
            labelsModel.list_country_ids = UserCountries;

            return View(labelsModel);
        }

        // POST: Labels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "label_id,label_name,controller_name,page_name,translated_label,language,status,date")] LabelsModel labelsModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(labelsModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(labelsModel);
        }

        // GET: Labels/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LabelsModel labelsModel = await db.LabelsModels.FindAsync(id);
            if (labelsModel == null)
            {
                return HttpNotFound();
            }
            return View(labelsModel);
        }

        // POST: Labels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LabelsModel labelsModel = await db.LabelsModels.FindAsync(id);
            db.LabelsModels.Remove(labelsModel);
            await db.SaveChangesAsync();
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

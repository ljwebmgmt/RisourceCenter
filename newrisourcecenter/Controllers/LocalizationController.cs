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
    public class LocalizationController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        private RisourceCenterMexicoEntities dbEntity = new RisourceCenterMexicoEntities();

        // GET: Localization
        public async Task<ActionResult> Index()
        {
            return View(await db.LocalizationModels.ToListAsync());
        }

        // GET: Localization/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocalizationModel localizationModel = await db.LocalizationModels.FindAsync(id);
            if (localizationModel == null)
            {
                return HttpNotFound();
            }
            return View(localizationModel);
        }

        // GET: Localization/Create
        public ActionResult Create(string tableName="", string columnName="", string message="", int parent_id=0, int lang=0)
        {
            LocalizationModel localizeModel = new LocalizationModel();
            //set the users country
            var countries = dbEntity.countries.Where(a => a.Language != null).OrderBy(a => a.country_long);
            List<SelectListItem> UserCountries = new List<SelectListItem>();
            UserCountries.Add(new SelectListItem { Text = "Select a Country", Value = "Select", Selected = true });
            foreach (var country in countries)
            {
                var localize = db.LocalizationModels.Where(a => a.column_name == columnName && a.parent_id == parent_id && a.language==country.country_id).ToList();
                if (localize.Count()==0)
                {
                    if (country.country_id != 38)
                    {
                        if (country.country_id != lang)
                        {
                            UserCountries.Add(new SelectListItem { Text = country.Language, Value = country.country_id.ToString() });
                        }
                    }
                }
            }

            //set the users language
            List<SelectListItem> Edit_lang = new List<SelectListItem>();
            Edit_lang.Add(new SelectListItem { Text = "Select a Language", Value = "Select", Selected = true });
            foreach (var country in countries)
            {
                var localize = db.LocalizationModels.Where(a => a.column_name == columnName && a.parent_id == parent_id && a.language == country.country_id).ToList();
                if (localize.Count() != 0)
                {
                    if (country.country_id != 38)
                    {
                       Edit_lang.Add(new SelectListItem { Text = country.Language, Value = localize.FirstOrDefault().localization_id.ToString() });
                    }
                }
            }
            //get sales comm for default message if it is not passed in the link
            if (message==null || message=="")
            {
                var salesComm = db.SalesCommViewModels.Where(a=>a.scID==parent_id);
                if (columnName=="sc_body")
                {
                    localizeModel.message_original = salesComm.FirstOrDefault().sc_body;
                }else if (columnName == "sc_headline")
                {
                    localizeModel.message_original = salesComm.FirstOrDefault().sc_headline;
                }
                else if (columnName == "sc_teaser")
                {
                    localizeModel.message_original = salesComm.FirstOrDefault().sc_teaser;
                }else{
                    localizeModel.message_original = message;
                }
            }
            else
            {
                localizeModel.message_original = message;
            }
            localizeModel.table_name = tableName;
            localizeModel.parent_id = Convert.ToByte(parent_id);
            localizeModel.column_name = columnName;
            localizeModel.list_country_ids = UserCountries;  
            localizeModel.edit_lang_ids = Edit_lang;

            return View(localizeModel);
        }

        // POST: Localization/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "localization_id,table_name,parent_id,column_name,message_original,message_translated,language,status,date")] LocalizationModel localizationModel)
        {
            if (ModelState.IsValid)
            {
                db.LocalizationModels.Add(localizationModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(localizationModel);
        }

        // GET: Localization/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocalizationModel localizationModel = await db.LocalizationModels.FindAsync(id);
            if (localizationModel == null)
            {
                return HttpNotFound();
            }
            return View(localizationModel);
        }

        // POST: Localization/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "localization_id,table_name,parent_id,column_name,message_original,message_translated,language,status,date")] LocalizationModel localizationModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(localizationModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(localizationModel);
        }

        // GET: Localization/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocalizationModel localizationModel = await db.LocalizationModels.FindAsync(id);
            if (localizationModel == null)
            {
                return HttpNotFound();
            }
            return View(localizationModel);
        }

        // POST: Localization/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            LocalizationModel localizationModel = await db.LocalizationModels.FindAsync(id);
            db.LocalizationModels.Remove(localizationModel);
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

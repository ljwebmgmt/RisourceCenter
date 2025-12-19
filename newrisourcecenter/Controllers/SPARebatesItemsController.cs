using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public class SPARebatesItemsController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: SPARebatesItems
        public async Task<ActionResult> Index()
        {
            return View(await db.SPARebatesItemsViewModels.ToListAsync());
        }

        // GET: SPARebatesItems/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPARebatesItemsViewModel sPARebatesItemsViewModel = await db.SPARebatesItemsViewModels.FindAsync(id);
            if (sPARebatesItemsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sPARebatesItemsViewModel);
        }

        // GET: SPARebatesItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SPARebatesItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "rebateItem_ID,rebate_ID,contract_ID,sku,last_price,spa_price,quantity_rebated,quantity_requested,capping_status,current_rebate,distributor_requests,original_difference,invoice_date,invoice_number,rebate_amount,reason,status")] SPARebatesItemsViewModel sPARebatesItemsViewModel)
        {
            if (ModelState.IsValid)
            {
                db.SPARebatesItemsViewModels.Add(sPARebatesItemsViewModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(sPARebatesItemsViewModel);
        }

        // GET: SPARebatesItems/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPARebatesItemsViewModel sPARebatesItemsViewModel = await db.SPARebatesItemsViewModels.FindAsync(id);
            if (sPARebatesItemsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sPARebatesItemsViewModel);
        }

        // POST: SPARebatesItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "rebateItem_ID,rebate_ID,contract_ID,sku,last_price,spa_price,quantity_rebated,quantity_requested,capping_status,current_rebate,distributor_requests,original_difference,invoice_date,invoice_number,rebate_amount,reason,status")] SPARebatesItemsViewModel sPARebatesItemsViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sPARebatesItemsViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sPARebatesItemsViewModel);
        }

        // GET: SPARebatesItems/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPARebatesItemsViewModel sPARebatesItemsViewModel = await db.SPARebatesItemsViewModels.FindAsync(id);
            if (sPARebatesItemsViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sPARebatesItemsViewModel);
        }

        // POST: SPARebatesItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SPARebatesItemsViewModel sPARebatesItemsViewModel = await db.SPARebatesItemsViewModels.FindAsync(id);
            db.SPARebatesItemsViewModels.Remove(sPARebatesItemsViewModel);
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

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
using Newtonsoft.Json;
using System.Text;

namespace newrisourcecenter.Controllers
{
    public class SPARebatesController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();
        SPAController spacontroller = new SPAController();

        // GET: SPARebates
        public async Task<ActionResult> Index()
        {
            List<SPARebatesViewModel> spa_rebates = await db.SPARebatesViewModels.ToListAsync();
            return View(spa_rebates);
        }

        // GET: SPARebates/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPARebatesViewModel sPARebatesViewModel = await db.SPARebatesViewModels.FindAsync(id);
            if (sPARebatesViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sPARebatesViewModel);
        }

        // GET: SPARebates/Create
        public async Task<ActionResult> Create()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long compaId = Convert.ToInt64(Session["companyId"]);
            List<partnerCompanyViewModel> compdata;
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "", Text = "Select A Company", Selected=true });

            if (User.IsInRole("Global User") || User.IsInRole("Super Admin"))
            {
                compdata = await db.partnerCompanyViewModels.ToListAsync();
            }
            else
            {
                compdata = await db.partnerCompanyViewModels.Where(a => a.comp_ID == compaId).ToListAsync();
            }

            foreach (var item in compdata)
            {
                list.Add(new SelectListItem { Value=item.comp_ID.ToString(),Text=item.comp_name}); 
            }

            SPARebatesViewModel spa_rebates = new SPARebatesViewModel();
            spa_rebates.list_comp = list;

            return View(spa_rebates);
        }
        
        public async Task<string> getLocations(int id)
        {
            var Locations = await db.partnerLocationViewModels.Where(a=>a.comp_ID==id).ToListAsync();
            var restunedString = JsonConvert.SerializeObject(new { locations = Locations, status = "OK" });

            return restunedString;
        }

        public async Task<string> getSPAContracts(string id)
        {
            var spa_contracts = await db.SPAViewModels.Where(a => a.Distributor_location == id).ToListAsync();
            foreach (var item in spa_contracts)
            {
                var spaitems = await db.SPAItemViewModels.Where(a=>a.Form_id==item.Spa_id).ToListAsync();
                item.count = spaitems.Count();
            }
            var restunedString = JsonConvert.SerializeObject(new { spa_contracts = spa_contracts, status = "OK" });

            return restunedString;
        }

        public async Task<string> getSPAContractItems(int id)
        {
            var spa_items = await db.SPAItemViewModels.Where(a => a.Form_id==id).ToListAsync();
            var restunedString = JsonConvert.SerializeObject(new { spa_items = spa_items, status = "OK" });

            return restunedString;
        }

        [HttpPost]
        public async Task<string> ValidateSPAContractItems(List<ItemsList> itemsList)
        {
            //itemsList = new List<ItemsList>();
            //itemsList.Add(new ItemsList { contract_id = 1, customer_invoice_num = "3243", invoice_date = "1/2/2017", position = "0", quantity_requested = "5", rittal_invoice_num = "3432", sku = "1005660" });
            //itemsList.Add(new ItemsList { contract_id = 1, customer_invoice_num = "3243", invoice_date = "1/2/2017", position = "1", quantity_requested = "10", rittal_invoice_num = "3432", sku = "1006600" });
            //itemsList.Add(new ItemsList { contract_id = 1, customer_invoice_num = "3243", invoice_date = "1/2/2017", position = "2", quantity_requested = "6", rittal_invoice_num = "3432", sku = "1005500" });
            //itemsList.Add(new ItemsList { contract_id = 1, customer_invoice_num = "3243", invoice_date = "1/2/2017", position = "3", quantity_requested = "10", rittal_invoice_num = "3432", sku = "8018899" });

            List<SPARebatesItemsViewModel> rebates_items = new List<SPARebatesItemsViewModel>();
            SPARebatesViewModel rebates = new SPARebatesViewModel();
            List<List_Results> list_results = new List<List_Results>();
            var contract_id = itemsList.FirstOrDefault().contract_id;
            bool not_passed = true;

            foreach (var item in itemsList)
            {
                string validate_results = await ValidateSPAContractItems(item.contract_id, item.sku, item.quantity_requested, item.invoice_date);
                list_results.Add(new List_Results { position=item.position, msg= validate_results });
                if(!string.IsNullOrEmpty(validate_results))
                {
                    not_passed = false;
                }
            }

            if (not_passed)
            {
                if (string.IsNullOrEmpty(itemsList.FirstOrDefault().rebate_id))
                {
                    rebates = new SPARebatesViewModel
                    {
                        contract_ID = contract_id,
                        status = "create",
                        submit_date = DateTime.Today,
                        rebate_total_amount = "1232",                        
                    };
                    db.SPARebatesViewModels.Add(rebates);
                    await db.SaveChangesAsync();
                }
                else
                {
                    rebates.rebate_ID = Convert.ToInt32(itemsList.FirstOrDefault().rebate_id);
                }

                foreach (var item in itemsList)
                {
                    ListPriceAmount listPriceAmount = await CalculateItemPrice(itemsList.FirstOrDefault().contract_id,item.sku,Convert.ToInt32(item.quantity_requested));

                    rebates_items.Add(new SPARebatesItemsViewModel
                    {
                        rebate_ID = rebates.rebate_ID,
                        contract_ID = contract_id,
                        sku = item.sku,
                        quantity_requested = Convert.ToInt32(item.quantity_requested),
                        rittal_invoice_number = item.rittal_invoice_num,
                        customer_invoice_number = item.customer_invoice_num,
                        invoice_date = Convert.ToDateTime(item.invoice_date),
                        rebate_amount = listPriceAmount.amount.ToString(),
                        spa_price = Convert.ToDouble(listPriceAmount.list_price),
                        status = "create"
                    });
                }

                db.SPARebatesItemsViewModels.AddRange(rebates_items);
                await db.SaveChangesAsync();
            }
            //Get rebated items
            var rebateItems = await getSPARebateItems(rebates.rebate_ID, contract_id,not_passed);

            var restunedString = JsonConvert.SerializeObject(new { list_results=list_results, status_msg = not_passed, rebate_id=rebates.rebate_ID, rebateItems = rebateItems });

            return restunedString;
        }

        public async Task<ListPriceAmount> CalculateItemPrice(int contract_id, string sku,int quantity)
        {
            //Get the SPA contact and extract the location 
            SPAViewModels spa_model = await db.SPAViewModels.FindAsync(contract_id);
            int location_id = Convert.ToInt32(spa_model.Distributor_location);
            //Get the price grouping from the distributors locations
            partnerLocationViewModel partner_location = await db.partnerLocationViewModels.FindAsync(location_id);
            string pricing_group = partner_location.price_group;

            //Get part number data
            var material = await db.SPAMaterialMasterViewModels.Where(a => a.material == sku).FirstOrDefaultAsync();
            if (material==null) {
                return new ListPriceAmount();
            }
            string mpg = material.mpg;
            var getmultiplier = await spacontroller.GetMultiplier(pricing_group, mpg);
            //formular for calculated rebate
            double amount = Convert.ToDouble(getmultiplier.Split(',')[0]) * Convert.ToDouble(material.list_price)*quantity;

            var listdata = new ListPriceAmount { amount=amount,list_price = material.list_price};

            return listdata;
        }

        async Task<List<SPARebatesItemsViewModel>> getSPARebateItems(int rebate_id,int contract_id,bool not_pass=true)
        {
            List<SPARebatesItemsViewModel> rebates_item = new List<SPARebatesItemsViewModel>();
            List<SPARebatesItemsViewModel> rebates_items = await db.SPARebatesItemsViewModels.Where(a=>a.rebate_ID == rebate_id).ToListAsync();

            foreach (var item in rebates_items)
            {
                string invoice_date = string.Format("{0:MM/dd/yyy}",item.invoice_date);

                string validate_results = await ValidateContractItems(item.contract_ID, item.sku, item.quantity_requested, item.invoice_date.ToString());
                if (!string.IsNullOrEmpty(validate_results))
                {
                    item.status = validate_results;
                }

                rebates_item.Add(new SPARebatesItemsViewModel { rebateItem_ID=item.rebateItem_ID ,rebate_ID = item.rebate_ID, contract_ID=item.contract_ID,sku=item.sku,distributor_requests=item.distributor_requests,quantity_requested=item.quantity_requested,rittal_invoice_number=item.rittal_invoice_number,customer_invoice_number=item.customer_invoice_number,invoicedate=invoice_date,status=item.status,rebate_amount=item.rebate_amount,spa_price=item.spa_price });            
            }

            //Update Rebates table with  total rebates
            if (not_pass)
            {
                await UpdateTotalRebates(rebate_id);
            }

            return rebates_item;
        }

        async Task<string> ValidateSPAContractItems(int contract_id, string sku, int quantity, string invoice_date)
        {
            StringBuilder results = new StringBuilder();
            var getContract = await db.SPAViewModels.FindAsync(contract_id);
            if (getContract.End_date < Convert.ToDateTime(invoice_date))
            {
                results.Append("<br /><span style=\"color:red;\" >The contract has expired. You cannot submit a rebate on an expired contract</span>");
            }
            var checkSku = await db.SPAMaterialMasterViewModels.Where(a=>a.material==sku).FirstOrDefaultAsync();
            if (checkSku==null)
            {
                results.Append("<br /><span style=\"color:blue;\" >The sku number does not exist</span>");
            }

            return results.ToString();
        }

        async Task<string> ValidateContractItems(int contract_id, string sku, int quantity, string invoice_date)
        {
            StringBuilder results = new StringBuilder();
            var getContract = await db.SPAViewModels.FindAsync(contract_id);
            if (getContract.End_date < Convert.ToDateTime(invoice_date))
            {
                results.Append("<br /><span style=\"color:red;\" >The contract has expired. You cannot submit a rebate on an expired contract</span>");
            }

            //Check if SKU is valid
            var isValidSku = await db.SPAMaterialMasterViewModels.Where(a => a.material == sku).FirstOrDefaultAsync();
            if (isValidSku == null)
            {
                results.Append("<br /><span style=\"color:pink;\" >The sku number is not valid</span>");
            }
            else
            {
                var checkSku = await db.SPAItemViewModels.Where(a => a.Sku == sku && a.Form_id == contract_id).FirstOrDefaultAsync();
                if (checkSku == null)
                {
                    results.Append("<br /><span style=\"color:blue;\" >The sku number does not exist on the contract</span>");
                }

                if (checkSku.Quantity != quantity.ToString())
                {
                    results.Append("<br /><span style=\"color:violet;\" >The quantity does not match what is on the contract. Correct this amount to submit</span>");
                }
            }
            return results.ToString();
        }

        // POST: SPARebates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<string> UpdateItem(ItemsList itemslist)
        {
            int id = Convert.ToInt32(itemslist.rebateItem_id);
            var getRebateItem = db.SPARebatesItemsViewModels.Find(id);
            getRebateItem.rittal_invoice_number = itemslist.rittal_invoice_num;
            getRebateItem.customer_invoice_number = itemslist.customer_invoice_num;
            getRebateItem.sku = itemslist.sku;
            getRebateItem.invoice_date = Convert.ToDateTime(itemslist.invoice_date);
            getRebateItem.quantity_requested = Convert.ToInt32(itemslist.quantity_requested);
            getRebateItem.status = "updated";

            List<List_Results> list_results = new List<List_Results>();
            string validate_results = await ValidateContractItems(itemslist.contract_id, itemslist.sku, itemslist.quantity_requested, itemslist.invoice_date);

            //Calculate the total rebate
            ListPriceAmount listPriceAmount = await CalculateItemPrice(getRebateItem.contract_ID, itemslist.sku, Convert.ToInt32(itemslist.quantity_requested));
            getRebateItem.rebate_amount = listPriceAmount.amount.ToString();
            await db.SaveChangesAsync();

            //Update Rebates table with  total rebates
            await UpdateTotalRebates(Convert.ToInt32(itemslist.rebate_id));

            if (!string.IsNullOrEmpty(validate_results))
            {
                return validate_results + "|" + listPriceAmount.amount.ToString();
            }
            else
            {
                return  getRebateItem.status + "|" + listPriceAmount.amount.ToString();
            }
        }

        async Task UpdateTotalRebates(int id)
        {
            double total_rebate = 0;
            int item_id = Convert.ToInt32(id);
            List<SPARebatesItemsViewModel> getRebateItems = await db.SPARebatesItemsViewModels.Where(a => a.rebate_ID == item_id).ToListAsync();
            foreach (var item in getRebateItems)
            {
                total_rebate = total_rebate + Convert.ToDouble(item.rebate_amount);
            }
            SPARebatesViewModel rebate = await db.SPARebatesViewModels.FindAsync(id);
            rebate.rebate_total_amount = total_rebate.ToString();
            await db.SaveChangesAsync();
        }

        // GET: SPARebates/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            long compaId = Convert.ToInt64(Session["companyId"]);
            List<partnerCompanyViewModel> compdata;
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "", Text = "Select A Company", Selected = true });

            if (User.IsInRole("Global User") || User.IsInRole("Super Admin"))
            {
                compdata = await db.partnerCompanyViewModels.ToListAsync();
            }
            else
            {
                compdata = await db.partnerCompanyViewModels.Where(a => a.comp_ID == compaId).ToListAsync();
            }

            foreach (var item in compdata)
            {
                list.Add(new SelectListItem { Value = item.comp_ID.ToString(), Text = item.comp_name });
            }

            //SPARebatesViewModel spa_rebates = new SPARebatesViewModel();
            SPARebatesViewModel spa_rebates = await db.SPARebatesViewModels.FindAsync(id);
            spa_rebates.list_comp = list;
            List<SPARebatesItemsViewModel> spa_rebates_items = await db.SPARebatesItemsViewModels.Where(a => a.rebate_ID == id).ToListAsync();
            foreach (var item in spa_rebates_items)
            {
                string validate_results = await ValidateContractItems(item.contract_ID, item.sku, item.quantity_requested, item.invoice_date.ToString());
                if (!string.IsNullOrEmpty(validate_results))
                {
                    item.status = validate_results;
                }
            }
            spa_rebates.spa_rebates_items = spa_rebates_items;
            return View(spa_rebates);
        }

        // POST: SPARebates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "rebate_ID,contract_ID,rebate_total_amount,credit_mome,status,submit_date,memo_date")] SPARebatesViewModel sPARebatesViewModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sPARebatesViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sPARebatesViewModel);
        }

        // GET: SPARebates/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SPARebatesViewModel sPARebatesViewModel = await db.SPARebatesViewModels.FindAsync(id);
            if (sPARebatesViewModel == null)
            {
                return HttpNotFound();
            }
            return View(sPARebatesViewModel);
        }

        public async Task<string> DeleteRebate(int id, int rebate_id)
        {
            SPARebatesItemsViewModel sPARebatesItemsViewModel = await db.SPARebatesItemsViewModels.FindAsync(id);
            db.SPARebatesItemsViewModels.Remove(sPARebatesItemsViewModel);
            await db.SaveChangesAsync();

            //Update Rebates table with  total rebates
            await UpdateTotalRebates(Convert.ToInt32(rebate_id));

            return "OK";
        }

        // POST: SPARebates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            SPARebatesViewModel sPARebatesViewModel = await db.SPARebatesViewModels.FindAsync(id);
            db.SPARebatesViewModels.Remove(sPARebatesViewModel);
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

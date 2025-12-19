using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Net;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using System.Configuration;

namespace newrisourcecenter.Controllers
{ 
   
    public class LargeEnclosureController : Controller
    {
        private RisourceCenterContext db = new RisourceCenterContext();

        // GET: LargeEnclosure
        public ActionResult Index()
        {
            string username = User.Identity.GetUserName();
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            Session["guid"] = "";

            return View();
        }

        // GET: LargeEnclosure
        public ActionResult Externallink()
        {
            return View();
        }

        #region Accessories
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public ActionResult Accessories(string bayingType = "", string height = "", string articlenumber = "")
        {
            //Check to see if accessories can be added
            return View();
        }

        public  string CheckAccessories(string type="")
        {
            string guid = Convert.ToString(Session["guid"]);
            var myaccessories = db.LargeEnclosureMyaccessories.Where(a=>a.Code==guid && a.Baying_NotBaying=="1");
            var Fmds = myaccessories.Where(model=>model.Type=="FMD");
            var TS8 = myaccessories.Where(model => model.Type == "TS8");

            if (Fmds.Count()==1 && TS8.Count() == 0 && type =="FMD")
            {
                return "not bayable";
            }

            if (Fmds.Count()-TS8.Count() >= 1 && type == "FMD")
            {
                return "not bayable";
            }
            else
            {
                return "bayable";
            }
        }

        [HttpPost]
        public string GetDimensionsBaying()
        {
            string guid = Convert.ToString(Session["guid"]);
            var myaccessories = db.LargeEnclosureMyaccessories.Where(a => a.Code == guid && a.Accessory_Number==null && a.Baying_NotBaying=="1").FirstOrDefault();
            if (myaccessories!= null)
            {
                return myaccessories.Height;
            }
            else
            {
                return "not already baying";
            }
        }
        #endregion

        #region Get Dimensions
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public string GetDimensions(string bayingType="",string width="",string height="",string depth="", string doors="")
        {
            List<Widths> Widths = new List<Widths>();
            List<Heights> Heights = new List<Heights>();
            List<Depths> Depths = new List<Depths>();

            if (bayingType == "1")
            {
                IEnumerable<Large_enclosureTs8> LargeEnclosureTs;

                if (height != "" && depth != "")
                {
                    LargeEnclosureTs = db.Large_enclosureTs8s.Where(model=>model.Height==height && model.Depth==depth);
                }
                else if (height != "" && depth == "")
                {
                    LargeEnclosureTs = db.Large_enclosureTs8s.Where(model=>model.Height==height);
                }
                else if (height == "" && depth != "")
                {
                    LargeEnclosureTs = db.Large_enclosureTs8s.Where(model=>model.Depth==depth);
                }
                else
                {
                    LargeEnclosureTs = db.Large_enclosureTs8s;
                }

                foreach (var item in LargeEnclosureTs)
                {
                    if (!string.IsNullOrEmpty(item.Depth) && item.Depth != "NULL")
                    {
                        Depths.Add(new Depths { Depth = item.Depth });
                    }
                    if (!string.IsNullOrEmpty(item.Width) && item.Width != "NULL")
                    {
                        Widths.Add(new Widths { Width = item.Width });
                    }
                    if (!string.IsNullOrEmpty(item.Height) && item.Height != "NULL")
                    {
                        Heights.Add(new Heights { Height = item.Height });
                    }
                }


                var newheights = Heights.ToList().OrderBy(x => x.Height).Select(x => x.Height).Distinct();
                var newwidths = Widths.ToList().OrderBy(x => x.Width).Select(x => x.Width).Distinct();
                var newdepts = Depths.ToList().OrderBy(x => x.Depth).Select(x => x.Depth).Distinct();

                string returnString = new JavaScriptSerializer().Serialize(new { height = newheights, width = newwidths, depth = newdepts, status = "OK" });

                return returnString;
            }
            else
            {
                var LargeEnclosureTset = db.Large_enclosureTs8s.Select(a => new { a.Width, a.Height, a.Depth })
                                          .Union(db.Large_enclosureFmds.Select(b => new { b.Width, b.Height,b.Depth }));

                if (height != "" && depth != "" && width!= "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a=>a.Height==height && a.Depth==depth && a.Width==width);
                }
                else if (height != "" && depth != "" && width== "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Height == height && a.Depth == depth);
                }
                else if (height != "" && depth == "" && width != "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Height == height && a.Width == width);
                }
                else if (height == "" && depth != "" && width != "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Depth == depth && a.Width == width);
                }
                else if (height != "" && depth == "" && width == "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Height == height);
                }
                else if (height == "" && depth != "" && width == "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Depth == depth);
                }
                else if (height == "" && depth == "" && width != "")
                {
                    LargeEnclosureTset = LargeEnclosureTset.Where(a => a.Width == width);
                }

                foreach (var item in LargeEnclosureTset.OrderBy(a=>a.Width))
                {
                    if (!string.IsNullOrEmpty(item.Depth) && item.Depth!="NULL")
                    {
                        Depths.Add(new Depths { Depth = item.Depth });
                    }
                    if (!string.IsNullOrEmpty(item.Width) && item.Width!="NULL")
                    {
                        Widths.Add(new Widths { Width = item.Width });
                    }
                    if (!string.IsNullOrEmpty(item.Height) && item.Height!="NULL")
                    {
                        Heights.Add(new Heights { Height = item.Height });
                    }
                }

                var newheights =  Heights.ToList().OrderBy(x=>x.Height).Select(x=>x.Height).Distinct();
                var newwidths = Widths.ToList().OrderBy(x => x.Width).Select(x => x.Width).Distinct();
                var newdepts = Depths.ToList().OrderBy(x => x.Depth).Select(x => x.Depth).Distinct();
      
                string returnString = new JavaScriptSerializer().Serialize(new { height = newheights, width = newwidths, depth = newdepts, status = "OK" });

                return returnString;
            }
        }
        #endregion

        #region Get Base Enclosures
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public string GetBaseEnclosures(string bayingType="", string width="", string height="", string depth="", string doors="",string articlenumber="")
        {
            List<BayingNotBaying> enclosureTs8 = new List<BayingNotBaying>();
            List<BayingNotBaying> enclosureFmds = new List<BayingNotBaying>();

            if (bayingType=="1" && articlenumber=="")
            {
                //Get TS8 Large Enclosures
                IEnumerable<Large_enclosureTs8> LargeEnclosureTs8 = db.Large_enclosureTs8s.Where(a=>a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");

                if (height != "" && depth != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(model => model.Height == height && model.Depth == depth);
                }
                else if (height != "" && depth == "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(model => model.Height == height).Distinct();
                }
                else if (height == "" && depth != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(model => model.Depth == depth).Distinct();
                }

                var Ts8Data = LargeEnclosureTs8.GroupBy(p=>new {p.TS8_partnumber,p.Height,p.Width,p.Depth,p.Doors }).OrderBy(p=>p.Key.TS8_partnumber).Distinct();
                foreach (var item in Ts8Data)
                {
                    enclosureTs8.Add(new BayingNotBaying { TS_Articelnummer = item.Key.TS8_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                //Get FMD Large Enclosures
                IEnumerable<Large_enclosureFmd> LargeEnclosureFMD = db.Large_enclosureFmds.Where(a => a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");

                if (height != "" && depth != "")
                {
                    LargeEnclosureFMD = LargeEnclosureFMD.Where(model => model.Height == height && model.Depth == depth);
                }
                else if (height != "" && depth == "")
                {
                    LargeEnclosureFMD = LargeEnclosureFMD.Where(model => model.Height == height);
                }
                else if (height == "" && depth != "")
                {
                    LargeEnclosureFMD = LargeEnclosureFMD.Where(model => model.Depth == depth);
                }

                var FmdData = LargeEnclosureFMD.GroupBy(p => new { p.FMD_partnumber, p.Height, p.Width, p.Depth, p.Doors }).OrderBy(p => p.Key.FMD_partnumber).Distinct();
                foreach (var item in FmdData)
                {
                    enclosureFmds.Add(new BayingNotBaying { TS_Articelnummer = item.Key.FMD_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                string returnString = new JavaScriptSerializer().Serialize(new { TS8=enclosureTs8,FMD=enclosureFmds, CTS =enclosureTs8.Count(), CFMD=enclosureFmds.Count(), status = "OK" });

                return returnString;
            }
            else if (bayingType == "0" && articlenumber == "")
            {
                //Get TS8 Large Enclosures
                IEnumerable<Large_enclosureTs8> LargeEnclosureTs8 = db.Large_enclosureTs8s.Where(a => a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");
                if (height != "" && depth != "" && width != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Height == height && a.Depth == depth && a.Width == width);
                }
                else if (height != "" && depth != "" && width == "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Height == height && a.Depth == depth);
                }
                else if (height != "" && depth == "" && width != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Height == height && a.Width == width);
                }
                else if (height == "" && depth != "" && width != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Depth == depth && a.Width == width);
                }
                else if (height != "" && depth == "" && width == "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Height == height);
                }
                else if (height == "" && depth != "" && width == "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Depth == depth);
                }
                else if (height == "" && depth == "" && width != "")
                {
                    LargeEnclosureTs8 = LargeEnclosureTs8.Where(a => a.Width == width);
                }

                var Ts8Data = LargeEnclosureTs8.GroupBy(p => new { p.TS8_partnumber, p.Height, p.Width, p.Depth, p.Doors }).OrderBy(p => p.Key.TS8_partnumber).Distinct();
                foreach (var item in Ts8Data)
                {
                    enclosureTs8.Add(new BayingNotBaying { TS_Articelnummer = item.Key.TS8_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                //Get FMD Large Enclosures
                IEnumerable<Large_enclosureFmd> LargeEnclosureFmd = db.Large_enclosureFmds.Where(a => a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");
                if (height != "" && depth != "" && width != "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Height == height && a.Depth == depth && a.Width == width);
                }
                else if (height != "" && depth != "" && width == "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Height == height && a.Depth == depth);
                }
                else if (height != "" && depth == "" && width != "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Height == height && a.Width == width);
                }
                else if (height == "" && depth != "" && width != "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Depth == depth && a.Width == width);
                }
                else if (height != "" && depth == "" && width == "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Height == height);
                }
                else if (height == "" && depth != "" && width == "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Depth == depth);
                }
                else if (height == "" && depth == "" && width != "")
                {
                    LargeEnclosureFmd = LargeEnclosureFmd.Where(a => a.Width == width);
                }

                var FmdData = LargeEnclosureFmd.GroupBy(p => new { p.FMD_partnumber, p.Height, p.Width, p.Depth, p.Doors }).OrderBy(p => p.Key.FMD_partnumber).Distinct();
                foreach (var item in FmdData)
                {
                    enclosureFmds.Add(new BayingNotBaying { TS_Articelnummer = item.Key.FMD_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                string returnString = new JavaScriptSerializer().Serialize(new { TS8 = enclosureTs8, FMD = enclosureFmds, CTS = enclosureTs8.Count(), CFMD = enclosureFmds.Count(), status = "OK" });

                return returnString;
            }
            else
            {
                //Get TS8 Large Enclosures
                IEnumerable<Large_enclosureTs8> LargeEnclosureTs8 = db.Large_enclosureTs8s.Where(a=>a.TS8_partnumber.Contains(articlenumber)).Where(a => a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");
                var Ts8Data = LargeEnclosureTs8.GroupBy(p => new { p.TS8_partnumber, p.Height, p.Width, p.Depth, p.Doors }).OrderBy(p => p.Key.TS8_partnumber).Distinct();
                foreach (var item in Ts8Data)
                {
                    enclosureTs8.Add(new BayingNotBaying { TS_Articelnummer = item.Key.TS8_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                //Get FMD Large Enclosures
                IEnumerable<Large_enclosureFmd> LargeEnclosureFMD = db.Large_enclosureFmds.Where(a => a.FMD_partnumber.Contains(articlenumber)).Where(a => a.Width != "NULL" || a.Height != "NULL" || a.Depth != "NULL");
                var FmdData = LargeEnclosureFMD.GroupBy(p => new { p.FMD_partnumber, p.Height, p.Width, p.Depth, p.Doors }).OrderBy(p => p.Key.FMD_partnumber).Distinct();
                foreach (var item in FmdData)
                {
                    enclosureFmds.Add(new BayingNotBaying { TS_Articelnummer = item.Key.FMD_partnumber, Width = item.Key.Width, Height = item.Key.Height, Depth = item.Key.Depth, Doors = item.Key.Doors });
                }

                string returnString = new JavaScriptSerializer().Serialize(new { TS8 = enclosureTs8, FMD = enclosureFmds, CTS = enclosureTs8.Count(), CFMD = enclosureFmds.Count(), status = "OK" });

                return returnString;
            }
        }
        #endregion

        #region Add Accessories
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public async Task<string> AddAccessories(string bayingType="", string enclosureBase="", string articelnummer="", string light="", string sidepanel="", string mountingplate="", string earthing="", string handle="", string chassis="", string printpacket="", string hingedegrees="", string eyebolts="", string holes="", string paint="", string singlepartnumber="")
        {
            List<LargeEnclosureMyaccessory> LargeEnclosureAccessories = new List<LargeEnclosureMyaccessory>();
            string guid = Convert.ToString(Session["guid"]);
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
                Session["guid"] = guid;
            }
            string username = User.Identity.GetUserName();
            var Type = articelnummer.Split('_');
            var partnumber = Type[1];
            string thebase = string.Empty;
            string thelight = string.Empty;
            string thesidepanel = string.Empty;
            string themountingplate = string.Empty;
            string anreihung = string.Empty;
            string base_desc = "";
            string baseUnitCost = "";
            string basedoors = "";
            string base_height = "";

            if (Type[0] =="TS8")
            {
                //Build the data
                List<LargeEnclosureViewModel> LargeEnclosures = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> SidepanelModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> thebaseModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> thelightModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> themountingplateModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> anreihungModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> earthingModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> handleModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> chassisModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> printpacketModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> hingedegreesModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> eyeboltsModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> holesModel = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> paintModel = new List<LargeEnclosureViewModel>();
               
                //Build the base enclosure data
                var baseEnclosureTS8 = db.Large_enclosurePricings.Where(a => a.Part_Number == partnumber);
                if (baseEnclosureTS8.Count()>0)
                {
                    base_desc = Type[0] + ", (" + Math.Round(Convert.ToInt32(baseEnclosureTS8.FirstOrDefault().Height) * 0.0393701) + " x " + Math.Round(Convert.ToInt32(baseEnclosureTS8.FirstOrDefault().Width) * 0.0393701) + " x " + Math.Round(Convert.ToInt32(baseEnclosureTS8.FirstOrDefault().Depth) * 0.0393701) + ") in - " + baseEnclosureTS8.FirstOrDefault().Doors + " Door(s)";
                    base_height = baseEnclosureTS8.FirstOrDefault().Height + "-"+ baseEnclosureTS8.FirstOrDefault().Depth;
                    basedoors = baseEnclosureTS8.FirstOrDefault().Doors;
                    baseUnitCost = baseEnclosureTS8.FirstOrDefault().Price.Remove(0, 1);
                }
                else
                {
                    base_desc = Type[0]+" No pricing data for enclosures";
                }

                var GetModel = db.Large_enclosureTs8s
                                .Join(
                                    db.Large_enclosurePricings,
                                    tsitem => tsitem.Accessory_partnumber,
                                    pric => pric.Part_Number,
                                    (tsitem, pric) => new { tsitem, pric }
                                );
                foreach (var item in GetModel)
                {
                    string unitcost = item.pric.Price.Remove(0,1);
                    LargeEnclosures.Add(new LargeEnclosureViewModel { TS_Articelnummer = item.tsitem.TS8_partnumber, Accessory_partnumber = item.tsitem.Accessory_partnumber, Description = item.tsitem.Description, Number_of_Packs = item.tsitem.Number_of_Packs, Quantity_Per_Pack = item.tsitem.Quantity_Per_Pack, Unit_Cost = unitcost, Username = User.Identity.Name, Code = guid,Allocation_1=item.tsitem.Allocation_1,Allocation_1_1=item.tsitem.Allocation_1_1 });
                }

                if (enclosureBase != "")
                {
                    //Process base
                    if (enclosureBase == "Base 100 mm")
                    {
                        thebase = "Flex-Block Sockel 100 mm";
                    }
                    else if (enclosureBase == "Base 200 mm")
                    {
                        thebase = "Flex-Block Sockel 200 mm";
                    }
                    else if (enclosureBase == "Base TS 100 mm")
                    {
                        thebase = "Sockel TS 100 mm";
                    }
                    else if (enclosureBase == "Base TS 200 mm")
                    {
                        thebase = "Sockel TS 200 mm";
                    }

                   thebaseModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == thebase).ToList();
                }

                //Get side panel
                if (sidepanel == "Sidepanel")
                {
                    if (bayingType == "1")
                    {
                        thesidepanel = "bayingSidePanel";
                    }

                    if (bayingType == "0")
                    {
                        thesidepanel = "notbayingSidePanel";
                    }

                    SidepanelModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == thesidepanel).ToList();
                }

                if (light!="")
                {
                    //Lighting 
                    if (light == "Universallamp")
                    {
                        thelight = "Leuchte mit Steckdose und Bewegungsmelder";
                    }
                    else if (light == "Comfortlight")
                    {
                        thelight = "Leuchte mit Steckdose und Turpositionsschalter";
                    }
                    else if (light == "LED")
                    {
                        thelight = "24 V DC Ausfuhrung";
                    }
                    if (bayingType=="1")
                    {
                        thelightModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == thelight).ToList();
                    }
                    else
                    {
                        thelightModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == thelight && a.Accessory_partnumber!= "2500530").ToList();
                    }
                }

                if (mountingplate == "Mountingplate-interm")
                {
                    themountingplate = "Montageplatten-ZwischenstÅ¸ck";
                    themountingplateModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == themountingplate).ToList();
                }

                if (earthing == "Earthing")
                {
                    earthing = "Erdung";
                    earthingModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == earthing).ToList();
                }

                if (handle != "")
                {
		            if (handle=="SecurityApplication")
		            {
			            handle = "Sicherheitseinsatz"; 
		            }

		            if (handle=="Safety insert and push button")
		            {
			            handle = "Sicherheitseinsatz und Druckknopf";
		            }
                    handleModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == handle).ToList();
                }

                if (chassis != "")
                {
                    if (chassis == "Innen")
                    {
                        chassis = "innen";
                    }

                    if (chassis == "Outside")
                    {
                        chassis = "outside";
                    }
                    chassisModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == chassis).ToList();
                }

                if (printpacket == "Printpacket")
                {
                    printpacket = "Schaltplantaschen";
                    printpacketModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == printpacket).ToList();
                }

                if (hingedegrees == "HingeDegrees")
                {
                    hingedegrees = "Scharnier";
                    hingedegreesModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == hingedegrees).ToList();
                }

                if (eyebolts == "Eyebolts")
                {
                    eyebolts = "Transportösen";
                    eyeboltsModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == eyebolts).ToList();
                }

                if (holes == "Holes")
                {
                    holes = "holes";
                    holesModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == holes).ToList();
                }

                if (paint != "")
                {
                    if (bayingType == "1" && paint == "Paint")
                    {
                        paint = "paint_baying";
                    }

                    if (bayingType == "0" && paint == "Paint")
                    {
                        paint = "paint_nobaying";
                    }

                    paintModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == paint).Take(1).ToList();
                }

                if (bayingType == "1" && Type[0] == "TS8")
                {
                    anreihung = "Anreihung";
                    anreihungModel = LargeEnclosures.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == anreihung).ToList();
                }

                var LargeEnclosures1 = thebaseModel.Concat(SidepanelModel).ToList();
                var LargeEnclosures2 = thelightModel.Concat(LargeEnclosures1).ToList();
                var LargeEnclosures3 = themountingplateModel.Concat(LargeEnclosures2).ToList();
                var LargeEnclosures4 = anreihungModel.Concat(LargeEnclosures3).ToList();
                var LargeEnclosures5 = earthingModel.Concat(LargeEnclosures4).ToList();
                var LargeEnclosures6 = handleModel.Concat(LargeEnclosures5).ToList();
                var LargeEnclosures7 = chassisModel.Concat(LargeEnclosures6).ToList();
                var LargeEnclosures8 = printpacketModel.Concat(LargeEnclosures7).ToList();
                var LargeEnclosures9 = hingedegreesModel.Concat(LargeEnclosures8).ToList();
                var LargeEnclosures10 = eyeboltsModel.Concat(LargeEnclosures9).ToList();
                var LargeEnclosures11 = holesModel.Concat(LargeEnclosures10).ToList();
                LargeEnclosures = paintModel.Concat(LargeEnclosures11).ToList();

                LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = Type[1], Description = base_desc, Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = baseUnitCost, Username = User.Identity.Name, Code = guid,Total_Cost = baseUnitCost, Baying_NotBaying = bayingType, Type = Type[0], Doors = basedoors, Height = base_height });
                foreach (var item in LargeEnclosures.Distinct())
                {
                    if (string.IsNullOrEmpty(item.Number_of_Packs))
                    {
                        item.Number_of_Packs = "0";
                    }
                    string totalcost = (Convert.ToDouble(item.Unit_Cost) * Convert.ToDouble(item.Number_of_Packs)).ToString();
                    LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = item.TS_Articelnummer, Accessory_Number = item.Accessory_partnumber, Description = item.Description, Number_of_Packs = item.Number_of_Packs, Quantity_Per_Pack = item.Quantity_Per_Pack, Unit_Cost = item.Unit_Cost, Username = username, Code = guid,Total_Cost= totalcost, Baying_NotBaying = bayingType });
                }
            }
            else
            {
                //Build the data
                List<LargeEnclosureViewModel> LargeEnclosuresFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> SidepanelModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> thebaseModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> thelightModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> themountingplateModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> anreihungModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> earthingModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> handleModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> chassisModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> printpacketModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> hingedegreesModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> eyeboltsModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> holesModelFMD = new List<LargeEnclosureViewModel>();
                List<LargeEnclosureViewModel> paintModelFMD = new List<LargeEnclosureViewModel>();

                var baseEnclosureFMD = db.Large_enclosurePricings.Where(a=>a.Part_Number== partnumber);
                if (baseEnclosureFMD.Count()>0) {
                    base_desc = Type[0] + ", (" + Math.Round(Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Height) * 0.0393701) + " x " + Math.Round(Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Width) * 0.0393701) + " x " + Math.Round(Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Depth) * 0.0393701) + ") in - " + baseEnclosureFMD.FirstOrDefault().Doors + " Door(s)";
                    base_height = baseEnclosureFMD.FirstOrDefault().Height + "-" + baseEnclosureFMD.FirstOrDefault().Depth;
                    basedoors = baseEnclosureFMD.FirstOrDefault().Doors;
                    baseUnitCost = baseEnclosureFMD.FirstOrDefault().Price.Remove(0, 1);
                }
                else
                {
                    base_desc = Type[0] + " No pricing data for enclosures";
                }

                var GetModel = db.Large_enclosureFmds
                                .Join(
                                    db.Large_enclosurePricings,
                                    fmditem => fmditem.Accessory_partnumber,
                                    pric => pric.Part_Number,
                                    (fmditem, pric) => new { fmditem, pric }
                                );

                foreach (var item in GetModel)
                {
                    string unitcost = item.pric.Price.Remove(0, 1);
                    LargeEnclosuresFMD.Add(new LargeEnclosureViewModel { TS_Articelnummer = item.fmditem.FMD_partnumber, Accessory_partnumber = item.fmditem.Accessory_partnumber, Description = item.fmditem.Description, Number_of_Packs = item.fmditem.Number_of_Packs, Quantity_Per_Pack = item.fmditem.Quantity_Per_Pack, Unit_Cost = unitcost, Username = User.Identity.Name, Code = guid, Allocation_1 = item.fmditem.Allocation_1, Allocation_1_1 = item.fmditem.Allocation_1_1 });
                }

                if (enclosureBase != "")
                {
                    //Process base
                    if (enclosureBase == "Base 100 mm")
                    {
                        thebase = "Flex-Block Sockel 100 mm";
                    }
                    else if (enclosureBase == "Base 200 mm")
                    {
                        thebase = "Flex-Block Sockel 200 mm";
                    }
                    else if (enclosureBase == "Base TS 100 mm")
                    {
                        thebase = "Sockel TS 100 mm";
                    }
                    else if (enclosureBase == "Base TS 200 mm")
                    {
                        thebase = "Sockel TS 200 mm";
                    }

                    thebaseModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == thebase).ToList();
                }

                //Get side panel
                if (sidepanel == "Sidepanel")
                {
                    if (bayingType == "1")
                    {
                        thesidepanel = "bayingSidePanel";
                    }

                    if (bayingType == "0")
                    {
                        thesidepanel = "notbayingSidePanel";
                    }
                    SidepanelModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == thesidepanel).ToList();
                }

                if (light != "")
                {
                    //Lighting 
                    if (light == "Universallamp")
                    {
                        thelight = "Leuchte mit Steckdose und Bewegungsmelder";
                    }
                    else if (light == "Comfortlight")
                    {
                        thelight = "Leuchte mit Steckdose und Turpositionsschalter";
                    }
                    else if (light == "LED")
                    {
                        thelight = "24 V DC Ausfuhrung";
                    }
                    if (bayingType == "1")
                    {
                        thelightModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == thelight).ToList();
                    }
                    else
                    {
                        thelightModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == thelight && a.Accessory_partnumber != "4315200").ToList();
                    }
                }

                if (mountingplate == "Mountingplate-interm")
                {
                    themountingplate = "Montageplatten-ZwischenstÅ¸ck";
                    themountingplateModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == themountingplate).ToList();
                }

                if (earthing == "Earthing")
                {
                    earthing = "Erdung";
                    earthingModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == earthing).ToList();
                }

                if (handle != "")
                {
                    if (handle == "SecurityApplication")
                    {
                        handle = "Sicherheitseinsatz";
                    }

                    if (handle == "Safety insert and push button")
                    {
                        handle = "Sicherheitseinsatz und Druckknopf";
                    }
                    handleModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == handle).ToList();
                }

                if (chassis != "")
                {
                    if (chassis == "Innen")
                    {
                        chassis = "innen";
                    }

                    if (chassis == "Outside")
                    {
                        chassis = "outside";
                    }
                    chassisModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1_1 == chassis).ToList();
                }

                if (printpacket == "Printpacket")
                {
                    printpacket = "Schaltplantaschen";
                    printpacketModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == printpacket).ToList();
                }

                if (hingedegrees == "HingeDegrees")
                {
                    hingedegrees = "180°-Scharnier";
                    hingedegreesModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == hingedegrees).ToList();
                }

                if (eyebolts == "Eyebolts")
                {
                    eyebolts = "Transportösen";
                    eyeboltsModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == eyebolts).ToList();
                }

                if (holes == "Holes")
                {
                    holes = "holes";
                    holesModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == holes).ToList();
                }

                if (paint != "")
                {
                    if (bayingType == "1" && paint == "Paint")
                    {
                        paint = "paint_baying";
                    }

                    if (bayingType == "0" && paint == "Paint")
                    {
                        paint = "paint_nobaying";
                    }

                    paintModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == paint).Take(1).ToList();
                }

                if (bayingType == "1" && Type[0] == "TS8")
                {
                    anreihung = "Anreihung";
                    anreihungModelFMD = LargeEnclosuresFMD.Where(a => a.TS_Articelnummer == partnumber && a.Allocation_1 == anreihung).ToList();
                }

                var LargeEnclosuresFMD1 = thebaseModelFMD.Concat(SidepanelModelFMD).ToList();
                var LargeEnclosuresFMD2 = thelightModelFMD.Concat(LargeEnclosuresFMD1).ToList();
                var LargeEnclosuresFMD3 = themountingplateModelFMD.Concat(LargeEnclosuresFMD2).ToList();
                var LargeEnclosuresFMD4 = anreihungModelFMD.Concat(LargeEnclosuresFMD3).ToList();
                var LargeEnclosuresFMD5 = earthingModelFMD.Concat(LargeEnclosuresFMD4).ToList();
                var LargeEnclosuresFMD6 = handleModelFMD.Concat(LargeEnclosuresFMD5).ToList();
                var LargeEnclosuresFMD7 = chassisModelFMD.Concat(LargeEnclosuresFMD6).ToList();
                var LargeEnclosuresFMD8 = printpacketModelFMD.Concat(LargeEnclosuresFMD7).ToList();
                var LargeEnclosuresFMD9 = hingedegreesModelFMD.Concat(LargeEnclosuresFMD8).ToList();
                var LargeEnclosuresFMD10 = eyeboltsModelFMD.Concat(LargeEnclosuresFMD9).ToList();
                var LargeEnclosuresFMD11 = holesModelFMD.Concat(LargeEnclosuresFMD10).ToList();
                LargeEnclosuresFMD = paintModelFMD.Concat(LargeEnclosuresFMD11).ToList();

                LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = Type[1], Description = base_desc, Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = baseUnitCost, Username = User.Identity.Name, Code = guid,Total_Cost = baseUnitCost, Baying_NotBaying = bayingType, Type = Type[0], Doors = basedoors, Height = base_height });

                foreach (var item in LargeEnclosuresFMD.Distinct())
                {
                    if (string.IsNullOrEmpty(item.Number_of_Packs))
                    {
                        item.Number_of_Packs = "0";
                    }
                    string totalcost = (Convert.ToDouble(item.Unit_Cost) * Convert.ToDouble(item.Number_of_Packs)).ToString();
                    LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = item.TS_Articelnummer, Accessory_Number = item.Accessory_partnumber, Description = item.Description, Number_of_Packs = item.Number_of_Packs, Quantity_Per_Pack = item.Quantity_Per_Pack, Unit_Cost = item.Unit_Cost, Username = username, Code = guid,Total_Cost = totalcost, Baying_NotBaying = bayingType });
                }

                //For FMDs add the baying accesories
                if (bayingType=="1" && Type[0]=="FMD")
                {
                    LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = "4911000", Accessory_Number = "0", Description = "DOOR INTERLOCK KIT F-FMDC", Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = "109.00", Username = username, Code = guid, Total_Cost = "109.00", Baying_NotBaying = bayingType });
                    LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = "4911100", Accessory_Number = "0", Description = "ADJACENT DOOR INTERLOCK KIT", Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = "141.00", Username = username, Code = guid, Total_Cost = "141.00", Baying_NotBaying = bayingType });

                    if ( Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Width) <= 600)
                    {
                        LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = "4916000", Accessory_Number = "0", Description = "INTERLOCK BARS F-24W ENCL", Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = "91.80", Username = username, Code = guid, Total_Cost = "91.80", Baying_NotBaying = bayingType });
                    }

                    if (Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Width) > 600 && Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Width) <= 800)
                    {
                        LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = "4918000", Accessory_Number = "0", Description = "INTERLOCK BARS F-32W ENCL", Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = "136.00", Username = username, Code = guid, Total_Cost = "136.00", Baying_NotBaying = bayingType });
                    }

                    if (Convert.ToInt32(baseEnclosureFMD.FirstOrDefault().Width) > 800)
                    {
                        LargeEnclosureAccessories.Add(new LargeEnclosureMyaccessory { Part_Number = "4920000", Accessory_Number = "0", Description = "INTERLOCK BARS F-48W ENCL", Number_of_Packs = "1", Quantity_Per_Pack = "1", Unit_Cost = "189.00", Username = username, Code = guid, Total_Cost = "189.00", Baying_NotBaying = bayingType });
                    }
                }

            }

            db.LargeEnclosureMyaccessories.AddRange(LargeEnclosureAccessories);
            await db.SaveChangesAsync();

            if (singlepartnumber == "done")
            {
                //function to return the agregated enclosures
                List<LargeEnclosureMyaccessory> myaccessariesBaying = GetAggregatedAccessoriesBaying(guid);
                List<LargeEnclosureMyaccessory> myaccessariesNonBaying = GetAggregatedAccessoriesNonBaying(guid);
                string returnString = new JavaScriptSerializer().Serialize(new { guid = guid, myaccessaries = myaccessariesNonBaying, myaccessariesBaying = myaccessariesBaying, status = "OK" });
                return returnString;
            }
            else
            {
                //function to return the agregated enclosures
                List<LargeEnclosureMyaccessory> myaccessariesBaying = db.LargeEnclosureMyaccessories.Where(a => a.Code == guid && a.Baying_NotBaying == "1" && a.Accessory_Number==null).ToList();
                string returnString = new JavaScriptSerializer().Serialize(new { guid = guid, myaccessariesBaying = myaccessariesBaying, status = "OK" });
                return returnString;
            }
        }

        public List<LargeEnclosureMyaccessory> GetAggregatedAccessoriesBaying(string guid)
        {
            List<LargeEnclosureMyaccessory> my_saved_accessaries = db.LargeEnclosureMyaccessories.Where(a => a.Code == guid && a.Baying_NotBaying=="1").ToList();
            List<LargeEnclosureMyaccessory> myaccessaries = new List<LargeEnclosureMyaccessory>();
            var groupedItems = my_saved_accessaries.GroupBy(a => a.Accessory_Number, (Key, Values) => new { Accessory = Key, Counts = Values.Count() });

            foreach (var items in my_saved_accessaries)
            {
                if (items.Accessory_Number==null)
                {
                   myaccessaries.Add(new LargeEnclosureMyaccessory { Part_Number = items.Part_Number, Accessory_Number = items.Accessory_Number, Description = items.Description, Number_of_Packs = items.Number_of_Packs, Quantity_Per_Pack = items.Quantity_Per_Pack, Unit_Cost = items.Unit_Cost, Code = guid, Total_Cost = items.Total_Cost });
                }
            }

            foreach (var groupeditems in groupedItems)
            {
                foreach (var items in my_saved_accessaries)
                {
                    if (items.Accessory_Number != null)
                    {
                        if (groupeditems.Accessory == items.Accessory_Number)
                        {
                            int countaccessories = groupeditems.Counts;
                            int piecesperpack = Convert.ToInt32(items.Quantity_Per_Pack);
                            int piecesperenclosure = Convert.ToInt32(items.Number_of_Packs);
                            float numberrequired = piecesperenclosure * countaccessories;
                            float numberofpacks = numberrequired/piecesperpack;

                            string numberpiecesrequired =(Math.Ceiling(numberofpacks)).ToString();
                            myaccessaries.Add(new LargeEnclosureMyaccessory { Part_Number = items.Part_Number, Accessory_Number = items.Accessory_Number, Description = items.Description, Number_of_Packs = numberpiecesrequired, Quantity_Per_Pack = items.Quantity_Per_Pack, Unit_Cost = items.Unit_Cost, Code = guid, Total_Cost = items.Total_Cost });
                            break;
                        }
                    }
                }
            }

            return myaccessaries;
        }

        public List<LargeEnclosureMyaccessory> GetAggregatedAccessoriesNonBaying(string guid)
        {
            List<LargeEnclosureMyaccessory> my_saved_accessariesNonBaying = db.LargeEnclosureMyaccessories.Where(a => a.Code == guid && a.Baying_NotBaying == "0").ToList();
            List<LargeEnclosureMyaccessory> myaccessariesNonBaying = new List<LargeEnclosureMyaccessory>();

            foreach (var items in my_saved_accessariesNonBaying)
            {
                if (items.Accessory_Number != null)
                {
                        int countaccessories = 1;
                        int piecesperpack = Convert.ToInt32(items.Quantity_Per_Pack);
                        int piecesperenclosure = Convert.ToInt32(items.Number_of_Packs);
                        float numberrequired = piecesperenclosure * countaccessories;
                        float numberofpacks = numberrequired/piecesperpack;

                        string numberpiecesrequired = (Math.Ceiling(numberofpacks)).ToString();
                    myaccessariesNonBaying.Add(new LargeEnclosureMyaccessory { Part_Number = items.Part_Number, Accessory_Number = items.Accessory_Number, Description = items.Description, Number_of_Packs = numberpiecesrequired, Quantity_Per_Pack = items.Quantity_Per_Pack, Unit_Cost = items.Unit_Cost, Code = guid, Total_Cost = items.Total_Cost });
                }
                else
                {
                    myaccessariesNonBaying.Add(new LargeEnclosureMyaccessory { Part_Number = items.Part_Number, Accessory_Number = items.Accessory_Number, Description = items.Description, Number_of_Packs = items.Number_of_Packs, Quantity_Per_Pack = items.Quantity_Per_Pack, Unit_Cost = items.Unit_Cost, Code = guid, Total_Cost = items.Total_Cost });
                }
            }

            return myaccessariesNonBaying;
        }
        #endregion

        #region Get Accessories
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public string GetAccessories(string string_guid = null)
        {
            List<LargeEnclosureMyaccessory> myaccessories = new List<LargeEnclosureMyaccessory>();
            string unitcost = "";
            string totalcost = "";

            var myaccessaries = db.LargeEnclosureMyconfigs.Where(a=>a.Unique_id==string_guid).FirstOrDefault();
            string[] rows = myaccessaries.Materials.Split('|');
            int cnrows = rows.Count()-3;
            
            for (int i = 1; i <= cnrows; i++)
            {
                string[] columns = rows[i].Split('\t');
                int cncolumns = columns.Count();
                //check if unit cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[4]))
                {
                    unitcost = columns[4].Remove(0, 1);
                }
                //check if total cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[5]))
                {
                    totalcost = columns[5].Remove(0, 1);
                }

                //Add enclosures
                if (!columns[0].Contains("Base Enclosure"))
                {
                    if (columns[0].Contains("-"))
                    {
                        var parts = columns[0].Split('-');
                        myaccessories.Add(new LargeEnclosureMyaccessory { Part_Number = parts[0], Accessory_Number = parts[1], Description = columns[1], Number_of_Packs = columns[3], Quantity_Per_Pack = columns[2], Unit_Cost = unitcost, Total_Cost = totalcost });
                    }
                    else
                    {
                        myaccessories.Add(new LargeEnclosureMyaccessory { Part_Number = columns[0], Accessory_Number = "", Description = columns[1], Number_of_Packs = columns[3], Quantity_Per_Pack = columns[2], Unit_Cost = unitcost, Total_Cost = totalcost });
                    }
                }
            }
            //reset the session
            Session["guid"] = myaccessaries.Unique_id;

            string returnString = new JavaScriptSerializer().Serialize(new { guid = myaccessaries.Unique_id, Config_name = myaccessaries.Config_name, myaccessaries = myaccessories, status = "OK" });

            return returnString;
        }
        #endregion

        #region Delete Accessories
        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public async Task<string> deleteAccessories(int id = 0)
        {
            var myaccessaries = db.LargeEnclosureMyaccessories.Find(id);
            db.LargeEnclosureMyaccessories.Remove(myaccessaries);
            await db.SaveChangesAsync();
            string returnString = new JavaScriptSerializer().Serialize(new { status = "OK" });

            return returnString;
        }

        [AcceptVerbs(WebRequestMethods.Http.Get, WebRequestMethods.Http.Post)]
        public async Task<string> deleteAccessoriesBaying(string part_number="",string accessory_number="")
        {
            string guid = Convert.ToString(Session["guid"]);
            var myaccessaries = db.LargeEnclosureMyaccessories.Where(a=>a.Part_Number==part_number && a.Accessory_Number==accessory_number && a.Baying_NotBaying=="1" && a.Code==guid);
            if (myaccessaries!=null)
            {
                db.LargeEnclosureMyaccessories.RemoveRange(myaccessaries);
            }
            await db.SaveChangesAsync();
            string returnString = new JavaScriptSerializer().Serialize(new { status = "OK" });

            return returnString;
        }
        #endregion

        #region Save, Get Delete Configuration
        [HttpPost]
        public string SaveConfiguration()
        {
            string username = User.Identity.GetUserName();
            string configname = Request.Form[0];
            string guid = Request.Form[2];
            var configs = db.LargeEnclosureMyconfigs.Where(a=>a.Config_name==configname && a.Unique_id==guid);
            if (configs.Count()>0) {
                return "A configuration with the name already exist";
            }

            if (Request.Form[3]=="save")
            {
                Config Largeenclosuremyconfig = new Config { Config_name = Request.Form[0], Unique_id = Request.Form[2],Date_created = DateTime.Today,Materials= Request.Unvalidated.Form[1],Username=username };
                db.LargeEnclosureMyconfigs.Add(Largeenclosuremyconfig);
                db.SaveChanges();
                Session["guid"] = "";
            }
            return "saved";
        }

        [HttpPost]
        public string GetConfiguration()
        {
            string username = User.Identity.GetUserName();

            var configs = db.LargeEnclosureMyconfigs.Where(a=>a.Username==username); ;
            foreach (var item in configs)
            {
                item.DateCreated = string.Format("{0:MM/dd/yyyy}", item.Date_created);
            }

            string returnString = new JavaScriptSerializer().Serialize(new { LargeEnclosureMyconfigs = configs, status = "OK" });

            return returnString;
        }

        [HttpPost]
        public string UpdateConfiguration()
        {
            string code = Request.Form[2];
            var configs = db.LargeEnclosureMyconfigs.Where(a => a.Unique_id == code);
            if (configs.Count() == 0)
            {
                return "A configuration with the ID does not exist";
            }

            configs.FirstOrDefault().Config_name = Request.Form[0];
            configs.FirstOrDefault().Materials = Request.Unvalidated.Form[1];
            db.SaveChanges();
            
            return "updated";
        }

        [HttpPost]
        public string DeleteConfiguration(int id)
        {
            var configs = db.LargeEnclosureMyconfigs.Find(id);
            db.LargeEnclosureMyconfigs.Remove(configs);
            //db.SaveChangesAsync();

            var myaccessaries = db.LargeEnclosureMyaccessories.Where(a=>a.Code==configs.Unique_id);
            if (myaccessaries!=null)
            {
                db.LargeEnclosureMyaccessories.RemoveRange(myaccessaries);
            }
            db.SaveChangesAsync();
            string returnString = new JavaScriptSerializer().Serialize(new { status = "OK" });

            return "deleted";
        }
        #endregion

        #region Create PDF
        public ActionResult PDFView()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Exportpdf()
        {
            string data = Request.Form[0];
            string token = Request.Form[1];
            ViewBag.token = token;
            List<LargeEnclosureMyaccessory> myaccessories = new List<LargeEnclosureMyaccessory>();
            string unitcost = "";
            string totalcost = "";

            string[] rows = data.Split('|');
            int cnrows = rows.Count() - 3;

            for (int i = 1; i <= cnrows; i++)
            {
                string[] columns = rows[i].Split('\t');
                int cncolumns = columns.Count();
                //check if unit cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[4]))
                {
                    unitcost = columns[4].Remove(0, 1);
                }
                //check if total cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[5]))
                {
                    totalcost = columns[5].Remove(0, 1);
                }

                //Add enclosures
                if (columns[0].Contains("-") && columns[0].Split('-')[1]=="NULL")
                {
                    var parts = columns[0].Split('-');
                    myaccessories.Add(new LargeEnclosureMyaccessory { Part_Number = parts[0], Accessory_Number = parts[1], Description = columns[1], Number_of_Packs = columns[3], Quantity_Per_Pack = columns[2], Unit_Cost = unitcost, Total_Cost = totalcost });
                }
                else
                {
                    myaccessories.Add(new LargeEnclosureMyaccessory { Part_Number = columns[0], Accessory_Number = "", Description = columns[1], Number_of_Packs = columns[3], Quantity_Per_Pack = columns[2], Unit_Cost = unitcost, Total_Cost = totalcost });
                }
            }

            //Add the total cost row text
            string[] columns_final_row = rows[rows.Count() - 2].Split('\t');
            myaccessories.Add(new LargeEnclosureMyaccessory { Part_Number = columns_final_row[0], Accessory_Number = "", Description = columns_final_row[1], Number_of_Packs = columns_final_row[3], Quantity_Per_Pack = columns_final_row[2], Unit_Cost = unitcost, Total_Cost = columns_final_row[1] });

            //call the function that returns the view to string for the email body
            string Body = RenderViewToString(ControllerContext, "~/Views/largeenclosure/PDFView.cshtml", myaccessories, true);
            ViewBag.Message = MvcHtmlString.Create(Body);
            MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(Body);
            // step 1: creation of a document-object
            Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
            doc.AddTitle("Return Request Tool");
            doc.AddCreator("RiSourceCenter");
            doc.AddAuthor("Return Request Tool");
            doc.AddHeader("Rittal USA", "The RiSourceCenter");
            // step 2:
            // we create a writer that listens to the document
            // and directs a XML-stream to a file
            PdfWriter writer = PdfWriter.GetInstance(doc, msOutput);
            // step 3: we open document and start the worker on the document
            doc.Open();
            //step 4: user the XMLWorkerHelper to read the file and add it to the doc
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, reader);
            //step 5: close all processes
            doc.Close();
            writer.CloseStream = false;
            reader.Close();
            writer.Close();
            //step 7:parse the output into a byte array to be read by the
            byte[] content = msOutput.ToArray();
            //return the content
            // Write out PDF from memory stream.
            string path = Server.MapPath("~/attachments/LargeEnclosure/PDFs/pdf.pdf");
            System.IO.File.Delete(path);
            System.IO.File.WriteAllBytes(path, content);//create the PDF file

            return Redirect("~/attachments/LargeEnclosure/PDFs/pdf.pdf");
           // return "";
        }
        #endregion

        #region render view to string function
        static string RenderViewToString(ControllerContext context, string viewPath, object model = null, bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view, context.Controller.ViewData, context.Controller.TempData, sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }
        #endregion

        #region Export as excel
        [HttpPost]
        public string Exportexcel()
        {
            StringBuilder largeEnclosure_excel = new StringBuilder();
            string data = Request.Form[0];
            string token = Request.Form[1];

            List<LargeEnclosureMyaccessory> myaccessories = new List<LargeEnclosureMyaccessory>();
            string unitcost = "";
            string totalcost = "";

            string[] rows = data.Split('|');
            int cnrows = rows.Count() - 3;

            largeEnclosure_excel.Append("<div><table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;width:100%;\" align=\"left\" ><tr><td><h1>Rittal</h1></td><th colspan=\"5\" >Reference #:  " + token+ "</th></tr>");

            for (int i = 1; i <= cnrows; i++)
            {
                string[] columns = rows[i].Split('\t');
                int cncolumns = columns.Count();
                //check if unit cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[4]))
                {
                    unitcost = columns[4].Remove(0, 1);
                }
                //check if total cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[5]))
                {
                    totalcost = columns[5].Remove(0, 1);
                }
                //Add the enclosure text
                if (columns[0].Contains("Bayed enclosures"))
                {
                    largeEnclosure_excel.Append("<tr><td colspan=\"6\" style=\"background:lightblue;font-weight:bolder;\" >" + columns[0] + "</td></tr>");
                }
                //Add the enclosure text
                if (columns[0].Contains("Base Enclosure"))
                {
                    largeEnclosure_excel.Append("<tr><td>" + columns[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                }
                //Add enclosures
                if (!columns[0].Contains("Base Enclosure") && !columns[0].Contains("Bayed enclosures"))
                {
                    if (columns[0].Contains("-"))
                    {
                        var parts = columns[0].Split('-');
                        largeEnclosure_excel.Append("<tr><td>" + parts[0] + "-" + parts[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                    }
                    else
                    {
                        largeEnclosure_excel.Append("<tr><td>" + columns[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                    }
                }
            }

            //Add the total cost row text
            string[] columns_final_row = rows[rows.Count() - 2].Split('\t');
            largeEnclosure_excel.Append("<tr><td style=\"background:silver;font-weight:bolder;\" colspan=\"5\" >" + columns_final_row[0] + "</td><td style=\"background:silver;font-weight:bolder;\" >" + columns_final_row[1] + "</td></tr></table></div>");

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachement; filename=BuildofMaterials.xls");
            Response.ContentType = "application/excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            Response.Output.Write(largeEnclosure_excel);
            Response.Flush();
            Response.End();

            string returnString = new JavaScriptSerializer().Serialize(new { guid = token, myaccessaries = myaccessories, status = "OK" });

            return returnString;
        }
        #endregion

        #region Email the excel file
        [HttpPost]
        public string Emailexcel()
        {
            StringBuilder largeEnclosure_excel = new StringBuilder();
            string usernames = User.Identity.Name;
            string fullname = Convert.ToString(Session["firstName"]) + " " + Convert.ToString(Session["lastName"]);

            string token = Request.Form[0];
            string data = Request.Form[1];
            string soldtoparty = Request.Form[2];
            string soldtoContact = Request.Form[3];
            string projectName = Request.Form[4];
            string comment = Request.Form[5];
            string paintColors = Request.Form[6];
            HttpPostedFileBase uploadedfile = Request.Files["attechedDrawing"];

            largeEnclosure_excel.Append("<div><table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;width:100%;\" align=\"left\" ><tr><td><h1>Rittal</h1></td><th colspan=\"5\" >Reference #:  " + token + "</th></tr>");

            List<LargeEnclosureMyaccessory> myaccessories = new List<LargeEnclosureMyaccessory>();
            string unitcost = "";
            string totalcost = "";

            string[] rows = data.Split('|');
            int cnrows = rows.Count() - 3;

            for (int i = 1; i <= cnrows; i++)
            {
                string[] columns = rows[i].Split('\t');
                int cncolumns = columns.Count();
                //check if unit cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[4]))
                {
                    unitcost = columns[4].Remove(0, 1);
                }
                //check if total cost is not null and remove the $ sign
                if (!string.IsNullOrEmpty(columns[5]))
                {
                    totalcost = columns[5].Remove(0, 1);
                }
                //Add the enclosure text
                if (columns[0].Contains("Bayed enclosures"))
                {
                    largeEnclosure_excel.Append("<tr><td colspan=\"6\" style=\"background:lightblue;font-weight:bolder;\" >" + columns[0] + "</td></tr>");
                }
                //Add the enclosure text
                if (columns[0].Contains("Base Enclosure"))
                {
                    largeEnclosure_excel.Append("<tr><td>" + columns[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                }

                //Add enclosures
                if (!columns[0].Contains("Base Enclosure") && !columns[0].Contains("Bayed enclosures"))
                {
                    if (columns[0].Contains("-"))
                    {
                        var parts = columns[0].Split('-');
                        largeEnclosure_excel.Append("<tr><td>" + parts[0] + "-" + parts[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                    }
                    else
                    {
                        largeEnclosure_excel.Append("<tr><td>" + columns[0] + "</td><td>" + columns[1] + "</td><td>" + columns[3] + "</td><td></td><td>$" + unitcost + "</td><td>$" + totalcost + "</td></tr>");
                    }
                }
            }

            //Add the total cost row text
            string[] columns_final_row = rows[rows.Count() - 2].Split('\t');
            largeEnclosure_excel.Append("<tr><td style=\"background:silver;font-weight:bolder;\" colspan=\"5\" >" + columns_final_row[0] + "</td><td style=\"background:silver;font-weight:bolder;\" >" + columns_final_row[1] + "</td></tr></table></div>");

            MemoryStream createExcelFile = new MemoryStream();
            Encoding Enc = Encoding.Default;
            byte[] mBArray = Enc.GetBytes(largeEnclosure_excel.ToString());
            createExcelFile = new MemoryStream(mBArray, false);

            string emailContent =
                "<html>" +
                "<head>" +
                "</head>" +
                "<body style='font-size:12px;'>" +
                "<table width=100% style='border-collapse:collapse;'>" +
                "<tr>" +
                "<td><h1>Thank you very much for your inquiry!</h1>" +
                "<p>" +
                "Your request has been sent to our Rittal quotes team. Note that if you chose holes and cutouts as an option and failed to attached a drawing, we'll contact you to inquire if you have one available. Have a question or concern? Please contact our Customer Service team at customerservice@rittal.us, or call us at (800) 477-4000 and we'll be happy to assist." +
                "<br />" +
                "<br />" +
                "Please see the information you submitted below along with an attached spreadsheet containing your part number build of materials including pricing and a unique reference ID." +
                "<br />" +
                "<br />" +
                "<b>Your data:</b><br />" +
                "Your Name: " + fullname + "<br>" +
                "Your Email: " + usernames + "<br>" +
                "Sold to Party: " + soldtoparty + "<br>" +
                "Sold to Contact: " + soldtoContact + "<br>" +
                "Paint Color: " + paintColors + "<br>" +
                "Project Name: " + projectName + "<br> " +
                "Comment: " + comment + "<br>" +
                "<br><b>Requested information:</b><br>" +
                "Date of request: " + DateTime.Today + "<br>" +
                "</p>" +
                "</td>" +
                "</tr>" +
                "</table>" +
                "</body>" +
                "</html>";

            MailMessage mail = new MailMessage();
            mail.To.Add("quotes@rittal.us");
            mail.To.Add(usernames);
            mail.From = new MailAddress("RiSource@rittal.us");
            mail.Body = emailContent;
            mail.Subject = "Rittal Large Enclosure Selector";
            //Attach the excel file
            Attachment Excel = new Attachment(createExcelFile, "BuildofMaterials.xls");
            mail.Attachments.Add(Excel);
            //Attach the uploaded file
            Attachment attach_uploadedFile = new Attachment(uploadedfile.InputStream, uploadedfile.FileName);
            mail.Attachments.Add(attach_uploadedFile);
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["Host"]);
            smtp.UseDefaultCredentials = true;
            smtp.Send(mail);
        
            return "OK";
        }
        #endregion
    }
}
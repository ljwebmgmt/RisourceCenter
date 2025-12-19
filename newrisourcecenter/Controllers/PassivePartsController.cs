using newrisourcecenter.Models;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static NPOI.HSSF.Util.HSSFColor;

namespace newrisourcecenter.Controllers
{
    public class PassivePartsController : Controller
    {
        private RittalUSAPIEntities db = new RittalUSAPIEntities();
        private RisourceCenterMexicoEntities rittalDB = new RisourceCenterMexicoEntities();
        private SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["RittalDataConnection"].ConnectionString);
        private SqlConnection rittalUSConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["RittalUSConnection"].ConnectionString);


        // GET: PassiveParts
        public ActionResult Index()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var checkuser = rittalDB.usr_user.Where(a => a.usr_ID == userId).FirstOrDefault();
            if (checkuser == null || checkuser.usr_SAP == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: Details
        public ActionResult Details(string part)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            var checkuser = rittalDB.usr_user.Where(a => a.usr_ID == userId).FirstOrDefault();
            if (checkuser == null || checkuser.usr_SAP == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            conn.Open();
            rittalUSConn.Open();
            NonPIMViewModel viewModel = new NonPIMViewModel();
            viewModel.partNumber = part;
            viewModel.product = new Dictionary<string, object>();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM riprod_specs LEFT JOIN riprod_info ON riprod_specs.specs_partnum = riprod_info.dimm_partnum WHERE specs_partnum = @part AND (prod_status = 1 OR prod_status = 3) AND riprod_specs.prod_level = 'LS' AND riprod_specs.cat_status = '1'";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        viewModel.product = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                    }
                }
            }
            // Discontinued Product
            if (viewModel.product != null && viewModel.product.ContainsKey("prod_status") && viewModel.product["prod_status"].ToString() == "3")
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT riprod_discontinued.disc_partnum, riprod_discontinued.prod_discdescription, riprod_discontinued.prod_disclookup, riprod_discontinued.prod_discdate, riprod_disc_lookup.ID, riprod_disc_lookup.disc_desc FROM riprod_discontinued FULL OUTER JOIN riprod_disc_lookup ON riprod_discontinued.prod_discdescription = riprod_disc_lookup.ID WHERE disc_partnum = @part;";
                    cmd.Parameters.AddWithValue("@part", part);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            DiscontinuedProduct product = new DiscontinuedProduct()
                            {
                                disc_partnum = reader["disc_partnum"].ToString(),
                                prod_discdescription = reader["prod_discdescription"].ToString(),
                                prod_disclookup = reader["prod_disclookup"].ToString(),
                                prod_discdate = Convert.ToDateTime(reader["prod_discdate"].ToString()),
                                disc_desc = reader["disc_desc"].ToString()
                            };
                            viewModel.discontinuedProduct = product;
                        }
                    }
                }

            }
            // Literature
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT riprod_catalog.cat_id, riprod_catalog.cat_pn, riprod_catalog.cat_catalog, riprod_catalog.cat_pg, riprod_catalog.cat_xml, riprod_catalog.cat_insdate, riprod_catalog.cat_sortorder, lit_library.lit_title FROM riprod_catalog JOIN lit_library ON lit_library.litid = riprod_catalog.cat_catalog WHERE riprod_catalog.cat_pn = @part AND lit_library.litid = 689 ORDER BY cat_sortorder";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        viewModel.literature = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                    }
                }
            }
            // Image Files
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = rittalUSConn;
                cmd.CommandText = "SELECT image_file,image_desc,image_order FROM riprod_image WHERE specs_partnum = @part ORDER BY image_order ASC";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    viewModel.imageFiles = new List<ImageFile>();
                    while (reader.Read())
                    {
                        viewModel.imageFiles.Add(new ImageFile()
                        {
                            image_file = reader["image_file"].ToString(),
                            image_desc = reader["image_desc"].ToString(),
                            image_order = Convert.ToInt16(reader["image_order"].ToString())
                        });
                    }
                }
            }
            // Features
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT txt_marketing, txt_type, text_relate FROM riprod_text WHERE txt_partnum = @part";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    viewModel.textFeatures = new List<TextFeatures>();
                    while (reader.Read())
                    {
                        viewModel.textFeatures.Add(new TextFeatures()
                        {
                            txt_marketing = reader["txt_marketing"].ToString(),
                            txt_type = reader["txt_type"].ToString(),
                            text_relate = reader["text_relate"].ToString()
                        });
                    }
                }
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT prod_height, prod_width, prod_depth FROM riprod_info WHERE dimm_partnum = @part";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if(reader.HasRows && reader.Read())
                    {
                        viewModel.dimension = new Dimensions()
                        {
                            prod_height = (!string.IsNullOrEmpty(reader["prod_height"].ToString()) ? Convert.ToDecimal(reader["prod_height"].ToString()) : 0),
                            prod_width = (!string.IsNullOrEmpty(reader["prod_width"].ToString()) ? Convert.ToDecimal(reader["prod_width"].ToString()) : 0),
                            prod_depth = (!string.IsNullOrEmpty(reader["prod_depth"].ToString()) ? Convert.ToDecimal(reader["prod_depth"].ToString()) : 0),
                        };
                    }
                }
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT * FROM riprod_additSpecsValues WHERE specs_partnum = @part";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    viewModel.additionalSpecifications = new List<AdditionalSpecification>();
                    while (reader.Read())
                    {
                        int asID = Convert.ToInt16(reader["as_ID"].ToString());
                        if (asID <= 0)
                            continue;
                        using (SqlCommand innerCmd = new SqlCommand())
                        {
                            innerCmd.Connection = conn;
                            innerCmd.CommandText = "SELECT as_desc FROM riprod_additSpecs WHERE as_ID = @as_ID";
                            innerCmd.Parameters.AddWithValue("@as_ID", asID);
                            using (SqlDataReader innerReader = innerCmd.ExecuteReader())
                            {
                                if (innerReader.HasRows && innerReader.Read())
                                {
                                    viewModel.additionalSpecifications.Add(new AdditionalSpecification()
                                    {
                                        as_ID = asID,
                                        asv_value = reader["asv_value"].ToString(),
                                        as_desc = innerReader["as_desc"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT selectprod_ratings.srate_desc, riprod_ratings.prod_ratevalue FROM selectprod_ratings INNER JOIN riprod_ratings ON selectprod_ratings.srate_id = riprod_ratings.prod_ratetype WHERE riprod_ratings.rate_partnum = @part ORDER BY prod_ratetype ASC";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    viewModel.ratings = new List<Rating>();
                    while (reader.Read())
                    {
                        viewModel.ratings.Add(new Rating()
                        {
                            srate_desc = reader["srate_desc"].ToString(),
                            prod_ratevalue = reader["prod_ratevalue"].ToString()
                        });
                    }
                }
            }
            // Accessories
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT specs_partnum, ascList FROM dbo.riprod_specs WHERE specs_partnum = @part";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        string ascList = reader["ascList"].ToString();
                        if (!string.IsNullOrEmpty(ascList))
                        {
                            viewModel.accessories = new List<Accessory>();
                            using (SqlCommand innerCmd = new SqlCommand())
                            {
                                innerCmd.Connection = conn;
                                innerCmd.CommandText = "SELECT na.aId, na.titleID, na.specs_partnum, nt.titleName FROM dbo.NavAccessory na INNER JOIN NavTitle nt ON nt.titleID = na.titleID WHERE aId IN (" + ascList + ") Group BY na.titleID, na.aID, na.specs_partnum, nt.titleName ORDER BY na.specs_partnum";

                                using (SqlDataReader innerReader = innerCmd.ExecuteReader())
                                {
                                    while (innerReader.Read())
                                    {
                                        Accessory obj = new Accessory()
                                        {
                                            aId = Convert.ToInt32(innerReader["aId"].ToString()),
                                            titleID = Convert.ToInt32(innerReader["titleID"].ToString()),
                                            titleName = innerReader["titleName"].ToString(),
                                            descriptions = new List<Description>()
                                        };
                                        using (SqlCommand descCmd = new SqlCommand())
                                        {
                                            descCmd.Connection = conn;
                                            descCmd.CommandText = "SELECT DISTINCT specs_partnum,prod_description,use_prod_descsap,prod_descsap,prod_level,Xpress_active,cat_status FROM riprod_specs WHERE specs_partnum IN (" + innerReader["specs_partnum"].ToString() + ") ORDER BY specs_partnum";
                                            using (SqlDataReader descReader = descCmd.ExecuteReader())
                                            {
                                                while (descReader.Read())
                                                {
                                                    obj.descriptions.Add(new Description()
                                                    {
                                                        prod_description = descReader["prod_description"].ToString(),
                                                        use_prod_descsap = descReader["use_prod_descsap"].ToString(),
                                                        prod_descsap = descReader["prod_descsap"].ToString(),
                                                        specs_partnum = descReader["specs_partnum"].ToString()
                                                    });
                                                }
                                            }
                                        }
                                        viewModel.accessories.Add(obj);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            //Literature
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT cad_id, cad_dwf, cad_dwg, cad_fp, cad_pdf, partnum, cad_stp, cad_desc, cad_video, cad_zip FROM dbo.riprod_cad WHERE cad_partnum = @part";
                cmd.Parameters.AddWithValue("@part", part);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    viewModel.uploads = new List<Dictionary<string, object>>();
                    viewModel.pdfList = new List<string>();
                    viewModel.dwfList = new List<string>();
                    viewModel.zipList = new List<string>();
                    while (reader.Read())
                    {
                        viewModel.uploads.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
                        if (!string.IsNullOrEmpty(reader["cad_pdf"].ToString()))
                        {
                            viewModel.pdfList.Add(reader["cad_pdf"].ToString());
                        }
                        if (!string.IsNullOrEmpty(reader["cad_dwf"].ToString()))
                        {
                            viewModel.dwfList.Add(reader["cad_dwf"].ToString());
                        }
                        if (!string.IsNullOrEmpty(reader["cad_zip"].ToString()))
                        {
                            viewModel.zipList.Add(reader["cad_zip"].ToString());
                        }
                    }
                }
            }
            viewModel.catalogs = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (string type in new List<string>() { "catalog", "white paper", "flyer", "brochure", "msds" })
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT riprod_catalog.cat_id, riprod_catalog.cat_pn, riprod_catalog.cat_catalog, riprod_catalog.cat_pg, riprod_catalog.cat_xml, riprod_catalog.cat_insdate, riprod_catalog.cat_sortorder, lit_library.lit_title FROM riprod_catalog JOIN lit_library ON lit_library.litid = riprod_catalog.cat_catalog WHERE (riprod_catalog.cat_pn = @part) AND lit_type = @type ORDER BY cat_sortorder";
                    cmd.Parameters.AddWithValue("@part", part);
                    cmd.Parameters.AddWithValue("@type", type);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        viewModel.catalogs[type] = new List<Dictionary<string, object>>();
                        while (reader.Read())
                        {
                            viewModel.catalogs[type].Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
                        }
                    }
                }
            }

            conn.Close();
            rittalUSConn.Close();
            return View(viewModel);
        }
    }
}
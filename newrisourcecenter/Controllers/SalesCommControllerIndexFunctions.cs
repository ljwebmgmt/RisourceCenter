using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{

    public partial class SalesCommController
    {
        public List<SalesCommunicationsViewModel> childIdis27(List<SalesCommunicationsViewModel> salesCommunications, string companyType,int n3id,string querystring=null, string recent = "latest")
        {
            DateTime date = Convert.ToDateTime("1900-01-01 00:00:00.000");//format the date for the table request
            DateTime dateStart = Convert.ToDateTime("2019-01-01 00:00:00.000");//format the date for the table request
            if (recent == "latest")
            {
                date = Convert.ToDateTime(DateTime.Now.Year + "-12-31 00:00:00.000");//format the date for the table request
                dateStart = Convert.ToDateTime((DateTime.Now.Year - 1) + "-01-01 00:00:00.000");//format the date for the table request
            }

            dynamic salescom;
            if (querystring!=null)
            {
                salescom = dbEntity.salesComms.Join(
                         dbEntity.nav3,
                          sales => sales.n3ID,
                          nav3 => nav3.n3ID,
                         (sales, nav3) => new { sales, nav3 }
                     )
                     .Join(
                         dbEntity.nav2,
                         nav3 => nav3.nav3.n2ID,
                         nav2 => nav2.n2ID,
                         (nav3, nav2) => new { nav3, nav2 }
                     ).Where(a => a.nav3.nav3.n2ID == 27 || a.nav3.nav3.n2ID == 1 || a.nav3.nav3.n2ID == 3)
                     .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                     .Where(a => a.nav3.sales.sc_industry.Contains("1")).Where(a=>a.nav3.sales.sc_startDate >= dateStart)
                     .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                     .Where(a => a.nav3.sales.sc_headline.Contains(querystring) || a.nav3.sales.sc_keywords.Contains(querystring) || a.nav3.sales.sc_body.Contains(querystring))
                     .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }
            else
            {
                salescom = dbEntity.salesComms.Join(
                         dbEntity.nav3,
                          sales => sales.n3ID,
                          nav3 => nav3.n3ID,
                         (sales, nav3) => new { sales, nav3 }
                     )
                     .Join(
                         dbEntity.nav2,
                         nav3 => nav3.nav3.n2ID,
                         nav2 => nav2.n2ID,
                         (nav3, nav2) => new { nav3, nav2 }
                     ).Where(a => a.nav3.nav3.n2ID == 27 || a.nav3.nav3.n2ID == 1 || a.nav3.nav3.n2ID == 3)
                     .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                     .Where(a => a.nav3.sales.sc_industry.Contains("1")).Where(a => a.nav3.sales.sc_startDate >= dateStart)
                     .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                     .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }

            if (n3id == 0)
            {
                //if childId or n2Id is 27, filter by n2ID==27,n2ID==1,n2ID==3 without the n3ID while ignoring whether rittal user or not
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count() != 0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                }
            }
            else
            {
                //if childId or n2Id is 27, filter by n2ID==27,n2ID==1,n2ID==3 with the n3ID while ignoring whether rittal user or not
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count() != 0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }
                    if (comm.nav3.nav3.n3ID == n3id)
                    {
                        salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                    }
                }
            }

            return salesCommunications;

        }

        public List<SalesCommunicationsViewModel> childIdisNot27(List<SalesCommunicationsViewModel> salesCommunications, int childId, string companyType, string userIndustry, int n3id, string querystring = null, string recent = "latest")
        {
            //filter by n2ID or childID where id is not 27
            DateTime date = Convert.ToDateTime("1900-01-01 00:00:00.000");//format the date for the table request
            DateTime dateStart = Convert.ToDateTime("2019-01-01 00:00:00.000");//format the date for the table request
            if (recent == "latest")
            {
                date = Convert.ToDateTime(DateTime.Now.Year + "-12-31 00:00:00.000");//format the date for the table request
                dateStart = Convert.ToDateTime((DateTime.Now.Year - 1) + "-01-01 00:00:00.000");//format the date for the table request
            }            

            dynamic salescom;
            if (querystring!=null)
            {
                //if child id is set
                salescom = dbEntity.salesComms.Join(
                          dbEntity.nav3,
                           nav3 => nav3.n3ID,
                           sales => sales.n3ID,
                          (sales, nav3) => new { sales, nav3 }
                      )
                      .Join(
                          dbEntity.nav2,
                          nav3 => nav3.nav3.n2ID,
                          nav2 => nav2.n2ID,
                          (nav3, nav2) => new { nav3, nav2 }
                      ).Where(a => a.nav3.nav3.n2ID == childId)
                      .Where(a => a.nav3.sales.sc_status == 2)
                      .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                      .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                      .Where(a => a.nav3.sales.sc_startDate >= dateStart)
                      .Where(a => a.nav3.sales.sc_headline.Contains(querystring) || a.nav3.sales.sc_keywords.Contains(querystring) || a.nav3.sales.sc_body.Contains(querystring))
                      .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }
            else
            {
                //if child id is set
                salescom = dbEntity.salesComms.Join(
                          dbEntity.nav3,
                           nav3 => nav3.n3ID,
                           sales => sales.n3ID,
                          (sales, nav3) => new { sales, nav3 }
                      )
                      .Join(
                          dbEntity.nav2,
                          nav3 => nav3.nav3.n2ID,
                          nav2 => nav2.n2ID,
                          (nav3, nav2) => new { nav3, nav2 }
                      ).Where(a => a.nav3.nav3.n2ID == childId)
                      .Where(a => a.nav3.sales.sc_status == 2)
                      .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                      .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType)).Where(a => a.nav3.sales.sc_startDate >= dateStart)
                      .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }

            if (userIndustry != "3")
            {
                if (n3id == 0)
                {
                    //filter by those who are not rittal users without n3id
                    foreach (var comm in salescom)
                    {
                        //Add attachment list to the Edit page
                        string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                        List<Nav1List> list_attachments = new List<Nav1List>();
                        if (arrayOfAttachments != null)
                        {
                            int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                            foreach (int item in nums)
                            {
                                var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                                if (risour.Count() != 0)
                                {
                                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                                }
                            }
                        }
                        if (comm.nav3.sales.sc_industry.Contains(userIndustry))
                        {
                            salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                        }
                    }
                }
                else
                {
                    //filter by those who are not rittal users with n3id
                    foreach (var comm in salescom)
                    {
                        //Add attachment list to the Edit page
                        string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                        List<Nav1List> list_attachments = new List<Nav1List>();
                        if (arrayOfAttachments != null)
                        {
                            int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                            foreach (int item in nums)
                            {
                                var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                                if (risour.Count() != 0)
                                {
                                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                                }
                            }
                        }

                        if (comm.nav3.sales.sc_industry.Contains(userIndustry) && comm.nav3.nav3.n3ID == n3id)
                        {
                            salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                        }
                    }
                }
            }
            else
            {
                if (n3id == 0)
                {
                    //filter by those who are rittal users without n3id
                    foreach (var comm in salescom)
                    {
                        //Add attachment list to the Edit page
                        string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                        List<Nav1List> list_attachments = new List<Nav1List>();
                        if (arrayOfAttachments != null)
                        {
                            int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                            foreach (int item in nums)
                            {
                                var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                                if (risour.Count() != 0)
                                {
                                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                                }
                            }
                        }
                        if (comm.nav3.sales.sc_industry.Contains("1") || comm.nav3.sales.sc_industry.Contains("2"))
                        {
                            salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                        }
                    }
                }
                else
                {
                    //filter by those who are rittal users with n3id
                    foreach (var comm in salescom)
                    {
                        //Add attachment list to the Edit page
                        string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                        List<Nav1List> list_attachments = new List<Nav1List>();
                        if (arrayOfAttachments != null)
                        {
                            int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                            foreach (int item in nums)
                            {
                                var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                                if (risour.Count() != 0)
                                {
                                    list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                                }
                            }
                        }
                        if (comm.nav3.sales.sc_industry.Contains("1") || comm.nav3.sales.sc_industry.Contains("2") && comm.nav3.nav3.n3ID == n3id)
                        {
                            salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                        }
                    }
                }
            }

            return salesCommunications;
        }

        public List<SalesCommunicationsViewModel>  onlyN3Id(List<SalesCommunicationsViewModel> salesCommunications, string companyType, string userIndustry, int n3id, string querystring = null, string recent = "latest")
        {
            //filter by n2ID or childID where id is not 27
            DateTime date = Convert.ToDateTime("1900-01-01 00:00:00.000");//format the date for the table request
            DateTime dateStart = Convert.ToDateTime("2019-01-01 00:00:00.000");//format the date for the table request
            if (recent == "latest")
            {
                date = Convert.ToDateTime(DateTime.Now.Year + "-12-31 00:00:00.000");//format the date for the table request
                dateStart = Convert.ToDateTime((DateTime.Now.Year - 1) + "-01-01 00:00:00.000");//format the date for the table request
            }

            dynamic salescom;
            if (querystring!=null)
            {
                //if child id is set
                salescom = dbEntity.salesComms.Join(
                          dbEntity.nav3,
                           nav3 => nav3.n3ID,
                           sales => sales.n3ID,
                          (sales, nav3) => new { sales, nav3 }
                      )
                      .Join(
                          dbEntity.nav2,
                          nav3 => nav3.nav3.n2ID,
                          nav2 => nav2.n2ID,
                          (nav3, nav2) => new { nav3, nav2 }
                      ).Where(a => a.nav3.nav3.n3ID == n3id)
                      .Where(a => a.nav3.sales.sc_status == 2)
                      .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                      .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                      .Where(a => a.nav3.sales.sc_startDate >= dateStart)
                      .Where(a => a.nav3.sales.sc_headline.Contains(querystring) || a.nav3.sales.sc_keywords.Contains(querystring) || a.nav3.sales.sc_body.Contains(querystring))
                      .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }
            else
            {
                //if child id is set
                salescom = dbEntity.salesComms.Join(
                          dbEntity.nav3,
                           nav3 => nav3.n3ID,
                           sales => sales.n3ID,
                          (sales, nav3) => new { sales, nav3 }
                      )
                      .Join(
                          dbEntity.nav2,
                          nav3 => nav3.nav3.n2ID,
                          nav2 => nav2.n2ID,
                          (nav3, nav2) => new { nav3, nav2 }
                      ).Where(a => a.nav3.nav3.n3ID == n3id)
                      .Where(a => a.nav3.sales.sc_status == 2)
                      .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                      .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType)).Where(a => a.nav3.sales.sc_startDate >= dateStart)
                      .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }

            if (userIndustry != "3")
            {
                //filter by those who are not rittal users with n3id only
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count() != 0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    if (comm.nav3.sales.sc_industry.Contains(userIndustry))
                    {
                        salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                    }
                }
            }
            else
            {
                //filter by those who are rittal users with n3id only
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count() != 0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    if (comm.nav3.sales.sc_industry.Contains("1") || comm.nav3.sales.sc_industry.Contains("2"))
                    {
                        salesCommunications.Add(new SalesCommunicationsViewModel { scID = comm.nav3.sales.scID, n1ID = comm.nav2.n1ID, n3ID = comm.nav3.nav3.n3ID, n2ID = comm.nav2.n2ID, sc_headline = comm.nav3.sales.sc_headline, sc_body = comm.nav3.sales.sc_body, sc_endDate = comm.nav3.sales.sc_endDate, sc_startDate = comm.nav3.sales.sc_startDate, sc_teaser = comm.nav3.sales.sc_teaser, sc_status = comm.nav3.sales.sc_status, nav2_longName = comm.nav2.n2_nameLong, nav3_longName = comm.nav3.nav3.n3_nameLong, list_attachments = list_attachments, sc_products = comm.nav3.sales.sc_products });
                    }
                }
            }

            return salesCommunications;
        }

        public List<SalesCommunicationsViewModel> defaultNoChildId(List<SalesCommunicationsViewModel> salesCommunications, string companyType, string userIndustry,int n3id, string querystring = null, string recent = "latest")
        {
            int languageId = Convert.ToInt32(Session["userLanguageId"]);
            int countryId = Convert.ToInt32(Session["userCountryId"]);
            var locController = new CommonController();
            int i = 0;

            //Default function without any filters
            DateTime date = Convert.ToDateTime("1900-01-01 00:00:00.000");//format the date for the table request
            DateTime dateStart = Convert.ToDateTime("2019-01-01 00:00:00.000");//format the date for the table request
            if (recent == "latest")
            {
                date = Convert.ToDateTime(DateTime.Now.Year + "-12-31 00:00:00.000");//format the date for the table request
                dateStart = Convert.ToDateTime((DateTime.Now.Year) + "-01-01 00:00:00.000");//format the date for the table request
            }

            dynamic salescom;
            if (querystring!=null)
            {
                salescom = dbEntity.salesComms.Join(
                      dbEntity.nav3,
                       nav3 => nav3.n3ID,
                       sales => sales.n3ID,
                      (sales, nav3) => new { sales, nav3 }
                  )
                  .Join(
                      dbEntity.nav2,
                      nav3 => nav3.nav3.n2ID,
                      nav2 => nav2.n2ID,
                      (nav3, nav2) => new { nav3, nav2 }
                  ).Where(a => a.nav3.sales.sc_status == 2 && a.nav3.sales.countries.Contains(countryId.ToString()))
                  .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now ||  a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                  .Where(a=>a.nav3.sales.sc_startDate<=DateTime.Today && a.nav3.sales.sc_startDate >= dateStart)
                  .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                  .Where(a=>a.nav3.sales.sc_headline.Contains(querystring) || a.nav3.sales.sc_keywords.Contains(querystring) || a.nav3.sales.sc_body.Contains(querystring))
                  .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }
            else
            {
                  salescom = dbEntity.salesComms.Join(
                      dbEntity.nav3,
                       nav3 => nav3.n3ID,
                       sales => sales.n3ID,
                      (sales, nav3) => new { sales, nav3 }
                  )
                  .Join(
                      dbEntity.nav2,
                      nav3 => nav3.nav3.n2ID,
                      nav2 => nav2.n2ID,
                      (nav3, nav2) => new { nav3, nav2 }
                  ).Where(a => a.nav3.sales.sc_status == 2 && a.nav3.sales.countries.Contains(countryId.ToString()))
                  .Where(a => a.nav3.sales.sc_endDate >= DateTime.Now || a.nav3.sales.sc_endDate == null || a.nav3.sales.sc_endDate == date)
                  .Where(a=>a.nav3.sales.sc_startDate<=DateTime.Today && a.nav3.sales.sc_startDate >= dateStart)
                  .Where(a => a.nav3.sales.sc_usrTypes.Contains(companyType))
                  .OrderByDescending(a => a.nav3.sales.sc_startDate);
            }


            if (userIndustry != "3")
            {
                //Get to those who are not rittal users
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','), int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count() != 0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    if (comm.nav3.sales.sc_industry.Contains(userIndustry))
                    {
                        salesCommunications.Add(new SalesCommunicationsViewModel
                        {
                            scID = comm.nav3.sales.scID,
                            n1ID = comm.nav2.n1ID,
                            n3ID = comm.nav3.nav3.n3ID,
                            n2ID = comm.nav2.n2ID,
                            sc_headline = locController.localization("salesComm", "sc_headline", comm.nav3.sales.sc_headline, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_body = locController.localization("salesComm", "sc_body", comm.nav3.sales.sc_body, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_endDate = comm.nav3.sales.sc_endDate,
                            sc_startDate = comm.nav3.sales.sc_startDate,
                            sc_teaser = locController.localization("salesComm", "sc_teaser", comm.nav3.sales.sc_teaser, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_status = comm.nav3.sales.sc_status,
                            nav2_longName = comm.nav2.n2_nameLong,
                            nav3_longName = comm.nav3.nav3.n3_nameLong,
                            list_attachments = list_attachments,
                            sc_products = comm.nav3.sales.sc_products,
                            default_lang = comm.nav3.sales.default_lang
                        });
                    }

                    i++;
                }
            }
            else
            {
                //Get to those who are rittal users
                foreach (var comm in salescom)
                {
                    //Add attachment list to the Edit page
                    string arrayOfAttachments = Convert.ToString(comm.nav3.sales.attach_risource);
                    List<Nav1List> list_attachments = new List<Nav1List>();
                    if (arrayOfAttachments != null)
                    {
                        int[] nums = Array.ConvertAll(arrayOfAttachments.Split(','),int.Parse);
                        foreach (int item in nums)
                        {
                            var risour = dbEntity.RiSources.Where(a => a.ris_ID == item);
                            if (risour.Count()!=0)
                            {
                                list_attachments.Add(new Nav1List { id = risour.FirstOrDefault().ris_ID, name = risour.FirstOrDefault().ris_headline, img = risour.FirstOrDefault().ris_link });
                            }
                        }
                    }

                    if (comm.nav3.sales.sc_industry.Contains("1") || comm.nav3.sales.sc_industry.Contains("2"))
                    {
                        salesCommunications.Add(new SalesCommunicationsViewModel
                        {
                            scID = comm.nav3.sales.scID,
                            n1ID = comm.nav2.n1ID,
                            n3ID = comm.nav3.nav3.n3ID,
                            n2ID = comm.nav2.n2ID,
                            sc_headline = locController.localization("salesComm", "sc_headline", comm.nav3.sales.sc_headline, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_body = locController.localization("salesComm", "sc_body", comm.nav3.sales.sc_body, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_endDate = comm.nav3.sales.sc_endDate,
                            sc_startDate = comm.nav3.sales.sc_startDate,
                            sc_teaser = locController.localization("salesComm", "sc_teaser", comm.nav3.sales.sc_teaser, Convert.ToInt32(comm.nav3.sales.scID), languageId),
                            sc_status = comm.nav3.sales.sc_status,
                            nav2_longName = comm.nav2.n2_nameLong,
                            nav3_longName = comm.nav3.nav3.n3_nameLong,
                            list_attachments = list_attachments,
                            sc_products = comm.nav3.sales.sc_products,
                            default_lang = comm.nav3.sales.default_lang
                        });
                    }
                    i++;
                }
            }

            return salesCommunications;
        }
    }
}
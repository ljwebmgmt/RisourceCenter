using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using newrisourcecenter.Models;

namespace newrisourcecenter.Controllers
{
    public class MenuContoller : Controller
    {
        public static List<MenuItems> TopMenu(string username, int languageId, int location)
        {
            RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
            List<MenuItems> list = new List<MenuItems>();
            var locController = new CommonController();

            var nav1 = db.nav1.Where(a => a.n1_active == 1 && a.locations.Contains(location.ToString())).OrderBy(a => a.n1order);
            list.Add(new MenuItems
            {
                Id = 0,
                Name = "" + locController.localization("nav1", "n1_nameLong", "Dashboard/Home", 0, languageId) + "",
                PageName = "Index",
                Controller = "Home",
                LinkID = "dashboard"
            });

            foreach (var topmenu in nav1)
            {
                list.Add(new MenuItems
                {
                    Id = topmenu.n1ID,
                    Name = "" + locController.localization("nav1", "n1_nameLong", topmenu.n1_nameLong, topmenu.n1ID, languageId) + "",
                    PageName = "" + topmenu.pageName + "",
                    Controller = "" + topmenu.controller + "",
                    LinkID = "" + topmenu.linkId + "",
                    Usrgroup = topmenu.usr_group,
                    Order = topmenu.n1order
                });
            }

            return list;
        }

        public static List<MenuItems> SubMenu(int id, string[] arg, string username, int languageId, string audience, string industry, string products, string siteRole)
        {
            RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
            List<MenuItems> list = new List<MenuItems>();
            var locController = new CommonController();
            List<nav2> nav2 = new List<nav2>();

            //Colect sub nav data
            IQueryable<nav2> nav2data = db.nav2.Where(a => a.n2_active == 1).Where(a => a.n1ID == id && a.n2_usrTypes.Contains(audience) && a.n2_industry != null && a.n2_products != null).OrderBy(a => a.n2order);
            //Filter sub nav data
            // 2nd level menu
            //if (id == 0)
            //{
            //    foreach (var item in getDashboardmenus())
            //    {
            //        nav2.Add(new nav2 { n1ID = 0, n2ID = Convert.ToInt64(item.Key.ToString()), n2_nameLong = item.Value, Controller = "", PageName = null, usr_group = null, n2_redirect = null, n2_redirectJS = null, n2order = null });
            //    }
            //}
            if (siteRole == "1")
            {
                //Super admin sees all of it
                foreach (var item in nav2data)
                {
                    nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                }
            }
            else
            {
                foreach (var item in nav2data)
                {
                    //proccess industry filters for IT and Industrial
                    if (industry == "3")
                    {
                        //process products filters 
                        if (products != null)
                        {
                            char[] prod = products.ToArray();
                            string prods = new string(prod);
                            foreach (var num in prods)
                            {
                                if (item.n2_products.Contains(num))
                                {
                                    nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //process industry filter for IT or Industry and show links that are both
                        if (item.n2_industry == industry || item.n2_industry == "3")
                        {
                            //process products filter
                            if (products != null)
                            {
                                char[] prod = products.ToArray();
                                string prods = new string(prod);
                                foreach (var num in prods)
                                {
                                    if (item.n2_products.Contains(num.ToString()))
                                    {
                                        nav2.Add(new nav2 { n1ID = item.n1ID, n2ID = item.n2ID, n2_nameLong = item.n2_nameLong, Controller = item.Controller, PageName = item.PageName, usr_group = item.usr_group, n2_redirect = item.n2_redirect, n2_redirectJS = item.n2_redirectJS, n2order = item.n2order });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var submenu in nav2)
            {
                int n2id = Convert.ToInt32(submenu.n2ID);

                if (submenu.PageName == null && submenu.Controller != null)
                {

                    list.Add(new MenuItems
                    {
                        ChildId = submenu.n2ID,
                        Name = "" + locController.localization("nav2", "n2_nameLong", submenu.n2_nameLong, n2id, languageId) + "",
                        PageName = "" + arg[0] + "",
                        Controller = "" + submenu.Controller + "",
                        ParentId = id,
                        Usrgroup = submenu.usr_group,
                        redirect = submenu.n2_redirect,
                        n2_redirectJS = submenu.n2_redirectJS,
                        Order = submenu.n2order
                    });

                }
                else if (submenu.PageName != null && submenu.Controller == null)
                {

                    list.Add(new MenuItems
                    {
                        ChildId = submenu.n2ID,
                        Name = "" + locController.localization("nav2", "n2_nameLong", submenu.n2_nameLong, n2id, languageId) + "",
                        PageName = "" + submenu.PageName + "",
                        Controller = "" + arg[1] + "",
                        ParentId = id,
                        Usrgroup = submenu.usr_group,
                        redirect = submenu.n2_redirect,
                        n2_redirectJS = submenu.n2_redirectJS,
                        Order = submenu.n2order
                    });

                }
                else if (submenu.PageName == null && submenu.Controller == null)
                {

                    list.Add(new MenuItems
                    {
                        ChildId = submenu.n2ID,
                        Name = "" + locController.localization("nav2", "n2_nameLong", submenu.n2_nameLong, n2id, languageId) + "",
                        PageName = "" + arg[0] + "",
                        Controller = "" + arg[1] + "",
                        ParentId = id,
                        Usrgroup = submenu.usr_group,
                        redirect = submenu.n2_redirect,
                        n2_redirectJS = submenu.n2_redirectJS,
                        Order = submenu.n2order
                    });

                }
                else
                {

                    list.Add(new MenuItems
                    {
                        ChildId = submenu.n2ID,
                        Name = "" + locController.localization("nav2", "n2_nameLong", submenu.n2_nameLong, n2id, languageId) + "",
                        PageName = "" + submenu.PageName + "",
                        Controller = "" + submenu.Controller + "",
                        ParentId = id,
                        Usrgroup = submenu.usr_group,
                        redirect = submenu.n2_redirect,
                        n2_redirectJS = submenu.n2_redirectJS,
                        Order = submenu.n2order
                    });
                }
            }

            return list;
        }

        public static SectionsCounts partcounts(List<MenuItems> submenudata, string siteRole = null, string[] user_pages = null)
        {
            int one = 0; int two = 0; int three = 0; int four = 0; int five = 0; int six = 0; int seven = 0;

            foreach (var submenu in submenudata)
            {

                if (submenu.Usrgroup == null)
                {
                    if (submenu.Order > 1 && submenu.Order < 20)
                    {
                        one++;
                    }
                    if (submenu.Order > 20 && submenu.Order < 40)
                    {
                        two++;
                    }
                    if (submenu.Order > 40 && submenu.Order < 60)
                    {
                        three++;
                    }
                    if (submenu.Order > 60 && submenu.Order < 80)
                    {
                        four++;
                    }
                    if (submenu.Order > 80 && submenu.Order < 100)
                    {
                        five++;
                    }
                    if (submenu.Order > 100 && submenu.Order < 120)
                    {
                        six++;
                    }
                    if (submenu.Order > 120 && submenu.Order < 140)
                    {
                        seven++;
                    }
                }
                else if (submenu.Usrgroup.Contains(siteRole) || user_pages.Contains(submenu.ChildId.ToString()))
                {
                    if (@submenu.redirect != null || @submenu.n2_redirectJS != null)
                    {
                        if (submenu.Order > 1 && submenu.Order < 20)
                        {
                            one++;
                        }
                        if (submenu.Order > 20 && submenu.Order < 40)
                        {
                            two++;
                        }
                        if (submenu.Order > 40 && submenu.Order < 60)
                        {
                            three++;
                        }
                        if (submenu.Order > 60 && submenu.Order < 80)
                        {
                            four++;
                        }
                        if (submenu.Order > 80 && submenu.Order < 100)
                        {
                            five++;
                        }
                        if (submenu.Order > 100 && submenu.Order < 120)
                        {
                            six++;
                        }
                        if (submenu.Order > 120 && submenu.Order < 140)
                        {
                            seven++;
                        }
                    }
                    else
                    {
                        if (submenu.Order > 1 && submenu.Order < 20)
                        {
                            one++;
                        }
                        if (submenu.Order > 20 && submenu.Order < 40)
                        {
                            two++;
                        }
                        if (submenu.Order > 40 && submenu.Order < 60)
                        {
                            three++;
                        }
                        if (submenu.Order > 60 && submenu.Order < 80)
                        {
                            four++;
                        }
                        if (submenu.Order > 80 && submenu.Order < 100)
                        {
                            five++;
                        }
                        if (submenu.Order > 100 && submenu.Order < 120)
                        {
                            six++;
                        }
                        if (submenu.Order > 120 && submenu.Order < 140)
                        {
                            seven++;
                        }
                    }
                }
            }

            SectionsCounts secCount = new SectionsCounts
            {
                nav_settings = one,
                user_settings = two,
                partner_settings = three,
                risource = four,
                support_settings = five,
                communications = six,
                rittal_uni = seven
            };

            return secCount;
        }
        private static Dictionary<string, string> getDashboardmenus()
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            try
            {
                names.Add("0", "Navigation Settings");
                names.Add("1", "Manage Users Settings");
                names.Add("2", "Manage Partner Company");
                names.Add("3", "Manage RiSources");
                names.Add("4", "Manage Support Tools");
                names.Add("5", "Manage Sales Communication");
                names.Add("6", "Manage Rittal University");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return names;
        }

    }
}
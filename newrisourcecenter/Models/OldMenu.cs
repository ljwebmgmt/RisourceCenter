using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using newrisourcecenter.Models;

namespace newrisourcecenter.Models
{
    public class OldMenu
    {
        private static DateTime LastRefresh { get; set; }
        private static List<MenuItem> _Items;
        private RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();

        public static List<MenuItem> Items
        {
            get
            {
                if (_Items == null || (DateTime.Now - LastRefresh).TotalMinutes > 1) { RefreshItems(); }
                return _Items;
            }
            set { _Items = value; }
        }

        static OldMenu()
        {
            RefreshItems();
        }

        private static void RefreshItems()
        {

            //connect to a database and pull the elements
            //loop through the items and add the elements to the list

            //Load Items From Database
            Items = new List<MenuItem>()
            {
                new MenuItem() {Id= 0, Name="Home/Dashboard", PageName="Index", Controller="Home", LinkID="dashboard" },
                new MenuItem() {Id= 1, Name="Sales Communications", PageName="Index", Controller="SalesCommunications", LinkID="sales" },
                new MenuItem() {Id= 2, Name="Support Tools", PageName="Index", Controller="SupportTools", LinkID="support" },
                new MenuItem() {Id= 3, Name="Rittal University", PageName="Index", Controller="RittalUniversity", LinkID="university" },
                new MenuItem() {Id= 4, Name="RiSources", PageName="Index", Controller="RiSources", LinkID="risources" },
                new MenuItem() {Id= 5, Name="Child", PageName="Index", Controller="RiSources", LinkID="risources", ParentId= 0 }
            };

            LastRefresh = DateTime.Now;
        }

        public List<MenuItem> TopLevelItems()
        {
            return Items.Where(o => o.ParentId == null).ToList();
        }
        public List<MenuItem> Children(int id)
        {
            return Items.Where(o => o.ParentId == id).ToList();
        }

    }

    public class MenuItem
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; }
        public string Controller { get; set; }
        public string PageName { get; set;}
        public string LinkID { get; set; }
        public int? ParentId { get; set; }
    }
}
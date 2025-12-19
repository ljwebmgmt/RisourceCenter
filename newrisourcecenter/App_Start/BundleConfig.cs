using System.Web;
using System.Web.Optimization;

namespace newrisourcecenter
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/bootstrap-datepicker.min.js",
                        "~/Scripts/bootstrap-select.min.js",
                        "~/Scripts/jquery-ui.min-1.12.0.js",
                        "~/Scripts/jquery.tablesorter.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs").Include(
            "~/Scripts/angular.min.js",
            "~/Scripts/angular-route.min.js",
            "~/Scripts/angular-animate.min.js",
            "~/Scripts/angular-resource.min.js",
            "~/Scripts/app.js",
            "~/Scripts/controller.js",
            "~/Scripts/controller-RiSources.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/script.js"));

            bundles.Add(new StyleBundle("~/Content/css/dark").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/dark.css",
                      "~/Content/style.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Content/bootstrap-select.min.css"));

            bundles.Add(new StyleBundle("~/Content/css/blue").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/blue.css",
                      "~/Content/style.css"));


            bundles.Add(new StyleBundle("~/Content/css/gray").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/gray.css",
                      "~/Content/style.css"));

            bundles.Add(new StyleBundle("~/Content/css/white").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/white.css",
                      "~/Content/style.css"));
        }
    }
}

using System.Web.Optimization;

namespace EStudyBase.UI
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //...
            // Jquery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //...
            // Custom javascript bundles
            bundles.Add(new ScriptBundle("~/bundles/custom/js").Include("~/Scripts/Custom/*.js"));

            //...
            // Recorder javascript bundles
            bundles.Add(new ScriptBundle("~/bundles/recorder/js").Include("~/Scripts/Recorder/*.js"));

            
            //...
            // Javacript plugins
            bundles.Add(new ScriptBundle("~/bundles/colorbox/js").Include("~/Scripts/ColorBox/jquery.colorbox-min.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap/js").Include(
                "~/Content/bootstrap/assets/js/jquery.js",
                "~/Content/bootstrap/assets/js/bootstrap-transition.js",
                "~/Content/bootstrap/assets/js/bootstrap-alert.js",
                "~/Content/bootstrap/assets/js/bootstrap-modal.js",
                "~/Content/bootstrap/assets/js/bootstrap-dropdown.js",
                "~/Content/bootstrap/assets/js/bootstrap-scrollspy.js",
                "~/Content/bootstrap/assets/js/bootstrap-tab.js",
                "~/Content/bootstrap/assets/js/bootstrap-tooltip.js",
                "~/Content/bootstrap/assets/js/bootstrap-popover.js",
                "~/Content/bootstrap/assets/js/bootstrap-button.js",
                "~/Content/bootstrap/assets/js/bootstrap-collapse.js",
                "~/Content/bootstrap/assets/js/bootstrap-carousel.js",
                "~/Content/bootstrap/assets/js/bootstrap-typeahead.js",
                "~/Content/bootstrap/assets/js/bootstrap-affix.js",
                "~/Content/bootstrap/assets/js/bootbox.min.js",
                "~/Content/bootstrap/assets/js/holder/holder.js",
                "~/Content/bootstrap/assets/js/google-code-prettify/prettify.js",
                "~/Content/bootstrap/assets/js/application.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Content/datatables/media/js/jquery.dataTables.min.js",
                "~/Content/datatables/media/js/bootstrap.dataTables.js"));

            //...
            // Css files
            bundles.Add(new StyleBundle("~/bundles/custom/css").Include("~/Content/custom/site.css"));
            bundles.Add(new StyleBundle("~/bundles/bootstrap/css").Include(
                "~/Content/bootstrap/assets/css/bootstrap.css",
                "~/Content/bootstrap/assets/css/bootstrap-responsive.css",
                "~/Content/bootstrap/assets/css/social-buttons.css",
                "~/Content/bootstrap/assets/css/font-awesome.min.css"
                ));
            bundles.Add(new StyleBundle("~/bundles/datatables/css").Include(
                "~/Content/datatables/media/css/bootstrap.dataTables.css"));

            BundleTable.EnableOptimizations = false;
        }
    }
}
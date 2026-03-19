using System.Web.Optimization;

namespace Wikimedia
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/validation").Include(
                "~/Scripts/validation.js",
                "~/Scripts/jquery-maskedinput.js",
                "~/Scripts/bootbox.js",
                "~/Scripts/autoRefreshPanel.js",
                "~/Scripts/image-control.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/session.js",
                "~/Scripts/SiteNotificationsHandler.js",
                "~/Scripts/SiteScripts.js"
            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/_layout.css",
                "~/Content/site.css",
                "~/Content/menu.css",
                "~/Content/media.css",
                "~/Content/image-control.css",
                "~/Content/jqui-custom-datepicker.css",
                "~/Content/Accounts.css",
                "~/Content/Icons.css",
                "~/Content/Selections.css",
                "~/Content/popup.css"
            ));
        }
    }
}
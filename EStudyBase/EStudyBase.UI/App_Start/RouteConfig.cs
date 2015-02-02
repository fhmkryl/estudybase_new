using System.Web.Mvc;
using System.Web.Routing;

namespace EStudyBase.UI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //   "SearchRoute",
            //   "ara/{term}",
            //   new { controller = "Keyword", action = "Search" }
            //   );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Keyword", action = "Search", id = UrlParameter.Optional }
            );
        }
    }
}
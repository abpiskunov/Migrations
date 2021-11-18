using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OldMvc1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void Application_PostResolveRequestCache(object sender, EventArgs e)
        {
            Microsoft.AspNet.Migrations.MigrationRouter.RouteRequest(Context);
        }
     }
}

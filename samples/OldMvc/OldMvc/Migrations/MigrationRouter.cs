using System;
using System.IO;
using System.Web;

namespace Microsoft.AspNet.Migrations
{
    public static class MigrationRouter
    {
        private static MigrationConfig _migrationConfig;

        private static MigrationConfig GetMigrationJson()
        {
            // TODO synchronize
            if (_migrationConfig is null)
            {
                var migrationsJsonPath = Path.Combine(HttpContext.Current.Server.MapPath("~"), MigrationConfig.FileName);

                var json = File.Exists(migrationsJsonPath)
                    ? JsonHelper.LoadFromFile(migrationsJsonPath)
                    : null;

                _migrationConfig = json?.ToObject<MigrationConfig>() ?? new MigrationConfig();
                
                // TODO Add it on top or have a separate set of stadard routes that we process first.
                _migrationConfig.AddRoute(MigrationConstants.StandardRoutes.Static);
            }

            return _migrationConfig;
        }

        public static void RouteRequest(HttpContext httpContext)
        {
            if (httpContext?.Request?.Path is null)
            {
                return;
            }

            var migrationConfig = GetMigrationJson();

            foreach (var route in migrationConfig.Routes)
            {
                var url = route.Key;
                var options = route.Value;

                if (IsMatching(httpContext.Request.Path, url, options))
                {
                    UseRoute(httpContext, options);
                    break;
                }
            }
        }

        private static void UseRoute(HttpContext httpContext, MigrationRouteOptions options)
        {
            var mapTo = options.Static ? MigrationConstants.StandardRoutes.Static : options.MapTo;

            if (!string.IsNullOrEmpty(mapTo))
            {
                httpContext.Request.ServerVariables[MigrationConstants.ServerVariables.OriginalUrl] = httpContext.Request.Path;

                if (options.Redirect)
                {
                    httpContext.Response.RedirectPermanent(mapTo);
                }
                else
                {
                    httpContext.RewritePath(mapTo);
                }
            }

            httpContext.RemapHandler(null);
        }

        private static bool IsMatching(string requestUrl, string routeUrl, MigrationRouteOptions routeOptions)
        {
            if (routeUrl is null)
            {
                return false;
            }

            bool result = false;
            switch (routeOptions.Match)
            {
                case MigrationRouteMatch.EndsWith:
                    result = requestUrl.EndsWith(routeUrl, StringComparison.OrdinalIgnoreCase);
                    break;
                case MigrationRouteMatch.Regex:
                    // TODO
                    break;
                case MigrationRouteMatch.StartsWith:
                default:
                    result = requestUrl.StartsWith(routeUrl, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            return result;
        }
    }
}
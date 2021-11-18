using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Migrations
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMigrationStaticRoute(this IApplicationBuilder app)
        {
            var rewriteOptions = new RewriteOptions()
                .Add(new StaticFilesRewriteRule());

            app.UseRewriter(rewriteOptions);

            return app;
        }

        //// TODO add extension method that would read static file routing rule from "app root"/migrations.json and apply static files rules here.
        //// Alternative approach for static files would be to use IIS mime rules and request rewrites on legacy side to process all static files in the old app's pipeline.

        //app.UseStaticFiles(new StaticFileOptions
        //{
        //    FileProvider = new PhysicalFileProvider(@"<old app root>\Content"),
        //    RequestPath = "/Content"
        //});
    }
}
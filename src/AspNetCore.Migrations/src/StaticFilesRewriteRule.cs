using Microsoft.AspNet.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace Microsoft.AspNetCore.Migrations
{
    public class StaticFilesRewriteRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var path = request?.Path.Value;
            if (path is null)
            {
                return;
            }

            if (path.Equals(MigrationConstants.StandardRoutes.Static, StringComparison.OrdinalIgnoreCase))
            {
                var newUrl = context.HttpContext.GetServerVariable(MigrationConstants.ServerVariables.OriginalUrl);
                if (!string.IsNullOrEmpty(newUrl))
                {
                    context.HttpContext.Request.Path = newUrl;
                    context.Result = RuleResult.SkipRemainingRules;
                }
            }
        }
    }
}
using Microsoft.AspNetCore.Rewrite;

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

        if (path.Equals("/static", StringComparison.OrdinalIgnoreCase))
        {
            var newUrl = context.HttpContext.GetServerVariable("OriginalUrl"); // request!.Query["path"].ToString();
            if (!string.IsNullOrEmpty(newUrl))
            {
                context.HttpContext.Request.Path = newUrl;
                context.Result = RuleResult.SkipRemainingRules;
            }
        }
    }
}
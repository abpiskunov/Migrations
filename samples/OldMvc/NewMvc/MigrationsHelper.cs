public static class MigrationHelper
{
    public static WebApplicationOptions CreateMigrationWebApplicationOptions(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        var contentRoot = config[WebHostDefaults.ContentRootKey] ?? AppContext.BaseDirectory;
        contentRoot = Path.GetFullPath(contentRoot);

        var options = new WebApplicationOptions
        {
            ContentRootPath = contentRoot
        };

        return options;
    }
}
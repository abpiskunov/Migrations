using Microsoft.AspNetCore.Rewrite;

var options = MigrationHelper.CreateMigrationWebApplicationOptions(args);
var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var rewriteOptions = new RewriteOptions()
    .Add(new StaticFilesRewriteRule());
app.UseRewriter(rewriteOptions);

app.UseStaticFiles();

//// TODO add extension method that would read static file routing rule from "app root"/migrations.json and apply static files rules here.
//// Alternative approach for static files would be to use IIS mime rules and request rewrites on legacy side to process all static files in the old app's pipeline.

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(@"C:\Users\antonpis\source\repos\OldMvc1\OldMvc1\Content"),
//    RequestPath = "/Content"
//});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


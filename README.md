# Prototype for infrastructure API helping with migrationg ASP.NET apps to ASP>NET Core

## Overview

There are multiple scenarios we could help with when new core app runs within existing legacy app:
- Mapping whole controller to ASP.NET Core or map individual routes/actions for existing cotrollers if controller is just to large. 
- Mapping static files in such a way that both portions of the app can use them

## Scenarios

### Mapping controller

In this scenario we can map particular routes or even whole controller and force them to be routed to ASP.Net Core portion of the app. To do so we should use `MigrationsRouter` which reads `migration.json` located in the source project root.

Here are sample file contents.

In global.asax.cs we use new helper:

``` c#
        private void Application_PostResolveRequestCache(object sender, EventArgs e)
        {
            Microsoft.AspNet.Migrations.MigrationRouter.RouteRequest(Context);
        }
```

In the project root we have migration.json which can be eother hand edited by user or tool can mange it and keep up to date by scanning controllers and their actions in the old and new apps:

``` json
{
  "routes": {
    "/home/privacy": {},
    "/home/about": {
      "mapTo": "/home/newabout"
    },
    "/css": {
      "static": true
    },
    "/lib": {
      "static": true
    },
    "/js": {
      "static": true
    },
    "/NewMvc.styles.css": {
      "static": true
    }
  }
}
```

In this example, assuming that old app has a `HomeController` and new core app also has a `HomeController`, presence of route mapping `"/home/privacy": {}` in the `migration.json` suggests that old ASP.NET app should not process this route and it will be handled by new ASP.NET Core app. It can be absolutely new route that never existed before in old app, or an existing one, in which case old controller would never be called.

Similarly, presence of `"/home/about"` suggests that this existing route should be mapped to a new route in the new core app `/home/newabout`. This can be useful if users cannot port full `HomeController` and its corresponding views and models at once and want to do it in portions. Also it can help when users want to "refactor" their routing system along with migration routing old routes to new ones.

### Mapping static files

This scenario is tricky and can have different solution depending on the way users handle static files for their existing app:
- CDN: static files in production naturally could be served via CDN and no routing would be needed same urls will be used on both sides: old and core. Note: we need to confirm with existing customers would inner loop would be looking like for them, i.e. css and js files are not on CDN during development, so how do they test their app locally and then switch to CDN later for production? We could help with inner loop see below.

- Handle static content on both sides: using `migration.json` and it's `static: true` option for a route we could redirect requests for that route to core app and let it's UseStaticFiles middleware handle it. Downside of this approach is it would:
    - either require `<modules runAllManagedModulesForAllRequests="true" />` which makes everything work like magic, but has performance implications
    - or have individual handler registrations for particular static files of interest explicitly like `*.css` or `*.js` which again would route to Core's UseStaticFiles via same routing options in the `migration.json`. Has less performance implications that `runAllManagedModulesForAllRequests`, but requires more granular configuration manually or via tool. However still would use ANCM to handle static file request which is slower than IIS StaticFilesModule.

- suggest users to keep static files on the legacy side until the end of migration and just use IIS StaticFilesModule. New views could just reference those files using legacy app's paths. Note: after migration is complete, users should bring UseStaticFiles and either update all paths in files via some script or just pass alternative path to UseStaticFiles. This action can be toolable: we can auto replace paths in all files with new relative root specified by user or inject 
    ``` c#
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(@"<old app root>\Content"),
            RequestPath = "/Content"
        });
    ```
    if the decided to keep old content in place.

- IIS mime types and rewrite rules: we could inject rules in the web.config that would reroute explicit routes like `/css/...` to `/newapp/wwwwroot/...` on the production side during publish. On the local machines we could inject `runAllManagedModulesForAllRequests`. Downside is it has different setups and might create confusions or surprises. Also it would not handle dynamic `.css` files used for CSS isolation.

## How routing infrastructure works

Here is a sample of `migrations.json` route options:

``` json
{
  "lock": false, // when true, tooling should not touch any routes and never update them in this file.
  "routes": {
    "/home/privacy": {},
    "/home/about": {
      "mapTo": "/home/newabout"
    },
    "some route": {
      "match": "default|startsWith|endsWith|regex",
      "mapTo": "some new url", 
      "redirect": true, // when true request will be permanently redirected to mapTo, when false it will be rewritten on the server
      "static": true, // when true this urls will be routed in such a way that to UseStaticFiles middleware would be able to find it
      "lock": false // can be true or false to suggests that tooling should leave it along and never change this route
    }
  }
}
```

Each route in this file when has no options would be just passed to ASP.NET app to handle on it's side. 

If `mapTo` specified this route would be either redirected or rewritten to a new route on the ASP.NET Core side (depending on `redirect` option's value).

`match` specifies how to match this route with `HttpContext.Request.Path`.

`static` when true enables some magic to route to static files on ASP.NET Core side:
- on legacy side `MigrationsRouter` would rewrite request to map to special route `/static` and adds a server variable to this request containing original `HttpContext.Request.Path`,
- then when new request `/static` is handled again (still on legacy side) `MigrationRouter` routes it to the ASP.NET Core side
- there we injected `app.UseMigrationStaticRoute();` extension right before `app.UseStaticFiles();` which when see `/static` route, rewrites it to original route obtained from a server variable.
- then original route is going again through the middleware and is handled by `UseStaticFiles()`

To make this flow available we need `runAllManagedModulesForAllRequests=true` or individual handler registration in the web.config as mentioned above.

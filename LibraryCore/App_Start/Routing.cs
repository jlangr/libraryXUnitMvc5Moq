using Microsoft.AspNetCore.Builder;

namespace LibraryCore
{
    // public class RouteConfig
    // {
    //     public static void RegisterRoutes(RouteCollection routes)
    //     {
    //         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
    //         routes.MapRoute(
    //             name: "Default",
    //             url: "{controller}/{action}/{id}",
    //             defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
    //         );
    //     }

    public static class Routing
    {
        public static void Include(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
                {
                    routes.MapRoute("Default", "", new
                    {
                        controller = "Home",
                        action = "Index"
                    });
                }
            );
        }
    }
}

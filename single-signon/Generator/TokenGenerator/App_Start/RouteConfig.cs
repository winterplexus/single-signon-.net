//
//  RouteConfig.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System.Web.Mvc;
using System.Web.Routing;

namespace TokenGenerator
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
//
//  FilterConfig.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System.Web.Mvc;

namespace TokenGenerator
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
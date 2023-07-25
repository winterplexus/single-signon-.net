//
//  HomeController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System.Web.Mvc;

namespace TokenGenerator.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("/")]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
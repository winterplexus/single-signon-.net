//
//  HomeController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Authenticator.Log;
using Authenticator.Saml;
using Authenticator.Utility;
using Authenticator.ViewModels;

namespace Authenticator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> controllerLogger;

        public HomeController(ILogger<HomeController> logger)
        {
            controllerLogger = logger;
        }

        public ActionResult Index()
        {
            controllerLogger.LogTrace($"{ControllerMessages.Entering}");

            var samlEndpoint = ApplicationSettings.GetStringValue("Saml", "SamlEndpoint");

            try
            {
                var request = new AuthenticationRequest(
                    ApplicationSettings.GetStringValue("Saml", "EntityId"),
                    ApplicationSettings.GetStringValue("Saml", "AssertionConsumerService")
                );

                controllerLogger.LogTrace($"{ControllerMessages.Leaving}: redirect to SAML endpoint");

                return Redirect(request.GetRedirectUrl(samlEndpoint));
            }
            catch (Exception ex)
            {
                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), ex);
            }

            controllerLogger.LogTrace($"{ControllerMessages.Leaving}");

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
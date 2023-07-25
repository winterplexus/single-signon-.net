//
//  HomeController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
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
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            LogTraceMessage($"{ControllerMessages.Entering}");

            var samlEndpoint = ApplicationSettings.GetStringValue("Saml", "SamlEndpoint");

            try
            {
                var request = new AuthenticationRequest(
                    ApplicationSettings.GetStringValue("Saml", "EntityId"),
                    ApplicationSettings.GetStringValue("Saml", "AssertionConsumerService")
                );

                LogTraceMessage($"{ControllerMessages.Leaving}: redirect to SAML endpoint");

                return Redirect(request.GetRedirectUrl(samlEndpoint).ToString());
            }
            catch (Exception ex)
            {
                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), ex);
            }

            LogTraceMessage($"{ControllerMessages.Leaving}");

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "{message}")]
        public partial void LogTraceMessage(string message);
    }
}
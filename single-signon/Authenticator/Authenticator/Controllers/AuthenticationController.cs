//
//  AuthenticationController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Authenticator.Log;
using Authenticator.Models;
using Authenticator.ViewModels;
using Authenticator.Saml;
using Authenticator.Utility;
using RestSharp;

namespace Authenticator.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> controllerLogger;
        private readonly int callServiceLimit;
        private readonly int callServiceDelayTime;
        private readonly string tokenGeneratorEndpoint;
        private readonly int tokenGeneratorEndpointTimeout;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            controllerLogger = logger;
            callServiceLimit = ApplicationSettings.GetNumberValue("Saml", "CallServiceLimit");
            callServiceDelayTime = ApplicationSettings.GetNumberValue("Saml", "CallServiceDelayTime");
            tokenGeneratorEndpoint = ApplicationSettings.GetStringValue("Saml", "TokenGeneratorEndpoint");
            tokenGeneratorEndpointTimeout = ApplicationSettings.GetNumberValue("Saml", "TokenGeneratorEndpointTimeout");
        }

        public IActionResult Saml()
        {
            controllerLogger.LogTrace($"{ControllerMessages.Entering}");

            var viewModel = new AuthenticationViewModel();

            var samlCertificateFilePath = ApplicationSettings.GetStringValue("Saml", "CertificateFilePath");

            if (!System.IO.File.Exists(samlCertificateFilePath))
            {
                const string message = "SAML certificate file does not exist";

                controllerLogger.LogTrace($"{ControllerMessages.ErrorIndicator}: {message}");

                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                return View(viewModel);
            }

            try
            {
                var samlCertificate = System.IO.File.ReadAllText(samlCertificateFilePath);
                if (string.IsNullOrEmpty(samlCertificate))
                {
                    const string message = "SAML certificate file is null or empty";

                    controllerLogger.LogTrace($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }

                var samlResponse = Request.Form["SAMLResponse"];
                if (string.IsNullOrEmpty(samlResponse))
                {
                    const string message = "SAML response is null or empty";

                    controllerLogger.LogTrace($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }

                using var response = new AssertionResponse(samlCertificate, samlResponse);
                if (response.IsValid())
                {
                    ActivityLog.WriteAuthentication(response.GetNameId(), response.GetFirstName(), response.GetLastName(), response.GetEmail());

                    var token = GenerateToken(response.GetNameId(), response.GetEmail());

                    if (string.IsNullOrEmpty(token))
                    {
                        viewModel.ErrorMessage = "SAML token generation failed";

                        controllerLogger.LogTrace($"{ControllerMessages.ErrorIndicator}: {viewModel.ErrorMessage}");

                        EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{viewModel.ErrorMessage}");

                        return View(viewModel);
                    }

                    controllerLogger.LogTrace($"{ControllerMessages.Success}: SAML token generated");

                    viewModel.UserName = $"{response.GetFirstName()} {response.GetLastName()}";

                    controllerLogger.LogTrace($"{ControllerMessages.Success}: redirect and POST");

                    this.RedirectAndPost(token);
                }
                else
                {
                    const string message = "SAML assertion response is null";

                    controllerLogger.LogTrace($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{ControllerMessages.GeneralException}", ex);
            }

            controllerLogger.LogTrace($"{ControllerMessages.Leaving}");

            return View(viewModel);
        }

        private string GenerateToken(string username, string email)
        {
            var trials = 0;

            var samlTokenRequest = new SamlTokenRequest { Username = username, Email = email };

            var uriBuilder = new UriBuilder(tokenGeneratorEndpoint) { Path = "/saml/generate" };

            var restClient = new RestClient(new Uri(uriBuilder.ToString())) { Timeout = tokenGeneratorEndpointTimeout };

            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(samlTokenRequest);

            do
            {
                var response = restClient.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Thread.Sleep(callServiceDelayTime);
                    continue;
                }
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    var samlTokenResponse = JsonSerializer.Deserialize<SamlTokenResponse>(response.Content, options);
                    if (samlTokenResponse != null)
                    {
                        if (string.IsNullOrEmpty(samlTokenResponse.SamlToken))
                        {
                            return string.Empty;
                        }

                        var tokenBytes = Encoding.UTF8.GetBytes(samlTokenResponse.SamlToken);
                        return Convert.ToBase64String(tokenBytes);
                    }
                    return string.Empty;
                }
            } while (trials++ < callServiceLimit);

            return string.Empty;
        }
    }
}
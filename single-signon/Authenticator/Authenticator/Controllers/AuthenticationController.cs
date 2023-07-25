//
//  AuthenticationController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Authenticator.Log;
using Authenticator.Models;
using Authenticator.Saml;
using Authenticator.Utility;
using Authenticator.ViewModels;
using System.Threading.Tasks;

namespace Authenticator.Controllers
{
    public partial class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly string _tokenGeneratorEndpoint;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
            _tokenGeneratorEndpoint = ApplicationSettings.GetStringValue("Saml", "TokenGeneratorEndpoint");
        }

        public async Task<IActionResult> Saml()
        {
            LogTraceMessage($"{ControllerMessages.Entering}");

            var viewModel = new AuthenticationViewModel();

            var samlCertificateFilePath = ApplicationSettings.GetStringValue("Saml", "CertificateFilePath");

            if (!System.IO.File.Exists(samlCertificateFilePath))
            {
                const string message = "SAML certificate file does not exist";

                LogTraceMessage($"{ControllerMessages.ErrorIndicator}: {message}");

                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                return View(viewModel);
            }

            try
            {
                var samlCertificate = System.IO.File.ReadAllText(samlCertificateFilePath);
                if (string.IsNullOrEmpty(samlCertificate))
                {
                    const string message = "SAML certificate file is null or empty";

                    LogTraceMessage($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }

                var samlResponse = Request.Form["SAMLResponse"];
                if (string.IsNullOrEmpty(samlResponse))
                {
                    const string message = "SAML response is null or empty";

                    LogTraceMessage($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }

                using var response = new AssertionResponse(samlCertificate, samlResponse);
                if (response.IsValid())
                {
                    ActivityLog.WriteAuthentication(response.GetNameId(), response.GetFirstName(), response.GetLastName(), response.GetEmail());

                    var token = await GenerateToken(response.GetNameId(), response.GetEmail()).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(token))
                    {
                        viewModel.ErrorMessage = "SAML token generation failed";

                        LogTraceMessage($"{ControllerMessages.ErrorIndicator}: {viewModel.ErrorMessage}");

                        EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{viewModel.ErrorMessage}");

                        return View(viewModel);
                    }

                    LogTraceMessage($"{ControllerMessages.Success}: SAML token generated");

                    viewModel.UserName = $"{response.GetFirstName()} {response.GetLastName()}";

                    LogTraceMessage($"{ControllerMessages.Success}: redirect and POST");

                    this.RedirectAndPost(token);
                }
                else
                {
                    const string message = "SAML assertion response is null";

                    LogTraceMessage($"{ControllerMessages.ErrorIndicator}: {message}");

                    EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{message}");

                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), $"{ControllerMessages.GeneralException}", ex);
            }

            LogTraceMessage($"{ControllerMessages.Leaving}");

            return View(viewModel);
        }

        private async Task<string> GenerateToken(string username, string email)
        {
            var samlTokenRequest = new SamlTokenRequest { Username = username, Email = email };

            using HttpClient client = new();
            client.BaseAddress = new Uri(_tokenGeneratorEndpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonData = JsonConvert.SerializeObject(samlTokenRequest);
            using var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(new Uri("/saml/generate"), contentData).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var stringData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var samlTokenResponse = JsonConvert.DeserializeObject<SamlTokenResponse>(stringData);

            var tokenBytes = Encoding.UTF8.GetBytes(samlTokenResponse.SamlToken);
            return Convert.ToBase64String(tokenBytes);
        }

        [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "{message}")]
        public partial void LogTraceMessage(string message);
    }
}
//
//  Extensions.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Authenticator.Utility;

namespace Authenticator.Controllers
{
    public static class ControllerExtensions
    {
        private const string _template = @"<html><body onload='document.forms[""form""].submit()'><form name='form' action='{0}' method='post'><input type=""hidden"" id=""token"" name=""token"" value=""{1}""></form></body></html>";

        public static void RedirectAndPost(this Controller controller, string token)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            var location = ApplicationSettings.GetStringValue("Saml", "TargetEndpoint");

            var html = string.Format(CultureInfo.InvariantCulture, _template, location, token);
            var encodedHtml = Encoding.UTF8.GetBytes(html);

            controller.Response.Headers.Add("Location", location);
            controller.Response.Body.WriteAsync(encodedHtml).AsTask();
        }
    }
}
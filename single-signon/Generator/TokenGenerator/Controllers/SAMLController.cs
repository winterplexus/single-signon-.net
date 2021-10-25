//
//  SAMLController.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System.Web.Http;
using TokenGenerator.Log;
using TokenGenerator.Models;
using TokenGenerator.Saml;

namespace TokenGenerator.Controllers
{
    public class SAMLController : ApiController
    {
        [HttpPost]
        [Route("saml/generate")]
        public SamlTokenResponse Generate([FromBody] SamlTokenRequest request)
        {
            var response = new SamlTokenResponse();

            var token = SAMLToken.Generate(request.Username, request.Email);
            if (token == null)
            {
                token = string.Empty;
            }

            response.SamlToken = token;

            ActivityLog.WriteGeneration(request.Username, request.Email, token);

            return response;
        }
    }
}
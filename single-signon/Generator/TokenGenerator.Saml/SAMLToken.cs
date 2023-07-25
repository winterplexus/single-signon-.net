//
//  SAMLToken.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using TokenGenerator.Log;
using TokenGenerator.Utility;

namespace TokenGenerator.Saml
{
    public static class SAMLToken
    {
        public static string Generate(string username, string email)
        {
            var now = DateTime.Now;

            try
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
                    Lifetime = new Lifetime(now, now + new TimeSpan(8, 0, 0)),
                    AppliesToAddress = Assistant.GetConfigurationValue("AppliesToAddress"),
                    TokenIssuerName = Assistant.GetConfigurationValue("TokenIssuerName")
                };

                var claimList = new List<Claim>()
                {
                    new Claim("Username", username),
                    new Claim("Email", email)
                };

                tokenDescriptor.Subject = new ClaimsIdentity(claimList, "Active Directory");

                var certificate = new X509Certificate2(Assistant.GetConfigurationValue("CertificateFilePath"), Assistant.GetConfigurationValue("CertificateFilePassword"));

                var keyIdentifier = new SecurityKeyIdentifier(new X509SecurityToken(certificate).CreateKeyIdentifierClause<X509SubjectKeyIdentifierClause>());

                var signingCredentials = new X509SigningCredentials(certificate, keyIdentifier);

                tokenDescriptor.SigningCredentials = signingCredentials;

                var securityTokenHandler = new Saml2SecurityTokenHandler();

                using (var stringWriter = new StringWriter())
                {
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        if (securityTokenHandler.CreateToken(tokenDescriptor) is Saml2SecurityToken securityToken)
                        {
                            securityTokenHandler.WriteToken(writer, securityToken);
                        }
                    }
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEvent(Assistant.GetMethodFullName(MethodBase.GetCurrentMethod()), "SAML token generation failed", ex);
            }

            return null;
        }
    }
}
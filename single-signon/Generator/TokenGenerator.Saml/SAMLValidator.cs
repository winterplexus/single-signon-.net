//
//  SAMLValidator.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Xml;

namespace TokenGenerator.Saml
{
    public static class SAMLValidator
    {
        public static bool ValidateSAMLToken(string tokenString, string expectedIssuer, string expectedAudience, string certificatePath, Dictionary<string, string> claimsDictionary, out string error)
        {
            error = null;

            try
            {
                var tokenHandler = CreateTokenHandler(expectedIssuer, expectedAudience, certificatePath);
                ValidateAndGetClaims(ParseToken(tokenString, tokenHandler), tokenHandler, claimsDictionary);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static Saml2SecurityTokenHandler CreateTokenHandler(string expectedIssuer, string expectedAudience, string certificatePath)
        {
            var tokenResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(new List<SecurityToken>()
            {
                 new X509SecurityToken(new X509Certificate2(certificatePath))
            }.AsReadOnly(), true);

            var audienceRestriction = new AudienceRestriction(AudienceUriMode.Always);
            audienceRestriction.AllowedAudienceUris.Add(new Uri(expectedAudience));

            var issuerNameRegistry = new SAMLIssuerNameRegistry()
            {
                ExpectedIssuerName = expectedIssuer
            };

            var securityTokenHandler = new Saml2SecurityTokenHandler
            {
                Configuration = new SecurityTokenHandlerConfiguration()
                {
                    AudienceRestriction = audienceRestriction,
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                    IssuerTokenResolver = tokenResolver,
                    CertificateValidator = X509CertificateValidator.None,
                    IssuerNameRegistry = issuerNameRegistry
                }
            };

            return securityTokenHandler;
        }

        private static Saml2SecurityToken ParseToken(string tokenString, Saml2SecurityTokenHandler tokenHandler)
        {
            using (var stringReader = new StringReader(tokenString))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    return tokenHandler.ReadToken(reader) as Saml2SecurityToken;
                }
            }
        }

        private static void ValidateAndGetClaims(Saml2SecurityToken token, Saml2SecurityTokenHandler tokenHandler, Dictionary<string, string> claimsDictionary)
        {
            if (claimsDictionary == null)
            {
                throw new Exception("Please specify appropriate claims to retrieve");
            }

            var claimsIdentity = tokenHandler.ValidateToken(token).First();

            foreach (var dictionaryItem in claimsDictionary.Keys.ToList())
            {
                var key = dictionaryItem;
                var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type.Equals(key.ToLower(), StringComparison.OrdinalIgnoreCase));
                claimsDictionary[key] = claim != null ? claim.Value : string.Empty;
            }
        }
    }
}
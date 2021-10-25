//
//  SAMLIssuerNameRegistry.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using System.IdentityModel.Tokens;

[assembly: CLSCompliant(true)]
namespace TokenGenerator.Saml
{
    internal class SAMLIssuerNameRegistry : IssuerNameRegistry
    {
        public string ExpectedIssuerName { get; set; }

        public override string GetIssuerName(SecurityToken securityToken)
        {
            if (!(securityToken is X509SecurityToken token))
            {
                throw new SecurityTokenValidationException("Invalid token");
            }
            return token.Certificate.FriendlyName;
        }

        public override string GetIssuerName(SecurityToken securityToken, string requestedIssuerName)
        {
            if (requestedIssuerName != this.ExpectedIssuerName)
            {
                throw new SecurityTokenValidationException("Untrusted issuer token");
            }
            return requestedIssuerName;
        }

        public override string GetWindowsIssuerName()
        {
            return "WINDOWS AUTHORITY";
        }
    }
}
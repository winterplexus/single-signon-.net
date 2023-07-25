//
//  SamlTokenRequest.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023 (SMC)
//
namespace Authenticator.Models
{
    public class SamlTokenRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
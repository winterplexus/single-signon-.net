//
//  AuthenticationViewModel.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System.ComponentModel.DataAnnotations;

namespace Authenticator.ViewModels
{
    public class AuthenticationViewModel
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Error Message")]
        public string ErrorMessage { get; set; }
    }
}
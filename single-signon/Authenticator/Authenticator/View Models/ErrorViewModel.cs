//
//  ErrorViewModel.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
namespace Authenticator.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
//
//  ActivityLog.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System;
using NLog;

[assembly: CLSCompliant(true)]
namespace Authenticator.Log
{
    public static class ActivityLog
    {
        private const string _singleIndent = "  -";
        private const string _doubleIndent = "    -";

        public static void WriteAuthentication(string nameId, string firstName, string lastName, string email)
        {
            var logger = LogManager.GetLogger("Activity");

            logger.Info("Azure AD authentication:");
            logger.Info($"{_singleIndent} SAML attributes:");
            logger.Info($"{_doubleIndent} ID: {nameId}");
            logger.Info($"{_doubleIndent} name: {firstName} {lastName}");
            logger.Info($"{_doubleIndent} email: {email}");
        }
    }
}
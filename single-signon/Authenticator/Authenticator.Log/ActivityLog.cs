//
//  ActivityLog.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using NLog;

[assembly: CLSCompliant(true)]
namespace Authenticator.Log
{
    public static class ActivityLog
    {
        private const string singleIndent = "  -";
        private const string doubleIndent = "    -";

        public static void WriteAuthentication(string nameId, string firstName, string lastName, string email)
        {
            var logger = LogManager.GetLogger("Activity");

            logger.Info("Azure AD authentication:");
            logger.Info($"{singleIndent} SAML attributes:");
            logger.Info($"{doubleIndent} ID: {nameId}");
            logger.Info($"{doubleIndent} name: {firstName} {lastName}");
            logger.Info($"{doubleIndent} email: {email}");
        }
    }
}
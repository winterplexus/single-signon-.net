//
//  ActivityLog.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using NLog;
using TokenGenerator.Utility;

[assembly: CLSCompliant(true)]
namespace TokenGenerator.Log
{
    public static class ActivityLog
    {
        private const string singleIndent = "  -";
        private const string doubleIndent = "    -";

        public static void WriteGeneration(string nameId, string email, string token)
        {
            var logger = LogManager.GetLogger("Activity");

            logger.Info("SAML token generation:");
            logger.Info($"{singleIndent} SAML attributes (parameters):");
            logger.Info($"{doubleIndent} ID: {nameId}");
            logger.Info($"{doubleIndent} email: {email}");
            logger.Info($"{singleIndent} SAML token (generated):");
            logger.Info($"{doubleIndent} token: {ReplaceControlCharacters(token)}");
        }

        private static string ReplaceControlCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.Replace(EscapeCharacters.Backspace, "").
                                                               Replace(EscapeCharacters.FormFeed, "|").
                                                               Replace(EscapeCharacters.Linefeed, "").
                                                               Replace(EscapeCharacters.CarriageReturn, "").
                                                               Replace(EscapeCharacters.HorizontalTab, " ").
                                                               Replace(EscapeCharacters.VerticalTab, "|");
        }
    }
}
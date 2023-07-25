//
//  EventLog.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System;
using NLog;
using Authenticator.Utility;

namespace Authenticator.Log
{
    public static class EventLog
    {
        private const string _singleIndent = "  -";

        public static void WriteEvent(string source, string message)
        {
            var logger = LogManager.GetLogger("Events");

            logger.Info($"source method (including namespace and class): {source}");
            logger.Info($"message: {message}");
        }

        public static void WriteEvent(string source, Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            var logger = LogManager.GetLogger("Events");

            logger.Info($"source method (including namespace and class): {source}");
            logger.Info($"{_singleIndent} exception: {ReplaceControlCharacters(ex.Message)}");
            logger.Info($"{_singleIndent} exception stack trace: {ReplaceControlCharacters(ex.StackTrace)}");
        }

        public static void WriteEvent(string source, string message, Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            var logger = LogManager.GetLogger("Events");

            logger.Info($"source method (including namespace and class): {source}");
            logger.Info($"message: {message}");
            logger.Info($"{_singleIndent} exception: {ReplaceControlCharacters(ex.Message)}");
            logger.Info($"{_singleIndent} exception stack trace: {ReplaceControlCharacters(ex.StackTrace)}");
        }

        private static string ReplaceControlCharacters(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.Replace(EscapeCharacters.Backspace, "", StringComparison.InvariantCulture).
                                                               Replace(EscapeCharacters.FormFeed, "|", StringComparison.InvariantCulture).
                                                               Replace(EscapeCharacters.Linefeed, "", StringComparison.InvariantCulture).
                                                               Replace(EscapeCharacters.CarriageReturn, "", StringComparison.InvariantCulture).
                                                               Replace(EscapeCharacters.HorizontalTab, " ", StringComparison.InvariantCulture).
                                                               Replace(EscapeCharacters.VerticalTab, "|", StringComparison.InvariantCulture);
        }
    }
}
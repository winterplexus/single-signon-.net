//
//  EventLog.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using NLog;
using TokenGenerator.Utility;

namespace TokenGenerator.Log
{
    public static class EventLog
    {
        private const string singleIndent = "  -";

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
            logger.Info($"{singleIndent} exception: {ReplaceControlCharacters(ex.Message)}");
            logger.Info($"{singleIndent} exception stack trace: {ReplaceControlCharacters(ex.StackTrace)}");
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
            logger.Info($"{singleIndent} exception: {ReplaceControlCharacters(ex.Message)}");
            logger.Info($"{singleIndent} exception stack trace: {ReplaceControlCharacters(ex.StackTrace)}");
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
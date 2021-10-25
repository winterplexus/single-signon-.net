//
//  Assistant.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;

[assembly: CLSCompliant(true)]
namespace TokenGenerator.Utility
{
    public static class Assistant
    {
        public static string GetConfigurationValue(string name)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[name]) ? ConfigurationManager.AppSettings[name] : string.Empty;
        }

        public static string GetMethodFullName(MethodBase methodInfo)
        {
            if (methodInfo == null)
            {
                return string.Empty;
            }
            return methodInfo.DeclaringType != null ? string.Format(CultureInfo.InvariantCulture, "{0}.{1}()", methodInfo.DeclaringType.FullName, methodInfo.Name) : null;
        }
    }
}
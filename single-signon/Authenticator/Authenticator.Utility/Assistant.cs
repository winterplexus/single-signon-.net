//
//  Assistant.cs
//
//  Copyright (c) Wiregrass Code Technology 2021-2023
//
using System.Globalization;
using System.Reflection;

namespace Authenticator.Utility
{
    public static class Assistant
    {
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
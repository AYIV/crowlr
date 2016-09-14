using System;

namespace crowlr.core
{
    public static class StringExtensions
    {
        public static string HrefParam(this string @this, string paramName, string root = "")
        {
            if (string.IsNullOrWhiteSpace(@this))
                return null;
            
            return new Uri(root + @this).Parameters(paramName);
        }
    }
}

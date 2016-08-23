using System;
using System.Web;

namespace crowlr.core
{
    public static class UriExtensions
    {
        public static string Parameters(this Uri @this, string parameter)
        {
            return HttpUtility
                .ParseQueryString(@this.Query)
                .Get(parameter);
        }
    }
}

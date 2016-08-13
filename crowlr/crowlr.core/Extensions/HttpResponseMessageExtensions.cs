using System.Net.Http;

namespace crowlr.core
{
    public static class HttpResponseMessageExtensions
    {
        public static string Content(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}

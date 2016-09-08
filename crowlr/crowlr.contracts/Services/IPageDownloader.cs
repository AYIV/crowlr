using System;
using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IPageDownloader : IDisposable
    {
        IPage Get(string url, string[,] headers, ResponseType type = ResponseType.Html);

        IPage Get(Uri uri, string[,] headers, ResponseType type = ResponseType.Html);

        IPage Get(string url, IDictionary<string, string> headers = null, ResponseType type = ResponseType.Html);

        IPage Get(Uri uri, IDictionary<string, string> headers = null, ResponseType type = ResponseType.Html);

        IPage Post(string url, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null);

        IPage Post(Uri uri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null);
    }
}

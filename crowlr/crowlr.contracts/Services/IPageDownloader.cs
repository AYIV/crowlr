using System;
using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IPageDownloader
    {
        IPage Get(string url, IDictionary<string, string> headers = null);

        IPage Get(Uri uri, IDictionary<string, string> headers = null);

        IPage Post(string url, IDictionary < string, string> parameters = null, IDictionary<string, string> headers = null);

        IPage Post(Uri uri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null);
    }
}

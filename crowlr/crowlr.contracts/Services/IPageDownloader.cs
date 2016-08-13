using System;
using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IPageDownloader
    {
        IPage Get(string url);

        IPage Get(Uri uri);

        IPage Post(string url, IDictionary<string, string> parameters = null);

        IPage Post(Uri uri, IDictionary<string, string> parameters = null);
    }
}

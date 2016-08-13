using crowlr.contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace crowlr.core
{
    public class PageDownloader : IPageDownloader, IDisposable
    {
        private HttpClient client;
        private HttpClient Client
        {
            get
            {
                return client ?? (client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() }));
            }
        }

        public PageDownloader()
        {
        }

        public IPage Get(Uri uri)
        {
            return new Page(
                Client.GetAsync(uri).Result.Content()
            );
        }

        public IPage Get(string url)
        {
            return Get(new Uri(Uri.EscapeUriString(url)));
        }

        public IPage Post(Uri uri, IDictionary<string, string> parameters = null)
        {
            return new Page(
                Client.PostAsync(uri, new FormUrlEncodedContent(parameters)).Result.Content()
            );
        }

        public IPage Post(string url, IDictionary<string, string> parameters = null)
        {
            return Post(new Uri(Uri.EscapeUriString(url)), parameters);
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}

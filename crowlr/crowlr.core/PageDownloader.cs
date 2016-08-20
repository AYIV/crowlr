using crowlr.contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace crowlr.core
{
    public class PageDownloader : IPageDownloader, IDisposable
    {
        private HttpClient client;
        //TODO: do it private
        public HttpClient Client
        {
            get
            {
                return client ?? (client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() }));
            }
        }

        public PageDownloader()
        {
        }

        public IPage Get(Uri uri, IDictionary<string, string> headers = null)
        {
            AddHeaders(null, headers);            

            return new Page(
                Client.GetAsync(uri).Result.Content()
            );
        }

        public IPage Get(string url, IDictionary<string, string> headers = null)
        {
            return Get(new Uri(Uri.EscapeUriString(url)), headers);
        }

        public IPage Post(Uri uri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            var content = new FormUrlEncodedContent(parameters);

            AddHeaders(content, headers);

            return new Page(
                Client.PostAsync(uri, new FormUrlEncodedContent(parameters)).Result.Content()
            );
        }

        public IPage Post(string url, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            return Post(new Uri(Uri.EscapeUriString(url)), parameters, headers);
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        private void AddHeaders(HttpContent content, IDictionary<string, string> headers = null)
        {
            if (headers == null || !headers.Any())
                return;

            if (content != null)
            {
                foreach (var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }

                return;
            }

            foreach (var header in headers)
            {
                if (Client.DefaultRequestHeaders.Contains(header.Key))
                    Client.DefaultRequestHeaders.Remove(header.Key);

                Client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}

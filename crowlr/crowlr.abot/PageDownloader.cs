using Abot.Crawler;
using Abot.Poco;
using crowlr.contracts;
using HtmlAgilityPack;
using System;
using System.Threading;
using System.Collections.Generic;

namespace crowlr.abot
{
    public class PageDownloader : IPageDownloader
    {
        private CrawlConfiguration Config { get; set; }

        public PageDownloader()
        {
            Config = new CrawlConfiguration
            {
                UserAgentString = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko",
                DownloadableContentTypes = "text/html, text/plain",
                IsRespectRobotsDotTextEnabled = false,
                MaxCrawlDepth = 1,
                IsSendingCookiesEnabled = true
            };
        }

        public IPage Get(string url, IDictionary<string, string> headers = null)
        {
            return Get(new Uri(url));
        }
        
        public IPage Get(Uri uri, IDictionary<string, string> headers = null)
        {
            bool crawlDisallowed = false;
            IPage page = null;

            using (var crawler = new PoliteWebCrawler(Config))
            {
                crawler.PageCrawlCompleted += (sender, e) => page = new Page(e.CrawledPage.HtmlDocument);
                crawler.PageCrawlDisallowed += (sender, e) => crawlDisallowed = true;

                crawler.Crawl(uri);

                while (page == null && !crawlDisallowed)
                    Thread.Sleep(100);

                //TODO:: stop it somehow.
            }

            return page;
        }

        public IPage GetPageEx(string htmlContent)
        {
            var document = new HtmlDocument();

            document.LoadHtml(htmlContent);

            return new Page(document);
        }

        public IPage Post(string url, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public IPage Post(Uri uri, IDictionary<string, string> parameters = null, IDictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }
    }
}

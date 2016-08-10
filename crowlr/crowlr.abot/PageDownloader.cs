using Abot.Crawler;
using Abot.Poco;
using crowlr.contracts;
using System;
using HtmlAgilityPack;
using System.Threading;

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
                MaxCrawlDepth = 1
            };
        }

        public IPage GetPage(string url)
        {
            return GetPage(new Uri(url));
        }

        public IPage GetPage(Uri uri)
        {
            var page = CrawlPage(uri);

            return new Page(page);
        }

        private HtmlDocument CrawlPage(Uri uri)
        {
            bool crawlDisallowed = false;
            HtmlDocument document = null;

            using (var crawler = new PoliteWebCrawler(Config))
            {
                crawler.PageCrawlCompleted += (sender, e) => document = e.CrawledPage.HtmlDocument;
                crawler.PageCrawlDisallowed += (sender, e) => crawlDisallowed = true;

                crawler.Crawl(uri);

                while (document == null && !crawlDisallowed)
                    Thread.Sleep(100);

                //TODO:: stop it somehow.
            }

            return document;
        }
    }
}

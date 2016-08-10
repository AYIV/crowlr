using crowlr.abot;
using System;

namespace crowlr.console
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloader = new PageDownloader();

            var page = downloader.GetPage("https://www.linkedin.com/uas/login");
            var node = page.GetNodeById("loginCsrfParam-login");

            Console.WriteLine(node.Value);
            Console.ReadLine();
        }
    }
}

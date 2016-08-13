using crowlr.core;
using System;
using System.Collections.Generic;

namespace crowlr.console
{
    class Program
    {
        static void Main(string[] args)
        {
            //var page = downloader.GetPage("https://www.linkedin.com/vsearch/f?type=all&keywords=korol'kova&orig=GLHD&rsid&pagekey=oz-winner&trkInfo&search=Поиск");

            using (var downloader = new PageDownloader())
            {
                var loginPage = downloader.Get("https://www.linkedin.com/uas/login");
                var login = loginPage.GetNodeById("loginCsrfParam-login");
                var sourceAlias = loginPage.GetNodeById("sourceAlias-login");

                var homePage = downloader.Post("https://www.linkedin.com/uas/login-submit", new Dictionary<string, string>
                {
                    { "session_key", "korolkova.julia.hr@gmail.com" },
                    { "session_password", "1qaz2wsx3edc" },
                    { "isJsEnabled", "false" },
                    { "loginCsrfParam", login.Value },
                    { "sourceAlias", sourceAlias.Value },
                    { "submit", "Войти" }
                });

                var csrfToken = homePage.GetNodeById("jet-csrfToken").Content;
                var logoutPage = downloader.Get($@"https://www.linkedin.com/uas/logout?session_full_logout=&csrfToken={csrfToken}&trk=nav_account_sub_nav_signout");

                var content = logoutPage.ToString();
            }

            Console.ReadLine();
        }
    }
}

using crowlr.core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

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

                var orig = homePage.GetNodeByName("orig").Value;
                var rsid = homePage.GetNodeByName("rsid").Value;
                var pageKey = homePage.GetNodeByName("pageKey").Content;
                var trkInfo = homePage.GetNodeByName("trkInfo").Value;

                //var searchPage = downloader.Get($@"https://www.linkedin.com/vsearch/f?type=all&keywords=korol'kova&orig={orig}&rsid={rsid}&pagekey={pageKey}&trkInfo={trkInfo}");
                //var c = searchPage.ToString();
                Console.Write("Keywords: ");
                var keywords = Console.ReadLine();
                Console.Write("Total: ");
                var total = Convert.ToInt32(Console.ReadLine());
                Console.Write("Start: ");
                var start = Convert.ToInt32(Console.ReadLine());

                var processed = 0;
                while (processed < total)
                {
                    var searchPage = downloader.Get(
                        $@"https://www.linkedin.com/voyager/api/search/hits?q=people&keywords={keywords}&origin=HISTORY&start={start}&count=20",
                        new Dictionary<string, string>
                        {
                        { "Csrf-Token", csrfToken }
                        });

                    var t = JsonConvert.DeserializeObject<dynamic>(searchPage.ToString());
                    var searchResult = (t.elements as IEnumerable<dynamic>)
                        .Where(x => x.hitInfo["com.linkedin.voyager.search.SearchProfile"] != null)
                        .Select(x => x.hitInfo["com.linkedin.voyager.search.SearchProfile"])
                        .Where(x => x.distance.value == "DISTANCE_2" || x.distance.value == "DISTANCE_3")
                        .Select(x =>
                        {
                            var id = (x["miniProfile"]["entityUrn"].ToString().Split(':') as string[]).Last();
                            return new { x.miniProfile.firstName, x.miniProfile.lastName, id, x.miniProfile.trackingId };
                        })
                        .ToList();

                    Console.WriteLine(Environment.NewLine + $@"[{start}] Profiles -> {searchResult.Count} of 20");
                    searchResult
                        .ForEach(x =>
                        {
                            Thread.Sleep(new Random().Next(1000, 5000));
                            var json = string.Format(@"{{""trackingId"":""{0}"",""invitations"":[],""invitee"":{{""com.linkedin.voyager.growth.invitation.InviteeProfile"":{{""profileId"":""{1}""}}}}}}", x.trackingId, x.id);

                            var result = downloader.Client.PostAsync($@"https://www.linkedin.com/voyager/api/growth/normInvitations", new StringContent(json, System.Text.Encoding.UTF8, "application/json")).Result;
                            Console.WriteLine($@"[{result.StatusCode} :: {x.id}] {x.firstName} {x.lastName}");
                        });

                    processed += searchResult.Count;

                    Console.WriteLine($@"Processed -> {processed} of {total}");

                    start += 20;

                    if (!searchResult.Any())
                        break;
                }
                // https://www.linkedin.com/m/profile/ACoAABdVV-gBgEkUGQaDXcOrRbQrcJK-wXPJlsw/

                var logoutPage = downloader.Get($@"https://www.linkedin.com/uas/logout?session_full_logout=&csrfToken={csrfToken}&trk=nav_account_sub_nav_signout");

                var content = logoutPage.ToString();

                // https://wwww.linkedin.com/people/invites/scroll?pageNum=0&pagesize=50
            }

            Console.ReadLine();
        }
    }
}

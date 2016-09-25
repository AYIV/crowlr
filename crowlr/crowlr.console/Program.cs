using crowlr.contracts;
using crowlr.core;
using crowlr.linkedin;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;

// https://www.linkedin.com/people/invites/scroll?pageNum=0&pagesize=50
// https://www.linkedin.com/m/profile/ACoAABdVV-gBgEkUGQaDXcOrRbQrcJK-wXPJlsw/
// https://www.linkedin.com/profile/view?id={id}&trk=hp-identity-name

namespace crowlr.console
{
    class Program
    {
        static void Main()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Write("Category:  ");
            var category = Console.ReadLine();
            Console.Write("Secondary: ");
            var secondary = Console.ReadLine()?.Split(',');
            Console.Write("Seniority: ");
            var seniority = Console.ReadLine();
            Console.Write("Keywords:  ");
            var keywords = Console.ReadLine();
            Console.Write("Except:    ");
            var except = Console.ReadLine()?.Split(',');
            Console.Write("Bottom:    ");
            var bottom = Convert.ToInt32(Console.ReadLine());
            Console.Write("Total:     ");
            var total = Convert.ToInt32(Console.ReadLine());
            Console.Write("Start:     ");
            var start = Convert.ToInt32(Console.ReadLine());

            Process(category, secondary, seniority, keywords, except, bottom, total, start);

            Console.ReadLine();
        }

        public static void Process(
            string category,
            string[] secondary,
            string seniority,
            string keywords,
            string[] except,
            int bottom,
            int total,
            int start)
        {
            var textProcessor = new TextProcessor(new CategoryProvider());
            using (var downloader = new PageDownloader())
            using (var linkedin = (ISiteProvider)new SiteProvider(downloader))
            {
                var homePage = linkedin.ExecutePage(Pages.PostLogin, new[,]
                {
                    { "session_key", "korolkova.julia.hr@gmail.com" },
                    { "session_password", "1qaz2wsx3edc" }
                });

                var csrfToken = homePage.GetNodeById("jet-csrfToken").Content;

                var processed = 0;
                var added = 0;
                while (added < total)
                {
                    var searchPage = linkedin.ExecutePage(Pages.GetSearchPage, new[,]
                    {
                        { "Csrf-Token", csrfToken },
                        { "category", category },
                        { "seniority", seniority },
                        { "keywords", keywords },
                        { "start", start.ToString() }
                    });

                    var searchResult = ((IPage<MiniProfile>)searchPage).Process().Values.First().ToList();

                    Console.WriteLine(Environment.NewLine + $@"[{start}] Profiles -> {searchResult.Count} of 20");

                    searchResult
                        .ForEach(x =>
                        {
                            Thread.Sleep(new Random().Next(1000, 5000));

                            var account = linkedin.ExecutePage(Pages.GetAccount, new[,] { { "id", x.Id } });

                            var result = textProcessor.Process(account, new[,]
                            {
                                { "secondary", string.Join(",", secondary) },
                                { "except", string.Join(",", except) },
                                { "category", category },
                                { "seniority", seniority },
                                { "bottom", bottom.ToString() }
                            });

                            var color = result.Status == OperationStatus.Skipped
                                ? result.Reason.Message == "except"
                                    ? ConsoleColor.DarkMagenta
                                    : ConsoleColor.DarkYellow
                                : result.Status == OperationStatus.Declined
                                    ? ConsoleColor.DarkRed
                                    : Console.ForegroundColor;

                            ConsoleEx.WriteLine($@"[{result.Status}] :: {result.Reason} {result.Reason.Data.Print()}", color);
                            ConsoleEx.WriteLine($@"{result.Data.Key("fullname")} -> {result.Data.Key("title")}", color);
                            ConsoleEx.WriteLine($@"[+] :: {result.Data.Key("positiveMatches")}", color);
                            ConsoleEx.WriteLine($@"[-] :: {result.Data.Key("negativeMatches")}", color);
                            ConsoleEx.WriteLine($@"[%] :: {result.Data.Key("percent")}", color);

                            if (result.Status != OperationStatus.Accepted)
                                return;

                            // invite user
                            var json = $@"{{""trackingId"":""{x.TrackingId}"",""invitations"":[],""invitee"":{{""com.linkedin.voyager.growth.invitation.InviteeProfile"":{{""profileId"":""{x.Id}""}}}}}}";
                            var inviteResult = downloader.Client.PostAsync(@"https://www.linkedin.com/voyager/api/growth/normInvitations", new StringContent(json, System.Text.Encoding.UTF8, "application/json")).Result;
                            Console.WriteLine($@"[{inviteResult.StatusCode} :: https://www.linkedin.com/profile/view?id={x.Id}&trk=hp-identity-name]{Environment.NewLine}");
                            Console.WriteLine();

                            added += 1;
                        });

                    processed += searchResult.Count;

                    Console.WriteLine($@"Processed -> {processed} of {total}");
                    Console.WriteLine($@"Added -> {added} of {total}");

                    start += 20;

                    if (!searchResult.Any())
                        break;
                }

                var logoutPage = downloader.Get($@"https://www.linkedin.com/uas/logout?session_full_logout=&csrfToken={csrfToken}&trk=nav_account_sub_nav_signout");
            }
        }
    }
}

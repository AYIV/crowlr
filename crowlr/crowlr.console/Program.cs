using crowlr.contracts;
using crowlr.core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

// https://wwww.linkedin.com/people/invites/scroll?pageNum=0&pagesize=50
// https://www.linkedin.com/m/profile/ACoAABdVV-gBgEkUGQaDXcOrRbQrcJK-wXPJlsw/
// https://www.linkedin.com/profile/view?id={id}&trk=hp-identity-name

namespace crowlr.console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var downloader = new PageDownloader())
            using (var linkedin = (ISiteProvider)new crowlr.linkedin.SiteProvider(downloader))
            {
                var homePage = linkedin.ExecutePage(crowlr.linkedin.Pages.PostLogin, new[,]
                {
                    { "session_key", "korolkova.julia.hr@gmail.com" },
                    { "session_password", "1qaz2wsx3edc" }
                });

                var csrfToken = homePage.GetNodeById("jet-csrfToken").Content;

                var orig = homePage.GetNodeByName("orig").Value;
                var rsid = homePage.GetNodeByName("rsid").Value;
                var pageKey = homePage.GetNodeByName("pageKey").Content;
                var trkInfo = homePage.GetNodeByName("trkInfo").Value;
                
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

                    searchPage.IsJson = true;

                    // remove bullshit like triangle braces etc.
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

                            var account = downloader.Get($@"https://www.linkedin.com/profile/view?id={x.id}&trk=hp-identity-name");

                            // location
                            var href = account.GetNodeByName("location")?.Href;
                            if (!string.IsNullOrWhiteSpace(href))
                            {
                                var country = new Uri("http://linkedin.com" + href).Parameters("countryCode");
                                if (country != "ua")
                                {
                                    ConsoleEx.WarningLine($@"[Skipped] {x.id} '{x.firstName + " " + x.lastName}' -> country [{country}]");
                                    return;
                                }
                            }

                            // no request
                            var addButton = account.GetNodeByXpath("//a[@data-action-name=\"add-to-network\"]")?.Href;
                            if (addButton == null)
                            {
                                ConsoleEx.WarningLine($@"[Skipped] {x.id} '{x.firstName + " " + x.lastName}' -> already added");
                                return;
                            }

                            // candidate industry
                            var industryNode = account.GetNodeByName("industry");
                            var industry = industryNode?.Href;
                            if (!string.IsNullOrWhiteSpace(industry))
                            {
                                int industryCode;
                                var parsed = Int32.TryParse(new Uri("http://linkedin.com" + industry).Parameters("f_I"), out industryCode);
                                if (parsed)
                                {
                                    if (industryCode == 104 || industryCode == 137)
                                    {
                                        ConsoleEx.WriteLine($@"[Recruiter] {x.id} '{x.firstName + " " + x.lastName}' -> industry ({industryNode?.Text})[{industryCode}]", ConsoleColor.DarkRed);
                                        return;
                                    }
                                }
                            }

                            // get top 10 skillz
                            var skillz = account.GetNodeListByXpath(@"//*[@id=""profile-skills""]/*[@class=""skills-section""]//*[@class=""endorse-item-name""]")
                                .Select(node => node?.Text)
                                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                                .Distinct();

                            var recruiterMarks = new[]
                            {
                                "hr", "human resources", "hiring", "recruit", "headhunt",
                                "resource manager", "recruiter", "рекрутер", "research"
                            };

                            {
                                // is it recruiter?
                                var hit = recruiterMarks
                                    .SelectMany(xx => skillz.Where(item => item.ToLower().Contains(xx)))
                                    .ToArray();

                                if (hit.Length > 2)
                                {
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.id} '{x.firstName + " " + x.lastName}' -> skillz [{string.Join(",", hit)}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            // current job title
                            var currentJob = account.GetNodeByXpath(@"//*[starts-with(@id,'experience-')]//h4/a")?.Text;
                            if (!string.IsNullOrWhiteSpace(currentJob))
                            {
                                var hit = recruiterMarks
                                    .SelectMany(xx => new[] { currentJob }.Where(item => item.ToLower().Contains(xx)))
                                    .ToArray();

                                if (hit.Any())
                                {
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.id} '{x.firstName + " " + x.lastName}' -> current job [{currentJob}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            // profile title
                            var title = account.GetNodeByClass("title")?.Text;
                            if (!string.IsNullOrWhiteSpace(title))
                            {
                                var hit = recruiterMarks
                                    .SelectMany(xx => new[] { title }.Where(item => item.ToLower().Contains(xx)))
                                    .ToArray();

                                if (hit.Any())
                                {
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.id} '{x.firstName + " " + x.lastName}' -> profile title [{title}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            // invite user
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
                
                var logoutPage = downloader.Get($@"https://www.linkedin.com/uas/logout?session_full_logout=&csrfToken={csrfToken}&trk=nav_account_sub_nav_signout");
            }

            Console.ReadLine();
        }
    }
}

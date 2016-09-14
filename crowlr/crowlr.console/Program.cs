﻿using crowlr.contracts;
using crowlr.core;
using crowlr.linkedin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

// https://www.linkedin.com/people/invites/scroll?pageNum=0&pagesize=50
// https://www.linkedin.com/m/profile/ACoAABdVV-gBgEkUGQaDXcOrRbQrcJK-wXPJlsw/
// https://www.linkedin.com/profile/view?id={id}&trk=hp-identity-name

namespace crowlr.console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            var seniorityCategories = new Dictionary<string, IEnumerable<string>>
            {
                { "senior", new[] { "senior", "sr." } }
            };

            var skillCategories = new Dictionary<string, IEnumerable<string>>
            {
                { "asp.net", new[] { ".net", "mvc", "javascript", "angular", "c#", "js" } },
                { "ruby", new[] { "java" } },
                { "python", new[] { "postgres", "sqlalchemy", "linux", "tornado", "pyramid", "erlang", "jenkins", "redis" } },
                { "java", new[] { "oop", "scala", "groovy", "clojure", "python", "idea" } }
            };

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

                Console.Write("Category:  ");
                var category = Console.ReadLine();
                Console.Write("Secondary: ");
                var secondary = Console.ReadLine().Split(',');
                Console.Write("Seniority: ");
                var seniority = Console.ReadLine();
                Console.Write("Keywords:  ");
                var keywords = Console.ReadLine();
                Console.Write("Except:    ");
                var except = Console.ReadLine().Split(',');
                Console.Write("Bottom:    ");
                var bottom = Convert.ToInt32(Console.ReadLine());
                Console.Write("Total:     ");
                var total = Convert.ToInt32(Console.ReadLine());
                Console.Write("Start:     ");
                var start = Convert.ToInt32(Console.ReadLine());

                var processed = 0;
                var added = 0;
                while (added < total)
                {
                    var searchPage = linkedin.ExecutePage(crowlr.linkedin.Pages.GetSearchPage, new[,]
                    {
                        { "Csrf-Token", csrfToken },
                        { "category", category },
                        { "seniority", seniority },
                        { "keywords", keywords },
                        { "start", start.ToString() }
                    });
                    
                    var searchResult = ((IPage<MiniProfile>) searchPage).Process().Values.First().ToList();

                    Console.WriteLine(Environment.NewLine + $@"[{start}] Profiles -> {searchResult.Count} of 20");
                    searchResult
                        .ForEach(x =>
                        {
                            Thread.Sleep(new Random().Next(1000, 5000));

                            var account = linkedin.ExecutePage(crowlr.linkedin.Pages.GetAccount, new[,] { { "id", x.Id } });

                            var nodes = ((IPage<string>) account).Process(new Dictionary<string, INodeMeta>
                            {
                                { "country", new NodeMeta(new NameSelector("location"), new HrefAttrParam("countryCode")) },
                                { "isAdded", new NodeMeta(new XpathSelector("//a[@data-action-name=\"add-to-network\"]"), new HrefAttr()) },
                                { "industry", new NodeMeta(new NameSelector("industry"), new HrefAttrParam("f_I")) },
                                { "currentJob", new NodeMeta(new XpathSelector(@"//*[starts-with(@id,'experience-')]//h4/a"), new TextAttr()) }
                            });

                            // location
                            var country = nodes.Key("country")?.FirstOrDefault();
                            if (!country.IsNull() && country != "ua")
                            {
                                ConsoleEx.WarningLine($@"[Skipped] {x.Id} '{x.FirstName + " " + x.LastName}' -> country [{country}]");
                                return;
                            }

                            // no request
                            if (nodes.Key("isAdded").IsNull())
                            {
                                ConsoleEx.WarningLine($@"[Skipped] {x.Id} '{x.FirstName + " " + x.LastName}' -> already added");
                                return;
                            }

                            // candidate industry
                            var industry = nodes.Key("industry")?.FirstOrDefault();
                            if (!industry.IsNull() && (industry == "104" || industry == "137"))
                            {
                                ConsoleEx.WriteLine($@"[Recruiter] {x.Id} '{x.FirstName + " " + x.LastName}' -> industry ({industry})", ConsoleColor.DarkRed);
                                return;
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
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.Id} '{x.FirstName + " " + x.LastName}' -> skillz [{string.Join(",", hit)}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            // current job title
                            var currentJob = nodes.Key("currentJob")?.FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(currentJob))
                            {
                                var hit = recruiterMarks
                                    .SelectMany(xx => new[] { currentJob }.Where(item => item.ToLower().Contains(xx)))
                                    .ToArray();

                                if (hit.Any())
                                {
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.Id} '{x.FirstName + " " + x.LastName}' -> current job [{currentJob}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            // profile title
                            var title = account.GetNodeByClass("title").Text;
                            if (!title.IsNull())
                            {
                                var hit = recruiterMarks
                                    .SelectMany(xx => new[] { title }.Where(item => item.ToLower().Contains(xx)))
                                    .ToArray();

                                if (hit.Any())
                                {
                                    ConsoleEx.WriteLine($@"[Recruiter] {x.Id} '{x.FirstName + " " + x.LastName}' -> profile title [{title}]", ConsoleColor.DarkRed);
                                    return;
                                }
                            }

                            var backgroundExperience = account.GetNodeById("background-experience");
                            var description = account.GetNodeByXpath(@"//*[@class=""summary""]/*[@class=""description""]");

                            var array = (secondary.Count() > 1 || !string.IsNullOrWhiteSpace(secondary.First())
                                ? secondary
                                : skillCategories[category])
                                .Concat(new[] { category })
                                .Concat(except)
                                .Select(Regex.Escape);

                            if (seniorityCategories.ContainsKey(seniority))
                            {
                                array = array.Concat(seniorityCategories[seniority].Select(i => Regex.Escape(i) + @"\s+"));
                            }

                            Func<Regex, string, ILookup<string, int>> matchLookup = (_regex, text) =>
                                _regex
                                    .Matches(text)
                                    .Cast<Match>()
                                    .GroupBy(i => i.Value.ToLower())
                                    .ToLookup(i => i.Key, i => i.Count());

                            var regexString = string.Format(@"(?=\b)({0})(?=\b)", string.Join("|", array));

                            var regex = new Regex(regexString, RegexOptions.IgnoreCase);

                            var titleMatches = matchLookup(regex, title);
                            var bgMatches = matchLookup(regex, backgroundExperience.Text ?? string.Empty);
                            var descriptionMatches = matchLookup(regex, description.Text ?? string.Empty);
                            var skillzMatches = matchLookup(regex, skillz.Aggregate("", (s, e) => s + " ," + e));

                            Func<string, ILookup<string, int>, string> showMatches = (group, matches) =>
                            {
                                var totalMatches = matches.Sum(match => match.First());
                                return matches.Aggregate($@"{group}:{totalMatches} -> ", (acc, el) => acc + $@"{el.Key.Trim()}:{el.First()}, ");
                            };

                            var totalPercentage = (titleMatches.Any() ? 25 : 0) +
                                                  (descriptionMatches.Any() ? 25 : 0) +
                                                  (bgMatches.Any() ? 25 : 0) +
                                                  (skillzMatches.Any() ? 25 : 0);
                            /*
                            var pageText = account.Text(
                                new ClassSelector("title"),
                                new IdSelector("background-experience"),
                                new XpathSelector(@"//*[@class=""summary""]/*[@class=""description""]")
                            );*/

                            var _totalMatches = titleMatches.Select(i => i.Key)
                                .Union(bgMatches.Select(i => i.Key))
                                .Union(descriptionMatches.Select(i => i.Key))
                                .Union(skillzMatches.Select(i => i.Key))
                                .Select(i => i.Trim().ToLower())
                                .Distinct();

                            var exceptFound = _totalMatches.Intersect(except);

                            var matched = (secondary.Count() > 1 || !string.IsNullOrWhiteSpace(secondary.First())
                                ? secondary
                                : skillCategories[category])
                                .Concat(new[] { category })
                                .Concat(seniorityCategories[seniority])
                                .Select(i => i.Trim().ToLower())
                                .ToList();

                            totalPercentage = matched.Intersect(_totalMatches).Count() * 100 / matched.Count();

                            var color = totalPercentage > bottom
                                ? Console.ForegroundColor
                                : ConsoleColor.DarkCyan;

                            ConsoleEx.WriteLine($@"{x.FirstName} {x.LastName} -> {title}", color);
                            ConsoleEx.WriteLine(showMatches("title", titleMatches), color);
                            ConsoleEx.WriteLine(showMatches("description", descriptionMatches), color);
                            ConsoleEx.WriteLine(showMatches("projects", bgMatches), color);
                            ConsoleEx.WriteLine(showMatches("skillz", skillzMatches), color);
                            ConsoleEx.WriteLine($@"total -> {totalPercentage}% [{string.Join(",", _totalMatches)}]", color);

                            if (exceptFound.Any())
                            {
                                ConsoleEx.WriteLine($@"[Except] {string.Join(", ", exceptFound)}", ConsoleColor.DarkMagenta);
                            }

                            if (totalPercentage <= bottom || exceptFound.Any())
                            {
                                ConsoleEx.WriteLine($@"[LowLevel :: https://www.linkedin.com/profile/view?id={x.Id}&trk=hp-identity-name]{Environment.NewLine}", color);
                                return;
                            }

                            // invite user
                            var json = string.Format(@"{{""trackingId"":""{0}"",""invitations"":[],""invitee"":{{""com.linkedin.voyager.growth.invitation.InviteeProfile"":{{""profileId"":""{1}""}}}}}}", x.TrackingId, x.Id);
                            var result = downloader.Client.PostAsync($@"https://www.linkedin.com/voyager/api/growth/normInvitations", new StringContent(json, System.Text.Encoding.UTF8, "application/json")).Result;
                            Console.WriteLine($@"[{result.StatusCode} :: https://www.linkedin.com/profile/view?id={x.Id}&trk=hp-identity-name]{Environment.NewLine}");

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

            Console.ReadLine();
        }
    }
}

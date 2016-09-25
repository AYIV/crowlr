using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using crowlr.contracts;
using crowlr.core;

namespace crowlr.linkedin
{
    public class TextProcessor : ITextProcessor
    {
        private readonly ICategoryProvider categoryProvider;

        public TextProcessor(ICategoryProvider categoryProvider)
        {
            this.categoryProvider = categoryProvider;
        }

        public IOperationResult Process(IPage page, string[,] @params)
        {
            return Process(page, @params.ToDictionary());
        }

        public IOperationResult Process(IPage page, IDictionary<string, string> @params)
        {
            var result = new OperationResult();

            var skillCategories = categoryProvider.Get("skill");
            var seniorityCategories = categoryProvider.Get("seniority");

            var nodes = ((IPage<string>)page).Process(new Dictionary<string, INodeMeta>
            {
                { "fullname", new NodeMeta(new ClassSelector("full-name"), new TextAttr())},
                { "country", new NodeMeta(new NameSelector("location"), new HrefAttrParam("countryCode")) },
                { "countryTitle", new NodeMeta(new NameSelector("location"), new TextAttr()) },
                { "isAdded", new NodeMeta(new XpathSelector("//a[@data-action-name=\"add-to-network\"]"), new HrefAttr()) },
                { "industry", new NodeMeta(new NameSelector("industry"), new HrefAttrParam("f_I")) },
                { "industryTitle", new NodeMeta(new NameSelector("industry"), new TextAttr()) },
                { "currentJob", new NodeMeta(new XpathSelector(@"//*[starts-with(@id,'experience-')]//h4/a"), new TextAttr()) },
                { "title", new NodeMeta(new ClassSelector("title"), new TextAttr()) },
                { "experince", new NodeMeta(new IdSelector("background-experience"), new TextAttr()) },
                { "description", new NodeMeta(new XpathSelector(@"//*[@class=""summary""]/*[@class=""description""]"), new TextAttr()) },
            });

            var isAdded = nodes.Key("isAdded");
            var country = nodes.Key("country")?.FirstOrDefault();
            var backgroundExperience = nodes.Key("experince")?.FirstOrDefault();
            var description = nodes.Key("description")?.FirstOrDefault();
            var skillz = page.GetNodeListByXpath(@"//*[@id=""profile-skills""]/*[@class=""skills-section""]//*[@class=""endorse-item-name""]").Select(node => node?.Text).Where(skill => !string.IsNullOrWhiteSpace(skill)).Distinct();
            var title = nodes.Key("title")?.FirstOrDefault();

            result.Data.Add("fullname", nodes.Key("fullname")?.FirstOrDefault());
            result.Data.Add("title", title);

            // location
            if (country.IsNull() || country != "ua")
            {
                return result.Skip("country", new[,] { { "country", nodes.Key("countryTitle")?.FirstOrDefault() } });
            }

            // no request
            if (isAdded.IsNull())
            {
                return result.Skip("already added");
            }

            var isRecruiter = IsNotRecruiter(page, nodes);
            if (isRecruiter.Status != OperationStatus.Accepted)
            {
                isRecruiter.Data.AddRange(result.Data);
                return isRecruiter;
            }

            var secondary = @params["secondary"].Split(',');
            var except = @params["except"].Split(',');
            var category = @params["category"];
            var array = (secondary.Length > 1 || !string.IsNullOrWhiteSpace(secondary.First())
                    ? secondary
                    : skillCategories.Key(category) ?? Enumerable.Empty<string>())
                .Concat(new[] {category})
                .Concat(except);

            if (seniorityCategories.ContainsKey(@params["seniority"]))
            {
                array = array.Concat(seniorityCategories[@params["seniority"]]);
            }

            array = array.Select(i => Regex.Escape(i) + @"\s+");
            
            var regexString = $@"({string.Join("|", array)})";

            var regex = new Regex(regexString, RegexOptions.IgnoreCase);

            Func<Regex, string, ILookup<string, int>> matchLookup = (_regex, text) =>
                _regex
                    .Matches(text)
                    .Cast<Match>()
                    .GroupBy(i => i.Value.ToLower())
                    .ToLookup(i => i.Key, i => i.Count());

            var titleMatches = matchLookup(regex, title);
            var bgMatches = matchLookup(regex, backgroundExperience ?? string.Empty);
            var descriptionMatches = matchLookup(regex, description ?? string.Empty);
            var skillzMatches = matchLookup(regex, skillz.Aggregate("", (s, e) => s + " ," + e));

            var _totalMatches = titleMatches.Select(i => i.Key)
                .Union(bgMatches.Select(i => i.Key))
                .Union(descriptionMatches.Select(i => i.Key))
                .Union(skillzMatches.Select(i => i.Key))
                .Select(i => i.Trim().ToLower())
                .Distinct();

            var matched = (secondary.Length > 1 || !string.IsNullOrWhiteSpace(secondary.First())
                ? secondary
                : skillCategories.Key(category) ?? Enumerable.Empty<string>())
                .Concat(new[] { category })
                .Concat(seniorityCategories[@params["seniority"]])
                .Select(i => i.Trim().ToLower())
                .ToList();

            var totalPercentage = matched.Intersect(_totalMatches).Count() * 100 / matched.Count;

            var exceptFound = _totalMatches.Intersect(except);
            if (exceptFound.Any())
            {
                return new DeclinedResult("except", new[,]
                {
                    { "except", string.Join(", ", exceptFound) }
                });
            }

            if (totalPercentage <= Convert.ToInt32(@params["bottom"]))
            {
                return new DeclinedResult("low level", new[,]
                {
                    { "percent", totalPercentage.ToString() }
                });
            }

            return result.Accept(new[,]
            {
                {"percent", totalPercentage.ToString()},
                {"positiveMatches", string.Join(",", _totalMatches.Except(except))},
                {"negativeMatches", string.Join(",", exceptFound)}
            });
        }

        private IOperationResult IsNotRecruiter(IPage page, IDictionary<string, IEnumerable<string>> nodes)
        {
            var recruiterMarks = categoryProvider.Get("hr")["hr"];

            var industry = nodes.Key("industry")?.FirstOrDefault();
            var currentJob = nodes.Key("currentJob")?.FirstOrDefault();

            var skillz = page.GetNodeListByXpath(@"//*[@id=""profile-skills""]/*[@class=""skills-section""]//*[@class=""endorse-item-name""]").Select(node => node?.Text).Where(skill => !string.IsNullOrWhiteSpace(skill)).Distinct();
            var title = nodes.Key("title")?.FirstOrDefault();

            if (!industry.IsNull() && (industry == "104" || industry == "137"))
            {
                return new DeclinedResult("recruiter", new[,] { { "industry", industry }, { "industry", nodes.Key("industryTitle")?.FirstOrDefault() } });
            }

            {
                // is it recruiter?
                var hit = recruiterMarks
                    .SelectMany(xx => skillz.Where(item => item.ToLower().Contains(xx)))
                    .ToArray();

                if (hit.Length > 2)
                {
                    return new DeclinedResult("recruiter", new[,] { { "skillz", string.Join(",", hit) } });
                }
            }

            // current job title
            if (!string.IsNullOrWhiteSpace(currentJob))
            {
                var hit = recruiterMarks
                    .SelectMany(xx => new[] { currentJob }.Where(item => item.ToLower().Contains(xx)))
                    .ToArray();

                if (hit.Any())
                {
                    return new DeclinedResult("recruiter", new[,] { { "current job", currentJob } });
                }
            }

            // profile title
            if (!title.IsNull())
            {
                var hit = recruiterMarks
                    .SelectMany(xx => new[] { title }.Where(item => item.ToLower().Contains(xx)))
                    .ToArray();

                if (hit.Any())
                {
                    return new DeclinedResult("recruiter", new[,] { { "profile title", title } });
                }
            }

            return new AcceptedResult();
        }
    }
}

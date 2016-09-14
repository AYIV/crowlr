using crowlr.contracts;
using crowlr.core;
using System.Collections.Generic;
using System.Linq;

namespace crowlr.linkedin
{
    public class MiniProfile
    {
        public MiniProfile(string firstName, string lastName, string id, string trackingId)
        {
            FirstName = firstName;
            LastName = lastName;
            Id = id;
            TrackingId = trackingId;
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TrackingId { get; set; }
    }

    public class SearchPage : Page<MiniProfile>
    {
        public SearchPage(IPage page)
            : this(page.Html, page.IsJson)
        {

        }

        public SearchPage(string html, bool isJson)
            : base(html, isJson)
        {
        }

        public override IDictionary<string, IEnumerable<MiniProfile>> Process(IDictionary<string, INodeMeta> dictionary = null)
        {
            return new Dictionary<string, IEnumerable<MiniProfile>>
            {
                {
                    "main",
                    (this.Json.elements as IEnumerable<dynamic>)
                        .Where(x => x.hitInfo["com.linkedin.voyager.search.SearchProfile"] != null)
                        .Select(x => x.hitInfo["com.linkedin.voyager.search.SearchProfile"])
                        .Where(x => x.distance.value == "DISTANCE_2" || x.distance.value == "DISTANCE_3")
                        .Select(x =>
                        {
                            var id = (x["miniProfile"]["entityUrn"].ToString().Split(':') as string[]).Last();
                            return new MiniProfile(x.miniProfile.firstName.ToString(), x.miniProfile.lastName.ToString(), id.ToString(), x.miniProfile.trackingId.ToString());
                        })
                        .ToList()
                }
            };
        }
    }

    public class AccountPage : Page<string>
    {
        public AccountPage(IPage page)
            : base(page.Html, page.IsJson)
        {
        }

        public override IDictionary<string, IEnumerable<string>> Process(IDictionary<string, INodeMeta> dictionary)
        {
            var result = new Dictionary<string, IEnumerable<string>>();

            foreach (var pair in dictionary)
            {
                var attribute = base
                    .GetNodeByXpath(pair.Value.Selector.ToString())
                    .Attribute(pair.Value.Result?.AttributeName);

                if (attribute.IsNull())
                    continue;

                result.Add(
                    pair.Key,
                    new[] { pair.Value.Result.Parse(attribute) }
                );
            }

            return result;
        }
    }
}

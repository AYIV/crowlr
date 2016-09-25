using crowlr.contracts;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace crowlr.core
{
    public class Page : IPage
    {
        private HtmlDocument _htmlDocument;
        protected HtmlDocument HtmlDocument => _htmlDocument ?? (_htmlDocument = Page.FromHtml(Html));

        public string Html { get; }
        public bool IsJson { get; }

        public dynamic Json => IsJson
            ? JsonConvert.DeserializeObject<dynamic>(Html)
            : null;

        public Page(string html)
            : this(html, isJson: false)
        {
        }

        public Page(string html, bool isJson)
        {
            Html = html;
            IsJson = isJson;
        }

        public override string ToString()
        {
            return IsJson
                ? Html
                : HtmlDocument.DocumentNode.OuterHtml;
        }

        public INode GetNodeById(string id)
        {
            return new Node(
                HtmlDocument.GetElementbyId(id)
            );
        }

        public INode GetNodeByName(string name)
        {
            return GetNodeByXpath($@"//*[@name=""{name}""]");
        }

        public INode GetNodeByClass(string @class)
        {
            return GetNodeByXpath($@"//*[@class=""{@class}""]");
        }

        public INode GetNodeByXpath(string xpath)
        {
            //HINT:: xpath example
            //       "//input[@id = \"loginCsrfParam-login\"]"

            return new Node(
                HtmlDocument.DocumentNode.SelectSingleNode(xpath)
            );
        }

        public IEnumerable<INode> GetNodeListByXpath(string xpath)
        {
            var nodes = HtmlDocument.DocumentNode.SelectNodes(xpath);

            return nodes?.Select(x => new Node(x)).ToArray() ?? Enumerable.Empty<INode>();
        }

        private static HtmlDocument FromHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }
    }

    public class Page<T> : Page, IPage<T>
    {
        public Page(string html) : base(html)
        {
        }

        public Page(string html, bool isJson) : base(html, isJson)
        {
        }

        public virtual IDictionary<string, IEnumerable<T>> Process(IDictionary<string, INodeMeta> dictionary)
        {
            return DictionaryExtesions.Empty<string, IEnumerable<T>>();
        }
    }
}

using crowlr.contracts;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace crowlr.core
{
    public class Page : IPage
    {
        private string _html;
        private HtmlDocument _htmlDocument;
        
        private HtmlDocument HtmlDocument
        {
            get
            {
                return _htmlDocument ?? (_htmlDocument = Page.FromHtml(_html));
            }
        }

        public bool IsJson { get; set; }

        public Page(string html, bool isJson)
            : this(html)
        {
            IsJson = isJson;
        }

        public Page(string html)
        {
            _html = html;
        }

        public Page(HtmlDocument document)
        {
            _htmlDocument = document;
        }
        
        public static IPage Ctor(string html)
        {
            return new Page(
                Page.FromHtml(html)
            );
        }

        public override string ToString()
        {
            return IsJson
                ? _html
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

            return nodes != null
                ? nodes.Select(x => new Node(x)).ToArray()
                : Enumerable.Empty<INode>();
        }

        private static HtmlDocument FromHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }
    }
}

using crowlr.contracts;
using HtmlAgilityPack;

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
            return HtmlDocument.DocumentNode.OuterHtml;
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

        private static HtmlDocument FromHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
        }
    }
}

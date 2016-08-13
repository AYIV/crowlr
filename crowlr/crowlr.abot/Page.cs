using crowlr.contracts;
using HtmlAgilityPack;

namespace crowlr.abot
{
    public class Page : IPage
    {
        private HtmlDocument htmlDocument;
        
        public Page(HtmlDocument document)
        {
            this.htmlDocument = document;
        }
        
        public override string ToString()
        {
            return htmlDocument.DocumentNode.OuterHtml;
        }

        public INode GetNodeById(string id)
        {
            return new Node(
                htmlDocument.GetElementbyId(id)
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
                htmlDocument.DocumentNode.SelectSingleNode(xpath)
            );
        }
    }
}

using HtmlAgilityPack;

namespace crowlr.contracts
{
    public class Page : IPage
    {
        private HtmlDocument _document;

        public Page(HtmlDocument document)
        {
            _document = document;
        }

        public override string ToString()
        {
            return _document.DocumentNode.OuterHtml;
        }

        public INode GetNodeById(string id)
        {
            return new Node(
                _document.GetElementbyId(id)
            );
        }

        public INode GetNodeByXpath(string xpath)
        {
            //HINT:: xpath example
            //       "//input[@id = \"loginCsrfParam-login\"]"

            return new Node(
                _document.DocumentNode.SelectSingleNode(xpath)
            );
        }
    }
}

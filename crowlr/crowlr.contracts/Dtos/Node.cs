using HtmlAgilityPack;

namespace crowlr.contracts
{
    public class Node : INode
    {
        private HtmlNode _node;

        public Node(HtmlNode node)
        {
            _node = node;
        }

        public string Value
        {
            get
            {
                return _node.GetAttributeValue("value", null);
            }
        }
    }
}

﻿using crowlr.contracts;
using HtmlAgilityPack;

namespace crowlr.core
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
                return _node?.GetAttributeValue("value", null);
            }
        }

        public string Href
        {
            get
            {
                return _node?.GetAttributeValue("href", null);
            }
        }

        public string Content
        {
            get
            {
                return _node?.GetAttributeValue("content", null);
            }
        }

        public string Text
        {
            get
            {
                return _node?.InnerText;
            }
        }

        public override string ToString()
        {
            return _node.OuterHtml;
        }

        public string Attribute(string attributeName)
        {
            switch(attributeName)
            {
                case "text":
                    return this.Text;

                case null:
                    return null;

                default:
                    return _node?.GetAttributeValue(attributeName, null);
            }
        }
    }
}

﻿using System;
using crowlr.contracts;
using HtmlAgilityPack;

namespace crowlr.abot
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
                throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return _node.OuterHtml;
        }

        public string Attribute(string attributeName)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using crowlr.contracts;
using HtmlAgilityPack;

namespace crowlr.abot
{
    public class Page : IPage
    {
        private HtmlDocument htmlDocument;

        public bool IsJson
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Html
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public dynamic Json
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public IEnumerable<INode> GetNodeListByXpath(string xpath)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IEnumerable<T>> Process<T>(IDictionary<string, INodeMeta> dictionary = null)
        {
            throw new NotImplementedException();
        }
    }
}

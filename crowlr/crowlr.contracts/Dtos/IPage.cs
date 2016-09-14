using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IPage
    {
        string Html { get; }

        bool IsJson { get; }

        dynamic Json { get; }

        INode GetNodeById(string id);

        INode GetNodeByName(string name);

        INode GetNodeByClass(string @class);

        INode GetNodeByXpath(string xpath);

        IEnumerable<INode> GetNodeListByXpath(string xpath);
    }

    public interface IPage<T> : IPage
    {
        IDictionary<string, IEnumerable<T>> Process(IDictionary<string, INodeMeta> dictionary = null);

        //IDictionary<string, INodeMeta> Criteria();
    }
}

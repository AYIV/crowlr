using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IPage
    {
        bool IsJson { get; set; }

        INode GetNodeById(string id);

        INode GetNodeByName(string name);

        INode GetNodeByClass(string @class);

        INode GetNodeByXpath(string xpath);

        IEnumerable<INode> GetNodeListByXpath(string xpath);
    }
}

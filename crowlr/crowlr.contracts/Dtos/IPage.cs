using System.Net;

namespace crowlr.contracts
{
    public interface IPage
    {
        INode GetNodeById(string id);

        INode GetNodeByName(string name);

        INode GetNodeByClass(string @class);

        INode GetNodeByXpath(string xpath);
    }
}

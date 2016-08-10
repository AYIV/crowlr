namespace crowlr.contracts
{
    public interface IPage
    {
        INode GetNodeById(string id);

        INode GetNodeByXpath(string xpath);
    }
}

namespace crowlr.contracts
{
    public interface IAttributeMeta
    {
        string AttributeName { get; }

        string Parse(string attribute);
    }
}

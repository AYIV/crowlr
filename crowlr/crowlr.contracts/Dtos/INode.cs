namespace crowlr.contracts
{
    public interface INode
    {
        string Href { get; }
        string Value { get; }
        string Content { get; }
        string Text { get; }

        string Attribute(string attributeName);
    }
}

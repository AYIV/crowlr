namespace crowlr.contracts
{
    public interface INodeMeta
    {
        ISelector Selector { get; set; }

        IAttributeMeta Result { get; set; }
    }
}

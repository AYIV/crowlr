using crowlr.contracts;

namespace crowlr.core
{
    public class NodeMeta : INodeMeta
    {
        public ISelector Selector { get; set; }

        public IAttributeMeta Result { get; set; }

        public NodeMeta(ISelector selector, IAttributeMeta result)
        {
            Selector = selector;
            Result = result;
        }
    }
}

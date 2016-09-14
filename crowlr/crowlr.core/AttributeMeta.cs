using crowlr.contracts;

namespace crowlr.core
{
    public abstract class AttributeMeta : IAttributeMeta
    {
        public string AttributeName { get; private set; }

        public AttributeMeta(string name)
        {
            AttributeName = name;
        }

        public abstract string Parse(string attribute);
    }
}

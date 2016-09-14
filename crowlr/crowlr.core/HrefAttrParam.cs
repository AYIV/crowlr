namespace crowlr.core
{
    public class HrefAttrParam : AttributeMeta
    {
        public string Name { get; private set; }

        public HrefAttrParam(string name) : base("href")
        {
            Name = name;
        }

        public override string Parse(string attribute)
        {
            return attribute.HrefParam(Name, "http://linkedin.com");
        }
    }
}

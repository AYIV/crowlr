namespace crowlr.core
{
    public class HrefAttr : AttributeMeta
    {
        public HrefAttr() : base("href")
        {
        }

        public override string Parse(string attribute)
        {
            return attribute;
        }
    }
}

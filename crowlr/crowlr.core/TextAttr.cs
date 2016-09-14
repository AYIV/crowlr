namespace crowlr.core
{
    public class TextAttr : AttributeMeta
    {
        public TextAttr() : base("text")
        {
        }

        public override string Parse(string attribute)
        {
            return attribute;
        }
    }
}

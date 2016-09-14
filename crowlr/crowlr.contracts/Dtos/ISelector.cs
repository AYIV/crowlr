namespace crowlr.contracts
{
    public interface ISelector
    {
    }

    public class SelectorBase : ISelector
    {
        protected string Classificator { get; private set; }

        public SelectorBase(string classificator)
        {
            this.Classificator = classificator;
        }
    }

    public class IdSelector : SelectorBase
    {
        public IdSelector(string elementId)
            : base(elementId)
        {
        }

        public override string ToString()
        {
            return $@"//*[@id=""{base.Classificator}""]";
        }
    }

    public class ClassSelector : SelectorBase
    {
        public ClassSelector(string @class)
            : base(@class)
        {
        }

        public override string ToString()
        {
            return $@"//*[@class=""{base.Classificator}""]";
        }
    }

    public class NameSelector : SelectorBase
    {
        public NameSelector(string name)
            : base(name)
        {
        }

        public override string ToString()
        {
            return $@"//*[@name=""{base.Classificator}""]";
        }
    }

    public class XpathSelector : SelectorBase
    {
        public XpathSelector(string elementId)
            : base(elementId)
        {
        }

        public override string ToString()
        {
            return base.Classificator;
        }
    }
}

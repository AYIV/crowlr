using crowlr.contracts;
using System.Linq;

namespace crowlr.core.Extensions
{
    public static class PageExtensions
    {
        public static string Text(this IPage page, params ISelector[] selectors)
        {
            return selectors.Aggregate("", (acc, e) => acc + page.GetNodeByXpath(e.ToString()).Text);
        }
    }

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
        public ClassSelector(string elementId)
            : base(elementId)
        {
        }

        public override string ToString()
        {
            return $@"//*[@class=""{base.Classificator}""]";
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

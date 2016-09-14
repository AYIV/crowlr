using crowlr.contracts;
using System.Linq;

namespace crowlr.core
{
    public static class PageExtensions
    {
        public static string Text(this IPage page, params ISelector[] selectors)
        {
            return selectors.Aggregate("", (acc, e) => acc + page.GetNodeByXpath(e.ToString()).Text);
        }
    }
}

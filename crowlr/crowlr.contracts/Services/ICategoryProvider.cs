using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface ICategoryProvider
    {
        IDictionary<string, IEnumerable<string>> Get(string type);
    }
}

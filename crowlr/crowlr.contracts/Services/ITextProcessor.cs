using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface ITextProcessor
    {
        IOperationResult Process(IPage page, string[,] @params);
        IOperationResult Process(IPage page, IDictionary<string, string> @params);
    }
}

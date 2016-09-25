using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IReasonResult
    {
        string Message { get; }

        IDictionary<string, string> Data { get; }
    }
}

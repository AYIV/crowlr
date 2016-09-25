using System.Collections.Generic;

namespace crowlr.contracts
{
    public interface IOperationResult
    {
        OperationStatus Status { get; }
        IReasonResult Reason { get; }

        IDictionary<string, string> Data { get; }
    }
}

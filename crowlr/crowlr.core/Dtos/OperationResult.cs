using crowlr.contracts;
using System.Collections.Generic;

namespace crowlr.core
{
    public class OperationResult : IOperationResult
    {
        public IDictionary<string, string> Data { get; set; }

        public IReasonResult Reason { get; set; }

        public OperationStatus Status { get; set; }
        
        public OperationResult(OperationStatus status, string reason, IDictionary<string, string> data)
        {
            Status = status;
            Reason = new ReasonResult(reason, data);
        }

        public OperationResult()
        {
            Data = new Dictionary<string, string>();
        }
    }
}

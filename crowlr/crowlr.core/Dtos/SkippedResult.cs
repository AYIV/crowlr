using System.Collections.Generic;
using crowlr.contracts;

namespace crowlr.core
{
    public class SkippedResult : OperationResult
    {
        public SkippedResult(string reason)
            : this(reason, default(string[,]))
        {
        }

        public SkippedResult(string reason, string[,] data)
            : this(reason, data?.ToDictionary())
        {
        }

        public SkippedResult(string reason, IDictionary<string, string> data)
            : base(OperationStatus.Skipped, reason, data)
        {
            
        }
    }
}
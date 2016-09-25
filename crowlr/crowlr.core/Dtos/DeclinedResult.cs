using crowlr.contracts;

namespace crowlr.core
{
    public class DeclinedResult : OperationResult
    {
        public DeclinedResult(string reason)
            : this(reason, null)
        {
        }

        public DeclinedResult(string reason, string[,] data)
            : base(OperationStatus.Declined, reason, data?.ToDictionary())
        {
        }
    }
}

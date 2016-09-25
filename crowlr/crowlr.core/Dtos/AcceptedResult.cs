using crowlr.contracts;

namespace crowlr.core
{
    public class AcceptedResult : OperationResult
    {
        public AcceptedResult()
            : this(null)
        {
        }

        public AcceptedResult(string[,] data)
            : base(OperationStatus.Accepted, null, data?.ToDictionary())
        {
        }
    }
}

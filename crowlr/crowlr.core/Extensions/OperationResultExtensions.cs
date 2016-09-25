using crowlr.contracts;

namespace crowlr.core
{
    public static class OperationResultExtensions
    {
        public static IOperationResult Skip(this IOperationResult @this, string reason, string[,] data)
        {
            return new SkippedResult(reason, data.ToDictionary())
            {
                Data = @this.Data
            };
        }

        public static IOperationResult Skip(this IOperationResult @this, string reason)
        {
            return new SkippedResult(reason)
            {
                Data = @this.Data
            };
        }

        public static IOperationResult Accept(this IOperationResult @this, string[,] data)
        {
            var result = new AcceptedResult
            {
                Data = @this.Data
            };

            result.Data.AddRange(data.ToDictionary());

            return result;
        }
    }
}

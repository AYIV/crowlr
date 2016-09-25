using System.Collections.Generic;
using crowlr.contracts;

namespace crowlr.core
{
    public class ReasonResult : IReasonResult
    {
        public string Message { get; }
        public IDictionary<string, string> Data { get; }

        public ReasonResult(string mesage, IDictionary<string, string> data)
        {
            Message = mesage;
            Data = data;
        }

        public override string ToString()
        {
            return this.Message;
        }
    }
}
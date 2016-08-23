using System;

namespace crowlr.contracts
{
    public interface ISiteProvider : IDisposable
    {
        string GetSessionParams(string param);

        IPage ExecutePage<T>(string pageName, T @params);
    }
}

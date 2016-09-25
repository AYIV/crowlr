using crowlr.contracts;
using Microsoft.AspNet.SignalR;
using System;

namespace crowlr.signalr
{
    public class SignalrNotifier : INotifier
    {
        public string HubName { get; private set; }

        public SignalrNotifier(Type hubType)
        {
            HubName = hubType.Name;
        }

        public bool Notify<T>(T @params)
        {
            try
            {
                GlobalHost
                    .ConnectionManager
                    .GetHubContext(HubName)
                    .Clients
                    .All
                    .notify(@params);

                return true;
            }
            catch (Exception ex)
            {
                //TODO:: log this.
                return false;
            }
        }
    }
}

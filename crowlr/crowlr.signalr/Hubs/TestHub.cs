using crowlr.contracts;
using Microsoft.AspNet.SignalR;

namespace crowlr.signalr
{
    public class TestHub : Hub
    {
        private static INotifier notifier = new SignalrNotifier(typeof(TestHub));

        public static bool Notify<T>(T @params)
        {
            return notifier.Notify(@params);
        }
    }
}

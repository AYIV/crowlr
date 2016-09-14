using Microsoft.AspNet.SignalR;

namespace crowlr.web.Hubs
{
    public class TestHub : Hub
    {
        public static void Test()
        {
            GlobalHost
                .ConnectionManager
                .GetHubContext<TestHub>()
                .Clients
                .All
                .test();
        }
    }
}

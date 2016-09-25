using crowlr.contracts;
using crowlr.core;
using crowlr.core.Services;
using crowlr.signalr;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace crowlr.web.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private INotifier Notifier { get; set; }

        private Random rnd = new Random();

        public HomeController()
        {
            Notifier = new SignalrNotifier(typeof(TestHub));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("test")]
        public string Test()
        {
            return "Hello World! With generic notify";
        }

        [HttpGet("text")]
        public async Task<int> Text()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };
            
            arr.Notify(Notifier, (item, index) => {
                return new { guid = Guid.NewGuid(), index };
            });

            return arr.Length;
        }

        // ==================================================================

        Dictionary<string, IEnumerable<string>> seniorityCategories = new Dictionary<string, IEnumerable<string>>
            {
                { "senior", new[] { "senior", "sr." } }
            };

        Dictionary<string, IEnumerable<string>> skillCategories = new Dictionary<string, IEnumerable<string>>
            {
                { "asp.net", new[] { ".net", "mvc", "javascript", "angular", "c#", "js" } },
                { "ruby", new[] { "java" } },
                { "python", new[] { "postgres", "sqlalchemy", "linux", "tornado", "pyramid", "erlang", "jenkins", "redis" } },
                { "java", new[] { "oop", "scala", "groovy", "clojure", "python", "idea" } }
            };
    }

    public static class ArrExt
    {
        public async static void Notify<T1, T2>(this IEnumerable<T1> collection, INotifier notifier, Func<T1, int, Task<T2>> func)
        {
            await NotifyInternal(collection, notifier, func);
        }

        public async static void Notify<T1, T2>(this IEnumerable<T1> collection, INotifier notifier, Func<T1, int, T2> func)
        {
            await NotifyInternal(collection, notifier, async (item, index) => await Task.Run(() => func(item, index)));
        }

        private async static Task NotifyInternal<T1, T2>(this IEnumerable<T1> collection, INotifier notifier, Func<T1, int, Task<T2>> func)
        {
            var array = collection.ToArray();
            var count = collection.Count();

            for (var i = 0; i < count; i++)
            {
                var item = await func(array[i], i);
                if (item.IsNull())
                    continue;

                notifier.Notify(new
                {
                    item,
                    isLast = i >= count - 1
                });
            }
        }
    }
}

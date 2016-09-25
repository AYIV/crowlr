using System;
using System.Collections.Generic;
using crowlr.contracts;

namespace crowlr.core
{
    public class CategoryProvider : ICategoryProvider
    {
        public IDictionary<string, IEnumerable<string>> Get(string type)
        {
            switch (type)
            {
                case "skill":
                    return new Dictionary<string, IEnumerable<string>>
                    {
                        { "asp.net", new[] { ".net", "mvc", "javascript", "angular", "c#", "js" } },
                        { "ruby", new[] { "java" } },
                        { "python", new[] { "postgres", "sqlalchemy", "linux", "tornado", "pyramid", "erlang", "jenkins", "redis" } },
                        { "java", new[] { "oop", "scala", "groovy", "clojure", "python", "idea" } }
                    };

                case "seniority":
                    return new Dictionary<string, IEnumerable<string>>
                    {
                        { "senior", new[] { "senior", "sr." } }
                    };

                case "hr":
                    return new Dictionary<string, IEnumerable<string>>
                    {
                        { "hr", new[] { "hr", "human resources", "hiring", "recruit", "headhunt", "resource manager", "recruiter", "рекрутер", "research" } }
                    };

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crowlr.core
{
    public static class ObjectExtenions
    {
        public static bool IsNull<T>(this T obj)
        {
            return obj == null;
        }
    }
}

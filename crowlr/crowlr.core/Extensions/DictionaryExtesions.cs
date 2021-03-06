﻿using System.Collections.Generic;
using System.Linq;

namespace crowlr.core
{
    public static class DictionaryExtesions
    {
        public static void AddRange<T1, T2>(this IDictionary<T1, T2> @this, IDictionary<T1, T2> merging, bool overwrite = false)
        {
            foreach (var item in merging)
            {
                if (!@this.ContainsKey(item.Key))
                {
                    @this.Add(item.Key, item.Value);
                }

                if (overwrite)
                {
                    @this[item.Key] = item.Value;
                }
            }
        }

        public static TValue Key<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            return @this.ContainsKey(key)
                ? @this[key]
                : default(TValue);
        }

        public static IDictionary<TKey, TValue> Empty<TKey, TValue>()
        {
            return new Dictionary<TKey, TValue>();
        }

        public static string Print<T1, T2>(this IDictionary<T1, T2> @this)
        {
            if (@this.IsNull())
                return string.Empty;

            return $@"[{string.Join(",", @this.Select(e => e.Key + "->" + e.Value))}]";
        }
    }
}

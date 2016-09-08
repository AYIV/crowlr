using System.Collections.Generic;

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
    }
}

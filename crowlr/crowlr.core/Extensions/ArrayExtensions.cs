using System.Collections.Generic;

namespace crowlr.core
{
    public static class ArrayExtensions
    {
        public static IDictionary<T, T> ToDictionary<T>(this T[,] @this)
        {
            var dictionary = new Dictionary<T, T>();

            for (int i = 0; i < @this.GetLength(0); i++)
            {
                if (dictionary.ContainsKey(@this[i, 0]))
                    continue;

                dictionary.Add(@this[i, 0], @this[i, 1]);
            }

            return dictionary;
        }
    }
}

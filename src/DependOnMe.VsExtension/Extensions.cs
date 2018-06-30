using System;
using System.Collections.Generic;

namespace DependOnMe.VsExtension
{
    internal static class Extensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> addFunc)
        {
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }

            var newValue = addFunc(key);
            dict.Add(key, newValue);

            return newValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryDataList
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merges two dictionaries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="original"></param>
        /// <param name="additional"></param>
        /// <remarks>
        /// Values in original dictionary will be replaced with values from additional dictionary with same keys.
        /// </remarks>
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> original, IDictionary<TKey, TValue> additional)
        {
            if (original == null) throw new ArgumentNullException("original");
            if (additional == null) throw new ArgumentNullException("additional");

            foreach (var pair in additional)
                original[pair.Key] = pair.Value;
        }

        public static IDictionary<string, object> ToDictionaty(this object obj)
        {
            var result = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties())
                result.Add(prop.Name, prop.GetValue(obj, null));
            return result;
        }

        public static IDictionary<string, TValue> ToDictionaty<TValue>(this object obj)
        {
            var result = new Dictionary<string, TValue>();
            foreach (var prop in obj.GetType().GetProperties())
                result.Add(prop.Name, (TValue)prop.GetValue(obj, null));
            return result;
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            else
                return default(TValue);
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null) throw new ArgumentNullException("dictionary");

            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            else
                return defaultValue;
        }
    }
}
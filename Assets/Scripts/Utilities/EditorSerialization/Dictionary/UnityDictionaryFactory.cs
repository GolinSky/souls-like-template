using System.Collections.Generic;

namespace MultiPlayerTemplate
{
    public static class UnityDictionaryFactory
    {
        public static Dictionary<TKey, TValue> Build<TKey,TValue>(IEnumerable<IKeyValue<TKey, TValue>> enumerable)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            foreach (var keyValue in enumerable)
            {
                if(dictionary.ContainsKey(keyValue.Key)) continue;
                dictionary.Add(keyValue.Key, keyValue.Value);
            }

            return dictionary;
        }
    }
}
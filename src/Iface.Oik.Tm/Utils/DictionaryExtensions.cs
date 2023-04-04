using System;
using System.Collections.Generic;
using System.Linq;

namespace Iface.Oik.Tm.Utils
{
	public static class DictionaryExtensions
	{
		// важно - работает только для простых типов Value, для которых реализован EqualityComparer
		public static bool DictionaryEquals<TKey, TValue>(this IDictionary<TKey, TValue> dict1,
														  IDictionary<TKey, TValue> dict2)
		{
			if (dict1 == dict2)
			{
				return true;
			}
			if (dict1 == null || dict2 == null)
			{
				return false;
			}
			if (dict1.Count != dict2.Count)
			{
				return false;
			}
			return dict1.All(kvp => dict2.TryGetValue(kvp.Key, out var value2) &&
									(kvp.Value == null ? value2 == null : kvp.Value.Equals(value2)));
		}

		public static void AddWithUniquePostfixIfNeeded<TValue>(this IDictionary<string, TValue> dict,
																string key,
																TValue value)
		{
			if (!dict.ContainsKey(key))
			{
				dict.Add(key, value);
				return;
			}

			var postfix = 0;
			string uniqueKey;
			do
			{
				postfix++;
				uniqueKey = $"{key}_{postfix}";
			} while (dict.ContainsKey(uniqueKey));

			dict.Add(uniqueKey, value);
		}
		public static TValue ValueOrDefault<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary,
			TKey key,
			TValue defaultValue)
		{
			return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
		}

		public static TValue ValueOrDefault<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary,
			TKey key,
			Func<TValue> defaultValueProvider)
		{
			return dictionary.TryGetValue(key, out var value) ? value : defaultValueProvider();
		}
		public static TValue GetValueOrFirst<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary,
			TKey key)
		{
			return dictionary.TryGetValue(key, out var value) ? value : dictionary.Values.First();
		}
	}
}
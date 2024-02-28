namespace Mvvm.Nucleus.Maui;

public static class DictionaryExtensions
{
    public static T? GetValueOrDefault<T>(this IDictionary<string, object> dictionary, string key, T? fallback = default) where T : class
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        return dictionary[key] as T ?? fallback;
    }

    public static T GetStructOrDefault<T>(this IDictionary<string, object> dictionary, string key, T fallback = default) where T : struct
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        return dictionary[key] is T ? (T)dictionary[key]! : fallback;
    }
}
namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="DictionaryExtensions"/> contains extensions for interacting with the navigation parameters.
/// It can be used for any <see cref="IDictionary{TKey, TValue}"/> that holds strings and objects.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets a value from the dictionary, or the fallback if it does not exist or is of a unexpected <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The expected <see cref="Type"/> of the value.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The value or its fallback value.</returns>
    public static T? GetValueOrDefault<T>(this IDictionary<string, object> dictionary, string key, T? fallback = default) where T : class
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        return dictionary[key] as T ?? fallback;
    }

    /// <summary>
    /// Gets a value from the dictionary, or the fallback if it does not exist or is of a unexpected <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The expected <see cref="Type"/> of the value.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The value or its fallback value.</returns>
    public static T GetStructOrDefault<T>(this IDictionary<string, object> dictionary, string key, T fallback = default) where T : struct
    {
        if (!dictionary.ContainsKey(key))
        {
            return fallback;
        }

        return dictionary[key] is T ? (T)dictionary[key]! : fallback;
    }
}
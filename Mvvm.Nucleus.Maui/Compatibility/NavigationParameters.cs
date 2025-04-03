namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="NavigationParameters"/> is a compatibility class for Prism's NavigationParameters.
/// It differs from <see cref="IDictionary{TKey, TValue}"/> in that accessing a key that is not set will return
/// a default value, rather than throw an <see cref="Exception"/>.
/// </summary>
public class NavigationParameters : Dictionary<string, object>, INavigationParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationParameters"/> class.
    /// </summary>
    public NavigationParameters() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationParameters"/> class.
    /// </summary>
    /// <param name="dictionary">A <see cref="IDictionary{TKey, TValue}"/> containing initial values.</param>
    public NavigationParameters(IDictionary<string, object> dictionary) : base(dictionary)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationParameters"/> class.
    /// </summary>
    /// <param name="collection">A <see cref="IEnumerable{T}"/> containing initial values.</param>
    public NavigationParameters(IEnumerable<KeyValuePair<string, object>> collection) : base(collection)
    {
    }

    /// <summary>
    /// Gets a value for a given [key]. If no value exists it will return 'null'.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value or null.</returns>
    public new object this[string key]
    {
        get
        {
            if (TryGetValue(key, out object? value))
            {
                return value;
            }

            return null!;
        }
    }
}
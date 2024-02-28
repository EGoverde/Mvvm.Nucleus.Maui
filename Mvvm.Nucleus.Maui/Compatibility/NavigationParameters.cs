namespace Mvvm.Nucleus.Maui;

public class NavigationParameters : Dictionary<string, object?>, INavigationParameters
{
    public NavigationParameters() : base()
    {
    }

    public NavigationParameters(IDictionary<string, object?> dictionary) : base(dictionary)
    {
    }

    public NavigationParameters(IEnumerable<KeyValuePair<string, object?>> collection) : base(collection)
    {
    }

    public new object? this[string key]
    {
        get
        {
            if (TryGetValue(key, out object? value))
            {
                return value;
            }

            return null;
        }
    }
}
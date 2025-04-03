namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="INavigationParameters"/> is a compatibility class for Prism's NavigationParameters.
/// It differs from an usual <see cref="IDictionary2"/> in that accessing a key that is not
/// set will return a default value, rather than throw an <see cref="Exception"/>.
/// </summary>
public interface INavigationParameters : IDictionary<string, object>
{
}
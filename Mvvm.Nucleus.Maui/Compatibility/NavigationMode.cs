namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="NavigationMode"/> is a compatibility class for Prism's NavigationMode. It is recommended to only use 
/// this enum when you are migrating from a Prism codebase, as its values don't translate well to advanced navigation.
/// Recommended alternatives are <see cref="IInitializable"/>, <see cref="IInitializableAsync"/> or
/// <see cref="DictionaryExtensions.GetShellNavigationSource(IDictionary{string, object})"/>.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// Indicates that a navigation operation occured that resulted in navigating backwards in the navigation stack.
    /// </summary>
    Back,

    /// <summary>
    /// Indicates that a new navigation operation has occured and a new page has been added to the navigation stack.
    /// </summary>
    New,
}
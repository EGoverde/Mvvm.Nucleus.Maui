namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="CompatibilityNavigationParameters"/> holds keys used in the navigation parameters for compatibility.
/// </summary>
public static class CompatibilityNavigationParameters
{
    /// <summary>
    /// This key is automatically added to the parameters during navigation and holds the value for
    /// <see cref="NavigationMode"/>. This is a compatibility value and is not recommended to use other than for
    /// migrating a codebase from Prism. 
    /// </summary>
    public const string NavigationMode = "NavigationMode";
}
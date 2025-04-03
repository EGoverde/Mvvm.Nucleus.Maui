namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// The <see cref="CompatibilityExtensions"/> contains compatability extensions for migrating from a Prism codebase.
/// </summary>
public static class CompatibilityExtensions
{
    /// <summary>
    /// Compatbility extension for Prism. This extension is used to determine if the navigation is a back navigation.
    /// This is a compatibility value and is not recommended to use other than for migrating a codebase from Prism.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A value indicating whether a back navigation occurred (or will occur).</returns>
    public static bool IsBackNavigation(this IDictionary<string, object> parameters)
    {
        return parameters.GetNavigationMode() == NavigationMode.Back;
    }

    /// <summary>
    /// Compatbility extension for Prism. This extension is used to determine if the navigation is a back navigation
    /// or a navigation to a new page. This is a compatibility value and is not recommended to use other than for migrating
    /// a codebase from Prism.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>A value indicating the type of navigation that occurred (or will occur).</returns>
    public static NavigationMode GetNavigationMode(this IDictionary<string, object> parameters)
    {
        return parameters.GetStructOrDefault(CompatibilityNavigationParameters.NavigationMode, NavigationMode.New);
    }
}
namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NavigationDirection"/> is used in <see cref="IConfirmNavigation"/>  and <see cref="IConfirmNavigationAsync"/> 
/// and details the direction of a navigation.
/// </summary>
public enum NavigationDirection
{
    /// <summary>
    /// Indicates that a navigation operation will result in navigating backwards in the navigation stack. This is either a
    /// <see cref="ShellNavigationSource.Pop"/> or <see cref="ShellNavigationSource.PopToRoot"/>.
    /// </summary>
    Back,

    /// <summary>
    /// Indicates that a navigation operation will result in a new page to be added to the navigation stack. This is always
    /// a <see cref="ShellNavigationSource.Push"/>.
    /// </summary>
    Forwards,

    /// <summary>
    /// Indicates that a navigation operation is requested that is neither <see cref="Back"/> or <see cref="Forwards"/>. For details
    /// use <see cref="DictionaryExtensions.GetShellNavigationSource(IDictionary{string, object})"/> on the navigation parameters.
    /// Only used when <see cref="NucleusMvvmOptions.UseConfirmNavigationForAllNavigationRequests"/> is true.
    /// </summary>
    Other,
}
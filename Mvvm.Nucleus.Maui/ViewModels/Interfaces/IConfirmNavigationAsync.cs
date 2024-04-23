namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IConfirmNavigationAsync"/> can be used to interupt a page from navigating.
/// </summary>
public interface IConfirmNavigationAsync
{
    /// <summary>
    /// Triggered when a page is about to navigated away from.
    /// </summary>
    /// <param name="navigationDirection">The <see cref="NavigationDirection"/>.</param>
    /// <param name="navigationParameters">The navigation parameters.</param>
    /// <returns>A value indicating whether or not the navigation should proceed. If set to 'false' the navigation will be cancelled.</returns>
    Task<bool> CanNavigateAsync(NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters);
}
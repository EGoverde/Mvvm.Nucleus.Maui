namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IRefreshAsync"/> can be used to refresh data when a <see cref="Page"/> is being navigated
/// again, while it is already in the navigation stack or is being reused due to its scope.
/// Supports both ViewModels and Pages.
/// </summary>
public interface IRefreshAsync
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to while it is already in the navigation stack.
    /// Usually this means navigating backwards after closing a modal or popping a page. It also triggers when
    /// navigating to a reused page with a scope other than <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    Task RefreshAsync(IDictionary<string, object> navigationParameters);
}
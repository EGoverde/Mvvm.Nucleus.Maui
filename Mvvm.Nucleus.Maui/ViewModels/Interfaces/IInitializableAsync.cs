namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IInitializableAsync"/> can be used to load data when a page is being loaded or returned to.
/// </summary>
public interface IInitializableAsync
{
    /// <summary>
    /// Triggered when a page or popup is being navigated to for the first time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>

    Task InitAsync(IDictionary<string, object> navigationParameters);

    /// <summary>
    /// Triggered when a page or popup is being navigated to for a second or later time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    Task RefreshAsync(IDictionary<string, object> navigationParameters);
}
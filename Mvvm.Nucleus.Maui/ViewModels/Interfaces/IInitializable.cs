namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IInitializable"/> can be used to load data when a page is being loaded or returned to.
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Triggered when a page is being navigated to for the first time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Init(IDictionary<string, object> navigationParameters);

    /// <summary>
    /// Triggered when a page is being navigated to for a second or later time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Refresh(IDictionary<string, object> navigationParameters);
}
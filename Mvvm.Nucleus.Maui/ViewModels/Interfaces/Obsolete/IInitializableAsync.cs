namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IInitializableAsync"/> can be used to load data when a <see cref="Page"/> is being loaded or returned to.
/// </summary>
[Obsolete("Use IPrepareAsync and IRefreshAsync instead.")]
public interface IInitializableAsync
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for the first time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    [Obsolete("Use IPrepareAsync.PrepareAsync instead.")]
    Task InitAsync(IDictionary<string, object> navigationParameters);

    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for a second or later time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    [Obsolete("Use IRefreshAsync.RefreshAsync instead.")]
    Task RefreshAsync(IDictionary<string, object> navigationParameters);
}
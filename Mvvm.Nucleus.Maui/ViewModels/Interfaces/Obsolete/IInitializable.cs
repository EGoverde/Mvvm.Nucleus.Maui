namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IInitializable"/> can be used to load data when a <see cref="Page"/> is being loaded or returned to.
/// </summary>
[Obsolete("Use IPrepare and IRefresh instead.")]
public interface IInitializable
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for the first time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    [Obsolete("Use IPrepare.Prepare instead.")]
    void Init(IDictionary<string, object> navigationParameters);

    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for a second or later time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    [Obsolete("Use IRefresh.Refresh instead.")]
    void Refresh(IDictionary<string, object> navigationParameters);
}
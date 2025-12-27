namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupInitializableAsync"/> can be used to load data when a popup is being loaded.
/// </summary>
[Obsolete("Use IPopupPrepareAsync instead.")]
public interface IPopupInitializableAsync
{
    /// <summary>
    /// Gets a value indicating whether <see cref="InitAsync(IDictionary{string, object})"/> should be awaited
    /// before showing the popup.
    /// </summary>
    [Obsolete("Use IPopupPrepareAsync.AwaitInitializeBeforeShowing instead.")]
    public bool AwaitInitializeBeforeShowing { get; }

    /// <summary>
    /// Triggered before a popup is shown to load data. It will either await the function before continueing or
    /// finish it in the background, depending on <see cref="AwaitInitializeBeforeShowing"/>.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    [Obsolete("Use IPopupPrepareAsync.PrepareAsync instead.")]
    Task InitAsync(IDictionary<string, object> navigationParameters);
}
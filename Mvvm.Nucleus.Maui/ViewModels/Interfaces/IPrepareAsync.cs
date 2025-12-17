namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPrepareAsync"/> is triggered when a <see cref="Page"/> is being created for the first time.
/// The <see cref="PrepareAsync(IDictionary{string, object})"/> may or may not have finished when the <see cref="Page"/>
/// is being displayed. The alternative <see cref="IPrepare"/> is ensured to be finished, but is instead synchronous.
/// Note that when different scopes than <see cref="ServiceLifetime.Transient"/> this will only ever be triggered
/// once, as it will not be called for re-used instances.
/// </summary>
public interface IPrepareAsync
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being created for the first time, before it is returned to <see cref="Shell"/> for navigation.
    /// It will not be awaited before the navigation continues, but run asynchronously.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    Task PrepareAsync(IDictionary<string, object> navigationParameters);
}
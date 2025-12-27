namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPrepareAsync"/> can be used to load data when a <see cref="Page"/> is being created.
/// Supports both ViewModels and Pages.
/// </summary>
public interface IPrepareAsync
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being created for the first time, before it is returned to <see cref="Shell"/> 
    /// for navigation. It will not be awaited before the navigation continues, but run asynchronously.
    /// Note that with different scopes than <see cref="ServiceLifetime.Transient"/> this will only ever be triggered
    /// once, while reused instances will call <see cref="IRefresh"/> instead.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    Task PrepareAsync(IDictionary<string, object> navigationParameters);
}
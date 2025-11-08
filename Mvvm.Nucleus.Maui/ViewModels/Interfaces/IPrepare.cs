namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPrepare"/> is triggered when a <see cref="Page"/> is being created for the first time.
/// This allows for using values from the NavigationParameters before the page is used.
/// Note that when different scopes than <see cref="ServiceLifetime.Transient"/> this will only ever be triggered
/// once, as it will not be called for re-used instances.
/// </summary>
public interface IPrepare
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being created for the first time, before it is returned to <see cref="Shell"/> for navigation.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Prepare(IDictionary<string, object> navigationParameters);
}
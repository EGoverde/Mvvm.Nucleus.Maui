namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="INavigationService"/> handles navigation from and to given routes or views, which should be registered
/// in the initialization of Nucleus.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="INavigationService"/> is currently navigating.
    /// </summary>
    bool IsNavigating { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> of the currently loaded route.
    /// </summary>
    Uri CurrentRoute { get; }

    /// <summary>
    /// Navigates to a given view and corresponding viewmodel.
    /// </summary>
    /// <typeparam name="TView">The <see cref="Type"/> of the view to navigate to.</typeparam>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateAsync<TView>();

    /// <summary>
    /// Navigates to a given view and corresponding viewmodel.
    /// </summary>
    /// <typeparam name="TView">The <see cref="Type"/> of the view to navigate to.</typeparam>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateAsync<TView>(IDictionary<string, object>? navigationParameters, bool isAnimated = true);

    /// <summary>
    /// Navigates to a given view and corresponding viewmodel.
    /// </summary>
    /// <param name="viewType">The <see cref="Type"/> of the view to navigate to.</param>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

    /// <summary>
    /// Navigates to a given route.
    /// </summary>
    /// <param name="route">The route.</param>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

    /// <summary>
    /// Navigates one step backwards.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateBackAsync();

    /// <summary>
    /// Navigates one step backwards.
    /// </summary>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task NavigateBackAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true);

    /// <summary>
    /// Close the current modally presented page or navigation page, but does nothing if no modal presentation is active.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CloseModalAsync();

    /// <summary>
    /// Close the current modally presented page or navigation page, but does nothing if no modal presentation is active.
    /// </summary>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CloseModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true);

    /// <summary>
    /// Close all modally presented pages or navigation pages, but does nothing if no modal presentation is active.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CloseAllModalAsync();

    /// <summary>
    /// Close all modally presented pages or navigation pages, but does nothing if no modal presentation is active.
    /// </summary>
    /// <param name="navigationParameters">An optional dictionary passed to the destination.</param>
    /// <param name="isAnimated">A value indicating whether the navigation should be animated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true);
}
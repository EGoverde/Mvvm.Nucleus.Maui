using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupPrepare"/> can be used to load data when a <see cref="Popup"/> is being created.
/// Supports both ViewModels and Popups.
/// </summary>
public interface IPopupPrepareAsync
{
    /// <summary>
    /// Gets a value indicating whether <see cref="PrepareAsync(IDictionary{string, object})"/> should be awaited
    /// before showing the popup.
    /// </summary>
    public bool AwaitInitializeBeforeShowing { get; }

    /// <summary>
    /// Triggered before a popup is shown to load data. It will either await the function before continueing or
    /// finish it in the background, depending on <see cref="AwaitInitializeBeforeShowing"/>.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task PrepareAsync(IDictionary<string, object> navigationParameters);
}
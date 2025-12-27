using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupLifecycleAware"/> handles the events of the <see cref="Popup"/> opening and closing.
/// Supports both ViewModels and Popups.
/// </summary>
public interface IPopupLifecycleAware
{
    /// <summary>
    /// Triggered when the <see cref="Popup"/> opens.
    /// </summary>
    void OnOpened();

    /// <summary>
    /// Triggered when the <see cref="Popup"/> closes.
    /// </summary>
    void OnClosed();
}
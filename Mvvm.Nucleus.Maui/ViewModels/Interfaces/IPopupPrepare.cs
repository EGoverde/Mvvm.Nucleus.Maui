using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupPrepare"/> can be used to load data when a <see cref="Popup"/> is being created.
/// Supports both ViewModels and Popups.
/// </summary>
public interface IPopupPrepare
{
    /// <summary>
    /// Triggered before a popup is shown to load data.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Prepare(IDictionary<string, object> navigationParameters);
}
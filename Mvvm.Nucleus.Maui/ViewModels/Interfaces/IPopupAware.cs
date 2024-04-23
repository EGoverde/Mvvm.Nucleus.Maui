using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupAware"/> is used for a viewmodel bound to a <see cref="Popup"/> to get a reference
/// to the <see cref="Popup"/> - primarily used for closing it.
/// </summary>
public interface IPopupAware
{
    /// <summary>
    /// Gets or sets a <see cref="WeakReference"/> to the <see cref="Popup"/>.
    /// </summary>
    WeakReference<Popup>? Popup { get; set; }
}
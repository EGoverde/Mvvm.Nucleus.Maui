using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupAware"/> is used for a viewmodel bound to a <see cref="Popup"/> to get a reference
/// to the <see cref="Popup"/>. This allows for access to <see cref="Popup.CloseAsync(CancellationToken)"/>.
/// If you want the reference to have a specific type, use <see cref="IPopupAware{T}"/> instead.
/// </summary>
public interface IPopupAware
{
    /// <summary>
    /// Gets or sets a <see cref="WeakReference"/> to the <see cref="Popup"/>.
    /// </summary>
    WeakReference<Popup>? Popup { get; set; }
}

/// <summary>
/// The <see cref="IPopupAware"/> is used for a viewmodel bound to a <see cref="Popup"/> to get a reference
/// to the <see cref="Popup"/>. This allows for access to <see cref="Popup{T}.CloseAsync(T, CancellationToken)"/>.
/// </summary>
/// <typeparam name="T">The <see cref="Type"/> of the <see cref="Popup"/>.</typeparam>
public interface IPopupAware<T> where T : Popup
{
    /// <summary>
    /// Gets or sets a <see cref="WeakReference"/> to the <see cref="Popup"/>.
    /// </summary>
    WeakReference<T>? Popup { get; set; }
}
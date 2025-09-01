using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IInitializable"/> can be used to load data when a <see cref="Page"/> or <see cref="Popup"/> is being loaded or returned to.
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for the first time,
    /// or when a <see cref="Popup"/> is opened for the first time.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Init(IDictionary<string, object> navigationParameters);

    /// <summary>
    /// Triggered when a <see cref="Page"/> is being navigated to for a second or later time,
    /// or when a <see cref="Popup"/> is opened again in the case of a reused popup instance.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Refresh(IDictionary<string, object> navigationParameters);
}
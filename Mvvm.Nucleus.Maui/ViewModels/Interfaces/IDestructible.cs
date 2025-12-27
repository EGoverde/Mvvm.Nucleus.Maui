using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IDestructible"/> can be used to cleanup resources from memory.
/// Supports both ViewModels and Pages.
/// </summary>
public interface IDestructible
{
    /// <summary>
    /// Triggered when a <see cref="Page"/> or <see cref="Popup"/> is no longer necessary.
    /// </summary>
    void Destroy();
}
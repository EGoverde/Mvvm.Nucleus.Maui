namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupInitializable"/> can be used to load data when a popup is being loaded.
/// </summary>
public interface IPopupInitializable
{
    /// <summary>
    /// Triggered before a popup is shown to load data.
    /// </summary>
    /// <param name="navigationParameters">The navigation parameters.</param>
    void Init(IDictionary<string, object> navigationParameters);
}
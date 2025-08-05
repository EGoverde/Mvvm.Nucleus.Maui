using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupViewFactory"/> is called when MAUI creates views for popups and is used to wire up events and viewmodels.
/// </summary>
public interface IPopupViewFactory
{
    /// <summary>
    /// Creates a requested View through IoC.
    /// </summary>
    /// <param name="viewType">The <see cref="Type"/> of view to create.</param>
    /// <returns>The created view.</returns>
    object CreateView(Type viewType);

    /// <summary>
    /// Applies a behavior to a given view that handles the various events, as well as creates and attaches the viewmodel.
    /// If a View is resolved that is not a <see cref="Popup"/> it listens to parent changes until it is wrapped in one by
    /// the Community Toolkit.
    /// </summary>
    /// <param name="element">The view.</param>
    /// <returns>The configured view.</returns>
    object ConfigureView(Element element);
}
namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IViewFactory"/> is called when MAUI creates views and is used to wire up events and viewmodels.
/// </summary>
public interface IViewFactory
{
    /// <summary>
    /// Creates a requested View through IoC.
    /// </summary>
    /// <param name="viewType">The <see cref="Type"/> of view to create.</param>
    /// <returns>The created view.</returns>
    object CreateView(Type viewType);

    /// <summary>
    /// Applies a behavior to a given view that handles the various events, as well as creates and attaches the viewmodel.
    /// If a View is resolved that is not a <see cref="Page"/> it attempts to register the behavior at its parent, so it will
    /// also receive its events.
    /// </summary>
    /// <param name="element">The view.</param>
    /// <returns>The configured view.</returns>
    object ConfigureView(Element element);
}
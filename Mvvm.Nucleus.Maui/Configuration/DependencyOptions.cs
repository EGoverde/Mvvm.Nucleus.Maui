using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="DependencyOptions"/> is used for registering Views, ViewModels and Popups.
/// </summary>
public class DependencyOptions
{
    private readonly IList<ViewMapping> _viewMappings = new List<ViewMapping>();
    
    private readonly IList<PopupMapping> _popupMappings = new List<PopupMapping>();

    internal IReadOnlyCollection<ViewMapping> ViewMappings => new ReadOnlyCollection<ViewMapping>(_viewMappings);
    
    internal IReadOnlyCollection<PopupMapping> PopupMappings => new ReadOnlyCollection<PopupMapping>(_popupMappings);

    /// <summary>
    /// Registers a View and ViewModel. This is for Views that are not present in AppShell.xaml and that use 'global' routes.
    /// These Views can be pushed on any NavigationStack. For Views present in AppShell.xaml see <see cref="RegisterShellView{TView, TViewModel}(string, ServiceLifetime)"/>.
    /// </summary>
    /// <typeparam name="TView">The <see cref="Type"/> of the View.</typeparam>
    /// <typeparam name="TViewModel">The <see cref="Type"/> of the ViewModel.</typeparam>
    /// <param name="route">The route. If not set it uses the name of the View.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to be used.</param>
    /// <exception cref="ArgumentException">Thrown if a registration conflicts with an already existing registration.</exception>
    public void RegisterView<TView, TViewModel>(string? route = null, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TView : NavigableElement
        where TViewModel : notnull
    {
        var viewType = typeof(TView);

        if (string.IsNullOrEmpty(route))
        {
            route = viewType.Name;
        }

        RegisterView<TView, TViewModel>(route, ViewRouteType.GlobalRoute, serviceLifetime);
    }

    /// <summary>
    /// Registers a View and ViewModel. This is for Views that are present in AppShell.xaml and that use 'absolute' routes.
    /// For Views not present in AppShell.xaml see <see cref="RegisterView{TView, TViewModel}(string?, ServiceLifetime)"/>.
    /// </summary>
    /// <typeparam name="TView">The <see cref="Type"/> of the View.</typeparam>
    /// <typeparam name="TViewModel">The <see cref="Type"/> of the ViewModel.</typeparam>
    /// <param name="absoluteRoute">The route. This should match the route set in Shell.xaml.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to be used.</param>
    /// <exception cref="ArgumentException">Thrown if a registration conflicts with an already existing registration.</exception>
    public void RegisterShellView<TView, TViewModel>(string absoluteRoute, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TView : NavigableElement
        where TViewModel : notnull
    {
        if (string.IsNullOrEmpty(absoluteRoute) || !absoluteRoute.StartsWith("//"))
        {
            throw new ArgumentException("This function is for Shell absolute routes and requires a route starting with '//'. They should also be registered in your AppShell.xaml. Use RegisterView for Global Routes.");
        }

        RegisterView<TView, TViewModel>(absoluteRoute, ViewRouteType.AbsoluteRoute, serviceLifetime);
    }

    /// <summary>
    /// Registers a <see cref="Popup"/> and ViewModel. To register a <see cref="Popup"/> without a ViewModel use <see cref="RegisterPopup{TPopupView}(ServiceLifetime)"/>.
    /// </summary>
    /// <typeparam name="TPopupView">The <see cref="Type"/> of the <see cref="Popup"/> or <see cref="ContentView"/>.</typeparam>
    /// <typeparam name="TPopupViewModel">The <see cref="Type"/> of the ViewModel.</typeparam>
    /// <exception cref="ArgumentException">Thrown if a registration already exists for a given <see cref="Popup"/>.</exception>
    public void RegisterPopup<TPopupView, TPopupViewModel>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TPopupView : ContentView
        where TPopupViewModel : notnull
    {
        var popupViewType = typeof(TPopupView);
        var popupViewModelType = typeof(TPopupViewModel);

        var existingPopupViewRegistration = _popupMappings.FirstOrDefault(x => x.PopupViewType == popupViewType);
        if (existingPopupViewRegistration != null)
        {
            throw new ArgumentException($"Registration for Popup '{popupViewType}' already found.");
        }

        _popupMappings.Add(PopupMapping.Create<TPopupView, TPopupViewModel>(serviceLifetime));
    }

    /// <summary>
    /// Registers a <see cref="Popup"/>. To register a <see cref="Popup"/> with a ViewModel use <see cref="RegisterPopup{TPopupView, TPopupViewModel}(ServiceLifetime)"/>.
    /// </summary>
    /// <typeparam name="TPopupView">The <see cref="Type"/> of the <see cref="Popup"/> or <see cref="ContentView"/>.</typeparam>
    /// <exception cref="ArgumentException">Thrown if a registration already exists for a given <see cref="Popup"/>.</exception>
    public void RegisterPopup<TPopupView>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TPopupView : ContentView
    {
        var popupViewType = typeof(TPopupView);

        var existingPopupViewRegistration = _popupMappings.FirstOrDefault(x => x.PopupViewType == popupViewType);
        if (existingPopupViewRegistration != null)
        {
            throw new ArgumentException($"Registration for Popup '{popupViewType}' already found.");
        }

        _popupMappings.Add(PopupMapping.Create<TPopupView>(serviceLifetime));
    }

    private void RegisterView<TView, TViewModel>(string? route, ViewRouteType registrationType, ServiceLifetime serviceLifetime)
    {
        var viewType = typeof(TView);
        var viewModelType = typeof(TViewModel);

        var existingRoute = _viewMappings.FirstOrDefault(x => x.Route == route);
        if (existingRoute != null)
        {
            throw new ArgumentException($"Registration for route '{route}' already found, bound to View '{existingRoute.ViewType}'.");
        }

        var existingViewRegistration = _viewMappings.FirstOrDefault(x => x.ViewType == viewType);
        if (existingViewRegistration != null && existingViewRegistration.Route == route)
        {
            throw new ArgumentException($"Registration for View '{viewType}' already found with the same route of '{route}'.");
        }
        else if (existingViewRegistration != null && existingViewRegistration.ViewModelType != viewModelType)
        {
            throw new ArgumentException($"Registration for View '{viewType}' already found with a different ViewModel.");
        }

        _viewMappings.Add(ViewMapping.Create<TView, TViewModel>(route, registrationType, serviceLifetime));
    }
}
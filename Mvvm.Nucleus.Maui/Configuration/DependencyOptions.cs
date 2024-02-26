using System.Collections.ObjectModel;

namespace Mvvm.Nucleus.Maui;

public class DependencyOptions
{
    private readonly IList<ViewMapping> _viewMappings = new List<ViewMapping>();

    internal IReadOnlyCollection<ViewMapping> ViewMappings => new ReadOnlyCollection<ViewMapping>(_viewMappings);

    public void RegisterView<TView, TViewModel>(string? route = null, ViewScope registrationScope = default)
        where TView : NavigableElement
        where TViewModel : notnull
    {
        var viewType = typeof(TView);

        if (string.IsNullOrEmpty(route))
        {
            route = viewType.Name;
        }

        RegisterView<TView, TViewModel>(route, ViewRouteType.GlobalRoute, registrationScope);
    }

    public void RegisterShellView<TView, TViewModel>(string absoluteRoute, ViewScope registrationScope = default)
        where TView : NavigableElement
        where TViewModel : notnull
    {
        if (string.IsNullOrEmpty(absoluteRoute) || !absoluteRoute.StartsWith("//"))
        {
            throw new ArgumentException("This function is for Shell absolute routes and requires a route starting with '//'. They should also be registered in your AppShell.xaml. Use RegisterView for Global Routes.");
        }

        RegisterView<TView, TViewModel>(absoluteRoute, ViewRouteType.AbsoluteRoute, registrationScope);
    }

    private void RegisterView<TView, TViewModel>(string? route, ViewRouteType registrationType, ViewScope registrationScope)
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

        _viewMappings.Add(ViewMapping.Create<TView, TViewModel>(route, registrationType, registrationScope));
    }
}
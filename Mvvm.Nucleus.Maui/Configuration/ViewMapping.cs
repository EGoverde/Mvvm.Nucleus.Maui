namespace Mvvm.Nucleus.Maui;

public class ViewMapping
{
    internal ViewMapping(Type viewType, Type viewModelType, string? route, ViewRouteType registrationType, ViewScope registrationScope)
    {
        ViewType = viewType;
        ViewModelType = viewModelType;
        Route = route;
        RegistrationType = registrationType;
        RegistrationScope = registrationScope;
    }

    public Type ViewType { get; }

    public Type ViewModelType { get; }

    public string? Route { get; }

    public ViewRouteType RegistrationType { get; }

    public ViewScope RegistrationScope { get; }

    internal static ViewMapping Create<TView, TViewModel>(string? route, ViewRouteType registrationType, ViewScope registrationScope)
    {
        return new ViewMapping(typeof(TView), typeof(TViewModel), route, registrationType, registrationScope);
    }
}
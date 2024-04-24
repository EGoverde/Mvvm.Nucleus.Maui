namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewMapping"/> class holds the registration for a View and ViewModel.
/// </summary>
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

    /// <summary>
    /// The <see cref="Type"/> of the View.
    /// </summary>
    public Type ViewType { get; }

    /// <summary>
    /// The <see cref="Type"/> of the ViewModel.
    /// </summary>
    public Type ViewModelType { get; }

    /// <summary>
    /// The route for this given registration.
    /// </summary>
    public string? Route { get; }

    /// <summary>
    /// The <see cref="ViewRouteType"/>, e.a. absolute or global.
    /// </summary>
    public ViewRouteType RegistrationType { get; }

    /// <summary>
    /// The <see cref="ViewScope"/> of this registration.
    /// </summary>
    public ViewScope RegistrationScope { get; }

    internal static ViewMapping Create<TView, TViewModel>(string? route, ViewRouteType registrationType, ViewScope registrationScope)
    {
        return new ViewMapping(typeof(TView), typeof(TViewModel), route, registrationType, registrationScope);
    }
}
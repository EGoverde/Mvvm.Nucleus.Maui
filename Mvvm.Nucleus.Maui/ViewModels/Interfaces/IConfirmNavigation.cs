namespace Mvvm.Nucleus.Maui;

public interface IConfirmNavigation
{
    bool CanNavigate(NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters);
}
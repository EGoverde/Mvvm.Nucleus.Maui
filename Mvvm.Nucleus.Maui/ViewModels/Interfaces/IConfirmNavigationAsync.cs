namespace Mvvm.Nucleus.Maui;

public interface IConfirmNavigationAsync
{
    Task<bool> CanNavigateAsync(NavigationDirection navigationDirection, IDictionary<string, object?> navigationParameters);
}
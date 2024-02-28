namespace Mvvm.Nucleus.Maui
{
    public interface INavigationService
	{
        bool IsNavigating { get; }

        Uri CurrentRoute { get; }

        Task NavigateAsync<TView>();

        Task NavigateAsync<TView>(IDictionary<string, object?>? navigationParameters, bool isAnimated = true);

        Task NavigateAsync(Type viewType, IDictionary<string, object?>? navigationParameters = null, bool isAnimated = true);

        Task NavigateToRouteAsync(string route, IDictionary<string, object?>? navigationParameters = null, bool isAnimated = true);

        Task NavigateBackAsync();

        Task NavigateBackAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true);

        Task CloseModalAsync();

        Task CloseModalAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true);

        Task CloseAllModalAsync();

        Task CloseAllModalAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true);
    }
}
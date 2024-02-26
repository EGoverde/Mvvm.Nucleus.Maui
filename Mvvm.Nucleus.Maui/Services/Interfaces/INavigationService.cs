using System.Runtime.Versioning;

namespace Mvvm.Nucleus.Maui
{
    public interface INavigationService
	{
        bool IsNavigating { get; }

        Task NavigateAsync<TView>(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

        Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

        Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

        Task NavigateBackAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

        [RequiresPreviewFeatures]
        Task NavigateModalAsync<TView>(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true, bool wrapNavigationPage = true);

        [RequiresPreviewFeatures]
        Task NavigateModalAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true, bool wrapNavigationPage = true);

        [RequiresPreviewFeatures]
        Task CloseModalAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);

        [RequiresPreviewFeatures]
        Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true);
    }
}
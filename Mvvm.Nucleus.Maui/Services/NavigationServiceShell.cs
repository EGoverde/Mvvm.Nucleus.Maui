using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class NavigationServiceShell : INavigationService
    {
        private readonly NucleusMvvmOptions _nucleusMvvmOptions;
        private readonly ILogger<NavigationServiceShell> _logger;

        public bool IsNavigating => NucleusMvvmCore.Current.IsNavigating;

        public Uri CurrentRoute => Shell.Current.CurrentState.Location;

        public NavigationServiceShell(NucleusMvvmOptions nucleusMvvmOptions, ILogger<NavigationServiceShell> logger)
        {
            _nucleusMvvmOptions = nucleusMvvmOptions;
            _logger = logger;
        }

        public Task NavigateAsync<TView>()
        {
            return NavigateAsync<TView>(null);
        }

        public Task NavigateAsync<TView>(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            return NavigateAsync(typeof(TView), navigationParameters, isAnimated);
        }

        public virtual async Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            var viewMapping = GetViewMapping(viewType);
            if (viewMapping == null)
            {
                _logger.LogError($"No valid mapping found for view of type '{viewType}'.");
                return;
            }

            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);
            await Shell.Current.GoToAsync(viewMapping.Route, isAnimated, NucleusMvvmCore.Current.NavigationParameters);
        }

        public virtual Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);
            return Shell.Current.GoToAsync(route, isAnimated, new ShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public Task NavigateBackAsync()
        {
            return NavigateBackAsync(null);
        }

        public virtual Task NavigateBackAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);
            return Shell.Current.GoToAsync("..", isAnimated, new ShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public Task CloseModalAsync()
        {
            return CloseModalAsync(null);
        }

        public virtual async Task CloseModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            var navigation = Shell.Current.CurrentPage?.Navigation;

            var modalStackCount = navigation?.ModalStack?.Count ?? 0;
            if (modalStackCount < 1)
            {
                return;
            }

            var navigationStackCount = navigation?.NavigationStack?.Count ?? 1;
            navigationStackCount = navigationStackCount - 1;

            var navigationPath = "..";

            for (int i = 0; i < navigationStackCount; i++)
            {
                navigationPath += "/..";
            }

            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);
            await Shell.Current.GoToAsync(navigationPath, isAnimated, NucleusMvvmCore.Current.NavigationParameters);
        }

        public Task CloseAllModalAsync()
        {
            return CloseAllModalAsync(null);
        }

        public virtual async Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            var currentPage = Shell.Current.CurrentPage;
            var navigation = currentPage?.Navigation;

            var modalStackCount = navigation?.ModalStack?.Count ?? 0;
            if (modalStackCount < 1)
            {
                return;
            }

            var navigationStackCount = 0;

            foreach (var modalItem in navigation!.ModalStack)
            {
                navigationStackCount += modalItem?.Navigation?.NavigationStack?.Count ?? 1;
            }

            navigationStackCount = navigationStackCount - 1;

            var navigationPath = "..";

            for (int i = 0; i < navigationStackCount; i++)
            {
                navigationPath += "/..";
            }

            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);
            await Shell.Current.GoToAsync(navigationPath, isAnimated, NucleusMvvmCore.Current.NavigationParameters);
        }

        private ViewMapping? GetViewMapping(Type viewType)
        {
            var viewMappings = _nucleusMvvmOptions.ViewMappings.Where(x => x.ViewType == viewType);
            if (viewMappings.Count() > 1)
            {
                _logger.LogWarning($"Multiple mappings found for View '{viewType}', choosing the first result.");
            }

            return viewMappings.FirstOrDefault();
        }

        private IDictionary<string, object> GetOrCreateNavigationParameters(IDictionary<string, object>? navigationParameters)
        {
            return navigationParameters ?? new Dictionary<string, object>();
        }
    }
}
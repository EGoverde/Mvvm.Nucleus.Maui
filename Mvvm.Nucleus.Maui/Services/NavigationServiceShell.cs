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

        public Task NavigateAsync<TView>(IDictionary<string, object?>? navigationParameters, bool isAnimated = true)
        {
            return NavigateAsync(typeof(TView), navigationParameters, isAnimated);
        }

        public virtual async Task NavigateAsync(Type viewType, IDictionary<string, object?>? navigationParameters = null, bool isAnimated = true)
        {
            if (ShouldIgnoreNavigationRequest())
            {
                return;
            }

            var viewMapping = GetViewMapping(viewType);
            if (viewMapping == null)
            {
                _logger.LogError($"No valid mapping found for view of type '{viewType}'.");
                return;
            }

            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);

            await Shell.Current.GoToAsync(viewMapping.Route, isAnimated, GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public virtual Task NavigateToRouteAsync(string route, IDictionary<string, object?>? navigationParameters = null, bool isAnimated = true)
        {
            if (ShouldIgnoreNavigationRequest())
            {
                return Task.CompletedTask;
            }

            var parameters = GetOrCreateNavigationParameters(navigationParameters);

            if (_nucleusMvvmOptions.AddQueryParametersToDictionary)
            {
                var queryParameters = GetQueryParameterDictionary(route);

                foreach (var parameter in queryParameters)
                {
                    if (parameters.ContainsKey(parameter.Key))
                    {
                        _logger.LogWarning($"Query parameter with key '{parameter.Key}' already exists in NavigationParameters, not adding value.");
                        continue;
                    }
                        
                    parameters.Add(parameter.Key, parameter.Value);
                }
            }

            NucleusMvvmCore.Current.NavigationParameters = parameters;

            return Shell.Current.GoToAsync(route, isAnimated, GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public Task NavigateBackAsync()
        {
            return NavigateBackAsync(null);
        }

        public virtual Task NavigateBackAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true)
        {
            if (ShouldIgnoreNavigationRequest())
            {
                return Task.CompletedTask;
            }

            NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);

            return Shell.Current.GoToAsync("..", isAnimated, GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public Task CloseModalAsync()
        {
            return CloseModalAsync(null);
        }

        public virtual async Task CloseModalAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true)
        {
            if (ShouldIgnoreNavigationRequest())
            {
                return;
            }

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

            await Shell.Current.GoToAsync(navigationPath, isAnimated, GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        public Task CloseAllModalAsync()
        {
            return CloseAllModalAsync(null);
        }

        public virtual async Task CloseAllModalAsync(IDictionary<string, object?>? navigationParameters, bool isAnimated = true)
        {
            if (ShouldIgnoreNavigationRequest())
            {
                return;
            }

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

            await Shell.Current.GoToAsync(navigationPath, isAnimated, GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters));
        }

        private bool ShouldIgnoreNavigationRequest()
        {
            if (_nucleusMvvmOptions.IgnoreNavigationWhenInProgress && IsNavigating)
            {
                _logger.LogWarning($"Ignoring this navigation request as we're already navigating. You can change this setting in the MauiProgram initialization.");

                return true;
            }

            return false;
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

        private IDictionary<string, object?> GetOrCreateNavigationParameters(IDictionary<string, object?>? navigationParameters)
        {
            return navigationParameters ?? new Dictionary<string, object?>();
        }

        private IDictionary<string, object?> GetOrCreateShellNavigationQueryParameters(IDictionary<string, object?>? navigationParameters)
        {
            if (navigationParameters is ShellNavigationQueryParameters)
            {
                return navigationParameters;
            }

            if (_nucleusMvvmOptions.UseShellNavigationQueryParameters)
            {
                return new ShellNavigationQueryParameters(navigationParameters ?? new Dictionary<string, object?>());
            }

            return navigationParameters ?? new Dictionary<string, object?>();
        }

        private IDictionary<string, string> GetQueryParameterDictionary(string route)
		{
            Dictionary<string, string> result = new(StringComparer.Ordinal);

            var isSuccess = Uri.TryCreate(new Uri ("app://root/"), route, out Uri? uri);
            if (!isSuccess || string.IsNullOrWhiteSpace(uri?.Query))
            {
                return result;
            }

			var query = uri.Query.StartsWith("?", StringComparison.Ordinal) ? uri.Query.Substring(1) : uri.Query;

			foreach (var part in query.Split('&'))
			{
				var p = part.Split('=');
				if (p.Length != 2)
                {
					continue;
                }

				result[p[0]] = p[1];
			}

			return result;
		}
    }
}
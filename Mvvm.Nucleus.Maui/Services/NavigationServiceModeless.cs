using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class NavigationServiceModeless : INavigationService
    {
        private readonly NucleusMvvmOptions _nucleusMvvmOptions;
        private readonly ILogger<NavigationServiceModeless> _logger;

        public bool IsNavigating => NucleusMvvmCore.Current.IsNavigating;

        public Uri CurrentRoute => throw new NotImplementedException();

        public NavigationServiceModeless(NucleusMvvmOptions nucleusMvvmOptions, ILogger<NavigationServiceModeless> logger)
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

        public virtual Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            var viewMapping = GetViewMapping(viewType);
            if (viewMapping == null)
            {
                _logger.LogError($"No valid mapping found for view of type '{viewType}'.");
                
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        public virtual Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        public Task NavigateBackAsync()
        {
            return NavigateBackAsync(null);
        }

        public virtual Task NavigateBackAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        public Task CloseModalAsync()
        {
            return CloseModalAsync(null);
        }

        public virtual Task CloseModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        public Task CloseAllModalAsync()
        {
            return CloseAllModalAsync(null);
        }

        public virtual Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
        {
            throw new NotImplementedException();
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
    }
}
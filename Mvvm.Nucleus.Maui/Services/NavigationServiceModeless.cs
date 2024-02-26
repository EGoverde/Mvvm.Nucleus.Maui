using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace Mvvm.Nucleus.Maui
{
    public class NavigationServiceModeless : INavigationService
    {
        private readonly NucleusMvvmOptions _nucleusMvvmOptions;
        private readonly ILogger<NavigationServiceModeless> _logger;

        public bool IsNavigating => NucleusMvvmCore.Current.IsNavigating;

        public NavigationServiceModeless(NucleusMvvmOptions nucleusMvvmOptions, ILogger<NavigationServiceModeless> logger)
        {
            _nucleusMvvmOptions = nucleusMvvmOptions;
            _logger = logger;
        }

        public Task NavigateAsync<TView>(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            return NavigateAsync(typeof(TView), navigationParameters, isAnimated, false);
        }

        public Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            return NavigateAsync(viewType, navigationParameters, isAnimated, false);
        }

        public Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        public Task NavigateBackAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        [RequiresPreviewFeatures]
        public Task NavigateModalAsync<TView>(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true, bool wrapNavigationPage = true)
        {
            return NavigateAsync(typeof(TView), navigationParameters, isAnimated, true, wrapNavigationPage);
        }

        [RequiresPreviewFeatures]
        public Task NavigateModalAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true, bool wrapNavigationPage = true)
        {
            return NavigateAsync(viewType, navigationParameters, isAnimated, true, wrapNavigationPage);
        }

        [RequiresPreviewFeatures]
        public Task CloseModalAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        [RequiresPreviewFeatures]
        public Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
        {
            throw new NotImplementedException();
        }

        private Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters, bool isAnimated, bool isModal, bool wrapNavigationPage = false)
        {
            var viewMapping = GetViewMapping(viewType);
            if (viewMapping == null)
            {
                _logger.LogError($"No valid mapping found for view of type '{viewType}'.");
                
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
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
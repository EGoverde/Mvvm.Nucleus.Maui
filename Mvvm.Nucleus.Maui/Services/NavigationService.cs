using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NavigationService"/> is the default implementation for <see cref="INavigationService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class NavigationService : INavigationService
{    
    private readonly NucleusMvvmOptions _nucleusMvvmOptions;

    private readonly ILogger<NavigationService> _logger;

    private IList<Page> _transientPagesOnNavigating = new List<Page>();

    private bool _isNavigating;

    /// <inheritdoc/>
    public virtual bool IsNavigating
    { 
        get => _isNavigating;
        protected set => SetIsNavigating(value);
    }

    /// <inheritdoc/>
    public Uri CurrentRoute => Shell.Current.CurrentState.Location;

    /// <inheritdoc/>
    public event EventHandler<bool>? IsNavigatingChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public NavigationService(NucleusMvvmOptions nucleusMvvmOptions, ILogger<NavigationService> logger)
    {
        _nucleusMvvmOptions = nucleusMvvmOptions;
        _logger = logger;

        NucleusMvvmCore.Current.ShellNavigating += ShellNavigating!;
        NucleusMvvmCore.Current.ShellNavigated += ShellNavigated!;
    }

    /// <inheritdoc/>
    public Task NavigateAsync<TView>()
    {
        return NavigateAsync<TView>(null);
    }

    /// <inheritdoc/>
    public Task NavigateAsync<TView>(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
    {
        return NavigateAsync(typeof(TView), navigationParameters, isAnimated);
    }

    /// <inheritdoc/>
    public virtual async Task NavigateAsync(Type viewType, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
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

        await HandleShellNavigationAsync(() => Shell.Current.GoToAsync(viewMapping.Route, GetIsAnimated(isAnimated), GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters)));
    }

    /// <inheritdoc/>
    public virtual Task NavigateToRouteAsync(string route, IDictionary<string, object>? navigationParameters = null, bool isAnimated = true)
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

        return HandleShellNavigationAsync(() => Shell.Current.GoToAsync(route, GetIsAnimated(isAnimated), GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters)));
    }

    /// <inheritdoc/>
    public Task NavigateBackAsync()
    {
        return NavigateBackAsync(null);
    }

    /// <inheritdoc/>
    public virtual Task NavigateBackAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
    {
        if (ShouldIgnoreNavigationRequest())
        {
            return Task.CompletedTask;
        }

        NucleusMvvmCore.Current.NavigationParameters = GetOrCreateNavigationParameters(navigationParameters);

        return HandleShellNavigationAsync(() => Shell.Current.GoToAsync("..", GetIsAnimated(isAnimated), GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters)));
    }

    /// <inheritdoc/>
    public Task CloseModalAsync()
    {
        return CloseModalAsync(null);
    }

    /// <inheritdoc/>
    public virtual async Task CloseModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
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

        await HandleShellNavigationAsync(() => Shell.Current.GoToAsync(navigationPath, GetIsAnimated(isAnimated), GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters)));
    }

    /// <inheritdoc/>
    public Task CloseAllModalAsync()
    {
        return CloseAllModalAsync(null);
    }

    /// <inheritdoc/>
    public virtual async Task CloseAllModalAsync(IDictionary<string, object>? navigationParameters, bool isAnimated = true)
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

        await HandleShellNavigationAsync(() => Shell.Current.GoToAsync(navigationPath, GetIsAnimated(isAnimated), GetOrCreateShellNavigationQueryParameters(NucleusMvvmCore.Current.NavigationParameters)));
    }

    /// <summary>
    /// Trigger the requested Shell logic on the UI thread, ensuring that 'IsNavigation' is reset in case
    /// an <see cref="Exception"/> occurs during the execution.
    /// </summary>
    /// <param name="shellNavigationTask">A function that returns the navigation <see cref="Task"/>.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    protected virtual async Task HandleShellNavigationAsync (Func<Task> shellNavigationTask)
    {
        if (ShouldIgnoreNavigationRequest())
        {
            return;
        }

        IsNavigating = true;

        try
        {
            await shellNavigationTask();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed handling navigation request with exception: " + ex.Message);
        }
        
        IsNavigating = false;
    }

    /// <summary>
    /// Gets the list of <see cref="Page"/> to destroy based on the <see cref="ShellNavigatedEventArgs"/> and the loaded pages
    /// before and after the navigation.
    /// </summary>
    /// <param name="e">The <see cref="ShellNavigatedEventArgs"/>.</param>
    /// <param name="pagesBeforeNavigating">The pages from the navigation stack before navigation.</param>
    /// <param name="pagesAfterNavigating">The pages from the navigation stack after navigation.</param>
    /// <returns>The list of <see cref="Page"/> to destroy.</returns>
    protected virtual IList<Page> GetPagesToDestroy(ShellNavigatedEventArgs e, IList<Page> pagesBeforeNavigating, IList<Page> pagesAfterNavigating)
    {
        var result = new List<Page>();

        if (e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot || e.Source == ShellNavigationSource.ShellItemChanged)
        {
            result = pagesBeforeNavigating!.Reverse().Where(x => !pagesAfterNavigating.Contains(x)).ToList();
        }

        return result;
    }

    /// <summary>
    /// Mark the given pages for destruction through the <see cref="NucleusMvvmPageBehavior"/>. The pages are only destroyed after
    /// the NavigationFrom event is triggered, to prevent missing this event.
    /// </summary>
    /// <param name="pages">The list of <see cref="Page"/> to destroy.</param>
    protected virtual void DestroyPages(IList<Page> pages)
    {
        foreach (var page in pages)
        {   
            var pageNucleusBehaviors = page.Behaviors?
                .Where(x => x is NucleusMvvmPageBehavior)?
                .Select(x => (NucleusMvvmPageBehavior)x)
                .ToList() ?? new List<NucleusMvvmPageBehavior>();
            
            foreach (var nucleusMvvmPageBehavior in pageNucleusBehaviors)
            {
                nucleusMvvmPageBehavior.DestroyAfterNavigatedFrom = true;
            }
        }
    }

    private async void ShellNavigating(object sender, ShellNavigatingEventArgs e)
    {
        var isCanceled = false;

        ShellNavigatingDeferral? shellNavigatingDeferralToken = null;
        
        if (e.CanCancel && (e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot || e.Source == ShellNavigationSource.Push))
        {
            var currentBindingContext = NucleusMvvmCore.Current.Shell?.CurrentPage?.BindingContext;
            var navigationDirection = default(NavigationDirection);

            switch(e.Source)
            {
                case ShellNavigationSource.Pop:
                case ShellNavigationSource.PopToRoot:
                    navigationDirection = NavigationDirection.Back;
                    break;
                case ShellNavigationSource.Push:
                    navigationDirection = NavigationDirection.Forwards;
                    break;
            }

            var confirmNavigation = currentBindingContext as IConfirmNavigation;
            if (confirmNavigation != null && !confirmNavigation.CanNavigate(navigationDirection, NucleusMvvmCore.Current.NavigationParameters))
            {
                isCanceled = true;
                e.Cancel();
            }
            else
            {
                var confirmNavigationAsync =  currentBindingContext as IConfirmNavigationAsync;
                if (confirmNavigationAsync != null)
                {
                    shellNavigatingDeferralToken = e.GetDeferral();
                    
                    var confirm = await confirmNavigationAsync.CanNavigateAsync(navigationDirection, NucleusMvvmCore.Current.NavigationParameters);
                    if (!confirm)
                    {
                        isCanceled = true;
                        e.Cancel();
                    }
                }
            }
        }

        _logger.LogInformation(isCanceled ? "Shell Navigation Canceled." : $"Shell Navigating '{e.Current?.Location}' > '{e.Target?.Location}' ({e.Source}).");

        if (!isCanceled)
        {
            _transientPagesOnNavigating = GetTransientPagesFromNavigationStack();
        }
        
        IsNavigating = !isCanceled;

        shellNavigatingDeferralToken?.Complete();
    }

    private void ShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        _logger.LogInformation($"Shell Navigated '{e.Current?.Location}' ({e.Source}).");

        var pagesAfterNavigating = GetTransientPagesFromNavigationStack();
        var pagesBeforeNavigating = new List<Page>(_transientPagesOnNavigating ?? new List<Page>());

        _transientPagesOnNavigating!.Clear();

        DestroyPages(GetPagesToDestroy(e, pagesBeforeNavigating, pagesAfterNavigating));
    
        IsNavigating = false;
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

    private IDictionary<string, object> GetOrCreateShellNavigationQueryParameters(IDictionary<string, object>? navigationParameters)
    {
        if (navigationParameters is ShellNavigationQueryParameters)
        {
            return navigationParameters;
        }

        if (_nucleusMvvmOptions.UseShellNavigationQueryParameters)
        {
            return new ShellNavigationQueryParameters(navigationParameters ?? new Dictionary<string, object>());
        }

        return navigationParameters ?? new Dictionary<string, object>();
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

    private bool GetIsAnimated(bool isAnimated)
    {
        return _nucleusMvvmOptions.AlwaysDisableNavigationAnimation ? false : isAnimated;
    }

    private IList<Page> GetTransientPagesFromNavigationStack()
    {
        var result = new List<Page>(NucleusMvvmCore.Current.Shell?.CurrentPage?.Navigation?.NavigationStack?.Skip(1) ?? new List<Page>());
        
        foreach (var modalPage in NucleusMvvmCore.Current.Shell?.CurrentPage?.Navigation?.ModalStack ?? new List<Page>())
        {
            if (modalPage.Navigation.NavigationStack.Count > 0)
            {
                result.AddRange(modalPage.Navigation.NavigationStack);
            }
            else
            {
                result.Add(modalPage);
            }
        }

        var nonTransientPageTypes = _nucleusMvvmOptions
            .ViewMappings
            .Where(x => x.RegistrationScope != ViewScope.Transient)
            .Select(x => x.ViewType);

        result = result.Where(x => x != null && !nonTransientPageTypes.Contains(x.GetType())).ToList();

        return result;
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

    private void SetIsNavigating(bool value)
    {
        if (_isNavigating == value)
        {
            return;
        }
        
        _isNavigating = value;
        
        IsNavigatingChanged?.Invoke(this, value);
        
        if (_nucleusMvvmOptions.IgnoreNavigationWhenInProgress)
        {
            _logger?.LogInformation($"IsNavigating changed to '{_isNavigating}'." + (_isNavigating ? " Incoming navigation requests will be ignored." : string.Empty));
        }
    }
}
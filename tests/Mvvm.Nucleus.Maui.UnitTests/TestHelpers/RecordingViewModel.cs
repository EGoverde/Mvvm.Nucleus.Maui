namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Base class for recording ViewModels. Records an ordered list of callback names and the
/// navigation parameters each callback received. Concrete variants implement the Nucleus
/// interfaces one-by-one, so tests can pin the exact <c>is TInterface</c> branches of
/// <see cref="NucleusMvvmPageBehavior"/> (the behavior switches on which interfaces a
/// binding context implements).
/// </summary>
public abstract class RecordingViewModel
{
    /// <summary>
    /// The ordered list of callbacks that were invoked (e.g. "OnNavigatedTo", "Init").
    /// </summary>
    public List<string> Callbacks { get; } = [];

    /// <summary>
    /// The navigation parameters received by each callback, in the same order as <see cref="Callbacks"/>.
    /// </summary>
    public List<IDictionary<string, object>> ReceivedParameters { get; } = [];

    /// <summary>
    /// The <see cref="NavigationDirection"/> values received by the confirm-navigation callbacks.
    /// </summary>
    public List<NavigationDirection> NavigationDirections { get; } = [];

    /// <summary>
    /// The value <see cref="IConfirmNavigation"/>/<see cref="IConfirmNavigationAsync"/> variants return.
    /// </summary>
    public bool AllowNavigation { get; set; } = true;

    /// <summary>
    /// Set by tests before triggering a lifecycle event: every async callback routed through
    /// the behavior's <c>RunTaskInVoidAndTrackException</c> (an <c>async void</c> the tests
    /// cannot await directly) completes this when it finishes, so tests can await it
    /// deterministically.
    /// </summary>
    public TaskCompletionSource<bool>? AsyncCallbackCompleted { get; set; }

    /// <summary>
    /// Optional gate for the async confirm-navigation callback. When set,
    /// <c>CanNavigateAsync</c> awaits this before completing, allowing tests to observe the
    /// in-flight state of the (async void) Shell navigating handler.
    /// </summary>
    public TaskCompletionSource<bool>? ConfirmNavigationAsyncGate { get; set; }

    /// <summary>
    /// The number of times a callback was invoked.
    /// </summary>
    public int Count(string callback)
    {
        return Callbacks.Count(x => x == callback);
    }

    protected void Record(string callback, IDictionary<string, object> navigationParameters)
    {
        Callbacks.Add(callback);
        ReceivedParameters.Add(navigationParameters);
    }

    protected void Record(string callback, NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters)
    {
        Callbacks.Add(callback);
        ReceivedParameters.Add(navigationParameters);
        NavigationDirections.Add(navigationDirection);
    }

    /// <summary>
    /// Records an async callback and signals <see cref="AsyncCallbackCompleted"/> (when set).
    /// </summary>
    protected Task RecordAsync(string callback, IDictionary<string, object> navigationParameters)
    {
        Record(callback, navigationParameters);
        AsyncCallbackCompleted?.TrySetResult(true);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Records <see cref="INavigatedAware"/> callbacks.
/// </summary>
public class NavigatedAwareViewModel : RecordingViewModel, INavigatedAware
{
    public void OnNavigatedTo(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedTo", navigationParameters);

    public void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedFrom", navigationParameters);

    public void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatingFrom", navigationParameters);
}

/// <summary>
/// Records <see cref="INavigatedAware"/> callbacks plus the obsolete <see cref="IInitializable"/>
/// callbacks (without implementing <see cref="IRefresh"/>/<see cref="IRefreshAsync"/>, to hit
/// the behavior's <c>else if</c> fallback branches).
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete (IInitializable is supported by design of the behavior under test).
public class InitializableViewModel : RecordingViewModel, INavigatedAware, IInitializable
{
    public void OnNavigatedTo(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedTo", navigationParameters);

    public void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedFrom", navigationParameters);

    public void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatingFrom", navigationParameters);

    public void Init(IDictionary<string, object> navigationParameters)
        => Record("Init", navigationParameters);

    public void Refresh(IDictionary<string, object> navigationParameters)
        => Record("InitializableRefresh", navigationParameters);
}
#pragma warning restore CS0618

/// <summary>
/// Records <see cref="INavigatedAware"/> callbacks plus the obsolete <see cref="IInitializableAsync"/>
/// callbacks (without implementing <see cref="IRefreshAsync"/>, to hit the behavior's
/// <c>else if</c> fallback branches).
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete (IInitializableAsync is supported by design of the behavior under test).
public class InitializableAsyncViewModel : RecordingViewModel, INavigatedAware, IInitializableAsync
{
    public void OnNavigatedTo(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedTo", navigationParameters);

    public void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedFrom", navigationParameters);

    public void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatingFrom", navigationParameters);

    public Task InitAsync(IDictionary<string, object> navigationParameters)
        => RecordAsync("InitAsync", navigationParameters);

    public Task RefreshAsync(IDictionary<string, object> navigationParameters)
        => RecordAsync("InitializableAsyncRefreshAsync", navigationParameters);
}
#pragma warning restore CS0618

/// <summary>
/// Records <see cref="INavigatedAware"/> callbacks plus <see cref="IRefreshAsync"/> only (init is
/// never implied).
/// </summary>
public class RefreshAsyncViewModel : RecordingViewModel, INavigatedAware, IRefreshAsync
{
    public void OnNavigatedTo(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedTo", navigationParameters);

    public void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedFrom", navigationParameters);

    public void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatingFrom", navigationParameters);

    public Task RefreshAsync(IDictionary<string, object> navigationParameters)
        => RecordAsync("RefreshAsync", navigationParameters);
}

/// <summary>
/// Records <see cref="IPageLifecycleAware"/> callbacks.
/// </summary>
public class PageLifecycleViewModel : RecordingViewModel, IPageLifecycleAware
{
    public void OnAppearing()
        => Callbacks.Add("OnAppearing");

    public void OnDisappearing()
        => Callbacks.Add("OnDisappearing");

    public void OnFirstAppearing()
        => Callbacks.Add("OnFirstAppearing");
}

/// <summary>
/// Records <see cref="IApplicationLifecycleAware"/> callbacks.
/// </summary>
public class ApplicationLifecycleViewModel : RecordingViewModel, IApplicationLifecycleAware
{
    public void OnResume()
        => Callbacks.Add("OnResume");

    public void OnSleep()
        => Callbacks.Add("OnSleep");
}

/// <summary>
/// Records <see cref="INavigatedAware"/> + <see cref="IDestructible"/> callbacks.
/// </summary>
public class DestructibleViewModel : RecordingViewModel, INavigatedAware, IDestructible
{
    public void OnNavigatedTo(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedTo", navigationParameters);

    public void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatedFrom", navigationParameters);

    public void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
        => Record("OnNavigatingFrom", navigationParameters);

    public void Destroy()
        => Callbacks.Add("Destroy");
}

/// <summary>
/// Records <see cref="IConfirmNavigation"/> invocations (direction + parameters).
/// </summary>
public class ConfirmNavigationViewModel : RecordingViewModel, IConfirmNavigation
{
    public bool CanNavigate(NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters)
    {
        Record("CanNavigate", navigationDirection, navigationParameters);
        return AllowNavigation;
    }
}

/// <summary>
/// Records <see cref="IConfirmNavigationAsync"/> invocations (direction + parameters).
/// </summary>
public class ConfirmNavigationAsyncViewModel : RecordingViewModel, IConfirmNavigationAsync
{
    public async Task<bool> CanNavigateAsync(NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters)
    {
        Record("CanNavigateAsync", navigationDirection, navigationParameters);

        if (ConfirmNavigationAsyncGate != null)
        {
            await ConfirmNavigationAsyncGate.Task;
        }

        return AllowNavigation;
    }
}

/// <summary>
/// Records <see cref="IPrepare"/> invocations.
/// </summary>
public class PrepareViewModel : RecordingViewModel, IPrepare
{
    public void Prepare(IDictionary<string, object> navigationParameters)
        => Record("Prepare", navigationParameters);
}

/// <summary>
/// Records <see cref="IPrepareAsync"/> invocations.
/// </summary>
public class PrepareAsyncViewModel : RecordingViewModel, IPrepareAsync
{
    public Task PrepareAsync(IDictionary<string, object> navigationParameters)
        => RecordAsync("PrepareAsync", navigationParameters);
}

using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Behaviors;

/// <summary>
/// Pins the full lifecycle matrix of the <see cref="NucleusMvvmPageBehavior"/>: first-vs-renavigation
/// callback routing (including the <see cref="IInitializable"/>, <see cref="IInitializableAsync"/> and
/// <see cref="IRefreshAsync"/> branches), the Shell double-<c>NavigatedTo</c> work-around,
/// <see cref="IPageLifecycleAware"/> first-appearing-once, <see cref="INavigatedAware"/>
/// to/from/navigating-from, <see cref="IApplicationLifecycleAware"/> subscribe/unsubscribe,
/// <see cref="IDestructible"/> + <see cref="NucleusMvvmPageBehavior.DestroyAfterNavigatedFrom"/>
/// cleanup, and navigation-parameter identity.
/// </summary>
public class NucleusMvvmPageBehaviorTests : NucleusTestHarness
{
    [Fact]
    public void NavigatedTo_First_IInitializable_CallsInitButNotRefresh()
    {
        var viewModel = new InitializableViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);

        Assert.Equal(1, viewModel.Count("Init"));
        Assert.Equal(0, viewModel.Count("InitializableRefresh"));
        Assert.Equal(1, viewModel.Count("OnNavigatedTo"));
    }

    [Fact]
    public void NavigatedTo_TwiceWithoutNavigatedFrom_SecondIsDropped()
    {
        var viewModel = new InitializableViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);

        // Shell NavigationPage + Modal double-trigger: the second NavigatedTo must be dropped.
        PageEvents.NavigatedTo(page);

        Assert.Equal(1, viewModel.Count("OnNavigatedTo"));
        Assert.Equal(1, viewModel.Count("Init"));
        Assert.Equal(0, viewModel.Count("InitializableRefresh"));
    }

    [Fact]
    public void NavigatedTo_From_To_RoutesInitOnceAndRefreshOnRenavigation()
    {
        var viewModel = new InitializableViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);
        PageEvents.NavigatedFrom(page);
        PageEvents.NavigatedTo(page);

        Assert.Equal(2, viewModel.Count("OnNavigatedTo"));
        Assert.Equal(1, viewModel.Count("OnNavigatedFrom"));
        Assert.Equal(1, viewModel.Count("Init"));
        Assert.Equal(1, viewModel.Count("InitializableRefresh"));
        Assert.Equal(
            new[] { "OnNavigatedTo", "Init", "OnNavigatedFrom", "OnNavigatedTo", "InitializableRefresh" },
            viewModel.Callbacks);
    }

    [Fact]
    public void NavigatedTo_Renavigation_IInitializableWithoutIRefresh_CallsInitializableRefresh()
    {
        // InitializableViewModel deliberately does not implement IRefresh/IRefreshAsync, so the
        // re-navigation must fall through to the IInitializable.Refresh `else if` branch.
        var viewModel = new InitializableViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);
        PageEvents.NavigatedFrom(page);
        PageEvents.NavigatedTo(page);

        Assert.Equal(1, viewModel.Count("Init"));
        Assert.Equal(1, viewModel.Count("InitializableRefresh"));
    }

    [Fact]
    public async Task NavigatedTo_From_To_IInitializableAsyncWithoutIRefreshAsync_AwaitsRefreshAsyncOnRenavigation()
    {
        var viewModel = new InitializableAsyncViewModel
        {
            AsyncCallbackCompleted = new TaskCompletionSource<bool>(),
        };
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);
        Assert.True(await TestAwaiter.CompletedWithin(viewModel.AsyncCallbackCompleted.Task));
        Assert.Equal(1, viewModel.Count("InitAsync"));
        Assert.Equal(0, viewModel.Count("InitializableAsyncRefreshAsync"));

        PageEvents.NavigatedFrom(page);

        viewModel.AsyncCallbackCompleted = new TaskCompletionSource<bool>();
        PageEvents.NavigatedTo(page);
        Assert.True(await TestAwaiter.CompletedWithin(viewModel.AsyncCallbackCompleted.Task));

        Assert.Equal(1, viewModel.Count("InitAsync"));
        Assert.Equal(1, viewModel.Count("InitializableAsyncRefreshAsync"));
    }

    [Fact]
    public async Task NavigatedTo_From_To_IRefreshAsyncOnly_CallsRefreshAsyncOnRenavigationButNeverInit()
    {
        var viewModel = new RefreshAsyncViewModel
        {
            AsyncCallbackCompleted = new TaskCompletionSource<bool>(),
        };
        var (page, _) = CreatePage(viewModel);

        // First navigation: the ViewModel is not (a)synchronous initializable, so no init is
        // implied; and IRefreshAsync is only called after initialization, so it is not called yet.
        PageEvents.NavigatedTo(page);
        Assert.Equal(0, viewModel.Count("RefreshAsync"));

        PageEvents.NavigatedFrom(page);

        viewModel.AsyncCallbackCompleted = new TaskCompletionSource<bool>();
        PageEvents.NavigatedTo(page);
        Assert.True(await TestAwaiter.CompletedWithin(viewModel.AsyncCallbackCompleted.Task));

        Assert.Equal(1, viewModel.Count("RefreshAsync"));
        Assert.Equal(0, viewModel.Count("InitAsync"));
    }

    [Fact]
    public void Appearing_TwiceDisappearing_Once_OnFirstAppearingOnlyOnce()
    {
        var viewModel = new PageLifecycleViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.Appearing(page);
        PageEvents.Appearing(page);
        PageEvents.Disappearing(page);

        Assert.Equal(2, viewModel.Count("OnAppearing"));
        Assert.Equal(1, viewModel.Count("OnFirstAppearing"));
        Assert.Equal(1, viewModel.Count("OnDisappearing"));
    }

    [Fact]
    public void NavigatingFrom_RoutesOnNavigatingFrom()
    {
        var viewModel = new NavigatedAwareViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatingFrom(page);

        Assert.Equal(1, viewModel.Count("OnNavigatingFrom"));
        Assert.Equal(0, viewModel.Count("OnNavigatedTo"));
        Assert.Equal(0, viewModel.Count("OnNavigatedFrom"));
    }

    [Fact]
    public void AfterFirstNavigatedTo_AppLifecycleEventsRouteToViewModel()
    {
        var viewModel = new ApplicationLifecycleViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);

        CoreEvents.AppResumed(Core);
        CoreEvents.AppStopped(Core);

        Assert.Equal(1, viewModel.Count("OnResume"));
        Assert.Equal(1, viewModel.Count("OnSleep"));
    }

    [Fact]
    public void AfterNavigatedFrom_AppLifecycleEventsNoLongerRoute()
    {
        var viewModel = new ApplicationLifecycleViewModel();
        var (page, _) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);
        CoreEvents.AppResumed(Core);
        Assert.Equal(1, viewModel.Count("OnResume"));

        PageEvents.NavigatedFrom(page);
        CoreEvents.AppResumed(Core);
        CoreEvents.AppStopped(Core);

        // The unsubscribe is pinned by the app-lifecycle callbacks no longer reaching the ViewModel:
        // OnResume stays at 1 (the pre-From raise) and OnSleep stays at 0 (the post-From raise dropped).
        Assert.Equal(1, viewModel.Count("OnResume")); // AppResumed must not route to the ViewModel after NavigatedFrom (unsubscribed).
        Assert.Equal(0, viewModel.Count("OnSleep"));
    }

    [Fact]
    public void NavigatedFrom_DestroyAfterNavigatedFrom_CallsDestroyAndDeconstructsPageByDefault()
    {
        var viewModel = new DestructibleViewModel();
        var (page, behavior) = CreatePage(viewModel);
        behavior.DestroyAfterNavigatedFrom = true;

        PageEvents.NavigatedTo(page);
        PageEvents.NavigatedFrom(page);

        Assert.Equal(1, viewModel.Count("OnNavigatedFrom"));
        Assert.Equal(1, viewModel.Count("Destroy"));
        Assert.DoesNotContain(page.Behaviors, x => x == behavior);
        Assert.Null(page.BindingContext);
    }

    [Fact]
    public void NavigatedFrom_DestroyAfterNavigatedFrom_WithoutDeconstruct_CallsDestroyButKeepsPageIntact()
    {
        Options.UseDeconstructPageOnDestroy = false;

        var viewModel = new DestructibleViewModel();
        var (page, behavior) = CreatePage(viewModel);
        behavior.DestroyAfterNavigatedFrom = true;

        PageEvents.NavigatedTo(page);
        PageEvents.NavigatedFrom(page);

        Assert.Equal(1, viewModel.Count("Destroy"));
        Assert.Contains(page.Behaviors, x => x == behavior);
        Assert.Same(viewModel, page.BindingContext);
    }

    [Fact]
    public void NavigatedFrom_WithoutDestroyFlag_DoesNotDestroy()
    {
        var viewModel = new DestructibleViewModel();
        var (page, behavior) = CreatePage(viewModel);

        PageEvents.NavigatedTo(page);
        PageEvents.NavigatedFrom(page);

        Assert.Equal(1, viewModel.Count("OnNavigatedFrom"));
        Assert.Equal(0, viewModel.Count("Destroy"));
        Assert.Contains(page.Behaviors, x => x == behavior);
        Assert.Same(viewModel, page.BindingContext);
    }

    [Fact]
    public void Callbacks_ReceiveTheExactNavigationParametersDictionarySetOnCore()
    {
        var viewModel = new NavigatedAwareViewModel();
        var (page, _) = CreatePage(viewModel);

        var parameters = new Dictionary<string, object> { ["Sentinel"] = "value" };
        Core.NavigationParameters = parameters;

        PageEvents.NavigatedTo(page);
        Assert.Same(parameters, Assert.Single(viewModel.ReceivedParameters));
        Assert.Equal("value", parameters["Sentinel"]);

        PageEvents.NavigatedFrom(page);
        Assert.Same(parameters, viewModel.ReceivedParameters[^1]);
    }
}

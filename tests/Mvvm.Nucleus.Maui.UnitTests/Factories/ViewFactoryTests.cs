using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Factories;

/// <summary>
/// Pins the <see cref="ViewFactory"/> creation lane (<c>ConfigureView</c>): ViewModel binding from
/// the registered <see cref="ViewMapping"/>, <c>IPrepare(Async)</c> invocation with the current
/// navigation parameters, idempotent <see cref="NucleusMvvmPageBehavior"/> attachment,
/// <c>WrapInNavigationPage</c> and <c>NavigatingPresentationMode</c> handling, and behavior
/// attachment for non-Page elements through their <c>Parent</c>.
/// </summary>
public class ViewFactoryTests
{
    [Fact]
    public void CreateView_PageWithMappingAndNoBindingContext_BindsMappedViewModel()
    {
        var fixture = new ViewFactoryFixture(
            registerTypes: x => x.RegisterView<PlainPage, PlainPageViewModel>("PlainPage", ServiceLifetime.Transient),
            registerServices: x => x.AddTransient<PlainPageViewModel>());

        var result = fixture.Factory.CreateView(typeof(PlainPage));

        var page = Assert.IsType<PlainPage>(result);
        Assert.IsType<PlainPageViewModel>(page.BindingContext);
    }

    [Fact]
    public void ConfigureView_Page_AttachesExactlyOneBehavior_AndIsIdempotent()
    {
        var fixture = new ViewFactoryFixture();
        var page = new PlainPage();

        var first = fixture.Factory.ConfigureView(page);
        var second = fixture.Factory.ConfigureView(page);

        Assert.Same(page, first);
        Assert.Same(page, second);

        var behavior = Assert.Single(page.Behaviors.OfType<NucleusMvvmPageBehavior>());
        Assert.Same(page, behavior.Page);
        Assert.Null(behavior.Element);
    }

    [Fact]
    public void ConfigureView_IPrepareOnElementAndViewModel_BothReceiveNavigationParameters()
    {
        var fixture = new ViewFactoryFixture();
        fixture.Core.NavigationParameters["Sentinel"] = "value";

        var viewModel = new PrepareViewModel();
        var page = new PreparePage { BindingContext = viewModel };

        fixture.Factory.ConfigureView(page);

        var pageParameters = Assert.Single(page.PreparedParameters);
        var viewModelParameters = Assert.Single(viewModel.ReceivedParameters);
        Assert.Equal("value", pageParameters["Sentinel"]);
        Assert.Equal("value", viewModelParameters["Sentinel"]);
    }

    [Fact]
    public async Task ConfigureView_IPrepareAsyncOnElementAndViewModel_Completes()
    {
        var fixture = new ViewFactoryFixture();

        var viewModel = new PrepareAsyncViewModel
        {
            AsyncCallbackCompleted = new TaskCompletionSource<bool>(),
        };
        var page = new PreparePageAsync
        {
            BindingContext = viewModel,
            Completed = new TaskCompletionSource<bool>(),
        };

        fixture.Factory.ConfigureView(page);

        Assert.True(await TestAwaiter.CompletedWithin(page.Completed.Task));
        Assert.True(await TestAwaiter.CompletedWithin(viewModel.AsyncCallbackCompleted.Task));
        Assert.Equal(1, viewModel.Count("PrepareAsync"));
    }

    [Fact]
    public void ConfigureView_WrapInNavigationPage_ReturnsNavigationPageWrappingThePage()
    {
        var fixture = new ViewFactoryFixture();
        fixture.Core.NavigationParameters[NucleusNavigationParameters.WrapInNavigationPage] = true;

        var page = new PlainPage();
        var result = fixture.Factory.ConfigureView(page);

        var navigationPage = Assert.IsType<NavigationPage>(result);
        Assert.Same(page, navigationPage.RootPage);
    }

    [Fact]
    public void ConfigureView_NavigatingPresentationMode_SetsPresentationModeOnThePage()
    {
        var fixture = new ViewFactoryFixture();
        fixture.Core.NavigationParameters[NucleusNavigationParameters.NavigatingPresentationMode] = PresentationMode.Modal;

        var page = new PlainPage();
        fixture.Factory.ConfigureView(page);

        Assert.Equal(PresentationMode.Modal, Shell.GetPresentationMode(page));
    }

    [Fact]
    public void ConfigureView_WrapAndPresentationMode_AppliesToWrapperAndPage()
    {
        var fixture = new ViewFactoryFixture();
        fixture.Core.NavigationParameters[NucleusNavigationParameters.WrapInNavigationPage] = true;
        fixture.Core.NavigationParameters[NucleusNavigationParameters.NavigatingPresentationMode] = PresentationMode.Modal;

        var page = new PlainPage();
        var result = fixture.Factory.ConfigureView(page);

        var navigationPage = Assert.IsType<NavigationPage>(result);
        Assert.Same(page, navigationPage.RootPage);
        Assert.Equal(PresentationMode.Modal, Shell.GetPresentationMode(page));
        Assert.Equal(PresentationMode.Modal, Shell.GetPresentationMode(navigationPage));
    }

    [Fact]
    public void ConfigureView_NonPageElement_AttachesBehaviorToParentPageWhenParented()
    {
        var fixture = new ViewFactoryFixture();
        var child = new Label { Text = "child" };

        var result = fixture.Factory.ConfigureView(child);
        Assert.Same(child, result);

        var page = new ContentPage();
        child.Parent = page;

        var behavior = Assert.Single(page.Behaviors.OfType<NucleusMvvmPageBehavior>());
        Assert.Same(page, behavior.Page);
        Assert.Same(child, behavior.Element);
    }

    /// <summary>
    /// A page without its own behavior, used to register a View/ViewModel mapping.
    /// </summary>
    private sealed class PlainPage : ContentPage
    {
    }

    /// <summary>
    /// The ViewModel registered for <see cref="PlainPage"/>.
    /// </summary>
    private sealed class PlainPageViewModel
    {
    }

    /// <summary>
    /// A page that records the parameters it received from <see cref="IPrepare.Prepare"/>.
    /// </summary>
    private sealed class PreparePage : ContentPage, IPrepare
    {
        public List<IDictionary<string, object>> PreparedParameters { get; } = [];

        public void Prepare(IDictionary<string, object> navigationParameters)
        {
            PreparedParameters.Add(navigationParameters);
        }
    }

    /// <summary>
    /// A page that records <see cref="IPrepareAsync.PrepareAsync"/> completion.
    /// </summary>
    private sealed class PreparePageAsync : ContentPage, IPrepareAsync
    {
        public TaskCompletionSource<bool>? Completed { get; set; }

        public Task PrepareAsync(IDictionary<string, object> navigationParameters)
        {
            Completed?.TrySetResult(true);
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// A core + <see cref="ViewFactory"/> fixture with a real <see cref="ServiceCollection"/>
/// (<c>ActivatorUtilities</c> resolution), the <c>InternalsVisibleTo</c> seam for initialization,
/// and direct access to the internal <c>DependencyOptions</c> for registering a
/// <see cref="ViewMapping"/>.
/// </summary>
internal sealed class ViewFactoryFixture
{
    public ViewFactoryFixture(
        Action<DependencyOptions>? registerTypes = null,
        Action<NucleusMvvmOptions>? configure = null,
        Action<ServiceCollection>? registerServices = null)
    {
        Options = new NucleusMvvmOptions();
        configure?.Invoke(Options);
        registerTypes?.Invoke(Options.DependencyOptions);

        var services = new ServiceCollection();
        registerServices?.Invoke(services);
        var provider = services.BuildServiceProvider();

        Factory = new ViewFactory(NullLogger<ViewFactory>.Instance, provider, Options);

        Core = new NucleusMvvmCore(new Application(), Factory, Options, NullLogger<NucleusMvvmCore>.Instance);
        Core.Initialize(provider);
        Core.Shell = new Shell();
    }

    public NucleusMvvmCore Core { get; }

    public ViewFactory Factory { get; }

    public NucleusMvvmOptions Options { get; }
}

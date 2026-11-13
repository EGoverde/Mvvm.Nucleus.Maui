using Microsoft.Extensions.Logging;
using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;
using CompatibilityNavigationParameters = Mvvm.Nucleus.Maui.Compatibility.CompatibilityNavigationParameters;
using NavigationMode = Mvvm.Nucleus.Maui.Compatibility.NavigationMode; // Ambiguous with Microsoft.Maui.ApplicationModel.NavigationMode (global using).

namespace Mvvm.Nucleus.Maui.UnitTests.Services;

/// <summary>
/// Pins the <see cref="NavigationService"/> confirm-navigation handler: the
/// <see cref="IConfirmNavigation"/> deny/allow branches (sync), the source-to-direction mapping,
/// the <see cref="NucleusMvvmOptions.UseConfirmNavigationForAllNavigationRequests"/> guard, and the
/// <see cref="IConfirmNavigationAsync"/> deny branch through <c>GetDeferral()</c> (both proven to
/// work in the bare host, so the plan's skip-fallback was not needed).
///
/// Each test registers a fresh <see cref="Shell"/> whose <c>CurrentPage.BindingContext</c> is the
/// recording ViewModel (the source the handler reads), then raises <c>Shell.Navigating</c> with a
/// constructed <see cref="ShellNavigatingEventArgs"/>.
/// </summary>
public class NavigationServiceConfirmNavigationTests : NucleusTestHarness
{
    private const string CurrentLocation = "app://root/";

    private const string TargetLocation = "app://root/details";

    [Fact]
    public void Navigating_Pop_IConfirmNavigationDenying_NavigationIsCanceled()
    {
        var viewModel = new ConfirmNavigationViewModel { AllowNavigation = false };
        var (_, service, logger) = CreateShellWithConfirmViewModel(viewModel);

        var eventArgs = ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, ShellNavigationSource.Pop);

        Assert.True(eventArgs.Cancelled);
        Assert.False(service.IsNavigating);
        Assert.True(logger.HasEntry(LogLevel.Information, "Shell Navigation Canceled."));
        Assert.Equal(1, viewModel.Count("CanNavigate"));
        Assert.Equal(NavigationDirection.Back, viewModel.NavigationDirections[^1]);
        Assert.Equal(ShellNavigationSource.Pop, Core.NavigationParameters[NucleusNavigationParameters.ShellNavigationSource]);
        Assert.Equal(NavigationMode.Back, Core.NavigationParameters[CompatibilityNavigationParameters.NavigationMode]);
    }

    [Fact]
    public void Navigating_Pop_IConfirmNavigationAllowing_NavigationProceeds()
    {
        var viewModel = new ConfirmNavigationViewModel { AllowNavigation = true };
        var (_, service, logger) = CreateShellWithConfirmViewModel(viewModel);

        var eventArgs = ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, ShellNavigationSource.Pop);

        Assert.False(eventArgs.Cancelled);
        Assert.True(service.IsNavigating);
        Assert.True(logger.HasEntryContaining(LogLevel.Information, "Shell Navigating"));
        Assert.False(logger.HasEntry(LogLevel.Information, "Shell Navigation Canceled."));
        Assert.Equal(NavigationDirection.Back, viewModel.NavigationDirections[^1]);
    }

    [Theory]
    [InlineData(ShellNavigationSource.Pop, NavigationDirection.Back)]
    [InlineData(ShellNavigationSource.PopToRoot, NavigationDirection.Back)]
    [InlineData(ShellNavigationSource.Push, NavigationDirection.Forwards)]
    public void Navigating_SourceMapping_DirectionIsMappedFromSource(ShellNavigationSource source, NavigationDirection expectedDirection)
    {
        var viewModel = new ConfirmNavigationViewModel { AllowNavigation = true };
        var (_, _, _) = CreateShellWithConfirmViewModel(viewModel);

        ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, source);

        Assert.Equal(1, viewModel.Count("CanNavigate"));
        Assert.Equal(expectedDirection, viewModel.NavigationDirections[^1]);
    }

    [Fact]
    public void Navigating_ShellItemChanged_DefaultOptions_CanNavigateNeverCalled()
    {
        // UseConfirmNavigationForAllNavigationRequests defaults to false: a source that is neither
        // Push, Pop, nor PopToRoot must skip confirmation entirely.
        var viewModel = new ConfirmNavigationViewModel { AllowNavigation = true };
        var (_, _, _) = CreateShellWithConfirmViewModel(viewModel);

        var eventArgs = ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, ShellNavigationSource.ShellItemChanged);

        Assert.Equal(0, viewModel.Count("CanNavigate"));
        Assert.False(eventArgs.Cancelled);
    }

    [Fact]
    public void Navigating_ShellItemChanged_UseConfirmNavigationForAllNavigationRequests_CanNavigateCalledWithOther()
    {
        Options.UseConfirmNavigationForAllNavigationRequests = true;

        var viewModel = new ConfirmNavigationViewModel { AllowNavigation = true };
        var (_, _, _) = CreateShellWithConfirmViewModel(viewModel);

        var eventArgs = ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, ShellNavigationSource.ShellItemChanged);

        Assert.Equal(1, viewModel.Count("CanNavigate"));
        Assert.Equal(NavigationDirection.Other, viewModel.NavigationDirections[^1]);
        Assert.False(eventArgs.Cancelled);
    }

    [Fact]
    public async Task Navigating_Pop_IConfirmNavigationAsyncDenying_NavigationIsCanceledAndDeferralCompletes()
    {
        var viewModel = new ConfirmNavigationAsyncViewModel
        {
            AllowNavigation = false,
            ConfirmNavigationAsyncGate = new TaskCompletionSource<bool>(),
        };
        var (_, service, logger) = CreateShellWithConfirmViewModel(viewModel);

        var eventArgs = ShellNavigatingEventArgsBuilder.RaiseNavigating(Core.Shell!, CurrentLocation, TargetLocation, ShellNavigationSource.Pop);

        // The (async void) handler suspended on the deferral path: not canceled yet, but the
        // direction was already recorded synchronously.
        Assert.False(eventArgs.Cancelled);
        Assert.Equal(NavigationDirection.Back, viewModel.NavigationDirections[^1]);

        viewModel.ConfirmNavigationAsyncGate!.TrySetResult(true);

        Assert.True(await TestAwaiter.CompletedWithin(() => eventArgs.Cancelled));
        Assert.False(service.IsNavigating);
        Assert.True(logger.HasEntry(LogLevel.Information, "Shell Navigation Canceled."));
    }

    private (Page Page, NavigationService Service, RecordingLogger<NavigationService> Logger) CreateShellWithConfirmViewModel(RecordingViewModel viewModel)
    {
        var page = new ContentPage { BindingContext = viewModel };
        CreateShellWithCurrentPage(page);

        var logger = new RecordingLogger<NavigationService>();
        var service = new NavigationService(Options, logger);

        return (page, service, logger);
    }
}

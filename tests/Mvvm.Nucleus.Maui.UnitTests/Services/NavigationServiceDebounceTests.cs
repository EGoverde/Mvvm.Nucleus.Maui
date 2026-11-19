using Microsoft.Extensions.Logging;
using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Services;

/// <summary>
/// Pins the debounce matrix of <c>NavigationService.SetIsNavigatingOrIgnoreRequest</c>: the
/// <see cref="NucleusNavigationParameters.DoNotIgnoreThisNavigationRequest"/> bypass, the
/// in-progress guard (default options), the time-window guard, and the options-off fast path.
/// The shared static <c>NavigationService._isNavigatingLastTriggeredUtc</c> is reset before each
/// test through <see cref="NavigationServiceStateReset"/>, so the matrix is deterministic and
/// order-independent (no sleeps).
///
/// In addition to the direct probe, the bypass is also verified through the public entry points
/// (<c>NavigateToRouteAsync</c>, <c>NavigateAsync&lt;TView&gt;</c>, <c>NavigateBackAsync</c>) with the
/// Shell navigation stubbed, pinning that the caller's parameter dictionary is actually forwarded
/// to the check (a past bug dropped it at every call site, making the bypass dead code).
/// </summary>
public class NavigationServiceDebounceTests : NucleusTestHarness
{
    [Fact]
    public void SetIsNavigatingOrIgnoreRequest_BypassParameter_AllowedEvenWhileInProgress()
    {
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestNavigationService(Options, logger);

        Assert.True(service.Probe());

        var bypass = new Dictionary<string, object>
        {
            [NucleusNavigationParameters.DoNotIgnoreThisNavigationRequest] = true,
        };
        Assert.True(service.Probe(bypass));

        Assert.DoesNotContain(logger.Entries, x => x.Level == LogLevel.Warning);
        Assert.True(service.IsNavigating);
    }

    [Fact]
    public void SetIsNavigatingOrIgnoreRequest_Defaults_SecondRequestWhileInProgressIsIgnored()
    {
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestNavigationService(Options, logger);
        var isNavigatingChanges = new List<bool>();
        service.IsNavigatingChanged += (s, value) => isNavigatingChanges.Add(value);

        Assert.True(service.Probe());
        Assert.False(service.Probe());

        Assert.Equal(new[] { true }, isNavigatingChanges);
        Assert.True(service.IsNavigating);
        Assert.True(logger.HasEntry(LogLevel.Warning,
            "Ignoring this navigation request as another request is already in progress. " +
            "You can change this value in NucleusMvvmOptions.IgnoreNavigationWhenInProgress."));
    }

    [Fact]
    public void SetIsNavigatingOrIgnoreRequest_TimeWindow_SecondRequestWithinWindowIsIgnored()
    {
        Options.IgnoreNavigationWhenInProgress = false;
        Options.IgnoreNavigationWithinMilliseconds = 100;
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestNavigationService(Options, logger);

        Assert.True(service.Probe());
        Assert.False(service.Probe());

        Assert.True(logger.HasEntry(LogLevel.Warning,
            "Ignoring this navigation request as it is within 100ms since the previous one. " +
            "You can change this value in NucleusMvvmOptions.IgnoreNavigationWithinMilliseconds."));
    }

    [Fact]
    public void SetIsNavigatingOrIgnoreRequest_OptionsOff_ConsecutiveRequestsAllAllowed()
    {
        Options.IgnoreNavigationWhenInProgress = false;
        Options.IgnoreNavigationWithinMilliseconds = 0;
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestNavigationService(Options, logger);

        Assert.True(service.Probe());
        Assert.True(service.Probe());
        Assert.True(service.Probe());

        Assert.DoesNotContain(logger.Entries, x => x.Level == LogLevel.Warning);
        Assert.True(service.IsNavigating);
    }

    [Fact]
    public async Task PublicEntryPoints_BypassParameter_AllowedEvenWhileInProgress()
    {
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        Options.DependencyOptions.RegisterView<ContentPage, object>(route: "TestPage");
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestableNavigationService(Options, logger);

        var bypass = new Dictionary<string, object>
        {
            [NucleusNavigationParameters.DoNotIgnoreThisNavigationRequest] = true,
        };

        // Three back-to-back requests while the first is still 'in progress' (the stubbed Shell
        // handler leaves IsNavigating true): every one must be processed per the documented
        // DoNotIgnoreThisNavigationRequest bypass — including through the generic entry point,
        // which delegates to the virtual NavigateAsync(Type, ...).
        await service.NavigateToRouteAsync("//TabBar/Tab1", bypass, isAnimated: false);
        await service.NavigateAsync<ContentPage>(bypass, isAnimated: false);
        await service.NavigateBackAsync(bypass, isAnimated: false);

        Assert.Equal(3, service.HandledNavigationCount);
        Assert.True(service.IsNavigating);
        Assert.DoesNotContain(logger.Entries, x => x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task PublicEntryPoints_Defaults_SecondRequestWhileInProgressIsIgnored()
    {
        NavigationServiceStateReset.ResetLastTriggeredUtc();
        var logger = new RecordingLogger<NavigationService>();
        var service = new TestableNavigationService(Options, logger);

        await service.NavigateToRouteAsync("//TabBar/Tab1");
        await service.NavigateToRouteAsync("//TabBar/Tab2");

        Assert.Equal(1, service.HandledNavigationCount);
        Assert.True(service.IsNavigating);
        Assert.True(logger.HasEntry(LogLevel.Warning,
            "Ignoring this navigation request as another request is already in progress. " +
            "You can change this value in NucleusMvvmOptions.IgnoreNavigationWhenInProgress."));
    }

    /// <summary>
    /// Exposes the <c>protected virtual</c> debounce probe for direct testing (no library change;
    /// see the plan's rejected 'refactor for testability' alternative).
    /// </summary>
    private sealed class TestNavigationService : NavigationService
    {
        public TestNavigationService(NucleusMvvmOptions options, ILogger<NavigationService> logger)
            : base(options, logger)
        {
        }

        public bool Probe(IDictionary<string, object>? navigationParameters = null)
            => SetIsNavigatingOrIgnoreRequest(navigationParameters);
    }

    /// <summary>
    /// Drives the public navigation entry points with the Shell navigation stubbed out
    /// (<see cref="NavigationService.HandleShellNavigationAsync"/> overridden), so the
    /// debounce/bypass gating at the call sites can be observed without a live <see cref="Shell"/>.
    /// Because the stub never resets <c>IsNavigating</c>, the service stays 'in progress' across
    /// requests — the exact condition the <see cref="NucleusNavigationParameters.DoNotIgnoreThisNavigationRequest"/>
    /// bypass exists for.
    /// </summary>
    private sealed class TestableNavigationService : NavigationService
    {
        public int HandledNavigationCount { get; private set; }

        public TestableNavigationService(NucleusMvvmOptions options, ILogger<NavigationService> logger)
            : base(options, logger)
        {
        }

        protected override Task HandleShellNavigationAsync(Func<Task> shellNavigationTask)
        {
            HandledNavigationCount++;
            return Task.CompletedTask;
        }
    }
}

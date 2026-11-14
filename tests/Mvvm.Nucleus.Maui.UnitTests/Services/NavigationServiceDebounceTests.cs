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
}

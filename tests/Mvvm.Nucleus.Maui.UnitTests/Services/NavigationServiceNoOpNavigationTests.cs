using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Services;

/// <summary>
/// Pins the no-op navigation behavior of <see cref="NavigationService"/>: a navigation that
/// targets the location that is already current (e.g. an automation driver issuing 'go to tab X'
/// while the app is already on X) fires <c>Shell.Navigating</c> but never fires
/// <c>Shell.Navigated</c>. <see cref="NavigationService.IsNavigating"/> must therefore stay
/// false, otherwise it would be stuck.
/// </summary>
public class NavigationServiceNoOpNavigationTests : NucleusTestHarness
{
    private const string CurrentLocation = "app://root/";

    private const string OtherLocation = "app://root/details";

    [Fact]
    public void Navigating_NoOpToCurrentLocation_IsNavigatingStaysFalse()
    {
        var service = new NavigationService(Options, new RecordingLogger<NavigationService>());

        ShellNavigatingEventArgsBuilder.RaiseNavigating(Shell, CurrentLocation, CurrentLocation, ShellNavigationSource.ShellItemChanged);

        Assert.False(service.IsNavigating);
    }

    [Fact]
    public void Navigating_NoOpToCurrentLocation_IsNavigatingChangedNotRaised()
    {
        var service = new NavigationService(Options, new RecordingLogger<NavigationService>());

        var changedValues = new List<bool>();
        service.IsNavigatingChanged += (_, value) => changedValues.Add(value);

        ShellNavigatingEventArgsBuilder.RaiseNavigating(Shell, CurrentLocation, CurrentLocation, ShellNavigationSource.ShellItemChanged);

        Assert.Empty(changedValues);
    }

    [Fact]
    public void Navigating_RealNavigation_IsNavigatingTrueUntilNavigatedFires()
    {
        var service = new NavigationService(Options, new RecordingLogger<NavigationService>());

        ShellNavigatingEventArgsBuilder.RaiseNavigating(Shell, CurrentLocation, OtherLocation, ShellNavigationSource.Push);
        Assert.True(service.IsNavigating);

        RaiseNavigated(OtherLocation);
        Assert.False(service.IsNavigating);
    }

    /// <summary>
    /// Raises <see cref="Shell.Navigated"/> on the harness shell (the delegate is invoked via
    /// reflection, mirroring <see cref="ShellNavigatingEventArgsBuilder.RaiseNavigating"/>).
    /// </summary>
    private void RaiseNavigated(string location)
    {
        var handler = Shell
            .GetEventDelegate(nameof(Shell.Navigated))
            ?? throw new InvalidOperationException("No delegate registered for the 'Navigated' event; is NucleusMvvmCore (or a NavigationService) subscribed?");

        var eventArgs = new ShellNavigatedEventArgs(new ShellNavigationState(CurrentLocation), new ShellNavigationState(location), ShellNavigationSource.Push);
        handler.DynamicInvoke(Shell, eventArgs);
    }
}

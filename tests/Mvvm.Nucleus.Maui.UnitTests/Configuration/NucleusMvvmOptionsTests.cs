using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Configuration;

/// <summary>
/// Pins the documented defaults of <see cref="NucleusMvvmOptions"/> — these drive the debounce,
/// confirm-navigation and deconstruction behavior that the rest of the suite exercises.
/// </summary>
public class NucleusMvvmOptionsTests
{
    [Fact]
    public void Defaults_MatchDocumentedValues()
    {
        var options = new NucleusMvvmOptions();

        Assert.True(options.AddQueryParametersToDictionary);
        Assert.False(options.AlwaysDisableNavigationAnimation);
        Assert.True(options.IgnoreNavigationWhenInProgress);
        Assert.Equal(250, options.IgnoreNavigationWithinMilliseconds);
        Assert.True(options.UseDeconstructPageOnDestroy);
        Assert.True(options.UseDeconstructPopupOnDestroy);
        Assert.True(options.UseShellNavigationQueryParameters);
        Assert.False(options.UseConfirmNavigationForAllNavigationRequests);
        Assert.True(options.UseCommunityToolkitPopupServiceCompatibility);
        Assert.True(options.UseAlternativePopupOpenedAndClosedEvents);
#pragma warning disable CS0618 // Type or member is obsolete (pinned on purpose: this obsolete default is part of the 0.8.x contract).
        Assert.Null(options.CommunityToolkitV1PopupServicePopupOptions);
#pragma warning restore CS0618

        Assert.Empty(options.ViewMappings);
        Assert.Empty(options.PopupMappings);
    }
}

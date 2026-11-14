using Mvvm.Nucleus.Maui.Compatibility;
using Xunit;
using NavigationMode = Mvvm.Nucleus.Maui.Compatibility.NavigationMode; // Ambiguous with Microsoft.Maui.ApplicationModel.NavigationMode (global using).

namespace Mvvm.Nucleus.Maui.UnitTests.Compatibility;

/// <summary>
/// Pins the Prism compatibility helpers: <see cref="CompatibilityExtensions.IsBackNavigation"/> and
/// <see cref="CompatibilityExtensions.GetNavigationMode"/> read the <see cref="NavigationMode"/> value that
/// <see cref="NavigationService"/> writes to the navigation parameters during Shell navigation.
/// </summary>
public class CompatibilityExtensionsTests
{
    [Fact]
    public void IsBackNavigation_WhenStoredModeBack_ReturnsTrue()
    {
        var parameters = new Dictionary<string, object>
        {
            [CompatibilityNavigationParameters.NavigationMode] = NavigationMode.Back,
        };

        Assert.True(parameters.IsBackNavigation());
    }

    [Fact]
    public void IsBackNavigation_WhenStoredModeNew_ReturnsFalse()
    {
        var parameters = new Dictionary<string, object>
        {
            [CompatibilityNavigationParameters.NavigationMode] = NavigationMode.New,
        };

        Assert.False(parameters.IsBackNavigation());
    }

    [Fact]
    public void IsBackNavigation_WhenKeyMissing_ReturnsFalse()
    {
        var parameters = new Dictionary<string, object>();

        Assert.False(parameters.IsBackNavigation());
    }

    [Fact]
    public void GetNavigationMode_WhenStored_ReturnsStoredValue()
    {
        var parameters = new Dictionary<string, object>
        {
            [CompatibilityNavigationParameters.NavigationMode] = NavigationMode.Back,
        };

        Assert.Equal(NavigationMode.Back, parameters.GetNavigationMode());
    }

    [Fact]
    public void GetNavigationMode_WhenKeyMissingOrWrongType_DefaultsToNew()
    {
        var missing = new Dictionary<string, object>();
        Assert.Equal(NavigationMode.New, missing.GetNavigationMode());

        var wrongType = new Dictionary<string, object>
        {
            [CompatibilityNavigationParameters.NavigationMode] = "Back",
        };

        Assert.Equal(NavigationMode.New, wrongType.GetNavigationMode());
    }
}

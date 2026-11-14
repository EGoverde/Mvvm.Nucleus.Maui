using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Extensions;

/// <summary>
/// Pins the null-safe, type-safe dictionary access helpers used throughout Nucleus for reading
/// navigation parameters.
/// </summary>
public class DictionaryExtensionsTests
{
    [Fact]
    public void GetValueOrDefault_PresentAndCorrectType_ReturnsValue()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["Key"] = "value",
        };

        Assert.Equal("value", dictionary.GetValueOrDefault<string>("Key"));
    }

    [Fact]
    public void GetValueOrDefault_MissingKey_WithFallback_ReturnsFallback()
    {
        var dictionary = new Dictionary<string, object>();

        Assert.Equal("fallback", dictionary.GetValueOrDefault("Key", "fallback"));
    }

    [Fact]
    public void GetValueOrDefault_MissingKey_WithoutFallback_ReturnsNull()
    {
        var dictionary = new Dictionary<string, object>();

        Assert.Null(dictionary.GetValueOrDefault<string>("Key"));
    }

    [Fact]
    public void GetValueOrDefault_PresentButWrongType_ReturnsFallback()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["Key"] = 42,
        };

        Assert.Equal("fallback", dictionary.GetValueOrDefault("Key", "fallback"));
        Assert.Null(dictionary.GetValueOrDefault<string>("Key"));
    }

    [Fact]
    public void GetStructOrDefault_PresentAndCorrectType_ReturnsValue()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["Key"] = 42,
        };

        Assert.Equal(42, dictionary.GetStructOrDefault("Key", 0));
    }

    [Fact]
    public void GetStructOrDefault_MissingKey_ReturnsFallback()
    {
        var dictionary = new Dictionary<string, object>();

        Assert.Equal(42, dictionary.GetStructOrDefault("Key", 42));
    }

    [Fact]
    public void GetStructOrDefault_PresentButWrongType_ReturnsFallback()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["Key"] = "not an int",
        };

        Assert.Equal(42, dictionary.GetStructOrDefault("Key", 42));
    }

    [Fact]
    public void GetShellNavigationSource_Present_ReturnsStoredSource()
    {
        var dictionary = new Dictionary<string, object>
        {
            [NucleusNavigationParameters.ShellNavigationSource] = ShellNavigationSource.Pop,
        };

        Assert.Equal(ShellNavigationSource.Pop, dictionary.GetShellNavigationSource());
    }

    [Fact]
    public void GetShellNavigationSource_MissingKey_ReturnsUnknown()
    {
        var dictionary = new Dictionary<string, object>();

        Assert.Equal(ShellNavigationSource.Unknown, dictionary.GetShellNavigationSource());
    }

    [Fact]
    public void GetShellNavigationSource_PresentButWrongType_ReturnsUnknown()
    {
        var dictionary = new Dictionary<string, object>
        {
            [NucleusNavigationParameters.ShellNavigationSource] = "Pop",
        };

        Assert.Equal(ShellNavigationSource.Unknown, dictionary.GetShellNavigationSource());
    }
}

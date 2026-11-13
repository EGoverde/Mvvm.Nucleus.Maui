using Mvvm.Nucleus.Maui.Compatibility;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Compatibility;

/// <summary>
/// Pins the Prism compatibility contract of <see cref="NavigationParameters"/>: accessing a missing
/// key returns <c>null</c> instead of throwing <see cref="KeyNotFoundException"/>.
/// </summary>
public class NavigationParametersTests
{
    [Fact]
    public void Indexer_MissingKey_ReturnsNull_DoesNotThrow()
    {
        // Access through a NavigationParameters-typed reference so the `new` indexer wins over Dictionary.this[].
        var parameters = new NavigationParameters();

        Assert.Null(parameters["missing"]);
    }

    [Fact]
    public void Indexer_PresentKey_ReturnsStoredValue()
    {
        var parameters = new NavigationParameters();

        // Assign through the Dictionary base type: the `new` indexer on NavigationParameters is read-only,
        // so the collection-initializer form would resolve to it and fail to compile.
        ((Dictionary<string, object>)parameters)["Key"] = "Value";

        Assert.Equal("Value", parameters["Key"]);
    }

    [Fact]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        var parameters = new NavigationParameters();

        Assert.False(parameters.TryGetValue("missing", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void Ctor_Idictionary_CopiesEntries()
    {
        var source = new Dictionary<string, object>
        {
            ["First"] = 1,
            ["Second"] = "two",
        };

        var parameters = new NavigationParameters(source);

        Assert.Equal(2, parameters.Count);
        Assert.Equal(1, parameters["First"]);
        Assert.Equal("two", parameters["Second"]);
    }

    [Fact]
    public void Ctor_Enumerable_CopiesEntries()
    {
        var source = new[]
        {
            new KeyValuePair<string, object>("First", 1),
            new KeyValuePair<string, object>("Second", "two"),
        };

        var parameters = new NavigationParameters(source);

        Assert.Equal(2, parameters.Count);
        Assert.Equal(1, parameters["First"]);
        Assert.Equal("two", parameters["Second"]);
    }

    [Fact]
    public void Type_Checks()
    {
        var parameters = new NavigationParameters();

        Assert.IsAssignableFrom<INavigationParameters>(parameters);
        Assert.IsAssignableFrom<Dictionary<string, object>>(parameters);
    }
}

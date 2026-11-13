using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;
using Xunit;

namespace Mvvm.Nucleus.Maui.UnitTests.Services;

/// <summary>
/// Pins the private route query parsing of <see cref="NavigationService"/>
/// (<c>GetQueryParameterDictionary</c>) — pure string parsing that is unreachable through the
/// public API without a running Shell/<c>MainThread</c>.
/// </summary>
public class NavigationServiceRouteParsingTests : NucleusTestHarness
{
    [Fact]
    public void GetQueryParameterDictionary_ParsesQueryParameters()
    {
        Assert.Equal(
            new Dictionary<string, string> { ["Id"] = "42", ["Name"] = "Foo" },
            Parse("Details?Id=42&Name=Foo"));
    }

    [Fact]
    public void GetQueryParameterDictionary_SkipsMalformedParts()
    {
        // A part without '=' (here "Malformed") is skipped, not thrown away with the valid parts.
        Assert.Equal(
            new Dictionary<string, string> { ["Id"] = "42" },
            Parse("Details?Id=42&Malformed"));
    }

    [Fact]
    public void GetQueryParameterDictionary_NoQuery_ReturnsEmpty()
    {
        Assert.Empty(Parse("Details"));
    }

    [Fact]
    public void GetQueryParameterDictionary_UnparseableRoute_ReturnsEmpty()
    {
        // A space in the host makes Uri.TryCreate fail, so the method returns an empty
        // dictionary instead of throwing (an invalid route never surfaces a parse error).
        Assert.Empty(Parse("http://ex exemple.com/x?Id=42"));
    }

    private Dictionary<string, string> Parse(string route)
    {
        var method = typeof(NavigationService)
            .GetMethod("GetQueryParameterDictionary", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Private method 'GetQueryParameterDictionary' not found on NavigationService.");

        return (Dictionary<string, string>)method.Invoke(Service, [route])!;
    }

    private NavigationService? _service;

    /// <summary>
    /// The only <see cref="NavigationService"/> instance needed to host the private method
    /// (created lazily, after the base harness has made its <see cref="NucleusMvvmCore"/> Current).
    /// </summary>
    private NavigationService Service => _service ??= new(Options, NullLogger<NavigationService>.Instance);
}

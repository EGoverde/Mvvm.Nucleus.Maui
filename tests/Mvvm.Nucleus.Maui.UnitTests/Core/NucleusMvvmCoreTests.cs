using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

namespace Mvvm.Nucleus.Maui.UnitTests.Core;

/// <summary>
/// Pins the initialization seam of <see cref="NucleusMvvmCore"/>: once <c>Initialize</c> has been
/// called (through the <c>InternalsVisibleTo</c> seam) the core is registered as <c>Current</c>
/// and exposes the <see cref="IServiceProvider"/>; the <c>Shell</c> property resolves once a
/// <see cref="Shell"/> has been registered.
/// </summary>
public class NucleusMvvmCoreTests : NucleusTestHarness
{
    public NucleusMvvmCoreTests()
        : base(serviceProvider: new NoopServiceProvider())
    {
    }

    [Fact]
    public void Initialize_SetsIsInitialized_AndRegistersCurrent()
    {
        Assert.True(NucleusMvvmCore.IsInitialized);
        Assert.Same(Core, NucleusMvvmCore.Current);
    }

    [Fact]
    public void Initialize_ExposesServiceProvider()
    {
        Assert.NotNull(Core.ServiceProvider);
        Assert.IsType<NoopServiceProvider>(Core.ServiceProvider);
    }

    [Fact]
    public void Shell_AfterRegistration_ResolvesToRegisteredShell()
    {
        Assert.Same(Shell, Core.Shell);
    }

    [Fact]
    public void NavigationParameters_NullAssignment_FallsBackToEmptyDictionary()
    {
        // The library contract under test: a null assignment falls back to an empty (non-null) dictionary.
        Core.NavigationParameters = null!;

        Assert.NotNull(Core.NavigationParameters);
        Assert.Empty(Core.NavigationParameters);
    }
}

/// <summary>
/// Dedicated class that never initializes a core (no <c>Initialize</c> call, so it never touches
/// the process-wide <c>NucleusMvvmCore.Current</c>): it pins that the <c>Shell</c> getter of an
/// unregistered core throws <see cref="InvalidOperationException"/>.
/// </summary>
public class NucleusMvvmCoreShellBeforeRegistrationTests
{
    [Fact]
    public void Shell_BeforeRegistration_ThrowsInvalidOperationException()
    {
        var core = new NucleusMvvmCore(
            new Application(),
            new NoopViewFactory(),
            new NucleusMvvmOptions(),
            NullLogger<NucleusMvvmCore>.Instance);

        Assert.Throws<InvalidOperationException>(() => core.Shell);
    }
}

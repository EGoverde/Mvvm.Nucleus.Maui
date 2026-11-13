namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// A minimal <see cref="IServiceProvider"/> stub for core-only fixtures that do not resolve
/// any services. Use a real <c>ServiceCollection</c> where a <see cref="ViewFactory"/> needs
/// <c>ActivatorUtilities.CreateInstance</c> resolution.
/// </summary>
public class NoopServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return null;
    }
}

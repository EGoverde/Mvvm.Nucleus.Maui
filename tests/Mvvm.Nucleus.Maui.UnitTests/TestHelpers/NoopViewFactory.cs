namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// A minimal <see cref="IViewFactory"/> stub for core-only fixtures that do not create views.
/// </summary>
public class NoopViewFactory : IViewFactory
{
    public object CreateView(Type viewType)
    {
        throw new NotSupportedException("NoopViewFactory does not create views.");
    }

    public object ConfigureView(Element element)
    {
        return element;
    }
}

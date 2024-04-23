namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmMarkupExtensions"/> is a markup extension that can be used on a view to set
/// the value for <see cref="NucleusMvvm.NucleusConfigureViewProperty"/>.
/// </summary>
public static class NucleusMvvmMarkupExtensions
{
    /// <summary>
    /// Sets the value for the bindable property <see cref="NucleusMvvm.NucleusConfigureViewProperty"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the view.</typeparam>
    /// <param name="element">The view.</param>
    /// <param name="nucleusConfigureView">The value indicating whether to configure this view or not.</param>
    /// <returns>The view.</returns>
    public static T NucleusConfigureView<T>(this T element, bool nucleusConfigureView) where T : VisualElement
    {
        element.SetValue(NucleusMvvm.NucleusConfigureViewProperty, nucleusConfigureView);

        return element;
    }
}
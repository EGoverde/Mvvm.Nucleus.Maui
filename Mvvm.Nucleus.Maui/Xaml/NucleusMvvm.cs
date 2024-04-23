using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvm"/> static class holds the 'NucleusConfigureView' property that can be used to
/// skip the configuring of a view (e.a. wiring up the events and BindingContext) or to do it later.
/// </summary>
public static class NucleusMvvm
{
    /// <summary>
    /// This bindable property defaults to 'true' and only needs to be set if the <see cref="IViewFactory"/> should NOT automatically
    /// configure the view after it has been created. If set to 'true' later on it will configure it then.
    /// </summary>
    public static readonly BindableProperty NucleusConfigureViewProperty = BindableProperty.CreateAttached("NucleusConfigureView", typeof(bool), typeof(NucleusMvvm), null, propertyChanged: OnNucleusConfigureViewPropertyChanged);

    private static void OnNucleusConfigureViewPropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (newValue is not bool nucleusConfigureView || !nucleusConfigureView)
        {
            return;
        }

        if (bindable is not Element element)
        {
            NucleusMvvmCore.Current.Logger?.LogError($"Failed to configure bindingcontext for '{bindable.GetType()}', because it is not of type Element.");
            return;
        }

        var viewFactory = NucleusMvvmCore.Current.ViewFactory;
        if (viewFactory == null)
        {
            NucleusMvvmCore.Current.Logger?.LogError($"Failed to create '{bindable.GetType()}', because the ViewFactory could not be found.");
            return;
        }

        viewFactory.ConfigureView(element);
    }
}
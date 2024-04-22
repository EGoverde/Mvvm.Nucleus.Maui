using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

public static class NucleusMvvm
{
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
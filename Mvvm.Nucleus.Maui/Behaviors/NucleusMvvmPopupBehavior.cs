using System.Reflection;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmPageBehavior"/> is a <see cref="Behavior"/> added to all views created by Nucleus
/// which will register the various events used by the interfaces.
/// </summary>
public class NucleusMvvmPopupBehavior() : Behavior
{
    /// <summary>
    /// The <see cref="Popup"/> this behavior is attached to and which events are used.
    /// </summary>
    public Popup? Popup { get; set; }

    /// <summary>
    /// The <see cref="Element"/> this behavior is handling. This is either the same as the <see cref="Popup"/>, but can
    /// also be a <see cref="ContentView"/> that will be wrapped in a <see cref="Popup"/> by the Community Toolkit.
    /// </summary>
    public Element? Element { get; set; }

    /// <inheritdoc/>
    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);

        var bindingContext = GetBindingContext();

        if (bindingContext is IPopupAware popupAware)
        {
            popupAware.Popup = new WeakReference<Popup>(Popup!);
        }

        if (bindingContext != null)
        {
            var popupType = Popup!.GetType();
            
            var matchingGenericInterface = typeof(IPopupAware<>).MakeGenericType(popupType);
            if (matchingGenericInterface.IsAssignableFrom(bindingContext.GetType()))
            {
                var weakReference = typeof(WeakReference<>)
                    .MakeGenericType(popupType)
                    .GetConstructor([popupType])?
                    .Invoke([Popup!]);

                var propertyInfo = matchingGenericInterface.GetProperty("Popup");
                propertyInfo?.SetValue(bindingContext, weakReference);
            }
        }

        Popup!.Opened += OnPopupOpened!;
        Popup!.Closed += OnPopupClosed!;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFrom(BindableObject bindable)
    {
        base.OnDetachingFrom(bindable);

        Popup!.Opened -= OnPopupOpened!;
        Popup!.Closed -= OnPopupClosed!;
    }
    private void OnPopupOpened(object? sender, EventArgs e)
    {
        if (GetBindingContext() is IPopupLifecycleAware popupLifecycleAware)
        {
            popupLifecycleAware.OnOpened();
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (GetBindingContext() is IPopupLifecycleAware popupLifecycleAware)
        {
            popupLifecycleAware.OnClosed();
        }
    }

    private object? GetBindingContext()
    {
        return Element != null ? Element.BindingContext : Popup?.BindingContext;
    }

    private static bool IsSubclassOfGeneric(Type genericType, Type? typeToCheck, out Type? typeArgument)
    {
        typeArgument = null;

        while (typeToCheck != null && typeToCheck != typeof(Popup))
        {
            var currentDefinition = typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
            if (currentDefinition == genericType)
            {
                typeArgument = typeToCheck.GenericTypeArguments.First();
                return true;
            }

            typeToCheck = typeToCheck?.BaseType;
        }

        return false;
    }
}
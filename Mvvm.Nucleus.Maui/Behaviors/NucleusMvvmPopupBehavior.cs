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
    /// also be a <see cref="View"/> that will be wrapped in a <see cref="Popup"/> by the Community Toolkit.
    /// </summary>
    public Element? Element { get; set; }

    /// <inheritdoc/>
    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);

        var bindingContext = GetBindingContext();

        if (Element != Popup && Element is IPopupAware popupAwareElement)
        {
            popupAwareElement.Popup = new WeakReference<Popup>(Popup!);
        }

        if (bindingContext is IPopupAware popupAwareViewModel)
        {
            popupAwareViewModel.Popup = new WeakReference<Popup>(Popup!);
        }

        if (bindingContext != null)
        {
            var popupType = Popup!.GetType();

            var matchingGenericInterfaceForPopupAware = typeof(IPopupAware<>).MakeGenericType(popupType);
            if (matchingGenericInterfaceForPopupAware.IsAssignableFrom(bindingContext.GetType()))
            {
                var weakReference = typeof(WeakReference<>)
                    .MakeGenericType(popupType)
                    .GetConstructor([popupType])?
                    .Invoke([Popup!]);

                var propertyInfo = matchingGenericInterfaceForPopupAware.GetProperty("Popup");
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
        var bindingContext = GetBindingContext();

        if (bindingContext is IPopupLifecycleAware popupLifecycleAware)
        {
            popupLifecycleAware.OnOpened();
        }

        if (NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit)
        {
            NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = false;

            if (Element is IPopupInitializable popupInitializable)
            {
                popupInitializable.Init(NucleusMvvmCore.Current.PopupNavigationParameters);
            }

            if (GetBindingContext() is IPopupInitializable popupInitializableViewModel)
            {
                popupInitializableViewModel.Init(NucleusMvvmCore.Current.PopupNavigationParameters);
            }

            if (Element is IPopupInitializableAsync popupInitializableAsync)
            {
                NucleusMvvmCore.Current.RunTaskInVoidAndTrackException(() => popupInitializableAsync.InitAsync(NucleusMvvmCore.Current.NavigationParameters));
            }

            if (GetBindingContext() is IPopupInitializableAsync popupInitializableAsyncViewModel)
            {
                NucleusMvvmCore.Current.RunTaskInVoidAndTrackException(() => popupInitializableAsyncViewModel.InitAsync(NucleusMvvmCore.Current.NavigationParameters));
            }
        }

        NucleusMvvmCore.Current.AppResumed += AppResumed!;
        NucleusMvvmCore.Current.AppStopped += AppStopped!;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        NucleusMvvmCore.Current.AppResumed -= AppResumed!;
        NucleusMvvmCore.Current.AppStopped -= AppStopped!;

        var bindingContext = GetBindingContext();

        if (bindingContext is IPopupLifecycleAware popupLifecycleAware)
        {
            popupLifecycleAware.OnClosed();
        }

        var popupMapping = NucleusMvvmCore.Current.NucleusMvvmOptions.PopupMappings.FirstOrDefault(x => x.PopupViewType == Element?.GetType());
        if (popupMapping == null)
        {
            return;
        }

        if (popupMapping.ServiceLifetime == ServiceLifetime.Transient)
        {
            if (bindingContext is IDestructible destructibleViewModel)
            {
                destructibleViewModel.Destroy();
            }

            if (Element is IDestructible destructiblePopup)
            {
                destructiblePopup.Destroy();
            }

            if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseDeconstructPopupOnDestroy)
            {
                NucleusMvvmCore.Current.Logger?.LogInformation($"Deconstructing Popup '{Element?.GetType()?.Name}'.");

                Popup!.Behaviors.Remove(this);
                Popup!.BindingContext = null;
                Popup!.Parent = null;

                if (Element != null && Element != Popup)
                {
                    Element.BindingContext = null;
                    Element.Parent = null;
                }
            }
        }

        if (popupMapping.ServiceLifetime != ServiceLifetime.Transient && Element != Popup)
        {
            if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseDeconstructPopupOnDestroy)
            {
                Popup!.Behaviors.Remove(this);
                Popup!.BindingContext = null;
                Popup!.Parent = null;
            }

            PopupViewFactory.ListenToParentChanges(Element!); // The CommunityToolkit will create a new Popup later, while using the reused view
        }
    }

    private void AppResumed(object sender, EventArgs e)
    {
        if (GetBindingContext() is IApplicationLifecycleAware applicationLifecycleAware)
        {
            applicationLifecycleAware.OnResume();
        }
    }

    private void AppStopped(object sender, EventArgs e)
    {
        if (GetBindingContext() is IApplicationLifecycleAware applicationLifecycleAware)
        {
            applicationLifecycleAware.OnSleep();
        }
    }

    private object? GetBindingContext()
    {
        return Element != null ? Element.BindingContext : Popup?.BindingContext;
    }
}
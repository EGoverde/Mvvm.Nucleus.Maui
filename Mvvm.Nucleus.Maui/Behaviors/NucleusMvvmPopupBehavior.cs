using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmPopupBehavior"/> is a <see cref="Behavior"/> added to views created by Nucleus
/// through the <see cref="PopupViewFactory"/>. It uses the <see cref="CommunityToolkit.Maui.Views.Popup"/>
/// and its events.
/// </summary>
public class NucleusMvvmPopupBehavior() : Behavior
{
    private readonly bool useAlternativePopupOpenedAndClosedEvents = NucleusMvvmCore.Current.NucleusMvvmOptions.UseAlternativePopupOpenedAndClosedEvents;

    private string? _popupShellLocation;

    /// <summary>
    /// The <see cref="CommunityToolkit.Maui.Views.Popup"/> this behavior is attached to and which events are used.
    /// </summary>
    public Popup? Popup { get; set; }

    /// <summary>
    /// The <see cref="Microsoft.Maui.Controls.Element"/> this behavior is handling. This is either the same as the <see cref="Popup"/>, but can
    /// also be a <see cref="View"/> that is wrapped in a <see cref="CommunityToolkit.Maui.Views.Popup"/> by the Community Toolkit.
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
        var isInitialNavigation = string.IsNullOrEmpty(_popupShellLocation);

        var isOpened = !useAlternativePopupOpenedAndClosedEvents || isInitialNavigation;
        if (isOpened)
        {
            _popupShellLocation = NucleusMvvmCore.Current.Shell?.CurrentState?.Location?.ToString();

            if (Element is IPopupLifecycleAware popupLifecycleAware)
            {
                popupLifecycleAware.OnOpened();
            }

            if (GetBindingContext() is IPopupLifecycleAware popupViewModelLifecycleAware)
            {
                popupViewModelLifecycleAware.OnOpened();
            }
        }
        else
        {
            NucleusMvvmCore.Current.Logger?.LogInformation("Popup '{popupType}' triggered the 'Opened' event, but was ignored due to 'UseAlternativePopupOpenedAndClosedEvents'.", Element?.GetType());
        }

        if (isInitialNavigation && NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit)
        {
            NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = false;

            if (GetBindingContext() is IPopupInitializable popupInitializableViewModel)
            {
                popupInitializableViewModel.Init(NucleusMvvmCore.Current.PopupNavigationParameters);
            }

            if (GetBindingContext() is IPopupInitializableAsync popupInitializableAsyncViewModel)
            {
                NucleusMvvmCore.Current.RunTaskInVoidAndTrackException(() => popupInitializableAsyncViewModel.InitAsync(NucleusMvvmCore.Current.PopupNavigationParameters));
            }
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        var currentLocation = NucleusMvvmCore.Current.Shell?.CurrentState?.Location?.ToString() ?? string.Empty;
        var isPopupPagePopped = !currentLocation.Contains(_popupShellLocation ?? string.Empty);

        var isClosed = !useAlternativePopupOpenedAndClosedEvents || isPopupPagePopped;
        if (isClosed)
        {
            if (Element is IPopupLifecycleAware popupLifecycleAware)
            {
                popupLifecycleAware.OnClosed();
            }

            if (GetBindingContext() is IPopupLifecycleAware popupViewModelLifecycleAware)
            {
                popupViewModelLifecycleAware.OnClosed();
            }
        }
        else
        {
            NucleusMvvmCore.Current.Logger?.LogInformation("Popup '{popupType}' triggered the 'Closed' event, but was ignored due to 'UseAlternativePopupOpenedAndClosedEvents'.", Element?.GetType());
        }

        if (!isPopupPagePopped)
        {
            return;
        }

        var popupMapping = NucleusMvvmCore.Current.NucleusMvvmOptions.PopupMappings.FirstOrDefault(x => x.PopupViewType == Element?.GetType());
        if (popupMapping == null)
        {
            return;
        }

        _popupShellLocation = null;

        if (popupMapping.ServiceLifetime == ServiceLifetime.Transient)
        {
            (GetBindingContext() as IDestructible)?.Destroy();
            (Element as IDestructible)?.Destroy();

            if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseDeconstructPopupOnDestroy)
            {
                NucleusMvvmCore.Current.Logger?.LogInformation("Deconstructing Popup '{popupType}'.", Element?.GetType());

                if (Popup?.Behaviors != null)
                {
                    Popup.Behaviors.Remove(this);
                    Popup.Parent = null;
                }

                if (Element != null)
                {
                    Element.BindingContext = null;
                    Element.Parent = null;
                }
            }

            return;
        }

        // We do not call IDestructible or deconstruct when using a Scoped or Singleton registration.
        // However, if a 'View' was registered instead of a 'Popup' instance, we can still clean up the
        // Popup created by the Community Toolkit, as it will create a new one each time while reusing
        // the View.
        if (Element != Popup)
        {
            PopupViewFactory.ListenToParentChanges(Element!);
            Popup?.Behaviors.Remove(this);
        }
    }

    private object? GetBindingContext()
    {
        return Element != null ? Element.BindingContext : Popup?.BindingContext;
    }
}
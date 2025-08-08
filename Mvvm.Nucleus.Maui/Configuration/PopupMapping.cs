using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewMapping"/> class holds the registration for a <see cref="Popup"/>  and (optiona) ViewModel.
/// </summary>
public class PopupMapping
{
    internal PopupMapping(Type popupViewType, ServiceLifetime serviceLifetime)
    {
        PopupViewType = popupViewType;
        ServiceLifetime = serviceLifetime;
    }

    internal PopupMapping(Type popupViewType, Type popupViewModelType, ServiceLifetime serviceLifetime)
    {
        PopupViewType = popupViewType;
        PopupViewModelType = popupViewModelType;
        ServiceLifetime = serviceLifetime;
    }

    /// <summary>
    /// The <see cref="Type"/> of the <see cref="Popup"/>.
    /// </summary>
    public Type PopupViewType { get; }

    /// <summary>
    /// The <see cref="Type"/> of the ViewModel.
    /// </summary>
    public Type? PopupViewModelType { get; }

    /// <summary>
    /// The <see cref="ServiceLifetime"/> of this registration.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; }

    internal static PopupMapping Create<TPopupView, TPopupViewModel>(ServiceLifetime serviceLifetime) where TPopupView : View
    {
        return new PopupMapping(typeof(TPopupView), typeof(TPopupViewModel), serviceLifetime);
    }

    internal static PopupMapping Create<TPopupView>(ServiceLifetime serviceLifetime) where TPopupView : View
    {
        return new PopupMapping(typeof(TPopupView), serviceLifetime);
    }
}
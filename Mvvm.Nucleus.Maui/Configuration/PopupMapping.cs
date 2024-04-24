using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewMapping"/> class holds the registration for a <see cref="Popup"/>  and (optiona) ViewModel.
/// </summary>
public class PopupMapping
{
    internal PopupMapping(Type popupViewType)
    {
        PopupViewType = popupViewType;
    }

    internal PopupMapping(Type popupViewType, Type popupViewModelType)
    {
        PopupViewType = popupViewType;
        PopupViewModelType = popupViewModelType;
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
    /// Gets a value indicating whether this registration includes a ViewModel.
    /// </summary>
    public bool IsWithoutViewModel => PopupViewModelType == null;

    internal static PopupMapping Create<TPopupView, TPopupViewModel>() where TPopupView : Popup
    {
        return new PopupMapping(typeof(TPopupView), typeof(TPopupViewModel));
    }

    internal static PopupMapping Create<TPopupView>() where TPopupView : Popup
    {
        return new PopupMapping(typeof(TPopupView));
    }
}
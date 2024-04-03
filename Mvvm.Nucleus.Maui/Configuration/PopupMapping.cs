using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui
{
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

        public Type PopupViewType { get; }

        public Type? PopupViewModelType { get; }

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
}
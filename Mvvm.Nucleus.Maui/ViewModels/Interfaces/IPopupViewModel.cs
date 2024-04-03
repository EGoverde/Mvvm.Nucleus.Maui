using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

public interface IPopupViewModel
{
    WeakReference<Popup> Popup { get; set; }

    void OnOpened();

    void OnClosed();
}

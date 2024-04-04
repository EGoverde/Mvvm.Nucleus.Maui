using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

public interface IPopupAware
{
    WeakReference<Popup>? Popup { get; set; }
}
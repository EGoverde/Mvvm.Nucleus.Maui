using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class AdvancedPopupViewModel : Compatibility.BindableBase, IPopupAware, IPopupLifecycleAware, IPopupInitializable, IPageLifecycleAware
{
    public WeakReference<Popup>? Popup { get; set; }

    [ObservableProperty]
    private string _popupText = string.Empty;

    [ObservableProperty]
    private string _popupState = "Default";

    public void Init(IDictionary<string, object> navigationParameters)
    {
        PopupText = navigationParameters.GetValueOrDefault<string>("Text") ??  "Sample Text";
        PopupState = "Initialized";
    }

    public void OnOpened()
    {
        PopupState = "Opened";
    }

    public void OnClosed()
    {
        PopupState = "Closed";
    }

    [RelayCommand]
    private async Task CloseAsync(object? result = null)
    {
        if (Popup != null && Popup.TryGetTarget(out Popup? popup) && popup != null)
        {
            // await popup.CloseAsync(result);
            await popup.CloseAsync();
        }
    }

    public void OnAppearing()
    {
        PopupState = "OnAppearing";
    }

    public void OnDisappearing()
    {
        PopupState = "OnDisappearing";
    }

    public void OnFirstAppearing()
    {
        PopupState = "OnFirstAppearing";
    }
}
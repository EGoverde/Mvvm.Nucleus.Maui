using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class AdvancedPopupViewModel : BindableBase, IPopupAware, IPopupInitializable
{
    public WeakReference<Popup>? Popup { get; set; }

    [ObservableProperty]
    private string? _popupText;

    public void Init(IDictionary<string, object> navigationParameters)
    {
        PopupText = navigationParameters.GetValueOrDefault("Text", "Sample Text");
    }

    [RelayCommand]
    private async Task CloseAsync(object? result = null)
    {
        if (Popup != null && Popup.TryGetTarget(out Popup? popup) && popup != null)
        {
            await popup.CloseAsync(result);
        }
    }
}
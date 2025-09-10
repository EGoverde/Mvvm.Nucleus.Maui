using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class AdvancedPopupViewModel(IPopupService popupService) : Compatibility.BindableBase, IPopupAware<AdvancedPopup>, IPopupLifecycleAware, IPopupInitializable, IPageLifecycleAware
{
    private readonly IPopupService _popupService = popupService;

    public WeakReference<AdvancedPopup>? Popup { get; set; }

    [ObservableProperty]
    private string _popupText = string.Empty;

    [ObservableProperty]
    private string _popupState = "Default";

    public void Init(IDictionary<string, object> navigationParameters)
    {
        PopupText = navigationParameters.GetValueOrDefault<string>("Text") ??  "Sample Text";
        PopupState += ", Initialized";
    }

    public void OnOpened()
    {
        PopupState += ", Opened";
    }

    public void OnClosed()
    {
        PopupState += ", Closed";
    }

    [RelayCommand]
    private async Task CloseAsync(string? result = null)
    {
        if (result?.Contains("IPopupService)") == true)
        {
            await _popupService.CloseMostRecentPopupAsync(result);
            return;
        }

        if (Popup != null && Popup.TryGetTarget(out AdvancedPopup? popup) && popup != null)
        {
            await (result != null ? popup.CloseAsync(result) : popup.CloseAsync());
        }
    }

    [RelayCommand]
    private async Task OpenCascadingPopupAsync()
    {
        await _popupService.ShowPopupAsync<AdvancedPopup>(new Dictionary<string, object>
        {
            { "Text", $"Cascading Popup: {Guid.NewGuid()}" }
        });
    }

    public void OnAppearing()
    {
        PopupState += ", OnAppearing";
    }

    public void OnDisappearing()
    {
        PopupState += ", OnDisappearing";
    }

    public void OnFirstAppearing()
    {
        PopupState += ", OnFirstAppearing";
    }
}
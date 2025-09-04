using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class LegacyPopupViewModel : Compatibility.BindableBase, IPopupAware<LegacyPopup>
{
    public LegacyPopupViewModel(IPopupService popupService)
    {
        _popupService = popupService;
    }

    private readonly IPopupService _popupService;

    public WeakReference<LegacyPopup>? Popup { get; set; }

    [RelayCommand]
    private async Task CloseUsingServiceAsync(string? result = null)
    {
        await _popupService.CloseMostRecentPopupAsync<object?>(result);
    }

    [RelayCommand]
    private async Task CloseUsingPopupAwareAsync(string? result = null)
    {
        if (Popup?.TryGetTarget(out LegacyPopup? popup) == true && popup != null)
        {
            await popup.CloseAsync(result);
        }
    }
}
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class LegacyPopupViewModel(IPopupService popupService) : Compatibility.BindableBase, IPopupAware<LegacyPopup>
{
    private readonly IPopupService _popupService = popupService;

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
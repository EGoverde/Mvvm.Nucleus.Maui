using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class LegacyPopupViewModel : Compatibility.BindableBase
{
    public LegacyPopupViewModel(IPopupService popupService)
    {
        _popupService = popupService;
    }

    private readonly IPopupService _popupService;

    [RelayCommand]
    private async Task CloseAsync(string? result = null)
    {
        await _popupService.CloseMostRecentPopupAsync(result);
    }
}
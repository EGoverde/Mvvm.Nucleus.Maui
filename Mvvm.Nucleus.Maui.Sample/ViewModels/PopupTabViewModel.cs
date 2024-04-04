using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class PopupTabViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly IPageDialogService _pageDialogService;

    public PopupTabViewModel(IPopupService popupService, IPageDialogService pageDialogService)
    {
        _popupService = popupService;
        _pageDialogService = pageDialogService;
    }

    [RelayCommand]
    private async Task ShowSimplePopupAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Text", "Text from navigation parameters." }
        };

        var result = await _popupService.ShowPopupAsync<SimplePopup, string>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
    }

    [RelayCommand]
    private async Task ShowAdvancedPopupAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Text", "Text from navigation parameters." }
        };

        var result = await _popupService.ShowPopupAsync<AdvancedPopup, string>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
    }

    [RelayCommand]
    private async Task ShowNucleusPopupAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Text", "Text from navigation parameters." }
        };

        var result = await _popupService.ShowPopupAsync<AdvancedPopup, string>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
    }

    
}
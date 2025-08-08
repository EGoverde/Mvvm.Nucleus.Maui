using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class PopupTabViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly CommunityToolkit.Maui.IPopupService _communityToolkitPopupService;
    private readonly IPageDialogService _pageDialogService;

    public PopupTabViewModel(IPopupService popupService, IPageDialogService pageDialogService, CommunityToolkit.Maui.IPopupService communityToolkitPopupService)
    {
        _popupService = popupService;
        _pageDialogService = pageDialogService;
        _communityToolkitPopupService = communityToolkitPopupService;
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

        var popupResult = await _communityToolkitPopupService.ShowPopupAsync<AdvancedPopupViewModel>(Shell.Current);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{popupResult}'", "Okay");

        var result = await _popupService.ShowPopupAsync<AdvancedPopup, object?>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
    }

    [RelayCommand]
    private void ShowBackgroundThreadPopup()
    {
        Task.Run(async () =>
        {
            var navigationParameters = new Dictionary<string, object>
            {
                { "Text", $"IsMainThread = {MainThread.IsMainThread}" }
            };

            var result = await _popupService.ShowPopupAsync<AdvancedPopup, string>(navigationParameters);

            await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
        });
    }
}
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

        await _popupService.ShowPopupAsync<SimplePopup>(navigationParameters);
        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed'", "Okay");
    }

    [RelayCommand]
    private async Task ShowAdvancedPopupAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Text", "Text from navigation parameters." }
        };

        // _ = await _communityToolkitPopupService.ShowPopupAsync<AdvancedPopupViewModel>(Shell.Current, null, shellParameters: navigationParameters);

        var popupResult = await _popupService.ShowPopupAsync<AdvancedPopup, string>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{popupResult.Result}'", "Okay");

        // var result = await _popupService.ShowPopupAsync<AdvancedPopup, object?>(navigationParameters);

        // await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{result}'", "Okay");
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

            var result = await _popupService.ShowPopupAsync<SimplePopup>();
        });
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mvvm.Nucleus.Maui.Compatibility;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class PopupTabViewModel(IPopupService popupService, IPageDialogService pageDialogService, CommunityToolkit.Maui.IPopupService communityToolkitPopupService, CommunityToolkitV1PopupService communityToolkitV1PopupService) : ObservableObject
{
    private readonly IPopupService _popupService = popupService;

    private readonly CommunityToolkit.Maui.IPopupService _communityToolkitPopupService = communityToolkitPopupService;

    private readonly IPageDialogService _pageDialogService = pageDialogService;

    private readonly CommunityToolkitV1PopupService _communityToolkitV1PopupService = communityToolkitV1PopupService;

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

        var popupResult = await _popupService.ShowPopupAsync<AdvancedPopup, string>(navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{popupResult.Result}'. WasDismissedByTappingOutsideOfPopup: {popupResult.WasDismissedByTappingOutsideOfPopup}.", "Okay");
    }

    [RelayCommand]
    private async Task ShowSingletonPopupAsync()
    {
        var popupResult = await _popupService.ShowPopupAsync<SingletonPopup>();

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed. WasDismissedByTappingOutsideOfPopup: {popupResult.WasDismissedByTappingOutsideOfPopup}.", "Okay");
    }

    [RelayCommand]
    private async Task ShowCommunityToolkitPopupAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Text", "Text from navigation parameters." }
        };

        var popupResult = await _communityToolkitPopupService.ShowPopupAsync<AdvancedPopup, string>(Shell.Current, shellParameters: navigationParameters);

        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{popupResult.Result}'. WasDismissedByTappingOutsideOfPopup: {popupResult.WasDismissedByTappingOutsideOfPopup}.", "Okay");
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

    [RelayCommand]
    private async Task ShowCompatibilityPopupAsync()
    {
        var popupResult = await _communityToolkitV1PopupService.ShowPopupAsync<LegacyPopup, string>();
        
        await _pageDialogService.DisplayAlertAsync("Alert", $"Popup was closed, result was '{popupResult}'.", "Okay");
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class NavigationTabViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : ObservableObject
{
    private readonly INavigationService _navigationService = navigationService;

    private readonly IPageDialogService _pageDialogService = pageDialogService;

    [RelayCommand]
    private async Task NavigateByTypeAsync()
    {
        await _navigationService.NavigateAsync<Details>();
    }

    [RelayCommand]
    private async Task NavigateByRouteAsync()
    {
        await _navigationService.NavigateToRouteAsync(nameof(Details));
    }

    [RelayCommand]
    private async Task NavigateByModalAsync()
    {
        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { NucleusNavigationParameters.NavigatingPresentationMode, PresentationMode.ModalAnimated }
        });
    }

    [RelayCommand]
    private async Task NavigationByModalWithNavigationStackAsync()
    {
        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { NucleusNavigationParameters.NavigatingPresentationMode, PresentationMode.ModalAnimated },
            { NucleusNavigationParameters.WrapInNavigationPage, true }
        });
    }

    [RelayCommand]
    private async Task NavigateWithParametersAsync()
    {
        var parameterValue = await _pageDialogService.DisplayPromptAsync("Prompt", "Enter a value for the parameter.");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return;
        }

        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { "Sample", parameterValue }
        });
    }

    [RelayCommand]
    private async Task NavigateWithQueryAsync()
    {
        var parameterValue = await _pageDialogService.DisplayPromptAsync("Prompt", "Enter a value for the parameter.");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return;
        }

        await _navigationService.NavigateToRouteAsync($"{nameof(Details)}?Sample={parameterValue}");
    }

    [RelayCommand]
    private void NavigateFromBackgroundThread()
    {
        Task.Run(async () =>
        {
            var text = $"IsMainThread = {MainThread.IsMainThread}";
            
            await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
            {
                { "Sample", text }
            });
        });
    }

    [RelayCommand]
    private void NavigateMultipleTriggers()
    {
        _ = _navigationService.NavigateToRouteAsync(nameof(Details));
        _ = _navigationService.NavigateToRouteAsync(nameof(Details));
    }

    [RelayCommand]
    private async Task SwitchTabsAsync()
    {
        await _navigationService.NavigateAsync<PopupTab>();
    }
}
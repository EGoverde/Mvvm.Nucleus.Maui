using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class NavigationTabViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IPageDialogService _pageDialogService;

    public NavigationTabViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
    {
        _navigationService = navigationService;
        _pageDialogService = pageDialogService;
    }

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
        await _navigationService.NavigateModalAsync<Details>();
    }

    [RelayCommand]
    private async Task NavigateWithParametersAsync()
    {
        var parameterValue = await _pageDialogService.DisplayPromptAsync("Prompt", "Enter a value for the parameter.");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return;
        }

        await _navigationService.NavigateModalAsync<Details>(new Dictionary<string, object>
        {
            { "Sample", parameterValue }
        });
    }

    [RelayCommand]
    private async Task SwitchTabsAsync()
    {
        await _navigationService.NavigateAsync<HelpTab>();
    }

    [RelayCommand]
    private async Task RestartAppAsync()
    {
        await _navigationService.NavigateAsync<Intro>();
    }
}
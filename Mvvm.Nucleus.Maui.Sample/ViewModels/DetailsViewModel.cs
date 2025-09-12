using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class DetailsViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : ObservableObject, IInitializable, IConfirmNavigationAsync
{
    private readonly INavigationService _navigationService = navigationService;

    private readonly IPageDialogService _pageDialogService = pageDialogService;

    private bool _shouldConfirmNavigation;

    [ObservableProperty]
    private string _pageIdentifier = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _navigationParameterData = "-";

    [ObservableProperty]
    private string _onInitData = "-";

    [ObservableProperty]
    private string _onRefreshData = "-";

    public void Init(IDictionary<string, object> navigationParameters)
    {
        OnInitData = $"Triggered ({DateTime.Now:HH:mm:ss)})";
        SetNavigationParameterData(navigationParameters);
    }

    public void Refresh(IDictionary<string, object> navigationParameters)
    {
        OnRefreshData = $"Triggered ({DateTime.Now:HH:mm:ss)})";
        SetNavigationParameterData(navigationParameters);
    }

    public async Task<bool> CanNavigateAsync(NavigationDirection navigationDirection, IDictionary<string, object> navigationParameters)
    {
        if (!_shouldConfirmNavigation)
        {
            return true;
        }

        _shouldConfirmNavigation = false;

        return await _pageDialogService.DisplayAlertAsync("Please confirm", "Navigation has been requested, do you want to proceed?", "Confirm", "Cancel");
    }

    [RelayCommand]
    private async Task NavigatePushAsync()
    {
        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { "Navigation", $"Pushed from {PageIdentifier}."}
        });
    }

    [RelayCommand]
    private async Task NavigateModalAsync()
    {
        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { "Navigation", $"Modally from {PageIdentifier}."},
            { NucleusNavigationParameters.NavigatingPresentationMode, PresentationMode.ModalAnimated }
        });
    }

    [RelayCommand]
    private async Task NavigateModalWithNavigationStackAsync()
    {
        await _navigationService.NavigateAsync<Details>(new Dictionary<string, object>
        {
            { "Navigation", $"Modally from {PageIdentifier}."},
            { NucleusNavigationParameters.NavigatingPresentationMode, PresentationMode.ModalAnimated },
            { NucleusNavigationParameters.WrapInNavigationPage, true },
        });
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateBackAsync(new Dictionary<string, object>
        {
            { "Navigation", $"Popped from {PageIdentifier}."}
        });
    }

    [RelayCommand]
    private async Task NavigateBackWithConfirmAsync()
    {
        _shouldConfirmNavigation = true;

        await _navigationService.NavigateBackAsync(new Dictionary<string, object>
        {
            { "Navigation", $"Popped from {PageIdentifier}."}
        });
    }

    [RelayCommand]
    private async Task NavigateBackTwiceAsync()
    {
        await _navigationService.NavigateToRouteAsync("../..", new Dictionary<string, object>
        {
            { "Navigation", $"Popped from {PageIdentifier}."}
        });
    }

    [RelayCommand]
    private async Task CloseModalAsync()
    {
        await _navigationService.CloseModalAsync(new Dictionary<string, object>
        {
            { "Navigation", $"Modal Closed from {PageIdentifier}."}
        });
    }

    [RelayCommand]
    private async Task CloseAllModalAsync()
    {
        await _navigationService.CloseAllModalAsync();
    }

    private void SetNavigationParameterData(IDictionary<string, object> navigationParameters)
    {
        var stringBuilder = new StringBuilder();

        var parameters = navigationParameters.Where(x =>
            x.Key != NucleusNavigationParameters.NavigatingPresentationMode &&
            x.Key != NucleusNavigationParameters.WrapInNavigationPage &&
            x.Key != Compatibility.CompatibilityNavigationParameters.NavigationMode);

        foreach (var navigationParameter in parameters)
        {
            if (stringBuilder.Length == 0)
            {
                stringBuilder.Append($"{navigationParameter.Key} : {navigationParameter.Value}");
            }
            else
            {
                stringBuilder.Append($", {navigationParameter.Key} : {navigationParameter.Value}");
            }
        }

        var result = stringBuilder.ToString();

        NavigationParameterData = string.IsNullOrEmpty(result) ? "-" : result;
    }
}
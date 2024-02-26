using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class HelpTabViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public HelpTabViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task ShowDetailsAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Example 1", 4 },
            { "Example 2", 8 }
        };

        await _navigationService.NavigateModalAsync<Details>(parameters);
    }
}
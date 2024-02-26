using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class IntroViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

        public IntroViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task StartAppAsync()
        {
            await _navigationService.NavigateAsync<NavigationTab>();
        }
}
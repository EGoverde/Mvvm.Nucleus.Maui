using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class SingletonPopupViewModel(IPopupService popupService, IPageDialogService pageDialogService) : Compatibility.BindableBase, IPopupLifecycleAware, IPopupInitializable
{
    private readonly IPopupService _popupService = popupService;

    private readonly IPageDialogService _pageDialogService = pageDialogService;

    [ObservableProperty]
    private string _popupState = "Default";

    [ObservableProperty]
    private string _instanceValue = "Unset";

    public void Init(IDictionary<string, object> navigationParameters)
    {
        PopupState += ", Initialized";
    }

    public void OnOpened()
    {
        PopupState += ", Opened";
    }

    public void OnClosed()
    {
        PopupState += ", Closed";
    }

    [RelayCommand]
    private async Task SetValueAsync()
    {
        var value = await _pageDialogService.DisplayPromptAsync("Prompt", "Enter a value for the value.");
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        InstanceValue = value;
    }

    [RelayCommand]
    private async Task CloseAsync(string? result = null)
    {
        await _popupService.CloseMostRecentPopupAsync();
    }
}
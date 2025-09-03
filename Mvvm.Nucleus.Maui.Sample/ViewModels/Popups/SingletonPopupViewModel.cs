using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class SingletonPopupViewModel : Compatibility.BindableBase, IPopupLifecycleAware, IPopupInitializable
{
    public SingletonPopupViewModel(IPopupService popupService, IPageDialogService pageDialogService)
    {
        _popupService = popupService;
        _pageDialogService = pageDialogService;
    }

    private readonly IPopupService _popupService;

    private readonly IPageDialogService _pageDialogService;

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
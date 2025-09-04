using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class DialogTabViewModel(IPageDialogService pageDialogService) : ObservableObject
{
    private readonly IPageDialogService _pageDialogService = pageDialogService;

    [RelayCommand]
    private async Task DisplayAlertAsync()
    {
        await _pageDialogService.DisplayAlertAsync("Alert", "This is an alert", "Okay");
    }

    [RelayCommand]
    private async Task DisplayConfirmAsync()
    {
        var input = await _pageDialogService.DisplayAlertAsync("Alert", "This is an alert", "Confirm", "Cancel");
        var inputText = input ? "to confirm" : "to cancel";
        
        await _pageDialogService.DisplayAlertAsync("Alert", $"You choose {inputText} as input.", "Okay");
    }

    [RelayCommand]
    private async Task DisplayPromptAsync()
    {
        var input = await _pageDialogService.DisplayPromptAsync("Prompt", "Enter a value for the parameter.");
        input = string.IsNullOrEmpty(input) ? "nothing" : $"'{input}'";

        await _pageDialogService.DisplayAlertAsync("Alert", $"You choose {input} as input.", "Okay");
    }

    [RelayCommand]
    private async Task DisplayActionSheetAsync()
    {
        var input = await _pageDialogService.DisplayActionSheetAsync("Action Sheet", "Cancel", null, "Option One", "Option Two");
        input = string.IsNullOrEmpty(input) ? "nothing" : $"'{input}'";

        await _pageDialogService.DisplayAlertAsync("Alert", $"You choose {input} as input.", "Okay");
    }

    [RelayCommand]
    private void DisplayBackgroundThreadAlert()
    {
        Task.Run(async () =>
        {
            var text = $"IsMainThread = {MainThread.IsMainThread}";
            await _pageDialogService.DisplayAlertAsync("Alert", $"This is an alert triggered from another thread ({text})", "Okay");
        });
    }
}
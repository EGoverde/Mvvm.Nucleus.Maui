namespace Mvvm.Nucleus.Maui;

public interface IPageDialogService
{
    Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton);

    Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection);

    Task DisplayAlertAsync(string title, string message, string cancelButton);

    Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection);

    Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);

    Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons);

    Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "");
}

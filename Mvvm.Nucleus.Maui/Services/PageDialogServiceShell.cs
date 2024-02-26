namespace Mvvm.Nucleus.Maui;

public class PageDialogServiceShell : IPageDialogService
{
    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
    {
        return Shell.Current.DisplayActionSheet(title, cancel, destruction, buttons);
    }

    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
    {
        return Shell.Current.DisplayActionSheet(title, cancel, destruction, flowDirection, buttons);
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
    {
        return Shell.Current.DisplayAlert(title, message, acceptButton, cancelButton);
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection)
    {
        return Shell.Current.DisplayAlert(title, message, acceptButton, cancelButton, flowDirection);
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton)
    {
        return Shell.Current.DisplayAlert(title, message, cancelButton);
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection)
    {
        return Shell.Current.DisplayAlert(title, message, cancelButton, flowDirection);
    }

    public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "")
    {
        return Shell.Current.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboardType, initialValue);
    }
}

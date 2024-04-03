namespace Mvvm.Nucleus.Maui;

public class PageDialogService : IPageDialogService
{
    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }

    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayActionSheet(title, cancel, destruction, flowDirection, buttons);
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, acceptButton, cancelButton);
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, acceptButton, cancelButton, flowDirection);
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, cancelButton);
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, cancelButton, flowDirection);
    }

    public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "")
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboardType, initialValue);
    }
}

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PageDialogService"/> is the default implementation for <see cref="IPageDialogService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class PageDialogService : IPageDialogService
{
    /// <inheritdoc/>
    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }

    /// <inheritdoc/>
    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayActionSheet(title, cancel, destruction, flowDirection, buttons);
    }

    /// <inheritdoc/>
    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, acceptButton, cancelButton);
    }

    /// <inheritdoc/>
    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, acceptButton, cancelButton, flowDirection);
    }

    /// <inheritdoc/>
    public Task DisplayAlertAsync(string title, string message, string cancelButton)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, cancelButton);
    }

    /// <inheritdoc/>
    public Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection)
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayAlert(title, message, cancelButton, flowDirection);
    }

    /// <inheritdoc/>
    public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "")
    {
        return NucleusMvvmCore.Current.CurrentPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboardType, initialValue);
    }
}
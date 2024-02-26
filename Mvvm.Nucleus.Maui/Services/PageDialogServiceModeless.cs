
namespace Mvvm.Nucleus.Maui;

public class PageDialogServiceModeless : IPageDialogService
{
    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
    {
        throw new NotImplementedException();
    }

    public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection)
    {
        throw new NotImplementedException();
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton)
    {
        throw new NotImplementedException();
    }

    public Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection)
    {
        throw new NotImplementedException();
    }

    public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "")
    {
        throw new NotImplementedException();
    }
}

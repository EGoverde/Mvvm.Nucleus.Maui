using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

public interface IPopupService
{
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : Popup;

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : Popup;

    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup;

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : Popup;

    public Task<object?> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default);
    
    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default);
}
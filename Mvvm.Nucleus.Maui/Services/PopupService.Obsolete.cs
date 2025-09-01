using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupService"/> is the default implementation for <see cref="IPopupService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public partial class PopupService : IPopupService
{
       /// <inheritdoc/>
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : View
    {
        return CreateAndShowPopupAsync(typeof(TPopup));
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : View
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(typeof(TPopup)), defaultResult);
    }

    /// <inheritdoc/>
    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : View
    {
        return CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : View
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token), defaultResult);
    }

    /// <inheritdoc/>
    public Task<object?> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        return CreateAndShowPopupAsync(popupViewType, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default)
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(popupViewType, navigationParameters, token), defaultResult);
    }

    private Task<object?> CreateAndShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters = default, CancellationToken token = default)
    {
        return Task.FromResult<object?>(null);
    }

    private async Task<TResult?> ConvertResultAsync<TResult>(Task<object?> resultTask, TResult? defaultResult)
    {
        var result = await resultTask;

        if (result == null)
        {
            _logger.LogInformation($"Return value from popup is null, using the default result (if given).");
            return defaultResult;
        }
        
        if (result is not TResult)
        {
            _logger.LogError($"Return value '{result.GetType()}' from popup does not match expected type ({typeof(TResult)}), using the default result (if given).");
            return defaultResult;
        }

        return (TResult)result;
    }
/*
    /// <inheritdoc/>
    public async Task<IPopupResult> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup
    {
        IPopupResult result;

        if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseCommunityToolkitPopupService)
        {
            result = await _ctPopupService.ShowPopupAsync<TPopup>(NucleusMvvmCore.Current.Shell!, cancellationToken: token);
        }
        else
        {
            var popupView = _serviceProvider.GetRequiredService<TPopup>();
            result = await NucleusMvvmCore.Current.Shell!.ShowPopupAsync(popupView, token: token);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : Popup
    {
        IPopupResult<TResult> result;

        if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseCommunityToolkitPopupService)
        {
            result = await _ctPopupService.ShowPopupAsync<TPopup, TResult>(NucleusMvvmCore.Current.Shell!, cancellationToken: token);
        }
        else
        {
            var popupView = _serviceProvider.GetRequiredService<TPopup>();
            result = await NucleusMvvmCore.Current.Shell!.ShowPopupAsync<TResult>(popupView, token: token);
        }

        return result.Result ?? defaultResult;
    }

    /// <inheritdoc/>
    public async Task<IPopupResult> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        if (_serviceProvider.GetRequiredService(popupViewType) is not View popupView)
        {
            throw new InvalidOperationException($"Nucleus failed to create a popup of type '{popupViewType}'");
        }

        return await NucleusMvvmCore.Current.Shell!.ShowPopupAsync(popupView, token: token);
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default)
    {
        if (_serviceProvider.GetRequiredService(popupViewType) is not View popupView)
        {
            throw new InvalidOperationException($"Nucleus failed to create a popup of type '{popupViewType}'");
        }

        var result = await NucleusMvvmCore.Current.Shell!.ShowPopupAsync<TResult>(popupView, token: token);

        return result.Result ?? defaultResult;
    }
    */
}
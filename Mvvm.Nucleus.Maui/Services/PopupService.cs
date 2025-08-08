using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupService"/> is the default implementation for <see cref="IPopupService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class PopupService : IPopupService
{
    private readonly ILogger<PopupService> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly CommunityToolkit.Maui.IPopupService _popupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="popupService">The <see cref="CommunityToolkit.Maui.IPopupService"/>.</param>
    public PopupService(ILogger<PopupService> logger, IServiceProvider serviceProvider, CommunityToolkit.Maui.IPopupService popupService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _popupService = popupService;
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<TPopup>() where TPopup : View
    {
        return ShowPopupAsync<TPopup>(null);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : View
    {
        return ShowPopupAsync<TPopup, TResult>(null, defaultResult);
    }

    /// <inheritdoc/>
    public async Task<IPopupResult> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : View
    {
        IPopupResult result;

        if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseCommunityToolkitPopupService)
        {
            result = await _popupService.ShowPopupAsync<TPopup>(NucleusMvvmCore.Current.Shell!, cancellationToken: token);
        }
        else
        {
            var popupView = _serviceProvider.GetRequiredService<TPopup>();
            result = await NucleusMvvmCore.Current.Shell!.ShowPopupAsync(popupView, token: token);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : View
    {
        IPopupResult<TResult> result;

        if (NucleusMvvmCore.Current.NucleusMvvmOptions.UseCommunityToolkitPopupService)
        {
            result = await _popupService.ShowPopupAsync<TPopup, TResult>(NucleusMvvmCore.Current.Shell!, cancellationToken: token);
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
}
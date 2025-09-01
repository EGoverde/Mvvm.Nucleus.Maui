using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupService"/> is the default implementation for <see cref="IPopupService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public partial class PopupService : IPopupService
{
    private readonly ILogger<PopupService> _logger;

    private readonly IServiceProvider _serviceProvider;

    private readonly CommunityToolkit.Maui.IPopupService _ctPopupService;

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
        _ctPopupService = popupService;
    }

    /// <inheritdoc/>
    public Task ShowPopupAsync<TPopup>() where TPopup : View
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : Popup<TResult>
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : View
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
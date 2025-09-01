using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupService"/> is the default implementation for <see cref="IPopupService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public partial class PopupService
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
    public Task<IPopupResult> ShowPopupAsync<T>(Page page, CommunityToolkit.Maui.IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T>(page, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<T>(INavigation navigation, CommunityToolkit.Maui.IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T>(navigation, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<T>(Shell shell, CommunityToolkit.Maui.IPopupOptions? options, IDictionary<string, object>? shellParameters = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T>(shell, options, shellParameters, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(Page page, CommunityToolkit.Maui.IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T, TResult>(page, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(INavigation navigation, CommunityToolkit.Maui.IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T, TResult>(navigation, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(Shell shell, CommunityToolkit.Maui.IPopupOptions? options = null, IDictionary<string, object>? shellParameters = null, CancellationToken cancellationToken = default) where T : notnull
    {
        return _ctPopupService.ShowPopupAsync<T, TResult>(shell, options, shellParameters, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ClosePopupAsync(Page page, CancellationToken cancellationToken = default)
    {
        return _ctPopupService.ClosePopupAsync(page, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<T>> ClosePopupAsync<T>(Page page, T result, CancellationToken cancellationToken = default)
    {
        return _ctPopupService.ClosePopupAsync(page, result, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ClosePopupAsync(INavigation navigation, CancellationToken cancellationToken = default)
    {
        return _ctPopupService.ClosePopupAsync(navigation, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<T>> ClosePopupAsync<T>(INavigation navigation, T result, CancellationToken cancellationToken = default)
    {
        return _ctPopupService.ClosePopupAsync(navigation, result, cancellationToken);
    }

    /// <inheritdoc/>
    public void ShowPopup<T>(Page page, CommunityToolkit.Maui.IPopupOptions? options = null) where T : notnull
    {
        _ctPopupService.ShowPopup<T>(page, options);
    }

    /// <inheritdoc/>
    public void ShowPopup<T>(INavigation navigation, CommunityToolkit.Maui.IPopupOptions? options = null) where T : notnull
    {
        _ctPopupService.ShowPopup<T>(navigation, options);
    }

    /// <inheritdoc/>
    public void ShowPopup<T>(Shell shell, CommunityToolkit.Maui.IPopupOptions? options = null, IDictionary<string, object>? shellParameters = null) where T : notnull
    {
        _ctPopupService.ShowPopup<T>(shell, options, shellParameters);
    }
}
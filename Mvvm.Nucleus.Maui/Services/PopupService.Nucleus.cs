using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
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

    private readonly CommunityToolkit.Maui.Services.PopupService _communityToolkitPopupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="communityToolkitPopupService">The <see cref="CommunityToolkit.Maui.Services.PopupService"/>.</param>
    public PopupService(ILogger<PopupService> logger, IServiceProvider serviceProvider, CommunityToolkit.Maui.Services.PopupService communityToolkitPopupService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _communityToolkitPopupService = communityToolkitPopupService;
    }

    /// <inheritdoc/>
    public void ShowPopup<TPopup>(IDictionary<string, object>? navigationParameters = null) where TPopup : View
    {
        ShowPopup<TPopup>(null, navigationParameters);
    }

    /// <inheritdoc/>
    public void ShowPopup<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null) where TPopup : View
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();
        NucleusMvvmCore.Current.Shell!.ShowPopup(CreatePopupContent<TPopup>(), options, navigationParameters);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View
    {
        return ShowPopupAsync<TPopup>(null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();
        return NucleusMvvmCore.Current.Shell!.ShowPopupAsync(CreatePopupContent<TPopup>(), options, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        return ShowPopupAsync<TPopup, TResult>(null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();
        return NucleusMvvmCore.Current.Shell!.ShowPopupAsync<TResult>(CreatePopupContent<TPopup>(), options, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        return ShowPopupAsync<TPopup, TResult>(defaultValue, null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        var result = await ShowPopupAsync<TPopup, TResult>(options, navigationParameters, token);
        return result.Result ?? defaultValue;
    }

    /// <inheritdoc/>
    public async Task CloseMostRecentPopupAsync(CancellationToken token = default)
    {
        await NucleusMvvmCore.Current.Shell!.ClosePopupAsync(token);
    }

    /// <inheritdoc/>
    public async Task CloseMostRecentPopupAsync<TResult>(TResult result, CancellationToken token = default)
    {
        await NucleusMvvmCore.Current.Shell!.ClosePopupAsync(result, token);
    }
    
    private View CreatePopupContent<T>() where T : View
	{
		if (_serviceProvider.GetService(typeof(T)) is View content)
		{
			return content;
		}

        _logger.LogInformation($"Failed to create a popup from the type '{typeof(T)}'. Register the popup through the DependencyOptions in the NucleusMvvmBuilder.");

		throw new InvalidOperationException($"Could not locate {typeof(T).FullName}");
	}
}
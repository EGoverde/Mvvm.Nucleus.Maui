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
/// <remarks>
/// Initializes a new instance of the <see cref="PopupService"/> class.
/// </remarks>
/// <param name="logger">The <see cref="ILogger"/>.</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
/// <param name="communityToolkitPopupService">The <see cref="CommunityToolkit.Maui.Services.PopupService"/>.</param>
public partial class PopupService(ILogger<PopupService> logger, IServiceProvider serviceProvider, CommunityToolkit.Maui.Services.PopupService communityToolkitPopupService) : IPopupService
{
    private readonly ILogger<PopupService> _logger = logger;

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private readonly CommunityToolkit.Maui.Services.PopupService _communityToolkitPopupService = communityToolkitPopupService;

    /// <inheritdoc/>
    public async void ShowPopup<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null) where TPopup : View
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();

        var popup = await CreateAndInitializePopupAsync<TPopup>();

        MainThread.BeginInvokeOnMainThread(() => NucleusMvvmCore.Current.Shell!.ShowPopup(popup, options, navigationParameters));
    }

    /// <inheritdoc/>
    public async Task<IPopupResult> ShowPopupAsync<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();

        var popup = await CreateAndInitializePopupAsync<TPopup>();
        var result = await MainThread.InvokeOnMainThreadAsync(() => NucleusMvvmCore.Current.Shell!.ShowPopupAsync(popup, options, navigationParameters, token));

        return result;
    }

    /// <inheritdoc/>
    public async Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();

        var popup = await CreateAndInitializePopupAsync<TPopup>();
        var result = await MainThread.InvokeOnMainThreadAsync(() => NucleusMvvmCore.Current.Shell!.ShowPopupAsync<TResult>(popup, options, navigationParameters, token));

        return result;
    }

    /// <inheritdoc/>
    public async Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        var result = await ShowPopupAsync<TPopup, TResult>(options, navigationParameters, token);

        return result.Result ?? defaultValue;
    }

    /// <inheritdoc/>
    public void ShowPopup<TPopup>(IDictionary<string, object>? navigationParameters = null) where TPopup : View
    {
        ShowPopup<TPopup>(null, navigationParameters);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View
    {
        return ShowPopupAsync<TPopup>(null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        return ShowPopupAsync<TPopup, TResult>(null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>
    {
        return ShowPopupAsync<TPopup, TResult>(defaultValue, null, navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task CloseMostRecentPopupAsync(CancellationToken token = default)
    {
        return MainThread.InvokeOnMainThreadAsync(() => NucleusMvvmCore.Current.Shell!.ClosePopupAsync(token));
    }

    /// <inheritdoc/>
    public Task CloseMostRecentPopupAsync<TResult>(TResult result, CancellationToken token = default)
    {
        return MainThread.InvokeOnMainThreadAsync(() => NucleusMvvmCore.Current.Shell!.ClosePopupAsync(result, token));
    }
    
    /// <summary>
    /// Creates a <see cref="Popup"/> or <see cref="View"/> through the <see cref="IServiceProvider"/> and initializes it if it 
    /// implements <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="View"/> to create.</typeparam>
    /// <returns>The created <see cref="View"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested View cannot be resolved.</exception>
    protected async Task<View> CreateAndInitializePopupAsync<T>() where T : View
    {
        if (_serviceProvider.GetService(typeof(T)) is View content)
        {
            if (content is IPopupInitializable popupInitializable)
            {
                popupInitializable.Init(NucleusMvvmCore.Current.PopupNavigationParameters);
            }

            if (content.BindingContext is IPopupInitializable popupInitializableViewModel)
            {
                popupInitializableViewModel.Init(NucleusMvvmCore.Current.PopupNavigationParameters);
            }

            if (content is IPopupInitializableAsync popupInitializableAsync)
            {
                if (popupInitializableAsync.AwaitInitializeBeforeShowing)
                {
                    await popupInitializableAsync.InitAsync(NucleusMvvmCore.Current.PopupNavigationParameters);
                }
                else
                {
                    _ = popupInitializableAsync.InitAsync(NucleusMvvmCore.Current.PopupNavigationParameters);
                }
            }

            if (content.BindingContext is IPopupInitializableAsync popupInitializableAsyncViewModel)
            {
                if (popupInitializableAsyncViewModel.AwaitInitializeBeforeShowing)
                {
                    await popupInitializableAsyncViewModel.InitAsync(NucleusMvvmCore.Current.PopupNavigationParameters);
                }
                else
                {
                    _ = popupInitializableAsyncViewModel.InitAsync(NucleusMvvmCore.Current.PopupNavigationParameters);
                }
            }

            return content;
        }

        _logger.LogInformation("Failed to create a popup from the type '{expectedType}'. Register the popup through the DependencyOptions in the NucleusMvvmBuilder.", typeof(T));

        throw new InvalidOperationException($"Could not locate {typeof(T).FullName}");
    }
}
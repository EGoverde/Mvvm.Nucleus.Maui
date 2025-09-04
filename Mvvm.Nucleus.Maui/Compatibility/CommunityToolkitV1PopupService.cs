using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// A compatibility service that can be used as an intermediate step when migrating from the earlier CommunityToolkit
/// V1 Popups used in Nucleus versions below 0.6.x. It is recommended to migrate to the new <see cref="IPopupService"/>.
/// </summary>
[Obsolete("This class is only for limited compatibility with the original PopupService and will be removed in future versions. Use IPopupService instead.")]
public class CommunityToolkitV1PopupService : PopupService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopupService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="communityToolkitPopupService">The <see cref="CommunityToolkit.Maui.Services.PopupService"/>.</param>
    public CommunityToolkitV1PopupService(ILogger<CommunityToolkitV1PopupService> logger, IServiceProvider serviceProvider, CommunityToolkit.Maui.Services.PopupService communityToolkitPopupService) : base(logger, serviceProvider, communityToolkitPopupService)
    {
        _logger = logger;
    }

    private readonly ILogger<CommunityToolkitV1PopupService> _logger;

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/>.</returns>
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : CommunityToolkitV1Popup
    {
        return ShowPopupAsync<TPopup>(null, default);
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="defaultResult">The default return value.</param>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : CommunityToolkitV1Popup
    {
        return ShowPopupAsync<TPopup, TResult>(null, defaultResult, default);
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/>.</returns>
    public new async Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : CommunityToolkitV1Popup
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = navigationParameters ?? new Dictionary<string, object>();

        var popup = await CreateAndInitializePopupAsync<TPopup>() as CommunityToolkitV1Popup;

        var fallbackPopupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = popup!.CanBeDismissedByTappingOutsideOfPopup,
            Shadow = default,
            Shape = default
        };

        var popupResult = await MainThread.InvokeOnMainThreadAsync(() => NucleusMvvmCore.Current.Shell!.ShowPopupAsync<object?>(popup!, fallbackPopupOptions, navigationParameters, token));

        if (popupResult.WasDismissedByTappingOutsideOfPopup && popup!.ResultWhenUserTapsOutsideOfPopup != null)
        {
            _logger.LogInformation("Popup was dismissed by tapping outside of the popup, returning compatibility ResultWhenUserTapsOutsideOfPopup value.");
            return popup.ResultWhenUserTapsOutsideOfPopup;
        }

        return popupResult.Result;
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="defaultResult">The default return value.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : CommunityToolkitV1Popup
    {
        return ConvertResultAsync(ShowPopupAsync<TPopup>(navigationParameters, token), defaultResult);
    }

    /// <summary>
    /// Attempts to cast the result of the <see cref="Popup"/> to the expected <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TResult">The expected result <see cref="Type"/>.</typeparam>
    /// <param name="resultTask">The <see cref="Task"/> that returns the result.</param>
    /// <param name="defaultResult">The result to return if an unexpected value was found.</param>
    /// <returns>The casted result, or the default value if invalid.</returns>
    protected virtual async Task<TResult?> ConvertResultAsync<TResult>(Task<object?> resultTask, TResult? defaultResult)
    {
        var result = await resultTask;

        if (result == null)
        {
            _logger.LogInformation("Return value from popup is null, using the default result (if given).");
            return defaultResult;
        }
        
        if (result is not TResult)
        {
            _logger.LogError("Return value '{resultType}' from popup does not match expected type ({expectedType}), using the default result (if given).", result.GetType(), typeof(TResult));
            return defaultResult;
        }

        return (TResult)result;
    }
}
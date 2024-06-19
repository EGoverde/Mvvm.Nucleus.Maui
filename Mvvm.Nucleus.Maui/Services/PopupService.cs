using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupService"/> is the default implementation for <see cref="IPopupService"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class PopupService : IPopupService
{
    private readonly NucleusMvvmOptions _nucleusMvvmOptions;

    private readonly ILogger<PopupService> _logger;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupService"/> class.
    /// </summary>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    public PopupService(NucleusMvvmOptions nucleusMvvmOptions, ILogger<PopupService> logger, IServiceProvider serviceProvider)
    {
        _nucleusMvvmOptions = nucleusMvvmOptions;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : Popup
    {
        return CreateAndShowPopupAsync(typeof(TPopup));
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : Popup
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(typeof(TPopup)), defaultResult);
    }

    /// <inheritdoc/>
    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup
    {
        return CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token);
    }

    /// <inheritdoc/>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : Popup
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

    /// <summary>
    /// Creates a Popup through <see cref="CreatePopup(Type)"/> and wires up the various events. If a viewmodel registration exists it is created
    /// and set as BindingContext as well. Finally it is displayed through the current active <see cref="Page"/>.
    /// </summary>
    /// <param name="popupViewType">The <see cref="Type"/> of the Popup.</param>
    /// <param name="navigationParameters"></param>
    /// <param name="token"></param>
    /// <returns>The result of the <see cref="Popup"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a <see cref="Type"/> was given that is not a <see cref="Popup"/>.</exception>
    protected virtual async Task<object?> CreateAndShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters = default, CancellationToken token = default)
    {
        var popup = CreatePopup(popupViewType);
        if (popup == null)
        {
            throw new InvalidOperationException($"Nucleus failed to create a popup of type '{popupViewType}'");
        }

        var currentPage = NucleusMvvmCore.Current.CurrentPage;

        object? bindingContext = null;

        var popupMapping = _nucleusMvvmOptions.PopupMappings.FirstOrDefault(x => x.PopupViewType == popupViewType);
        if (popupMapping != null && popupMapping.PopupViewModelType != null)
        {
            bindingContext = ActivatorUtilities.CreateInstance(_serviceProvider, popupMapping!.PopupViewModelType);
        }

        void popupOpened(object? sender, PopupOpenedEventArgs eventArgs)
        {
            if (sender is Popup popup && popup.BindingContext is IPopupLifecycleAware popupViewModel)
            {
                popupViewModel.OnOpened();
            }
        };

        void popupClosed(object? sender, PopupClosedEventArgs eventArgs)
        {
            if (sender is Popup popup)
            {
                popup.Opened -= popupOpened;
                popup.Closed -= popupClosed;

                if (popup.BindingContext is IPopupLifecycleAware popupViewModel)
                {
                    popupViewModel.OnClosed();
                }

                if (popup.BindingContext is IDestructible destructibleViewModel)
                {
                    destructibleViewModel.Destroy();
                }

                if (popup is IDestructible destructiblePopup)
                {
                    destructiblePopup.Destroy();
                }

                if (_nucleusMvvmOptions.UseDeconstructPopupOnDestroy)
                {
                    _logger?.LogInformation($"Deconstructing Popup '{popup.GetType().Name}'.");

                    popup.Parent = null;
                    popup.BindingContext = null;
                }
            }
        };

        popup.Opened += popupOpened;
        popup.Closed += popupClosed;

        if (bindingContext is IPopupInitializable popupBindingContextInitializable)
        {
            popupBindingContextInitializable.Init(navigationParameters ?? new Dictionary<string, object>());
        }

        if (bindingContext is IPopupInitializableAsync popupBindingContextInitializableAsync)
        {
            var initTask = popupBindingContextInitializableAsync.InitAsync(navigationParameters ?? new Dictionary<string, object>());

            if (popupBindingContextInitializableAsync.AwaitInitializeBeforeShowing)
            {
                await initTask;
            }
        }

        popup.BindingContext = bindingContext;

        if (bindingContext is IPopupAware popupAware)
        {
            popupAware.Popup = new WeakReference<Popup>(popup);
        }

        if (popup is IPopupInitializable popupInitializable)
        {
            popupInitializable.Init(navigationParameters ?? new Dictionary<string, object>());
        }

        if (popup is IPopupInitializableAsync popupInitializableAsync)
        {
            var initTask  = popupInitializableAsync.InitAsync(navigationParameters ?? new Dictionary<string, object>());

            if (popupInitializableAsync.AwaitInitializeBeforeShowing)
            {
                await initTask;
            }
        }

        return await MainThread.InvokeOnMainThreadAsync(() => currentPage.ShowPopupAsync(popup, token));
    }

    /// <summary>
    /// Creates a <see cref="Popup"/> from the given <see cref="Type"/>. If not registed in IoC it is created directly.
    /// </summary>
    /// <param name="popupViewType"></param>
    /// <returns></returns>
    protected virtual Popup CreatePopup(Type popupViewType)
    {
        var view = _serviceProvider.GetService(popupViewType);
        if (view is Popup popup)
        {
            return popup;
        }

        _logger.LogInformation($"Popup of type '{popupViewType}' has not been registered, created a non-IoC instance instead.");

        return (Popup)ActivatorUtilities.CreateInstance(_serviceProvider, popupViewType);
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
}
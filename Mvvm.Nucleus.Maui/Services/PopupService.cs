using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

public class PopupService : IPopupService
{
    private readonly NucleusMvvmOptions _nucleusMvvmOptions;

    private readonly ILogger<PopupService> _logger;

    private readonly IServiceProvider _serviceProvider;

    public PopupService(NucleusMvvmOptions nucleusMvvmOptions, ILogger<PopupService> logger, IServiceProvider serviceProvider)
    {
        _nucleusMvvmOptions = nucleusMvvmOptions;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : Popup
    {
        return CreateAndShowPopupAsync(typeof(TPopup));
    }

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : Popup
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(typeof(TPopup)), defaultResult);
    }

    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup
    {
        return CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token);
    }

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : Popup
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token), defaultResult);
    }

    public Task<object?> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        return CreateAndShowPopupAsync(popupViewType, navigationParameters, token);
    }

    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default)
    {
        return ConvertResultAsync(CreateAndShowPopupAsync(popupViewType, navigationParameters, token), defaultResult);
    }

    protected virtual async Task<object?> CreateAndShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters = default, CancellationToken token = default)
    {
        var popup = CreatePopup(popupViewType);
        if (popup == null)
        {
            throw new InvalidOperationException($"Nucleus failed to create a popup of type '{popupViewType}'");
        }

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

                popup.Parent = null;
                popup.BindingContext = null;
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

        return await NucleusMvvmCore.Current.CurrentPage.ShowPopupAsync(popup, token);
    }

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
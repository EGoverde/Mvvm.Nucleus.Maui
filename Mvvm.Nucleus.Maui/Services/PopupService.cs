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

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>() where TPopup : Popup
    {
        return ConvertResultAsync<TResult>(CreateAndShowPopupAsync(typeof(TPopup)));
    }

    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup
    {
        return CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token);
    }

    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : Popup
    {
        return ConvertResultAsync<TResult>(CreateAndShowPopupAsync(typeof(TPopup), navigationParameters, token));
    }

    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        return ConvertResultAsync<TResult>(CreateAndShowPopupAsync(popupViewType, navigationParameters, token));
    }

   public Task<object?> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default)
    {
        return CreateAndShowPopupAsync(popupViewType, navigationParameters, token);
    }

    protected virtual async Task<object?> CreateAndShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters = default, CancellationToken token = default)
    {
        var popup = CreatePopup(popupViewType);
        if (popup == null)
        {
            throw new InvalidOperationException($"Nucleus failed to create a popup of type '{popupViewType}'");
        }

        var popupMapping = _nucleusMvvmOptions.PopupMappings.FirstOrDefault(x => x.PopupViewType == popupViewType);
        if (popupMapping != null && popupMapping.PopupViewModelType != null)
        {
            popup.BindingContext = ActivatorUtilities.CreateInstance(_serviceProvider, popupMapping!.PopupViewModelType);
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

        if (popup.BindingContext != null)
        {
            if (popup.BindingContext is IPopupInitializable popupBindingContextInitializable)
            {
                popupBindingContextInitializable.Init(navigationParameters ?? new Dictionary<string, object>());
            }

            if (popup.BindingContext is IPopupInitializableAsync popupBindingContextInitializableAsync)
            {
                var initTask = popupBindingContextInitializableAsync.InitAsync(navigationParameters ?? new Dictionary<string, object>());

                if (popupBindingContextInitializableAsync.AwaitInitializeBeforeShowing)
                {
                    await initTask;
                }
            }
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

    protected virtual async Task<TResult?> ConvertResultAsync<TResult>(Task<object?> resultTask)
    {
        var result = await resultTask;
        if (result == null)
        {
            return (TResult?)result;
        }

        return (TResult?)Convert.ChangeType(result, typeof(TResult?));
    }
}
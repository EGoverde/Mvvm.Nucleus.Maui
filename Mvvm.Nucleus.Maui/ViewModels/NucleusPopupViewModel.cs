using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

public interface INucleusPopupViewModel : INotifyPropertyChanged, INotifyPropertyChanging, IPopupAware, IPopupInitializableAsync, IPopupLifecycleAware
{
    bool IsInitialized { get; }

    bool IsInitializing { get; }

    Task OnInitAsync(IDictionary<string, object> parameters);
}

public abstract partial class NucleusPopupViewModel : BindableBase, INucleusPopupViewModel
{
    private bool _isInitializing;

    private bool _isInitialized;

    public virtual bool AwaitInitializeBeforeShowing => false;

    public WeakReference<Popup>? Popup { get; set; }

    public bool IsInitialized
    {
        get => _isInitialized;
        private set => SetProperty(ref _isInitialized, value);
    }

    public bool IsInitializing
    {
        get => _isInitializing;
        private set => SetProperty(ref _isInitializing, value);
    }

    public async Task InitAsync(IDictionary<string, object> navigationParameters)
    {
        if (IsInitialized)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("PopupViewModel '{0}' is already initialized.", GetType());
            return;
        }

        if (IsInitializing)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("PopupViewModel '{0}' is already in the process of initializing.", GetType());
            return;
        }

        IsInitializing = true;

        try
        {
            await OnInitAsync(navigationParameters);
        }
        finally
        {
            IsInitialized = true;
            IsInitializing = false;
        };
    }

    public virtual Task OnInitAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    public virtual void OnClosed()
    {
    }

    public virtual void OnOpened()
    {
    }

    [RelayCommand]
    protected virtual async Task CloseAsync(object? result = null)
    {
        if (GetPopup() is Popup popup)
        {
            await popup.CloseAsync(result);
        }
    }

    protected Popup? GetPopup()
    {
        return Popup?.TryGetTarget(out Popup? popup) == true && popup != null ? popup : default;
    }
}
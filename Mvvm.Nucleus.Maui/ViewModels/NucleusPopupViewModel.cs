using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="INucleusPopupViewModel"/> holds initialization functions and properties used by <see cref="NucleusPopupViewModel"/>.
/// </summary>
public interface INucleusPopupViewModel : INotifyPropertyChanged, INotifyPropertyChanging, IPopupAware, IPopupInitializableAsync, IPopupLifecycleAware
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> is currently initializing.
    /// </summary>
    bool IsInitializing { get; }

    /// <summary>
    /// Triggered during <see cref="IPopupInitializableAsync.InitAsync(IDictionary{string, object})"/>.
    /// This method should be overridden instead of using <see cref="IPopupInitializableAsync.InitAsync(IDictionary{string, object})"/>.
    /// </summary>
    /// <param name="parameters">The initialization parameters.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task OnInitAsync(IDictionary<string, object> parameters);
}

/// <summary>
/// An optional ViewModel that can be inherited to have commonly used functions and properties.
/// </summary>
public abstract partial class NucleusPopupViewModel : Compatibility.BindableBase, INucleusPopupViewModel
{
    private bool _isInitializing;

    private bool _isInitialized;

    /// <inheritdoc/>
    public virtual bool AwaitInitializeBeforeShowing => false;

    /// <inheritdoc/>
    public WeakReference<Popup>? Popup { get; set; }

    /// <inheritdoc/>
    public bool IsInitialized
    {
        get => _isInitialized;
        private set => SetProperty(ref _isInitialized, value);
    }

    /// <inheritdoc/>
    public bool IsInitializing
    {
        get => _isInitializing;
        private set => SetProperty(ref _isInitializing, value);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public virtual Task OnInitAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual void OnClosed()
    {
    }

    /// <inheritdoc/>
    public virtual void OnOpened()
    {
    }

    /// <summary>
    /// Closes the popup and returns the given result value to the caller.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    [RelayCommand]
    protected virtual async Task CloseAsync(object? result = null)
    {
        if (GetPopup() is Popup popup)
        {
            await popup.CloseAsync(result);
        }
    }

    /// <summary>
    /// Attempts to get the <see cref="Popup"/> from the <see cref="WeakReference"/>.
    /// </summary>
    /// <returns>The <see cref="Popup"/>.</returns>
    protected Popup? GetPopup()
    {
        return Popup?.TryGetTarget(out Popup? popup) == true && popup != null ? popup : default;
    }
}
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="INucleusViewModel"/> holds initialization functions and properties used by <see cref="NucleusViewModel"/>.
/// </summary>
public interface INucleusViewModel : INotifyPropertyChanged, INotifyPropertyChanging, IInitializableAsync, INavigatedAware
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> is currently initializing.
    /// This property should not be set manually. It is used by <see cref="IsBusy"/>.
    /// </summary>
    bool IsInitializing { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> is currently refreshing.
    /// This property should not be set manually. It is used by <see cref="IsBusy"/>.
    /// </summary>
    bool IsRefreshing { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> is currently loading.
    /// This property is used to manually set a loading state. It is used by <see cref="IsBusy"/>.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="INucleusPopupViewModel"/> is currently busy with
    /// a task, such as initializing, refreshing or loading.
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Triggered during <see cref="IInitializableAsync.InitAsync(IDictionary{string, object})"/>.
    /// This method should be overridden instead of using <see cref="IInitializableAsync.InitAsync(IDictionary{string, object})"/>.
    /// </summary>
    /// <param name="parameters">The initialization parameters.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task OnInitAsync(IDictionary<string, object> parameters);

    /// <summary>
    /// Triggered during <see cref="IInitializableAsync.RefreshAsync(IDictionary{string, object})"/>.
    /// This method should be overridden instead of using <see cref="IInitializableAsync.RefreshAsync(IDictionary{string, object})"/>.
    /// </summary>
    /// <param name="parameters">The refresh parameters.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task OnRefreshAsync(IDictionary<string, object> parameters);
}

/// <summary>
/// An optional ViewModel that can be inherited to have commonly used functions and properties.
/// </summary>
public abstract class NucleusViewModel : Compatibility.BindableBase, INucleusViewModel
{
    private bool _isInitializing;

    private bool _isInitialized;

    private bool _isRefreshing;

    private bool _isLoading;

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
        private set
        {
            if (SetProperty(ref _isInitializing, value))
            {
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        private set
        {
            if (SetProperty(ref _isRefreshing, value))
            {
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    /// <inheritdoc/>
    public bool IsBusy => GetIsBusy();

    /// <inheritdoc/>
    public async Task InitAsync(IDictionary<string, object> navigationParameters)
    {
        if (IsInitialized)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{type}' is already initialized.", GetType());
            return;
        }

        if (IsInitializing)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{type}' is already in the process of initializing.", GetType());
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
    public async Task RefreshAsync(IDictionary<string, object> navigationParameters)
    {
        if (IsRefreshing)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{type}' is already in the process of refreshing.", GetType());
            return;
        }

        IsRefreshing = true;

        try
        {
            await OnRefreshAsync(navigationParameters);
        }
        finally
        {
            IsRefreshing = false;
        };
    }

    /// <inheritdoc/>
    public virtual Task OnInitAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task OnRefreshAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the value for <see cref="IsBusy"/>. Override it to have additional properties affect the value.
    /// Don't forget to call the <see cref="INotifyPropertyChanged.PropertyChanged"/> method in that case.
    /// </summary>
    /// <returns>The value for <see cref="IsBusy"/>.</returns>
    protected virtual bool GetIsBusy()
    {
        return IsInitializing || IsRefreshing || IsLoading;
    }

    /// <inheritdoc/>
    public virtual void OnNavigatedTo(IDictionary<string, object> navigationParameters)
    {
    }

    /// <inheritdoc/>
    public virtual void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
    {
    }

    /// <inheritdoc/>
    public virtual void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
    {
    }
}
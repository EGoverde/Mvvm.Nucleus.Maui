using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

public interface INucleusViewModel : INotifyPropertyChanged, INotifyPropertyChanging, IInitializableAsync, INavigatedAware
{
    bool IsInitialized { get; }

    bool IsInitializing { get; }

    bool IsRefreshing { get; }

    bool IsLoading { get; }

    bool IsBusy { get; }

    Task OnInitAsync(IDictionary<string, object> parameters);

    Task OnRefreshAsync(IDictionary<string, object> parameters);
}

public abstract class NucleusViewModel : BindableBase, INucleusViewModel
{
    private bool _isInitializing;

    private bool _isInitialized;

    private bool _isRefreshing;

    private bool _isLoading;

    public bool IsInitialized
    {
        get => _isInitialized;
        private set => SetProperty(ref _isInitialized, value);
    }

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

    public bool IsBusy => GetIsBusy();

    public async Task InitAsync(IDictionary<string, object> navigationParameters)
    {
        if (IsInitialized)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{0}' is already initialized.", GetType());
            return;
        }

        if (IsInitializing)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{0}' is already in the process of initializing.", GetType());
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

    public async Task RefreshAsync(IDictionary<string, object> navigationParameters)
    {
        if (IsRefreshing)
        {
            NucleusMvvmCore.Current.Logger?.LogWarning("ViewModel '{0}' is already in the process of refreshing.", GetType());
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

    public virtual Task OnInitAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnRefreshAsync(IDictionary<string, object> parameters)
    {
        return Task.CompletedTask;
    }

    protected virtual bool GetIsBusy()
    {
        return IsInitializing || IsRefreshing || IsLoading;
    }

    public virtual void OnNavigatedTo(IDictionary<string, object> navigationParameters)
    {
    }

    public virtual void OnNavigatedFrom(IDictionary<string, object> navigationParameters)
    {
    }

    public virtual void OnNavigatingFrom(IDictionary<string, object> navigationParameters)
    {
    }
}
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;

namespace Mvvm.Nucleus.Maui;

public partial class PopupService : CommunityToolkit.Maui.IPopupService
{
    /// <inheritdoc/>
    public void ShowPopup<T>(Shell shell, IPopupOptions? options = null, IDictionary<string, object>? shellParameters = null) where T : notnull
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = shellParameters ?? new Dictionary<string, object>();
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        _communityToolkitPopupService.ShowPopup<T>(shell, options, shellParameters);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(Shell shell, IPopupOptions? options = null, IDictionary<string, object>? shellParameters = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = shellParameters ?? new Dictionary<string, object>();
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T, TResult>(shell, options, shellParameters, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<T>(Shell shell, IPopupOptions? options, IDictionary<string, object>? shellParameters = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupNavigationParameters = shellParameters ?? new Dictionary<string, object>();
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T>(shell, options, shellParameters, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(Page page, IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T, TResult>(page, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<TResult>> ShowPopupAsync<T, TResult>(INavigation navigation, IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T, TResult>(navigation, options, cancellationToken);
    }

    /// <inheritdoc/>
    public void ShowPopup<T>(Page page, IPopupOptions? options = null) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        _communityToolkitPopupService.ShowPopup<T>(page, options);
    }

    /// <inheritdoc/>
    public void ShowPopup<T>(INavigation navigation, IPopupOptions? options = null) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        _communityToolkitPopupService.ShowPopup<T>(navigation, options);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<T>(Page page, IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T>(page, options, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ShowPopupAsync<T>(INavigation navigation, IPopupOptions? options = null, CancellationToken cancellationToken = default) where T : notnull
    {
        NucleusMvvmCore.Current.PopupOpenedThroughCommunityToolkit = true;

        return _communityToolkitPopupService.ShowPopupAsync<T>(navigation, options, cancellationToken);
    }
    
     /// <inheritdoc/>
    public Task<IPopupResult> ClosePopupAsync(Page page, CancellationToken cancellationToken = default)
    {
        return _communityToolkitPopupService.ClosePopupAsync(page, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<T>> ClosePopupAsync<T>(Page page, T result, CancellationToken cancellationToken = default)
    {
        return _communityToolkitPopupService.ClosePopupAsync(page, result, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult> ClosePopupAsync(INavigation navigation, CancellationToken cancellationToken = default)
    {
        return _communityToolkitPopupService.ClosePopupAsync(navigation, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IPopupResult<T>> ClosePopupAsync<T>(INavigation navigation, T result, CancellationToken cancellationToken = default)
    {
        return _communityToolkitPopupService.ClosePopupAsync(navigation, result, cancellationToken);
    }
}
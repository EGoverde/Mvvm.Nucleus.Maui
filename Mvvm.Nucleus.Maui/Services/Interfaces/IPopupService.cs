using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupService"/> handles the creation of Popups and their (optional) ViewModels, as well as displaying them.
/// Using this service is required to make use of the various popup interfaces (e.a. <see cref="IPopupLifecycleAware"/>).
/// The interface extends <see cref="CommunityToolkit.Maui.IPopupService"/> as to intercept the otherwise inaccessible navigation
/// parameters.
/// </summary>
public interface IPopupService : CommunityToolkit.Maui.IPopupService
{
    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="View"/>.</typeparam>
    /// <returns>The result from the <see cref="Popup"/>.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="View"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="defaultResult">The default return value.</param>
    /// <returns>The result from the <see cref="Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="View"/>.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/>.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="View"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="defaultResult">The default return value.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <param name="popupViewType">The <see cref="Type"/> of the <see cref="View"/>.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/>.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<object?> ShowPopupAsync(Type popupViewType, IDictionary<string, object>? navigationParameters, CancellationToken token = default);

    /// <summary>
    /// Creates and shows a <see cref="Popup"/> with an expected result.
    /// </summary>
    /// <param name="popupViewType">The <see cref="Type"/> of the <see cref="View"/>.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="defaultResult">The default return value.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    [Obsolete("Use the methods from the CommunityToolkit.Maui.IPopupService interface instead.")]
    public Task<TResult?> ShowPopupAsync<TResult>(Type popupViewType, IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default);
}
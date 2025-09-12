using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPopupService"/> handles the creation of Popups and their (optional) ViewModels, as well as displaying them.
/// Using this service is required to make use of the various popup interfaces (e.a. <see cref="IPopupLifecycleAware"/>).
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/> or <see cref="View"/>.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    void ShowPopup<TPopup>(IDictionary<string, object>? navigationParameters = null) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/> or <see cref="View"/>.</typeparam>
    /// <param name="options">The <see cref="IPopupOptions"/> for displaying the popup.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    void ShowPopup<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/> or <see cref="View"/>.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<IPopupResult> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/> or <see cref="View"/>.</typeparam>
    /// <param name="options">The <see cref="IPopupOptions"/> for displaying the popup.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<IPopupResult> ShowPopupAsync<TPopup>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : View;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/> with a return value wrapped in an <see cref="IPopupResult"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/> with a return value wrapped in an <see cref="IPopupResult"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="options">The <see cref="IPopupOptions"/> for displaying the popup.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<IPopupResult<TResult>> ShowPopupAsync<TPopup, TResult>(IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>, with a return value based on <see cref="IPopupResult"/>.
    /// The value will fallback to the default if the result is <see langword="null"/> or the popup dismissed.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="defaultValue">The default return value.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>;

    /// <summary>
    /// Creates and shows a <see cref="Popup"/>, with a return value based on <see cref="IPopupResult"/>.
    /// The value will fallback to the default if the result is <see langword="null"/> or the popup dismissed.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="defaultValue">The default return value.</param>
    /// <param name="options">The <see cref="IPopupOptions"/> for displaying the popup.</param>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IPopupResult"/>> from the <see cref="Popup"/>.</returns>
    Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultValue, IPopupOptions? options, IDictionary<string, object>? navigationParameters = null, CancellationToken token = default) where TPopup : Popup<TResult>;

    /// <summary>
    /// Closes the most recently opened <see cref="Popup"/> without a return value.
    /// </summary>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>A bool indicating whether or not the action succeeded.</returns>
    Task CloseMostRecentPopupAsync(CancellationToken token = default);

    /// <summary>
    /// Closes the most recently opened <see cref="Popup"/> with the given return value.
    /// Note that the given return value must match the type used to show the popup.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The return value.</param>
    /// <param name="token">The <see cref="CancellationToken"/>.</param>
    /// <returns>A bool indicating whether or not the action succeeded.</returns>
    Task CloseMostRecentPopupAsync<TResult>(TResult result, CancellationToken token = default);
}
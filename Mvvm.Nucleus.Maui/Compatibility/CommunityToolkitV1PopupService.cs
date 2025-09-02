namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// A compatibility service that can be used as an intermediate step when migrating from the original CommunityToolkit
/// <see cref="CommunityToolkit.Maui.Services.PopupService"/>. It is recommended to migrate to the new
/// <see cref="IPopupService"/> as soon as possible.
/// </summary>
[Obsolete("This class is only for compatibility with the original PopupService and will be removed in future versions. Use IPopupService instead.")]
public class CommunityToolkitV1PopupService
{
    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/>.</returns>
    public Task<object?> ShowPopupAsync<TPopup>() where TPopup : CommunityToolkitV1Popup
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="defaultResult">The default return value.</param>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(TResult? defaultResult = default) where TPopup : CommunityToolkitV1Popup
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/>.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="CommunityToolkitV1Popup"/>.</returns>
    public Task<object?> ShowPopupAsync<TPopup>(IDictionary<string, object>? navigationParameters, CancellationToken token = default) where TPopup : CommunityToolkitV1Popup
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates and shows a <see cref="CommunityToolkitV1Popup"/> with an expected result.
    /// </summary>
    /// <typeparam name="TPopup">The type of the <see cref="CommunityToolkitV1Popup"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="navigationParameters">The navigation parameters to pass to <see cref="IPopupInitializable"/> or <see cref="IPopupInitializableAsync"/>.</param>
    /// <param name="defaultResult">The default return value.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to cancel the popup.</param>
    /// <returns>The result from the <see cref="Popup"/> or the default if <see langword="null"/> or an invalid type.</returns>
    public Task<TResult?> ShowPopupAsync<TPopup, TResult>(IDictionary<string, object>? navigationParameters, TResult? defaultResult = default, CancellationToken token = default) where TPopup : CommunityToolkitV1Popup
    {
        throw new NotImplementedException();
    }
}
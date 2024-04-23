namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPageLifecycleAware"/> handles the events of a page appearing or disappearing.
/// </summary>
public interface IPageLifecycleAware
{
    /// <summary>
    /// Triggered when a page is appearing.
    /// </summary>
    void OnAppearing();

    /// <summary>
    /// Triggered when a page is disappearing.
    /// </summary>
    void OnDisappearing();

    /// <summary>
    /// Triggered when a page is appearing for the first time.
    /// </summary>
    void OnFirstAppearing();
}
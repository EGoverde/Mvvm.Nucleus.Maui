namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IApplicationLifecycleAware"/> handles the events of the app going to and from the background.
/// </summary>
public interface IApplicationLifecycleAware
{
    /// <summary>
    /// Triggered when the app goes to the background.
    /// </summary>
    void OnSleep();

    /// <summary>
    /// Triggered when the app goes to the foreground.
    /// </summary>
    void OnResume();
}
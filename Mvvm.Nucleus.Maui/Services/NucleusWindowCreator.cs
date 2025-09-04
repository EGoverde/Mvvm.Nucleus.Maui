namespace Mvvm.Nucleus.Maui;

/// <summary>
/// <see cref="NucleusWindowCreator"/> creates a <see cref="Window"/> that sets <see cref="Shell"/>
/// as a MainPage. Register your own <see cref="IWindowCreator"/> or child of this class for any
/// custom <see cref="Window"/> logic.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IWindowCreator"/> class.
/// </remarks>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
public class NucleusWindowCreator(IServiceProvider serviceProvider) : IWindowCreator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public virtual Window CreateWindow(Application app, IActivationState? activationState)
    {
        if (!NucleusMvvmCore.IsInitialized)
        {
            var nucleusMvvmCore = _serviceProvider.GetRequiredService<NucleusMvvmCore>();
            var nucleusMvvmOptions = _serviceProvider.GetRequiredService<NucleusMvvmOptions>();

            nucleusMvvmCore.Initialize(_serviceProvider);
            nucleusMvvmOptions.OnInitialized?.Invoke(_serviceProvider);
        }

        var shell = _serviceProvider.GetRequiredService<Shell>();
        var window = new Window(shell);

        NucleusMvvmCore.Current.Window = window;
        NucleusMvvmCore.Current.Shell = shell;

        return window;
    }
}
namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="WindowCreator"/> handles the creation of a <see cref="Window"/>, which has the <see cref="Shell"/>, registered through
/// <see cref="AppBuilderExtensions.UseNucleusMvvm{TApp, TShell}(MauiAppBuilder, Action{Mvvm.Nucleus.Maui.NucleusMvvmOptions}?)"/>, set as
/// its <see cref="Window.Page"/>.
/// </summary>
public class WindowCreator : IWindowCreator
{
    /// <inheritdoc/>
    public Window CreateWindow(Application app, IActivationState? activationState)
    {
        var shell = NucleusMvvmCore.Current!.ServiceProvider!.GetRequiredService<Shell>();
        var window = new Window(shell);

        NucleusMvvmCore.Current!.Window = window;

        return window;
    }
}
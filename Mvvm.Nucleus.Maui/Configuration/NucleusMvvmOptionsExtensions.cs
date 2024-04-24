namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmOptionsExtensions"/> contains the extensions used for the <see cref="MauiAppBuilder"/>.
/// </summary>
public static class NucleusMvvmOptionsExtensions
{
    /// <summary>
    /// Register Views, ViewModels and Popups using the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/> instance.</param>
    /// <param name="registerTypes">The <see cref="Action"/> that holds the registration logic added by an app.</param>
    /// <returns>The <see cref="NucleusMvvmOptions"/>.</returns>
    public static NucleusMvvmOptions RegisterTypes(this NucleusMvvmOptions nucleusMvvmOptions, Action<DependencyOptions> registerTypes)
    {
        nucleusMvvmOptions.RegisterTypes = registerTypes;
        return nucleusMvvmOptions;
    }

    /// <summary>
    /// Called after Nucleus is initialized and has not yet navigated to the first <see cref="Page"/>.
    /// </summary>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/> instance.</param>
    /// <param name="onInitialized">The <see cref="Action"/> that holds the additional logic added by an app.</param>
    /// <returns>The <see cref="NucleusMvvmOptions"/>.</returns>
    public static NucleusMvvmOptions OnInitialized(this NucleusMvvmOptions nucleusMvvmOptions, Action<IServiceProvider> onInitialized)
    {
        nucleusMvvmOptions.OnInitialized = onInitialized;
        return nucleusMvvmOptions;
    }

    /// <summary>
    /// Called after Nucleus is initialized and has finished its initial navigation.
    /// </summary>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/> instance.</param>
    /// <param name="onAppStart">The <see cref="Action"/> that holds the additional logic added by an app.</param>
    /// <returns>The <see cref="NucleusMvvmOptions"/>.</returns>
    public static NucleusMvvmOptions OnAppStart(this NucleusMvvmOptions nucleusMvvmOptions, Action<IServiceProvider> onAppStart)
    {
        nucleusMvvmOptions.OnAppStart = onAppStart;
        return nucleusMvvmOptions;
    }
}

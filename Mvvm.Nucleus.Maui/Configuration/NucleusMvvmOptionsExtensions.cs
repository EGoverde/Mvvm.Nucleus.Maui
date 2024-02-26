namespace Mvvm.Nucleus.Maui;

public static class NucleusMvvmOptionsExtensions
{
    public static NucleusMvvmOptions RegisterTypes(this NucleusMvvmOptions nucleusMvvmOptions, Action<DependencyOptions> registerTypes)
    {
        nucleusMvvmOptions.RegisterTypes = registerTypes;
        return nucleusMvvmOptions;
    }

    public static NucleusMvvmOptions OnInitialized(this NucleusMvvmOptions nucleusMvvmOptions, Action<IServiceProvider> onInitialized)
    {
        nucleusMvvmOptions.OnInitialized = onInitialized;
        return nucleusMvvmOptions;
    }

    public static NucleusMvvmOptions OnAppStart(this NucleusMvvmOptions nucleusMvvmOptions, Action<IServiceProvider> onAppStart)
    {
        nucleusMvvmOptions.OnAppStart = onAppStart;
        return nucleusMvvmOptions;
    }
}

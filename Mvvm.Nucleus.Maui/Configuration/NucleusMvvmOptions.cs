namespace Mvvm.Nucleus.Maui
{
    public class NucleusMvvmOptions
    {
        internal Action<DependencyOptions>? RegisterTypes { get; set; }

        internal Action<IServiceProvider>? OnInitialized { get; set; }

        internal Action<IServiceProvider>? OnAppStart { get; set; }

        public NavigationType NavigationType { get; } = NavigationType.Shell;

        public DialogServiceOptions DialogOptions { get; } = new DialogServiceOptions();
        
        public IReadOnlyCollection<ViewMapping> ViewMappings => DependencyOptions.ViewMappings;

        internal DependencyOptions DependencyOptions { get; } = new DependencyOptions();
    }
}
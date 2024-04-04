namespace Mvvm.Nucleus.Maui
{
    public class NucleusMvvmOptions
    {
        internal Action<DependencyOptions>? RegisterTypes { get; set; }

        internal Action<IServiceProvider>? OnInitialized { get; set; }

        internal Action<IServiceProvider>? OnAppStart { get; set; }

        public IReadOnlyCollection<ViewMapping> ViewMappings => DependencyOptions.ViewMappings;

        public IReadOnlyCollection<PopupMapping> PopupMappings => DependencyOptions.PopupMappings;

        internal DependencyOptions DependencyOptions { get; } = new DependencyOptions();

        public NavigationType NavigationType { get; } = NavigationType.Shell;

        public bool AddQueryParametersToDictionary { get; set; } = true;

        public bool AlwaysDisableNavigationAnimation { get; set; } = false;

        public bool IgnoreNavigationWhenInProgress { get; set; } = false;

        public bool UsePageDestructionOnNavigation { get; set; } = true;

        public bool UseShellNavigationQueryParameters {get; set; } = true;

        public bool UsePopupDestructionAfterClose {get; set; } = true;
    }
}
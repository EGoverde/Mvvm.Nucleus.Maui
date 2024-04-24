namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmOptions"/> contains all the relevant configuration values set during initialization
/// of Nucleus. Currently no additional IoC registrations can be done after initialization, but the changing of
/// the configuration values is supported.
/// </summary>
public class NucleusMvvmOptions
{
    internal Action<DependencyOptions>? RegisterTypes { get; set; }

    internal Action<IServiceProvider>? OnInitialized { get; set; }

    internal Action<IServiceProvider>? OnAppStart { get; set; }

    internal DependencyOptions DependencyOptions { get; } = new DependencyOptions();

    /// <summary>
    /// Gets the read-only collection of the <see cref="ViewMapping"/> that have been registered.
    /// </summary>
    public IReadOnlyCollection<ViewMapping> ViewMappings => DependencyOptions.ViewMappings;

    /// <summary>
    /// Gets the read-only collection of the <see cref="PopupMapping"/> that have been registered.
    /// </summary>
    public IReadOnlyCollection<PopupMapping> PopupMappings => DependencyOptions.PopupMappings;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically add query parameters during navigation to the navigation parameters dictionary.
    /// </summary>
    public bool AddQueryParametersToDictionary { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to always disable animation when navigating. This does not affect default iOS animations.
    /// </summary>
    public bool AlwaysDisableNavigationAnimation { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore navigation requests when the <see cref="INavigationService"/> is already navigating.
    /// </summary>
    public bool IgnoreNavigationWhenInProgress { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to 'destroy' pages after they are no longer in the navigation stack.
    /// </summary>
    public bool UsePageDestructionOnNavigation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to 'destroy' popups after they are dismissed.
    /// </summary>
    public bool UsePopupDestructionAfterClose {get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use <see cref="ShellNavigationQueryParameters"/> when navigating (see MAUI documentation).
    /// </summary>
    public bool UseShellNavigationQueryParameters {get; set; } = true;
}
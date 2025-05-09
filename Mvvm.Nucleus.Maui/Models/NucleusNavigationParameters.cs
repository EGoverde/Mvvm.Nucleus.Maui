﻿namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusNavigationParameters"/> holds keys used in the navigation parameters for Nucleus functionality.
/// </summary>
public static class NucleusNavigationParameters
{
    /// <summary>
    /// Allows to display a page using a custom <see cref="PresentationMode"/>.
    /// </summary>
    public const string NavigatingPresentationMode = "NavigatingPresentationMode";

    /// <summary>
    /// Allows to wrap a page in a <see cref="NavigationPage"/> before presenting, which is used for modal presentation.
    /// </summary>
    public const string WrapInNavigationPage = "WrapInNavigationPage";

    /// <summary>
    /// When set this will make a navigation request always process, even though it would otherwise be ignored
    /// due to <see cref="NucleusMvvmOptions.IgnoreNavigationWhenInProgress"/> or
    /// <see cref="NucleusMvvmOptions.IgnoreNavigationWithinMilliseconds"/>.
    /// </summary>
    public const string DoNotIgnoreThisNavigationRequest = "DoNotIgnoreThisNavigationRequest";

    /// <summary>
    /// This key is automatically added to the parameters during navigation and holds the value of the
    /// <see cref="Microsoft.Maui.Controls.ShellNavigationSource"/> property. Do not access the key
    /// directly, but through <see cref="DictionaryExtensions.GetShellNavigationSource(IDictionary{string, object})"/>.
    /// </summary>
    public const string ShellNavigationSource = "ShellNavigationSource";
}
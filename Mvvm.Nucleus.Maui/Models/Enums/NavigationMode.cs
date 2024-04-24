namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NavigationDirection"/> details if a navigation is done to a deeper page or a previous one.
/// </summary>
public enum NavigationDirection
{
    /// <summary>
    /// Navigating backwards (popping).
    /// </summary>
    Back,

    /// <summary>
    /// Navigating forwards (pushing).
    /// </summary>
    Forwards
}
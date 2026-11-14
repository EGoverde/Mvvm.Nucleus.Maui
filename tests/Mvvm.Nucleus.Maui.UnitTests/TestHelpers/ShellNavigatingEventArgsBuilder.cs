namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Raises <see cref="Shell.Navigating"/> with a constructed <see cref="ShellNavigatingEventArgs"/> — the entry
/// point for the confirm-navigation tests. Raising <c>Shell.Navigating</c> flows through
/// <see cref="NucleusMvvmCore"/> (which hooks it when the <c>Shell</c> is registered) into the
/// <see cref="NavigationService"/> handler.
///
/// The <see cref="ShellNavigatingEventArgs"/> constructor (MAUI 11) is public:
/// <c>(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)</c>.
/// Only the event invocation itself needs reflection, because MAUI 11 keeps the event delegate in a
/// same-name backing field (no public <c>Raise</c> method).
/// </summary>
public static class ShellNavigatingEventArgsBuilder
{
    /// <summary>
    /// Constructs a <see cref="ShellNavigatingEventArgs"/>.
    /// </summary>
    /// <param name="currentLocation">The location the navigation starts from.</param>
    /// <param name="targetLocation">The location the navigation is targeting.</param>
    /// <param name="source">The <see cref="ShellNavigationSource"/> (e.g. <c>Push</c>, <c>Pop</c>, <c>PopToRoot</c>).</param>
    /// <param name="canCancel">Whether the navigation is cancellable (as MAUI sets it for push/pop requests).</param>
    public static ShellNavigatingEventArgs Create(
        string currentLocation,
        string targetLocation,
        ShellNavigationSource source,
        bool canCancel = true)
    {
        return new ShellNavigatingEventArgs(
            new ShellNavigationState(currentLocation),
            new ShellNavigationState(targetLocation),
            source,
            canCancel);
    }

    /// <summary>
    /// Raises <see cref="Shell.Navigating"/> on the given shell with the given event-args
    /// (and returns the event-args, so tests can inspect <see cref="ShellNavigatingEventArgs.Cancelled"/>).
    /// </summary>
    public static ShellNavigatingEventArgs RaiseNavigating(
        Shell shell,
        string currentLocation,
        string targetLocation,
        ShellNavigationSource source,
        bool canCancel = true)
    {
        var eventArgs = Create(currentLocation, targetLocation, source, canCancel);

        var handler = shell
            .GetEventDelegate(nameof(Shell.Navigating))
            ?? throw new InvalidOperationException("No delegate registered for the 'Navigating' event; is NucleusMvvmCore (or a NavigationService) subscribed?");

        handler.DynamicInvoke(shell, eventArgs);

        return eventArgs;
    }
}

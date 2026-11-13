namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Raises the <see cref="NucleusMvvmCore"/> application lifecycle events (<c>AppResumed</c>/
/// <c>AppStopped</c>). Events may only be invoked from within the declaring assembly, so the
/// backing-field delegate is invoked via reflection — the same pattern as <see cref="PageEvents"/>.
/// </summary>
public static class CoreEvents
{
    /// <summary>
    /// Raises <see cref="NucleusMvvmCore.AppResumed"/>.
    /// </summary>
    public static void AppResumed(NucleusMvvmCore core)
        => Raise(core, nameof(NucleusMvvmCore.AppResumed));

    /// <summary>
    /// Raises <see cref="NucleusMvvmCore.AppStopped"/>.
    /// </summary>
    public static void AppStopped(NucleusMvvmCore core)
        => Raise(core, nameof(NucleusMvvmCore.AppStopped));

    private static void Raise(NucleusMvvmCore core, string eventName)
    {
        // The delegate may be null when nothing is subscribed — raising is then a no-op.
        if (core.GetEventDelegate(eventName) is not Delegate handler)
        {
            return;
        }

        handler.DynamicInvoke(core, EventArgs.Empty);
    }
}

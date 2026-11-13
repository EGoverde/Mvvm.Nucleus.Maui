using System.Reflection;

namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Deterministic isolation for the <see cref="NavigationService"/> debounce tests.
/// <see cref="NavigationService"/> keeps a process-wide static
/// (<c>_isNavigatingLastTriggeredUtc</c>) that would otherwise leak timing state between tests;
/// resetting it (instead of sleeping) keeps the matrix order-independent and flake-free.
/// </summary>
public static class NavigationServiceStateReset
{
    private static FieldInfo LastTriggeredField
    {
        get
        {
            return typeof(NavigationService)
                .GetField("_isNavigatingLastTriggeredUtc", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Static field '_isNavigatingLastTriggeredUtc' not found on NavigationService.");
        }
    }

    /// <summary>
    /// Resets the shared static debounce timestamp back to <see cref="DateTime.MinValue"/>
    /// (i.e. "no previous navigation"), as a freshly loaded process would have.
    /// </summary>
    public static void ResetLastTriggeredUtc()
    {
        LastTriggeredField.SetValue(null, default(DateTime));
    }

    /// <summary>
    /// Sets the shared static debounce timestamp to a specific value, e.g. to simulate a
    /// previous navigation that happened far enough in the past to be outside the time window.
    /// </summary>
    public static void SetLastTriggeredUtc(DateTime utc)
    {
        LastTriggeredField.SetValue(null, utc);
    }

    /// <summary>
    /// Resets the instance-level <c>_isNavigating</c> flag (the <see cref="NavigationService.IsNavigating"/>
    /// setter is private; assigning the field directly also avoids raising <c>IsNavigatingChanged</c>).
    /// </summary>
    public static void ResetIsNavigating(NavigationService navigationService)
    {
        var field = typeof(NavigationService)
            .GetField("_isNavigating", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Instance field '_isNavigating' not found on NavigationService.");

        field.SetValue(navigationService, false);
    }
}

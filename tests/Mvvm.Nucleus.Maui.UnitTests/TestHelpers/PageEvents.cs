using System.Reflection;

namespace Mvvm.Nucleus.Maui.UnitTests.TestHelpers;

/// <summary>
/// Raises <see cref="Page"/> lifecycle events from outside the MAUI assembly.
///
/// The <c>NavigatedTo</c>/<c>NavigatedFrom</c>/<c>NavigatingFrom</c> event-args types have
/// non-public <c>(Page, NavigationType)</c> constructors in MAUI 11 (they are only ever raised
/// by MAUI internals), so they are constructed via reflection.
///
/// MAUI 11 exposes the event backing fields under the same name as the event (not the
/// compiler-generated <c>&lt;Event&gt;k__BackingField</c>), so the delegate is resolved for both
/// shapes before it is invoked.
/// </summary>
public static class PageEvents
{
    /// <summary>
    /// Raises <see cref="Page.Appearing"/>.
    /// </summary>
    public static void Appearing(Page page)
        => Raise(page, nameof(Page.Appearing), EventArgs.Empty);

    /// <summary>
    /// Raises <see cref="Page.Disappearing"/>.
    /// </summary>
    public static void Disappearing(Page page)
        => Raise(page, nameof(Page.Disappearing), EventArgs.Empty);

    /// <summary>
    /// Raises <see cref="Page.NavigatedTo"/>.
    /// </summary>
    public static void NavigatedTo(Page page, NavigationType navigationType = NavigationType.Push)
        => Raise(page, nameof(Page.NavigatedTo), CreateNavigationEventArgs(typeof(NavigatedToEventArgs), page, navigationType));

    /// <summary>
    /// Raises <see cref="Page.NavigatedFrom"/>.
    /// </summary>
    public static void NavigatedFrom(Page page, NavigationType navigationType = NavigationType.Pop)
        => Raise(page, nameof(Page.NavigatedFrom), CreateNavigationEventArgs(typeof(NavigatedFromEventArgs), page, navigationType));

    /// <summary>
    /// Raises <see cref="Page.NavigatingFrom"/>.
    /// </summary>
    public static void NavigatingFrom(Page page, NavigationType navigationType = NavigationType.Pop)
        => Raise(page, nameof(Page.NavigatingFrom), CreateNavigationEventArgs(typeof(NavigatingFromEventArgs), page, navigationType));

    private static void Raise(Page page, string eventName, object eventArgs)
    {
        var handler = GetEventDelegate(page, eventName)
            ?? throw new InvalidOperationException($"No delegate registered for event '{eventName}' on {page.GetType().Name}; is the behavior under test attached?");

        handler.DynamicInvoke(page, eventArgs);
    }

    private static object CreateNavigationEventArgs(Type eventType, Page page, NavigationType navigationType)
    {
        var ctor = eventType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == 2
                    && parameters[0].ParameterType == typeof(Page)
                    && parameters[1].ParameterType == typeof(NavigationType);
            })
            ?? throw new InvalidOperationException($"No (Page, NavigationType) constructor found on {eventType}.");

        return ctor.Invoke([page, navigationType]);
    }

    /// <summary>
    /// Gets the current delegate of the given event on the given element (MAUI 11 names its backing
    /// fields after the event itself; auto-generated events use the <c>k__BackingField</c> shape).
    ///
    /// The fields are <i>private</i> on the declaring base class (e.g. <c>Page</c>), and
    /// <see cref="Type.GetField"/> does not surface non-public fields of base types, so the
    /// type hierarchy is walked from the runtime type up to <see cref="object"/>.
    /// </summary>
    internal static Delegate? GetEventDelegate(this object element, string eventName)
    {
        for (var type = element.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField($"<{eventName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
            {
                return field.GetValue(element) as Delegate;
            }
        }

        throw new InvalidOperationException($"No backing field for event '{eventName}' found on {element.GetType()} or its base types.");
    }
}

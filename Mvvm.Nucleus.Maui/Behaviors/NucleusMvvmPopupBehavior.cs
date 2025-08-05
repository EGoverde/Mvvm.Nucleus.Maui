using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmPageBehavior"/> is a <see cref="Behavior"/> added to all views created by Nucleus
/// which will register the various events used by the interfaces.
/// </summary>
public class NucleusMvvmPopupBehavior : Behavior
{
    /// <summary>
    /// The <see cref="Popup"/> this behavior is attached to and which events are used.
    /// </summary>
    public Popup? Popup { get; set; }

    /// <inheritdoc/>
    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);

        Popup!.ParentChanged += OnParentChanged;
        Popup!.Opened += OnPopupOpened!;
        Popup!.Closed += OnPopupClosed!;
    }

    /// <inheritdoc/>
    protected override void OnDetachingFrom(BindableObject bindable)
    {
        base.OnDetachingFrom(bindable);

        Popup!.ParentChanged -= OnParentChanged;
        Popup!.Opened -= OnPopupOpened!;
        Popup!.Closed -= OnPopupClosed!;
    }
    private void OnPopupOpened(object? sender, EventArgs e)
    {
        // Add any logic needed when the popup is opened.
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        // Add any logic needed when the popup is closed.
    }

    private void OnParentChanged(object? sender, EventArgs e)
    {
        // Add any logic needed when the parent of the popup changes.
    }
}
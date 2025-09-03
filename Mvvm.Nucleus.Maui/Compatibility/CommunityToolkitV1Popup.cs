using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// A compatibility class that can be used as an intermediate step when migrating from the original CommunityToolkit
/// <see cref="Popup"/>. It can be used in combination with <see cref="CommunityToolkitV1PopupService"/>. It is recommended
/// to migrate to the new <see cref="PopupService"/> as soon as possible.
/// </summary>
[Obsolete("This class is only for compatibility with the original CommunityToolkit.Maui.Popup and will be removed in future versions. Use a Popup<T> instead when requiring a return value.")]
public class CommunityToolkitV1Popup : Popup<object?>
{
    /// <summary>
    /// Please use <see cref="IPopupResult.WasDismissedByTappingOutsideOfPopup"/> instead,
    /// or rely on the <see cref="IPopupService"/> functions with a default value as parameter.
    /// </summary>
    [Obsolete("Use 'WasDismissedByTappingOutsideOfPopup' instead, or rely on the IPopupService functions with a default value as parameter.")]
    protected object? ResultWhenUserTapsOutsideOfPopup { get; set; }
}
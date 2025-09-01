using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui.Compatibility;

public class CommunityToolkitV1Popup : Popup<object?>
{
    /// <summary>
    /// This value is no longer used, please use <see cref="IPopupResult.WasDismissedByTappingOutsideOfPopup"/> instead.
	/// </summary>
    [Obsolete("This value is no longer used. Rely on 'WasDismissedByTappingOutsideOfPopup' instead.")]
	protected object? ResultWhenUserTapsOutsideOfPopup { get; set; }
}
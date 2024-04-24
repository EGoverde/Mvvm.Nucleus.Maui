namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewRouteType"/> defines whether a registration is absolute (e.a. in AppShell.xaml) or global.
/// </summary>
public enum ViewRouteType
{
	/// <summary>
	/// Absolute routes are defined in AppShell.xaml.
	/// </summary>
	AbsoluteRoute = 0,

	/// <summary>
	/// Global routes are used for pages that can be presented from any starting point.
	/// </summary>
	GlobalRoute = 1,
}
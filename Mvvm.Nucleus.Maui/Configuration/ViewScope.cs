namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewScope"/> defines the scope used in an IoC registration.
/// </summary>
public enum ViewScope
{
	/// <summary>
	/// Transient registrations resolve to a new instance on every request.
	/// </summary>
	Transient = 0,

	/// <summary>
	/// Transient registrations resolve to a new instance per scope.
	/// </summary>
	Scoped = 1,

	/// <summary>
	/// Singleton registrations resolve to always a singular instance.
	/// </summary>
	Singleton = 2
}
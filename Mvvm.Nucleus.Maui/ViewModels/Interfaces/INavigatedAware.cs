namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="INavigatedAware"/> handles the navigation events of <see cref="Shell"/>.
/// </summary>
public interface INavigatedAware
{
	/// <summary>
	/// Triggered when a <see cref="Page"/> has been navigated to.
	/// </summary>
	/// <param name="navigationParameters">The navigation parameters.</param>
	void OnNavigatedTo(IDictionary<string, object> navigationParameters);

	/// <summary>
	/// Triggered when a <see cref="Page"/> has been navigated from.
	/// </summary>
	/// <param name="navigationParameters">The navigation parameters.</param>
	void OnNavigatedFrom(IDictionary<string, object> navigationParameters);

	/// <summary>
	/// Triggered when a <see cref="Page"/> is going to be navigated from.
	/// </summary>
	/// <param name="navigationParameters">The navigation parameters.</param>
	void OnNavigatingFrom(IDictionary<string, object> navigationParameters);
}
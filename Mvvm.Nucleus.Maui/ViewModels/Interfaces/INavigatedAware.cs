namespace Mvvm.Nucleus.Maui;

public interface INavigatedAware
{
	void OnNavigatedTo(IDictionary<string, object> navigationParameters);

	void OnNavigatedFrom(IDictionary<string, object> navigationParameters);

	void OnNavigatingFrom(IDictionary<string, object> navigationParameters);
}
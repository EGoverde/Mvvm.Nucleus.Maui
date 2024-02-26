namespace Mvvm.Nucleus.Maui
{
    public interface IPageLifecycleAware
	{
        void OnAppearing();

        void OnDisappearing();

        void OnFirstAppearing();
    }
}
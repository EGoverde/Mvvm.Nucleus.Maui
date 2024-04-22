namespace Mvvm.Nucleus.Maui;

public interface IApplicationLifecycleAware
{
    void OnSleep();

    void OnResume();
}
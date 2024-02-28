namespace Mvvm.Nucleus.Maui;

public interface IInitializableAsync
{
    Task InitAsync(IDictionary<string, object> navigationParameters);

    Task RefreshAsync(IDictionary<string, object> navigationParameters);
}
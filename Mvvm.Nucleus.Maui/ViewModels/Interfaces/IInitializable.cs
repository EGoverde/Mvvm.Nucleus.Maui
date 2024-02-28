namespace Mvvm.Nucleus.Maui;

public interface IInitializable
{
    void Init(IDictionary<string, object> navigationParameters);

    void Refresh(IDictionary<string, object> navigationParameters);
}
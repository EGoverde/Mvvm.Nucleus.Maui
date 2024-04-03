namespace Mvvm.Nucleus.Maui;

public interface IPopupInitializableAsync
{
    public bool AwaitInitializeBeforeShowing { get; }

    Task InitAsync(IDictionary<string, object> navigationParameters);
}
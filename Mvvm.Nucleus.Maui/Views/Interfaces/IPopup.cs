namespace Mvvm.Nucleus.Maui;

public interface IPopup<TResult>
{
    public TResult ResultWhenUserTapsOutsideOfPopup { get; set; }
}

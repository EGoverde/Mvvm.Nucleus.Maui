namespace Mvvm.Nucleus.Maui
{
    public interface IViewFactory
    {
        T? CreateView<T>() where T : Element;

        object CreateView(Type viewType);

        object ConfigureView(Element element);
    }
}
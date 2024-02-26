namespace Mvvm.Nucleus.Maui
{
    public static class NucleusMvvmMarkupExtensions
	{
        public static T NucleusConfigureView<T>(this T element, bool nucleusConfigureView) where T : VisualElement
        {
            element.SetValue(NucleusMvvm.NucleusConfigureViewProperty, nucleusConfigureView);

            return element;
        }
    }
}
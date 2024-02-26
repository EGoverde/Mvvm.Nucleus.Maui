namespace Mvvm.Nucleus.Maui.Sample;

public partial class Details : ContentPage
{
	public Details()
	{
		InitializeComponent();

		ViewIdentifierSpan.Text = Guid.NewGuid().ToString();
	}
}
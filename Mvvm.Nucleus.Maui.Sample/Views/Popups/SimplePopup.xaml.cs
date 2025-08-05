using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class SimplePopup : Popup, IPopupInitializable
{
	public SimplePopup()
	{
		InitializeComponent();

		// ResultWhenUserTapsOutsideOfPopup = "Dismissed using tap.";
        // CloseButton.Clicked += (obj, e) => Close("Closed using Button.");
	}

    public void Init(IDictionary<string, object> navigationParameters)
    {
        TextLabel.Text = navigationParameters.GetValueOrDefault("Text", "Sample Text");
    }
}
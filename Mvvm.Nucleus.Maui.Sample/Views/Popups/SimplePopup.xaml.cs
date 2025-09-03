using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui.Sample;

public partial class SimplePopup : ContentView, IPopupAware, IPopupInitializable
{
	public SimplePopup(IPopupService popupService)
	{
		InitializeComponent();

        CloseButtonService.Clicked += (obj, e) =>
        {
            _ = popupService.CloseMostRecentPopupAsync();
        };

        CloseButtonPopupAware.Clicked += (obj, e) =>
        {
            if (Popup?.TryGetTarget(out var popupInstance) == true)
            {
                _ = popupInstance.CloseAsync();
            }
        };
	}

    public WeakReference<Popup>? Popup { get; set; }

    public void Init(IDictionary<string, object> navigationParameters)
    {
        TextLabel.Text = navigationParameters.GetValueOrDefault("Text", "Sample Text");
    }
}
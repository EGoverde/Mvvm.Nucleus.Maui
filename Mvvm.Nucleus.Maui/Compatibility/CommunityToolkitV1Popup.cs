using CommunityToolkit.Maui.Views;

namespace Mvvm.Nucleus.Maui.Compatibility;

/// <summary>
/// A compatibility class that can be used as an intermediate step when migrating from the original CommunityToolkit
/// <see cref="Popup"/>. It can be used in combination with <see cref="CommunityToolkitV1PopupService"/>. It is recommended
/// to migrate to the new <see cref="PopupService"/> as soon as possible.
/// </summary>
[Obsolete("This class is only for compatibility with the original CommunityToolkit.Maui.Popup and will be removed in future versions. Use a Popup<T> instead when requiring a return value.")]
public class CommunityToolkitV1Popup : Popup<object?>
{
    /// <summary>
    ///  Backing BindableProperty for the <see cref="Color"/> property.
    /// </summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(Popup), Colors.LightGray, propertyChanged: OnColorChanged);

    /// <summary>
    ///  Backing BindableProperty for the <see cref="Size"/> property.
    /// </summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(Size), typeof(Popup), default(Size), propertyChanged: OnSizeChanged);

    /// <summary>
	/// Gets or sets the result that will return when the user taps outside the Popup.
	/// </summary>
    [Obsolete("Use 'WasDismissedByTappingOutsideOfPopup' instead, or rely on the IPopupService functions with a default value as parameter.")]
    protected object? ResultWhenUserTapsOutsideOfPopup { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Color"/> of the Popup.
    /// </summary>
    /// <remarks>
    /// This color sets the native background color of the <see cref="Popup"/>, which is
    /// independent of any background color configured in the actual View.
    /// </remarks>
    [Obsolete("Use the 'BackgroundColor' property of the Popup instead.")]
    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
	/// Gets or sets the <see cref="Size"/> of the Popup Display.
	/// </summary>
	/// <remarks>
	/// The Popup will always try to constrain the actual size of the <see cref="Popup" />
	/// to the <see cref="Popup" /> of the View unless a <see cref="Size"/> is specified.
	/// If the <see cref="Popup" /> contains <see cref="LayoutOptions"/> a <see cref="Size"/>
	/// will be required. This will allow the View to have a concept of <see cref="Size"/>
	/// that varies from the actual <see cref="Size"/> of the <see cref="Popup" />
	/// </remarks>
	public Size Size
    {
        get => (Size)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunityToolkitV1Popup"/> class.
    /// </summary>
    public CommunityToolkitV1Popup()
    {
        OnColorChanged(this, default, Color);
        OnSizeChanged(this, default, Size);
    }

    private static void OnColorChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is CommunityToolkitV1Popup popup && newValue is Color color)
        {
            popup.BackgroundColor = color;
        }
    }
    
    private static void OnSizeChanged(BindableObject bindable, object? oldValue, object? newValue)
	{
        if (bindable is CommunityToolkitV1Popup popup && newValue is Size size && !size.IsZero)
        {
            popup.WidthRequest = size.Width;
            popup.HeightRequest = size.Height;
        }
	}
}
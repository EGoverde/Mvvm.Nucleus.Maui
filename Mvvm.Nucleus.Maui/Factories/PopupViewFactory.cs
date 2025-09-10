using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupViewFactory"/> is the default implementation for <see cref="IPopupViewFactory"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PopupViewFactory"/> class.
/// </remarks>
/// <param name="logger">The <see cref="ILogger"/>.</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
/// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
public class PopupViewFactory(ILogger<ViewFactory> logger, IServiceProvider serviceProvider, NucleusMvvmOptions nucleusMvvmOptions) : IPopupViewFactory
{
    private readonly ILogger _logger = logger;

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private readonly NucleusMvvmOptions _nucleusMvvmOptions = nucleusMvvmOptions;

    /// <inheritdoc/>
    public object CreateView(Type viewType)
    {
        var viewObject = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
        if (viewObject is not View view)
        {
            return viewObject;
        }

        return ConfigureView(view);
    }

    /// <inheritdoc/>
    public object ConfigureView(View view)
    {
        var popupMapping = _nucleusMvvmOptions
            .PopupMappings
            .FirstOrDefault(x => x.PopupViewType == view.GetType());

        if (popupMapping != null && popupMapping.PopupViewModelType != null && view.BindingContext == null)
        {
            view.BindingContext = ActivatorUtilities.CreateInstance(_serviceProvider, popupMapping!.PopupViewModelType!);
        }

        if (view is Popup popup)
        {
            popup.Behaviors.Add(new NucleusMvvmPopupBehavior { Popup = popup, Element = view });
        }
        else
        {
            view.ListenToParentChanges(typeof(Popup), (rootElement) =>
            {
                if (rootElement is not Popup popup)
                {
                    return false;
                }

                popup.Behaviors.Add(new NucleusMvvmPopupBehavior { Popup = popup, Element = view });

                return true;
            });
        }

        return view;
    }

    internal static void ListenToParentChanges(Element element,  Func<Element, bool> shouldStopListening)
    {
        void OnParentChanged(object sender, EventArgs args)
        {
            if (sender is not Element element)
            {
                return;
            }

            var rootElement = element.Parent.GetRootElement(typeof(Popup));
            if (rootElement == null)
            {
                return;
            }

            element.ParentChanged -= OnParentChanged!;

            if (shouldStopListening(rootElement))
            {
                return;
            }

            rootElement.ParentChanged += OnParentChanged!;
        }

        element.ParentChanged += OnParentChanged!;
    }
}
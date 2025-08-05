using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="PopupViewFactory"/> is the default implementation for <see cref="IPopupViewFactory"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class PopupViewFactory : IPopupViewFactory
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly NucleusMvvmOptions _nucleusMvvmOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupViewFactory"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
    public PopupViewFactory(ILogger<ViewFactory> logger, IServiceProvider serviceProvider, NucleusMvvmOptions nucleusMvvmOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _nucleusMvvmOptions = nucleusMvvmOptions;
    }

    /// <inheritdoc/>
    public object CreateView(Type viewType)
    {
        var view = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
        if (view is not Element element)
        {
            return view;
        }

        return ConfigureView(element);
    }

    /// <inheritdoc/>
    public object ConfigureView(Element element)
    {
        var popupMapping = _nucleusMvvmOptions
            .PopupMappings
            .FirstOrDefault(x => x.PopupViewType == element.GetType());

        if (popupMapping != null && !popupMapping.IsWithoutViewModel && element.BindingContext == null)
        {
            element.BindingContext = ActivatorUtilities.CreateInstance(_serviceProvider, popupMapping!.PopupViewModelType!);
        }

        if (element is Popup popup)
        {
            // Already a popup.
        }
        else
        {
            // Needs to be addded to a popup.
            // ListenToParentChanges(element);
        }

        return element;
    }

    private void ListenToParentChanges(Element createdElement)
    {
        void OnParentChanged(object sender, EventArgs args)
        {
            if (sender is not Element element)
            {
                return;
            }

            element.ParentChanged -= OnParentChanged!;

            var rootElement = GetRootElement(element.Parent);
            if (rootElement == null)
            {
                return;
            }

            if (rootElement is Page page)
            {
                page.Behaviors.Add(new NucleusMvvmPageBehavior { Page = page, Element = createdElement });
                return;
            }

            rootElement.ParentChanged += OnParentChanged!;
        }

        createdElement.ParentChanged += OnParentChanged!;
    }

    private static Element? GetRootElement(Element element)
    {
        var rootElement = element?.Parent ?? element;
        var parentElement = element?.Parent;

        while (parentElement != null)
        {
            rootElement = parentElement;

            if (rootElement is Page)
            {
                break;
            }

            parentElement = rootElement.Parent;
        }

        return rootElement;
    }
}
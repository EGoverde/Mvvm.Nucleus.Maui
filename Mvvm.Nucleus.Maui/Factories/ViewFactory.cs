using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="ViewFactory"/> is the default implementation for <see cref="IViewFactory"/>.
/// It can be customized through inheritence and registering the service before initializing Nucleus.
/// </summary>
public class ViewFactory : IViewFactory
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly NucleusMvvmOptions _nucleusMvvmOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewFactory"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
    public ViewFactory(ILogger<ViewFactory> logger, IServiceProvider serviceProvider, NucleusMvvmOptions nucleusMvvmOptions)
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
        var viewMapping = _nucleusMvvmOptions
            .ViewMappings
            .FirstOrDefault(x => x.ViewType == element.GetType());

        if (viewMapping != null && element.BindingContext == null)
        {
            element.BindingContext = ActivatorUtilities.CreateInstance(_serviceProvider, viewMapping!.ViewModelType);
        }

        if (element is Window || element is Shell)
        {
            return element;
        }

        if (element is Page page)
        {
            if (!page.Behaviors.Any(x => x is NucleusMvvmPageBehavior))
            {
                page.Behaviors.Add(new NucleusMvvmPageBehavior { Page = page });
            }

            var navigationParameters = NucleusMvvmCore.Current.NavigationParameters ?? new Dictionary<string, object>();

            PresentationMode? presentationMode = null;
            var wrapNavigationPage = false;

            if (navigationParameters.TryGetValue(NucleusNavigationParameters.NavigatingPresentationMode, out object? valueAsPresentationMode) && valueAsPresentationMode is PresentationMode)
            {
                presentationMode = (PresentationMode)valueAsPresentationMode!;
            }

            if (navigationParameters.TryGetValue(NucleusNavigationParameters.WrapInNavigationPage, out object? valueAsBool) && valueAsBool is bool)
            {
                wrapNavigationPage = (bool)valueAsBool!;
            }

            if (presentationMode != null)
            {
                Shell.SetPresentationMode(page, presentationMode.Value);
            }

            if (wrapNavigationPage == true)
            {
                element = new NavigationPage(page);

                if (presentationMode != null)
                {
                    Shell.SetPresentationMode(element, presentationMode.Value);
                }
            }
        }
        else
        {
            ViewFactory.ListenToParentChanges(element);
        }

        return element;
    }

    private static void ListenToParentChanges(Element element)
    {
        static void OnParentChanged(object sender, EventArgs args)
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
                page.Behaviors.Add(new NucleusMvvmPageBehavior { Page = page, Element = element });
                return;
            }

            rootElement.ParentChanged += OnParentChanged!;
        }

        element.ParentChanged += OnParentChanged!;
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
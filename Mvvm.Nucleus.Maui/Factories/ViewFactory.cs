using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class ViewFactory : IViewFactory
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly NucleusMvvmOptions _nucleusMvvmOptions;

        public ViewFactory(ILogger<ViewFactory> logger, IServiceProvider serviceProvider, NucleusMvvmOptions nucleusMvvmOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _nucleusMvvmOptions = nucleusMvvmOptions;
        }

        public T? CreateView<T>() where T : Element
        {
            return CreateView(typeof(T)) as T;
        }

        public object CreateView(Type viewType)
        {
            var view = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
            if (view is not Element element)
            {
                return view;
            }

            return ConfigureView(element);
        }

        public object ConfigureView(Element element)
        {
            var viewMapping = _nucleusMvvmOptions
                .ViewMappings
                .FirstOrDefault(x => x.ViewType == element.GetType());

            var setBindingContext = viewMapping != null &&
                element.BindingContext == null &&
                (!element.IsSet(NucleusMvvm.NucleusConfigureViewProperty) ||
                (element.GetValue(NucleusMvvm.NucleusConfigureViewProperty) is bool nucleusConfigureView && nucleusConfigureView));

            if (setBindingContext)
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

                var presentationMode = NucleusMvvmCore.Current.NavigatingPresentationMode;
                var wrapNavigationPage = NucleusMvvmCore.Current.NavigatingWrapNavigationPage;

                NucleusMvvmCore.Current.NavigatingPresentationMode = null;
                NucleusMvvmCore.Current.NavigatingWrapNavigationPage = null;

                if (presentationMode != null)
                {
                    Shell.SetPresentationMode(page, presentationMode.Value);
                }

                if (wrapNavigationPage == true)
                {
                    element = new NavigationPage(page);;

                    if (presentationMode != null)
                    {
                        Shell.SetPresentationMode(element, presentationMode.Value);
                    }
                }
            }
            else
            {
                ListenToParentChanges(element);
            }

            return element;
        }

        private void ListenToParentChanges(Element createdElement)
        {
            void OnParentChanged(object sender, EventArgs args)
            {
                var element = sender as Element;
                if (element == null)
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

        private Element? GetRootElement(Element element)
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
}
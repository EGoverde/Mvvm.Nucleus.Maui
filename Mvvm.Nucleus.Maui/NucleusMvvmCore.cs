using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="NucleusMvvmCore"/> is a singleton class that keeps track of various events. It also contains
/// state properties that can be accessed by other parts of Nucleus (or by custom logic).
/// </summary>
public class NucleusMvvmCore
{
    private static NucleusMvvmCore? _current;

    private IDictionary<string, object> _navigationParameters = new Dictionary<string, object>();

    internal ILogger<NucleusMvvmCore>? Logger { get; }

    internal NucleusMvvmOptions NucleusMvvmOptions { get; }

    internal IDictionary<string, object> NavigationParameters
    {
        get => _navigationParameters;
        set => _navigationParameters = value ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets the instance of Nucleus. It needs to have finishing initializing before it can be used.
    /// </summary>
    public static NucleusMvvmCore Current
    {
        get =>  _current ?? throw new InvalidOperationException("NucleusMvvm has not yet been initialized.");
        private set => _current = value;
    }

    private Shell? _shell;

    private Window? _window;

    internal event EventHandler<ShellNavigatedEventArgs>? ShellNavigated;

    internal event EventHandler<ShellNavigatingEventArgs>? ShellNavigating;

    /// <summary>
    /// An event that is triggered when the app is sent to the background.
    /// </summary>
    public event EventHandler? AppStopped;

    /// <summary>
    /// An event that is triggered when the app returns from the background.
    /// </summary>
    public event EventHandler? AppResumed;

    /// <summary>
    /// Gets the current <see cref="Application"/>.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> for IoC purposes.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Gets the <see cref="IViewFactory"/> used to create Views and ViewModels.
    /// </summary>
    public IViewFactory? ViewFactory { get; }

    /// <summary>
    /// Gets the current <see cref="Shell"/>. This property will automatically be updated if a new <see cref="Shell"/> is presented.
    /// </summary>
    public Shell? Shell
    {
        get => _shell ?? throw new InvalidOperationException("NucleusMvvm could not detect a Shell. Set the MainPage to a Shell before using any Shell-related function, such as navigating.");
        private set => RegisterShell(value);
    }

    /// <summary>
    /// Gets the <see cref="Window"/>. This will be created through the default Nucleus <see cref="WindowCreator"/>, unless overriden. Proper multi-window
    /// support is not yet available, so only one <see cref="Window"/> will ever be created.
    /// </summary>
    public Window? Window
    {
        get => _window ?? throw new InvalidOperationException("NucleusMvvm could not detect a Window.");
        internal set => RegisterWindow(value);
    }

    /// <summary>
    /// Gets the current <see cref="Page"/>. It takes into account modally presented pages, as well as pages like <see cref="FlyoutPage"/>.
    /// </summary>
    public Page CurrentPage => GetCurrentPage(Window?.Page ?? throw new InvalidOperationException("NucleusMvvm could not detect the current page."));

    /// <summary>
    /// Initializes a new instance of the <see cref="NucleusMvvmCore"/> class.
    /// </summary>
    /// <param name="application">The <see cref="Application"/>.</param>
    /// <param name="viewFactory">The <see cref="IViewFactory"/>.</param>
    /// <param name="nucleusMvvmOptions">The <see cref="NucleusMvvmOptions"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public NucleusMvvmCore(Application application, IViewFactory viewFactory, NucleusMvvmOptions nucleusMvvmOptions, ILogger<NucleusMvvmCore> logger)
    {
        Application = application;
        ViewFactory = viewFactory;
        NucleusMvvmOptions = nucleusMvvmOptions;
        Logger = logger;
    }

    internal void Initialize(IServiceProvider serviceProvider)
    {
        Current = this;
        ServiceProvider = serviceProvider;
    }

    internal async void RunTaskInVoidAndTrackException(Func<Task> task, Action? onFinished = null, [CallerMemberName] string callerName = "")
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            Logger?.LogCritical(ex, $"Failed to run async method '{callerName}' in '{GetType()}', with exception: {ex.Message}.");
        }
        finally
        {
            onFinished?.Invoke();
        }
    }

    private void RegisterShell(Shell? shell)
    {
        if (_shell == shell)
        {
            return;
        }

        if (_shell != null)
        {
            _shell.Navigated -= OnShellNavigated!;
            _shell.Navigating -= OnShellNavigating!;
        }

        _shell = shell;

        if (shell != null)
        {
            shell.Navigating += OnShellNavigating!;
            shell.Navigated += OnShellNavigated!;

            Logger?.LogInformation("Set or updated NucleusMvvm Shell reference.");
        }
    }

    private void RegisterWindow(Window? window)
    {
        if (_window == window)
        {
            return;
        }

        if (_window != null)
        {
            _window.Stopped -= OnAppStopped!;
            _window.Resumed -= OnAppResumed!;
            _window.PropertyChanged -= OnWindowPropertyChanged!;

        }

        _window = window;

        if (window != null)
        {
            window.Stopped += OnAppStopped!;
            window.Resumed += OnAppResumed!;
            window.PropertyChanged += OnWindowPropertyChanged!;

            Logger?.LogInformation("Set or updated NucleusMvvm Window reference.");
        }

        Shell = window?.Page as Shell;
    }

    private void OnAppStopped(object sender, EventArgs e)
    {
        Logger?.LogInformation("App is stopping.");
        AppStopped?.Invoke(sender, e);
    }

    private void OnAppResumed(object sender, EventArgs e)
    {
        Logger?.LogInformation("App is resuming.");
        AppResumed?.Invoke(sender, e);
    }

    private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
    {
        ShellNavigating?.Invoke(sender, e);
    }

    private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        ShellNavigated?.Invoke(sender, e);
    }

    private void OnWindowPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Window.Page))
        {
            Shell = (sender as Window)?.Page as Shell;
        }
    }

    private Page GetCurrentPage(Page page)
    {
        if (page?.Navigation.ModalStack.LastOrDefault() is Page modalPage)
        {
            return modalPage;
        }
        else if (page is FlyoutPage flyoutPage)
        {
            return GetCurrentPage(flyoutPage.Detail);
        }
        else if (page is Shell shell && shell.CurrentItem?.CurrentItem is IShellSectionController shellSectionController)
        {
            return shellSectionController.PresentedPage;
        }
        else if (page is IPageContainer<Page> pageContainer)
        {
            return GetCurrentPage(pageContainer.CurrentPage);
        }

        return page ?? throw new InvalidOperationException("NucleusMvvm could not detect the current page.");
    }
}
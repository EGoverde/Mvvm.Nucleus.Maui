using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class NucleusMvvmCore
    {
        private static NucleusMvvmCore? _current;

        private IDictionary<string, object> _navigationParameters = new Dictionary<string, object>();

        internal ILogger<NucleusMvvmCore>? Logger { get; }

        internal IDictionary<string, object> NavigationParameters
        {
            get => _navigationParameters;
            set => _navigationParameters = value ?? new Dictionary<string, object>();
        }

        public static NucleusMvvmCore Current
        {
            get =>  _current ?? throw new InvalidOperationException("NucleusMvvm has not yet been initialized.");
            private set => _current = value;
        }

        private Shell? _shell;

        private Window? _window;

        internal event EventHandler<ShellNavigatedEventArgs>? ShellNavigated;

        internal event EventHandler<ShellNavigatingEventArgs>? ShellNavigating;

        public event EventHandler? AppStopped;

        public event EventHandler? AppResumed;

        public Application Application { get; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public IViewFactory? ViewFactory { get; }

        public Shell? Shell
        {
            get => _shell ?? throw new InvalidOperationException("NucleusMvvm could not detect a Shell. Set the MainPage to a Shell before using any Shell-related function, such as navigating.");
            private set => RegisterShell(value);
        }

        public Window? Window
        {
            get => _window ?? throw new InvalidOperationException("NucleusMvvm could not detect a Window.");
            private set => RegisterWindow(value);
        }

        public NucleusMvvmCore(Application application, IViewFactory viewFactory, ILogger<NucleusMvvmCore> logger)
        {
            Application = application;
            ViewFactory = viewFactory;
            Logger = logger;
        }

        internal void Initialize(IServiceProvider serviceProvider)
        {
            Current = this;

            ServiceProvider = serviceProvider;

            Application.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Application.MainPage))
                {
                    Shell = (sender as Application)?.MainPage as Shell;
                    Window = (sender as Application)?.MainPage?.Window;
                }
            };
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
            }

            _window = window;

            if (window != null)
            {
                window.Stopped += OnAppStopped!;
                window.Resumed += OnAppResumed!;

                Logger?.LogInformation("Set or updated NucleusMvvm Window reference.");
            }
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
    }
}
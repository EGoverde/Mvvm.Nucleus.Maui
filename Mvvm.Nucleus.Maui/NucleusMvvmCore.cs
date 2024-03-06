using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class NucleusMvvmCore
    {
        private static NucleusMvvmCore? _current;

        private IDictionary<string, object> _navigationParameters = new Dictionary<string, object>();

        private bool _isNavigating;

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

        public EventHandler? AppStopped;

        public EventHandler? AppResumed;

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

        public bool IsNavigating
        {
            get => _isNavigating;
            internal set => _isNavigating = value;
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

            AppStopped += OnAppStopped!;
            AppResumed += OnAppResumed!;
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
                _shell.Navigating -= ShellNavigating!;
                _shell.Navigated -= ShellNavigated!;
            }

            _shell = shell;

            if (shell != null)
            {
                shell.Navigating += ShellNavigating!;
                shell.Navigated += ShellNavigated!;

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
                _window.Stopped -= AppStopped!;
                _window.Resumed -= AppResumed!;

            }

            _window = window;

            if (window != null)
            {
                window.Stopped += AppStopped!;
                window.Resumed += AppResumed!;

                Logger?.LogInformation("Set or updated NucleusMvvm Window reference.");
            }
        }

        private async void ShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            var isCanceled = false;

            if (e.CanCancel && (e.Source == ShellNavigationSource.Pop || e.Source == ShellNavigationSource.PopToRoot || e.Source == ShellNavigationSource.Push))
            {
                var navigationDirection = default(NavigationDirection);

                switch(e.Source)
                {
                    case ShellNavigationSource.Pop:
                    case ShellNavigationSource.PopToRoot:
                        navigationDirection = NavigationDirection.Back;
                        break;
                    case ShellNavigationSource.Push:
                        navigationDirection = NavigationDirection.Forwards;
                        break;
                }

                var confirmNavigation = Shell?.CurrentPage?.BindingContext as IConfirmNavigation;
                if (confirmNavigation != null && !confirmNavigation.CanNavigate(navigationDirection, NavigationParameters))
                {
                    isCanceled = true;
                    e.Cancel();
                }
                else
                {
                    var confirmNavigationAsync = Shell?.CurrentPage?.BindingContext as IConfirmNavigationAsync;
                    if (confirmNavigationAsync != null)
                    {
                        var token = e.GetDeferral();
                        
                        var confirm = await confirmNavigationAsync.CanNavigateAsync(navigationDirection, NavigationParameters);
                        if (!confirm)
                        {
                            isCanceled = true;
                            e.Cancel();
                        }

                        token.Complete();
                    }
                }
            }

            if (isCanceled)
            {
                Logger?.LogInformation($"Shell Navigation Canceled.");
                return;
            }

            Logger?.LogInformation($"Shell Navigating '{e.Current?.Location}' > '{e.Target?.Location}' ({e.Source}).");

            IsNavigating = true;
        }

        private void ShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            Logger?.LogInformation($"Shell Navigated '{e.Current?.Location}' ({e.Source}).");

            IsNavigating = false;
        }

        private void OnAppStopped(object sender, EventArgs e)
        {
            Logger?.LogInformation("App is stopping.");
        }

        private void OnAppResumed(object sender, EventArgs e)
        {
            Logger?.LogInformation("App is resuming.");
        }
    }
}
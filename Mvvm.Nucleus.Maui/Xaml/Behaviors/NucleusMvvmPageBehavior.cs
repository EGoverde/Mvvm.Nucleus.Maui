using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui
{
    public class NucleusMvvmPageBehavior : Behavior
    {
        private bool _isAppearedBefore;

        private bool _isInitializedBefore;

        private bool _isNavigatedTo;

        private bool _isNavigatedFrom;

        public Page? Page { get; set; }

        public Element? Element { get; set; }

        public bool DestroyAfterNavigatedFrom { get; set; }

        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);

            Page!.Appearing += PageAppearing!;
            Page!.Disappearing += PageDisappearing!;
            
            Page!.NavigatedTo += PageNavigatedTo!;
            Page!.NavigatedFrom += PageNavigatedFrom!;
            Page!.NavigatingFrom += PageNavigatingFrom!;
        }

        protected override void OnDetachingFrom(BindableObject bindable)
        {
            base.OnDetachingFrom(bindable);

            Page!.Appearing -= PageAppearing!;
            Page!.Disappearing -= PageDisappearing!;

            Page!.NavigatedTo -= PageNavigatedTo!;
            Page!.NavigatedFrom -= PageNavigatedFrom!;
            Page!.NavigatingFrom -= PageNavigatingFrom!;

            NucleusMvvmCore.Current.AppResumed -= AppResumed!;
            NucleusMvvmCore.Current.AppStopped -= AppStopped!;
        }

        private void PageAppearing(object sender, EventArgs e)
        {
            if (GetBindingContext() is IPageLifecycleAware pageLifecycleAware)
            {
                pageLifecycleAware.OnAppearing();

                if (!_isAppearedBefore)
                {
                    pageLifecycleAware.OnFirstAppearing();
                }
            }

            _isAppearedBefore = true;
        }

        private void PageDisappearing(object sender, EventArgs e)
        {
            if (GetBindingContext() is IPageLifecycleAware pageLifecycleAware)
            {
                pageLifecycleAware.OnDisappearing();
            }
        }

        private void PageNavigatedTo(object sender, NavigatedToEventArgs e)
        {
            if (_isNavigatedTo && !_isNavigatedFrom)
            {
                // Work-around for Shell NavigationPage + Modal, which doubly triggers the NavigatedTo event.
                return;
            }

            _isNavigatedTo = true;

            var bindingContext = GetBindingContext();
            
            var isInitializedBefore = _isInitializedBefore;
            _isInitializedBefore = true;

            if (bindingContext is INavigatedAware navigatedAware)
            {
                navigatedAware.OnNavigatedTo(NucleusMvvmCore.Current.NavigationParameters);
            }

            if (bindingContext is IInitializable initializable)
            {
                if (!isInitializedBefore)
                {
                    initializable.Init(NucleusMvvmCore.Current.NavigationParameters);
                }
                else
                {
                    initializable.Refresh(NucleusMvvmCore.Current.NavigationParameters);
                }
            }

            if (bindingContext is IInitializableAsync initializableAsync)
            {
                if (!isInitializedBefore)
                {
                    NucleusMvvmCore.Current.RunTaskInVoidAndTrackException(() => initializableAsync.InitAsync(NucleusMvvmCore.Current.NavigationParameters));
                }
                else
                {
                    NucleusMvvmCore.Current.RunTaskInVoidAndTrackException(() => initializableAsync.RefreshAsync(NucleusMvvmCore.Current.NavigationParameters));
                }
            }

            NucleusMvvmCore.Current.AppResumed += AppResumed!;
            NucleusMvvmCore.Current.AppStopped += AppStopped!;
        }

        private void PageNavigatedFrom(object sender, NavigatedFromEventArgs e)
        {
            _isNavigatedFrom = true;

            NucleusMvvmCore.Current.AppResumed -= AppResumed!;
            NucleusMvvmCore.Current.AppStopped -= AppStopped!;

            if (GetBindingContext() is INavigatedAware navigatedAware)
            {
                navigatedAware.OnNavigatedFrom(NucleusMvvmCore.Current.NavigationParameters);
            }

            if (DestroyAfterNavigatedFrom)
            {
                NucleusMvvmCore.Current.Logger?.LogInformation(Element != null ?
                    $"Destroying Element '{Element.GetType().Name}'." :
                    $"Destroying Page '{Page?.GetType().Name}'.");


                var element = Element != null ? Element : Page;
                (element?.BindingContext as IDestructible)?.Destroy();

                if (Page?.Behaviors != null)
                {
                    Page.Behaviors.Remove(this);
                }
                
                if (element != null)
                {
                    element!.BindingContext = null;
                }
            }
        }

        private void PageNavigatingFrom(object sender, NavigatingFromEventArgs e)
        {
            if (GetBindingContext() is INavigatedAware navigatedAware)
            {
                navigatedAware.OnNavigatingFrom(NucleusMvvmCore.Current.NavigationParameters);
            }
        }

        private void AppResumed(object sender, EventArgs e)
        {
            if (GetBindingContext() is IApplicationLifecycleAware applicationLifecycleAware)
            {
                applicationLifecycleAware.OnResume();
            }
        }

        private void AppStopped(object sender, EventArgs e)
        {
            if (GetBindingContext() is IApplicationLifecycleAware applicationLifecycleAware)
            {
                applicationLifecycleAware.OnSleep();
            }
        }

        private object? GetBindingContext()
        {
            return Element != null ? Element.BindingContext : Page?.BindingContext;
        }
    }
}
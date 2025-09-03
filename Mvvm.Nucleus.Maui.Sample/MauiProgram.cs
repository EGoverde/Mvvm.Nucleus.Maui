using Microsoft.Extensions.Logging;

namespace Mvvm.Nucleus.Maui.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseNucleusMvvm<App, AppShell>(options =>
            {
				// This method is used to register and map your Views and ViewModels.
                options.RegisterTypes(dependencyOptions => 
				{
					// These are pages registered within the AppShell.xaml with 'Absolute' routes.
					// It is important to match the given routes found in the XAML.
					dependencyOptions.RegisterShellView<NavigationTab, NavigationTabViewModel>("//Tabs/NavigationTab");
                	dependencyOptions.RegisterShellView<PopupTab, PopupTabViewModel>("//Tabs/PopupTab");
					dependencyOptions.RegisterShellView<DialogTab, DialogTabViewModel>("//Tabs/DialogTab");

					// These are pages not found within AppShell.xaml and use 'Global' routes.
					// They can be pushed from any page and are not restricted to a path.
                	dependencyOptions.RegisterView<Details, DetailsViewModel>();

					// These are popups, both with and without viewmodels. These can be used through IPopupService.
					dependencyOptions.RegisterPopup<SimplePopup>();
					dependencyOptions.RegisterPopup<AdvancedPopup, AdvancedPopupViewModel>();
					dependencyOptions.RegisterPopup<SingletonPopup, SingletonPopupViewModel>(ServiceLifetime.Singleton);
				});

				// This is called when Nucleus MVVM is initialized and before navigating to the first page.
				options.OnInitialized(serviceProvider => Console.WriteLine("OnInitialized"));

				// This is called when Nucleus MVVM is initialized and the first navigation has finished.
				options.OnAppStart(serviceProvider => Console.WriteLine("OnAppStart"));

				// Some additional configuration is available.
				options.AddQueryParametersToDictionary = true; // Default `true`. If set, query parameters (e.a. `route?key=val`) are automatically added to the navigation parameter dictionary.
				options.AlwaysDisableNavigationAnimation = true; // Default `false`. If set, no animations will be used during navigating, regardless of `isAnimated` (only when using the `INavigationService`).
				options.UseShellNavigationQueryParameters = true; // Default `true`. If set navigation parameters are passed to Shell as the one-time-use `ShellNavigationQueryParameters`.
				options.UseDeconstructPageOnDestroy = true; // Default `true`. Unload behaviors and unset bindingcontext of pages when they are popped.
				options.UseDeconstructPopupOnDestroy = true; // Default `true`. Unset the bindingcontext and parent of popups when they are dismissed.
				options.IgnoreNavigationWhenInProgress = true; // Default `true`. If set, when trying to navigate using the `INavigationService` while it is already busy will ignore other requests.
				options.IgnoreNavigationWithinMilliseconds = 250; // Default `250`. If set, when trying to navigate using the `INavigationService` while a previous request was done within the given milliseconds will ignore other requests.
				options.UseConfirmNavigationForAllNavigationRequests = false; // Default 'false'. If set, all navigation requests will be passed to the `IConfirmNavigation` and `IConfirmNavigationAsync` interfaces. Otherwise only Push and Pop requests are used.
				options.UseCommunityToolkitPopupServiceCompatibility = true; // Default 'true'. If set to false, the CommunityToolkit.Maui.IPopupService will not work properly, but performance might be better.

            })
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
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
					dependencyOptions.RegisterShellView<Intro, IntroViewModel>("//Intro");
					dependencyOptions.RegisterShellView<NavigationTab, NavigationTabViewModel>("//Tabs/NavigationTab");
                	dependencyOptions.RegisterShellView<PopupTab, PopupTabViewModel>("//Tabs/PopupTab");
					dependencyOptions.RegisterShellView<DialogTab, DialogTabViewModel>("//Tabs/DialogTab");

					// These are pages not found within AppShell.xaml and use 'Global' routes.
					// They can be pushed from any page and are not restricted to a path.
                	dependencyOptions.RegisterView<Details, DetailsViewModel>();

					// These are popups, both with and without viewmodels. These can be used through IPopupService.
					dependencyOptions.RegisterPopup<SimplePopup>();
					dependencyOptions.RegisterPopup<AdvancedPopup, AdvancedPopupViewModel>();
				});

				// This is called when Nucleus MVVM is initialized and before navigating to the first page.
				options.OnInitialized(serviceProvider => Console.WriteLine("OnInitialized"));

				// This is called when Nucleus MVVM is initialized and the first navigation has finished.
				options.OnAppStart(serviceProvider => Console.WriteLine("OnAppStart"));

				// Some additional configuration is available.
				options.AddQueryParametersToDictionary = true; // Default: True. If set query parameters (e.a. `route?key=val`) are automatically added to the navigation parameter dictionary.
				options.AlwaysDisableNavigationAnimation = true; // Default: False. IIf set the `isAnimated` flag while navigating will always be ignored and no animations will be used.
				options.UseShellNavigationQueryParameters = true; // Default: True. If set navigation parameters are passed to Shell as the one-time-use `ShellNavigationQueryParameters`.
				options.UsePageDestructionOnNavigation = true; // Default True. Attempts to unload behaviors and unset bindingcontext of pages when they are popped, as well as triggers the `IDestructible` interface.
				options.IgnoreNavigationWhenInProgress = true; // Default: False. If set when trying to navigate using the `INavigationService` while `IsNavigating` is `true` requests will be ignored.

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
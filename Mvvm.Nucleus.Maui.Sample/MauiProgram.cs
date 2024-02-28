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
                	dependencyOptions.RegisterShellView<HelpTab, HelpTabViewModel>("//Tabs/HelpTab");

					// These are pages not found within AppShell.xaml and use 'Global' routes.
					// They can be pushed from any page and are not restricted to a path.
                	dependencyOptions.RegisterView<Details, DetailsViewModel>();
				});

				// This is called when Nucleus MVVM is initialized and before navigating to the first page.
				options.OnInitialized(serviceProvider => Console.WriteLine("OnInitialized"));

				// This is called when Nucleus MVVM is initialzed and the first navigation has finished.
				options.OnAppStart(serviceProvider => Console.WriteLine("OnAppStart"));

				// Some additional configuration is available.
				options.AddRouteQueryParametersToDictionary = true;	// When true query parameters (e.a. route?key=val) are automatically added to the navigation parameter dictionary.
				options.UseShellNavigationQueryParameters = true; // When true passing navigation parameters using Shell the one-time-use ShellNavigationQueryParameters is used.
				options.IgnoreNavigationWhenInProgress = true; // When true trying to navigate using the INavigationService while IsNavigating is true will ignore those requests.

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

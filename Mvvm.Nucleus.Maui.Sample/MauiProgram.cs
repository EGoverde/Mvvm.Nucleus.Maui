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
				})
				.OnInitialized(serviceProvider =>
				{
					Console.WriteLine("OnInitialized");
				})
				.OnAppStart(serviceProvider =>
				{
					Console.WriteLine("OnAppStart");
				});
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

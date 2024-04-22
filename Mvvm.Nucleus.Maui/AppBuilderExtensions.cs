using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mvvm.Nucleus.Maui;

public static class AppBuilderExtensions
{
    /// <summary>
    /// Initializes and configures Nucleus.
    /// </summary>
    /// <typeparam name="TApp">The <see cref="Type"/> of the subclass of <see cref="Application"/> for this app.</typeparam>
    /// <typeparam name="TShell">The <see cref="Type"/> of the subclass of <see cref="Shell"/> for this app.</typeparam>
    /// <param name="builder">The <see cref="MauiAppBuilder"/>.</param>
    /// <param name="options">The <see cref="Action"/> that uses <see cref="NucleusMvvmOptions"/> to configure Nucleus.</param>
    /// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
    public static MauiAppBuilder UseNucleusMvvm<TApp, TShell>(this MauiAppBuilder builder, Action<NucleusMvvmOptions>? options = null)
        where TApp : Application
        where TShell : Shell
    {
        builder
            .UseMauiApp<TApp>()
            .UseMauiCommunityToolkit();

        var nucleusMvvmOptions = new NucleusMvvmOptions();
        options?.Invoke(nucleusMvvmOptions);

        nucleusMvvmOptions.RegisterTypes?.Invoke(nucleusMvvmOptions.DependencyOptions);

        RegisterMvvmOptions(builder, nucleusMvvmOptions);

        builder.Services.TryAddSingleton<Application, TApp>();
        builder.Services.TryAddTransient<Shell, TShell>();
        builder.Services.AddSingleton<NucleusMvvmCore>();
        builder.Services.TryAddSingleton<IViewFactory, ViewFactory>();
        builder.Services.TryAddSingleton<IPageDialogService, PageDialogService>();
        builder.Services.TryAddSingleton<IPopupService, PopupService>();

        switch(nucleusMvvmOptions.NavigationType)
        {
            case NavigationType.Shell:
                builder.Services.TryAddSingleton<INavigationService, NavigationServiceShell>();
                break;
            case NavigationType.Modeless:
                builder.Services.TryAddSingleton<INavigationService, NavigationServiceShell>();
                break;
        }

        builder.Services.AddSingleton<IApplication>(serviceProvider =>
        {
            var nucleusMvvmCore = serviceProvider.GetRequiredService<NucleusMvvmCore>();
            nucleusMvvmCore.Initialize(serviceProvider);

            var nucleusMvvmOptions = serviceProvider.GetRequiredService<NucleusMvvmOptions>();
            nucleusMvvmOptions.OnInitialized?.Invoke(serviceProvider);

            var shell = serviceProvider.GetRequiredService<Shell>();
            var application = nucleusMvvmCore.Application;

            application.MainPage = shell;
            application.PageAppearing += OnApplicationAppearing;

            return application;
        });

        return builder;
    }

    private static void OnApplicationAppearing(object? sender, Page page)
    {
        if (sender is Application application)
        {
            application.PageAppearing -= OnApplicationAppearing;
        }

        if (NucleusMvvmCore.Current.ServiceProvider is IServiceProvider serviceProvider && 
            serviceProvider.GetRequiredService<NucleusMvvmOptions>() is NucleusMvvmOptions nucleusMvvmOptions &&
            nucleusMvvmOptions.OnAppStart != null)
        {
            nucleusMvvmOptions.OnAppStart(serviceProvider);
        }
    }

    private static void RegisterMvvmOptions(MauiAppBuilder mauiAppBuilder, NucleusMvvmOptions nucleusMvvmOptions)
    {
        foreach (var viewMapping in nucleusMvvmOptions.ViewMappings)
        {
            Func<IServiceProvider, object> viewResolver = serviceProvider =>
            {
                var viewFactory = serviceProvider.GetRequiredService<IViewFactory>();
                return viewFactory.CreateView(viewMapping.ViewType);
            };

            switch (viewMapping.RegistrationScope)
            {
                case ViewScope.Scoped:
                    mauiAppBuilder.Services
                        .AddScoped(viewMapping.ViewType, viewResolver)
                        .TryAddScoped(viewMapping.ViewModelType);
                    break;

                case ViewScope.Singleton:
                    mauiAppBuilder.Services
                        .AddSingleton(viewMapping.ViewType, viewResolver)
                        .TryAddSingleton(viewMapping.ViewModelType);
                    break;
                case ViewScope.Transient:
                default:
                    mauiAppBuilder.Services
                        .AddTransient(viewMapping.ViewType, viewResolver)
                        .TryAddTransient(viewMapping.ViewModelType);
                    break;
            }

            if (nucleusMvvmOptions.NavigationType == NavigationType.Shell &&
                viewMapping.RegistrationType == ViewRouteType.GlobalRoute)
            {
                Routing.RegisterRoute(viewMapping.Route, viewMapping.ViewType);
            }
        }

        mauiAppBuilder.Services.AddSingleton(nucleusMvvmOptions);
    }
}
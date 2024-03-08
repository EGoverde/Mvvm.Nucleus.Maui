using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mvvm.Nucleus.Maui
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseNucleusMvvm<TApp, TShell>(this MauiAppBuilder builder, Action<NucleusMvvmOptions>? options = null)
            where TApp : Application
            where TShell : Shell
        {
            builder.UseMauiApp<TApp>();

            var nucleusMvvmOptions = new NucleusMvvmOptions();
            options?.Invoke(nucleusMvvmOptions);

            nucleusMvvmOptions.RegisterTypes?.Invoke(nucleusMvvmOptions.DependencyOptions);

            RegisterMvvmOptions(builder, nucleusMvvmOptions);

            builder.Services.AddSingleton<Application, TApp>();
            builder.Services.AddTransient<Shell, TShell>();
            builder.Services.AddSingleton<NucleusMvvmCore>();
            builder.Services.TryAddSingleton<IViewFactory, ViewFactory>();

            switch(nucleusMvvmOptions.NavigationType)
            {
                case NavigationType.Shell:
                    builder.Services.TryAddSingleton<INavigationService, NavigationServiceShell>();
                    builder.Services.TryAddSingleton<IPageDialogService, PageDialogServiceShell>();
                    break;
                case NavigationType.Modeless:
                    builder.Services.TryAddSingleton<INavigationService, NavigationServiceShell>();
                    builder.Services.TryAddSingleton<IPageDialogService, PageDialogServiceModeless>();
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
                            .AddScoped(viewMapping.ViewModelType);
                        break;

                    case ViewScope.Singleton:
                        mauiAppBuilder.Services
                            .AddSingleton(viewMapping.ViewType, viewResolver)
                            .AddSingleton(viewMapping.ViewModelType);
                        break;
                    case ViewScope.Transient:
                    default:
                        mauiAppBuilder.Services
                            .AddTransient(viewMapping.ViewType, viewResolver)
                            .AddTransient(viewMapping.ViewModelType);
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
}
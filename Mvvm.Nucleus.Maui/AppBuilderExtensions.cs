using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="AppBuilderExtensions"/> holds the <see cref="MauiAppBuilder"/> extension for Nucleus.
/// </summary>
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
        builder.Services.TryAddSingleton<IWindowCreator, NucleusWindowCreator>();
        builder.Services.AddSingleton<NucleusMvvmCore>();
        builder.Services.TryAddSingleton<IViewFactory, ViewFactory>();
        builder.Services.TryAddSingleton<IPopupViewFactory, PopupViewFactory>();
        builder.Services.TryAddSingleton<INavigationService, NavigationService>();
        builder.Services.TryAddSingleton<IPageDialogService, PageDialogService>();
        builder.Services.TryAddSingleton<IPopupService, PopupService>();

        builder.Services.AddSingleton<IApplication>(serviceProvider =>
        {
            var nucleusMvvmCore = serviceProvider.GetRequiredService<NucleusMvvmCore>();

            var application = nucleusMvvmCore.Application;
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
            object viewResolver(IServiceProvider serviceProvider)
            {
                var viewFactory = serviceProvider.GetRequiredService<IViewFactory>();
                return viewFactory.CreateView(viewMapping.ViewType);
            }

            mauiAppBuilder.Services.Add(new ServiceDescriptor(viewMapping.ViewType, viewResolver, viewMapping.ServiceLifetime));
            mauiAppBuilder.Services.TryAdd(new ServiceDescriptor(viewMapping.ViewModelType, viewResolver, viewMapping.ServiceLifetime));

            if (viewMapping.RegistrationType == ViewRouteType.GlobalRoute)
            {
                Routing.RegisterRoute(viewMapping.Route, viewMapping.ViewType);
            }
        }

        MethodInfo? registerScopedPopup = null;
        MethodInfo? registerSingletonPopup = null;
        MethodInfo? registerTransientPopup = null;

        foreach (var popupMapping in nucleusMvvmOptions.PopupMappings)
        {
            object popupViewResolver(IServiceProvider serviceProvider)
            {
                var viewFactory = serviceProvider.GetRequiredService<IPopupViewFactory>();
                return viewFactory.CreateView(popupMapping.PopupViewType);
            }

            if (popupMapping.PopupViewModelType != null)
            {
                if (nucleusMvvmOptions.UseCommunityToolkitPopupService)
                {
                    MethodInfo? registerPopup = null;

                    switch (popupMapping.ServiceLifetime)
                    {
                        case ServiceLifetime.Scoped:
                            registerScopedPopup ??= GetAddPopupExtensionMethod(nameof(ServiceCollectionExtensions.AddScopedPopup));
                            registerPopup = registerScopedPopup;
                            break;
                        case ServiceLifetime.Singleton:
                            registerSingletonPopup ??= GetAddPopupExtensionMethod(nameof(ServiceCollectionExtensions.AddSingletonPopup));
                            registerPopup = registerSingletonPopup;
                            break;
                        default:
                            registerTransientPopup ??= GetAddPopupExtensionMethod(nameof(ServiceCollectionExtensions.AddTransientPopup));
                            registerPopup = registerTransientPopup;
                            break;
                    }

                    registerPopup?
                        .MakeGenericMethod(popupMapping.PopupViewType, popupMapping.PopupViewModelType)
                        .Invoke(mauiAppBuilder.Services, [mauiAppBuilder.Services]);

                    mauiAppBuilder.Services.RemoveAll(popupMapping.PopupViewType);
                    mauiAppBuilder.Services.RemoveAll(popupMapping.PopupViewModelType);
                }

                mauiAppBuilder.Services.Add(new ServiceDescriptor(popupMapping.PopupViewModelType!, popupMapping.PopupViewModelType!, popupMapping.ServiceLifetime));
            }

            mauiAppBuilder.Services.Add(new ServiceDescriptor(popupMapping.PopupViewType, popupViewResolver, popupMapping.ServiceLifetime));
        }

        mauiAppBuilder.Services.AddSingleton(nucleusMvvmOptions);
    }

    private static MethodInfo? GetAddPopupExtensionMethod(string methodName, int genericArgumentCount = 2)
    {
        return typeof(ServiceCollectionExtensions)
            .GetMethods()?
            .FirstOrDefault(x => x.Name == methodName && x.GetGenericArguments().Length == genericArgumentCount);
    }
}
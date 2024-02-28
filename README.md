# Nucleus MVVM for MAUI

Nucleus MVVM is a framework written to be used in .NET MAUI projects. It is build on top of [MAUI](https://github.com/dotnet/maui) and the [CommunityToolkit.Mvvm](https://learn.microsoft.com/nl-nl/dotnet/communitytoolkit/mvvm/) and aims for better separation of UI and logic.

## Highlighted features

- Navigation from ViewModels (Shell or Modeless) through INavigationService.
- Displaying Alerts, Dialogs and ActionSheets through IPageDialogService.
- Automatic creation and assigning of ViewModels and Views (using a [Behavior](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors)).
- ViewModels events (e.a. Appearing, Navigation, Initialization) through interfaces.
- Flexibility in Views and ViewModels, no base classes are required.

## Getting started

Nucleus MVVM is available as a [NuGet package](https://www.nuget.org/packages/Mvvm.Nucleus.Maui). After adding the package it requires little code to get started and remains similar to a regular MAUI app. To get started remove the default `UseMauiApp<App>` and configure Nucleus using the options:

                builder.UseNucleusMvvm<App, AppShell>(options =>
                    options.RegisterTypes(dependencyOptions => 
                        dependencyOptions.RegisterShellView<MyAbsoluteView, MyAbsoluteViewModel>("//MyAbsoluteView");
                        dependencyOptions.RegisterView<MyGlobalView, MyGlobalViewModel>("//MyGlobalView");
                    );
                )..

See the documentation for **Navigation Service** to see the usage and differences between `RegisterShellView` and `RegisterView`.

ViewModels can be of any type and support dependency injection. By implementing interfaces (**see Event Interfaces**) they can trigger logic on events like navigation or its page appearing.

It is recommended for a ViewModel to have `ObserableObject` as a base for its bindings. An optional `NucleusViewModel` is included to have some boilerplate events like `OnInitAsync()` and `OnRefreshAsync`.

Additionally it is recommended to add the `Mvvm.Nucleus.Maui` namespace to your GlobalUsings.

See the *Sample Project* in the repository for more examples of Nucleus MVVM usage.

## Navigation Service

Navigation can be done through injecting the `INavigationService`. Currently only the `Shell` implemention is supported. Navigation is done by either specifying the (type of the) View or a Route.

- `await NavigateAsync<Home>();`
- `await NavigateAsync(typeof(Home))`
- `await NavigateAsync("//Home");`

Views and their ViewModels need to be registered in `MauiProgram.cs`. Pages defined within `AppShell.xaml` are known as *absolute routes* and should be registered using `RegisterShellView<MyView, MyViewModel>("//MyRoute")`. It is important that the given route matches the XAML. 

Any pages not defined witin `AppShell.xaml` are known as *global routes* and can be pushed from any page. You can register these simply as `RegisterView<MyGlobalView, MyGlobalViewModel>()`, as by default they will get their name as route. You can however supply a custom one. Routes always have to be unique, or the registration will fail.

Parameters can be added by supplying an `IDictionary<string, object>`, which will be passed to the `Init` and `Refresh` or various `Navigated` events. For Nucleus interfaces these parameters will only be passed during a single navigation action.

URL parameters (e.a. &param=true) are not used in the Nucleus events, but are still supported as their default Shell implementation when using routes. The dictionary however will also be passed as usual and could be used through `IQueryAttributable`.

## Modal Navigation

When navigating Nucleus will look for known parameters in the navigation parameter `IDictionary<string, object>`. Currently the following parameters are supported:

**NucleusNavigationParameters.NavigatingPresentationMode**: Expects a [PresentationMode](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.presentationmode?) that will be added to the page.
**NucleusNavigationParameters.WrapInNavigationPage**: Wraps a NavigationPage around the target, allowing for deeper navigation within a modal page.

*Note that above paramaters allow for modal presentation in Shell including deeper navigation (see sample project). However this appears an under developed area of Shell and might not be stable.*

## Event Interfaces

- **IApplicationLifeCycleAware:** When the app is going to the background or returning.
- **IConfirmNavigation(Async):** Allows to interupt the navigation, for example by asking for confirmation.
- **IDestructible:** Currently not implemented, see **Limitations / Planned features**.
- **IInitializable(Async):** Init and Refresh functions upon navigating the first or further times.
- **INavigatedAware:** Navigation events 'from' and 'to' the ViewModel.
- **IPageLifecycleAware:** Appearing and disappearing events from the view.

## Migrating from Prism

[Prism](https://prismlibrary.com/docs/maui/index.html) is a popular library offering the same (and quite a few more) features as this library. Nucleus MVVM aims to be a simpler alternative, only adding quality-of-life MVVM features as a layer on top of [MAUI](https://github.com/dotnet/maui. This should result in an easy to maintain library able to use the latest MAUI features.

Some compatibility classes have been included to simplify migration for projects limited to simple navigation and MVVM functionality. Nucleus MVVM is a much simpler library however, so when using more advanced Prism features this might require significant rework. Included are:

- Interfaces for similar ViewModels events (e.a. `IPageLifecycleAware`) are mostly kept identical to Prism.
- BindableBase class has been created to add some missing functions to `ObservableObject`.
- NavigationParameters class has been added as a named `IDictionary<string, object>`.

Contrary to Prism, dependency injection in Nucleus uses the default Microsoft implementation, which means that apart from registering Views/ViewModels, any other registration should be done through the usual `Services.AddSingleton<>` and similar.

## Limitations / Planned features

- Currently Shell is the only supported navigation type, but groundworks have been laid for a 'modeless' implementation.
- Currently Shell is automatically setup as the MainPage without any customization options for a startup flow.
- There is limited support for 'modal' presentation in Shell, this logic might be removed in favor of a modeless-specific implementation.
- The implementation for IDestructible as logic to clean up Views and ViewModels is not yet in place.
- Limited logic for passing events from a page to its children is there, but a more complete concept would be nice.
- More complete documentation, especially in the form of XML comments in the code.

## Support

- Bugs and feature requests can be created through [GitHub issues](https://github.com/EGoverde/Mvvm.Nucleus.Maui/issues/new).

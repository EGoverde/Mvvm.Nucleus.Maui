# Nucleus MVVM for MAUI

Nucleus MVVM is a framework written to be used in .NET MAUI projects. It is build on top of [MAUI](https://github.com/dotnet/maui) and the [CommunityToolkit.Mvvm](https://learn.microsoft.com/nl-nl/dotnet/communitytoolkit/mvvm/) and aims for better separation of UI and logic.

[![NuGet version (Mvvm.Nucleus.Maui)](https://img.shields.io/nuget/v/Mvvm.Nucleus.Maui.svg?style=flat-square)](https://www.nuget.org/packages/Mvvm.Nucleus.Maui/)

## Highlighted features

- Navigation from ViewModels (Shell or Modeless*) through INavigationService.
- Displaying Alerts, Dialogs and ActionSheets through IPageDialogService.
- Automatic creation and assigning of ViewModels and Views (using a [Behavior](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors)).
- ViewModels events (e.a. Appearing, Navigation, Initialization) through interfaces.
- Flexibility in Views and ViewModels, no base classes are required.

\* See [Limitations / Planned features](#limitations--planned-features)

## Getting started

Nucleus MVVM is available as a [NuGet package](https://www.nuget.org/packages/Mvvm.Nucleus.Maui). After adding the package it requires little code to get started and remains similar to a regular MAUI app. It is recommended to add the `Mvvm.Nucleus.Maui` namespace to your GlobalUsings. To get started remove the default `UseMauiApp<App>` and configure Nucleus using the options:

                builder.UseNucleusMvvm<App, AppShell>(options =>
                    options.RegisterTypes(dependencyOptions => 
                        dependencyOptions.RegisterShellView<MyAbsoluteView, MyAbsoluteViewModel>("//MyAbsoluteView");
                        dependencyOptions.RegisterView<MyGlobalView, MyGlobalViewModel>("//MyGlobalView");
                    );
                )..

See [Navigation](#navigation) to see the usage and differences between `RegisterShellView` and `RegisterView`.

ViewModels can be of any type and support dependency injection. By implementing interfaces (see [Event interfaces](#event-interfaces)) they can trigger logic on events like navigation or its page appearing. It is recommended for a ViewModel to have `ObserableObject` as a base for its bindings.

An optional `NucleusViewModel` is included to have some boilerplate events like `OnInitAsync()` and `OnRefreshAsync`.

### Configuration

Within the options the following additional settings can be changed:

- `AddQueryParametersToDictionary`: Default `true`. If set query parameters (e.a. `route?key=val`) are automatically added to the navigation parameter dictionary.
- `AlwaysDisableNavigationAnimation`: Default `false`. If set no animations will be used during navigating, regardless of `isAnimated` (only when using the `INavigationService`).
- `IgnoreNavigationWhenInProgress`: Default `false`. If set when trying to navigate using the `INavigationService` while it is already busy requests will be ignored.
- `UsePageDestructionOnNavigation`: Default `false`. Attempts to unload behaviors and unset bindingcontext of pages when they are popped, as well as triggers the `IDestructible` interface.
- `UseShellNavigationQueryParameters`: Default `true`. If set navigation parameters are passed to Shell as the one-time-use `ShellNavigationQueryParameters`.

See the *Sample Project* in the repository for more examples of Nucleus MVVM usage.

## Services

- `INavigationService`: Handles various navigation flows, see [Navigation](#navigation).
- `IPageDialogService`: Show alerts, action sheets and prompts using [MAUI Page Alerts](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups).

## Navigation

Navigation can be done through injecting the `INavigationService`. Currently only the Shell implemention is supported. Navigation is done by either specifying the (type of the) View or a Route.

- `await NavigateAsync<Home>();`
- `await NavigateAsync(typeof(Home))`
- `await NavigateAsync("//Home");`

Views and their ViewModels need to be registered in `MauiProgram.cs`. Pages defined within `AppShell.xaml` are known as *absolute routes* and should be registered using `RegisterShellView<MyView, MyViewModel>("//MyRoute")`. It is important that the given route matches the XAML. 

Any pages not defined witin `AppShell.xaml` are known as *global routes* and can be pushed from any page. You can register these simply as `RegisterView<MyGlobalView, MyGlobalViewModel>()`, as by default they will get their name as route. You can however supply a custom one. Routes always have to be unique, or the registration will fail.

*Note that in Shell pages on the root-level (e.a. //home) will be reused after the intial navigation, even if set to `Transient`. See [this issue](https://github.com/dotnet/maui/issues/9300)*

### Passing data

When navigating an `IDictionary<string, object>` can be passed to the `INavigationService`, which will be passed to the `Init` and `Refresh` or various `Navigated` events. The dictionary will only be passed once and it will never be null. In routes query string parameters are supported as well (e.a. `?myValue=value`), but not the recommended approach.

Values can be retrieved using regular `IDictionary` methods, but additionally there are the following extensions:

- `NavigationParameters.GetValueOrDefault<T>(key, defaultValue)`
- `NavigationParameters.GetStructOrDefault<T>(key, defaultValue)`

If using Shell these parameters can also be used as described in the [MAUI documentation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation#pass-data), including accessing them through `IQueryAttributable` and `QueryProperty`. By default the values will be wrapped inside `ShellNavigationQueryParameters`, but this can be turned off in the Nucleus MVVM options (see [Getting started](#getting-started)).

### Modal navigation

When navigating Nucleus will look for certain parameters in the navigation parameter `IDictionary<string, object>`. Currently the following parameters are supported:

- `NucleusNavigationParameters.NavigatingPresentationMode`: Expects a [PresentationMode](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.presentationmode?) that will be added to the page.
- `NucleusNavigationParameters.WrapInNavigationPage`: Wraps a NavigationPage around the target, allowing for deeper navigation within a modal page.

Note that above parameters allow for modal presentation in Shell including deeper navigation (see the Sample Project in the repository). However this appears an underdeveloped area of Shell and might not be stable. See 

### Navigation interfaces

- `IApplicationLifeCycleAware`: When the app is going to the background or returning.
- `IConfirmNavigation(Async)`: Allows to interupt the navigation, for example by asking for confirmation.
- `IDestructible`: Requires `UsePageDestructionOnNavigation`. Triggered when `transient` pages are removed from the stack.
- `IInitializable(Async)`: Init and Refresh functions upon navigating the first or further times.
- `INavigatedAware`: Navigation events 'from' and 'to' the ViewModel.
- `IPageLifecycleAware`: Appearing and disappearing events from the view.

## Migrating from Prism

[Prism](https://prismlibrary.com/docs/maui/index.html) is a popular library offering the same (and quite a few more) features as this library. Nucleus MVVM aims to be a simpler alternative, only adding quality-of-life MVVM features as a layer on top of [MAUI](https://github.com/dotnet/maui). This should result in an easy to maintain library able to use the latest MAUI features.

Some compatibility classes have been included to simplify migration for projects limited to simple navigation and MVVM functionality. Nucleus MVVM is a much simpler library however, so when using more advanced Prism features this might require significant rework. Included are:

- Interfaces for similar ViewModels events (e.a. `IPageLifecycleAware`) are mostly kept identical to Prism.
- BindableBase class has been created to add some missing functions to `ObservableObject`.
- NavigationParameters class has been added as a named `IDictionary<string, object>`.

Note that the `NavigationParameter` compatibility class (and Prism implementation) differs from IDictionary in that it returns null when accessing a non-existing key (e.a. navigationParameters["nonKey"]). See [Passing data](#passing-data).

Contrary to Prism, dependency injection in Nucleus uses the default Microsoft implementation, which means that apart from registering Views/ViewModels, any other registration should be done through the usual `Services.AddSingleton<>` and similar.

## Limitations / Planned features

- Currently Shell is the only supported navigation type, but groundworks have been laid for a 'modeless' implementation.
- Currently Shell is automatically setup as the MainPage without any customization options for a startup flow.
- There is limited support for 'modal' presentation in Shell, this logic might be removed in favor of a modeless-specific implementation.
- Limited logic for passing events from a page to its children is there, but a more complete concept would be nice.
- More complete documentation, especially in the form of XML comments in the code.

## Support

- Bugs and feature requests can be created through [GitHub issues](https://github.com/EGoverde/Mvvm.Nucleus.Maui/issues/new).

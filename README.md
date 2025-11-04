# Nucleus MVVM for MAUI

Nucleus MVVM is a framework written to be used in .NET MAUI projects. It is build on top of [MAUI](https://github.com/dotnet/maui), the [CommunityToolkit.Mvvm](https://learn.microsoft.com/nl-nl/dotnet/communitytoolkit/mvvm/) and the [CommunityToolkit.Maui](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/). Its purpose is for better separation of UI and logic using MVVM conventions.

[![NuGet version (Mvvm.Nucleus.Maui)](https://img.shields.io/nuget/v/Mvvm.Nucleus.Maui.svg?style=flat-square)](https://www.nuget.org/packages/Mvvm.Nucleus.Maui/)

## Index

- [Highlighted features](#highlighted-features)
- [Getting started](#getting-started)
    - [Configuration](#configuration)
- [Services](#services)
- [Navigation](#navigation)
    - [Passing data](#passing-data)
    - [Modal navigation](#modal-navigation)
    - [Avoiding double navigation](#avoiding-double-navigation)
    - [Navigation interfaces](#navigation-interfaces)
- [Popups](#popups)
    - [Closing and return values](#closing-and-return-values)
    - [Migrating from Nucleus 0.5.0](#migrating-from-nucleus-050)
    - [Popup interfaces](#popup-interfaces)
- [Prism compatibility](#prism-compatibility)
- [Limitations and plans](#limitations-and-plans)
- [Support](#support)

## Highlighted features

- Navigation from ViewModels through INavigationService (using [Shell](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/)).
- Displaying Alerts, Dialogs and ActionSheets through IPageDialogService.
- Displaying Popups through IPopupService.
- Automatic creation and assigning of ViewModels and Views (using a [Behavior](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/behaviors)).
- ViewModels and Popup events (e.a. Appearing, Navigation, Initialization) through interfaces.
- Flexibility in Views and ViewModels, no base classes are required.
- Basic [Prism compatibility](#migrating-from-prism) for migrating an existing codebase.

## Getting started

Nucleus MVVM is available as a [NuGet package](https://www.nuget.org/packages/Mvvm.Nucleus.Maui). After adding the package it requires little code to get started and remains similar to a regular MAUI app. It is recommended to add the `Mvvm.Nucleus.Maui` namespace to your GlobalUsings.

To get started:

1. **Remove `CreateWindow(IActivationState? activationState)` in `App.xaml.cs`.**
2. **Remove `UseMauiApp<App>` (and `UseCommunityToolkit` if there) and replace it with `UseNucleusMvvm<App, AppShell>`.**
3. **Then configure Nucleus, at the minimum the page(s) in `AppShell` using the options.**

See [Navigation](#navigation) and [Popups](#popups) for the usage of the `RegisterShellView`, `RegisterView` and `RegisterPopup`, .

*Note that the [CommunityToolkit.Maui](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/) is a dependency of Nucleus. You should not call `UseMauiCommunityToolkit` manually, as this is already done through `UseNucleusMvvm`. If you need to configure the Community Toolkit you can access the options through the `UseNucleusMvvm` method.*


    builder
    .UseNucleusMvvm<App, AppShell>(options =>
    {
        options.RegisterTypes(dependencyOptions => 
            dependencyOptions.RegisterShellView<MyAbsoluteView, MyAbsoluteViewModel>("//MyAbsoluteView");
            dependencyOptions.RegisterView<MyGlobalView, MyGlobalViewModel>("//MyGlobalView");
        );
    })
    .Etc..

ViewModels can be of any type and support dependency injection. By implementing interfaces (see [Navigation interfaces](#navigation-interfaces) and [Popup interfaces](#popup-interfaces)) they can trigger logic on events like navigation or its page appearing. It is recommended for a ViewModel to have `ObservableObject` as a base for its bindings.



### Configuration

Within the options the following additional settings can be changed:

- `AddQueryParametersToDictionary`: Default `true`. If set, query parameters (e.a. `route?key=val`) are automatically added to the navigation parameter dictionary.
- `AlwaysDisableNavigationAnimation`: Default `false`. If set, no animations will be used during navigating, regardless of `isAnimated` (only when using the `INavigationService`).
- `IgnoreNavigationWhenInProgress`: Default `true`. If set, when trying to navigate using the `INavigationService` while it is already busy will ignore other requests.
- `IgnoreNavigationWithinMilliseconds`: Default `250`. If set, when trying to navigate using the `INavigationService` while a previous request was done within the given milliseconds will ignore other requests.
- `UseShellNavigationQueryParameters`: Default `true`. If set navigation parameters are passed to Shell as the one-time-use `ShellNavigationQueryParameters`.
- `UseConfirmNavigationForAllNavigationRequests`: Default `false`. If set, all navigation requests will be passed to the `IConfirmNavigation` and `IConfirmNavigationAsync` interfaces. Otherwise only Push and Pop requests are used.
- `UseDeconstructPageOnDestroy`: Default `true`. Unload behaviors and unset bindingcontext of pages when they are popped.
- `UseDeconstructPopupOnDestroy`: Default `true`. Unset the bindingcontext and parent of popups when they are dismissed.
- `UseCommunityToolkitPopupServiceCompatibility`: Default `true`. Enables usage of the `CommunityToolkit.PopupService` on top of the build-in `Mvvm.Nucleus.Maui.PopupService`. You only need to register popups through Nucleus for this to work.
- `UseAlternativePopupOpenedAndClosedEvents`: Default: `true`. If set the `IPopupLifecycleAware` will do an additional check on the default Popup `Opened` and `Closed` events, see [Popups](#popups) for details.
- `CommunityToolkitV1PopupServicePopupOptions`: Default: `null`. If set this value will be used in the `CommunityToolkitV1PopupService` compatibility service.

See the *Sample Project* in the repository for more examples of Nucleus MVVM usage.

## Services

- `INavigationService`: Handles various navigation flows, see [Navigation](#navigation).
- `IPopupService`: Show popups using [CommunityToolkit.MAUI Popups](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup), see [Popups](#popups).
- `IPageDialogService`: Show alerts, action sheets and prompts using [MAUI Page Alerts](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups).

## Navigation

Navigation can be done through the `INavigationService`. Currently only the Shell implemention is supported. Navigation is done by either specifying the (type of the) View or a Route.

- `await NavigateAsync<Home>();`
- `await NavigateAsync(typeof(Home))`
- `await NavigateAsync("//Home");`

Views and their ViewModels need to be registered in `MauiProgram.cs`. Pages defined within `AppShell.xaml` are known as *absolute routes* and should be registered using `RegisterShellView<MyView, MyViewModel>("//MyRoute")`. It is important that the given route matches the XAML. 

Any pages not defined witin `AppShell.xaml` are known as *global routes* and can be pushed from any page. You can register these simply as `RegisterView<MyGlobalView, MyGlobalViewModel>()`, as by default they will get their name as route. You can however supply a custom one. Routes always have to be unique, or the registration will fail.

### Passing data

When navigating an `IDictionary<string, object>` can be passed to the `INavigationService`, which will be passed to the `Init` and `Refresh` or various `Navigated` events. The dictionary will only be passed once and it will never be null. In routes query string parameters are supported as well (e.a. `?myValue=value`), but not the recommended approach.

Values can be retrieved using regular `IDictionary` methods, but additionally there are the following extensions:

- `NavigationParameters.GetValueOrDefault<T>(key, defaultValue)`
- `NavigationParameters.GetStructOrDefault<T>(key, defaultValue)`

These parameters can also be used as described in the [MAUI documentation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation#pass-data), including accessing them through `IQueryAttributable` and `QueryProperty`. By default the values will be wrapped inside `ShellNavigationQueryParameters`, but this can be turned off in the Nucleus MVVM options (see [Getting started](#getting-started)).

Additional data is included by Nucleus through the navigation parameters for advances use-cases, notably through the extension `GetShellNavigationSource`.

### Modal navigation

When navigating Nucleus will look for certain parameters in the navigation parameter `IDictionary<string, object>`. Currently the following parameters are supported:

- `NucleusNavigationParameters.NavigatingPresentationMode`: Expects a [PresentationMode](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.presentationmode?) that will be added to the page.
- `NucleusNavigationParameters.WrapInNavigationPage`: Wraps a NavigationPage around the target, allowing for deeper navigation within a modal page.

Note that above parameters allow for modal presentation in Shell including deeper navigation (see the Sample Project in the repository). However this appears an underdeveloped area of Shell and might not be stable.

### Avoiding double navigation

On slower devices it is a common issue that users are able to trigger multiple navigation requests by pressing a button one more than once,
either too quickly or while waiting for the navigation to start. When using the CommunityToolkit `(Async)RelayCommand` this problem is reduced, as the Command will be disabled while it's processing. But since a navigation Task returns before it has finished navigating, it can still occur.

Nucleus offers two features to improve the navigation behavior, both are enabled by default. These are `IgnoreNavigationWhenInProgress` and `IgnoreNavigationWithinMilliseconds`, see [Configuration].

In specific cases you might want to bypass these restrictions, but not disable them fully. In those cases you can add `NucleusNavigationParameters.DoNotIgnoreThisNavigationRequest` in the NavigationParameters and set it to true.

Note that due to the nature of the `PopupService` there is no logic for avoiding multiple triggers, as it always expects a return object.

### Navigation interfaces

- `IApplicationLifeCycleAware`: When the app is going to the background or returning.
- `IConfirmNavigation(Async)`: Allows to interupt the navigation, by default limited to Pop and Push events (see [Configuration]).
- `IDestructible`: Triggered when `transient` pages are removed from the stack.
- `IInitializable(Async)`: Init and Refresh functions upon navigating the first or further times.
- `INavigatedAware`: Navigation events 'from' and 'to' the ViewModel.
- `IPageLifecycleAware`: Appearing and disappearing events from the page.

## Popups

Nucleus can display [CommunityToolkit.MAUI Popups](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup) through the `IPopupService`. This works very similar to navigation. The `CommunityToolkit.PopupService` is also supported for certain scenarios, as long as `options.UseCommunityToolkitPopupServiceCompatibility` is set to `true`. Note that you should always navigate by passing the `View` type, not the ViewModel.

Popups can be used with or without ViewModels and require registration in `MauiProgram.cs` using `RegisterPopup<MyPopup>` or `RegisterPopup<MyPopup, MyPopupViewModel>`. After registration popups can be shown by passing the view type to one of the various `ShowPopupAsync` methods. 

Parameters can be sent through an `IDictionary<string, object>`, which will be passed to Init or InitAsync (see [Popup interfaces](#popup-interfaces)), as well as `IQueryAttributable`. These methods will be called before showing the popup. The async variant can be configured such that it has to finish before showing the popup.

Popups are by default registered as `Transient`, but support `Scoped` and `Singleton` as well.

**Note:** By default the `Opened` and `Closed` events that are passed to `IPopupLifeCycleAware` have additional logic to ensure the `Popup` is actually closed, and not just currently deeper in the navigation stack. This can occur in the CommunityToolkit when presenting a Popup from within a Popup, which we consider a bug. This functionality can be disabled through `UseAlternativePopupOpenedAndClosedEvents`.

### Closing and return values
The `IPopupService` can either show a popup with or without an expected return value, wrapped in an `IPopupResult`. The methods that return a value other than the generic result, require the use of a `Popup<T>` (T being the return type).

To close the popup and return the value you can either use `IPopupService.CloseMostRecentPopupAsync`, or the `IPopupAware` and `IPopupAware<T>` interfaces. Note that you can always access `CloseAsync()` fom the `WeakReference<Popup>`, but To access `CloseAsync(value)` you need to use `IPopupAware<T>` (where T matches the used `Popup<T>`).

    if (Popup?.TryGetTarget(out MyCustomPopup? myCustomPopup) == true)
    {
        await myCustomPopup.CloseAsync("result");
    }

### Migrating from Nucleus 0.5.0

Nucleus uses the `Popup` functionality from the `Maui.CommunityToolkit`. In version 12.x of the toolkit a large breaking change was done, known as the V2 Popups. This required significant changes to the Nucleus implementation as well, which were part of the 0.6.0 release.

A [migration guide](/MIGRATIONS.md) has been written to help migrate from the previous implementation to the current.

### Popup interfaces

Below interfaces below work on both the View and the ViewModel, with the exception of `IPopupAware<T>`.

- `IPopupAware`: Allows access to the generic Popup type using a WeakReference.
- `IPopupAware<T>`: Allows access to an exact Popup type using a WeakReference.
- `IPopupInitializable(Async)`: Init functions triggered before showing the popup.
- `IPopupLifeCycleAware`: Events on opening and closing the popup.
- `IDestructible`: Triggered when a `transient` popup is closed.

## Prism Compatibility

[Prism](https://prismlibrary.com/docs/maui/index.html) is a popular library offering the same (and quite a few more) features as this library. Nucleus MVVM aims to be a simpler alternative, only adding quality-of-life MVVM features as a layer on top of [MAUI](https://github.com/dotnet/maui). This should result in an easy to maintain library able to use the latest MAUI features.

Some compatibility classes have been included to simplify migration for projects limited to simple navigation and MVVM functionality. Nucleus MVVM is a much simpler library however, so when using more advanced Prism features this might require significant rework. This functionality has been added to the *Mvvm.Nucleus.Maui.Compatibility* namespace.

Included are:

- Interfaces for similar ViewModels events (e.a. `IPageLifecycleAware`) are mostly kept identical to Prism.
- `BindableBase` class has been created to add some missing functions to `ObservableObject`.
- `NavigationParameters` class has been added as a named `IDictionary<string, object>`.
- `IsBackNavigation` and `GetNavigationMode` extensions (see below for more details).

The `NavigationParameters` compatibility class differs from IDictionary in that it returns null when accessing a non-existing key, which is how it works in Prism. It's here to ensure that if you have code that expects this logic, it will continue to work as expected.

The `NavigationMode` and `IsBackNavigation` and `GetNavigationMode` methods are included to ease migration, but the concept does not translate well for advanced cases, such as navigating multiple levels at once. It is recommended to use `IInitializable(Async)` for handling a return to a Page or ViewModel, or using `GetShellNavigationSource` when dealing with complex logic.

Contrary to Prism, dependency injection in Nucleus uses the default Microsoft implementation, which means that apart from registering Views/ViewModels, any other registration should be done through the usual `Services.AddSingleton<>` and similar.

## Limitations and plans

- There's no support yet for multiple Windows, a single Window with Shell will be created.
- Initial logic for subviews recieving page events has been added, but is not a fully supported concept yet. 

## Support

- Bugs and feature requests can be created through [GitHub issues](https://github.com/EGoverde/Mvvm.Nucleus.Maui/issues/new).

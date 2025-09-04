# Migration guide

## Nucleus 0.6.0: Breaking changes to `Popup` and `IPopupService`
Nucleus uses the `Popup` functionality from the `Maui.CommunityToolkit`. In version 12.x of the toolkit a large breaking change was done, known as the V2 Popups. This required significant changes to the Nucleus implementation as well, which was part of the 0.6.0 release.

**Related: [Migrating to Popup v2](https://github.com/CommunityToolkit/Maui/wiki/Migrating-to-Popup-v2) (from the official documentation).**

### Changes in the Maui.CommunityToolkit V2 Popup

* The way through which popups are displayed signifcantly changed, now wrapped in an internal `PopupPage`.
* Support has now been added for scopes (e.a. `Transient`, `Scoped` and `Singleton`)
* Before every `Popup` would be able to return an `object?`, which is now reserved for `Popup<T>`, with some caveats.
* When not requiring a result value a `View` can now also be pushed to show, it will automatically be wrapped in a `Popup`.
* Default styling is now applied to Popups, which can be customized through `PopupOptions`.
    * Note that you can set a default style for all popups through the CommunityToolkit options, accessible through `UseNucleusMvvm`

### Relevant changes in the Nucleus 0.6.0

* The creation of the Popups through IoC and registration of events is now similar to Pages, done through a `Behavior`.
* Popups can now be registered with different scopes, e.a. `Transient`, `Scoped` and `Singleton`.
* The IPopupAware has now been expanded with `PopupAware<T>`, which can receive a `Popup<T>` instance.
* The `CommunityToolkit.PopupService` is now compatible with Nucleus when using the  `UseCommunityToolkitPopupServiceCompatibility`.
    * You should always use the `View` instead of the `ViewModel`.
    * Don't register your popups through the CommunityToolkit, Nucleus will do this for you.
    * The `IPopupInitializable(Async)` interface is triggered when the popup is opened (instead of created) when using this method.
    * Note that the compatibility is achieved partially by replacing the IoC registration. Do not call `UseMauiCommunityToolkit()`.
* The `NucleusPopupViewModel` no longer includes the `CloseCommand` and `CloseAsync`, as it is at odds with the new `Popup` and `Popup<T>`
    * Similar to `NucleusViewModel` it has also been marked as `obsolete`, as you are recommended to implement your own base ViewModel features.

### Migration helpers

To make migrating to the new `Popup` functionality easier a few additional compatibility files have been created. This is mostly useful if in a large codebase a lot of popups have to be migrated, as it can then be done step-by-step.

#### `Compatibility.CommunityToolkitV1Popup`
This file can be used as a temporary replacement for files subclassing `Popup`, giving them a return value of `object?` like before. It includes the properties `ResultWhenUserTapsOutsideOfPopup`, `Color` and `Size`, which have since been removed or changed. This class is best to be used in conjunction with the `Compatibility.CommunityToolkitV1PopupService`.

#### `Compatibility.CommunityToolkitV1PopupService`
This service has an identical interface to the `IPopupService` from Nucleus before 0.6.0. It expects the above `Compatibility.CommunityToolkitV1Popup` and behaves mostly as the original service does in regard to return values, including using the `ResultWhenUserTapsOutsideOfPopup`.

Due to the difference in implementation between the V1 and V2 Popups the end-result will still look somewhat different. This service uses an empty `PopupOptions` by default, but it can be overriden by setting the option `CommunityToolkitV1PopupServicePopupOptions` in `UseNucleusMvvm`.

See the Sample project for example implementations of both `Compatibility.CommunityToolkitV1Popup` and `Compatibility.CommunityToolkitV1PopupService`.
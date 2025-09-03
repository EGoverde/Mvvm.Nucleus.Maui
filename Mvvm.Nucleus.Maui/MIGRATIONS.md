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
    * Not that you can set a default style for all popups through the CommunityToolkit options, accessible through `UseNucleusMvvm`

### Changes in the Nucleus 0.6.0

### Migration helpers

To make migrating to the new `Popup` functionality easier a few additional compatibility files have been created. This is mostly useful if in a large codebase a lot of popups have to be migrated, as it can then be done step-by-step.

#### `Compatibility.CommunityToolkitV1Popup`
This file can be used as a temporary replacement for files subclassing `Popup`, giving them a return value of `object?`. It includes a property `ResultWhenUserTapsOutsideOfPopup` which has been removed from the new `Popup` implementation. It works best when used with the `Compatibility.CommunityToolkitV1PopupService`.

#### `Compatibility.CommunityToolkitV1PopupService`
This service has an identical interface to the `IPopupService` from Nucleus before 0.6.0. It expects the above `Compatibility.CommunityToolkitV1Popup` and behaves mostly as the old service does in regard to return values. It overrides the default PopupOptions from the toolkit with one that has no styling.
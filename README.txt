Welcome to Nucleus MVVM for MAUI.

Documentation: https://github.com/EGoverde/Mvvm.Nucleus.Maui/blob/main/README.md
Upgrading: https://github.com/EGoverde/Mvvm.Nucleus.Maui/blob/main/MIGRATIONS.md

Quick getting started guide:

1. In MauiProgram.cs replace 'UseMauiApp' and 'UseMauiCommunityToolkit' with .UseNucleusMvvm.
2. Register your Pages, ViewModels and Popups through the options in the above method.
3. Use the INavigationService to navigate from ViewModel-to-ViewModel, and use the interfaces for events.

Important note when upgrading to 0.6.0 from a previous version:

The Popup implementation within Nucleus has been change significantly, due to breaking changes from the MauiCommunityToolkit. See the above upgrading guide and the updated documentation for details.
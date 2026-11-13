Welcome to Nucleus MVVM for MAUI.

Documentation: https://github.com/EGoverde/Mvvm.Nucleus.Maui/blob/main/README.md
Upgrading: https://github.com/EGoverde/Mvvm.Nucleus.Maui/blob/main/MIGRATIONS.md

Quick getting started guide:

1. In App.xaml.cs remove 'CreateWindow(IActivationState? activationState)'.
2. In MauiProgram.cs replace 'UseMauiApp' and 'UseMauiCommunityToolkit' with 'UseNucleusMvvm'.
3. Register your Pages, ViewModels and Popups through the options in the above method.
4. Use the INavigationService to navigate from your viewmodel, and use the interfaces for events.
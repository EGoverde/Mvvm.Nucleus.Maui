namespace Mvvm.Nucleus.Maui;

/// <summary>
/// The <see cref="IPageDialogService"/> handles displaying alerts and action sheets using the default MAUI functionality.
/// </summary>
public interface IPageDialogService
{
    /// <summary>
    /// Displays a platform action sheet, allowing the application user to choose from several buttons.
    /// </summary>
    /// <param name="title">Title of the displayed action sheet. Can be null to hide the title.</param>
    /// <param name="cancel">Text to be displayed in the 'Cancel' button. Can be null to hide the null action.</param>
    /// <param name="destruction">Text to be displayed in the 'Destruct' button. Can be null to hide the destructive option.</param>
    /// <param name="buttons">Text labels for additional buttons.</param>
    /// <returns>A <see cref="Task"/> that displays an action sheet and returns the string caption of the button pressed by the user.</returns>
    Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);

    /// <summary>
    /// Displays a platform action sheet, allowing the application user to choose from several buttons.
    /// </summary>
    /// <param name="title">Title of the displayed action sheet. Can be null to hide the title.</param>
    /// <param name="cancel">Text to be displayed in the 'Cancel' button. Can be null to hide the null action.</param>
    /// <param name="destruction">Text to be displayed in the 'Destruct' button. Can be null to hide the destructive option.</param>
    /// <param name="flowDirection">The flow direction to be used by the action sheet.</param>
    /// <param name="buttons">Text labels for additional buttons.</param>
    /// <returns>A <see cref="Task"/> that displays an action sheet and returns the string caption of the button pressed by the user.</returns>
    Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons);

    /// <summary>
    /// Displays an alert with confirm and cancel buttons.
    /// </summary>
    /// <param name="title">The title of the alert dialog. Can be null to hide the title.</param>
    /// <param name="message">The body text of the alert dialog.</param>
    /// <param name="acceptButton">Text to be displayed on the 'Accept' button.</param>
    /// <param name="cancelButton">Text to be displayed on the 'Cancel' button.</param>
    /// <returns>A <see cref="Task"/> that contains the user's choice as a bool value. true indicates that the user accepted the alert. false indicates that the user cancelled the alert.</returns>
    Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton);

    /// <summary>
    /// Displays an alert with confirm and cancel buttons.
    /// </summary>
    /// <param name="title">The title of the alert dialog. Can be null to hide the title.</param>
    /// <param name="message">The body text of the alert dialog.</param>
    /// <param name="acceptButton">Text to be displayed on the 'Accept' button.</param>
    /// <param name="cancelButton">Text to be displayed on the 'Cancel' button.</param>
    /// <param name="flowDirection">The <see cref="FlowDirection"/>.</param>
    /// <returns>A <see cref="Task"/> that contains the user's choice as a bool value. true indicates that the user accepted the alert. false indicates that the user cancelled the alert.</returns>
    Task<bool> DisplayAlertAsync(string title, string message, string acceptButton, string cancelButton, FlowDirection flowDirection);

    /// <summary>
    /// Displays an alert dialog to the application user with a single cancel button.
    /// </summary>
    /// <param name="title">The title of the alert dialog. Can be null to hide the title.</param>
    /// <param name="message">The body text of the alert dialog.</param>
    /// <param name="cancelButton">Text to be displayed on the 'Cancel' button.</param>
    /// <returns>A <see cref="Task"/> that is completed once the user has dismisses the alert.</returns>
    Task DisplayAlertAsync(string title, string message, string cancelButton);

    /// <summary>
    /// Displays an alert dialog to the application user with a single cancel button.
    /// </summary>
    /// <param name="title">The title of the alert dialog. Can be null to hide the title.</param>
    /// <param name="message">The body text of the alert dialog.</param>
    /// <param name="cancelButton">Text to be displayed on the 'Cancel' button.</param>
    /// <param name="flowDirection">The <see cref="FlowDirection"/>.</param>
    /// <returns>A <see cref="Task"/> that is completed once the user has dismisses the alert.</returns>
    Task DisplayAlertAsync(string title, string message, string cancelButton, FlowDirection flowDirection);

    /// <summary>
    /// Displays a prompt dialog to the application user with the intent to capture a single string value.
    /// </summary>
    /// <param name="title">The title of the prompt dialog.</param>
    /// <param name="message">The body text of the prompt dialog.</param>
    /// <param name="accept">Text to be displayed on the 'Accept' button.</param>
    /// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
    /// <param name="placeholder">The placeholder text to display in the prompt. Can be null when no placeholder is desired.</param>
    /// <param name="maxLength">The maximum length of the user response.</param>
    /// <param name="keyboardType">The keyboard type to use for the user response.</param>
    /// <param name="initialValue">A pre-defined response that will be displayed, and which can be edited by the user.</param>
    /// <returns>A <see cref="Task"/> that displays a prompt display and returns the string value as entered by the user.</returns>
    Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboardType = null, string initialValue = "");
}
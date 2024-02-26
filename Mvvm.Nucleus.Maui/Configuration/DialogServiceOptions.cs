namespace Mvvm.Nucleus.Maui
{
    public class DialogServiceOptions
    {
        public string DefaultAlertOk { get; set; } = "OK";
        public string DefaultConfirmPositive { get; set; } = "OK";
        public string DefaultConfirmNegative { get; set; } = "Cancel";
        public string DefaultPromptAccept { get; set; } = "OK";
        public string DefaultPromptCancel { get; set; } = "OK";
        public string DefaultTitleAlert { get; set; } = "Notification";
        public string DefaultTitleConfirm { get; set; } = "Please confirm";
        public string DefaultTitlePrompt { get; set; } = "Please enter a value";
    }
}
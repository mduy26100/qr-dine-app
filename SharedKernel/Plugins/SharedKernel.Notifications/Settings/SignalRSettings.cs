namespace SharedKernel.Notifications.Settings
{
    public class SignalRSettings
    {
        public const string SectionName = "SignalRSettings";

        public string HubRoute { get; set; } = "/hubs/notifications";
    }
}

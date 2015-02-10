namespace ConnectSdk.Windows.Service.Capability
{
    public class ToastControl : CapabilityMethods
    {
        public static string Any = "ToastControl.Any";

        public static string ShowToast = "ToastControl.Show";
        public static string ShowClickableToastApp = "ToastControl.Show.Clickable.App";
        public static string ShowClickableToastAppParams = "ToastControl.Show.Clickable.App.Params";
        public static string ShowClickableToastUrl = "ToastControl.Show.Clickable.URL";

        public static string[] Capabilities =
        {
            ShowToast,
            ShowClickableToastApp,
            ShowClickableToastAppParams,
            ShowClickableToastUrl
        };
    }
}
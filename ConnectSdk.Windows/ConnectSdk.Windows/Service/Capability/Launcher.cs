namespace MyRemote.ConnectSDK.Service.Capability
{
    public class Launcher : CapabilityMethods
    {
        public static string Any = "Launcher.Any";

        public static string Application = "Launcher.Application";
        public static string ApplicationParams = "Launcher.Application.Params";
        public static string ApplicationClose = "Launcher.Application.Close";
        public static string ApplicationList = "Launcher.Application.List";
        public static string Browser = "Launcher.Browser";
        public static string BrowserParams = "Launcher.Browser.Params";
        public static string Hulu = "Launcher.Hulu";
        public static string HuluParams = "Launcher.Hulu.Params";
        public static string Netflix = "Launcher.Netflix";
        public static string NetflixParams = "Launcher.Netflix.Params";
        public static string YouTube = "Launcher.YouTube";
        public static string YouTubeParams = "Launcher.YouTube.Params";
        public static string AppStore = "Launcher.AppStore";
        public static string AppStoreParams = "Launcher.AppStore.Params";
        public static string AppState = "Launcher.AppState";
        public static string AppStateSubscribe = "Launcher.AppState.Subscribe";
        public static string RunningApp = "Launcher.RunningApp";
        public static string RunningAppSubscribe = "Launcher.RunningApp.Subscribe";

        public static string[] Capabilities =
        {
            Application,
            ApplicationParams,
            ApplicationClose,
            ApplicationList,
            Browser,
            BrowserParams,
            Hulu,
            HuluParams,
            Netflix,
            NetflixParams,
            YouTube,
            YouTubeParams,
            AppStore,
            AppStoreParams,
            AppState,
            AppStateSubscribe,
            RunningApp,
            RunningAppSubscribe
        };
    }
}
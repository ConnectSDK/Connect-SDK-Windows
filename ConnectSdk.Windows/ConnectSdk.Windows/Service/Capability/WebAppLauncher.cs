namespace ConnectSdk.Windows.Service.Capability
{
    public class WebAppLauncher : CapabilityMethods
    {
        public static string Any = "WebAppLauncher.Any";

        public static string Launch = "WebAppLauncher.Launch";
        public static string LaunchParams = "WebAppLauncher.Launch.Params";
        public static string MessageSend = "WebAppLauncher.Message.Send";
        public static string MessageReceive = "WebAppLauncher.Message.Receive";
        public static string MessageSendJson = "WebAppLauncher.Message.Send.JSON";
        public static string MessageReceiveJson = "WebAppLauncher.Message.Receive.JSON";
        public static string Close = "WebAppLauncher.Close";

        public static string[] Capabilities =
        {
            Launch,
            LaunchParams,
            MessageSend,
            MessageReceive,
            MessageSendJson,
            MessageReceiveJson,
            Close
        };
    }
}
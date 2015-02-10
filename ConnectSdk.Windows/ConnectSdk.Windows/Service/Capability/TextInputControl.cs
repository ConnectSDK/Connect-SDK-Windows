namespace MyRemote.ConnectSDK.Service.Capability
{
    public class TextInputControl : CapabilityMethods
    {
        public static string Any = "TextInputControl.Any";

        public static string Send = "TextInputControl.Send";
        public static string Send_Enter = "TextInputControl.Enter";
        public static string Send_Delete = "TextInputControl.Delete";
        public static string Subscribe = "TextInputControl.Subscribe";

        public static string[] Capabilities =
        {
            Send,
            Send_Enter,
            Send_Delete,
            Subscribe
        };
    }
}